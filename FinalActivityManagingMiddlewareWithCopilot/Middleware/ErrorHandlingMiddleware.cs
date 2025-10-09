using System.Net;
using System.Text.Json;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Check if response has already been sent
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ArgumentException:
            case ValidationException:
                response.Error = "Bad request";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case UnauthorizedAccessException:
                response.Error = "Unauthorized";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            case KeyNotFoundException:
                response.Error = "Not found";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            default:
                // Log the exception type for debugging
                Console.WriteLine($"Unhandled exception type: {exception.GetType().Name} - {exception.Message}");
                response.Error = "Internal server error";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}