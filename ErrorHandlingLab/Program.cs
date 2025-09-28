using Serilog;

// Create a builder for the web application
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logger to write logs to the console
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Output logs to the console
    .CreateLogger();   // Create the logger instance

// Register Serilog as the logging provider for the app
builder.Host.UseSerilog();

// Register controller support (MVC/Web API)
builder.Services.AddControllers(); // Adds built-in controller dependency injection

// Remove all default logging providers
builder.Logging.ClearProviders();

// Add the console logger so logs appear in the terminal
builder.Logging.AddConsole();

// Build the web application
var app = builder.Build();

// Global error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        // Call the next middleware/component in the pipeline
        await next();
    }
    catch (Exception ex)
    {
        // Log the exception to the console
        Console.WriteLine($"Global exception caught:{ex}");
        // Set HTTP status code to 500 (Internal Server Error)
        context.Response.StatusCode = 500;
        // Return a generic error message to the client
        await context.Response.WriteAsync("An unexpected error occurred. Please try again later");
    }
});

// Enable routing for the app (matches requests to endpoints)
app.UseRouting();

// Map controller endpoints (activates API routes)
app.MapControllers();

// Start the web server and begin listening for requests
app.Run();