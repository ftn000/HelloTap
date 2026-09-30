export type ThemeId = 'cyberpunk' | 'monokai' | 'dracula' | 'matrix';

export interface IdeTheme {
  id: ThemeId;
  name: string;
  tagline: string;
  icon: string;
  terminalBg: string;
  terminalBorder: string;
  terminalGlow: string;
  codeColor: string;
  promptColor: string;
  btnBg: string;
  btnBorder: string;
  btnText: string;
  btnShadow: string;
  btnActiveBg: string;
  popupColor: string;
  crtScanline?: boolean;
}
