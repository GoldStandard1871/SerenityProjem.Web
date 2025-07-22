import { Decorators, EntityDialog } from '@serenity-is/corelib';
import { GenreForm, GenreRow, GenreService } from '../../ServerTypes/Default';

@Decorators.registerClass('SerenityProjem.Default.GenreDialog')
export class GenreDialog extends EntityDialog<GenreRow, any> {
    protected getFormKey() { return GenreForm.formKey; }
    protected getRowDefinition() { return GenreRow; }
    protected getService() { return GenreService.baseUrl; }

    protected form = new GenreForm(this.idPrefix);
}