#!/bin/bash

# Package deployment script for GitCG NetCord MainApp
# This script provides various packaging options

set -e

PROJECT_NAME="gitcg-netcord-mainapp"
IMAGE_TAG="latest"
DOCKERFILE_PATH="src/Gitcg.NetCord.MainApp/Dockerfile"

show_help() {
    echo "GitCG NetCord MainApp Packaging Tool"
    echo ""
    echo "Usage: $0 [COMMAND] [OPTIONS]"
    echo ""
    echo "Commands:"
    echo "  build     Build Docker image"
    echo "  run       Run the application container"
    echo "  stop      Stop the running container"
    echo "  logs      Show container logs"
    echo "  clean     Remove Docker image and containers"
    echo "  package   Create a deployment package"
    echo "  help      Show this help message"
    echo ""
    echo "Options:"
    echo "  -t, --tag IMAGE_TAG    Specify image tag (default: latest)"
    echo "  -p, --port PORT        Specify port mapping (default: 8080:8080)"
    echo ""
    echo "Examples:"
    echo "  $0 build                    # Build Docker image"
    echo "  $0 run                      # Run container with default settings"
    echo "  $0 build -t v1.0           # Build with custom tag"
    echo "  $0 package                  # Create deployment package"
}

build_image() {
    echo "🚀 Building Docker package for GitCG NetCord MainApp..."
    echo "📦 Building Docker image: $PROJECT_NAME:$IMAGE_TAG"
    
    docker build -f "$DOCKERFILE_PATH" -t "$PROJECT_NAME:$IMAGE_TAG" .
    
    if [ $? -eq 0 ]; then
        echo "✅ Successfully built Docker image: $PROJECT_NAME:$IMAGE_TAG"
        docker image ls | grep "$PROJECT_NAME" || true
    else
        echo "❌ Failed to build Docker image"
        exit 1
    fi
}

run_container() {
    echo "🚀 Starting GitCG NetCord MainApp container..."
    
    # Stop existing container if running
    docker stop "$PROJECT_NAME" 2>/dev/null || true
    docker rm "$PROJECT_NAME" 2>/dev/null || true
    
    # Run new container
    docker run -d \
        --name "$PROJECT_NAME" \
        -p 8080:8080 \
        -p 8081:8081 \
        --restart unless-stopped \
        "$PROJECT_NAME:$IMAGE_TAG"
    
    if [ $? -eq 0 ]; then
        echo "✅ Container started successfully"
        echo "🌐 Application available at:"
        echo "   HTTP:  http://localhost:8080"
        echo "   HTTPS: https://localhost:8081"
        echo ""
        echo "📋 Container status:"
        docker ps | grep "$PROJECT_NAME"
    else
        echo "❌ Failed to start container"
        exit 1
    fi
}

stop_container() {
    echo "🛑 Stopping GitCG NetCord MainApp container..."
    docker stop "$PROJECT_NAME" 2>/dev/null || echo "⚠️  Container not running"
    docker rm "$PROJECT_NAME" 2>/dev/null || echo "⚠️  Container not found"
    echo "✅ Container stopped and removed"
}

show_logs() {
    echo "📋 GitCG NetCord MainApp container logs:"
    docker logs "$PROJECT_NAME" --tail 50 -f
}

clean_all() {
    echo "🧹 Cleaning up Docker resources..."
    docker stop "$PROJECT_NAME" 2>/dev/null || true
    docker rm "$PROJECT_NAME" 2>/dev/null || true
    docker rmi "$PROJECT_NAME:$IMAGE_TAG" 2>/dev/null || true
    echo "✅ Cleanup completed"
}

create_package() {
    echo "📦 Creating deployment package..."
    
    local package_dir="gitcg-netcord-mainapp-package"
    local timestamp=$(date +%Y%m%d_%H%M%S)
    local package_name="gitcg-netcord-mainapp-${timestamp}.tar.gz"
    
    # Create temporary package directory
    rm -rf "$package_dir"
    mkdir -p "$package_dir"
    
    # Copy essential files
    cp docker-compose.yml "$package_dir/"
    cp build-docker.sh "$package_dir/"
    cp build-docker.bat "$package_dir/"
    cp -r src/Gitcg.NetCord.MainApp "$package_dir/"
    cp README.md "$package_dir/" 2>/dev/null || true
    cp .dockerignore "$package_dir/"
    
    # Create package README
    cat > "$package_dir/README.md" << 'EOF'
# GitCG NetCord MainApp Deployment Package

This package contains everything needed to deploy the GitCG NetCord MainApp.

## Quick Start

### Using Docker Compose (Recommended)
```bash
docker-compose up -d
```

### Using Build Script
```bash
# Linux/macOS
./build-docker.sh

# Windows
build-docker.bat
```

### Manual Docker Build
```bash
docker build -f Gitcg.NetCord.MainApp/Dockerfile -t gitcg-netcord-mainapp .
docker run -p 8080:8080 -p 8081:8081 gitcg-netcord-mainapp
```

## Access the Application
- HTTP: http://localhost:8080
- HTTPS: https://localhost:8081

## Requirements
- Docker Engine 20.0+
- Docker Compose 2.0+ (optional)

## Configuration
Edit the `docker-compose.yml` file to customize:
- Port mappings
- Environment variables
- Volume mounts
- Resource limits
EOF
    
    # Create the package
    tar -czf "$package_name" "$package_dir"
    
    echo "✅ Deployment package created: $package_name"
    echo "📁 Package contents:"
    tar -tzf "$package_name"
    
    # Cleanup
    rm -rf "$package_dir"
    
    echo ""
    echo "📋 To deploy this package:"
    echo "1. Extract: tar -xzf $package_name"
    echo "2. Navigate: cd gitcg-netcord-mainapp-package"
    echo "3. Deploy: docker compose up -d"
}

# Parse command line arguments
COMMAND=""
while [[ $# -gt 0 ]]; do
    case $1 in
        -t|--tag)
            IMAGE_TAG="$2"
            shift 2
            ;;
        -p|--port)
            PORT_MAPPING="$2"
            shift 2
            ;;
        build|run|stop|logs|clean|package|help)
            COMMAND="$1"
            shift
            ;;
        *)
            echo "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Execute command
case "$COMMAND" in
    build)
        build_image
        ;;
    run)
        run_container
        ;;
    stop)
        stop_container
        ;;
    logs)
        show_logs
        ;;
    clean)
        clean_all
        ;;
    package)
        create_package
        ;;
    help|"")
        show_help
        ;;
    *)
        echo "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac