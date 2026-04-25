# FBI Form 2257 Compliance System

A secure, web-based application for managing FBI Form 2257 submissions in compliance with federal law (18 U.S.C. § 2257).

## Features

- **Producer Portal**: Create production forms with company and production information
- **Performer Portal**: Complete forms with personal information and identity document uploads
- **PDF Generation**: Automatic generation of compliant Form 2257 PDFs with metadata
- **Image Management**: Upload and store ID photos (front, back, and face) with optional cropping
- **Rate Limiting**: Configurable rate limiting to prevent abuse
- **Health Monitoring**: Built-in health checks and usage statistics
- **Secure Storage**: Local file-based storage with UUID-based organization
- **Logging**: Comprehensive request and error logging with Serilog
- **Docker Ready**: Production-ready Docker setup with volume mounting

## Architecture

This application is built with:

- **.NET 9** with Minimal APIs and Razor Components
- **QuestPDF** for PDF generation
- **Serilog** for structured logging
- **Bootstrap 5** for responsive UI
- **Docker** for containerization

## Getting Started

### Prerequisites

- .NET 9 SDK
- Docker & Docker Compose (optional)
- 2GB+ free disk space for form storage

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/SignMy2257.git
   cd SignMy2257
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the application**
   - Open `http://localhost:5000` in your browser
   - API documentation available at `http://localhost:5000/swagger`
   - Status page at `http://localhost:5000/api/status`

### Docker Deployment

**Using Docker Compose (Recommended):**

```bash
docker-compose up -d
```

The application will be available at `http://localhost:5000`

**Using Docker CLI:**

```bash
docker build -t form2257 .
docker run -d \
  -p 5000:5000 \
  -v form2257-data:/app/data \
  -v form2257-logs:/app/logs \
  -e Storage__BasePath=/app/data \
  form2257
```

## Configuration

### appsettings.json

```json
{
  "Storage": {
    "BasePath": "/app/data"  // Path for form storage
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerWindow": 100,      // Requests allowed per window
    "WindowSizeInSeconds": 60      // Time window in seconds
  }
}
```

### Environment Variables

- `Storage__BasePath`: Directory for storing forms (default: `/app/data`)
- `RateLimiting__Enabled`: Enable/disable rate limiting (default: `true`)
- `RateLimiting__RequestsPerWindow`: Max requests per window (default: `100`)
- `RateLimiting__WindowSizeInSeconds`: Rate limit window size (default: `60`)

## API Endpoints

### Producer Forms

**Create Producer Form**
```
POST /api/forms/producer
Content-Type: application/json

{
  "fullLegalName": "John Doe",
  "companyName": "Production Company Inc",
  "address": "123 Main St, City, State 12345",
  "productionName": "Project Name",
  "dateOfProduction": "2026-04-24",
  "locationOfProduction": "City, State"
}
```

Response:
```json
{
  "formId": "550e8400-e29b-41d4-a716-446655440000",
  "formUrl": "http://localhost:5000/form?id=550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2026-04-24T10:00:00Z"
}
```

**Get Producer Form**
```
GET /api/forms/producer/{formId}
```

### Performer Forms

**Submit Performer Form**
```
POST /api/forms/performer/{formId}
Content-Type: application/json

{
  "fullLegalName": "Jane Smith",
  "aliases": "JS Productions",
  "stageName": "Jane",
  "dateOfBirth": "1995-06-15",
  "address": "456 Oak Ave, City, State 54321",
  "socialMediaAccounts": [
    {
      "platform": "Twitter",
      "accountName": "@janesmith",
      "url": "https://twitter.com/janesmith"
    }
  ]
}
```

**Upload Image**
```
POST /api/forms/images/{formId}?imageType=IDFront
Content-Type: multipart/form-data

file: <image file (JPG or PNG, max 2MB)>
```

**Validate and Complete Form (Generate PDF)**
```
POST /api/forms/validate-complete/{formId}
```

Returns PDF file download

### Health & Status

**Health Check**
```
GET /health
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2026-04-24T10:00:00Z",
  "version": "1.0.0"
}
```

**Application Status**
```
GET /api/status
```

Response:
```json
{
  "status": "operational",
  "timestamp": "2026-04-24T10:00:00Z",
  "statistics": {
    "totalFormsCreated": 42,
    "completedForms": 38,
    "pendingForms": 4,
    "lastFormCreated": "2026-04-24T09:30:00Z",
    "requestsPerHour": { ... }
  }
}
```

## File Storage Structure

