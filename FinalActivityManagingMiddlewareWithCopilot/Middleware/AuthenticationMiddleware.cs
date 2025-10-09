using System.Text.Json;
using FinalActivityManagingMiddlewareWithCopilot.Services;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

// Authentication Middleware - Second middleware in pipeline  
// Purpose: Validate JWT tokens and ensure only authenticated users access protected endpoints
public class AuthenticationMiddleware
{
    // Delegate to next middleware in pipeline
    private readonly RequestDelegate _next;
    // Logger for authentication events
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Array of endpoint paths that don't require authentication
    // These endpoints are accessible to everyone (registration, login, etc.)
    private readonly string[] _publicEndpoints = { 
        "/api/auth/register",  // User registration
        "/api/auth/login",     // User login
        "/weatherforecast",    // Sample public API
        "/swagger",            // API documentation
        "/openapi"            // OpenAPI specification
    };

    // Constructor: Inject dependencies (no JWT service here - resolved per request)
    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Main middleware execution method - processes authentication for each request
    public async Task InvokeAsync(HttpContext context)
    {
        // Get the request path and convert to lowercase for case-insensitive comparison
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Check if this is a public endpoint that doesn't need authentication
        if (IsPublicEndpoint(path))
        {
            // Skip authentication and continue to next middleware
            await _next(context);
            return;
        }

        // Extract JWT token from the Authorization header
        var token = ExtractTokenFromHeader(context);

        // If no token provided, return 401 Unauthorized
        if (string.IsNullOrEmpty(token))
        {
            await HandleUnauthorized(context, "Authorization header is missing");
            return;
        }

        // Resolve JWT service from DI container per request (avoids scoping issues)
        // Note: We can't inject IJwtService in constructor because middleware is singleton
        var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
        
        // Validate the JWT token and get user claims
        var principal = jwtService.GetPrincipalFromToken(token);
        if (principal == null)
        {
            // Token is invalid or expired
            await HandleUnauthorized(context, "Invalid or expired token");
            return;
        }

        // Set authenticated user context for controllers and subsequent middleware
        context.User = principal;
        
        // Continue to next middleware with authenticated user context
        await _next(context);
    }

    // Helper method to check if the request path is a public endpoint
    private bool IsPublicEndpoint(string path)
    {
        // Use LINQ to check if any public endpoint matches the start of the request path
        return _publicEndpoints.Any(endpoint => path.StartsWith(endpoint.ToLower()));
    }

    // Helper method to extract JWT token from Authorization header
    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        // Get the Authorization header value
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        // Check if header exists and starts with "Bearer "
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        // Extract token by removing "Bearer " prefix and trimming whitespace
        return authHeader["Bearer ".Length..].Trim();
    }

    // Helper method to send 401 Unauthorized response
    private static async Task HandleUnauthorized(HttpContext context, string message)
    {
        // Set HTTP status code to 401 Unauthorized
        context.Response.StatusCode = 401;
        // Set response content type to JSON
        context.Response.ContentType = "application/json";

        // Create standardized error response object
        var response = new { error = "Unauthorized" };
        
        // Serialize to JSON with camelCase naming
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Write JSON response to client
        await context.Response.WriteAsync(jsonResponse);
    }
}