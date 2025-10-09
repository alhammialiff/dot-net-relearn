using System.Net;
using System.Text.Json;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

// Error Handling Middleware - First middleware in pipeline
// Purpose: Catch all unhandled exceptions and convert them to standardized HTTP responses
public class ErrorHandlingMiddleware
{
    // Delegate to the next middleware in the pipeline
    private readonly RequestDelegate _next;
    // Logger for recording error details
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    // Constructor: Dependency injection of next middleware and logger
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Main middleware execution method - called for every HTTP request
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continue to the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the error for debugging and monitoring
            _logger.LogError(ex, "An unhandled exception occurred");
            // Convert the exception to a standardized HTTP response
            await HandleExceptionAsync(context, ex);
        }
    }

    // Helper method to convert exceptions into standardized HTTP error responses
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Check if response has already been sent to client - if so, we can't modify it
        if (context.Response.HasStarted)
        {
            return;
        }

        // Set response content type to JSON for consistent API responses
        context.Response.ContentType = "application/json";

        // Create error response object
        var response = new ErrorResponse();

        // Map different exception types to appropriate HTTP status codes
        switch (exception)
        {
            case ArgumentException:
            case ValidationException: // Custom validation exception
                response.Error = "Bad request";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                break;
            case UnauthorizedAccessException:
                response.Error = "Unauthorized";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                break;
            case KeyNotFoundException:
                response.Error = "Not found";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                break;
            default:
                // For debugging: log unexpected exception types to console
                Console.WriteLine($"Unhandled exception type: {exception.GetType().Name} - {exception.Message}");
                response.Error = "Internal server error";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // 500
                break;
        }

        // Serialize error response to JSON with camelCase property naming
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Write JSON error response to client
        await context.Response.WriteAsync(jsonResponse);
    }
}

// Data Transfer Object for standardized error responses
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

// Custom exception class for validation errors
// Inherits from Exception but allows us to specifically catch validation issues
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}