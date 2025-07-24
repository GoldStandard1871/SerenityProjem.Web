using Serenity.ComponentModel;
using System.ComponentModel;

namespace SerenityProjem.Administration;

[ColumnsScript("Administration.UserActivityHistory")]
[BasedOnRow(typeof(UserActivityHistoryRow), CheckNames = true)]
public class UserActivityHistoryColumns
{
    [EditLink, DisplayName("Db.Shared.RecordId"), AlignRight]
    public long Id { get; set; }
    
    [EditLink, Width(120)]
    public string Username { get; set; }
    
    [Width(100)]
    public string ActivityType { get; set; }
    
    [Width(120)]
    public string IpAddress { get; set; }
    
    [Width(200)]
    public string Location { get; set; }
    
    [Width(150)]
    public string Isp { get; set; }
    
    [Width(100)]
    public string Timezone { get; set; }
    
    [Width(160)]
    public DateTime ActivityTime { get; set; }
    
    [Width(300)]
    public string Details { get; set; }
}