import { ListRequest, ListResponse, ServiceOptions, ServiceRequest, serviceRequest } from "@serenity-is/corelib";
import { OnlineUserInfo } from "./OnlineUserInfo";
import { OnlineUsersCountResponse } from "./OnlineUsersCountResponse";

export namespace UserActivityService {
    export const baseUrl = 'Administration/UserActivity';

    export declare function GetOnlineUsers(request: ListRequest, onSuccess?: (response: ListResponse<OnlineUserInfo>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<OnlineUserInfo>>;
    export declare function GetOnlineUsersCount(request: ServiceRequest, onSuccess?: (response: OnlineUsersCountResponse) => void, opt?: ServiceOptions<any>): PromiseLike<OnlineUsersCountResponse>;

    export const Methods = {
        GetOnlineUsers: "Administration/UserActivity/GetOnlineUsers",
        GetOnlineUsersCount: "Administration/UserActivity/GetOnlineUsersCount"
    } as const;

    [
        'GetOnlineUsers', 
        'GetOnlineUsersCount'
    ].forEach(x => {
        (<any>UserActivityService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}