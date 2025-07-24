using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[DefaultDB, MigrationKey(20250722_1500)]
public class DefaultDB_20250722_1500_AddAdmin2User : Migration
{
    public override void Up()
    {
        // Add admin2 user with same password hash as admin (serenity)
        Execute.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin2')
            BEGIN
                INSERT INTO Users (Username, DisplayName, Email, Source, PasswordHash, PasswordSalt, InsertDate, InsertUserId, IsActive)
                VALUES ('admin2', 'Administrator 2', 'admin2@test.com', 'site', 
                       'rfqpSPYs0ekFlPyvIRTXsdhE/qrTHFF+kKsAUla7pFkXL4BgLGlTe89GDX5DBysenMDj8AqbIZPybqvusyCjwQ', 
                       'hJf_F', GETDATE(), 1, 1);
            END
        ");

        // Get admin2 user id and assign admin role (find existing role ID)
        Execute.Sql(@"
            DECLARE @Admin2UserId INT;
            DECLARE @AdminRoleId INT;
            
            SELECT @Admin2UserId = UserId FROM Users WHERE Username = 'admin2';
            SELECT TOP 1 @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Administrators';
            
            IF @Admin2UserId IS NOT NULL AND @AdminRoleId IS NOT NULL
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @Admin2UserId AND RoleId = @AdminRoleId)
                BEGIN
                    INSERT INTO UserRoles (UserId, RoleId)
                    VALUES (@Admin2UserId, @AdminRoleId);
                END
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM UserRoles WHERE UserId IN (SELECT UserId FROM Users WHERE Username = 'admin2')");
        Execute.Sql("DELETE FROM Users WHERE Username = 'admin2'");
    }
}