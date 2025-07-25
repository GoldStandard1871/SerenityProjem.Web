using FluentMigrator;
using System;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250725_1300)]
public class DefaultDB_20250725_1300_AddCurrentActivityData : Migration
{
    public override void Up()
    {
        // Add current activity data for testing
        Execute.Sql(@"
            -- Add activities for the last hour
            INSERT INTO UserActivityHistory (UserId, Username, ActivityType, Details, ActivityTime, IpAddress, UserAgent, Location)
            VALUES 
            -- admin2 activities
            (2, 'admin2', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(MINUTE, -45, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Dashboard', DATEADD(MINUTE, -40, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Movies', DATEADD(MINUTE, -35, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Activity Reports', DATEADD(MINUTE, -30, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'Action', 'View Movie List', DATEADD(MINUTE, -25, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Background Jobs', DATEADD(MINUTE, -20, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'User Management', DATEADD(MINUTE, -15, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'Action', 'Generate Report', DATEADD(MINUTE, -10, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Dashboard', DATEADD(MINUTE, -5, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            (2, 'admin2', 'PageView', 'Activity Reports', GETDATE(), '127.0.0.1', 'Mozilla/5.0', 'Local'),
            
            -- admin activities  
            (1, 'admin', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(HOUR, -2, GETDATE()), '192.168.1.1', 'Chrome/120.0', 'Office Network'),
            (1, 'admin', 'PageView', 'Dashboard', DATEADD(HOUR, -2, DATEADD(MINUTE, 5, GETDATE())), '192.168.1.1', 'Chrome/120.0', 'Office Network'),
            (1, 'admin', 'PageView', 'Movies', DATEADD(MINUTE, -90, GETDATE()), '192.168.1.1', 'Chrome/120.0', 'Office Network'),
            (1, 'admin', 'Action', 'Add Movie', DATEADD(MINUTE, -85, GETDATE()), '192.168.1.1', 'Chrome/120.0', 'Office Network'),
            (1, 'admin', 'PageView', 'Genres', DATEADD(MINUTE, -80, GETDATE()), '192.168.1.1', 'Chrome/120.0', 'Office Network'),
            (1, 'admin', 'Logout', '{""LogoutReason"":""Manual""}', DATEADD(MINUTE, -75, GETDATE()), '192.168.1.1', 'Chrome/120.0', 'Office Network')
        ");
    }

    public override void Down()
    {
        // Remove test data
        Execute.Sql(@"
            DELETE FROM UserActivityHistory 
            WHERE ActivityTime >= DATEADD(HOUR, -3, GETDATE())
            AND Username IN ('admin', 'admin2')
        ");
    }
}