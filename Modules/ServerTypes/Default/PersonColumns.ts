import { ColumnsBase, fieldsProxy } from "@serenity-is/corelib";
import { Column } from "@serenity-is/sleekgrid";
import { Gender } from "./Gender";
import { PersonRow } from "./PersonRow";

export interface PersonColumns {
    PersonId: Column<PersonRow>;
    FirstName: Column<PersonRow>;
    LastName: Column<PersonRow>;
    BirthDate: Column<PersonRow>;
    BirthPlace: Column<PersonRow>;
    Gender: Column<PersonRow>;
    Height: Column<PersonRow>;
}

export class PersonColumns extends ColumnsBase<PersonRow> {
    static readonly columnsKey = 'Default.Person';
    static readonly Fields = fieldsProxy<PersonColumns>();
}

[Gender]; // referenced types