# 01 — Concepts Kubernetes (appliqués à CE projet)

## L'idée en une phrase

Docker lance **un** conteneur sur **une** machine. Kubernetes gère **des centaines** de conteneurs sur **plusieurs** machines : tu lui décris l'**état désiré** en YAML ("je veux 2 OrderService, joignables sur le port 5001"), et il travaille en boucle pour que la réalité corresponde à cette description (**réconciliation**). Si un conteneur meurt, il le recrée. C'est *déclaratif* : tu dis **quoi**, pas **comment**.

## Architecture d'un cluster

```
┌──────────────── Control Plane (le cerveau) ────────────────┐
│ kube-apiserver  ← kubectl parle UNIQUEMENT à lui            │
│ etcd            ← base clé/valeur : tout l'état du cluster  │
│ scheduler       ← choisit sur quel node placer un Pod       │
│ controller-manager ← boucles de réconciliation              │
└─────────────────────────────────────────────────────────────┘
┌──────── Worker Node ────────┐  ┌──────── Worker Node ────────┐
│ kubelet  ← lance les Pods    │  │ kubelet                      │
│ kube-proxy ← réseau Services │  │ kube-proxy                   │
│ container runtime (containerd)│ │ ...                          │
└──────────────────────────────┘  └──────────────────────────────┘
```

Avec Docker Desktop, tout est sur **un seul node** (ta machine).

## Anatomie d'un fichier YAML

Tout objet K8s a 4 blocs :

```yaml
apiVersion: apps/v1      # version de l'API qui gère ce type
kind: Deployment         # le type d'objet
metadata:                # identité : name, namespace, labels
  name: orderservice
  namespace: microservices-app
spec:                    # l'ÉTAT DÉSIRÉ
  ...
```
(K8s ajoute un 5e bloc `status` = état réel. `kubectl get deploy orderservice -o yaml` pour le voir.)

---

## Les objets, du plus petit au plus grand

### Pod
- Plus petite unité déployable : **1 ou plusieurs conteneurs** qui partagent IP et volumes.
- **Éphémère** : il peut mourir et être remplacé par un autre avec une autre IP. On ne crée **presque jamais** un Pod directement.

### ReplicaSet
- Garantit qu'il y a toujours **N copies** d'un Pod. Tu ne le manipules pas : c'est le Deployment qui le crée.

### Deployment ⭐ (pour les apps sans état : nos 2 APIs)
- Gère les ReplicaSets → permet **rolling update** (nouvelle version progressive) et **rollback**.
- Le lien Deployment → Pods se fait par **labels** :

```yaml
spec:
  replicas: 2
  selector:
    matchLabels:
      app: orderservice        # "je gère les pods qui ont ce label"
  template:                    # le "moule" des pods
    metadata:
      labels:
        app: orderservice      # DOIT correspondre au selector
    spec:
      containers:
        - name: orderservice
          image: saadchahi/orderservice:v1
          ports:
            - containerPort: 5001
```

### StatefulSet (pour les apps avec état : SQL Server)
- Comme un Deployment mais avec **identité stable** (`sqlserverdb-0`) et un volume **attaché à chaque replica**. C'est le bon choix pour une base de données (le projet utilise actuellement un Deployment : ça marche avec 1 replica, mais StatefulSet est plus correct).

### Service ⭐ — le réseau
Problème : les Pods changent d'IP. Solution : un Service = **IP + nom DNS stables** qui répartit le trafic vers les Pods sélectionnés par label.

| Type | Accessible depuis | Usage ici |
|---|---|---|
| `ClusterIP` (défaut) | l'intérieur du cluster seulement | SQL Server (les APIs seules doivent le voir) |
| `NodePort` | `IP_du_node:30000-32767` | tests |
| `LoadBalancer` | IP externe (cloud) / `localhost` sur Docker Desktop | rapide pour tester les APIs |
| via **Ingress** | un seul point d'entrée HTTP | la bonne pratique pour les APIs |

**DNS interne** : `Server=sqlserverdb` dans la connection string fonctionne parce qu'il existe un Service nommé `sqlserverdb`. Nom complet : `sqlserverdb.microservices-app.svc.cluster.local`.

`port` = port du Service ; `targetPort` = port du conteneur.

### Ingress
- Routeur HTTP (L7) : `monapp.local/customers` → customerservice, `/orders` → orderservice.
- Nécessite un **Ingress Controller** installé (ex. ingress-nginx), sinon l'objet Ingress ne fait rien.

### Namespace
- "Dossier" logique pour isoler des ressources : `microservices-app`, `argocd`, `kube-system`…
- Supprimer un namespace supprime **tout** ce qu'il contient (pratique pour repartir de zéro).

### ConfigMap & Secret — la configuration
- **ConfigMap** : config non sensible (`ASPNETCORE_ENVIRONMENT`, URLs…).
- **Secret** : données sensibles (mot de passe SA). ⚠️ Encodé en **base64, pas chiffré** : ne jamais committer un Secret YAML avec la vraie valeur.
- .NET lit les variables d'env : `ConnectionStrings__DefaultConnection` (le `__` = `:` de appsettings.json).

```yaml
env:
  - name: SA_PASSWORD
    valueFrom:
      secretKeyRef:
        name: mssql-secret
        key: SA_PASSWORD
```

### Volumes : PV, PVC, StorageClass
- Un conteneur perd ses fichiers quand il meurt. Pour SQL Server il faut un stockage persistant.
- **PVC** (PersistentVolumeClaim) = "je demande 5 Gi". **PV** = le disque réel. **StorageClass** = qui crée le PV automatiquement (Docker Desktop : `hostpath`).

### Probes — la santé
- `readinessProbe` : "suis-je prêt à recevoir du trafic ?" → sinon retiré du Service.
- `livenessProbe` : "suis-je bloqué ?" → sinon redémarré.
- `startupProbe` : pour les démarrages lents (SQL Server met ~20-30 s).

### Resources
- `requests` = ce que le scheduler **réserve** ; `limits` = le **maximum** (dépassement mémoire → `OOMKilled`).

### HPA (HorizontalPodAutoscaler)
- Augmente/diminue le nombre de replicas selon CPU/mémoire. Nécessite `metrics-server`.

---

## Flux complet d'une requête dans ce projet

```
Navigateur → Ingress (/orders) → Service orderservice:5001 → Pod orderservice
                                                                   │
                                        "Server=sqlserverdb" (DNS) ▼
                                         Service sqlserverdb:1433 (ClusterIP) → Pod SQL → PVC
```

## Outils autour de K8s

| Outil | Rôle |
|---|---|
| **kubectl** | CLI officielle |
| **Kustomize** | Personnaliser des YAML par environnement sans template (intégré : `kubectl apply -k`) |
| **Helm** | "Gestionnaire de paquets" : charts templatisés (installer ingress-nginx, Argo CD, Prometheus…) |
| **k9s / Lens** | Interface pour visualiser le cluster |
| **Argo CD / Flux** | GitOps : le cluster se synchronise sur Git |
