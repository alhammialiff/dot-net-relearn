@echo off
echo === User Management API - Implementation Switcher ===
echo.
echo Current implementations available:
echo 1. In-Memory (Fast, data lost on restart)
echo 2. Database (Persistent, real async I/O)
echo.

:choice
set /p choice="Which implementation do you want to use? (1 or 2): "

if "%choice%"=="1" goto inmemory
if "%choice%"=="2" goto database
echo Invalid choice. Please enter 1 or 2.
goto choice

:inmemory
echo.
echo Switching to IN-MEMORY implementation...
copy Program.cs Program-Database-Backup.cs >nul 2>&1
echo using UserManagementAPI.Services; > Program.cs
echo. >> Program.cs
echo var builder = WebApplication.CreateBuilder(args); >> Program.cs
echo. >> Program.cs
echo // Add services to the container. >> Program.cs
echo builder.Services.AddControllers(); >> Program.cs
echo // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi >> Program.cs
echo builder.Services.AddOpenApi(); >> Program.cs
echo. >> Program.cs
echo // Register IN-MEMORY service >> Program.cs
echo builder.Services.AddScoped^<IUserService, UserService^>(); >> Program.cs
echo. >> Program.cs
echo var app = builder.Build(); >> Program.cs
echo. >> Program.cs
echo // Configure the HTTP request pipeline. >> Program.cs
echo if (app.Environment.IsDevelopment()) >> Program.cs
echo { >> Program.cs
echo     app.MapOpenApi(); >> Program.cs
echo } >> Program.cs
echo. >> Program.cs
echo app.UseHttpsRedirection(); >> Program.cs
echo. >> Program.cs
echo app.MapControllers(); >> Program.cs
echo. >> Program.cs
echo app.Run(); >> Program.cs

echo ✅ Switched to IN-MEMORY implementation
echo ⚡ Fast operations, data in RAM only
echo ⚠️  Data will be lost when app stops
goto end

:database
echo.
echo Switching to DATABASE implementation...
copy Program-Database.cs Program.cs >nul 2>&1
echo ✅ Switched to DATABASE implementation  
echo 🗄️  Persistent storage with SQLite
echo 🔥 Real async I/O operations
echo 💾 Data survives app restarts
goto end

:end
echo.
echo To run the application:
echo   dotnet run
echo.
echo To test the API:
echo   .\test-crud-api.ps1
echo.
pause