import { fieldsProxy } from "@serenity-is/corelib";

export interface UserActivityHistoryRow {
    Id?: number;
    UserId?: string;
    Username?: string;
    ActivityType?: string;
    IpAddress?: string;
    UserAgent?: string;
    Location?: string;
    Isp?: string;
    Timezone?: string;
    SessionId?: string;
    ActivityTime?: string;
    Details?: string;
}

export abstract class UserActivityHistoryRow {
    static readonly idProperty = 'Id';
    static readonly nameProperty = 'Username';
    static readonly localTextPrefix = 'Administration.UserActivityHistory';
    static readonly deletePermission = 'Administration:General';
    static readonly insertPermission = 'Administration:General';
    static readonly readPermission = 'Administration:General';
    static readonly updatePermission = 'Administration:General';

    static readonly Fields = fieldsProxy<UserActivityHistoryRow>();
}