import { ServiceResponse } from "@serenity-is/corelib";

export interface LogActivityResponse extends ServiceResponse {
    Success?: boolean;
    Message?: string;
}