#!/bin/bash
set -e

# Copy SQLite database from the Kubernetes pod to local
# Usage: ./db-pull.sh [local-db-path]
#   local-db-path: Path to save database (default: ../threa.db)

# Use pwd -W for Windows-compatible paths in Git Bash, fall back to pwd
get_path() {
    pwd -W 2>/dev/null || pwd
}

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && get_path)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && get_path)"
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
    # Remove SQLite auxiliary files (journal, WAL, shared memory)
    rm -f "$LOCAL_DB-journal" "$LOCAL_DB-wal" "$LOCAL_DB-shm"
fi

echo "Copying database from pod..."
# Use relative path and MSYS_NO_PATHCONV to avoid Windows/Git Bash path mangling
TEMP_DB="threa-temp-$$.db"
MSYS_NO_PATHCONV=1 kubectl cp -n "$NAMESPACE" "$POD:$REMOTE_PATH" "$TEMP_DB"
mv "$TEMP_DB" "$LOCAL_DB"

echo ""
echo "Done! Database saved to $LOCAL_DB"
