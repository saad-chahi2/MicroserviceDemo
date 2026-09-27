# MicroserviceDemo

Projet d'apprentissage **Docker → Kubernetes → CI/CD** avec 2 microservices .NET 10 et SQL Server.

| Service | Rôle | Port local | Swagger |
|---|---|---|---|
| CustomerService | Gestion des clients | 5002 | http://localhost:5002/swagger |
| OrderService | Gestion des commandes | 5001 | http://localhost:5001/swagger |
| SQL Server | 1 base par service (`CustomerDb`, `OrderDb`) | 1433 | — |

## Démarrer

```powershell
Copy-Item .env.example .env          # une seule fois
docker compose up --build -d         # construit les images et lance tout
docker compose ps                    # état des conteneurs
docker compose logs -f customerservice
docker compose down                  # arrêter (ajouter -v pour effacer la base)
```

## Développer / tester

```powershell
dotnet build MicroservicesApp.slnx
dotnet test --solution MicroservicesApp.slnx
```

## Structure

```
src/        code des microservices (1 dossier = 1 service = 1 image Docker)
tests/      tests unitaires (xUnit v3 + NSubstitute)
k8s/        manifests Kubernetes
docs/k8s/   guides d'apprentissage (roadmap, concepts, kubectl, CI/CD)
```

Détails de chaque fichier : [docs/k8s/05-STRUCTURE-DU-PROJET.md](docs/k8s/05-STRUCTURE-DU-PROJET.md).
