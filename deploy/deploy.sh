#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# ---- Configuration ----
GHCR_USER="${GHCR_USER:-}"
GHCR_TOKEN="${GHCR_TOKEN:-}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
# -----------------------

if [ -z "${DB_CONNECTION_STRING:-}" ]; then
  echo "ERROR: DB_CONNECTION_STRING is not set"
  exit 1
fi
if [ -z "${JWT_KEY:-}" ]; then
  echo "ERROR: JWT_KEY is not set"
  exit 1
fi

export DB_CONNECTION_STRING
export JWT_KEY

if [ -n "$GHCR_USER" ] && [ -n "$GHCR_TOKEN" ]; then
  echo "Authenticating with GHCR..."
  echo "$GHCR_TOKEN" | docker login ghcr.io -u "$GHCR_USER" --password-stdin
fi

echo "Pulling image tag: $IMAGE_TAG"
export IMAGE_TAG
docker compose pull cms

echo "Recreating containers..."
docker compose up -d --remove-orphans

echo "Done."
