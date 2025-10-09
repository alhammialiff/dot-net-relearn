using System.Text;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        
        // Log incoming request
        await LogRequest(context, requestId);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log outgoing response
            await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);
            
            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        var requestBody = string.Empty;

        // Read request body if present and not too large
        if (request.ContentLength.HasValue && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength.Value];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            requestBody = Encoding.UTF8.GetString(buffer);
            request.Body.Position = 0;
        }

        var logMessage = new
        {
            RequestId = requestId,
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = requestBody,
            Timestamp = DateTime.UtcNow,
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),
            RemoteIP = context.Connection.RemoteIpAddress?.ToString()
        };

        _logger.LogInformation("Incoming Request: {@RequestLog}", logMessage);
    }

    private async Task LogResponse(HttpContext context, string requestId, long elapsedMilliseconds)
    {
        var response = context.Response;
        var responseBody = string.Empty;

        // Read response body if not too large
        if (response.Body.CanSeek && response.Body.Length < 10000)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[response.Body.Length];
            await response.Body.ReadAsync(buffer, 0, buffer.Length);
            responseBody = Encoding.UTF8.GetString(buffer);
            response.Body.Seek(0, SeekOrigin.Begin);
        }

        var logMessage = new
        {
            RequestId = requestId,
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = responseBody,
            ElapsedMilliseconds = elapsedMilliseconds,
            Timestamp = DateTime.UtcNow
        };

        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, "Outgoing Response: {@ResponseLog}", logMessage);
    }
}