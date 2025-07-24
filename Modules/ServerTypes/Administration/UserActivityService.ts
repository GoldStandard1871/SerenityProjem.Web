import { ListRequest, ListResponse, ServiceOptions, ServiceRequest, serviceRequest } from "@serenity-is/corelib";
import { GetRecentActivityRequest } from "./GetRecentActivityRequest";
import { GetRecentActivityResponse } from "./GetRecentActivityResponse";
import { OnlineUserInfo } from "./OnlineUserInfo";
import { OnlineUsersCountResponse } from "./OnlineUsersCountResponse";
import { UpdateActivityRequest } from "./UpdateActivityRequest";
import { UpdateActivityResponse } from "./UpdateActivityResponse";

export namespace UserActivityService {
    export const baseUrl = 'Administration/UserActivity';

    export declare function GetOnlineUsers(request: ListRequest, onSuccess?: (response: ListResponse<OnlineUserInfo>) => void, opt?: ServiceOptions<any>): PromiseLike<ListResponse<OnlineUserInfo>>;
    export declare function GetOnlineUsersCount(request: ServiceRequest, onSuccess?: (response: OnlineUsersCountResponse) => void, opt?: ServiceOptions<any>): PromiseLike<OnlineUsersCountResponse>;
    export declare function UpdateActivity(request: UpdateActivityRequest, onSuccess?: (response: UpdateActivityResponse) => void, opt?: ServiceOptions<any>): PromiseLike<UpdateActivityResponse>;
    export declare function GetRecentActivity(request: GetRecentActivityRequest, onSuccess?: (response: GetRecentActivityResponse) => void, opt?: ServiceOptions<any>): PromiseLike<GetRecentActivityResponse>;

    export const Methods = {
        GetOnlineUsers: "Administration/UserActivity/GetOnlineUsers",
        GetOnlineUsersCount: "Administration/UserActivity/GetOnlineUsersCount",
        UpdateActivity: "Administration/UserActivity/UpdateActivity",
        GetRecentActivity: "Administration/UserActivity/GetRecentActivity"
    } as const;

    [
        'GetOnlineUsers', 
        'GetOnlineUsersCount', 
        'UpdateActivity', 
        'GetRecentActivity'
    ].forEach(x => {
        (<any>UserActivityService)[x] = function (r, s, o) {
            return serviceRequest(baseUrl + '/' + x, r, s, o);
        };
    });
}