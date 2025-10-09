using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinalActivityManagingMiddlewareWithCopilot.Models;

namespace FinalActivityManagingMiddlewareWithCopilot.Services;

// Interface defining JWT operations
// This allows for dependency injection and easier testing
public interface IJwtService
{
    string GenerateToken(User user);                    // Create JWT token for authenticated user
    bool ValidateToken(string token);                   // Quick validation check
    ClaimsPrincipal? GetPrincipalFromToken(string token); // Get user claims from valid token
}

// JWT Service implementation - handles creation and validation of JSON Web Tokens
// JWT tokens are used for stateless authentication (no server-side session storage needed)
public class JwtService : IJwtService
{
    // Configuration service to access appsettings.json values
    private readonly IConfiguration _configuration;
    // Logger for JWT-related events and errors
    private readonly ILogger<JwtService> _logger;

    // Constructor: Dependency injection of configuration and logging
    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Generate a JWT token for an authenticated user
    public string GenerateToken(User user)
    {
        // Get JWT configuration from appsettings.json
        var jwtSettings = _configuration.GetSection("JwtSettings");
        
        // Extract required JWT settings - throw exceptions if missing (fail fast)
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        // Create symmetric security key from secret (used for signing the token)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        // Create signing credentials using HMAC SHA-256 algorithm
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create claims (user information stored in the token)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID
            new Claim(ClaimTypes.Name, user.Username),                // Username
            new Claim(ClaimTypes.Email, user.Email),                  // Email address
            new Claim(ClaimTypes.Role, user.Role),                    // User role (Admin/User)
            new Claim("jti", Guid.NewGuid().ToString())               // JWT ID for token uniqueness
        };

        // Create JWT token with all required information
        var token = new JwtSecurityToken(
            issuer: issuer,                                           // Who issued the token
            audience: audience,                                       // Who the token is intended for
            claims: claims,                                           // User information/permissions
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes), // When token expires
            signingCredentials: credentials                           // How to verify token authenticity
        );

        // Serialize token to string format for transmission
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Simple boolean validation - returns true if token is valid
    public bool ValidateToken(string token)
    {
        try
        {
            // Use the more detailed validation method
            var principal = GetPrincipalFromToken(token);
            return principal != null;
        }
        catch
        {
            // Any exception means invalid token
            return false;
        }
    }

    // Validate JWT token and extract user claims if valid
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        try
        {
            // Get same JWT settings used for token generation
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

            // Create the same security key used for token generation
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Create token handler for validation
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Define validation parameters - what to check when validating the token
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,    // Verify token signature
                IssuerSigningKey = key,             // Key to verify signature
                ValidateIssuer = true,              // Check token issuer
                ValidIssuer = issuer,               // Expected issuer
                ValidateAudience = true,            // Check token audience
                ValidAudience = audience,           // Expected audience
                ValidateLifetime = true,            // Check if token has expired
                ClockSkew = TimeSpan.Zero           // No tolerance for time differences
            };

            // Validate the token and extract claims principal
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            // Log validation failure for debugging (don't throw - just return null)
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}