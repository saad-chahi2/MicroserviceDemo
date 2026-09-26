# ------------------------------
# CONFIGURATION
# ------------------------------

#To exec : powershell -ExecutionPolicy Bypass -File .\deploy-all.ps1

$dockerUser = "saadchahi"                     # Votre nom Docker Hub
$tag = "v1"                              # Tag pour les images
$namespace = "microservices-app"         # Namespace K8s

Write-Host "=== 1. BUILD DES IMAGES DOCKER ==="

docker build -t $dockerUser/customerservice:$tag  -f CustomerService/Dockerfile .
docker build -t $dockerUser/orderservice:$tag  -f OrderService/Dockerfile .

Write-Host "=== 2. PUSH DES IMAGES SUR DOCKER HUB ==="

docker login
docker push $dockerUser/customerservice:$tag
docker push $dockerUser/orderservice:$tag

Write-Host "=== 3. CREATION DU NAMESPACE SI INEXISTANT ==="

kubectl get namespace $namespace 2>$null
if ($LASTEXITCODE -ne 0) {
    kubectl create namespace $namespace
}

Write-Host "=== 4. DEPLOIEMENT SQL SERVER ==="

kubectl apply -f ./sqlserver-deployment.yaml -n $namespace

Write-Host "=== 5. DEPLOIEMENT CUSTOMER SERVICE ==="

kubectl apply -f CustomerService/K8s/customerservice-deployment.yaml -n $namespace

Write-Host "=== 6. DEPLOIEMENT ORDER SERVICE ==="

kubectl apply -f OrderService/K8s/orderservice-deployment.yaml -n $namespace

Write-Host "=== DEPLOIEMENT TERMINE ==="
