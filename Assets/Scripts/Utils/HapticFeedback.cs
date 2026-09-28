using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Обеспечивает тактильный отклик (Haptic Feedback / Vibration) при нажатиях:
/// В WebGL вызывает navigator.vibrate через нативный JS-плагин с дифференцированными паттернами,
/// а на мобильных платформах использует встроенный API.
/// </summary>
public static class HapticFeedback
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void TriggerHapticVibrate(int durationMs);

    [DllImport("__Internal")]
    private static extern void TriggerHapticPattern(int patternType);
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

    /// <summary>
    /// Лёгкий тактильный клик при стандартном тапе по клавиатуре или кнопке (15мс)
    /// </summary>
    public static void LightImpact()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(0); } catch { }
#else
        Vibrate(15);
#endif
    }

    /// <summary>
    /// Средний тактильный импульс при покупке улучшения или смене вкладки (35мс)
    /// </summary>
    public static void MediumImpact()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(1); } catch { }
#else
        Vibrate(35);
#endif
    }

    /// <summary>
    /// Мощный тактильный удар при критическом клике, джекпоте или завершении квеста (75мс)
    /// </summary>
    public static void HeavyImpact()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(2); } catch { }
#else
        Vibrate(75);
#endif
    }

    /// <summary>
    /// Торжественный двойной/тройной паттерн при завершении игры, получении ачивки или IPO
    /// </summary>
    public static void SuccessPattern()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(3); } catch { }
#else
        Vibrate(70);
#endif
    }

    /// <summary>
    /// Забавный вибро-паттерн "Кря-кря" при поглаживании резиновой уточки
    /// </summary>
    public static void DuckQuackHaptic()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(4); } catch { }
#else
        Vibrate(50);
#endif
    }

    /// <summary>
    /// Предупредительный вибро-паттерн (ошибка, нехватка средств)
    /// </summary>
    public static void WarningHaptic()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try { TriggerHapticPattern(5); } catch { }
#else
        Vibrate(40);
#endif
    }
}
