# FBI Form 2257 System - Architecture Overview

## System Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                      Client Browsers                         │
│                 (Producer & Performer)                       │
└────────────────────────┬─────────────────────────────────────┘
                         │
                    HTTP/HTTPS
                         │
┌────────────────────────▼─────────────────────────────────────┐
│                   Reverse Proxy (Nginx)                      │
│              SSL/TLS Termination & Rate Limiting             │
└────────────────────────┬─────────────────────────────────────┘
                         │
┌────────────────────────▼─────────────────────────────────────┐
│              .NET 9 Application Container                    │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │           Razor Pages SPA Frontend                  │    │
│  │  ├─ Index.razor (Home Page)                         │    │
│  │  └─ Form.razor (Main Form Interface)                │    │
│  │     ├─ Producer View                                │    │
│  │     └─ Performer View                               │    │
│  └─────────────────────────────────────────────────────┘    │
│                         ▲                                    │
│                         │ HTTP                              │
│                         ▼                                    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │          Minimal API Endpoints                      │    │
│  │                                                     │    │
│  │  POST   /api/forms/producer              ├─────────┤    │
│  │  GET    /api/forms/producer/{id}         │         │    │
│  │  POST   /api/forms/performer/{id}        │         │    │
│  │  POST   /api/forms/images/{id}           │ Routes  │    │
│  │  POST   /api/forms/validate-complete/{id}│         │    │
│  │  GET    /api/forms/form/{id}             │         │    │
│  │  GET    /health                          ├─────────┤    │
│  │  GET    /api/status                                │    │
│  └─────────────────────────────────────────────────────┘    │
│                         ▲                                    │
│                         │                                    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │          Services Layer                            │    │
│  │                                                     │    │
│  │  ┌─────────────────────────────────────────────┐   │    │
│  │  │ FormStorageService                          │   │    │
│  │  │ • SaveProducerForm()                        │   │    │
│  │  │ • LoadProducerForm()                        │   │    │
│  │  │ • SavePerformerForm()                       │   │    │
│  │  │ • SaveImage()                               │   │    │
│  │  └─────────────────────────────────────────────┘   │    │
│  │                                                     │    │
│  │  ┌─────────────────────────────────────────────┐   │    │
│  │  │ PdfGenerationService                        │   │    │
│  │  │ • GenerateForm2257Async()                   │   │    │
│  │  │ • (Uses QuestPDF library)                   │   │    │
│  │  └─────────────────────────────────────────────┘   │    │
│  │                                                     │    │
│  │  ┌─────────────────────────────────────────────┐   │    │
│  │  │ FormValidationService                       │   │    │
│  │  │ • ValidateProducerForm()                    │   │    │
│  │  │ • ValidatePerformerForm()                   │   │    │
│  │  │ • ValidateImages()                          │   │    │
│  │  └─────────────────────────────────────────────┘   │    │
│  │                                                     │    │
│  │  ┌─────────────────────────────────────────────┐   │    │
│  │  │ StatisticsService                           │   │    │
│  │  │ • RecordFormCreated()                       │   │    │
│  │  │ • RecordFormCompleted()                     │   │    │
│  │  │ • GetStatistics()                           │   │    │
│  │  └─────────────────────────────────────────────┘   │    │
│  └─────────────────────────────────────────────────────┘    │
│                         ▼                                    │
│  ┌─────────────────────────────────────────────────────┐    │
│  │       Middleware Pipeline                          │    │
│  │                                                     │    │
│  │  1. RateLimitingMiddleware                         │    │
│  │     • IP-based rate limiting                       │    │
│  │     • Configurable limits per time window          │    │
│  │                                                     │    │
│  │  2. RequestLoggingMiddleware                       │    │
│  │     • Logs all HTTP requests/responses             │    │
│  │     • Captures timing information                  │    │
│  │                                                     │    │
│  │  3. Error Handling Middleware                      │    │
│  │     • Global exception handler                     │    │
│  │     • Structured error responses                   │    │
│  └─────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
         ▼               ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Local Storage│ │   Logging    │ │   Metrics    │
