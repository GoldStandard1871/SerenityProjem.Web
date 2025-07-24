@echo off
echo ========================================
echo  Visual Studio Safe Start Script
echo ========================================

echo [1/4] Killing all development processes...
taskkill /f /im "SerenityProjem.Web.exe" /t 2>nul
taskkill /f /im "dotnet.exe" /t 2>nul
taskkill /f /im "iisexpress.exe" /t 2>nul
taskkill /f /im "w3wp.exe" /t 2>nul

echo [2/4] Waiting for file handles to release...
timeout /t 3 /nobreak >nul

echo [3/4] Cleaning build artifacts...
if exist "bin" rmdir /s /q "bin" 2>nul
if exist "obj" rmdir /s /q "obj" 2>nul

echo [4/4] Starting with Kestrel (not IIS Express)...
echo.
echo ========================================
echo  URL: http://localhost:5000
echo  Press Ctrl+C to stop
echo ========================================
echo.

start "SerenityProjem" cmd /k "dotnet run --no-build --no-restore"

echo Application starting in new window...
pause