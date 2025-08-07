#!/bin/bash

# Build script for GitCG NetCord MainApp Docker package
# This script builds the Docker image for the MainApp

set -e

PROJECT_NAME="gitcg-netcord-mainapp"
IMAGE_TAG="latest"
DOCKERFILE_PATH="src/Gitcg.NetCord.MainApp/Dockerfile"

echo "🚀 Building Docker package for GitCG NetCord MainApp..."

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed or not available in PATH"
    exit 1
fi

# Build the Docker image
echo "📦 Building Docker image: $PROJECT_NAME:$IMAGE_TAG"
docker build -f "$DOCKERFILE_PATH" -t "$PROJECT_NAME:$IMAGE_TAG" .

if [ $? -eq 0 ]; then
    echo "✅ Successfully built Docker image: $PROJECT_NAME:$IMAGE_TAG"
    echo "🔍 Image details:"
    docker image ls | grep "$PROJECT_NAME"
    echo ""
    echo "🚀 To run the container:"
    echo "   docker run -p 8080:8080 -p 8081:8081 $PROJECT_NAME:$IMAGE_TAG"
    echo ""
    echo "📋 Or use docker compose:"
    echo "   docker compose up -d"
else
    echo "❌ Failed to build Docker image"
    exit 1
fi