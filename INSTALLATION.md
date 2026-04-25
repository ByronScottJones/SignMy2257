# Installation Guide

This guide covers various installation and deployment scenarios for the FBI Form 2257 System.

## Table of Contents

1. [Local Development](#local-development)
2. [Docker Deployment](#docker-deployment)
3. [Production Deployment](#production-deployment)
4. [Cloud Deployment](#cloud-deployment)
5. [Troubleshooting](#troubleshooting)

## Local Development

### System Requirements

- **Operating System**: Windows 10+, macOS 10.15+, or Linux (Ubuntu 20.04+)
- **.NET SDK**: 9.0 or later
- **RAM**: 2GB minimum
- **Disk Space**: 5GB (for development tools and dependencies)
- **Git**: For version control

### Installation Steps

#### 1. Install .NET 9 SDK

**Windows:**
```bash
# Using Windows Package Manager
winget install Microsoft.DotNet.SDK.9

# Or download from: https://dotnet.microsoft.com/download
```

**macOS:**
```bash
# Using Homebrew
brew install dotnet-sdk

# Or download from: https://dotnet.microsoft.com/download
```

**Linux (Ubuntu/Debian):**
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0
```

#### 2. Clone Repository

```bash
git clone https://github.com/yourusername/SignMy2257.git
cd SignMy2257
```

#### 3. Restore Dependencies

```bash
dotnet restore
```

#### 4. Build Application

```bash
dotnet build
```

#### 5. Run Application

```bash
dotnet run
```

The application will start on `http://localhost:5000`

#### 6. Access Application

- **Main Site**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **Status Page**: http://localhost:5000/api/status

### Development Workflow

#### Running with Hot Reload

```bash
dotnet watch run
```

Changes to `.cs`, `.razor`, and static files will automatically reload.

#### Debugging

Using Visual Studio Code:

1. Install C# Dev Kit extension
2. Open project in VS Code
3. Press `F5` to start debugging
4. Set breakpoints as needed

Using Visual Studio:

1. Open `SignMy2257.sln` (or create one with `dotnet new sln`)
2. Press `F5` to start debugging

#### Viewing Logs

Logs are written to:
- **Console**: Real-time output in terminal
- **Files**: `logs/app-{date}.txt`

To view file logs:
```bash
tail -f logs/app-*.txt
```

## Docker Deployment

### Prerequisites

- Docker 20.10+
- Docker Compose 2.0+ (optional but recommended)

### Installation

#### Using Docker Compose (Recommended)

1. **Build and Run:**
   ```bash
   docker-compose up -d
   ```

2. **Check Status:**
   ```bash
   docker-compose ps
   docker-compose logs -f
   ```

3. **Access Application:**
   - Main: http://localhost:5000
   - Health: http://localhost:5000/health

4. **Stop Application:**
   ```bash
   docker-compose down
   ```

#### Using Docker CLI

1. **Build Image:**
   ```bash
   docker build -t form2257:latest .
   ```

2. **Create Volumes:**
   ```bash
   docker volume create form2257-data
   docker volume create form2257-logs
   ```

3. **Run Container:**
   ```bash
   docker run -d \
     --name form2257-app \
     -p 5000:5000 \
     -v form2257-data:/app/data \
     -v form2257-logs:/app/logs \
     -e Storage__BasePath=/app/data \
     -e RateLimiting__Enabled=true \
     form2257:latest
   ```

4. **Check Health:**
   ```bash
   docker logs form2257-app
   curl http://localhost:5000/health
   ```

5. **Stop Container:**
   ```bash
   docker stop form2257-app
   docker remove form2257-app
   ```

### Docker Configuration

#### Environment Variables

```yaml
services:
  form2257:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Storage__BasePath=/app/data
      - RateLimiting__Enabled=true
      - RateLimiting__RequestsPerWindow=100
      - RateLimiting__WindowSizeInSeconds=60
```

#### Volume Mounting

Persistent data requires volume mounts:

```bash
# Named volumes (recommended for production)
-v form2257-data:/app/data
-v form2257-logs:/app/logs

# Bind mounts (for development)
-v /local/path/data:/app/data
-v /local/path/logs:/app/logs
```

## Production Deployment

### Pre-Deployment Checklist

- [ ] All tests passing
- [ ] Security scan completed
- [ ] Performance testing done
- [ ] Backup strategy defined
- [ ] Monitoring configured
- [ ] SSL/TLS certificates obtained
- [ ] Rate limiting configured appropriately
- [ ] Storage path configured
- [ ] Logging configured

### Production Configuration

#### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Storage": {
    "BasePath": "/mnt/data/forms"
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerWindow": 50,
    "WindowSizeInSeconds": 60
  }
}
```

#### Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443
Storage__BasePath=/mnt/data/forms
Storage__BackupPath=/mnt/backups/forms
RateLimiting__Enabled=true
RateLimiting__RequestsPerWindow=50
RateLimiting__WindowSizeInSeconds=60
```

### Nginx Reverse Proxy Configuration

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/nginx/certs/certificate.crt;
    ssl_certificate_key /etc/nginx/certs/private.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://form2257:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
    }
}
```

### Systemd Service Configuration

Create `/etc/systemd/system/form2257.service`:

```ini
[Unit]
Description=FBI Form 2257 System
After=network.target
Wants=network-online.target

[Service]
Type=notify
User=www-data
WorkingDirectory=/app/form2257
ExecStart=/usr/bin/dotnet /app/form2257/SignMy2257.dll
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="Storage__BasePath=/data/forms"

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl daemon-reload
sudo systemctl enable form2257
sudo systemctl start form2257
```

### Backup Strategy

Daily backup of form data:

```bash
#!/bin/bash
BACKUP_DIR="/mnt/backups/forms"
DATA_DIR="/mnt/data/forms"
DATE=$(date +%Y%m%d_%H%M%S)

tar -czf "$BACKUP_DIR/forms_$DATE.tar.gz" "$DATA_DIR"

# Keep only last 30 days
find "$BACKUP_DIR" -name "forms_*.tar.gz" -mtime +30 -delete
```

Add to crontab:
```
0 2 * * * /usr/local/bin/backup-forms.sh
```

## Cloud Deployment

### Azure Container Instances

```bash
az container create \
  --resource-group myResourceGroup \
  --name form2257-container \
  --image form2257:latest \
  --cpu 1 \
  --memory 2 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    Storage__BasePath=/app/data \
  --port 5000 \
  --protocol TCP \
  --azure-file-volume-account-name myaccount \
  --azure-file-volume-account-key mykey \
  --azure-file-volume-share-name data \
  --azure-file-volume-mount-path /app/data
```

### Docker Hub Registry

```bash
# Tag image
docker tag form2257:latest yourdockerusername/form2257:latest
docker tag form2257:latest yourdockerusername/form2257:1.0.0

# Push to registry
docker push yourdockerusername/form2257:latest
docker push yourdockerusername/form2257:1.0.0
```

## Troubleshooting

### Application Won't Start

1. Check .NET installation:
   ```bash
   dotnet --version
   ```

2. Check port availability:
   ```bash
   # Windows
   netstat -ano | findstr :5000
   
   # Linux/macOS
   lsof -i :5000
   ```

3. Check logs:
   ```bash
   tail -f logs/app-*.txt
   ```

### Forms Not Saving

1. Verify storage path exists and is writable:
   ```bash
   ls -la /app/data
   chmod 755 /app/data
   ```

2. Check disk space:
   ```bash
   df -h /app/data
   ```

3. Verify permissions:
   ```bash
   # Docker
   docker exec form2257-app ls -la /app/data
   ```

### Rate Limiting Issues

1. Check current configuration:
   ```bash
   curl http://localhost:5000/api/status
   ```

2. Adjust in docker-compose.yml:
   ```yaml
   environment:
     - RateLimiting__RequestsPerWindow=200
     - RateLimiting__WindowSizeInSeconds=60
   ```

3. Reload service:
   ```bash
   docker-compose restart
   ```

### High Memory Usage

1. Check for large files:
   ```bash
   du -sh /app/data/*
   ```

2. Reduce garbage collection pressure:
   ```bash
   DOTNET_GCHeapCount=1
   ```

3. Monitor application:
   ```bash
   docker stats form2257-app
   ```

### SSL/TLS Issues

1. Verify certificate:
   ```bash
   openssl x509 -in certificate.crt -text -noout
   ```

2. Check Nginx configuration:
   ```bash
   nginx -t
   sudo systemctl restart nginx
   ```

## Performance Tuning

### Docker Resource Limits

```yaml
services:
  form2257:
    cpus: '1'
    mem_limit: 2g
```

### Connection Pooling

Adjust in configuration:
```json
{
  "ConnectionStrings": {
    "MaxPoolSize": 20
  }
}
```

### Caching Headers

For static files in Nginx:
```nginx
location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

## Support

For issues or questions:
- Check the [README.md](README.md)
- Review [CONTRIBUTING.md](CONTRIBUTING.md)
- Open an GitHub issue with details
- Contact the development team

---

**Last Updated**: April 24, 2026
