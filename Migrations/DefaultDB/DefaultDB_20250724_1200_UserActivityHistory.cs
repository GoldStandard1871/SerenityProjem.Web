using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250724_1200)]
public class DefaultDB_20250724_1200_UserActivityHistory : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("UserActivityHistory")
            .WithColumn("Id").AsInt64().PrimaryKey().Identity()
            .WithColumn("UserId").AsString(50).NotNullable()
            .WithColumn("Username").AsString(100).NotNullable()
            .WithColumn("ActivityType").AsString(20).NotNullable() // Login, Logout, Activity, Heartbeat
            .WithColumn("IpAddress").AsString(45).Nullable()
            .WithColumn("UserAgent").AsString(500).Nullable()
            .WithColumn("Location").AsString(200).Nullable()
            .WithColumn("Isp").AsString(200).Nullable()
            .WithColumn("Timezone").AsString(50).Nullable()
            .WithColumn("SessionId").AsString(100).Nullable()
            .WithColumn("ActivityTime").AsDateTime().NotNullable()
            .WithColumn("Details").AsString(1000).Nullable(); // JSON format for additional data

        Create.Index("IX_UserActivityHistory_UserId")
            .OnTable("UserActivityHistory")
            .OnColumn("UserId");

        Create.Index("IX_UserActivityHistory_ActivityTime")
            .OnTable("UserActivityHistory")
            .OnColumn("ActivityTime");

        Create.Index("IX_UserActivityHistory_ActivityType")
            .OnTable("UserActivityHistory")
            .OnColumn("ActivityType");
    }
}