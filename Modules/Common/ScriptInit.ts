import { Config, ErrorHandling, HtmlContentEditor, TranslationConfig, getjQuery } from "@serenity-is/corelib";
import { gridDefaults } from "@serenity-is/sleekgrid";
import { getLanguageList } from "./Helpers/LanguageList";
import { userActivityClient } from "../Administration/UserActivity/UserActivityClient";

Config.rootNamespaces.push('SerenityProjem');
TranslationConfig.getLanguageList = getLanguageList;
HtmlContentEditor.CKEditorBasePath = "~/Serenity.Assets/Scripts/ckeditor/";
gridDefaults.useCssVars = false;

let $ = getjQuery();
if ($?.fn?.colorbox) {
    $.fn.colorbox.settings.maxWidth = "95%";
    $.fn.colorbox.settings.maxHeight = "95%";
}

window.onerror = ErrorHandling.runtimeErrorHandler;
window.addEventListener('unhandledrejection', ErrorHandling.unhandledRejectionHandler);

// Start UserActivity SignalR connection for authenticated users
$(document).ready(function() {
    const username = document.querySelector('meta[name="username"]')?.getAttribute('content');
    if (username && username.trim() !== '') {
        userActivityClient.start().catch(err => {
            console.error('Failed to start UserActivity SignalR connection:', err);
        });
    }
});