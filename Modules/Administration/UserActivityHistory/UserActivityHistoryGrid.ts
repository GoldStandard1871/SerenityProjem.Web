import { Decorators, EntityGrid } from '@serenity-is/corelib';
import { UserActivityHistoryColumns, UserActivityHistoryRow, UserActivityHistoryService } from '../../ServerTypes/Administration';

@Decorators.registerClass('SerenityProjem.Administration.UserActivityHistoryGrid')
export class UserActivityHistoryGrid extends EntityGrid<UserActivityHistoryRow, any> {
    protected getColumnsKey() { return UserActivityHistoryColumns.columnsKey; }
    protected getDialogType() { return <any>'SerenityProjem.Administration.UserActivityHistoryDialog'; }
    protected getRowDefinition() { return UserActivityHistoryRow; }
    protected getService() { return UserActivityHistoryService.baseUrl; }

    constructor(container: JQuery) {
        super(container);
    }

    protected getDefaultSortBy() {
        return [UserActivityHistoryRow.Fields.ActivityTime, true]; // Sort by ActivityTime descending
    }

    protected getButtons() {
        var buttons = super.getButtons();
        
        // Remove Add button since we don't want manual creation
        buttons = buttons.filter(x => x.cssClass !== 'add-button');
        
        return buttons;
    }

    protected getItemCssClass(item: UserActivityHistoryRow, index: number): string {
        let cssClass = super.getItemCssClass(item, index);
        
        if (item.ActivityType === 'Login') {
            cssClass += ' text-success';
        } else if (item.ActivityType === 'Logout') {
            cssClass += ' text-danger';
        } else if (item.ActivityType === 'Activity' || item.ActivityType === 'Heartbeat') {
            cssClass += ' text-info';
        }
        
        return cssClass;
    }
}