Forms are stored locally with the following structure:

```
/app/data/
└── {UUID}/
    ├── producer.json           # Producer information
    ├── performer.json          # Performer information
    ├── Form_2257_{UUID}_{timestamp}.pdf    # Generated PDF
    └── images/
        ├── {filename}_IDFront.jpg
        ├── {filename}_IDBack.jpg
        └── {filename}_IDFace.jpg
```

## Image Upload Requirements

- **Formats**: JPG, PNG
- **Maximum size**: 2MB per image
- **Required images**: 3 (ID Front, ID Back, ID with Face)
- **Storage**: Saved with PDF as `{pdfname}_IDFront`, `{pdfname}_IDBack`, `{pdfname}_IDFace`

## Form Validation

### Producer Form Validation

- ✓ Full Legal Name (required)
- ✓ Company Name (required)
- ✓ Address (required)
- ✓ Production Name (required)
- ✓ Date of Production (required)
- ✓ Location of Production (required)

### Performer Form Validation

- ✓ Full Legal Name (required)
- ✓ Stage Name (required)
- ✓ Date of Birth (required, must be 18+)
- ✓ Address (required)
- ✓ All three ID images (required)

## PDF Output

Generated PDFs include:

- Complete producer information
- Complete performer information
- All submitted documents
- Legal attestation statement
- Metadata (generation timestamp, submission date)
- Form fields for manual annotation if needed

## Security Considerations

⚠️ **Important Legal Notice:**

This system helps with Form 2257 compliance as required under 18 U.S.C. § 2257. Users are responsible for:

- Ensuring all information is accurate and truthful
- Verifying performer age (18+) through appropriate means
- Maintaining records for inspection by Department of Justice
- Complying with all federal and local regulations

Violations of federal law may result in severe penalties.

## Rate Limiting

The application includes configurable rate limiting to prevent abuse:

- **Default**: 100 requests per 60 seconds per IP
- **Configuration**: Set in `appsettings.json` or environment variables
- **Headers**: Rate limit info returned in response headers
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Remaining requests
  - `Retry-After`: Seconds to wait if rate limited

## Logging

Logs are written to:

- **Console**: Real-time output
- **Files**: `logs/app-{date}.txt` with daily rotation
- **Level**: Configurable (Information by default)

Request logging includes:
- HTTP method and path
- Response status code
- Request duration
- Error details if applicable

## Monitoring & Health Checks

The application provides:

- **Health endpoint** (`/health`): Returns operational status
- **Status page** (`/api/status`): Shows form statistics and request metrics
- **Docker health check**: Automatic container health monitoring
- **Structured logging**: Detailed request/response tracking

## Development

### Building the Application

```bash
dotnet build -c Release
```

### Running Tests

Tests can be added in a `Tests` directory following the structure:

```bash
dotnet test
```

### Publishing

```bash
dotnet publish -c Release -o ./publish
```

## Deployment

### Kubernetes (Optional)

Example manifests can be created for Kubernetes deployment. Contact the development team for Helm charts.

### Azure Container Instances

```bash
az container create \
  --resource-group myResourceGroup \
  --name form2257-container \
  --image form2257:latest \
  --cpu 1 \
  --memory 2 \
  --ports 5000 \
  --environment-variables Storage__BasePath=/app/data
```

### Environment Variables for Production

```bash
ASPNETCORE_ENVIRONMENT=Production
Storage__BasePath=/app/data
RateLimiting__RequestsPerWindow=50
RateLimiting__WindowSizeInSeconds=60
```

## Troubleshooting

### Forms not saving

- Check that `/app/data` volume is properly mounted
- Verify application has write permissions
- Check logs in `/app/logs`

### Rate limiting too restrictive

- Adjust `RateLimiting__RequestsPerWindow` in configuration
- Increase `RateLimiting__WindowSizeInSeconds` for longer windows

### PDF generation fails

- Ensure QuestPDF license is valid
- Check available disk space
- Verify all required fonts are available

### Images not uploading

- Check file size (must be < 2MB)
- Verify format (JPG or PNG only)
- Check disk space availability

## Support & Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.

## License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

## Disclaimer

This application is provided as-is for Form 2257 compliance purposes. Users are solely responsible for ensuring compliance with all federal, state, and local laws regarding adult content verification and record retention.

The developers and maintainers of this application are not liable for any misuse or legal violations resulting from use of this software.

---

**Version**: 1.0.0  
**Last Updated**: April 24, 2026  
**Status**: Production Ready
