# 04 — Audit des fichiers K8s existants (2026-09-26)

> Mise à jour après le refactoring : **corrigés** → #5, #10, #11, #13, #14. **À moitié** → #7 (endpoints `/health/live` et `/health/ready` existent, les probes restent à écrire dans les manifests). Les ports des manifests sont passés à 8080 et chaque service a sa base (`CustomerDb`, `OrderDb`). Les autres points restent des exercices de la Phase 3.

Problèmes trouvés dans les manifests actuels. Excellent exercice : les corriger un par un en Phase 3.

| # | Fichier | Problème | Pourquoi c'est un problème | Correction |
|---|---|---|---|---|
| 1 | `namespace.yaml` | crée `microservices` | tout le reste utilise `microservices-app` → namespace inutile | renommer en `microservices-app` |
| 2 | `*-deployment.yaml` (x3) | `namespace:` placé sous `spec.template.metadata` | ignoré à cet endroit ; le Deployment n'atterrit dans le bon namespace **que** grâce au `-n` du script | mettre `namespace:` dans le `metadata` racine du Deployment |
| 3 | `sqlserver-deployment.yaml` + `sqlserver-service.yaml` | **deux** Services `sqlserverdb` (un LoadBalancer, un ClusterIP) | même nom → le dernier `apply` écrase l'autre, comportement imprévisible | garder **un seul** Service en `ClusterIP` (la DB ne doit pas être exposée) |
| 4 | tous | mot de passe `sa@123456` en clair (YAML, compose, appsettings) | fuite dès que le repo est public | Secret K8s + `secretKeyRef` ; GitHub Secrets pour la CI |
| 5 | `sqlserver-deployment.yaml` | variable `SA_PASSWORD` | dépréciée par Microsoft | `MSSQL_SA_PASSWORD` |
| 6 | Deployments APIs | image `:v1` fixe | pousser une nouvelle `v1` ne déclenche pas de redéploiement | tag = SHA du commit (CI) |
| 7 | tous | pas de `readinessProbe` / `livenessProbe` | les APIs reçoivent du trafic avant d'être prêtes ; SQL met ~30 s à démarrer → crash des APIs au démarrage | ajouter `app.MapHealthChecks("/health")` + probes |
| 8 | tous | pas de `resources` | SQL Server peut consommer toute la RAM du node | `requests`/`limits` (SQL : limit ≥ 2Gi) |
| 9 | APIs | `type: LoadBalancer` | OK pour tester sur Docker Desktop, mais en cloud = 1 IP publique payante par service | `ClusterIP` + Ingress |
| 10 | `Program.cs` (x2) | `UseHttpsRedirection()` alors que le conteneur n'écoute qu'en HTTP | warnings / redirections vers un port HTTPS inexistant | désactiver hors dev ou ignorer via Ingress (TLS terminé à l'Ingress) |
| 11 | Dockerfiles | pas de `.dockerignore` | `COPY ./CustomerService` copie `bin/` et `obj/` Windows dans l'image → builds plus lents, conflits possibles | ajouter `.dockerignore` : `**/bin`, `**/obj`, `**/.vs` |
| 12 | SQL | `Deployment` pour une base de données | fonctionne à 1 replica mais pas l'objet idiomatique | `StatefulSet` + `volumeClaimTemplates` (bonus) |
| 13 | Migrations EF | à vérifier : qui crée la base dans le cluster ? | si rien n'applique les migrations, les tables n'existent pas | `db.Database.Migrate()` au démarrage (simple) ou Job K8s de migration (propre) |
| 14 | Dockerfiles | commentaire `# �tape` | fichier enregistré en ANSI, pas UTF-8 | ré-enregistrer en UTF-8 |
