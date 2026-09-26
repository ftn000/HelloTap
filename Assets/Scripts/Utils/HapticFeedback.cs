using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Обеспечивает тактильный отклик (Haptic Feedback / Vibration) при нажатиях:
/// В WebGL вызывает navigator.vibrate через нативный JS-плагин,
/// а на мобильных платформах использует встроенный API.
/// </summary>
public static class HapticFeedback
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TriggerHapticVibrate(int durationMs);
#endif

    public static void Vibrate(int ms = 15)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            TriggerHapticVibrate(ms);
        }
        catch
        {
            // Silently ignore if WebGL vibration is blocked by browser policies
        }
#elif UNITY_ANDROID || UNITY_IOS
        if (SystemInfo.supportsVibration)
        {
            Handheld.Vibrate();
        }
#endif
    }
}
