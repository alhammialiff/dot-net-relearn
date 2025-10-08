@echo off
REM User Management API - CRUD Test Script (Windows Batch)
REM Make sure your API is running on http://localhost:5171 before running this script
REM This script uses curl commands that work on Windows

echo === User Management API CRUD Test Suite ===
echo Base URL: http://localhost:5171/api/users
echo.

set BASE_URL=http://localhost:5171/api/users

echo --- READ Operations ---
echo Test 1: Get all users (initial state)
curl -X GET "%BASE_URL%" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 2: Get user with ID 1
curl -X GET "%BASE_URL%/1" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 3: Get user with ID 2
curl -X GET "%BASE_URL%/2" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo --- CREATE Operations ---
echo Test 4: Create new user (Alice Johnson)
curl -X POST "%BASE_URL%" -H "Content-Type: application/json" -d "{\"name\":\"Alice Johnson\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 5: Create new user (Charlie Brown)
curl -X POST "%BASE_URL%" -H "Content-Type: application/json" -d "{\"name\":\"Charlie Brown\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 6: Get all users after creation
curl -X GET "%BASE_URL%" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo --- UPDATE Operations ---
echo Test 7: Update user with ID 1
curl -X PUT "%BASE_URL%/1" -H "Content-Type: application/json" -d "{\"name\":\"John Doe Updated\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 8: Verify user update
curl -X GET "%BASE_URL%/1" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo --- DELETE Operations ---
echo Test 9: Delete user with ID 2
curl -X DELETE "%BASE_URL%/2" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 10: Try to get deleted user (should return 404)
curl -X GET "%BASE_URL%/2" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 11: Get all users after deletion
curl -X GET "%BASE_URL%" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo --- Error Handling Tests ---
echo Test 12: Get non-existent user (should return 404)
curl -X GET "%BASE_URL%/999" -H "Accept: application/json" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 13: Update non-existent user (should return 404)
curl -X PUT "%BASE_URL%/999" -H "Content-Type: application/json" -d "{\"name\":\"Non-existent User\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 14: Delete non-existent user (should return 404)
curl -X DELETE "%BASE_URL%/999" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 15: Create user with empty name (should return 400)
curl -X POST "%BASE_URL%" -H "Content-Type: application/json" -d "{\"name\":\"\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo Test 16: Update user with empty name (should return 400)
curl -X PUT "%BASE_URL%/1" -H "Content-Type: application/json" -d "{\"name\":\"\"}" -w "\nStatus Code: %%{http_code}\n\n"

echo === Test Suite Completed ===
echo All CRUD operations have been tested!
pause