import { serviceCall } from "@serenity-is/corelib";

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

export class UserActivityGlobal {
    private static hubConnection: any = null;
    private static onlineCountCallbacks: ((count: number) => void)[] = [];
    private static activityTimer: number | null = null;
    private static lastActivityTime: Date = new Date();

    static async initializeSignalR(): Promise<void> {
        try {
            // For now, we'll just use AJAX polling instead of SignalR to avoid module loading issues
            console.log("[UserActivityGlobal] Initialized (using AJAX polling instead of SignalR for now)");
            this.startActivityTracking();
        } catch (err) {
            console.error("[UserActivityGlobal] Failed to initialize:", err);
        }
    }

    private static startActivityTracking(): void {
        // Track user interactions
        document.addEventListener('click', () => this.recordActivity());
        document.addEventListener('keypress', () => this.recordActivity());
        document.addEventListener('scroll', () => this.recordActivity());
        document.addEventListener('mousemove', () => this.recordActivity());

        // Send heartbeat every 30 seconds
        this.activityTimer = window.setInterval(() => {
            this.sendHeartbeat();
        }, 30000);

        console.log("[UserActivityGlobal] Activity tracking started");
    }

    private static recordActivity(): void {
        this.lastActivityTime = new Date();
    }

    private static async sendHeartbeat(): Promise<void> {
        try {
            await serviceCall({
                service: 'Administration/UserActivity/UpdateActivity',
                request: {
                    lastActivity: this.lastActivityTime.toISOString()
                }
            }) as any;
            console.log("[UserActivityGlobal] Heartbeat sent");
        } catch (err) {
            console.error("[UserActivityGlobal] Error sending heartbeat:", err);
        }
    }

    static async getOnlineUsersCount(): Promise<number> {
        try {
            const response = await serviceCall({
                service: 'Administration/UserActivity/GetOnlineUsersCount',
                request: {}
            }) as any;
            console.log("[UserActivityGlobal] Online users count response:", response);
            return response.Count || 0;
        } catch (err) {
            console.error("[UserActivityGlobal] Error getting online users count:", err);
            return 0;
        }
    }

    static onOnlineCountChanged(callback: (count: number) => void): void {
        this.onlineCountCallbacks.push(callback);
    }

    static notifyCountChanged(count: number): void {
        this.onlineCountCallbacks.forEach(callback => callback(count));
    }
}

// Make it globally available
(window as any).UserActivityGlobal = UserActivityGlobal;