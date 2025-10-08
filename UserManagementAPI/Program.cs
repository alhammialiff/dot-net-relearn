using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// ===== DATABASE CONFIGURATION =====
// Configure Entity Framework with SQLite
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
                     ?? "Data Source=users.db"));

// Register DATABASE service instead of in-memory service
builder.Services.AddScoped<IUserService, UserDbService>();

var app = builder.Build();

// ===== ENSURE DATABASE IS CREATED =====
// This ensures the database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    context.Database.EnsureCreated(); // Creates database with seed data
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();