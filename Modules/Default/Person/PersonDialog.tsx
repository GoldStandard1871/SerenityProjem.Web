import { Decorators, EntityDialog } from '@serenity-is/corelib';
import { PersonForm, PersonRow, PersonService } from '../../ServerTypes/Default';

@Decorators.registerClass('SerenityProjem.Default.PersonDialog')
export class PersonDialog extends EntityDialog<PersonRow, any> {
    protected getFormKey() { return PersonForm.formKey; }
    protected getRowDefinition() { return PersonRow; }
    protected getService() { return PersonService.baseUrl; }

    protected form = new PersonForm(this.idPrefix);
}