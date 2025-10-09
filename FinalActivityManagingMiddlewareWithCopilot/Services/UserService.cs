using BCrypt.Net;
using FinalActivityManagingMiddlewareWithCopilot.DTOs;
using FinalActivityManagingMiddlewareWithCopilot.Models;
using FinalActivityManagingMiddlewareWithCopilot.Middleware;

namespace FinalActivityManagingMiddlewareWithCopilot.Services;

// Interface defining user management operations
// This allows for dependency injection and makes testing easier
public interface IUserService
{
    Task<UserDto> RegisterUserAsync(RegisterDto registerDto);           // Create new user account
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);               // Authenticate user and return JWT token
    Task<List<UserDto>> GetAllUsersAsync();                            // Get all active users
    Task<UserDto?> GetUserByIdAsync(int id);                           // Get specific user by ID
    Task<UserDto?> GetUserByUsernameAsync(string username);            // Get specific user by username
    Task<bool> DeleteUserAsync(int id);                                // Soft delete user (set inactive)
}

// User Service implementation - handles all user management operations
// Note: Uses in-memory storage for demo - in production, use a database
public class UserService : IUserService
{
    // In-memory list to store users (for demo purposes only)
    // In production, this would be replaced with database access (Entity Framework, etc.)
    private readonly List<User> _users = new();
    
    // JWT service for generating authentication tokens
    private readonly IJwtService _jwtService;
    // Logger for user management events
    private readonly ILogger<UserService> _logger;
    // Simple counter for generating unique user IDs
    private int _nextId = 1;

    // Constructor: Dependency injection and initial data setup
    public UserService(IJwtService jwtService, ILogger<UserService> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
        
        // Create some initial test users for demo purposes
        SeedInitialUsers();
    }

    // Register a new user account
    public async Task<UserDto> RegisterUserAsync(RegisterDto registerDto)
    {
        // Business rule validation: Check if username already exists
        // Use case-insensitive comparison to prevent "Admin" and "admin" both existing
        if (_users.Any(u => u.Username.Equals(registerDto.Username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Username already exists");
        }

        // Business rule validation: Check if email already exists
        // Prevent duplicate email addresses in the system
        if (_users.Any(u => u.Email.Equals(registerDto.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Email already exists");
        }

        // Create new user entity
        var user = new User
        {
            Id = _nextId++,                                                 // Assign unique ID
            Username = registerDto.Username,                                // Store username
            Email = registerDto.Email,                                      // Store email
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password), // Hash password securely (never store plaintext!)
            Role = registerDto.Role,                                        // Assign role (Admin/User)
            CreatedAt = DateTime.UtcNow,                                   // Record creation timestamp
            IsActive = true                                                 // New users are active by default
        };

        // Add to in-memory storage (in production: save to database)
        _users.Add(user);
        
        // Log successful registration for auditing
        _logger.LogInformation("User registered: {Username}", user.Username);

        // Convert internal User entity to public UserDto and return
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