# Kubernetes Deployment

This directory contains Kubernetes manifests for deploying Threa to a K8s cluster.

## Prerequisites

- Kubernetes cluster with:
  - **Longhorn** storage class (for persistent volumes)
  - **Tailscale Kubernetes operator** (for ingress)
- Docker Hub access to push images to `rockylhotka/threa`

## Architecture

```
Internet --> Tailscale Funnel --> Ingress --> Service --> Deployment --> Pod
                                                              |
                                                     PersistentVolumeClaim
                                                        (SQLite database)
```

## Quick Start

```bash
# Build and push Docker image
./k8s/build.sh

# Deploy to cluster (or update existing deployment)
./k8s/deploy.sh

# Just restart to pull latest image
./k8s/deploy.sh --restart
```

## Manual Commands

### Build and Push Docker Image

```bash
# From repository root
docker build -t rockylhotka/threa:latest .
docker push rockylhotka/threa:latest

# Or with a specific tag
./k8s/build.sh v1.0.0
```

### Deploy to Cluster

```bash
# Apply all manifests in order
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/pvc.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
kubectl apply -f k8s/ingress.yaml
```

### Update Deployment (Rolling Update)

After pushing a new image:

```bash
kubectl rollout restart deployment/threa -n threa
kubectl rollout status deployment/threa -n threa
```

## Verify Deployment

```bash
# Check all resources
kubectl get all -n threa

# Check pod logs
kubectl logs -n threa -l app.kubernetes.io/name=threa -f

# Check persistent volume
kubectl get pvc -n threa

# Check ingress status
kubectl get ingress -n threa
```

## Access

The app is accessible via Tailscale Funnel at:
- **https://threa.tail920062.ts.net**

## Configuration

Environment variables are managed via ConfigMap (`configmap.yaml`):

| Variable | Value | Description |
|----------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Production | ASP.NET environment |
| `ASPNETCORE_URLS` | http://+:8080 | Listen URL |
| `CONNECTIONSTRINGS__SQLITE` | /app/data/threa.db | SQLite database path |

## Troubleshooting

### Pod not starting

```bash
kubectl describe pod -n threa -l app.kubernetes.io/name=threa
```

### Database issues

The SQLite database is stored on a Longhorn persistent volume at `/app/data/threa.db`.
Data persists across pod restarts and deployments.

To access the pod for debugging:

```bash
kubectl exec -it -n threa deployment/threa -- /bin/bash
```
