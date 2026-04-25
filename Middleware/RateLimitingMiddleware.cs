using System.Net;

namespace SignMy2257.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly Dictionary<string, RateLimitBucket> _buckets = new();
    private readonly RateLimitOptions _options;

    public class RateLimitBucket
    {
        public int RequestCount { get; set; }
        public DateTime ResetTime { get; set; }
    }

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;
        bool isRateLimited = false;
        int remaining = _options.RequestsPerWindow;

        lock (_buckets)
        {
            if (!_buckets.ContainsKey(clientId) || _buckets[clientId].ResetTime < now)
            {
                _buckets[clientId] = new RateLimitBucket
                {
                    RequestCount = 1,
                    ResetTime = now.AddSeconds(_options.WindowSizeInSeconds)
                };
            }
            else
            {
                _buckets[clientId].RequestCount++;
            }

            isRateLimited = _buckets[clientId].RequestCount > _options.RequestsPerWindow;
            remaining = Math.Max(0, _options.RequestsPerWindow - _buckets[clientId].RequestCount);

            context.Response.Headers.Add("X-RateLimit-Limit", _options.RequestsPerWindow.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
        }

        if (isRateLimited)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _options.WindowSizeInSeconds.ToString());
            await context.Response.WriteAsJsonAsync(new { error = "Rate limit exceeded" });
            return;
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        return context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded)
            ? forwarded.ToString().Split(',').First().Trim()
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

public class RateLimitOptions
{
    public bool Enabled { get; set; } = true;
    public int RequestsPerWindow { get; set; } = 100;
    public int WindowSizeInSeconds { get; set; } = 60;
}
