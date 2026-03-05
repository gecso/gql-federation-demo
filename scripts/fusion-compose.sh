#!/usr/bin/env zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
FUSION_DIR="$ROOT_DIR/.fusion"

PROJECT_URL="${PROJECT_SUBGRAPH_URL:-http://localhost:5249/graphql}"
CONVERSATION_URL="${CONVERSATION_SUBGRAPH_URL:-http://localhost:5102/graphql}"

mkdir -p "$FUSION_DIR"

echo "[fusion] fetching subgraph SDL..."
curl -fsSL "$PROJECT_URL?sdl" -o "$FUSION_DIR/project.graphql"
curl -fsSL "$CONVERSATION_URL?sdl" -o "$FUSION_DIR/conversation.graphql"

echo "[fusion] generating subgraph config files..."
dotnet fusion subgraph config set name Project -c "$FUSION_DIR/project.subgraph-config.json"
dotnet fusion subgraph config set http --url "$PROJECT_URL" -c "$FUSION_DIR/project.subgraph-config.json"

dotnet fusion subgraph config set name Conversation -c "$FUSION_DIR/conversation.subgraph-config.json"
dotnet fusion subgraph config set http --url "$CONVERSATION_URL" -c "$FUSION_DIR/conversation.subgraph-config.json"

echo "[fusion] packing subgraphs..."
dotnet fusion subgraph pack -p "$FUSION_DIR/project.fsp" -s "$FUSION_DIR/project.graphql" -c "$FUSION_DIR/project.subgraph-config.json"
dotnet fusion subgraph pack -p "$FUSION_DIR/conversation.fsp" -s "$FUSION_DIR/conversation.graphql" -c "$FUSION_DIR/conversation.subgraph-config.json"

echo "[fusion] composing gateway package..."
dotnet fusion compose -p "$FUSION_DIR/gateway.fgp" -s "$FUSION_DIR/project.fsp" -s "$FUSION_DIR/conversation.fsp"

echo "[fusion] exporting gateway graph for BFF..."
dotnet fusion export graph -p "$FUSION_DIR/gateway.fgp" -f "$ROOT_DIR/src/BFF/fusion-gateway.graphql"

echo "[fusion] done -> src/BFF/fusion-gateway.graphql"
