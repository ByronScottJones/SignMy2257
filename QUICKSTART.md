# Quick Start Guide

Get the FBI Form 2257 System up and running in 5 minutes.

## Prerequisites

- Docker and Docker Compose installed
- 2GB free disk space
- Port 5000 available

## Option 1: Docker Compose (Recommended)

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/SignMy2257.git
cd SignMy2257
```

### 2. Start the Application

```bash
docker-compose up -d
```

### 3. Wait for Startup

```bash
sleep 5
docker-compose logs
```

### 4. Access the Application

- **Main Site**: http://localhost:5000
- **API Docs**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

### 5. Stop the Application

```bash
docker-compose down
```

## Option 2: .NET CLI (Local Development)

### 1. Prerequisites

- .NET 9 SDK

### 2. Clone Repository

```bash
git clone https://github.com/yourusername/SignMy2257.git
cd SignMy2257
```

### 3. Run Application

```bash
dotnet run
```

Application starts on http://localhost:5000

### 4. Build and Publish

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

## First Use

### As a Producer

1. Open http://localhost:5000 in your browser
2. Click "Create Producer Form"
3. Fill in your company and production information
4. Click "Create 2257 Form URL for Performers"
5. Copy the generated URL
6. Share this URL with your performers

### As a Performer

1. Open the URL provided by the producer
2. Your producer's information will be pre-filled
3. Fill in your personal information
4. Upload photos of your ID (front, back, with your face)
5. Click "Complete Form"
6. Your Form 2257 PDF will automatically download

## Configuration

### Environment Variables

Set these for Docker:

```yaml
environment:
  - Storage__BasePath=/app/data
  - RateLimiting__Enabled=true
  - RateLimiting__RequestsPerWindow=100
  - RateLimiting__WindowSizeInSeconds=60
```

### appsettings.json

Edit for .NET CLI:

```json
{
  "Storage": {
    "BasePath": "./Data"
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerWindow": 100,
    "WindowSizeInSeconds": 60
  }
}
```

## Troubleshooting

### Port 5000 Already in Use

```bash
# Change port in docker-compose.yml
ports:
  - "5001:5000"  # Use 5001 instead
```

### Docker Won't Start

```bash
# Check Docker daemon
docker ps

# View logs
docker-compose logs

# Restart Docker
docker restart
```

### Volumes Not Persisting

```bash
# Check volume
docker volume ls
docker volume inspect form2257-data

# Verify mount
docker-compose ps
```

### Forms Not Saving

```bash
# Check directory permissions
docker exec form2257-app ls -la /app/data

# Check disk space
docker exec form2257-app df -h /app/data
```

## API Quick Reference

### Create Producer Form

```bash
curl -X POST http://localhost:5000/api/forms/producer \
  -H "Content-Type: application/json" \
  -d '{
    "fullLegalName": "John Doe",
    "companyName": "Production Inc",
    "address": "123 Main St",
    "productionName": "Project X",
    "dateOfProduction": "2026-04-24",
    "locationOfProduction": "Los Angeles, CA"
  }'
```

### Upload Image

```bash
curl -X POST http://localhost:5000/api/forms/images/{formId}?imageType=IDFront \
  -F "file=@id_front.jpg"
```

### Complete Form

```bash
curl -X POST http://localhost:5000/api/forms/validate-complete/{formId} \
  --output form2257.pdf
```

## Data Storage

Forms and images are stored in:

```
/app/data/
└── {uuid}/
    ├── producer.json
    ├── performer.json
    ├── Form_2257_{uuid}_{timestamp}.pdf
    └── images/
        ├── {filename}_IDFront.jpg
        ├── {filename}_IDBack.jpg
        └── {filename}_IDFace.jpg
```

## Health Monitoring

Check application status:

```bash
# Health check
curl http://localhost:5000/health

# Statistics
curl http://localhost:5000/api/status
```

## Logs

View logs in real-time:

```bash
# Docker
docker-compose logs -f

# Local files
tail -f logs/app-*.txt
```

## Next Steps

- Read [README.md](README.md) for detailed documentation
- See [INSTALLATION.md](INSTALLATION.md) for advanced setup
- Check [DEPLOYMENT.md](DEPLOYMENT.md) for production deployment
- Review [ARCHITECTURE.md](ARCHITECTURE.md) for technical details

## Support

- Issues: Open GitHub issue with details
- Documentation: See [README.md](README.md)
- Contributing: See [CONTRIBUTING.md](CONTRIBUTING.md)
- Security: Email security@yourdomain.com

---

**Ready to go!** You now have a working FBI Form 2257 system. 🎉