│              │ │              │ │              │
│ /app/data/   │ │ /app/logs/   │ │ In-Memory    │
│              │ │              │ │              │
│ • UUID/      │ │ • app-*.txt  │ │ • Requests   │
│   producer.  │ │ • Rotated    │ │ • Forms      │
│   json       │ │   daily      │ │ • Completed  │
│ • UUID/      │ │              │ │              │
│   performer. │ │ Serilog      │ │ (24hr window)│
│   json       │ │ Structured   │ │              │
│ • UUID/      │ │ Logging      │ │              │
│   images/    │ │              │ │              │
│ • UUID/      │ │              │ │              │
│   PDF files  │ │              │ │              │
└──────────────┘ └──────────────┘ └──────────────┘
```

## Data Flow

### Producer Form Creation Flow

1. **User Input** → Producer fills form on frontend (Form.razor)
2. **Validation** → Client-side validation
3. **Submission** → POST /api/forms/producer
4. **Processing**:
   - FormValidationService validates required fields
   - FormStorageService generates UUID and creates directory
   - Producer data saved as producer.json
5. **Response** → Return form ID and sharable URL
6. **Display** → Show URL to copy/share with performers

### Performer Form Completion Flow

1. **Access** → Performer opens URL with UUID query parameter
2. **Load** → GET /api/forms/producer/{id} retrieves producer data
3. **Display** → Form.razor populates producer info
4. **Image Upload** → Performer uploads 3 ID images
   - POST /api/forms/images/{id}?imageType=IDFront
   - POST /api/forms/images/{id}?imageType=IDBack
   - POST /api/forms/images/{id}?imageType=IDFace
5. **Form Completion** → Performer fills personal information
6. **Submission** → POST /api/forms/performer/{id}
7. **PDF Generation** → POST /api/forms/validate-complete/{id}
   - FormValidationService validates all fields and age (18+)
   - PdfGenerationService generates PDF with QuestPDF
   - Images embedded in storage
   - StatisticsService records completion
8. **Download** → PDF automatically downloads to performer
9. **Storage** → PDF and images saved to /app/data/{uuid}/

## Technology Stack

### Backend
- **Framework**: .NET 9 with Minimal APIs
- **Web Framework**: ASP.NET Core
- **PDF Generation**: QuestPDF (free/Community license)
- **Logging**: Serilog with console and file sinks
- **Data Format**: JSON (local files)

### Frontend
- **UI Framework**: Razor Components with Blazor Server
- **Styling**: Bootstrap 5
- **Icons**: Font Awesome 6
- **HTTP Client**: Built-in HttpClientFactory

### Infrastructure
- **Containerization**: Docker with multi-stage builds
- **Orchestration**: Docker Compose
- **Reverse Proxy**: Nginx (optional, recommended for production)
- **SSL/TLS**: Let's Encrypt / self-signed certificates

## Security Architecture

```
┌────────────────────────────────────────────────────────┐
│                  Security Layers                       │
├────────────────────────────────────────────────────────┤
│ 1. Transport Layer (TLS/SSL)                           │
│    • HTTPS enforcement                                 │
│    • Certificate validation                            │
│    • Cipher strength                                   │
├────────────────────────────────────────────────────────┤
│ 2. Application Layer                                   │
│    • Input validation on all endpoints                 │
│    • Age verification (18+ requirement)                │
│    • File type validation (JPG/PNG only)               │
│    • File size limits (2MB per image)                  │
├────────────────────────────────────────────────────────┤
│ 3. Rate Limiting                                       │
│    • IP-based rate limiting                            │
│    • Configurable request windows                      │
│    • Prevents abuse and DDoS                           │
├────────────────────────────────────────────────────────┤
│ 4. Data Protection                                     │
│    • UUID-based form identification                    │
│    • Secure random UUID generation                     │
│    • Local filesystem with directory permissions       │
│    • No plaintext storage of sensitive data            │
├────────────────────────────────────────────────────────┤
│ 5. Audit & Logging                                     │
│    • All requests logged with timestamps               │
│    • Error tracking and alerting                       │
│    • Form submission tracking                          │
│    • Access logging (who accessed what, when)          │
├────────────────────────────────────────────────────────┤
│ 6. Operational Security                                │
│    • Health checks for availability                    │
│    • Graceful error handling                           │
│    • No sensitive data in logs                         │
│    • Configuration management (no hardcoded secrets)   │
└────────────────────────────────────────────────────────┘
```

## Performance Considerations

### Scalability
- **Stateless Design**: Application servers can be load-balanced
- **Local Storage**: Can be replaced with S3/Azure Blob Storage
- **Async/Await**: All I/O operations are asynchronous
- **Connection Pooling**: Efficient HTTP client usage

### Optimization
- **Minimal APIs**: Reduced overhead vs MVC
- **Razor Components**: Server-side rendering for fast initial load
- **Bootstrap 5**: Optimized CSS framework
- **Docker**: Multi-stage build reduces image size
- **Caching**: HTTP cache headers on static assets
- **Compression**: GZIP compression on responses

### Resource Usage
- **CPU**: Low (typically < 10% idle)
- **Memory**: ~256MB base + buffer for concurrent requests
- **Disk**: ~100MB for application + data storage
- **Network**: ~10-50KB per form submission

## Deployment Architectures

### Single Instance (Development/Small)
```
Internet → Nginx → Docker Container → Local Storage
```

### High Availability (Production)
```
Internet → CDN → Load Balancer → Multiple Containers → Shared Storage
```

### Kubernetes (Enterprise)
```
Internet → Ingress → Service → Multiple Pods → Persistent Volume
```

## Data Retention & Compliance

- **Producer Forms**: Stored indefinitely (per 18 U.S.C. § 2257)
- **Performer Forms**: Stored indefinitely with submitted data
- **Images**: Stored with form data
- **Logs**: Rotated daily, archived for 30 days
- **Backups**: Daily incremental, retained for 30-90 days

## Monitoring & Observability

### Health Checks
- **Application Health**: GET /health
- **Status Endpoint**: GET /api/status with statistics
- **Docker Health Check**: Every 30 seconds

### Metrics Tracked
- Forms created (daily, hourly)
- Forms completed
- Pending forms
- Requests per time window
- Request latency
- Error rates

### Alerting (Optional)
- High error rate (> 5%)
- Disk space low (< 20% free)
- Memory usage high (> 80%)
- Rate limit threshold exceeded

---

**Last Updated**: April 24, 2026  
**Version**: 1.0.0
