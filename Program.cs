using SignMy2257.API;
using SignMy2257.Middleware;
using SignMy2257.Services;
using Serilog;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Auto-generate self-signed certificate for HTTPS in development
if (builder.Environment.IsDevelopment())
{
    var certPath = Path.Combine(AppContext.BaseDirectory, "cert.pfx");
    var certPassword = "SignMy2257-DevCert";

    if (!File.Exists(certPath))
    {
        // Generate a self-signed certificate
        using (var rsa = RSA.Create(2048))
        {
            var certReq = new CertificateRequest("cn=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var cert = certReq.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));
            var certBytes = cert.Export(X509ContentType.Pfx, certPassword);
            File.WriteAllBytes(certPath, certBytes);
            Log.Information("Generated self-signed certificate at {CertPath}", certPath);
        }
    }

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5000); // HTTP
        options.ListenLocalhost(5001, listenOptions =>
        {
            var cert = new X509Certificate2(certPath, certPassword);
            listenOptions.UseHttps(cert);
        });
    });
}

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents(options => 
{
    options.FormMappingProvider = null; // Disable default form mapping/antiforgery
})
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Register application services
builder.Services.AddScoped<IFormStorageService, FormStorageService>();
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>();
builder.Services.AddScoped<IFormValidationService, FormValidationService>();
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

// Rate limiting configuration
var rateLimitOptions = new RateLimitOptions();
builder.Configuration.GetSection("RateLimiting").Bind(rateLimitOptions);
builder.Services.AddSingleton(rateLimitOptions);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseStaticFiles();

// Map Razor components
app.MapRazorComponents<SignMy2257.App>()
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery();

// Map API endpoints
app.MapFormEndpoints();
app.MapHealthAndStatusEndpoints();

// Debug test endpoint
app.MapGet("/debug/test", () => 
{
    Log.Information("Debug test endpoint called");
    return Results.Ok(new { message = "Server is working", timestamp = DateTime.UtcNow });
}).AllowAnonymous();

app.Run();
