using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250725_1000)]
public class DefaultDB_20250725_1000_MovieStatistics : Migration
{
    public override void Up()
    {
        Create.Table("MovieStatistics")
            .WithColumn("MovieId").AsInt32().NotNullable().PrimaryKey()
                .ForeignKey("FK_MovieStatistics_MovieId", "Movie", "MovieId")
            .WithColumn("CastMemberCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("PopularityScore").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("LastAccessedAt").AsDateTime().Nullable()
            .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);

        // Create indexes for better performance
        Create.Index("IX_MovieStatistics_PopularityScore")
            .OnTable("MovieStatistics")
            .OnColumn("PopularityScore").Descending();

        Create.Index("IX_MovieStatistics_UpdatedAt")
            .OnTable("MovieStatistics")
            .OnColumn("UpdatedAt");
    }

    public override void Down()
    {
        Delete.Table("MovieStatistics");
    }
}