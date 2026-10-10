#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"

if [[ -z "${DEPLOY_HOST:-}" || -z "${DEPLOY_PATH:-}" || -z "${SERVICE:-}" ]]; then
  echo "set DEPLOY_HOST (user@host or an ssh alias)" >&2
  echo "set DEPLOY_PATH (eg. /opt/something)" >&2
  echo "set SERVICE (eg. some-service)" >&2
  exit 1
fi

dotnet publish -c Release -o ./publish
rsync -avz --delete ./publish/ "$DEPLOY_HOST:$DEPLOY_PATH/"
ssh "$DEPLOY_HOST" "sudo systemctl restart $SERVICE"
