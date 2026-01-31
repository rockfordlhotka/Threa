#!/bin/bash
set -e

# Copy local SQLite database to the Kubernetes pod
# Usage: ./db-push.sh [local-db-path]
#   local-db-path: Path to local database (default: ../threa.db)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
LOCAL_DB="${1:-$REPO_ROOT/threa.db}"
REMOTE_PATH="/app/data/threa.db"
NAMESPACE="threa"
POD_LABEL="app.kubernetes.io/name=threa"

if [[ ! -f "$LOCAL_DB" ]]; then
    echo "Error: Local database not found at $LOCAL_DB"
    exit 1
fi

# Get the pod name
POD=$(kubectl get pod -n "$NAMESPACE" -l "$POD_LABEL" -o jsonpath='{.items[0].metadata.name}')

if [[ -z "$POD" ]]; then
    echo "Error: No pod found in namespace $NAMESPACE with label $POD_LABEL"
    exit 1
fi

echo "Source: $LOCAL_DB"
echo "Target: $POD:$REMOTE_PATH"
echo ""

read -p "This will overwrite the remote database. Continue? [y/N] " -n 1 -r
echo ""

if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Aborted."
    exit 1
fi

echo "Copying database to pod..."
kubectl cp "$LOCAL_DB" "$NAMESPACE/$POD:$REMOTE_PATH"

echo ""
echo "Done! Database pushed to $POD"
echo ""
echo "You may need to restart the pod for changes to take effect:"
echo "  kubectl rollout restart deployment/threa -n threa"
