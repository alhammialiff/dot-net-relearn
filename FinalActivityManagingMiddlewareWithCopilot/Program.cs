// Import custom middleware and services
using FinalActivityManagingMiddlewareWithCopilot.Middleware;
using FinalActivityManagingMiddlewareWithCopilot.Services;

// Create web application builder - this sets up dependency injection container
var builder = WebApplication.CreateBuilder(args);

// === SERVICE REGISTRATION (Dependency Injection Setup) ===
// Add built-in ASP.NET Core services to the DI container

// Add MVC controllers support for handling API endpoints
builder.Services.AddControllers();
// Add OpenAPI/Swagger support for API documentation
builder.Services.AddOpenApi();

// Register our custom services with dependency injection
// Scoped = one instance per HTTP request (good for services that need request context)
builder.Services.AddScoped<IJwtService, JwtService>();      // JWT token generation and validation
builder.Services.AddScoped<IUserService, UserService>();    // User management operations

// Configure CORS (Cross-Origin Resource Sharing) to allow web browsers to call our API
// This is needed when your frontend (React, Angular, etc.) runs on a different port/domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()     // Allow requests from any website
              .AllowAnyMethod()     // Allow any HTTP method (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader();    // Allow any HTTP headers
    });
});

// Build the application with all registered services
var app = builder.Build();

// === HTTP REQUEST PIPELINE CONFIGURATION ===
// Configure the middleware pipeline - ORDER MATTERS!
// Middleware executes in the order added here

// Enable OpenAPI/Swagger UI only in development environment
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // Adds /openapi/v1.json endpoint for API documentation
}

// Built-in middleware for redirecting HTTP requests to HTTPS
app.UseHttpsRedirection();

// Enable CORS using the policy we defined above
app.UseCors("AllowAll");

// === CUSTOM MIDDLEWARE (IN SPECIFIC ORDER AS REQUESTED) ===
// (1) Error Handling Middleware - FIRST: Catch all exceptions and standardize error responses
app.UseMiddleware<ErrorHandlingMiddleware>();

// (2) Authentication Middleware - SECOND: Validate JWT tokens and set user context
app.UseMiddleware<AuthenticationMiddleware>();

// (3) Logging Middleware - THIRD: Log all requests and responses for auditing
app.UseMiddleware<LoggingMiddleware>();

// === ENDPOINT ROUTING ===
// Map controller endpoints (this enables our API controllers to handle requests)
app.MapControllers();

// === SAMPLE PUBLIC ENDPOINT (for testing) ===
// Keep the original weather endpoint as a public endpoint for testing middleware
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Define a minimal API endpoint (alternative to using controllers)
// This endpoint doesn't require authentication (it's in the public endpoints list)
app.MapGet("/weatherforecast", () =>
{
    // Generate 5 random weather forecasts for the next 5 days
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),     // Date
            Random.Shared.Next(-20, 55),                           // Temperature in Celsius
            summaries[Random.Shared.Next(summaries.Length)]        // Random weather description
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");  // Name for OpenAPI documentation

// Start the web application and begin listening for HTTP requests
app.Run();

// Record type for weather forecast data structure
// Records are immutable and provide value equality by default
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    // Computed property to convert Celsius to Fahrenheit
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
