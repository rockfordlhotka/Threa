#!/bin/bash
set -e

# Build and push Threa Docker image
# Usage: ./build.sh [tag]
#   tag: Optional image tag (default: latest)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
IMAGE_NAME="rockylhotka/threa"
TAG="${1:-latest}"

echo "Building $IMAGE_NAME:$TAG..."
docker build -t "$IMAGE_NAME:$TAG" "$REPO_ROOT"

echo ""
echo "Pushing $IMAGE_NAME:$TAG..."
docker push "$IMAGE_NAME:$TAG"

echo ""
echo "Done! Image pushed: $IMAGE_NAME:$TAG"
echo ""
echo "To deploy, run: ./deploy.sh"
