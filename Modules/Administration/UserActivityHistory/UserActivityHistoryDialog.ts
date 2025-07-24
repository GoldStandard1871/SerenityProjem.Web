import { Decorators, EntityDialog } from '@serenity-is/corelib';
import { UserActivityHistoryForm, UserActivityHistoryRow, UserActivityHistoryService } from '../../ServerTypes/Administration';

@Decorators.registerClass('SerenityProjem.Administration.UserActivityHistoryDialog')
export class UserActivityHistoryDialog extends EntityDialog<UserActivityHistoryRow, any> {
    protected getFormKey() { return UserActivityHistoryForm.formKey; }
    protected getRowDefinition() { return UserActivityHistoryRow; }
    protected getService() { return UserActivityHistoryService.baseUrl; }

    protected form = new UserActivityHistoryForm(this.idPrefix);

    constructor() {
        super();
    }

    protected getToolbarButtons() {
        let buttons = super.getToolbarButtons();
        
        // Remove save and delete buttons since this is read-only
        buttons = buttons.filter(x => x.cssClass !== 'save-and-close-button' && 
                                     x.cssClass !== 'apply-changes-button' && 
                                     x.cssClass !== 'delete-button');
        
        return buttons;
    }

    protected updateInterface() {
        super.updateInterface();
        
        // Make all fields readonly
        this.form.UserId.readOnly = true;
        this.form.Username.readOnly = true;
        this.form.ActivityType.readOnly = true;
        this.form.IpAddress.readOnly = true;
        this.form.UserAgent.readOnly = true;
        this.form.Location.readOnly = true;
        this.form.Isp.readOnly = true;
        this.form.Timezone.readOnly = true;
        this.form.SessionId.readOnly = true;
        this.form.ActivityTime.readOnly = true;
        this.form.Details.readOnly = true;
    }
}