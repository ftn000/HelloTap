using System;

/// <summary>
/// Утилита форматирования больших чисел для кликера/айдлера.
/// Преобразует числа вида 1500 -> "1.50K", 2300000 -> "2.30M",
/// расширенные суффиксы вплоть до 10^102 (Tg) и плавный переход к научной нотации (1.23e120).
/// </summary>
public static class NumberFormatter
{
    private static readonly string[] Suffixes = new string[]
    {
        "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", 
        "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod", 
        "Vg", "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg", 
        "Tg", "Utg", "Dtg"
    };

    public static string Format(double value)
    {
        if (double.IsNaN(value)) return "0";
        if (double.IsInfinity(value)) return "MAX";

        if (value < 0)
        {
            return "-" + Format(-value);
        }

        if (value < 1000)
        {
            return Math.Floor(value).ToString("F0");
        }

        int tier = (int)(Math.Log10(value) / 3);

        if (tier < Suffixes.Length)
        {
            double scaled = value / Math.Pow(10, tier * 3);
            return $"{scaled:F2} {Suffixes[tier]}";
        }

        // Если число превышает список суффиксов (> 10^102), переходим в научную экспоненциальную нотацию
        int exponent = (int)Math.Floor(Math.Log10(value));
        double mantissa = value / Math.Pow(10, exponent);
        return $"{mantissa:F2}e{exponent}";
    }

    public static string FormatCompact(double value)
    {
        if (double.IsNaN(value)) return "0";
        if (double.IsInfinity(value)) return "MAX";

        if (value < 1000) return Math.Floor(value).ToString("F0");
        return Format(value);
    }

    public static string FormatTime(float seconds)
    {
        TimeSpan span = TimeSpan.FromSeconds(Math.Max(0, seconds));
        if (span.TotalHours >= 1)
        {
            return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }
        return $"{span.Minutes:D2}:{span.Seconds:D2}";
    }
}
