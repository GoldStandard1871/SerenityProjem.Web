import { ServiceRequest } from "@serenity-is/corelib";

export interface GetRecentActivityRequest extends ServiceRequest {
    Take?: number;
}