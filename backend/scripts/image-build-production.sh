#!/bin/bash

set -e

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOCKER_DIR="$PROJECT_ROOT/docker"

GIT_HASH=$(git -C "$PROJECT_ROOT" rev-parse --short HEAD)
TAG="aspnet-server-image:$GIT_HASH"

docker build -t "$TAG" -f "$DOCKER_DIR/dockerfiles/Production.Server.Dockerfile" "$PROJECT_ROOT"