import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { UserActivityHistoryRow } from "./UserActivityHistoryRow";

export interface UserActivityHistoryColumns {
    Id: Column<UserActivityHistoryRow>;
    Username: Column<UserActivityHistoryRow>;
    ActivityType: Column<UserActivityHistoryRow>;
    IpAddress: Column<UserActivityHistoryRow>;
    Location: Column<UserActivityHistoryRow>;
    Isp: Column<UserActivityHistoryRow>;
    Timezone: Column<UserActivityHistoryRow>;
    ActivityTime: Column<UserActivityHistoryRow>;
    Details: Column<UserActivityHistoryRow>;
}

export class UserActivityHistoryColumns extends ColumnsBase<UserActivityHistoryRow> {
    static readonly columnsKey = 'Administration.UserActivityHistory';
    static readonly Fields = fieldsProxy<UserActivityHistoryColumns>();
}