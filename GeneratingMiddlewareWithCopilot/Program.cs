// Import the middleware namespace so we can call the extension method UseRouteCallCounting()
using GeneratingMiddlewareWithCopilot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Register minimal OpenAPI/Swagger services for easy exploration in Development
builder.Services.AddOpenApi();
// Register our route call counter as a singleton so counts are shared across requests
builder.Services.AddSingleton<GeneratingMiddlewareWithCopilot.Services.RouteCallCounter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Expose OpenAPI endpoints in Development (e.g., /openapi/v1.json)
    app.MapOpenApi();
}

// Redirect HTTP -> HTTPS in dev locally (Kestrel will listen on both)
app.UseHttpsRedirection();

// Insert our custom middleware into the pipeline to track per-route call counts
// What happens under the hood when app.UseRouteCallCounting is used?
// GeneratingMiddlewareWithCopilot.Middleware.RouteCallCountingMiddlewareExtensions.UseRouteCallCounting(app);
app.UseRouteCallCounting();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Example endpoint used for testing; hitting this will increment the counter for 
// the route pattern "/weatherforecast"
app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Utility endpoint to view current route invocation counts
app.MapGet("/__route-counts", (GeneratingMiddlewareWithCopilot.Services.RouteCallCounter counter) =>
{
    // Counter is resolved from DI and we return the current snapshot of counts
    return Results.Ok(counter.GetCounts());
})
.WithName("GetRouteCounts");

app.Run();

// Minimal-API style immutable DTO used by the sample endpoint above
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
