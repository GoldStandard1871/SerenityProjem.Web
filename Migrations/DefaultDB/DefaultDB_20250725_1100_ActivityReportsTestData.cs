using FluentMigrator;
using System;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250725_1100)]
public class DefaultDB_20250725_1100_ActivityReportsTestData : Migration
{
    public override void Up()
    {
        // Add test data for Activity Reports
        Execute.Sql(@"
            -- Test data for today
            INSERT INTO UserActivityHistory (Username, ActivityType, Details, ActivityTime, IpAddress, UserAgent, UserId, Location)
            VALUES 
            -- admin2 activities for today
            ('admin2', 'Login', '{""LoginMethod"":""Standard""}', GETDATE(), '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'Dashboard', DATEADD(MINUTE, -30, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'Activity Reports', DATEADD(MINUTE, -25, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'Background Jobs', DATEADD(MINUTE, -20, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'User Management', DATEADD(MINUTE, -15, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'Action', 'Export Report', DATEADD(MINUTE, -10, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Istanbul, Turkey'),
            
            -- admin activities for today  
            ('admin', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(HOUR, -2, GETDATE()), '192.168.1.1', 'Mozilla/5.0', 1, 'Ankara, Turkey'),
            ('admin', 'PageView', 'Dashboard', DATEADD(HOUR, -2, DATEADD(MINUTE, 5, GETDATE())), '192.168.1.1', 'Mozilla/5.0', 1, 'Ankara, Turkey'),
            ('admin', 'PageView', 'Movies', DATEADD(HOUR, -1, GETDATE()), '192.168.1.1', 'Mozilla/5.0', 1, 'Ankara, Turkey'),
            ('admin', 'Action', 'Add Movie', DATEADD(MINUTE, -45, GETDATE()), '192.168.1.1', 'Mozilla/5.0', 1, 'Ankara, Turkey'),
            ('admin', 'Logout', '{""LogoutReason"":""Manual""}', DATEADD(MINUTE, -40, GETDATE()), '192.168.1.1', 'Mozilla/5.0', 1, 'Ankara, Turkey'),
            
            -- Previous days activities for weekly report
            ('admin2', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -1, GETDATE()), '127.0.0.1', 'Chrome/120.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'Dashboard', DATEADD(DAY, -1, DATEADD(MINUTE, 10, GETDATE())), '127.0.0.1', 'Chrome/120.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'PageView', 'Roles', DATEADD(DAY, -1, DATEADD(MINUTE, 20, GETDATE())), '127.0.0.1', 'Chrome/120.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'Action', 'Update Role', DATEADD(DAY, -1, DATEADD(MINUTE, 25, GETDATE())), '127.0.0.1', 'Chrome/120.0', 2, 'Istanbul, Turkey'),
            ('admin2', 'Logout', '{""LogoutReason"":""SessionTimeout""}', DATEADD(DAY, -1, DATEADD(HOUR, 2, GETDATE())), '127.0.0.1', 'Chrome/120.0', 2, 'Istanbul, Turkey'),
            
            ('admin', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -2, GETDATE()), '10.0.0.1', 'Firefox/121.0', 1, 'Izmir, Turkey'),
            ('admin', 'PageView', 'Dashboard', DATEADD(DAY, -2, DATEADD(MINUTE, 5, GETDATE())), '10.0.0.1', 'Firefox/121.0', 1, 'Izmir, Turkey'),
            ('admin', 'PageView', 'Languages', DATEADD(DAY, -2, DATEADD(MINUTE, 15, GETDATE())), '10.0.0.1', 'Firefox/121.0', 1, 'Izmir, Turkey'),
            ('admin', 'PageView', 'Translations', DATEADD(DAY, -2, DATEADD(MINUTE, 20, GETDATE())), '10.0.0.1', 'Firefox/121.0', 1, 'Izmir, Turkey'),
            
            -- More activities for this week
            ('admin2', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -3, GETDATE()), '172.16.0.1', 'Edge/120.0', 2, 'Bursa, Turkey'),
            ('admin2', 'PageView', 'Translations', DATEADD(DAY, -3, DATEADD(MINUTE, 10, GETDATE())), '172.16.0.1', 'Edge/120.0', 2, 'Bursa, Turkey'),
            ('admin2', 'PageView', 'Activity Reports', DATEADD(DAY, -3, DATEADD(MINUTE, 15, GETDATE())), '172.16.0.1', 'Edge/120.0', 2, 'Bursa, Turkey'),
            ('admin2', 'Action', 'Generate Report', DATEADD(DAY, -3, DATEADD(MINUTE, 20, GETDATE())), '172.16.0.1', 'Edge/120.0', 2, 'Bursa, Turkey'),
            
            ('admin', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -4, GETDATE()), '192.168.0.1', 'Safari/17.0', 1, 'Antalya, Turkey'),
            ('admin', 'PageView', 'Dashboard', DATEADD(DAY, -4, DATEADD(MINUTE, 5, GETDATE())), '192.168.0.1', 'Safari/17.0', 1, 'Antalya, Turkey'),
            ('admin', 'PageView', 'Movies', DATEADD(DAY, -4, DATEADD(MINUTE, 10, GETDATE())), '192.168.0.1', 'Safari/17.0', 1, 'Antalya, Turkey'),
            ('admin', 'PageView', 'Genres', DATEADD(DAY, -4, DATEADD(MINUTE, 15, GETDATE())), '192.168.0.1', 'Safari/17.0', 1, 'Antalya, Turkey'),
            ('admin', 'Action', 'Update Movie', DATEADD(DAY, -4, DATEADD(MINUTE, 20, GETDATE())), '192.168.0.1', 'Safari/17.0', 1, 'Antalya, Turkey'),
            
            -- Some activities from last week for monthly report
            ('admin2', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -7, GETDATE()), '203.0.113.1', 'Chrome/119.0', 2, 'Adana, Turkey'),
            ('admin2', 'PageView', 'Dashboard', DATEADD(DAY, -7, DATEADD(MINUTE, 5, GETDATE())), '203.0.113.1', 'Chrome/119.0', 2, 'Adana, Turkey'),
            ('admin', 'Login', '{""LoginMethod"":""Standard""}', DATEADD(DAY, -10, GETDATE()), '198.51.100.1', 'Firefox/120.0', 1, 'Kayseri, Turkey'),
            ('admin', 'PageView', 'Dashboard', DATEADD(DAY, -10, DATEADD(MINUTE, 5, GETDATE())), '198.51.100.1', 'Firefox/120.0', 1, 'Kayseri, Turkey')
        ");
    }

    public override void Down()
    {
        // Delete test data
        Execute.Sql(@"
            DELETE FROM UserActivityHistory 
            WHERE Username IN ('admin', 'admin2') 
            AND ActivityTime >= DATEADD(DAY, -30, GETDATE())
        ");
    }
}