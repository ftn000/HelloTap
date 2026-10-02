export type ThemeId = 'cyberpunk' | 'monokai' | 'dracula' | 'matrix' | 'nordic' | 'tokyonight';

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
  // Syntax Highlighting Colors
  syntaxKeyword: string;
  syntaxType: string;
  syntaxFunction: string;
  syntaxString: string;
  syntaxNumber: string;
  syntaxComment: string;
  syntaxPunctuation: string;
}
