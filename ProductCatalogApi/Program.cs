using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register controllers for endpoint mapping
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpRedirection();

// Map controller routes to enable your controller endpoints
app.MapControllers();
app.Run();