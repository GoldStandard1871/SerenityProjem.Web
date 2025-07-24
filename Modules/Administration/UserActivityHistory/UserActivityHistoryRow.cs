using Serenity.ComponentModel;
using Serenity.Data;
using Serenity.Data.Mapping;
using System.ComponentModel;

namespace SerenityProjem.Administration;

[ConnectionKey("Default"), Module("Administration"), TableName("UserActivityHistory")]
[DisplayName("User Activity History"), InstanceName("Activity Record")]
[ReadPermission("Administration:General")]
[ModifyPermission("Administration:General")]
public sealed class UserActivityHistoryRow : Row<UserActivityHistoryRow.RowFields>, IIdRow, INameRow
{
    [DisplayName("Id"), Identity, IdProperty]
    public long? Id { get => fields.Id[this]; set => fields.Id[this] = value; }

    [DisplayName("User Id"), Size(50), NotNull, QuickSearch]
    public string UserId { get => fields.UserId[this]; set => fields.UserId[this] = value; }

    [DisplayName("Username"), Size(100), NotNull, NameProperty]
    public string Username { get => fields.Username[this]; set => fields.Username[this] = value; }

    [DisplayName("Activity Type"), Size(20), NotNull]
    public string ActivityType { get => fields.ActivityType[this]; set => fields.ActivityType[this] = value; }

    [DisplayName("IP Address"), Size(45)]
    public string IpAddress { get => fields.IpAddress[this]; set => fields.IpAddress[this] = value; }

    [DisplayName("User Agent"), Size(500)]
    public string UserAgent { get => fields.UserAgent[this]; set => fields.UserAgent[this] = value; }

    [DisplayName("Location"), Size(200)]
    public string Location { get => fields.Location[this]; set => fields.Location[this] = value; }

    [DisplayName("ISP"), Size(200)]
    public string Isp { get => fields.Isp[this]; set => fields.Isp[this] = value; }

    [DisplayName("Timezone"), Size(50)]
    public string Timezone { get => fields.Timezone[this]; set => fields.Timezone[this] = value; }

    [DisplayName("Session Id"), Size(100)]
    public string SessionId { get => fields.SessionId[this]; set => fields.SessionId[this] = value; }

    [DisplayName("Activity Time"), NotNull]
    public DateTime? ActivityTime { get => fields.ActivityTime[this]; set => fields.ActivityTime[this] = value; }

    [DisplayName("Details"), Size(1000)]
    public string Details { get => fields.Details[this]; set => fields.Details[this] = value; }

    public class RowFields : RowFieldsBase
    {
        public Int64Field Id;
        public StringField UserId;
        public StringField Username;
        public StringField ActivityType;
        public StringField IpAddress;
        public StringField UserAgent;
        public StringField Location;
        public StringField Isp;
        public StringField Timezone;
        public StringField SessionId;
        public DateTimeField ActivityTime;
        public StringField Details;
    }
}