# GitCG NetCord MainApp Docker Packaging Guide

This guide explains how to create and manage Docker packages for the GitCG NetCord MainApp.

## Overview

The GitCG NetCord MainApp is a .NET 9.0 Blazor Server application that can be packaged and deployed using Docker containers.

## Packaging Options

### 1. Quick Start with Docker Compose (Recommended)

The easiest way to build and run the application:

```bash
# Build and start the application
docker compose up -d

# Stop the application
docker compose down
```

### 2. Using Make Commands

If you have `make` available:

```bash
# Show all available commands
make help

# Build the Docker image
make build

# Run the application
make run

# Create a deployment package
make package

# Clean up
make clean
```

### 3. Using Package Script

Cross-platform packaging script with multiple options:

```bash
# Show help
./package.sh help

# Build Docker image
./package.sh build

# Run container
./package.sh run

# Create deployment package
./package.sh package

# Stop container
./package.sh stop

# View logs
./package.sh logs

# Clean up everything
./package.sh clean
```

### 4. Using Build Scripts

Simple build scripts for different platforms:

```bash
# Linux/macOS
./build-docker.sh

# Windows
build-docker.bat
```

### 5. Manual Docker Commands

Direct Docker commands for full control:

```bash
# Build image
docker build -f src/Gitcg.NetCord.MainApp/Dockerfile -t gitcg-netcord-mainapp:latest .

# Run container
docker run -d \
  --name gitcg-netcord-mainapp \
  -p 8080:8080 \
  -p 8081:8081 \
  --restart unless-stopped \
  gitcg-netcord-mainapp:latest
```

## Application Access

Once the container is running, the application will be available at:

- **HTTP**: http://localhost:8080
- **HTTPS**: https://localhost:8081

## Deployment Package

The `./package.sh package` command creates a complete deployment package containing:

- Docker Compose configuration
- Build scripts for all platforms
- Application source code
- Documentation
- Docker configuration files

The package can be distributed and deployed on any system with Docker installed.

## System Requirements

### For Building
- Docker Engine 20.0+
- .NET 9.0 SDK (for local development)

### For Deployment
- Docker Engine 20.0+
- Docker Compose 2.0+ (optional, but recommended)

## Configuration

### Environment Variables

The application supports the following environment variables:

- `ASPNETCORE_ENVIRONMENT`: Set to `Production`, `Development`, or `Staging`
- `ASPNETCORE_URLS`: Configure listening URLs

### Port Configuration

Default ports:
- HTTP: 8080
- HTTPS: 8081

To change ports, modify the `docker-compose.yml` file or use different port mappings in Docker commands.

### Volume Mounts

The Docker Compose configuration includes a logs volume mount:
- `./logs:/app/logs` - Application logs will be available in the local `logs` directory

## Troubleshooting

### Build Issues

1. **.NET Version Mismatch**: The application requires .NET 9.0. Make sure your Docker environment can access .NET 9.0 base images.

2. **Permission Issues**: Ensure build scripts are executable:
   ```bash
   chmod +x build-docker.sh
   chmod +x package.sh
   ```

3. **Port Conflicts**: If ports 8080 or 8081 are already in use, modify the port mappings in `docker-compose.yml`.

### Runtime Issues

1. **Container Won't Start**: Check logs with:
   ```bash
   docker logs gitcg-netcord-mainapp
   # or
   ./package.sh logs
   ```

2. **Application Not Accessible**: Verify port mappings and firewall settings.

### Performance Optimization

For production deployment, consider:

1. **Resource Limits**: Add resource constraints to `docker-compose.yml`:
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '2.0'
         memory: 1G
       reservations:
         memory: 512M
   ```

2. **Health Checks**: Add health check configuration:
   ```yaml
   healthcheck:
     test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
     interval: 30s
     timeout: 10s
     retries: 3
   ```

## Development Workflow

1. Make code changes
2. Test locally: `make build && make run`
3. Verify functionality at http://localhost:8080
4. Create deployment package: `make package`
5. Deploy package to target environment

## Security Considerations

- The container runs as a non-root user for security
- HTTPS is configured and available on port 8081
- Secrets should be managed through Docker secrets or environment variables
- Regular updates to base images are recommended

## Support

For issues with packaging or deployment, check:
1. Docker logs: `docker logs gitcg-netcord-mainapp`
2. Container status: `docker ps`
3. Image availability: `docker images | grep gitcg-netcord`