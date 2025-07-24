@echo off
echo ========================================
echo  SerenityProjem Clean Restart Script
echo ========================================

echo [1/6] Killing existing processes...
wmic process where "name='SerenityProjem.Web.exe'" delete 2>nul
wmic process where "name='dotnet.exe' and commandline like '%%SerenityProjem%%'" delete 2>nul
wmic process where "name='iisexpress.exe'" delete 2>nul
wmic process where "name='w3wp.exe'" delete 2>nul

echo [2/6] Waiting for file handles to release...
timeout /t 2 /nobreak >nul

echo [3/6] Cleaning bin and obj folders...
if exist "bin" rmdir /s /q "bin" 2>nul
if exist "obj" rmdir /s /q "obj" 2>nul
if exist "wwwroot\esm" rmdir /s /q "wwwroot\esm" 2>nul

echo [4/6] Restoring packages...
dotnet restore --no-cache

echo [5/6] Building project...
dotnet build --no-restore

echo [6/6] Starting with hot reload...
echo.
echo ========================================
echo  Starting Development Server
echo  URL: http://localhost:5000
echo  Press Ctrl+C to stop
echo ========================================
echo.
set DOTNET_USE_POLLING_FILE_WATCHER=true
set DOTNET_WATCH_RESTART_ON_RUDE_EDIT=true
dotnet watch run

pause