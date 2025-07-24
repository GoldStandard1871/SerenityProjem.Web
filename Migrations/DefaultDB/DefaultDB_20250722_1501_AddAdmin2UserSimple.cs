using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[DefaultDB, MigrationKey(20250722_1501)]
public class DefaultDB_20250722_1501_AddAdmin2UserSimple : Migration
{
    public override void Up()
    {
        // Just add admin2 user, role assignment will be done manually
        Execute.Sql(@"
            IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin2')
            BEGIN
                INSERT INTO Users (Username, DisplayName, Email, Source, PasswordHash, PasswordSalt, InsertDate, InsertUserId, IsActive)
                VALUES ('admin2', 'Administrator 2', 'admin2@test.com', 'site', 
                       'rfqpSPYs0ekFlPyvIRTXsdhE/qrTHFF+kKsAUla7pFkXL4BgLGlTe89GDX5DBysenMDj8AqbIZPybqvusyCjwQ', 
                       'hJf_F', GETDATE(), 1, 1);
                
                PRINT 'admin2 user created successfully. Password: serenity';
            END
            ELSE
            BEGIN
                PRINT 'admin2 user already exists.';
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM Users WHERE Username = 'admin2'");
    }
}