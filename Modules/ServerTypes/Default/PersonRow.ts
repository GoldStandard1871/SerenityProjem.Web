import { getLookup, getLookupAsync, fieldsProxy } from "@serenity-is/corelib";
import { Gender } from "./Gender";
import { MovieCastRow } from "./MovieCastRow";

export interface PersonRow {
    PersonId?: number;
    FirstName?: string;
    LastName?: string;
    BirthDate?: string;
    BirthPlace?: string;
    Gender?: Gender;
    Height?: number;
    FullName?: string;
    MoviesGrid?: MovieCastRow[];
    PrimaryImage?: string;
    GalleryImages?: string;
}

export abstract class PersonRow {
    static readonly idProperty = 'PersonId';
    static readonly nameProperty = 'FullName';
    static readonly localTextPrefix = 'Default.Person';
    static readonly lookupKey = 'Default.Person';

    /** @deprecated use getLookupAsync instead */
    static getLookup() { return getLookup<PersonRow>('Default.Person') }
    static async getLookupAsync() { return getLookupAsync<PersonRow>('Default.Person') }

    static readonly deletePermission = 'Administration:General';
    static readonly insertPermission = 'Administration:General';
    static readonly readPermission = 'Administration:General';
    static readonly updatePermission = 'Administration:General';

    static readonly Fields = fieldsProxy<PersonRow>();
}