using Serenity.ComponentModel;
using System.ComponentModel;

namespace SerenityProjem.Administration;

public enum UserActivityType
{
    [Description("Login")]
    Login = 1,

    [Description("Logout")]
    Logout = 2,

    [Description("Activity")]
    Activity = 3,

    [Description("Heartbeat")]
    Heartbeat = 4,

    [Description("Page View")]
    PageView = 5
}