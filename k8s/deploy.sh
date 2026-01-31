#!/bin/bash
set -e

# Deploy Threa to Kubernetes
# Usage: ./deploy.sh [--restart]
#   --restart: Only restart the deployment (pull latest image)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ "$1" == "--restart" ]]; then
    echo "Restarting deployment to pull latest image..."
    kubectl rollout restart deployment/threa -n threa
    echo ""
    echo "Watching rollout status..."
    kubectl rollout status deployment/threa -n threa
    exit 0
fi

echo "Applying Kubernetes manifests..."

echo "  Creating namespace..."
kubectl apply -f "$SCRIPT_DIR/namespace.yaml"

echo "  Creating configmap..."
kubectl apply -f "$SCRIPT_DIR/configmap.yaml"

echo "  Creating persistent volume claim..."
kubectl apply -f "$SCRIPT_DIR/pvc.yaml"

echo "  Creating deployment..."
kubectl apply -f "$SCRIPT_DIR/deployment.yaml"

echo "  Creating service..."
kubectl apply -f "$SCRIPT_DIR/service.yaml"

echo "  Creating ingress..."
kubectl apply -f "$SCRIPT_DIR/ingress.yaml"

echo ""
echo "Waiting for deployment to be ready..."
kubectl rollout status deployment/threa -n threa

echo ""
echo "Deployment complete!"
echo ""
kubectl get all -n threa
echo ""
echo "Access the app at: https://threa.tail920062.ts.net"
