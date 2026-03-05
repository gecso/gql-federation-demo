#!/usr/bin/env zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"

cleanup() {
  pkill -f 'src/Project.Web/Project.Web.csproj' || true
  pkill -f 'src/Conversation.Web/Conversation.Web.csproj' || true
  pkill -f 'src/BFF/BFF.csproj' || true
}

trap cleanup EXIT INT TERM

cd "$ROOT_DIR"

cleanup

echo "[run-all] starting Project.Web..."
dotnet run --project src/Project.Web/Project.Web.csproj --urls http://localhost:5249 >/tmp/project-web.log 2>&1 &

echo "[run-all] starting Conversation.Web..."
dotnet run --project src/Conversation.Web/Conversation.Web.csproj --urls http://localhost:5102 >/tmp/conversation-web.log 2>&1 &

sleep 5

echo "[run-all] composing Fusion gateway graph..."
./scripts/fusion-compose.sh

echo "[run-all] starting BFF..."
dotnet run --project src/BFF/BFF.csproj --urls http://localhost:5100
