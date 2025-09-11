var builder = WebApplication.CreateBuilder(args);

// ====================================
// Add services to the container.
// ====================================
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();
// builder.Services.AddControllers();
builder.Services.AddHttpLogging((o) => {});

var app = builder.Build();


// app.UseAuthorization();
// app.MapControllers();
// app.UseHttpLogging();

// The output:
// Logic before 1
// Logic before 2
// Logic before 3
// Logic after 3
// Logic after 2
// Logic after 1

// Why it runs that way?
// It runs in a nested function way, or LIFO.
// 
// Behind the scenes .NET runs these before our codes-:
// (1) app.UseRouting()
//   - Logic needed to run all the diff routes
//   - Runs before our routes below (/, /hello etc.)
// (2) app.UseAuthentication()  
// (3) app.UseAuthorization() 
//   - (2) and (3) will run to check if we have any services
//    need requires it
// (4) app.UseExceptionHandler() 
//   - Only runs in development env
// 
// THEN, .NET runs this after our code
// (5) app.UseEndpoints();

app.Use(async (context, next) =>
{
    // Can add logic here
    Console.WriteLine("Logic before 1");
    await next.Invoke();
    Console.WriteLine("Logic after 1");
    // Can add logic here
});
app.Use(async (context, next) =>
{
    // Can add logic here
    Console.WriteLine("Logic before 2");
    await next.Invoke();
    Console.WriteLine("Logic after 2");
    // Can add logic here
});
app.Use(async (context, next) =>
{
    // Can add logic here
    Console.WriteLine("Logic before 3");
    await next.Invoke();
    Console.WriteLine("Logic after 3");
    // Can add logic here
});

app.MapGet("/", () => "Hello World!");
app.MapGet("/hello", () => "This is the hello route!");



app.Run();

// record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
