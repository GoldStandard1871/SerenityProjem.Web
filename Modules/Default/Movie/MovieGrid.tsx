import { Decorators, EntityGrid,QuickSearchField } from '@serenity-is/corelib';
import { MovieColumns, MovieRow, MovieService } from '../../ServerTypes/Default';
import { MovieDialog } from './MovieDialog';

@Decorators.registerClass('SerenityProjem.Default.MovieGrid')
export class MovieGrid extends EntityGrid<MovieRow> {
    protected getColumnsKey() { return MovieColumns.columnsKey; }
    protected getDialogType() { return MovieDialog; }
    protected getRowDefinition() { return MovieRow; }
    protected getService() { return MovieService.baseUrl; }
    protected getQuickSearchFields(): QuickSearchField[] {
        const fld = MovieRow.Fields;
        return [
            { name: "", title: "All" },
            { name: fld.Description, title: "Description" },
            { name: fld.Storyline, title: "Storyline" },
            { name: fld.Year, title: "Year" }
        ];
    }
}