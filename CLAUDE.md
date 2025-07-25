# Claude Memory - SerenityProjem Development Notes

## Project Overview
- **Framework**: Serenity.NET 8.8.1 with ASP.NET Core 8.0
- **Database**: SQL Server with migrations
- **Real-time**: SignalR for live updates
- **Background Jobs**: Hangfire integration
- **Frontend**: TypeScript, Chart.js

## Key Features Implemented
### 1. Real-time User Activity Tracking
- SignalR hub for live user monitoring
- IP geolocation tracking
- Activity logging to UserActivityHistory table

### 2. Activity Reports System
- **Location**: `Modules/Administration/ActivityReports/`
- Daily/Weekly/Monthly analytics with Chart.js
- Multi-format exports (CSV, HTML, JSON)
- Interactive dashboards
- **API Endpoints**:
  - `/api/activity-reports/daily`
  - `/api/activity-reports/weekly` 
  - `/api/activity-reports/monthly`
  - `/api/activity-reports/export/csv`

## Development Commands
```bash
# Safe startup (prevents file locking)
vs-start.bat

# Alternative development start
dev-start.bat

# Standard commands
dotnet run
dotnet build
npm install
```

## Known Issues & Solutions

### File Locking Problems
- **Issue**: Visual Studio/IIS Express locks DLL files
- **Solution**: Use `vs-start.bat` or `dev-start.bat`
- **Config**: Project has optimized settings in `.csproj`

### Quick Fix for DLL Lock Errors
When you see errors like "Dosya şunun tarafından kilitlendi: dotnet.exe (PID)"
- **Fastest Solution**: Kill the locking process directly
```bash
# Method 1: Kill specific PID (replace 25572 with actual PID from error)
powershell -Command "Stop-Process -Id 25572 -Force"

# Method 2: Kill all dotnet.exe processes
taskkill //F //IM dotnet.exe

# Method 3: Clean all development processes
taskkill //F //IM dotnet.exe && taskkill //F //IM iisexpress.exe

# PowerShell one-liner to clean everything
Get-Process dotnet,iisexpress,VBCSCompiler -ErrorAction SilentlyContinue | Stop-Process -Force
```

- **Then rebuild**:
```bash
dotnet build && dotnet run
```

- **Prevention Tips**:
  - Always stop debugging properly (Shift+F5)
  - Use `dotnet watch` instead of Visual Studio for development
  - Add to `.csproj` to reduce locking:
  ```xml
  <PropertyGroup>
    <UseRazorBuildServer>false</UseRazorBuildServer>
    <UseSharedCompilation>false</UseSharedCompilation>
  </PropertyGroup>
  ```

### Export System Testing
- **Status**: Implemented but needs additional validation
- **Location**: `ActivityReportsPage.cs` ExportToCSV method
- **Fixed**: Parameter validation added for null reference errors

### Database Schema
```sql
-- Main activity tracking table
UserActivityHistory (
    Id, UserId, Username, ActivityType, 
    IpAddress, UserAgent, Location, 
    ActivityTime, Details
)
```

## Test Data
- **File**: `TestData.sql` - Contains sample activity data
- **Users**: admin, admin2
- **Activities**: Login, PageView, Action types

## Project Structure
```
Modules/Administration/
├── ActivityReports/          # Main reporting system
├── UserActivity/            # Real-time tracking
├── BackgroundJobs/          # Hangfire management
└── Hangfire/               # Custom dashboard

Development Scripts/
├── vs-start.bat            # Safe VS startup
├── dev-start.bat           # Kestrel startup
└── clean-restart.bat       # Process cleanup
```

## TypeScript Configuration
- **Fixed**: Promise constructor errors with ES2015.Promise lib
- **Location**: `tsconfig.json` updated for async/await support

## Git Information
- **Main Branch**: `main` (not master)
- **Remote**: origin
- **Latest Features**: Activity Reports system fully implemented

## Important Notes for Future Sessions
1. Always check file locking issues first - use development scripts
2. Activity Reports export needs thorough testing
3. SignalR connections work properly for real-time features
4. Database migrations are up to date
5. README.md contains comprehensive documentation

## Quick Start for New Sessions
1. Read this file first
2. Check git status: `git status`
3. Use safe startup: `vs-start.bat`
4. Test Activity Reports at `/Administration/ActivityReports`

## Development Workflow Optimizations
- Project configured to prevent file locking
- Hot reload optimized
- TypeScript compilation automated
- Background jobs properly configured

Last Updated: July 24, 2025