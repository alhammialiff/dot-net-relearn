# User Management API - PowerShell Test Script
# This script tests all CRUD operations for the User Management API
# Make sure your API is running on http://localhost:5171 before running this script

$baseUrl = "http://localhost:5171/api/users"
$headers = @{ "Content-Type" = "application/json" }

Write-Host "=== User Management API CRUD Test Suite ===" -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Function to make HTTP requests with error handling
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [string]$Description
    )
    
    Write-Host "Test: $Description" -ForegroundColor Cyan
    Write-Host "$Method $Uri" -ForegroundColor Gray
    
    try {
        if ($Body) {
            Write-Host "Body: $Body" -ForegroundColor Gray
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -Body $Body -ErrorAction Stop
        } else {
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Headers $Headers -ErrorAction Stop
        }
        
        Write-Host "✅ Success" -ForegroundColor Green
        if ($response) {
            $response | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor White
        }
        Write-Host ""
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $statusDescription = $_.Exception.Response.StatusDescription
        Write-Host "❌ Failed - $statusCode $statusDescription" -ForegroundColor Red
        Write-Host ""
        return $null
    }
}

# Test 1: Get all users (initial state)
Write-Host "--- READ Operations ---" -ForegroundColor Magenta
$users = Invoke-ApiRequest -Method "GET" -Uri $baseUrl -Description "Get all users (initial state)"

# Test 2: Get user by ID
$user1 = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/1" -Description "Get user with ID 1"

# Test 3: Get user by ID
$user2 = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/2" -Description "Get user with ID 2"

# Test 4: CREATE Operations
Write-Host "--- CREATE Operations ---" -ForegroundColor Magenta
$newUser1Body = @{
    name = "Alice Johnson"
} | ConvertTo-Json

$createdUser1 = Invoke-ApiRequest -Method "POST" -Uri $baseUrl -Headers $headers -Body $newUser1Body -Description "Create new user (Alice Johnson)"

$newUser2Body = @{
    name = "Charlie Brown"
} | ConvertTo-Json

$createdUser2 = Invoke-ApiRequest -Method "POST" -Uri $baseUrl -Headers $headers -Body $newUser2Body -Description "Create new user (Charlie Brown)"

# Test 5: Get all users after creation
$usersAfterCreate = Invoke-ApiRequest -Method "GET" -Uri $baseUrl -Description "Get all users after creation"

# Test 6: UPDATE Operations
Write-Host "--- UPDATE Operations ---" -ForegroundColor Magenta
$updateUserBody = @{
    name = "John Doe Updated"
} | ConvertTo-Json

$updatedUser = Invoke-ApiRequest -Method "PUT" -Uri "$baseUrl/1" -Headers $headers -Body $updateUserBody -Description "Update user with ID 1"

# Verify the update
$verifyUpdate = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/1" -Description "Verify user update"

# Test 7: DELETE Operations
Write-Host "--- DELETE Operations ---" -ForegroundColor Magenta
$deleteResult = Invoke-ApiRequest -Method "DELETE" -Uri "$baseUrl/2" -Description "Delete user with ID 2"

# Try to get the deleted user
$deletedUser = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/2" -Description "Try to get deleted user (should fail)"

# Get all users after deletion
$usersAfterDelete = Invoke-ApiRequest -Method "GET" -Uri $baseUrl -Description "Get all users after deletion"

# Test 8: Error Handling
Write-Host "--- Error Handling Tests ---" -ForegroundColor Magenta

# Test non-existent user
$nonExistentUser = Invoke-ApiRequest -Method "GET" -Uri "$baseUrl/999" -Description "Get non-existent user (should return 404)"

# Test update non-existent user
$updateNonExistentBody = @{
    name = "Non-existent User"
} | ConvertTo-Json

$updateNonExistent = Invoke-ApiRequest -Method "PUT" -Uri "$baseUrl/999" -Headers $headers -Body $updateNonExistentBody -Description "Update non-existent user (should return 404)"

# Test delete non-existent user
$deleteNonExistent = Invoke-ApiRequest -Method "DELETE" -Uri "$baseUrl/999" -Description "Delete non-existent user (should return 404)"

# Test create user with empty name
$emptyNameBody = @{
    name = ""
} | ConvertTo-Json

$createEmpty = Invoke-ApiRequest -Method "POST" -Uri $baseUrl -Headers $headers -Body $emptyNameBody -Description "Create user with empty name (should return 400)"

# Test update user with empty name
$updateEmptyBody = @{
    name = ""
} | ConvertTo-Json

$updateEmpty = Invoke-ApiRequest -Method "PUT" -Uri "$baseUrl/1" -Headers $headers -Body $updateEmptyBody -Description "Update user with empty name (should return 400)"

Write-Host "=== Test Suite Completed ===" -ForegroundColor Green
Write-Host "All CRUD operations have been tested!" -ForegroundColor Yellow