namespace SerenityProjem.Administration;

[NestedPermissionKeys]
[DisplayName("Administration")]
public class PermissionKeys
{
    [Description("User, Role Management and Permissions")]
    public const string Security = "Administration:Security";

    [Description("Languages and Translations")]
    public const string Translation = "Administration:Translation";

    [Description("Hangfire Background Jobs")]
    public const string BackgroundJob = "Administration:BackgroundJob"; // 🔧 eklenecek
}
