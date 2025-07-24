import { GridEditorDialog } from '@serenity-is/extensions';
import { MovieCastForm, MovieCastRow } from '../../ServerTypes/Default';

export class MovieCastDialog extends GridEditorDialog<MovieCastRow, any> {
    protected getFormKey() { return MovieCastForm.formKey; }
    protected getLocalTextPrefix() { return MovieCastRow.localTextPrefix; }
    
    protected getIdProperty() { return MovieCastRow.idProperty; } // MovieCastId kullan
    protected getService() { return MovieCastRow.service; }
    
    protected afterLoadEntity() {
        super.afterLoadEntity();
        
        // Debug için console log
        console.log('MovieCast dialog loaded entity:', this.entity);
    }
    
    // Client-side validation'ı kaldırıp server-side validation'a güvenelim
    // Form Required attribute'ları zaten mevcut, server'da da validation var
}
