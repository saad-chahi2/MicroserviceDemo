# 03 — CI/CD : principes et plan pour ce projet

## Les définitions

- **CI — Continuous Integration** : à chaque push/PR, une machine **compile, teste et construit l'artefact** (ici : les images Docker). But : détecter tôt qu'un changement casse quelque chose.
- **CD — Continuous Delivery** : l'artefact est toujours **prêt** à être déployé ; le déploiement en prod reste un clic humain.
- **CD — Continuous Deployment** : chaque changement validé est **déployé automatiquement**.

## Ce qu'on remplace

Aujourd'hui `deploy-all.ps1` fait tout à la main depuis ton PC :
```
docker build → docker push (tag fixe "v1") → kubectl apply
```
Problèmes : dépend de ta machine, pas de tests, tag `v1` réutilisé (K8s ne voit pas de changement → pas de redéploiement, impossible de savoir quelle version tourne, rollback impossible).

## Le pipeline cible

```
 git push (master)
      │
      ▼
┌──────────── CI : GitHub Actions ────────────┐
│ 1. dotnet restore / build / test             │
│ 2. docker build  (1 image par service)       │
│ 3. tag = SHA du commit  ex: orderservice:3f2a1c9
│ 4. docker push → Docker Hub                  │
│ 5. met à jour le tag dans k8s/overlays/dev   │
│    (commit automatique dans le repo)         │
└──────────────────────────────────────────────┘
      │  (Git = source de vérité)
      ▼
┌──────────── CD : Argo CD (dans le cluster) ──┐
│ surveille le repo → voit le nouveau tag      │
│ → kubectl apply automatique → rolling update │
└──────────────────────────────────────────────┘
```

## Push-based vs Pull-based (GitOps) — le choix clé

| | **Push** (GitHub Actions fait `kubectl apply`) | **Pull / GitOps** (Argo CD tire depuis Git) |
|---|---|---|
| Principe | Le pipeline se connecte au cluster | Un agent **dans** le cluster lit Git |
| Accès requis | Le runner GitHub doit **atteindre** l'API du cluster + avoir un kubeconfig | Aucun accès entrant au cluster |
| Cluster local (Docker Desktop) | ❌ impossible depuis GitHub cloud (sauf *self-hosted runner* sur ton PC) | ✅ fonctionne |
| Dérive (quelqu'un modifie à la main) | non détectée | détectée et corrigée automatiquement |
| Rollback | relancer un ancien pipeline | `git revert` |

➡️ **Recommandation pour ce projet : CI = GitHub Actions, CD = Argo CD (GitOps).** C'est aussi la pratique la plus demandée en entreprise.
Alternative plus simple pour débuter : *self-hosted runner* GitHub sur ton PC qui exécute `kubectl apply` (push-based).

## Règles d'or

1. **Tag immuable** : jamais `latest` ni `v1` réutilisé → SHA du commit (ou version sémantique `1.4.0`).
2. **Build once, deploy many** : la même image passe de dev à prod, seule la config change (overlays Kustomize).
3. **Secrets hors du code** : GitHub → *Settings → Secrets and variables → Actions*. Jamais de mot de passe dans le YAML committé.
4. **Git = source de vérité** : l'état du cluster doit pouvoir être recréé depuis le repo.
5. **Pipeline rapide** et qui échoue tôt (tests avant le build d'image).

## Vocabulaire GitHub Actions

| Terme | Sens |
|---|---|
| Workflow | fichier `.github/workflows/*.yml` |
| Trigger (`on:`) | `push`, `pull_request`, `workflow_dispatch` (bouton manuel) |
| Job | groupe d'étapes sur une même machine (runner) ; les jobs tournent en parallèle sauf `needs:` |
| Step | une commande (`run:`) ou une action réutilisable (`uses:`) |
| Runner | la VM qui exécute (`ubuntu-latest`) |
| Matrix | lancer le même job pour plusieurs valeurs (nos 2 services) |
| Secrets | `${{ secrets.DOCKERHUB_TOKEN }}` |

## Squelette du workflow CI (à construire ensemble en Phase 6)

```yaml
name: ci
on:
  push:
    branches: [master]
  pull_request:

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet build MicroservicesApp.slnx -c Release
      - run: dotnet test MicroservicesApp.slnx -c Release --no-build

  docker:
    needs: build-test
    if: github.ref == 'refs/heads/master'
    runs-on: ubuntu-latest
    strategy:
      matrix:
        service: [customerservice, orderservice]
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKERHUB_USERNAME }}
          password: ${{ secrets.DOCKERHUB_TOKEN }}
      - uses: docker/build-push-action@v6
        with:
          context: .
          file: ${{ matrix.service == 'customerservice' && 'CustomerService' || 'OrderService' }}/Dockerfile
          push: true
          tags: saadchahi/${{ matrix.service }}:${{ github.sha }}
```
Puis un job `update-manifests` : `kustomize edit set image ...:${{ github.sha }}` + commit → Argo CD prend le relais.

## Prérequis à préparer

- [ ] Token Docker Hub (*Account settings → Personal access tokens*) → secret `DOCKERHUB_TOKEN`
- [ ] Repo poussé sur GitHub
- [ ] Au moins un projet de tests (xUnit) pour que `dotnet test` ait du sens
- [ ] Dossier `k8s/` avec Kustomize (Phase 5)
