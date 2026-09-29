/**
 * Форматтер экстремально больших чисел для HelloTap
 * Поддерживает 33 порядка величин (до Dtg = 10^102) и плавный переход в экспоненциальную научную нотацию (1.23e105)
 */
const SUFFIXES: string[] = [
  "", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No",
  "Dc", "Ud", "Dd", "Td", "Qad", "Qid", "Sxd", "Spd", "Ocd", "Nod",
  "Vg", "Uvg", "Dvg", "Tvg", "Qavg", "Qivg", "Sxvg", "Spvg", "Ocvg", "Novg",
  "Tg", "Dtg"
];

export function formatNumber(value: number): string {
  if (value == null || isNaN(value)) return "0";
  if (!isFinite(value)) return "∞";

  if (value < 0) return "-" + formatNumber(-value);
  if (value < 1000) {
    return value < 10 ? value.toFixed(1).replace(/\.0$/, '') : Math.floor(value).toLocaleString();
  }

  const exponent = Math.floor(Math.log10(value));
  const tier = Math.floor(exponent / 3);

  if (tier >= SUFFIXES.length) {
    const mantissa = value / Math.pow(10, exponent);
    return `${mantissa.toFixed(2)}e${exponent}`;
  }

  const divisor = Math.pow(10, tier * 3);
  const formattedVal = value / divisor;
  const suffix = SUFFIXES[tier];

  if (formattedVal >= 100) {
    return `${formattedVal.toFixed(1)}${suffix}`;
  } else if (formattedVal >= 10) {
    return `${formattedVal.toFixed(2)}${suffix}`;
  } else {
    return `${formattedVal.toFixed(2)}${suffix}`;
  }
}
