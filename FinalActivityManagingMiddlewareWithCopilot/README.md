# User Management API with Middleware

A comprehensive User Management API built with ASP.NET Core 9.0 featuring custom middleware for error handling, authentication, and logging.

## Features

### Middleware Components (in order)
1. **Error Handling Middleware** - Standardizes error responses across all endpoints
2. **Authentication Middleware** - JWT token-based authentication with 401 for unauthorized requests  
3. **Logging Middleware** - Logs all incoming requests and outgoing responses for auditing

### API Endpoints

#### Authentication Endpoints (Public)
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and receive JWT token

#### User Management Endpoints (Protected)
- `GET /api/users` - Get all users (requires authentication)
- `GET /api/users/{id}` - Get user by ID (requires authentication)
- `GET /api/users/profile` - Get current user profile (requires authentication)
- `DELETE /api/users/{id}` - Delete user (admin or self only)

#### Public Endpoints
- `GET /weatherforecast` - Sample weather forecast (no authentication required)

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Visual Studio Code or Visual Studio 2022

### Installation & Running

1. **Clone and navigate to the project:**
   ```bash
   cd FinalActivityManagingMiddlewareWithCopilot
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7238`

### Default Users
The application comes with two seeded users:

| Username | Password  | Role  |
|----------|-----------|-------|
| admin    | admin123  | Admin |
| user     | user123   | User  |

## API Usage

### 1. Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "password123",
  "role": "User"
}
```

### 2. Login and Get Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin", 
  "password": "admin123"
}
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "role": "Admin",
    "isActive": true
  },
  "expiresAt": "2023-10-09T15:30:00Z"
}
```

### 3. Use Token for Protected Endpoints
```http
GET /api/users
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

## Middleware Details

### Error Handling Middleware
- Catches all unhandled exceptions
- Returns standardized JSON error responses:
  - `400 Bad Request` - For validation errors
  - `401 Unauthorized` - For authentication errors  
  - `404 Not Found` - For missing resources
  - `500 Internal Server Error` - For all other exceptions

Example error response:
```json
{
  "error": "Internal server error"
}
```

### Authentication Middleware
- Validates JWT tokens from Authorization header
- Skips authentication for public endpoints
- Returns `401 Unauthorized` for missing/invalid tokens
- Sets user context for authenticated requests

### Logging Middleware  
- Logs all HTTP requests and responses
- Includes request/response headers, body, timing
- Structured logging with correlation IDs
- Configurable log levels (Info for success, Warning for errors)

## Testing

The project includes comprehensive test scripts:

### PowerShell Test Script
```powershell
.\test-api.ps1
```

### Batch Test Script (Windows)
```cmd
test-api.bat
```

### Manual Testing with HTTP File
Use `FinalActivityManagingMiddlewareWithCopilot.http` in VS Code with the REST Client extension.

### Test Coverage
The test scripts cover:
- ✅ Public endpoint access (no auth required)
- ✅ User registration (valid and invalid data)
- ✅ User login (valid and invalid credentials)
- ✅ Protected endpoint access (with/without tokens)
- ✅ Authorization (admin vs user permissions)
- ✅ Error handling (404, 401, 400, 500)
- ✅ Malformed requests
- ✅ Edge cases and security scenarios

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "MyVerySecureSecretKeyForJWTTokenGeneration12345",
    "Issuer": "FinalActivityManagingMiddlewareWithCopilot", 
    "Audience": "FinalActivityManagingMiddlewareWithCopilot",
    "ExpirationMinutes": 60
  }
}
```

## Security Features

- **Password Hashing**: Uses BCrypt for secure password storage
- **JWT Authentication**: Stateless token-based authentication
- **Authorization**: Role-based access control (Admin/User)
- **Input Validation**: Data annotations and model validation
- **Error Masking**: Sensitive error details not exposed to clients
- **Request Logging**: Full audit trail of API access

## Architecture

```
┌─────────────────┐
│   HTTP Request  │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ Error Handling  │ ◄─── Catches exceptions, standardizes responses
│   Middleware    │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│ Authentication  │ ◄─── Validates JWT tokens, sets user context
│   Middleware    │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│    Logging      │ ◄─── Logs requests/responses for auditing
│   Middleware    │
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│  Controllers    │ ◄─── Handle business logic
└─────────┬───────┘
          │
          ▼
┌─────────────────┐
│   Services      │ ◄─── User management, JWT service
└─────────────────┘
```

## Dependencies

- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication support
- `System.IdentityModel.Tokens.Jwt` - JWT token handling
- `BCrypt.Net-Next` - Password hashing
- `Microsoft.AspNetCore.OpenApi` - OpenAPI/Swagger support

## Development Notes

- The application uses in-memory storage for demo purposes
- In production, replace with a proper database (Entity Framework, etc.)
- Consider adding rate limiting, HTTPS enforcement, and additional security headers
- The JWT secret key should be stored securely (Azure Key Vault, etc.)
- Add comprehensive unit and integration tests for production use