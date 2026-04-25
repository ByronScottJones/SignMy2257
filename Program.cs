using SignMy2257.API;
using SignMy2257.Middleware;
using SignMy2257.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorComponents()
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

app.UseHttpsRedirection();

// Add custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseStaticFiles();
app.UseAntiforgery();

// Map Razor components
app.MapRazorComponents<SignMy2257.App>()
    .AddInteractiveServerRenderMode();

// Map API endpoints
app.MapFormEndpoints();
app.MapHealthAndStatusEndpoints();

// Map Blazor components
app.MapGet("/", () => Results.Redirect("/index")).AllowAnonymous();

app.Run();
