using Microsoft.Extensions.Options;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Kestrel is ASP.NET lightweight web server
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5294);
});

var app = builder.Build();

// 1. FIRST: Check transport security (HTTPS simulation)
app.Use(async (context, next) =>
{
    if (context.Request.Query["secure"] != "true")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Simulated HTTPS Required");
        return;
    }

    await next();
});

// 2. SECOND: Block unauthorized paths early (fail fast)
// No need to authenticate users trying to access forbidden paths
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/unauthorized")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized Access");
        return; // Don't continue pipeline
    }
    await next();
});

// 3. THIRD: Verify user authentication
// Only proceed with authenticated users
app.Use(async (context, next) =>
{
    var isAuthenticated = context.Request.Query["authenticated"] == "true";

    if (!isAuthenticated)
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsync("Access Denied");
        return;
    }

    context.Response.Cookies.Append("SecureCookie", "SecureData", new CookieOptions
    {
        HttpOnly = true,
        Secure = true
    });

    await next();
});

// 4. FOURTH: Validate input from authenticated users only
// Don't waste resources validating input from unauthenticated users
app.Use(async (context, next) =>
{
    var input = context.Request.Query["input"];

    if (!IsValidInput(input))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid Input");
        return;
    }
    await next();
});

// 5. FINALLY: Process authenticated and validated requests
app.Use(async (context, next) =>
{
    // Sim async task
    await Task.Delay(100);
    await context.Response.WriteAsync("Processed Asynchronously\n");
    await next();
});


// Validation Helper
static bool IsValidInput(string? input)
{
    // Return false if input is null or empty
    if (string.IsNullOrWhiteSpace(input))
        return false;
    
    // Check if input contains only letters and digits and no XSS patterns
    return input.All(char.IsLetterOrDigit) && !input.Contains("<script>", StringComparison.OrdinalIgnoreCase);
}



app.Run();