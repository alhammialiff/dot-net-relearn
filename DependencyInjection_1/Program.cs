var builder = WebApplication.CreateBuilder(args);

// 1. Singleton - With Singleton, we only ever create one instance of this object
// Eg. DB Connection
// builder.Services.AddSingleton<IMyService, MyService>();

// 2. Add Scoped - Every request will run a new instance of the Service
//                 Hence, new id is generated every time on every request.
builder.Services.AddScoped<IMyService, MyService>();

// 3.  Add Transient - Every invocation of the service will run a new instance of it
//                     Hence, every middleware or request that invokes it will get a new ID
builder.Services.AddTransient<IMyService, MyService>();




var app = builder.Build();

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("First Middleware");
    await next.Invoke();

});

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("Second Middleware");
    await next.Invoke();

});

app.MapGet("/", (IMyService myService) =>
{
    myService.LogCreation("Root");
    return Results.Ok("Check the console for service creation log");
});

app.Run();

public interface IMyService
{
    void LogCreation(string message);

}

public class MyService : IMyService
{
    private readonly int _serviceId;

    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999);
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"{message} - Service ID: {_serviceId}");
    }


}
