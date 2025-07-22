import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { MovieCastRow } from "./MovieCastRow";

export interface MovieCastColumns {
    MovieCastId: Column<MovieCastRow>;
    MovieTitle: Column<MovieCastRow>;
    PersonFullName: Column<MovieCastRow>;
    Character: Column<MovieCastRow>;
}

export class MovieCastColumns extends ColumnsBase<MovieCastRow> {
    static readonly columnsKey = 'Default.MovieCast';
    static readonly Fields = fieldsProxy<MovieCastColumns>();
}