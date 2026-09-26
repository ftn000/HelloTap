mergeInto(LibraryManager.library, {
    TriggerHapticVibrate: function(durationMs) {
        try {
            if (typeof window !== "undefined" && window.navigator && window.navigator.vibrate) {
                window.navigator.vibrate(durationMs);
            }
        } catch (e) {
            // Ignore if vibration is disallowed or unsupported
        }
    }
});
