#!/usr/bin/env pwsh
# PowerShell Test Script for User Management API

# Configuration
$baseUrl = "https://localhost:7238"  # Adjust port as needed
$adminToken = ""
$userToken = ""

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Blue = "Blue"

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Details = ""
    )
    
    $color = if ($Success) { $Green } else { $Red }
    $status = if ($Success) { "PASS" } else { "FAIL" }
    
    Write-Host "[$status] $TestName" -ForegroundColor $color
    if ($Details) {
        Write-Host "       $Details" -ForegroundColor Gray
    }
    Write-Host ""
}

function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Token = $null,
        [bool]$ExpectSuccess = $true
    )
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $params = @{
            Uri = "$baseUrl$Endpoint"
            Method = $Method
            Headers = $headers
        }
        
        if ($Body) {
            $params["Body"] = ($Body | ConvertTo-Json)
        }
        
        $response = Invoke-RestMethod @params
        return @{
            Success = $true
            Data = $response
            StatusCode = 200
        }
    }
    catch {
        $statusCode = if ($_.Exception.Response) { 
            [int]$_.Exception.Response.StatusCode 
        } else { 
            0 
        }
        
        $errorBody = ""
        try {
            if ($_.ErrorDetails.Message) {
                $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
            }
        }
        catch {
            $errorBody = $_.ErrorDetails.Message
        }
        
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = $statusCode
            ErrorBody = $errorBody
        }
    }
}

Write-Host "=== User Management API Test Suite ===" -ForegroundColor $Blue
Write-Host "Base URL: $baseUrl" -ForegroundColor $Blue
Write-Host ""

# Test 1: Public endpoint access (should work without auth)
Write-Host "1. Testing Public Endpoints" -ForegroundColor $Yellow
$result = Invoke-ApiRequest -Method "GET" -Endpoint "/weatherforecast"
Write-TestResult -TestName "Access public weatherforecast endpoint" -Success $result.Success -Details "Status: $($result.StatusCode)"

# Test 2: Registration Tests
Write-Host "2. Testing User Registration" -ForegroundColor $Yellow

# Valid registration
$registerData = @{
    username = "testuser$(Get-Random)"
    email = "testuser$(Get-Random)@example.com"
    password = "password123"
    role = "User"
}

$result = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/register" -Body $registerData
Write-TestResult -TestName "Register new user with valid data" -Success $result.Success -Details "User: $($registerData.username)"

# Invalid registration - duplicate username
$result = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/register" -Body @{
    username = "admin"  # This should already exist
    email = "newadmin@example.com"
    password = "password123"
    role = "User"
} -ExpectSuccess $false
Write-TestResult -TestName "Register user with duplicate username (should fail)" -Success ($result.StatusCode -eq 400 -or $result.StatusCode -eq 500) -Details "Status: $($result.StatusCode)"

# Invalid registration - missing required fields
$result = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/register" -Body @{
    username = ""
    email = "invalid@example.com"
    password = "123"  # Too short
} -ExpectSuccess $false
Write-TestResult -TestName "Register user with invalid data (should fail)" -Success ($result.StatusCode -eq 400) -Details "Status: $($result.StatusCode)"

# Test 3: Login Tests
Write-Host "3. Testing User Login" -ForegroundColor $Yellow

# Valid admin login
$loginResult = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body @{
    username = "admin"
    password = "admin123"
}
if ($loginResult.Success) {
    $adminToken = $loginResult.Data.token
}
Write-TestResult -TestName "Login with valid admin credentials" -Success $loginResult.Success -Details "Token received: $($adminToken.Length -gt 0)"

# Valid user login
$userLoginResult = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body @{
    username = "user"
    password = "user123"
}
if ($userLoginResult.Success) {
    $userToken = $userLoginResult.Data.token
}
Write-TestResult -TestName "Login with valid user credentials" -Success $userLoginResult.Success -Details "Token received: $($userToken.Length -gt 0)"

# Invalid login
$result = Invoke-ApiRequest -Method "POST" -Endpoint "/api/auth/login" -Body @{
    username = "admin"
    password = "wrongpassword"
} -ExpectSuccess $false
Write-TestResult -TestName "Login with invalid credentials (should fail)" -Success ($result.StatusCode -eq 401 -or $result.StatusCode -eq 500) -Details "Status: $($result.StatusCode)"

