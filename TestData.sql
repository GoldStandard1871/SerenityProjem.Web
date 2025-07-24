-- Test verileri - Activity Reports için
-- Bugünkü tarih için örnek aktiviteler

INSERT INTO UserActivityHistory (Username, ActivityType, Details, ActivityTime, IpAddress, UserAgent, UserId)
VALUES 
-- admin2 için bugünkü aktiviteler
('admin2', 'Login', '{"LoginMethod":"Standard"}', GETDATE(), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Dashboard', DATEADD(MINUTE, -30, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Activity Reports', DATEADD(MINUTE, -25, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Background Jobs', DATEADD(MINUTE, -20, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'User Management', DATEADD(MINUTE, -15, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'Action', 'Export Report', DATEADD(MINUTE, -10, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),

-- admin için bugünkü aktiviteler  
('admin', 'Login', '{"LoginMethod":"Standard"}', DATEADD(HOUR, -2, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'PageView', 'Dashboard', DATEADD(HOUR, -2, DATEADD(MINUTE, 5, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'PageView', 'Movies', DATEADD(HOUR, -1, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'Action', 'Add Movie', DATEADD(MINUTE, -45, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'Logout', '{"LogoutReason":"Manual"}', DATEADD(MINUTE, -40, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),

-- Önceki günler için aktiviteler (haftalık rapor için)
('admin2', 'Login', '{"LoginMethod":"Standard"}', DATEADD(DAY, -1, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Dashboard', DATEADD(DAY, -1, DATEADD(MINUTE, 10, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Roles', DATEADD(DAY, -1, DATEADD(MINUTE, 20, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 4),

('admin', 'Login', '{"LoginMethod":"Standard"}', DATEADD(DAY, -2, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'PageView', 'Dashboard', DATEADD(DAY, -2, DATEADD(MINUTE, 5, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'PageView', 'Languages', DATEADD(DAY, -2, DATEADD(MINUTE, 15, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 1),

-- Bu hafta için daha fazla aktivite
('admin2', 'Login', '{"LoginMethod":"Standard"}', DATEADD(DAY, -3, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 4),
('admin2', 'PageView', 'Translations', DATEADD(DAY, -3, DATEADD(MINUTE, 10, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 4),
('admin', 'Login', '{"LoginMethod":"Standard"}', DATEADD(DAY, -4, GETDATE()), '127.0.0.1', 'Mozilla/5.0', 1),
('admin', 'PageView', 'Dashboard', DATEADD(DAY, -4, DATEADD(MINUTE, 5, GETDATE())), '127.0.0.1', 'Mozilla/5.0', 1);