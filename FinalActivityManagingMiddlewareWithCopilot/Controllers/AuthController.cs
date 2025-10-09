using Microsoft.AspNetCore.Mvc;
using FinalActivityManagingMiddlewareWithCopilot.DTOs;
using FinalActivityManagingMiddlewareWithCopilot.Services;

namespace FinalActivityManagingMiddlewareWithCopilot.Controllers;

// Authentication Controller - handles user registration and login
// These endpoints are public (no authentication required) as defined in AuthenticationMiddleware
[ApiController]                          // Enables automatic model validation and API-specific behaviors
[Route("api/[controller]")]              // Sets route pattern: /api/auth (controller name without "Controller" suffix)
public class AuthController : ControllerBase
{
    // User service for registration and login operations
    private readonly IUserService _userService;
    // Logger for authentication events (successful/failed logins, registrations)
    private readonly ILogger<AuthController> _logger;

    // Constructor: Dependency injection of services
    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // POST /api/auth/register - Create new user account
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        // Check if model validation passed (data annotations on RegisterDto)
        if (!ModelState.IsValid)
        {
            // Return 400 Bad Request with validation errors
            return BadRequest(new { error = "Invalid input data" });
        }

        try
        {
            // Attempt to register the user
            var user = await _userService.RegisterUserAsync(registerDto);
            
            // Return 200 OK with success message and user data (without sensitive info)
            return Ok(new { message = "User registered successfully", user });
        }
        catch (Exception ex)
        {
            // Log the registration failure for monitoring
            _logger.LogWarning(ex, "Registration failed for username: {Username}", registerDto.Username);
            
            // Re-throw exception to let Error Handling Middleware convert to appropriate HTTP response
            throw; // ValidationException becomes 400, others become 500
        }
    }

    // POST /api/auth/login - Authenticate user and return JWT token
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        // Check if model validation passed (Required attributes on LoginDto)
        if (!ModelState.IsValid)
        {
            // Return 400 Bad Request for missing username/password
            return BadRequest(new { error = "Invalid input data" });
        }

        try
        {
            // Attempt to authenticate user and generate JWT token
            var loginResponse = await _userService.LoginAsync(loginDto);
            
            // Return 200 OK with JWT token and user information
            // Client will use this token for subsequent authenticated requests
            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            // Log the login failure for security monitoring
            _logger.LogWarning(ex, "Login failed for username: {Username}", loginDto.Username);
            
            // Re-throw exception to let Error Handling Middleware handle it
            throw; // UnauthorizedAccessException becomes 401, others become 500
        }
    }
}