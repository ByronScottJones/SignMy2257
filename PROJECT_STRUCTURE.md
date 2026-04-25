# Complete Project Structure Reference

This document describes every file in the FBI Form 2257 System application.

## 📁 Root Level Files

### Configuration & Setup

| File | Purpose |
|------|---------|
| `SignMy2257.csproj` | Project file with NuGet dependencies |
| `SignMy2257.sln` | Visual Studio solution file |
| `appsettings.json` | Production configuration |
| `appsettings.Development.json` | Development configuration |
| `.gitignore` | Git ignore rules |
| `.markdownlintrc` | Markdown linting rules |

### Application Startup

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point, DI setup, middleware configuration |
| `App.razor` | Blazor application root |
| `Routes.razor` | Route definitions |

### Container & Deployment

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage Docker build |
| `docker-compose.yml` | Docker Compose orchestration |

## 📁 Directories

### `/API` - REST API Endpoints

```
FormEndpoints.cs (250 lines)
├── POST /api/forms/producer
├── GET /api/forms/producer/{formId}
├── POST /api/forms/performer/{formId}
├── POST /api/forms/images/{formId}
├── POST /api/forms/validate-complete/{formId}
└── GET /api/forms/form/{formId}

HealthAndStatusEndpoints.cs (40 lines)
├── GET /health
└── GET /api/status
```

### `/Services` - Business Logic

```
FormStorageService.cs (100 lines)
├── SaveProducerFormAsync() - Create UUID & save producer data
├── LoadProducerFormAsync() - Retrieve producer form
├── SavePerformerFormAsync() - Save performer data
├── SaveImageAsync() - Store uploaded images
└── GetFormStoragePath() - Get storage directory

PdfGenerationService.cs (150 lines)
├── GenerateForm2257Async() - Create PDF with all form fields
├── FormField() - Add form field to PDF
├── LegalStatement() - Add legal text
└── ImagesSection() - Add image references to PDF

ValidationService.cs (100 lines)
├── ValidateProducerForm() - Check required producer fields
├── ValidatePerformerForm() - Check performer data & age (18+)
└── ValidateImages() - Verify all 3 ID images uploaded
```

### `/Models` - Data Models

```
ProducerData.cs (20 lines)
└── Full Legal Name, Company, Address, Production Details

PerformerData.cs (20 lines)
├── Full Legal Name, Aliases, Stage Name
├── Date of Birth, Address
└── Social Media Accounts (optional)

FormRequest.cs (50 lines)
├── ProducerFormRequest
├── PerformerFormRequest
├── ProducerFormResponse
├── ImageUploadRequest
├── FormValidationResponse
└── StatisticsResponse
```

### `/Pages` - Razor Components (SPA)

```
Index.razor (80 lines)
└── Home page with producer/performer selection

Form.razor (350 lines)
├── Producer Form View
│   ├── Full Legal Name, Company Name
│   ├── Address, Production Name
│   ├── Date & Location of Production
│   └── "Create 2257 Form URL" Button
│
└── Performer Form View
    ├── Producer Info Display (pre-filled)
    ├── Full Legal Name, Aliases, Stage Name
    ├── Date of Birth, Address validation
    ├── Image Upload (Front, Back, Face)
    ├── Image Previews
    ├── Social Media Fields (optional)
    └── "Complete Form" Button
        ├── Validate all fields
        ├── Verify age (18+)
        ├── Generate PDF
        └── Download + Save copy
```

### `/Middleware` - HTTP Processing

```
RateLimitingMiddleware.cs (80 lines)
├── IP-based rate limiting
├── Configurable: requests per window
├── Configurable: window duration
└── Returns 429 when exceeded

RequestLoggingMiddleware.cs (50 lines)
├── Logs all HTTP requests
├── Logs response status codes
├── Records request duration
└── Logs errors with full context
```

### `/wwwroot` - Static Assets

```
index.html (50 lines)
├── Bootstrap 5 CDN
├── Font Awesome CDN
├── Blazor framework script
└── File download helper

css/
├── app.css - Application styling
│   ├── Theme colors
│   ├── Form styling
│   └── Responsive design
│
└── bootstrap.css - Font setup

js/
└── (Optional custom JavaScript)
```

### `/.github/workflows` - CI/CD

```
ci-cd.yml (120 lines)
├── Build Job
│   ├── Setup .NET 9
│   ├── Restore dependencies
│   ├── Build release
│   └── Run tests
│
├── Lint Job
│   ├── Markdown linting
│   └── YAML linting
│
├── Code Quality Job
│   └── SonarCloud analysis
│
├── Deploy Staging Job
│   └── Deploy to staging on develop branch
│
└── Deploy Production Job
    ├── Create release tag
    └── Deploy to production on main branch
```

### `/scripts` - Utility Scripts

```
build-and-run.sh (30 lines)
├── Check .NET SDK
├── Restore dependencies
├── Build application
├── Run tests
└── Start app

docker-build.sh (20 lines)
├── Build Docker image
├── Tag image
└── Show deployment instructions
```

### `/Data` - Local Storage (Runtime)

