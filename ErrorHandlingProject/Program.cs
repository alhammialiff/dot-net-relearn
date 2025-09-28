using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Instantiate Logger Config
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

/*******************************************
* Configurations for our ASP.Net Core app
* Not to be confused with dependency injections
* Dependency injections are custom services -:
*
*  Eg.
*  builder.Services.AddTransient<IMyService, MyService>();
*  builder.Services.AddScoped<IMyService, MyService>();
*  builder.Services.AddSingleton<IMyService, MyService>();
*  AddSingleton (one instance for the whole app)
*  AddScoped (one instance per request)
*  AddTransient (new instance every time requested)
*/
builder.Host.UseSerilog();
builder.Services.AddControllers(); // Only this is a dep injection, a built-in one that is
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
/*******************************************/

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Global exception caught:{ex}");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred. Please try again later");
    }

});

app.UseRouting();
app.MapControllers();
app.Run();

