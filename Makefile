# Makefile for GitCG NetCord MainApp packaging

PROJECT_NAME := gitcg-netcord-mainapp
IMAGE_TAG := latest
DOCKERFILE_PATH := src/Gitcg.NetCord.MainApp/Dockerfile

.PHONY: help build run stop logs clean package test

help: ## Show this help message
	@echo "GitCG NetCord MainApp Packaging Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'
	@echo ""
	@echo "Examples:"
	@echo "  make build     # Build Docker image"
	@echo "  make run       # Run the application"
	@echo "  make package   # Create deployment package"

build: ## Build Docker image
	@echo "🚀 Building Docker image..."
	docker build -f $(DOCKERFILE_PATH) -t $(PROJECT_NAME):$(IMAGE_TAG) .
	@echo "✅ Build completed"

run: stop ## Run the application container
	@echo "🚀 Starting container..."
	docker run -d \
		--name $(PROJECT_NAME) \
		-p 8080:8080 \
		-p 8081:8081 \
		--restart unless-stopped \
		$(PROJECT_NAME):$(IMAGE_TAG)
	@echo "✅ Container started"
	@echo "🌐 Application available at http://localhost:8080"

stop: ## Stop and remove the container
	@echo "🛑 Stopping container..."
	-docker stop $(PROJECT_NAME) 2>/dev/null
	-docker rm $(PROJECT_NAME) 2>/dev/null
	@echo "✅ Container stopped"

logs: ## Show container logs
	@echo "📋 Container logs:"
	docker logs $(PROJECT_NAME) --tail 50 -f

clean: stop ## Clean up Docker resources
	@echo "🧹 Cleaning up..."
	-docker rmi $(PROJECT_NAME):$(IMAGE_TAG) 2>/dev/null
	@echo "✅ Cleanup completed"

package: ## Create deployment package
	@echo "📦 Creating deployment package..."
	@./package.sh package
	@echo "✅ Package created"

test: ## Test the Docker build
	@echo "🧪 Testing Docker build..."
	docker build -f $(DOCKERFILE_PATH) -t $(PROJECT_NAME):test .
	@echo "✅ Build test passed"
	-docker rmi $(PROJECT_NAME):test 2>/dev/null

compose-up: ## Start with docker compose
	@echo "🚀 Starting with docker compose..."
	docker compose up -d
	@echo "✅ Services started"

compose-down: ## Stop docker compose services
	@echo "🛑 Stopping docker compose services..."
	docker compose down
	@echo "✅ Services stopped"

status: ## Show container status
	@echo "📋 Container status:"
	@docker ps | grep $(PROJECT_NAME) || echo "Container not running"
	@echo ""
	@echo "📋 Image status:"
	@docker images | grep $(PROJECT_NAME) || echo "No images found"