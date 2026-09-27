# 05 — Structure du projet : chaque fichier et son rôle

## Fichiers à la racine

| Fichier | Rôle | Pourquoi c'est une bonne pratique |
|---|---|---|
| `global.json` | Fixe la version du SDK .NET (10.0.x) et active la plateforme de test Microsoft.Testing.Platform | Ton PC, Docker et GitHub Actions utilisent le même SDK → pas de « ça marche chez moi » |
| `Directory.Build.props` | Propriétés communes à tous les .csproj (net10.0, Nullable, warnings = erreurs) | Réglé une seule fois au lieu de 4 fois ; la CI refusera du code avec des warnings |
| `Directory.Packages.props` | Versions NuGet centralisées | Mettre à jour EF Core = changer 1 ligne ; impossible d'avoir 2 versions différentes entre services |
| `nuget.config` | Seule source NuGet : nuget.org | Ta machine avait un feed privé d'entreprise (401) qui cassait le restore ; la CI n'y aura jamais accès non plus |
| `.editorconfig` | Style de code (indentation, namespaces file-scoped…) | Même style pour tout le monde, vérifié au build |
| `.dockerignore` | Exclut bin/, obj/, .git, tests… du contexte Docker | Build plus rapide, image plus propre, pas de fuite de fichiers |
| `.env.example` → `.env` | Mot de passe SA pour docker compose | `.env` est dans `.gitignore` : le secret n'est jamais commité |
| `docker-compose.yml` | Lance SQL + 2 APIs en local | Healthcheck : les APIs attendent que SQL soit vraiment prêt |

## Architecture d'un service (identique pour Customer et Order)

```
Controllers/     HTTP uniquement : reçoit la requête, renvoie 200/201/400/404/409
    ↓
Application/     Logique métier (AppService) — testable sans base ni HTTP
    ↓
Domain/          Entité (Customer, Order) + ses règles (factory Create, validations)
    ↑
Persistence/     EF Core : AppDbContext, Repository, Migrations
Contracts/       DTO exposés par l'API (Request/Response) ≠ entités
Infrastructure/  Technique : application des migrations au démarrage
```

Règles appliquées :
- **L'API n'expose jamais l'entité EF** → DTO `CreateXxxRequest` / `XxxResponse`.
- **Validation à 2 niveaux** : attributs `[Required]`, `[EmailAddress]`… sur le DTO (→ 400 automatique) + gardes dans `Domain.Create()` (l'objet ne peut pas exister invalide).
- **Injection de dépendances** d'interfaces (`ICustomerRepository`, `TimeProvider`) → remplaçables par des faux dans les tests.
- **`TimeProvider`** au lieu de `DateTime.Now` → la date est contrôlable en test.
- **Erreurs au format ProblemDetails** (RFC 9110), standard HTTP.
- **1 base par microservice** (`CustomerDb`, `OrderDb`) : un service ne lit jamais la base d'un autre.
- **Migrations appliquées au démarrage avec retry** (avant : dans le constructeur du DbContext = à chaque requête !).
- **Health checks** `/health/live` et `/health/ready` → utilisés par les probes Kubernetes.
- Pas de `UseHttpsRedirection` : le HTTPS sera terminé par l'Ingress K8s.

## Endpoints

| Méthode | Route | Réponses |
|---|---|---|
| GET | `/api/customers` | 200 |
| GET | `/api/customers/{id}` | 200, 404 |
| POST | `/api/customers` | 201, 400, 409 (email déjà utilisé) |
| GET | `/api/orders?customerId=1` | 200 |
| GET | `/api/orders/{id}` | 200, 404 |
| POST | `/api/orders` | 201, 400 |
| GET | `/health/live`, `/health/ready` | 200, 503 |

## Tests unitaires (`tests/`)

- **xUnit v3** : framework de test. **NSubstitute** : crée des faux (mocks) des interfaces. **FakeTimeProvider** : horloge figée.
- Par service, 4 familles : Domain (règles de l'entité), Application (logique avec repository simulé), Controllers (bon code HTTP), Contracts (règles de validation).
- Aucun test ne touche SQL Server → rapides (~2 s) et exécutables dans la CI.
- Commande : `dotnet test --solution MicroservicesApp.slnx`

## Dockerfile (multi-stage)

1. Stage `build` (image SDK ~900 Mo) : copie d'abord **seulement** les .csproj/.props → `restore` (couche mise en cache), puis le code → `publish`.
2. Stage `final` (image ASP.NET runtime) : ne contient que les DLL publiées.
3. Port **8080** (standard .NET 8+), utilisateur **non-root** (`USER $APP_UID`).
4. Contexte de build = racine du repo : `docker build -f src/CustomerService/Dockerfile .`
