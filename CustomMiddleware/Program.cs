var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


// Instantiate blogs
var blogs = new List<Blog>
{
    new Blog{ Title = "My First Post", Body = "This is my first post"},
    new Blog{ Title = "My Second Post", Body = "This is my second post"}
};


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
    context => context.Request.Method != "GET",
    appBuilder => appBuilder.Use( async (context, next) =>
    {

        var extractedPassword = context.Request.Headers["X-Api-Key"];

        if (extractedPassword == "thisIsABadPasswordExampleUseEnv")
        {
            await next.Invoke();
        }
        else
        {
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
app.MapGet("/blogs/{id}", (int id) =>
{
    if (id < 0 || id >= blogs.Count)
    {
        return Results.NotFound();
    }
    else
    {
        return Results.Ok(blogs[id]);
    }
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

