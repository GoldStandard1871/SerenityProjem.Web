import { StringEditor, DateEditor, TextAreaEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface UserActivityHistoryForm {
    UserId: StringEditor;
    Username: StringEditor;
    ActivityType: StringEditor;
    ActivityTime: DateEditor;
    IpAddress: StringEditor;
    Location: StringEditor;
    Isp: StringEditor;
    Timezone: StringEditor;
    UserAgent: TextAreaEditor;
    SessionId: StringEditor;
    Details: TextAreaEditor;
}

export class UserActivityHistoryForm extends PrefixedContext {
    static readonly formKey = 'Administration.UserActivityHistory';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!UserActivityHistoryForm.init)  {
            UserActivityHistoryForm.init = true;

            var w0 = StringEditor;
            var w1 = DateEditor;
            var w2 = TextAreaEditor;

            initFormType(UserActivityHistoryForm, [
                'UserId', w0,
                'Username', w0,
                'ActivityType', w0,
                'ActivityTime', w1,
                'IpAddress', w0,
                'Location', w0,
                'Isp', w0,
                'Timezone', w0,
                'UserAgent', w2,
                'SessionId', w0,
                'Details', w2
            ]);
        }
    }
}