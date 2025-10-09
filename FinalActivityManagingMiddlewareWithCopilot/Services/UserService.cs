using BCrypt.Net;
using FinalActivityManagingMiddlewareWithCopilot.DTOs;
using FinalActivityManagingMiddlewareWithCopilot.Models;
using FinalActivityManagingMiddlewareWithCopilot.Middleware;

namespace FinalActivityManagingMiddlewareWithCopilot.Services;

public interface IUserService
{
    Task<UserDto> RegisterUserAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<bool> DeleteUserAsync(int id);
}

public class UserService : IUserService
{
    private readonly List<User> _users = new(); // In-memory storage for demo
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserService> _logger;
    private int _nextId = 1;

    public UserService(IJwtService jwtService, ILogger<UserService> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
        
        // Seed some initial data
        SeedInitialUsers();
    }

    public async Task<UserDto> RegisterUserAsync(RegisterDto registerDto)
    {
        // Check if username already exists
        if (_users.Any(u => u.Username.Equals(registerDto.Username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Username already exists");
        }

        // Check if email already exists
        if (_users.Any(u => u.Email.Equals(registerDto.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Email already exists");
        }

        var user = new User
        {
            Id = _nextId++,
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = registerDto.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users.Add(user);
        _logger.LogInformation("User registered: {Username}", user.Username);

        return await Task.FromResult(MapToDto(user));
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(loginDto.Username, StringComparison.OrdinalIgnoreCase) && u.IsActive);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "BCrypt verification failed for user: {Username}", user.Username);
            isPasswordValid = false;
        }

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        user.LastLoginAt = DateTime.UtcNow;
        var token = _jwtService.GenerateToken(user);
        
        _logger.LogInformation("User logged in: {Username}", user.Username);

        return await Task.FromResult(new LoginResponseDto
        {
            Token = token,
            User = MapToDto(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Should match JWT expiration
        });
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await Task.FromResult(_users.Where(u => u.IsActive).Select(MapToDto).ToList());
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        return await Task.FromResult(user != null ? MapToDto(user) : null);
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = _users.FirstOrDefault(u => 
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        return await Task.FromResult(user != null ? MapToDto(user) : null);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
        {
            return false;
        }

        user.IsActive = false; // Soft delete
        _logger.LogInformation("User deleted: {Username}", user.Username);
        return await Task.FromResult(true);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            Role = user.Role
        };
    }

    private void SeedInitialUsers()
    {
        var adminUser = new User
        {
            Id = _nextId++,
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var regularUser = new User
        {
            Id = _nextId++,
            Username = "user",
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _users.AddRange(new[] { adminUser, regularUser });
    }
}