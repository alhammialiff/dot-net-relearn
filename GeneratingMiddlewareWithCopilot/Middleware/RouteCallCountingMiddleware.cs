using GeneratingMiddlewareWithCopilot.Services;
using Microsoft.AspNetCore.Routing;

namespace GeneratingMiddlewareWithCopilot.Middleware;

/// <summary>
/// ASP.NET Core middleware that counts how many times each API route is invoked.
/// It tries to use the matched <see cref="RouteEndpoint"/> and its route pattern (e.g. "/weatherforecast")
/// so calls to the same route template are aggregated. If no endpoint is available (e.g., static files),
/// it falls back to the raw request path.
/// </summary>
public class RouteCallCountingMiddleware
{
    private readonly RequestDelegate _next;

    public RouteCallCountingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Intercepts the request, determines the best key (route pattern or path) and increments the counter.
    /// </summary>
    /// <param name="context">The current HTTP request/response context.</param>
    /// <param name="counter">The shared counter service resolved from DI.</param>
    public async Task InvokeAsync(HttpContext context, RouteCallCounter counter)
    {
        // Try to get the matched endpoint's route pattern, fallback to raw path
        var endpoint = context.GetEndpoint() as RouteEndpoint;
        // Prefer the template (e.g., "/weatherforecast"), then the raw path, and finally "/" if somehow null
        string routeKey = endpoint?.RoutePattern.RawText
            ?? context.Request.Path.Value
            ?? "/";

        // Atomically increment the count for the resolved key
        counter.Increment(routeKey);

        // Call the next piece in the pipeline
        await _next(context);
    }
}

public static class RouteCallCountingMiddlewareExtensions
{
    /// <summary>
    /// Adds <see cref="RouteCallCountingMiddleware"/> to the application's request pipeline.
    /// </summary>
    public static IApplicationBuilder UseRouteCallCounting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RouteCallCountingMiddleware>();
    }
}
