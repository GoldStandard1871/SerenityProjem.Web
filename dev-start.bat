@echo off
echo Stopping any existing SerenityProjem processes...
wmic process where "name='SerenityProjem.Web.exe'" delete 2>nul

echo Cleaning build outputs...
if exist "bin\Debug\net8.0\SerenityProjem.Web.exe" (
    del /f /q "bin\Debug\net8.0\SerenityProjem.Web.exe" 2>nul
)
if exist "obj\Debug\net8.0\apphost.exe" (
    del /f /q "obj\Debug\net8.0\apphost.exe" 2>nul
)

echo Starting development server with hot reload...
dotnet watch run

pause