```
{uuid}/
├── producer.json - Producer info
├── performer.json - Performer info
├── Form_2257_{uuid}_{timestamp}.pdf - Generated PDF
└── images/
    ├── {filename}_IDFront.jpg
    ├── {filename}_IDBack.jpg
    └── {filename}_IDFace.jpg
```

## 📄 Documentation Files

### Getting Started

| File | Purpose | Length |
|------|---------|--------|
| `QUICKSTART.md` | 5-minute setup guide | 250 lines |
| `PROJECT_SUMMARY.md` | Project overview | 300 lines |
| `README.md` | Complete documentation | 400+ lines |

### Technical Guides

| File | Purpose | Length |
|------|---------|--------|
| `INSTALLATION.md` | Installation for all scenarios | 400 lines |
| `DEPLOYMENT.md` | Production deployment guide | 600 lines |
| `ARCHITECTURE.md` | Technical architecture | 400 lines |

### Development & Operations

| File | Purpose | Length |
|------|---------|--------|
| `CONTRIBUTING.md` | Contribution guidelines | 300 lines |
| `SECURITY.md` | Security policy & practices | 250 lines |
| `CHANGELOG.md` | Version history | 100 lines |
| `LICENSE` | MIT license + disclaimer | 50 lines |

## 🔑 Key Features by File

### Form Creation (Producer)
- **File**: `Pages/Form.razor` (lines 1-100)
- **API**: `API/FormEndpoints.cs` → CreateProducerForm()
- **Service**: `Services/FormStorageService.cs` → SaveProducerFormAsync()
- **Model**: `Models/FormRequest.cs` → ProducerFormRequest

### Form Completion (Performer)
- **File**: `Pages/Form.razor` (lines 100-350)
- **API**: `API/FormEndpoints.cs` → SubmitPerformerForm()
- **Service**: `Services/FormStorageService.cs` → SavePerformerFormAsync()
- **Model**: `Models/FormRequest.cs` → PerformerFormRequest

### Image Upload
- **File**: `Pages/Form.razor` (lines 200-250)
- **API**: `API/FormEndpoints.cs` → UploadImage()
- **Service**: `Services/FormStorageService.cs` → SaveImageAsync()
- **Validation**: Max 2MB, JPG/PNG only

### PDF Generation
- **API**: `API/FormEndpoints.cs` → ValidateAndComplete()
- **Service**: `Services/PdfGenerationService.cs` → GenerateForm2257Async()
- **Library**: QuestPDF (Community license)
- **Storage**: `/app/data/{uuid}/Form_2257_{uuid}_{timestamp}.pdf`

### Rate Limiting
- **Middleware**: `Middleware/RateLimitingMiddleware.cs`
- **Config**: `appsettings.json` → RateLimiting section
- **Method**: IP-based bucket algorithm
- **Default**: 100 requests per 60 seconds

### Logging
- **Middleware**: `Middleware/RequestLoggingMiddleware.cs`
- **Library**: Serilog
- **Output**: Console + File (`logs/app-*.txt`)
- **Rotation**: Daily

## 🚀 Build & Execution Flow

1. **Startup** → `Program.cs`
   - Register services
   - Configure middleware
   - Map endpoints

2. **Request** → Middleware Pipeline
   - RequestLoggingMiddleware: Log request
   - RateLimitingMiddleware: Check limits
   - Error handling

3. **Routing** → API Endpoints
   - FormEndpoints.cs
   - HealthAndStatusEndpoints.cs

4. **Business Logic** → Services
   - Validation
   - Storage
   - PDF Generation
   - Statistics

5. **Response** → Client
   - Status code
   - Data/JSON/PDF
   - Rate limit headers

## 📊 File Statistics

| Category | Count | LOC |
|----------|-------|-----|
| C# Source Files | 10 | ~1,500 |
| Razor Components | 2 | ~450 |
| Config Files | 4 | ~50 |
| Docker Files | 2 | ~100 |
| Documentation | 8 | ~2,500 |
| Scripts | 2 | ~50 |
| **Total** | **28** | **~4,650** |

## 🔒 Security by File

| Feature | File | Lines |
|---------|------|-------|
| Input Validation | `Services/ValidationService.cs` | 50-100 |
| Age Verification | `Services/ValidationService.cs` | 20-30 |
| File Validation | `API/FormEndpoints.cs` | 120-140 |
| Rate Limiting | `Middleware/RateLimitingMiddleware.cs` | 1-80 |
| Error Handling | `Program.cs` | Global |
| Logging | `Middleware/RequestLoggingMiddleware.cs` | 1-50 |

## ✅ Compilation Checklist

Before building, verify:

- [ ] All NuGet packages in `SignMy2257.csproj`
- [ ] .NET 9 SDK installed
- [ ] All using statements in C# files
- [ ] All files in `/Models` reference correctly
- [ ] Middleware registered in `Program.cs`
- [ ] Endpoints mapped in `Program.cs`
- [ ] Razor components have @page directives
- [ ] Docker files reference correct .NET version

## 🎯 Next Steps

1. **Build**: `dotnet build` or `docker-compose up -d`
2. **Test**: Access http://localhost:5000
3. **Deploy**: Follow `DEPLOYMENT.md`
4. **Configure**: Update `appsettings.json`
5. **Monitor**: Check `/health` and `/api/status`

---

**Total Project**: Production-ready, fully documented, immediately deployable.
