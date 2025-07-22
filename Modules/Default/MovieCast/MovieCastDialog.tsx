import { GridEditorDialog } from '@serenity-is/extensions';
import { MovieCastForm, MovieCastRow } from '../../ServerTypes/Default';


export class MovieCastDialog extends GridEditorDialog<MovieCastRow, any> {
    protected getFormKey() { return MovieCastForm.formKey; }
    protected getLocalTextPrefix() { return MovieCastRow.localTextPrefix; }
}
