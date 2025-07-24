using FluentMigrator;

namespace SerenityProjem.Migrations.DefaultDB;

[Migration(20250724_1300)]
public class DefaultDB_20250724_1300_UserActivityTables : Migration
{
    public override void Up()
    {
        // Günlük aktivite istatistikleri tablosu
        Create.Table("DailyActivityStats")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("Date").AsDate().NotNullable()
            .WithColumn("UniqueUsers").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("TotalLogins").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("TotalLogouts").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("UniqueLocations").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("AvgSessionMinutes").AsFloat().Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("IX_DailyActivityStats_Date")
            .OnTable("DailyActivityStats")
            .OnColumn("Date");

        // Online kullanıcı metrik tablosu
        Create.Table("OnlineUserMetrics")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("Timestamp").AsDateTime2().NotNullable()
            .WithColumn("OnlineCount").AsInt32().NotNullable().WithDefaultValue(0)
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

        Create.Index("IX_OnlineUserMetrics_Timestamp")
            .OnTable("OnlineUserMetrics")
            .OnColumn("Timestamp");

        // Sistem bildirimleri tablosu
        Create.Table("SystemNotifications")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("NotificationType").AsString(50).NotNullable()
            .WithColumn("Title").AsString(200).NotNullable()
            .WithColumn("Message").AsString(1000).NotNullable()
            .WithColumn("Severity").AsString(20).NotNullable().WithDefaultValue("Info") // Info, Warning, Error, Critical
            .WithColumn("IsRead").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("CreatedBy").AsString(100).Nullable()
            .WithColumn("CreatedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("ReadAt").AsDateTime2().Nullable()
            .WithColumn("ReadBy").AsString(100).Nullable()
            .WithColumn("Data").AsString(int.MaxValue).Nullable(); // JSON data

        Create.Index("IX_SystemNotifications_Type_Created")
            .OnTable("SystemNotifications")
            .OnColumn("NotificationType");

        Create.Index("IX_SystemNotifications_IsRead")
            .OnTable("SystemNotifications")
            .OnColumn("IsRead");

        // Haftalık/Aylık rapor metadata tablosu
        Create.Table("ActivityReports")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("ReportType").AsString(50).NotNullable() // Weekly, Monthly, Custom
            .WithColumn("PeriodStart").AsDate().NotNullable()
            .WithColumn("PeriodEnd").AsDate().NotNullable()
            .WithColumn("GeneratedAt").AsDateTime2().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
            .WithColumn("GeneratedBy").AsString(100).Nullable()
            .WithColumn("FilePath").AsString(500).Nullable()
            .WithColumn("Summary").AsString(int.MaxValue).Nullable() // JSON summary
            .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Generated"); // Generated, Sent, Archived

        Create.Index("IX_ActivityReports_Type_Period")
            .OnTable("ActivityReports")
            .OnColumn("ReportType");
    }

    public override void Down()
    {
        Delete.Table("ActivityReports");
        Delete.Table("SystemNotifications");
        Delete.Table("OnlineUserMetrics");
        Delete.Table("DailyActivityStats");
    }
}