# Test 4: Protected Endpoint Access
Write-Host "4. Testing Protected Endpoints" -ForegroundColor $Yellow

# Access without token (should fail)
$result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users" -ExpectSuccess $false
Write-TestResult -TestName "Access protected endpoint without token (should fail)" -Success ($result.StatusCode -eq 401) -Details "Status: $($result.StatusCode)"

# Access with invalid token (should fail)
$result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users" -Token "invalid-token" -ExpectSuccess $false
Write-TestResult -TestName "Access protected endpoint with invalid token (should fail)" -Success ($result.StatusCode -eq 401) -Details "Status: $($result.StatusCode)"

# Access with valid admin token (should succeed)
if ($adminToken) {
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users" -Token $adminToken
    Write-TestResult -TestName "Access protected endpoint with valid admin token" -Success $result.Success -Details "Users count: $($result.Data.Count)"
}

# Test 5: User Management Operations
Write-Host "5. Testing User Management Operations" -ForegroundColor $Yellow

if ($adminToken) {
    # Get all users
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users" -Token $adminToken
    Write-TestResult -TestName "Get all users as admin" -Success $result.Success -Details "Users found: $($result.Data.Count)"
    
    # Get specific user
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users/1" -Token $adminToken
    Write-TestResult -TestName "Get specific user by ID as admin" -Success $result.Success -Details "User: $($result.Data.username)"
    
    # Get user profile
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users/profile" -Token $adminToken
    Write-TestResult -TestName "Get own profile as admin" -Success $result.Success -Details "Profile: $($result.Data.username)"
}

if ($userToken) {
    # Get all users as regular user
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users" -Token $userToken
    Write-TestResult -TestName "Get all users as regular user" -Success $result.Success -Details "Users found: $($result.Data.Count)"
    
    # Get own profile as regular user
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users/profile" -Token $userToken
    Write-TestResult -TestName "Get own profile as regular user" -Success $result.Success -Details "Profile: $($result.Data.username)"
}

# Test 6: Authorization Tests
Write-Host "6. Testing Authorization" -ForegroundColor $Yellow

if ($userToken) {
    # Try to delete another user as regular user (should fail)
    $result = Invoke-ApiRequest -Method "DELETE" -Endpoint "/api/users/1" -Token $userToken -ExpectSuccess $false
    Write-TestResult -TestName "Regular user trying to delete admin (should fail)" -Success ($result.StatusCode -eq 403 -or $result.StatusCode -eq 401) -Details "Status: $($result.StatusCode)"
}

# Test 7: Error Handling Tests
Write-Host "7. Testing Error Handling" -ForegroundColor $Yellow

if ($adminToken) {
    # Try to get non-existent user
    $result = Invoke-ApiRequest -Method "GET" -Endpoint "/api/users/999" -Token $adminToken -ExpectSuccess $false
    Write-TestResult -TestName "Get non-existent user (should return 404)" -Success ($result.StatusCode -eq 404) -Details "Status: $($result.StatusCode)"
    
    # Try to delete non-existent user
    $result = Invoke-ApiRequest -Method "DELETE" -Endpoint "/api/users/999" -Token $adminToken -ExpectSuccess $false
    Write-TestResult -TestName "Delete non-existent user (should return 404)" -Success ($result.StatusCode -eq 404) -Details "Status: $($result.StatusCode)"
}

# Test 8: Malformed Request Tests
Write-Host "8. Testing Malformed Requests" -ForegroundColor $Yellow

# Send invalid JSON
try {
    $headers = @{ "Content-Type" = "application/json" }
    Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Headers $headers -Body "invalid json" -ErrorAction Stop
    Write-TestResult -TestName "Send malformed JSON (should fail)" -Success $false
} catch {
    $statusCode = [int]$_.Exception.Response.StatusCode
    Write-TestResult -TestName "Send malformed JSON (should fail)" -Success ($statusCode -eq 400 -or $statusCode -eq 500) -Details "Status: $statusCode"
}

Write-Host "=== Test Suite Complete ===" -ForegroundColor $Blue
Write-Host ""
Write-Host "Note: Make sure the API is running on $baseUrl before running these tests." -ForegroundColor $Yellow
Write-Host "To start the API, run: dotnet run" -ForegroundColor $Yellow