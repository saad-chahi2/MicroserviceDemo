# 02 — kubectl : aide-mémoire

Astuce : fixer le namespace par défaut pour ne plus taper `-n` :
```powershell
kubectl config set-context --current --namespace=microservices-app
```

## Cluster & contexte
```powershell
kubectl config get-contexts          # liste des clusters connus (* = actif)
kubectl config use-context docker-desktop
kubectl get nodes -o wide
kubectl cluster-info
```

## Lire l'état
```powershell
kubectl get all                      # pods, services, deployments, replicasets
kubectl get pods -o wide             # + IP et node
kubectl get pods -w                  # en temps réel (watch)
kubectl get deploy,svc,pvc,secret,configmap
kubectl get pod <pod> -o yaml        # définition complète + status
kubectl get events --sort-by=.metadata.creationTimestamp
```

## Appliquer / supprimer
```powershell
kubectl apply -f fichier.yaml        # crée OU met à jour (déclaratif) ← à utiliser
kubectl apply -f dossier/            # tous les fichiers du dossier
kubectl apply -k k8s/overlays/dev    # avec Kustomize
kubectl diff -f fichier.yaml         # voir ce qui VA changer avant d'appliquer
kubectl delete -f fichier.yaml
kubectl delete namespace microservices-app   # ⚠️ supprime tout le namespace
```

## Debug ⭐ (dans cet ordre)
```powershell
kubectl get pods                               # 1. quel est le STATUS ?
kubectl describe pod <pod>                     # 2. section "Events" en bas = la cause
kubectl logs <pod>                             # 3. logs de l'application
kubectl logs <pod> --previous                  #    logs du conteneur AVANT le crash
kubectl logs -f deploy/orderservice            #    suivre en direct
kubectl exec -it <pod> -- /bin/bash            # 4. entrer dans le conteneur
kubectl port-forward svc/orderservice 8081:5001  # 5. tester : http://localhost:8081/swagger
```

### Lire les STATUS
| Status | Signification | Cause fréquente |
|---|---|---|
| `Pending` | pas encore placé sur un node | PVC non lié, ressources insuffisantes |
| `ContainerCreating` | image en téléchargement / volume en montage | patience, ou `describe` |
| `ImagePullBackOff` / `ErrImagePull` | image introuvable | mauvais nom/tag, image privée, pas pushée |
| `CrashLoopBackOff` | le conteneur démarre puis crash en boucle | exception au démarrage → `logs --previous` |
| `OOMKilled` | dépassement de `limits.memory` | augmenter la limite (SQL Server ≥ 2 Gi) |
| `Running` mais `READY 0/1` | readinessProbe en échec | mauvais port/chemin de probe |

## Déploiements, versions, scaling
```powershell
kubectl set image deploy/orderservice orderservice=saadchahi/orderservice:v2
kubectl rollout status deploy/orderservice
kubectl rollout history deploy/orderservice
kubectl rollout undo deploy/orderservice        # rollback
kubectl rollout restart deploy/orderservice     # redémarrer tous les pods
kubectl scale deploy/orderservice --replicas=3
```

## Secrets & config
```powershell
kubectl create secret generic mssql-secret --from-literal=SA_PASSWORD='MonMotDePasse!1'
kubectl get secret mssql-secret -o jsonpath='{.data.SA_PASSWORD}'   # base64
kubectl create configmap app-config --from-literal=ASPNETCORE_ENVIRONMENT=Development
# Générer un YAML sans rien créer (pour apprendre la syntaxe) :
kubectl create deployment test --image=nginx --dry-run=client -o yaml
```

## Documentation intégrée
```powershell
kubectl explain deployment.spec.template.spec.containers
kubectl api-resources            # tous les types d'objets + noms courts (po, svc, deploy…)
```
