import { Decorators, EntityDialog } from '@serenity-is/corelib';
import { MovieForm, MovieRow, MovieService } from '../../ServerTypes/Default';

@Decorators.registerClass('SerenityProjem.Default.MovieDialog')
export class MovieDialog extends EntityDialog<MovieRow, any> {
    protected getFormKey() { return MovieForm.formKey; }
    protected getRowDefinition() { return MovieRow; }
    protected getService() { return MovieService.baseUrl; }

    protected form = new MovieForm(this.idPrefix);
    
    protected afterLoadEntity() {
        super.afterLoadEntity();
        
        // MovieCast editor'ındaki verileri kontrol et
        if (this.form.CastList && this.entityId) {
            console.log('Movie dialog loaded, entity ID:', this.entityId);
            console.log('Movie dialog cast list:', this.form.CastList.getItems());
        }
    }
}