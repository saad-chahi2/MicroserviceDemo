# 00 — Roadmap Kubernetes + CI/CD

Cocher `[x]` au fur et à mesure. Chaque phase se termine par un **critère de réussite** vérifiable.

---

## Phase 0 — Environnement

- [ ] Choisir le cluster local : **Docker Desktop → Settings → Kubernetes → Enable** (le plus simple sur Windows), ou `kind` / `minikube`
- [ ] `kubectl version` et `kubectl get nodes` → 1 node `Ready`
- [ ] `kubectl config get-contexts` → comprendre ce qu'est un *context* (quel cluster je pilote)
- [ ] Installer : `helm`, (optionnel) **k9s** ou Lens pour visualiser

✅ Réussite : `kubectl get nodes` affiche `Ready`.

## Phase 1 — Rappel Docker (la base de tout)

- [ ] `docker compose up --build` → les 2 APIs répondent sur `http://localhost:5001/swagger` et `:5002/swagger`
- [ ] Comprendre le Dockerfile multi-stage (stage `build` avec le SDK, stage final avec le runtime seul)
- [ ] Ajouter un `.dockerignore` (bin/, obj/) — voir audit

✅ Réussite : les images se construisent et tournent sans K8s.

## Phase 2 — Premiers objets K8s (manuel, à la main)

- [ ] Namespace : créer `microservices-app` à partir d'un fichier YAML
- [ ] Lancer un Pod seul (`kubectl run nginx --image=nginx`), le supprimer → constater qu'il **ne revient pas**
- [ ] Deployment : le même, mais géré → supprimer le Pod → il **revient** (self-healing)
- [ ] Service ClusterIP : comprendre le DNS interne (`sqlserverdb` = nom du Service)
- [ ] `kubectl port-forward` pour accéder à une API sans l'exposer

✅ Réussite : expliquer avec ses mots la différence Pod / ReplicaSet / Deployment / Service.

## Phase 3 — Déployer le projet proprement

- [ ] Corriger les problèmes de [04-AUDIT-EXISTANT.md](04-AUDIT-EXISTANT.md)
- [ ] Secret pour le mot de passe SQL (`kubectl create secret generic ...`)
- [ ] ConfigMap pour la config non sensible
- [ ] PersistentVolumeClaim pour SQL Server → supprimer le pod SQL, les données restent
- [ ] Probes `readinessProbe` / `livenessProbe` (endpoint `/health` à ajouter dans les APIs)
- [ ] `resources.requests/limits` sur chaque conteneur
- [ ] Réorganiser les YAML dans un dossier `k8s/` (voir structure cible ci-dessous)

✅ Réussite : `kubectl apply -k k8s/` déploie tout, les 3 pods sont `Running` et `READY 1/1`.

## Phase 4 — Exposition & opérations

- [ ] Ingress Controller (ingress-nginx) + 1 Ingress : `/customers` → CustomerService, `/orders` → OrderService
- [ ] Mise à jour d'image → observer le **rolling update** (`kubectl rollout status`)
- [ ] Rollback (`kubectl rollout undo`)
- [ ] Scaling manuel (`kubectl scale --replicas=3`) puis HPA (autoscaling CPU, nécessite metrics-server)

✅ Réussite : changer le code, déployer v2, revenir à v1 en une commande.

## Phase 5 — Kustomize (ou Helm)

- [ ] `base/` + `overlays/dev` et `overlays/prod` (replicas, ressources différentes)
- [ ] `kustomize edit set image` → c'est ce que le pipeline CD utilisera pour changer le tag

✅ Réussite : `kubectl kustomize k8s/overlays/dev` affiche le YAML final.

## Phase 6 — CI (GitHub Actions)

- [ ] Workflow `ci.yml` : restore → build → test sur chaque PR
- [ ] Build + push des images Docker taguées avec le **SHA du commit** sur `master`
- [ ] Secrets GitHub : `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`
- [ ] (bonus) scan de vulnérabilités de l'image (Trivy)

✅ Réussite : un push sur master produit `saadchahi/orderservice:<sha>` sur Docker Hub sans rien lancer à la main.

## Phase 7 — CD (GitOps avec Argo CD)

- [ ] Installer Argo CD dans le cluster
- [ ] Créer une `Application` qui pointe sur `k8s/overlays/dev` du repo
- [ ] Le pipeline CI met à jour le tag d'image dans le repo → Argo CD synchronise automatiquement
- [ ] Tester : modifier le code → push → nouvelle version en ligne sans `kubectl`

✅ Réussite : **plus jamais besoin de `deploy-all.ps1`**.

## Phase 8 — Bonus (niveau avancé)

- [ ] Helm chart du projet
- [ ] Observabilité : Prometheus + Grafana (kube-prometheus-stack)
- [ ] Sealed Secrets / External Secrets (secrets dans Git de façon sûre)
- [ ] NetworkPolicy (OrderService n'a pas le droit de parler à…)
- [ ] Déploiement sur un vrai cloud (AKS / GKE / EKS) → CD push-based possible

---

## Structure cible du dossier k8s

```
k8s/
├── base/
│   ├── kustomization.yaml
│   ├── namespace.yaml
│   ├── sqlserver/        (statefulset ou deployment + pvc + service + secret)
│   ├── customerservice/  (deployment + service + configmap)
│   └── orderservice/     (deployment + service + configmap)
└── overlays/
    ├── dev/kustomization.yaml
    └── prod/kustomization.yaml
.github/workflows/
├── ci.yml
└── cd.yml
```
