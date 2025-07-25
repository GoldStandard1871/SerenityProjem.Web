using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250725_1200)]
public class DefaultDB_20250725_1200_Admin2Permissions : Migration
{
    public override void Up()
    {
        // First get admin2 user's ID and their roles
        Execute.Sql(@"
            DECLARE @UserId INT;
            DECLARE @RoleId INT;
            
            -- Get admin2 user ID
            SELECT @UserId = UserId FROM Users WHERE Username = 'admin2';
            
            IF @UserId IS NOT NULL
            BEGIN
                -- Get admin2's current role from UserRoles table
                SELECT TOP 1 @RoleId = RoleId 
                FROM UserRoles 
                WHERE UserId = @UserId;
                
                -- If user has a role, add permissions to it
                IF @RoleId IS NOT NULL
                BEGIN
                    -- Add permissions to the role
                    INSERT INTO RolePermissions (RoleId, PermissionKey)
                    SELECT @RoleId, PermissionKey
                    FROM (
                        VALUES 
                        ('MovieDB:Movie'),
                        ('MovieDB:Movie:Modify'),
                        ('MovieDB:Movie:Delete'),
                        ('MovieDB:Movie:View'),
                        ('MovieDB:Genre'),
                        ('MovieDB:Genre:Modify'),
                        ('MovieDB:Genre:Delete'),
                        ('MovieDB:Genre:View'),
                        ('MovieDB:Person'),
                        ('MovieDB:Person:Modify'),
                        ('MovieDB:Person:Delete'),
                        ('MovieDB:Person:View'),
                        ('MovieDB:MovieCast'),
                        ('MovieDB:MovieCast:Modify'),
                        ('MovieDB:MovieCast:Delete'),
                        ('MovieDB:MovieCast:View'),
                        ('MovieDB:MovieGenres'),
                        ('MovieDB:MovieGenres:Modify'),
                        ('MovieDB:MovieGenres:Delete'),
                        ('MovieDB:MovieGenres:View'),
                        ('Administration:ActivityReports'),
                        ('Administration:BackgroundJobs'),
                        ('Administration:UserActivity')
                    ) AS permissions(PermissionKey)
                    WHERE NOT EXISTS (
                        SELECT 1 FROM RolePermissions 
                        WHERE RoleId = @RoleId AND PermissionKey = permissions.PermissionKey
                    );
                    
                    PRINT 'Permissions added to role ID: ' + CAST(@RoleId AS VARCHAR);
                END
                ELSE
                BEGIN
                    PRINT 'No role found for admin2 user';
                END
            END
            ELSE
            BEGIN
                PRINT 'admin2 user not found';
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DECLARE @UserId INT;
            DECLARE @RoleId INT;
            
            -- Get admin2 user ID
            SELECT @UserId = UserId FROM Users WHERE Username = 'admin2';
            
            IF @UserId IS NOT NULL
            BEGIN
                -- Get admin2's role from UserRoles table
                SELECT TOP 1 @RoleId = RoleId 
                FROM UserRoles 
                WHERE UserId = @UserId;
                
                IF @RoleId IS NOT NULL
                BEGIN
                    -- Remove Movie and Activity permissions
                    DELETE FROM RolePermissions 
                    WHERE RoleId = @RoleId 
                    AND (PermissionKey LIKE 'MovieDB:%' 
                         OR PermissionKey IN ('Administration:ActivityReports', 
                                              'Administration:BackgroundJobs', 
                                              'Administration:UserActivity'));
                END
            END
        ");
    }
}