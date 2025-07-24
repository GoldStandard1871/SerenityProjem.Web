using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250724_1401)]
public class DefaultDB_20250724_1401_SimpleMovieCastTest : Migration
{
    public override void Up()
    {
        // Önce mevcut tabloları listeleyelim
        Execute.Sql(@"
            PRINT 'Available tables:';
            SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
            ");
        
        // MovieCast'e basit test verisi ekleyelim (eğer Movie ve Person varsa)
        Execute.Sql(@"
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MovieCast')
               AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Movie') 
               AND EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Person')
            BEGIN
                PRINT 'All required tables exist, adding test data...';
                
                -- Test için basit veriler ekleyelim
                IF EXISTS (SELECT 1 FROM Movie WHERE MovieId = 1) 
                   AND EXISTS (SELECT 1 FROM Person WHERE PersonId = 1)
                   AND NOT EXISTS (SELECT 1 FROM MovieCast WHERE MovieId = 1 AND PersonId = 1)
                BEGIN
                    INSERT INTO MovieCast (MovieId, PersonId, Character) VALUES (1, 1, 'Test Character 1');
                    PRINT 'Added test MovieCast 1';
                END
                
                IF EXISTS (SELECT 1 FROM Movie WHERE MovieId = 1) 
                   AND EXISTS (SELECT 1 FROM Person WHERE PersonId = 2)
                   AND NOT EXISTS (SELECT 1 FROM MovieCast WHERE MovieId = 1 AND PersonId = 2)
                BEGIN
                    INSERT INTO MovieCast (MovieId, PersonId, Character) VALUES (1, 2, 'Test Character 2');
                    PRINT 'Added test MovieCast 2';
                END
                
                IF EXISTS (SELECT 1 FROM Movie WHERE MovieId = 2) 
                   AND EXISTS (SELECT 1 FROM Person WHERE PersonId = 1)
                   AND NOT EXISTS (SELECT 1 FROM MovieCast WHERE MovieId = 2 AND PersonId = 1)
                BEGIN
                    INSERT INTO MovieCast (MovieId, PersonId, Character) VALUES (2, 1, 'Test Character 3');
                    PRINT 'Added test MovieCast 3';
                END
            END
            ELSE
            BEGIN
                PRINT 'Required tables do not exist, skipping test data insertion';
            END
            ");
    }

    public override void Down()
    {
        Execute.Sql("DELETE FROM MovieCast WHERE Character LIKE 'Test Character%'");
    }
}