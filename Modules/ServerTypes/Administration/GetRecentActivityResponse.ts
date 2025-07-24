import { ServiceResponse } from "@serenity-is/corelib";
import { ActivityHistoryItem } from "./ActivityHistoryItem";

export interface GetRecentActivityResponse extends ServiceResponse {
    Activities?: ActivityHistoryItem[];
}