namespace FinalActivityManagingMiddlewareWithCopilot.Models;

// User entity class - represents a user in the system
// This is the internal data model (what we store in database/memory)
// Note: We don't expose this directly to API clients - use DTOs instead
public class User
{
    public int Id { get; set; }                                    // Unique identifier for the user
    public string Username { get; set; } = string.Empty;          // Unique username for login
    public string Email { get; set; } = string.Empty;             // User's email address
    public string PasswordHash { get; set; } = string.Empty;      // BCrypt hashed password (NEVER store plaintext!)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;    // When account was created
    public DateTime? LastLoginAt { get; set; }                    // When user last logged in (nullable - may never have logged in)
    public bool IsActive { get; set; } = true;                    // Whether account is active (for soft delete)
    public string Role { get; set; } = "User";                    // User role for authorization (Admin/User)
}