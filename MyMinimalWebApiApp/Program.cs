var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new List<string>
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Count)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/weatherforecast", (WeatherForecast forecast) =>
{
    
    // If string is not null or empty
    if (!string.IsNullOrEmpty(forecast.Summary))
    {
        summaries.Add(forecast.Summary);
    }

    return Results.Ok(summaries);

});

// Removed incorrect MapDelete for WeatherForecast object
app.MapDelete("/weatherforecast/{summary}", (string summary) =>
{
    if (summaries.Remove(summary))
        return Results.Ok($"Removed: {summary}");
    else
        return Results.NotFound($"Summary not found: {summary}");
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
