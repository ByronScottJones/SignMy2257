namespace SignMy2257.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var originalBodyStream = context.Response.Body;

        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "HTTP {Method} {Path} responded with {StatusCode} in {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    duration.TotalMilliseconds
                );
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex,
                    "HTTP {Method} {Path} failed after {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    duration.TotalMilliseconds
                );
                throw;
            }
            finally
            {
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
