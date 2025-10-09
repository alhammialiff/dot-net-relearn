using Microsoft.AspNetCore.Mvc;
using FinalActivityManagingMiddlewareWithCopilot.DTOs;
using FinalActivityManagingMiddlewareWithCopilot.Services;

namespace FinalActivityManagingMiddlewareWithCopilot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid input data" });
        }

        try
        {
            var user = await _userService.RegisterUserAsync(registerDto);
            return Ok(new { message = "User registered successfully", user });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registration failed for username: {Username}", registerDto.Username);
            throw; // Let error handling middleware handle it
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid input data" });
        }

        try
        {
            var loginResponse = await _userService.LoginAsync(loginDto);
            return Ok(loginResponse);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Login failed for username: {Username}", loginDto.Username);
            throw; // Let error handling middleware handle it
        }
    }
}