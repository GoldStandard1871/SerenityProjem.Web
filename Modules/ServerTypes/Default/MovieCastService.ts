import { SaveRequest, SaveResponse, ServiceOptions, DeleteRequest, DeleteResponse, RetrieveRequest, RetrieveResponse, ListRequest, ListResponse, serviceRequest } from "@serenity-is/corelib";
import { MovieCastRow } from "./MovieCastRow";

export namespace MovieCastService {
    export const baseUrl = 'Default/MovieCast';

    export declare function Create(request: SaveRequest<MovieCastRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Update(request: SaveRequest<MovieCastRow>, onSuccess?: (response: SaveResponse) => void, opt?: ServiceOptions<any>): PromiseLike<SaveResponse>;
    export declare function Delete(request: DeleteRequest, onSuccess?: (response: DeleteResponse) => void, opt?: ServiceOptions<any>): PromiseLike<DeleteResponse>;
    export declare function Retrieve(request: RetrieveRequest, onSuccess?: (response: RetrieveResponse<MovieCastRow>) => void, opt?: ServiceOptions<any>): PromiseLike<RetrieveResponse<MovieCastRow>>;
    export declare function List(request: ListRequest, onSuccess?: (response: ListResponse<MovieCastRow>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<MovieCastRow>>;

    export const Methods = {
        Create: "Default/MovieCast/Create",
        Update: "Default/MovieCast/Update",
        Delete: "Default/MovieCast/Delete",
        Retrieve: "Default/MovieCast/Retrieve",
        List: "Default/MovieCast/List"
    } as const;

    [
        'Create', 
        'Update', 
        'Delete', 
        'Retrieve', 
        'List'
    ].forEach(x => {
        (<any>MovieCastService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}