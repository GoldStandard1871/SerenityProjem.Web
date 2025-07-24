# Kill any existing processes
Write-Host "Stopping existing SerenityProjem processes..." -ForegroundColor Yellow
Get-Process -Name "SerenityProjem.Web" -ErrorAction SilentlyContinue | Stop-Process -Force

# Clean build files
Write-Host "Cleaning build files..." -ForegroundColor Yellow
$exePath = "bin\Debug\net8.0\SerenityProjem.Web.exe"
$apphostPath = "obj\Debug\net8.0\apphost.exe"

if (Test-Path $exePath) {
    Remove-Item $exePath -Force -ErrorAction SilentlyContinue
}
if (Test-Path $apphostPath) {
    Remove-Item $apphostPath -Force -ErrorAction SilentlyContinue
}

# Start with hot reload
Write-Host "Starting development server with hot reload..." -ForegroundColor Green
dotnet watch run