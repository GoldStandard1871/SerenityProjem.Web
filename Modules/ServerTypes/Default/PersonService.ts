import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { PersonRow } from "./PersonRow";

export namespace PersonService {
    export const baseUrl = 'Default/Person';

    export declare function Create(request: SaveRequest<PersonRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<PersonRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<PersonRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<PersonRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<PersonRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<PersonRow>>;

    export const Methods = {
        Create: "Default/Person/Create",
        Update: "Default/Person/Update",
        Delete: "Default/Person/Delete",
        Retrieve: "Default/Person/Retrieve",
        List: "Default/Person/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>PersonService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}