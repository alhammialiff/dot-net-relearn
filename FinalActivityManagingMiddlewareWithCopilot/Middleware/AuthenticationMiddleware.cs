using System.Text.Json;
using FinalActivityManagingMiddlewareWithCopilot.Services;

namespace FinalActivityManagingMiddlewareWithCopilot.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Endpoints that don't require authentication
    private readonly string[] _publicEndpoints = { 
        "/api/auth/register", 
        "/api/auth/login", 
        "/weatherforecast",
        "/swagger",
        "/openapi"
    };

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Skip authentication for public endpoints
        if (IsPublicEndpoint(path))
        {
            await _next(context);
            return;
        }

        var token = ExtractTokenFromHeader(context);

        if (string.IsNullOrEmpty(token))
        {
            await HandleUnauthorized(context, "Authorization header is missing");
            return;
        }

        // Get JWT service from service provider per request
        var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
        var principal = jwtService.GetPrincipalFromToken(token);
        if (principal == null)
        {
            await HandleUnauthorized(context, "Invalid or expired token");
            return;
        }

        // Set user context for subsequent middleware/controllers
        context.User = principal;
        await _next(context);
    }

    private bool IsPublicEndpoint(string path)
    {
        return _publicEndpoints.Any(endpoint => path.StartsWith(endpoint.ToLower()));
    }

    private static string? ExtractTokenFromHeader(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authHeader["Bearer ".Length..].Trim();
    }

    private static async Task HandleUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new { error = "Unauthorized" };
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}