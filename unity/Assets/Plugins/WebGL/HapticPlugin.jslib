mergeInto(LibraryManager.library, {
    TriggerHapticVibrate: function(durationMs) {
        try {
            if (typeof window !== "undefined" && window.navigator && window.navigator.vibrate) {
                window.navigator.vibrate(durationMs);
            }
        } catch (e) {
            // Ignore if vibration is disallowed or unsupported
        }
    },

    TriggerHapticPattern: function(patternType) {
        try {
            if (typeof window === "undefined" || !window.navigator || !window.navigator.vibrate) return;
            switch (patternType) {
                case 0: // Light impact (15ms)
                    window.navigator.vibrate(15);
                    break;
                case 1: // Medium impact (35ms)
                    window.navigator.vibrate(35);
                    break;
                case 2: // Heavy impact (75ms)
                    window.navigator.vibrate(75);
                    break;
                case 3: // Success pattern
                    window.navigator.vibrate([30, 40, 70]);
                    break;
                case 4: // Rubber duck quack pattern
                    window.navigator.vibrate([20, 30, 40, 30, 60]);
                    break;
                case 5: // Error / warning pattern
                    window.navigator.vibrate([50, 40, 50]);
                    break;
                default:
                    window.navigator.vibrate(25);
                    break;
            }
        } catch (e) {
            // Silently ignore browser vibration policy rejections
        }
    }
});
