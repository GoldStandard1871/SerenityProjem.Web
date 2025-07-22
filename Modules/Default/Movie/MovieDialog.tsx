import { Decorators, EntityDialog } from '@serenity-is/corelib';
import { MovieForm, MovieRow, MovieService } from '../../ServerTypes/Default';

@Decorators.registerClass('SerenityProjem.Default.MovieDialog')
export class MovieDialog extends EntityDialog<MovieRow, any> {
    protected getFormKey() { return MovieForm.formKey; }
    protected getRowDefinition() { return MovieRow; }
    protected getService() { return MovieService.baseUrl; }

    protected form = new MovieForm(this.idPrefix);
}