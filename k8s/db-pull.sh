#!/bin/bash
set -e

# Copy SQLite database from the Kubernetes pod to local
# Usage: ./db-pull.sh [local-db-path]
#   local-db-path: Path to save database (default: ../threa.db)

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
LOCAL_DB="${1:-$REPO_ROOT/threa.db}"
REMOTE_PATH="/app/data/threa.db"
NAMESPACE="threa"
POD_LABEL="app.kubernetes.io/name=threa"

# Get the pod name
POD=$(kubectl get pod -n "$NAMESPACE" -l "$POD_LABEL" -o jsonpath='{.items[0].metadata.name}')

if [[ -z "$POD" ]]; then
    echo "Error: No pod found in namespace $NAMESPACE with label $POD_LABEL"
    exit 1
fi

echo "Source: $POD:$REMOTE_PATH"
echo "Target: $LOCAL_DB"
echo ""

if [[ -f "$LOCAL_DB" ]]; then
    read -p "Local database exists. Overwrite? [y/N] " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 1
    fi
fi

echo "Copying database from pod..."
kubectl cp "$NAMESPACE/$POD:$REMOTE_PATH" "$LOCAL_DB"

echo ""
echo "Done! Database saved to $LOCAL_DB"
