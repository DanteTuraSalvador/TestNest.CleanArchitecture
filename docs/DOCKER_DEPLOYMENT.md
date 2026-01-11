# Docker Deployment Guide

This guide explains how to build, deploy, and manage Docker containers for TestNest.CleanArchitecture.

## Overview

The application uses a multi-stage Docker build for optimal image size and security:

1. **Build Stage**: Compiles the application using .NET SDK
2. **Publish Stage**: Creates optimized release build
3. **Runtime Stage**: Minimal runtime image with ASP.NET Core runtime only

## Dockerfile Structure

### Multi-Stage Build

```dockerfile
# Stage 1: Build - Full SDK for compilation
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Stage 2: Publish - Optimized release build
FROM build AS publish

# Stage 3: Runtime - Minimal ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
```

### Security Features

- **Non-root user**: Application runs as `appuser` (not root)
- **Minimal image**: Runtime image contains only necessary components
- **Health checks**: Built-in health monitoring
- **No secrets in image**: All sensitive data via environment variables

## Azure Container Registry (ACR) Setup

### Create Azure Container Registry

```bash
# Create resource group
az group create --name testnest-rg --location eastus

# Create container registry
az acr create \
  --resource-group testnest-rg \
  --name testnestregistry \
  --sku Standard \
  --admin-enabled true

# Get ACR credentials
az acr credential show --name testnestregistry
```

### Configure Service Connection

1. Navigate to **Azure DevOps > Project Settings > Service connections**
2. Click **New service connection**
3. Select **Docker Registry**
4. Choose **Azure Container Registry**
5. Select your subscription and ACR
6. Name it: `TestNestACR`
7. Click **Save**

## Pipeline Configuration

### Pipeline Variables

The following variables are configured in [azure-pipelines.yml](../azure-pipelines.yml):

| Variable | Value | Description |
|----------|-------|-------------|
| `dockerRegistryServiceConnection` | `TestNestACR` | Service connection name |
| `imageRepository` | `testnest-admin-api` | Image name in registry |
| `containerRegistry` | `testnestregistry.azurecr.io` | ACR URL |
| `dockerfilePath` | `$(Build.SourcesDirectory)/Dockerfile` | Path to Dockerfile |
| `tag` | `$(Build.BuildId)` | Image tag (build ID) |

### Docker Build and Push Flow

1. **Trigger**: Push to `master` or `develop` branch
2. **Build Stage**: Compile and test application
3. **Code Quality**: Static analysis
4. **Docker Stage**:
   - Build Docker image
   - Tag with build ID and `latest`
   - Push to Azure Container Registry
5. **Deployment Stages**: Deploy to environments

## Local Development

### Build Docker Image Locally

```bash
# Navigate to repository root
cd c:\Users\Dante Salvador\source\repos\TestNest.CleanArchitecture

# Build the Docker image
docker build -t testnest-admin-api:local .

# View built images
docker images testnest-admin-api
```

### Run Container Locally

```bash
# Run with default settings
docker run -d -p 8080:8080 --name testnest-api testnest-admin-api:local

# Run with environment variables
docker run -d -p 8080:8080 \
  --name testnest-api \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Server=localhost;Database=TestNest;..." \
  testnest-admin-api:local

# View logs
docker logs testnest-api

# Follow logs
docker logs -f testnest-api

# Stop container
docker stop testnest-api

# Remove container
docker rm testnest-api
```

### Test Health Check

```bash
# Container health status
docker ps

# Manual health check
curl http://localhost:8080/health

# Detailed health check
curl http://localhost:8080/health | jq
```

## Azure Container Instances (ACI) Deployment

### Deploy to Azure Container Instances

```bash
# Deploy from ACR
az container create \
  --resource-group testnest-rg \
  --name testnest-api-dev \
  --image testnestregistry.azurecr.io/testnest-admin-api:latest \
  --registry-login-server testnestregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password> \
  --dns-name-label testnest-api-dev \
  --ports 8080 \
  --cpu 1 \
  --memory 1.5 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Development \
    ConnectionStrings__DefaultConnection="<connection-string>"

# View container status
az container show --resource-group testnest-rg --name testnest-api-dev --output table

# View logs
az container logs --resource-group testnest-rg --name testnest-api-dev

# Delete container
az container delete --resource-group testnest-rg --name testnest-api-dev
```

## Docker Compose (Optional)

Create `docker-compose.yml` for local multi-container setup:

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=TestNest;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
    depends_on:
      - db
    networks:
      - testnest-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - testnest-network

volumes:
  sqldata:

networks:
  testnest-network:
    driver: bridge
```

### Run with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## Image Optimization

### Current Image Size

```bash
# Check image size
docker images testnest-admin-api

