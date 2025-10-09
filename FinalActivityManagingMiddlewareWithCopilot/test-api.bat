@echo off
echo === User Management API Test Suite (Batch Version) ===
echo.

set BASE_URL=https://localhost:7238
set ADMIN_TOKEN=
set USER_TOKEN=

echo Note: Make sure the API is running first with 'dotnet run'
echo Press any key to continue with tests...
pause > nul

echo.
echo 1. Testing Public Endpoints
echo ===========================
curl -s -o response.json -w "Status: %%{http_code}\n" %BASE_URL%/weatherforecast
echo Public weatherforecast endpoint test completed
echo.

echo 2. Testing User Registration  
echo ============================
curl -s -o response.json -w "Status: %%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"username\":\"testuser%RANDOM%\",\"email\":\"test%RANDOM%@example.com\",\"password\":\"password123\",\"role\":\"User\"}" %BASE_URL%/api/auth/register
echo Valid registration test completed

curl -s -o response.json -w "Status: %%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"email\":\"newadmin@example.com\",\"password\":\"password123\"}" %BASE_URL%/api/auth/register
echo Duplicate username test completed (should fail)
echo.

echo 3. Testing User Login
echo =====================
curl -s -o response.json -w "Status: %%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"admin123\"}" %BASE_URL%/api/auth/login
echo Admin login test completed

curl -s -o response.json -w "Status: %%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"username\":\"user\",\"password\":\"user123\"}" %BASE_URL%/api/auth/login  
echo User login test completed

curl -s -o response.json -w "Status: %%{http_code}\n" -X POST -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"wrongpassword\"}" %BASE_URL%/api/auth/login
echo Invalid login test completed (should fail)
echo.

echo 4. Testing Protected Endpoints
echo ===============================
curl -s -o response.json -w "Status: %%{http_code}\n" %BASE_URL%/api/users
echo Access without token test completed (should fail with 401)

curl -s -o response.json -w "Status: %%{http_code}\n" -H "Authorization: Bearer invalid-token" %BASE_URL%/api/users
echo Access with invalid token test completed (should fail with 401)
echo.

echo 5. Testing Error Handling
echo ==========================
curl -s -o response.json -w "Status: %%{http_code}\n" %BASE_URL%/api/users/999
echo Non-existent user test completed (should fail with 401 due to no auth)
echo.

echo === Test Suite Complete ===
echo.
echo To get a token for authenticated requests:
echo 1. Login using: curl -X POST -H "Content-Type: application/json" -d "{\"username\":\"admin\",\"password\":\"admin123\"}" %BASE_URL%/api/auth/login
echo 2. Copy the token from the response  
echo 3. Use it in Authorization header: -H "Authorization: Bearer YOUR_TOKEN"
echo.
echo Check response.json file for detailed responses
if exist response.json del response.json
pause