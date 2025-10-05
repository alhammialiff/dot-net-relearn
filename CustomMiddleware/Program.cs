using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Swagger services BEFORE building the app
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Instantiate blogs
var blogs = new List<Blog>
{
    new Blog{ Title = "My First Post", Body = "This is my first post"},
    new Blog{ Title = "My Second Post", Body = "This is my second post"}
};

// Only trigger Swagger and its UI in dev mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*******************************
* This middleware pipeline example performs
* (1) Process timer method
* (2) Prints Request Information (path)
* (3) Authorization check
*******************************/

/*******************************
* #1 Request Pipeline Timer Middleware
*******************************/
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    await next.Invoke();
    var duration = DateTime.UtcNow - startTime;
    Console.WriteLine($"Duration: {duration}");

});


/*******************************
* #2 API logic Middleware
*******************************/
app.Use(async (context, next) =>
{
    Console.WriteLine(context.Request.Path);
    // Can run code before (1,2,3...)
    await next.Invoke();
    // Can run code after (...3,2,1)
    Console.WriteLine(context.Response.StatusCode);
});


/*******************************
* #3 UseWhen Conditional Middleware Example
*******************************/
app.UseWhen(

    // The Condition:
    // Only trigger this ware on methods other than GET
    context => context.Request.Method != "GET",

    // Middleware configuration 
    // This is the middleware when condition is truthy
    appBuilder => appBuilder.Use( async (context, next) =>
    {

        // Extracts X-API-Key header
        var extractedPassword = context.Request.Headers["X-Api-Key"];

        // If authenticated, continue pipeline
        if (extractedPassword == "thisIsABadPasswordExampleUseEnv")
        {
            await next.Invoke();
        }
        else
        {   
            // Send an Unauthorized Code
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
        }
        
    })
);


// Root
app.MapGet("/", () => "I am root!");

// Get all blogs
app.MapGet("/blogs", () =>
{
    return Results.Ok(blogs);
});

// Get blog by ID
// The addition of strong-typed return declaration also
// allows OpenAPI to correctly deduce it in Swagger doc.
app.MapGet("/blogs/{id}", Results<Ok<Blog>, NotFound> (int id) =>
{
    if (id < 0 || id >= blogs.Count)
    {
        return TypedResults.NotFound();
    }
    else
    {
        return TypedResults.Ok(blogs[id]);
    }

// Added OpenAPI chaining to document what this API handler does.
}).WithOpenApi(operation =>
{
    operation.Parameters[0].Description = "The ID of the blog to retrieve.";
    operation.Summary = "Get single blog";
    operation.Description = "Returns a single blog";
    return operation;
});

// Add blog
app.MapPost("/blogs", (Blog blog) =>
{
    blogs.Add(blog);
    return Results.Created($"/blogs/{blogs.Count - 1}", blog);
});

// Delete Blog
app.MapDelete("/blogs/{id}", (int id) =>
{

    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    else
    {
        blogs.RemoveAt(id);
        return Results.NoContent();
    }

});

// Update blog
app.MapPut("/blogs/{id}", (int id, Blog blog) =>
{
    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    else
    {
        blogs[id] = blog;
        return Results.Ok(blog);
    }

});

app.Run();

public class Blog
{

    public required string Title { get; set; }
    public required string Body { get; set; }

};

