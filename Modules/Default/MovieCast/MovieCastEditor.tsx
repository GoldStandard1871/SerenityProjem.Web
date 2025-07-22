import { Decorators, WidgetProps } from '@serenity-is/corelib';
import { GridEditorBase } from '@serenity-is/extensions';
import { MovieCastColumns, MovieCastRow } from '../../ServerTypes/Default';
import { MovieCastDialog } from './MovieCastDialog'; // Bu import eksikti

@Decorators.registerEditor("SerenityProjem.Default.MovieCastEditor")
export class MovieCastEditor<P = {}> extends GridEditorBase<MovieCastRow, P> {
    protected getColumnsKey() { return MovieCastColumns.columnsKey; }

    protected getLocalTextPrefix() { return MovieCastRow.localTextPrefix; }

    protected getDialogType() { return MovieCastDialog; } // Bu fonksiyon eksikti!

    constructor(props: WidgetProps<P>) {
        super(props);
    }
}
