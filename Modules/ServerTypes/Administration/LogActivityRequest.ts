import { ServiceRequest } from "@serenity-is/corelib";

export interface LogActivityRequest extends ServiceRequest {
    ActivityType?: string;
    Details?: string;
    Timestamp?: string;
}