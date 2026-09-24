using System;

/// <summary>
/// Утилита форматирования больших чисел для кликера/айдлера.
/// Преобразует числа вида 1500 -> "1.50K", 2300000 -> "2.30M" и т.д.
/// </summary>
public static class NumberFormatter
{
    private static readonly string[] Suffixes = new string[]
    {
        "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc"
    };

    public static string Format(double value)
    {
        if (value < 0)
        {
            return "-" + Format(-value);
        }

        if (value < 1000)
        {
            return Math.Floor(value).ToString("F0");
        }

        int tier = (int)(Math.Log10(value) / 3);

        if (tier >= Suffixes.Length)
        {
            tier = Suffixes.Length - 1;
        }

        double scaled = value / Math.Pow(10, tier * 3);
        return $"{scaled:F2} {Suffixes[tier]}";
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
