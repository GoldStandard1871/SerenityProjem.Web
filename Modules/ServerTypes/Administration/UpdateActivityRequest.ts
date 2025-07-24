import { ServiceRequest } from "@serenity-is/corelib";

export interface UpdateActivityRequest extends ServiceRequest {
    LastActivity?: string;
}