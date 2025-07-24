using Serenity.ComponentModel;

namespace SerenityProjem.Administration;

[FormScript("Administration.UserActivityHistory")]
[BasedOnRow(typeof(UserActivityHistoryRow), CheckNames = true)]
public class UserActivityHistoryForm
{
    [Tab("General")]
    public string UserId { get; set; }
    public string Username { get; set; }
    public string ActivityType { get; set; }
    public DateTime ActivityTime { get; set; }
    
    [Tab("Location & Device")]
    public string IpAddress { get; set; }
    public string Location { get; set; }
    public string Isp { get; set; }
    public string Timezone { get; set; }
    
    [Tab("Technical")]
    [TextAreaEditor(Rows = 3)]
    public string UserAgent { get; set; }
    public string SessionId { get; set; }
    
    [TextAreaEditor(Rows = 5)]
    public string Details { get; set; }
}