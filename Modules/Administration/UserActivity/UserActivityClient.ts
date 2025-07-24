import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface OnlineUserInfo {
    userId: string;
    username: string;
    connectionTime: Date;
    lastActivity: Date;
    ipAddress: string;
    userAgent: string;
    location: string;
    isp: string;
    timezone: string;
}

export class UserActivityClient {
    private connection: HubConnection | null = null;
    private onUserConnectedCallbacks: ((user: OnlineUserInfo) => void)[] = [];
    private onUserDisconnectedCallbacks: ((user: OnlineUserInfo) => void)[] = [];
    private onOnlineCountChangedCallbacks: ((count: number) => void)[] = [];
    private onUserActivityUpdatedCallbacks: ((user: OnlineUserInfo) => void)[] = [];

    async start(): Promise<void> {
        if (this.connection) {
            return;
        }

        this.connection = new HubConnectionBuilder()
            .withUrl("/userActivityHub")
            .configureLogging(LogLevel.Information)
            .build();

        this.connection.on("UserConnected", (user: OnlineUserInfo) => {
            console.log('[UserActivityClient] User connected:', user);
            this.onUserConnectedCallbacks.forEach(callback => callback(user));
        });

        this.connection.on("UserDisconnected", (user: OnlineUserInfo) => {
            console.log('[UserActivityClient] User disconnected:', user);
            this.onUserDisconnectedCallbacks.forEach(callback => callback(user));
        });

        this.connection.on("OnlineUsersCountChanged", (count: number) => {
            console.log('[UserActivityClient] Online count changed:', count);
            this.onOnlineCountChangedCallbacks.forEach(callback => callback(count));
        });

        this.connection.on("UserActivityUpdated", (user: OnlineUserInfo) => {
            console.log('[UserActivityClient] User activity updated:', user);
            this.onUserActivityUpdatedCallbacks.forEach(callback => callback(user));
        });

        try {
            await this.connection.start();
            console.log("UserActivity SignalR connection started");
        } catch (err) {
            console.error("Error starting UserActivity SignalR connection:", err);
        }
    }

    async stop(): Promise<void> {
        if (this.connection) {
            await this.connection.stop();
            this.connection = null;
        }
    }

    onUserConnected(callback: (user: OnlineUserInfo) => void): void {
        this.onUserConnectedCallbacks.push(callback);
    }

    onUserDisconnected(callback: (user: OnlineUserInfo) => void): void {
        this.onUserDisconnectedCallbacks.push(callback);
    }

    onOnlineCountChanged(callback: (count: number) => void): void {
        this.onOnlineCountChangedCallbacks.push(callback);
    }

    onUserActivityUpdated(callback: (user: OnlineUserInfo) => void): void {
        this.onUserActivityUpdatedCallbacks.push(callback);
    }
}

// Global instance
export const userActivityClient = new UserActivityClient();