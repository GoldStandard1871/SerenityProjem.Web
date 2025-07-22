import { Decorators, Formatter, Lookup } from "@serenity-is/corelib";
import { FormatterContext } from "@serenity-is/sleekgrid";
import { GenreRow } from "../../ServerTypes/Default/GenreRow";


let lookup: Lookup<GenreRow>;
let promise: Promise<Lookup<GenreRow>>;

@Decorators.registerFormatter('SerenityProjem.GenreListFormatter')
export class GenreListFormatter implements Formatter {

    format(ctx: FormatterContext) {

        let idList = ctx.value as number[];
        if (!idList || !idList.length)
            return "";

        let byId = lookup?.itemById;
        if (byId) {
            return idList.map(id => {
                var genre = byId[id];
                return ctx.escape(genre == null ? id : genre.Name);
            }).join(", ");
        }

        promise ??= GenreRow.getLookupAsync().then(l => {
            lookup = l;
            try {
                ctx.grid?.invalidate();
            }
            finally {
                lookup = null;
                promise = null;
            }
        }).catch(() => promise = null);

        return <i class="fa fa-spinner"></i>;
    }
}