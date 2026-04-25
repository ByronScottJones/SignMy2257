# Deployment Guide

This guide provides comprehensive deployment procedures for the FBI Form 2257 System in various environments.

## Table of Contents

1. [Development Environment](#development-environment)
2. [Staging Environment](#staging-environment)
3. [Production Environment](#production-environment)
4. [Continuous Deployment](#continuous-deployment)
5. [Monitoring and Maintenance](#monitoring-and-maintenance)

## Development Environment

### Local Setup

See [INSTALLATION.md](INSTALLATION.md#local-development) for detailed setup instructions.

### Configuration

Development settings in `appsettings.Development.json`:
- Logging level: Debug
- Rate limiting: Disabled or very high limits
- Storage path: Local `./Data` directory
- HTTPS: Disabled

### Running

```bash
dotnet run
```

Application starts on http://localhost:5000 with hot-reload enabled.

## Staging Environment

### Deployment Steps

#### 1. Prepare Staging Server

```bash
# Install prerequisites
sudo apt-get update
sudo apt-get install -y docker.io docker-compose curl

# Create deployment directory
sudo mkdir -p /opt/form2257
sudo chown $USER:$USER /opt/form2257
cd /opt/form2257
```

#### 2. Clone Repository

```bash
git clone https://github.com/yourusername/SignMy2257.git .
```

#### 3. Create Staging Configuration

Create `.env.staging`:

```env
ASPNETCORE_ENVIRONMENT=Staging
ASPNETCORE_URLS=http://+:5000
Storage__BasePath=/app/data
RateLimiting__Enabled=true
RateLimiting__RequestsPerWindow=500
RateLimiting__WindowSizeInSeconds=60
```

#### 4. Deploy with Docker Compose

```bash
docker-compose -f docker-compose.yml \
  --env-file .env.staging \
  up -d
```

#### 5. Verify Deployment

```bash
# Check container status
docker-compose ps

# Check health
curl http://localhost:5000/health

# View logs
docker-compose logs -f
```

### Staging Environment Configuration

`appsettings.Staging.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "Storage": {
    "BasePath": "/app/data"
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerWindow": 500,
    "WindowSizeInSeconds": 60
  }
}
```

## Production Environment

### Pre-Deployment Checklist

```markdown
□ All tests passing locally and in CI/CD
□ Code reviewed and approved
□ Security scan completed without critical issues
□ Performance testing completed
□ Backup and recovery procedures documented
□ Monitoring and alerting configured
□ SSL/TLS certificates obtained and valid
□ Database migrations reviewed (if applicable)
□ Rate limiting settings appropriate for expected load
□ Storage path provisioned with adequate space
□ Disaster recovery plan in place
□ Documentation updated
□ Runbook created for common issues
```

### Production Deployment Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Users/Performers/Producers                                   │
└────────────────────────┬────────────────────────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │ CloudFlare/AWS CloudFront       │
        │ (CDN & DDoS Protection)         │
        └────────────────┬────────────────┘
                         │
        ┌────────────────┴────────────────┐
        │ AWS Route 53 / Azure DNS        │
        │ (Load Balancing)                │
        └────────────────┬────────────────┘
                         │
    ┌────────────────────┼────────────────────┐
    │                    │                    │
┌───▼──────┐        ┌────▼─────┐        ┌───▼──────┐
│ Nginx #1 │        │ Nginx #2 │        │ Nginx #3 │
│ (SSL)    │        │ (SSL)    │        │ (SSL)    │
└───┬──────┘        └────┬─────┘        └───┬──────┘
    │                    │                   │
    └────────────────────┼───────────────────┘
                         │
    ┌────────────────────┼───────────────────┐
    │                    │                   │
┌───▼──────┐        ┌────▼─────┐        ┌───▼──────┐
│ App #1   │        │ App #2   │        │ App #3   │
│ Container│        │ Container│        │ Container│
└───┬──────┘        └────┬─────┘        └───┬──────┘
    │                    │                   │
    └────────────────────┼───────────────────┘
                         │
         ┌───────────────┴───────────────┐
         │                               │
    ┌────▼──────┐               ┌────────▼──┐
    │ NFS Share │               │ EBS Volume│
    │ /data     │               │ /logs     │
    └───────────┘               └───────────┘
```

### Production Deployment Steps

#### 1. Infrastructure Setup

**Using AWS:**

```bash
# Create EC2 instances
aws ec2 run-instances \
  --image-id ami-0c55b159cbfafe1f0 \
  --instance-type t3.large \
  --key-name production \
  --security-groups form2257-sg \
  --count 3

# Create EBS volume for data
aws ec2 create-volume \
  --size 100 \
  --availability-zone us-east-1a \
  --volume-type gp3

# Create NFS share
aws efs create-file-system \
  --performance-mode generalPurpose \
  --throughput-mode bursting
```

#### 2. Configure Load Balancer

**AWS Application Load Balancer:**

```bash
aws elbv2 create-load-balancer \
  --name form2257-alb \
  --subnets subnet-12345678 subnet-87654321 \
  --security-groups sg-12345678 \
  --scheme internet-facing \
  --type application

aws elbv2 create-target-group \
  --name form2257-targets \
  --protocol HTTP \
  --port 5000 \
  --vpc-id vpc-12345678

# Register instances
aws elbv2 register-targets \
  --target-group-arn arn:aws:elasticloadbalancing:... \
  --targets Id=i-1234567890abcdef0 Id=i-0987654321abcdef0
```

#### 3. Deploy Application

```bash
# SSH into instance
ssh -i production.pem ubuntu@instance-ip

# Clone repository
git clone https://github.com/yourusername/SignMy2257.git
cd SignMy2257

# Create environment file
cat > .env.production << EOF
ASPNETCORE_ENVIRONMENT=Production
Storage__BasePath=/mnt/nfs/data
Storage__BackupPath=/mnt/backup/data
RateLimiting__Enabled=true
RateLimiting__RequestsPerWindow=100
RateLimiting__WindowSizeInSeconds=60
EOF

# Deploy
docker-compose -f docker-compose.yml \
  --env-file .env.production \
  up -d
```

#### 4. Configure SSL/TLS

**Using Let's Encrypt with Certbot:**

```bash
sudo apt-get install -y certbot python3-certbot-nginx

sudo certbot certonly --standalone \
  -d yourdomain.com \
  -d www.yourdomain.com \
  --email admin@yourdomain.com \
  --agree-tos \
  --non-interactive
```

**Update Nginx:**

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    return 301 https://$server_name$request_uri;
}
```

#### 5. Setup Monitoring

**CloudWatch Metrics:**

```bash
# Monitor CPU, Memory, Disk
aws cloudwatch put-metric-alarm \
  --alarm-name form2257-high-cpu \
  --alarm-description "Alert when CPU usage > 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/EC2 \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold

# Monitor Disk Space
aws cloudwatch put-metric-alarm \
  --alarm-name form2257-low-disk \
  --alarm-description "Alert when disk < 20% free" \
  --metric-name DiskUsedPercent \
  --namespace System/Linux \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold
```

## Continuous Deployment

### GitHub Actions CI/CD Pipeline

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build -c Release
      
      - name: Test
        run: dotnet test
      
      - name: Build Docker Image
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: |
          docker build -t ${{ secrets.DOCKER_REGISTRY }}/form2257:${{ github.sha }} .
          docker tag ${{ secrets.DOCKER_REGISTRY }}/form2257:${{ github.sha }} \
                     ${{ secrets.DOCKER_REGISTRY }}/form2257:latest
      
      - name: Push Docker Image
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: |
          echo ${{ secrets.DOCKER_PASSWORD }} | docker login -u ${{ secrets.DOCKER_USERNAME }} --password-stdin
          docker push ${{ secrets.DOCKER_REGISTRY }}/form2257:${{ github.sha }}
          docker push ${{ secrets.DOCKER_REGISTRY }}/form2257:latest
      
      - name: Deploy to Production
        if: github.event_name == 'push' && github.ref == 'refs/heads/main'
        run: |
          echo ${{ secrets.DEPLOY_KEY }} > /tmp/deploy_key
          chmod 600 /tmp/deploy_key
          ssh -i /tmp/deploy_key -o StrictHostKeyChecking=no \
            ubuntu@${{ secrets.PROD_SERVER }} \
            './scripts/deploy.sh'

```

### Manual Deployment Script

Create `scripts/deploy.sh`:

```bash
#!/bin/bash
set -e

echo "Starting deployment..."

cd /opt/form2257

# Pull latest changes
git pull origin main

# Pull latest image
docker pull form2257:latest

# Stop old containers
docker-compose down

# Start new containers
docker-compose up -d

# Wait for health check
sleep 10
curl -f http://localhost:5000/health || exit 1

echo "Deployment complete!"
```

## Monitoring and Maintenance

### Health Monitoring

```bash
#!/bin/bash
# Monitor health every 5 minutes

while true; do
    response=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health)
    if [ $response -ne 200 ]; then
        echo "Health check failed! Status: $response"
        # Send alert
        curl -X POST https://hooks.slack.com/services/YOUR/WEBHOOK/URL \
          -d '{"text":"Form 2257 health check failed!"}'
    fi
    sleep 300
done
```

### Log Management

**Rotate and Archive Logs:**

```bash
#!/bin/bash
# Daily log archival

LOGS_DIR="/opt/form2257/logs"
ARCHIVE_DIR="/opt/form2257/logs/archive"

mkdir -p $ARCHIVE_DIR

# Compress logs older than 1 day
find $LOGS_DIR -name "app-*.txt" -mtime +1 -exec \
  gzip -9 {} \; -exec mv {}.gz $ARCHIVE_DIR \;

# Delete archives older than 30 days
find $ARCHIVE_DIR -name "*.txt.gz" -mtime +30 -delete

echo "Log rotation complete"
```

Add to crontab:
```
0 1 * * * /opt/form2257/scripts/rotate-logs.sh
```

### Backup Procedures

```bash
#!/bin/bash
# Daily backup

BACKUP_DIR="/opt/form2257/backups"
DATA_DIR="/opt/form2257/data"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR

# Create backup
tar -czf "$BACKUP_DIR/form2257_$DATE.tar.gz" $DATA_DIR

# Upload to S3
aws s3 cp "$BACKUP_DIR/form2257_$DATE.tar.gz" \
  s3://form2257-backups/

# Cleanup local backups older than 7 days
find $BACKUP_DIR -name "*.tar.gz" -mtime +7 -delete

echo "Backup complete"
```

### Disaster Recovery

**Recovery Procedure:**

```bash
# 1. Stop current application
docker-compose down

# 2. Restore from backup
cd /tmp
aws s3 cp s3://form2257-backups/form2257_YYYYMMDD_HHMMSS.tar.gz .
tar -xzf form2257_YYYYMMDD_HHMMSS.tar.gz

# 3. Copy restored data
sudo cp -r data/* /opt/form2257/data/

# 4. Restart application
cd /opt/form2257
docker-compose up -d

# 5. Verify
curl http://localhost:5000/health
```

---

**Last Updated**: April 24, 2026  
**Version**: 1.0.0
