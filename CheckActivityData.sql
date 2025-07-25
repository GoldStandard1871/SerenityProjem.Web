-- Check recent activity data
SELECT TOP 20 
    Id,
    Username,
    ActivityType,
    Details,
    ActivityTime,
    IpAddress,
    Location
FROM UserActivityHistory
ORDER BY ActivityTime DESC;

-- Check today's activities
SELECT 
    Username,
    COUNT(*) as ActivityCount,
    MIN(ActivityTime) as FirstActivity,
    MAX(ActivityTime) as LastActivity
FROM UserActivityHistory
WHERE CAST(ActivityTime AS DATE) = CAST(GETDATE() AS DATE)
GROUP BY Username;

-- Check if there are any PageView activities
SELECT COUNT(*) as PageViewCount
FROM UserActivityHistory
WHERE ActivityType = 'PageView';

-- Insert test data for current time
INSERT INTO UserActivityHistory (Username, ActivityType, Details, ActivityTime, IpAddress, UserAgent, UserId, Location)
VALUES 
('admin2', 'Login', '{"LoginMethod":"Standard"}', GETDATE(), '127.0.0.1', 'Mozilla/5.0', 2, 'Local'),
('admin2', 'PageView', 'Dashboard', DATEADD(MINUTE, -5, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Local'),
('admin2', 'PageView', 'Activity Reports', DATEADD(MINUTE, -3, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Local'),
('admin2', 'PageView', 'Movies', DATEADD(MINUTE, -2, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Local'),
('admin2', 'Action', 'View Report', DATEADD(MINUTE, -1, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 2, 'Local');