# Typical sizes:
# - Build stage: ~2 GB (SDK)
# - Final image: ~200-300 MB (runtime only)
```

### Optimization Techniques

1. **Multi-stage builds**: Separate build and runtime
2. **.dockerignore**: Exclude unnecessary files
3. **Layer caching**: Order commands by change frequency
4. **Minimal base image**: Use ASP.NET runtime (not SDK)
5. **No test projects**: Excluded via .dockerignore

## Security Best Practices

### Container Security

1. **Non-root user**: ✅ Implemented (`appuser`)
2. **No secrets in image**: ✅ Use environment variables
3. **Minimal base image**: ✅ ASP.NET runtime only
4. **Health checks**: ✅ Built-in monitoring
5. **Regular updates**: Update base images frequently

### Scanning for Vulnerabilities

```bash
# Scan with Docker Scout (if available)
docker scout cves testnest-admin-api:local

# Scan with Trivy
trivy image testnest-admin-api:local

# Scan in Azure Container Registry
az acr task create \
  --name vulnerability-scan \
  --registry testnestregistry \
  --context /dev/null \
  --file - \
  --schedule "0 0 * * *"
```

### Image Signing (Optional)

```bash
# Sign image with Docker Content Trust
export DOCKER_CONTENT_TRUST=1

# Pull signed images only
docker pull testnestregistry.azurecr.io/testnest-admin-api:latest
```

## Kubernetes Deployment (Future)

### Example Kubernetes Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: testnest-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: testnest-api
  template:
    metadata:
      labels:
        app: testnest-api
    spec:
      containers:
      - name: api
        image: testnestregistry.azurecr.io/testnest-admin-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: testnest-secrets
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
```

## Troubleshooting

### Common Issues

#### 1. Image Build Fails

```bash
# Check Docker version
docker --version

# Clean build cache
docker builder prune

# Build with no cache
docker build --no-cache -t testnest-admin-api:local .
```

#### 2. Container Fails to Start

```bash
# View container logs
docker logs testnest-api

# Inspect container
docker inspect testnest-api

# Check health status
docker ps -a
```

#### 3. Health Check Fails

```bash
# Execute command in running container
docker exec -it testnest-api /bin/bash

# Check if curl is available
docker exec testnest-api curl --version

# Manual health check
docker exec testnest-api curl http://localhost:8080/health
```

#### 4. ACR Authentication Fails

```bash
# Login to ACR
az acr login --name testnestregistry

# Get credentials
az acr credential show --name testnestregistry

# Test connectivity
az acr repository list --name testnestregistry
```

### Debug Mode

Run container in interactive mode:

```bash
# Override entry point
docker run -it --entrypoint /bin/bash testnest-admin-api:local

# Run with debug verbosity
docker run -d -p 8080:8080 \
  -e Logging__LogLevel__Default=Debug \
  testnest-admin-api:local
```

## Monitoring

### Container Metrics

```bash
# View resource usage
docker stats testnest-api

# View processes
docker top testnest-api

# View container events
docker events --filter container=testnest-api
```

### Azure Container Registry Metrics

```bash
# View repository
az acr repository show \
  --name testnestregistry \
  --repository testnest-admin-api

# List tags
az acr repository show-tags \
  --name testnestregistry \
  --repository testnest-admin-api \
  --output table

# View manifest
az acr repository show-manifests \
  --name testnestregistry \
  --repository testnest-admin-api
```

## Cleanup

### Remove Old Images

```bash
# Remove images by tag
docker rmi testnest-admin-api:old-tag

# Remove dangling images
docker image prune

# Remove all unused images
docker image prune -a
```

### ACR Cleanup

```bash
# Delete old tags (keep last 10)
az acr repository show-tags \
  --name testnestregistry \
  --repository testnest-admin-api \
  --orderby time_asc \
  --output tsv | \
  head -n -10 | \
  xargs -I% az acr repository delete \
    --name testnestregistry \
    --image testnest-admin-api:% \
    --yes
```

## Best Practices

### Image Tagging Strategy

1. **Build ID**: `testnest-admin-api:1234` (unique per build)
2. **Latest**: `testnest-admin-api:latest` (most recent build)
3. **Semantic version**: `testnest-admin-api:1.0.0` (releases)
4. **Environment**: `testnest-admin-api:staging` (environment-specific)

### CI/CD Integration

- **Automatic builds**: Every push to master/develop
- **Tag with build ID**: Traceability to source code
- **Push to ACR**: Centralized image repository
- **Deploy from ACR**: Consistent deployment source

### Performance

- **Layer caching**: Order Dockerfile commands wisely
- **Minimize layers**: Combine RUN commands where possible
- **Use .dockerignore**: Reduce build context size
- **Multi-stage builds**: Smaller final images

## References

- [Docker Best Practices](https://docs.docker.com/develop/develop-images/dockerfile_best-practices/)
- [Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/)
- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Multi-stage builds](https://docs.docker.com/develop/develop-images/multistage-build/)
