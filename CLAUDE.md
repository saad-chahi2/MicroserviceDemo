# CLAUDE.md — Mémoire du projet

> Fichier lu automatiquement par Claude Code à chaque session. À garder court : le détail est dans [docs/k8s/](docs/k8s/).

## But du projet

Projet **d'apprentissage** (pas de prod). Objectif : maîtriser **Kubernetes** puis mettre en place une chaîne **CI/CD** complète sur un mini-projet microservices.
L'utilisateur a déjà déployé ce projet sur K8s il y a ~1 an et a oublié la méthode → **toujours expliquer le "pourquoi"**, pas seulement donner la commande.

## Stack

| Élément | Détail |
|---|---|
| CustomerService | ASP.NET Core .NET 10, EF Core SQL Server, port **5002** |
| OrderService | ASP.NET Core .NET 10, EF Core SQL Server, port **5001** |
| Base de données | SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`), port 1433, DB `MicroservicesDemo` |
| Images | Docker Hub `saadchahi/customerservice`, `saadchahi/orderservice` |
| Namespace K8s | `microservices-app` |
| Local sans K8s | `docker compose up --build` |
| Déploiement actuel | `powershell -ExecutionPolicy Bypass -File .\deploy-all.ps1` (manuel) |
| OS de dev | Windows 11, PowerShell |

## Règles pour Claude

1. **Répondre en français.** Termes techniques K8s en anglais (Pod, Deployment…) : c'est le vocabulaire officiel.
2. **Mode pédagogique** : pour chaque nouveau concept → 1) ce que c'est, 2) pourquoi on en a besoin ici, 3) le YAML/commande, 4) comment vérifier (`kubectl get/describe/logs`).
3. **Avancer étape par étape** en suivant [docs/k8s/00-ROADMAP.md](docs/k8s/00-ROADMAP.md). Cocher les étapes terminées dans ce fichier.
4. **Jamais de secret en clair** dans un nouveau fichier committé (mot de passe SA, tokens Docker Hub…) → Secret K8s / GitHub Secrets.
5. Avant de modifier un manifest, vérifier les problèmes connus listés dans [docs/k8s/04-AUDIT-EXISTANT.md](docs/k8s/04-AUDIT-EXISTANT.md).
6. Ne jamais `git push` ni déclencher un pipeline sans que l'utilisateur le demande.
7. Quand une décision est prise (outil choisi, cluster, convention), la noter dans la section « Décisions » ci-dessous.

## Guides

- [00-ROADMAP.md](docs/k8s/00-ROADMAP.md) — parcours d'apprentissage + avancement
- [01-CONCEPTS.md](docs/k8s/01-CONCEPTS.md) — tous les objets K8s expliqués avec CE projet
- [02-KUBECTL-CHEATSHEET.md](docs/k8s/02-KUBECTL-CHEATSHEET.md) — commandes du quotidien + debug
- [03-CICD.md](docs/k8s/03-CICD.md) — principes CI/CD, GitHub Actions, GitOps/Argo CD
- [04-AUDIT-EXISTANT.md](docs/k8s/04-AUDIT-EXISTANT.md) — erreurs trouvées dans les manifests actuels

## Décisions

- 2026-09-26 : CI = GitHub Actions. Registry = Docker Hub (existant). CD recommandé = Argo CD (GitOps) car le cluster est local — *à confirmer par l'utilisateur*.
- Cluster local : *à confirmer* (Docker Desktop Kubernetes probable, vu l'usage de `type: LoadBalancer`).
