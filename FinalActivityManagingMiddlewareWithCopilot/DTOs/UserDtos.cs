using System.ComponentModel.DataAnnotations;

namespace FinalActivityManagingMiddlewareWithCopilot.DTOs;

// Data Transfer Object for User information
// This is what we return to API clients - excludes sensitive data like password hash
// DTOs provide a clean API contract and allow us to change internal models without breaking clients
public class UserDto
{
    public int Id { get; set; }                                    // User's unique identifier
    public string Username { get; set; } = string.Empty;          // Username for display
    public string Email { get; set; } = string.Empty;             // Email address
    public DateTime CreatedAt { get; set; }                       // Account creation date
    public DateTime? LastLoginAt { get; set; }                    // Last login timestamp
    public bool IsActive { get; set; }                            // Account status
    public string Role { get; set; } = string.Empty;              // User role (Admin/User)
    // Note: Password hash is NOT included for security
}

// Data Transfer Object for user registration
// Contains validation rules using Data Annotations
public class RegisterDto
{
    [Required]                                  // Username is mandatory
    [StringLength(50, MinimumLength = 3)]      // Username must be 3-50 characters
    public string Username { get; set; } = string.Empty;

    [Required]                                  // Email is mandatory
    [EmailAddress]                              // Must be valid email format
    public string Email { get; set; } = string.Empty;

    [Required]                                  // Password is mandatory
    [StringLength(100, MinimumLength = 6)]     // Password must be 6-100 characters
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";  // Default role is "User" (can be overridden to "Admin")
}

// Data Transfer Object for user login
public class LoginDto
{
    [Required]                                  // Username is required for login
    public string Username { get; set; } = string.Empty;

    [Required]                                  // Password is required for login
    public string Password { get; set; } = string.Empty;
}

// Data Transfer Object for login response
// Contains everything the client needs after successful authentication
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;              // JWT token for subsequent API calls
    public UserDto User { get; set; } = new();                     // User information (without sensitive data)
    public DateTime ExpiresAt { get; set; }                        // When the token expires (client can refresh before this)
}