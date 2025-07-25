var SerenityProjem;
(function (SerenityProjem) {
    var UserActivityTracking;
    (function (UserActivityTracking) {
        var connection = null;
        var lastPageView = '';
        var pageViewDebounceTimer = null;
        
        function initialize() {
            // Only initialize if user is authenticated
            if (!Q.Authorization.isLoggedIn) {
                return;
            }

            console.log('[ActivityTracking] Initializing user activity tracking...');
            
            // Initialize SignalR connection
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/userActivityHub")
                .withAutomaticReconnect()
                .build();

            connection.on("ReceiveActivity", function (activity) {
                console.log('[ActivityTracking] Activity received:', activity);
            });

            connection.start()
                .then(function () {
                    console.log('[ActivityTracking] Connected to UserActivityHub');
                    trackPageView();
                })
                .catch(function (err) {
                    console.error('[ActivityTracking] Error connecting:', err);
                });

            // Track page changes
            $(document).on('panelactivate', '.s-Panel', function (e) {
                var panel = $(e.target);
                var title = panel.find('.panel-titlebar h5').text() || document.title;
                trackPageView(title);
            });

            // Track when navigating between pages
            window.addEventListener('popstate', function () {
                trackPageView();
            });

            // Track activity periodically (heartbeat)
            setInterval(function () {
                if (connection && connection.state === signalR.HubConnectionState.Connected) {
                    sendActivity('Heartbeat', null);
                }
            }, 300000); // Every 5 minutes
        }

        function trackPageView(pageTitle) {
            if (!Q.Authorization.isLoggedIn) return;

            pageTitle = pageTitle || getCurrentPageTitle();
            
            // Avoid duplicate tracking of the same page
            if (pageTitle === lastPageView) {
                return;
            }
            
            lastPageView = pageTitle;
            
            // Debounce page view tracking
            if (pageViewDebounceTimer) {
                clearTimeout(pageViewDebounceTimer);
            }
            
            pageViewDebounceTimer = setTimeout(function() {
                console.log('[ActivityTracking] Tracking page view:', pageTitle);
                sendActivity('PageView', pageTitle);
            }, 500);
        }

        function getCurrentPageTitle() {
            // Try to get title from various sources
            var title = '';
            
            // Check active panel
            var activePanel = $('.s-Panel:visible .panel-titlebar h5').first().text();
            if (activePanel) {
                title = activePanel;
            }
            // Check page title
            else if ($('.page-title').length) {
                title = $('.page-title').text();
            }
            // Check section header
            else if ($('.section-header h1').length) {
                title = $('.section-header h1').text().replace(/^\s*<i[^>]*><\/i>\s*/, '');
            }
            // Use URL path
            else {
                var path = window.location.pathname;
                var parts = path.split('/').filter(function(p) { return p; });
                if (parts.length > 0) {
                    title = parts[parts.length - 1];
                    // Convert from URL format to readable format
                    title = title.replace(/([A-Z])/g, ' $1').trim();
                }
            }
            
            return title || 'Unknown Page';
        }

        function sendActivity(activityType, details) {
            // Always try to send activity, even if SignalR is not connected
            console.log('[ActivityTracking] Attempting to send activity:', activityType, details);

            var activityData = {
                ActivityType: activityType,
                Details: details,
                Timestamp: new Date().toISOString()
            };

            // Send via AJAX to ensure it's logged in database
            $.ajax({
                url: Q.resolveUrl('~/Services/Administration/UserActivity/LogActivity'),
                type: 'POST',
                data: JSON.stringify(activityData),
                contentType: 'application/json',
                success: function(response) {
                    console.log('[ActivityTracking] Activity logged successfully:', activityType, details);
                },
                error: function(xhr, status, error) {
                    console.error('[ActivityTracking] Failed to log activity:', error);
                    console.error('[ActivityTracking] Response:', xhr.responseText);
                }
            });
        }

        // Public API
        UserActivityTracking.trackPageView = trackPageView;
        UserActivityTracking.sendActivity = sendActivity;
        UserActivityTracking.initialize = initialize;

    })(UserActivityTracking = SerenityProjem.UserActivityTracking || (SerenityProjem.UserActivityTracking = {}));
})(SerenityProjem || (SerenityProjem = {}));

// Initialize when document is ready
$(function () {
    SerenityProjem.UserActivityTracking.initialize();
});