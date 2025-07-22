import { ServiceLookupEditor, LookupEditor, StringEditor, PrefixedContext, initFormType } from "@serenity-is/corelib";

export interface MovieCastForm {
    MovieId: ServiceLookupEditor;
    PersonId: LookupEditor;
    Character: StringEditor;
}

export class MovieCastForm extends PrefixedContext {
    static readonly formKey = 'Default.MovieCast';
    private static init: boolean;

    constructor(prefix: string) {
        super(prefix);

        if (!MovieCastForm.init)  {
            MovieCastForm.init = true;

            var w0 = ServiceLookupEditor;
            var w1 = LookupEditor;
            var w2 = StringEditor;

            initFormType(MovieCastForm, [
                'MovieId', w0,
                'PersonId', w1,
                'Character', w2
            ]);
        }
    }
}