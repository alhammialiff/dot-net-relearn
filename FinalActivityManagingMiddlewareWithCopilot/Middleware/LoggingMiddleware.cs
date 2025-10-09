using System.Text;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

// Logging Middleware - Third middleware in pipeline
// Purpose: Log all incoming requests and outgoing responses for auditing and monitoring
public class LoggingMiddleware
{
    // Delegate to next middleware in pipeline
    private readonly RequestDelegate _next;
    // Logger for recording HTTP request/response details
    private readonly ILogger<LoggingMiddleware> _logger;

    // Constructor: Inject dependencies
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Main middleware execution method - logs both request and response
    public async Task InvokeAsync(HttpContext context)
    {
        // Generate unique ID for correlating request and response logs
        var requestId = Guid.NewGuid().ToString();
        
        // Log the incoming request details
        await LogRequest(context, requestId);

        // Intercept response stream to capture response body for logging
        // Save original response stream so we can restore it later
        var originalBodyStream = context.Response.Body;
        // Create memory stream to capture response data
        using var responseBody = new MemoryStream();
        // Replace response stream temporarily
        context.Response.Body = responseBody;

        // Start timing the request processing
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Continue to next middleware and process the request
            await _next(context);
        }
        finally
        {
            // Stop timing (this runs regardless of success/failure)
            stopwatch.Stop();
            
            // Log the outgoing response details
            await LogResponse(context, requestId, stopwatch.ElapsedMilliseconds);
            
            // Copy captured response back to original stream for client
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    // Helper method to log incoming request details
    private async Task LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        var requestBody = string.Empty;

        // Only read request body if it exists and isn't too large (avoid memory issues)
        if (request.ContentLength.HasValue && request.ContentLength < 10000)
        {
            // Enable buffering so we can read the body multiple times
            request.EnableBuffering();
            // Create buffer to hold request body data
            var buffer = new byte[request.ContentLength.Value];
            // Read the entire request body into buffer
            await request.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
            // Convert bytes to string using UTF-8 encoding
            requestBody = Encoding.UTF8.GetString(buffer);
            // Reset stream position so controllers can read it again
            request.Body.Position = 0;
        }

        // Create structured log object with all request details
        var logMessage = new
        {
            RequestId = requestId,                                               // Correlation ID
            Method = request.Method,                                            // HTTP method (GET, POST, etc.)
            Path = request.Path.Value,                                          // Request URL path
            QueryString = request.QueryString.Value,                           // URL query parameters
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), // All HTTP headers
            Body = requestBody,                                                 // Request body content
            Timestamp = DateTime.UtcNow,                                       // When request was received
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),        // Client browser/app info
            RemoteIP = context.Connection.RemoteIpAddress?.ToString()           // Client IP address
        };

        // Log the request information using structured logging
        _logger.LogInformation("Incoming Request: {@RequestLog}", logMessage);
    }

    // Helper method to log outgoing response details
    private async Task LogResponse(HttpContext context, string requestId, long elapsedMilliseconds)
    {
        var response = context.Response;
        var responseBody = string.Empty;

        // Only read response body if stream supports seeking and isn't too large
        if (response.Body.CanSeek && response.Body.Length < 10000)
        {
            // Move to beginning of response stream
            response.Body.Seek(0, SeekOrigin.Begin);
            // Create buffer to hold response data
            var buffer = new byte[response.Body.Length];
            // Read the entire response body
            await response.Body.ReadExactlyAsync(buffer, 0, buffer.Length);
            // Convert bytes to string using UTF-8 encoding
            responseBody = Encoding.UTF8.GetString(buffer);
            // Reset stream position for copying back to original stream
            response.Body.Seek(0, SeekOrigin.Begin);
        }

        // Create structured log object with all response details
        var logMessage = new
        {
            RequestId = requestId,                                              // Correlation ID to match with request
            StatusCode = response.StatusCode,                                   // HTTP status code (200, 401, 500, etc.)
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), // All response headers
            Body = responseBody,                                                // Response body content
            ElapsedMilliseconds = elapsedMilliseconds,                         // How long request took to process
            Timestamp = DateTime.UtcNow                                        // When response was sent
        };

        // Use different log levels based on HTTP status code
        // 4xx and 5xx status codes are logged as warnings, others as information
        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, "Outgoing Response: {@ResponseLog}", logMessage);
    }
}