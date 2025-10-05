var builder = WebApplication.CreateBuilder(args);

/*****************************************************
* Add services for logging, auth, and authorization
******************************************************/
builder.Services.AddHttpLogging(logging =>
{

    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;

});
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
/*****************************************************/


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // If prod, use this middleware to handle exception
    app.UseExceptionHandler("/Home/Error");
}
else
{
    // If dev, use this instead
    app.UseDeveloperExceptionPage();
}

// Auth middleware
app.UseAuthentication();

// Authorization middleware
app.UseAuthorization();

// HTTP Logging middleware
app.UseHttpLogging();

app.MapGet("/", () => "Root");


// Add HTTP logging middleware
app.Use(async (context, next) =>
{

    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next();
    Console.WriteLine($"Request Status Code: {context.Response.StatusCode}");

});

// Middleware to track request duration
app.Use(async (context, next) =>
{

    var startTime = DateTime.UtcNow;

    // Log the request start time
    Console.WriteLine($"Start Time: {DateTime.UtcNow}");

    // This is where the application logic middleware occurs
    await next();

    // Log the request finish time
    // After this line, the pipeline will go back to the prevous pipeline, the line after next() (which is the middleware above)
    var duration = DateTime.UtcNow - startTime;
    Console.WriteLine($"Response Time: {duration.TotalMilliseconds} ms");

});


// Run server
app.Run();