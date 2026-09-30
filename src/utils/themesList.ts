import { IdeTheme } from '../types/themes';

export const IDE_THEMES: Record<string, IdeTheme> = {
  cyberpunk: {
    id: 'cyberpunk',
    name: 'Cyberpunk Neon',
    tagline: 'Неоновый бирюзово-фиолетовый кибер-стиль будущего',
    icon: '⚡',
    terminalBg: 'from-slate-900/95 via-purple-950/20 to-slate-950/95',
    terminalBorder: 'border-cyan-500/40 hover:border-cyan-400/70',
    terminalGlow: 'rgba(6, 182, 212, 0.35)',
    codeColor: 'text-cyan-400',
    promptColor: 'text-purple-400',
    btnBg: 'bg-slate-800/80 hover:bg-slate-800',
    btnBorder: 'border-cyan-500/50',
    btnText: 'text-cyan-300',
    btnShadow: 'shadow-[0_4px_0_#0891b2]',
    btnActiveBg: 'bg-cyan-500 text-slate-950 border-cyan-300',
    popupColor: 'text-cyan-300',
    crtScanline: false
  },
  monokai: {
    id: 'monokai',
    name: 'Monokai Pro',
    tagline: 'Легендарная палитра Sublime Text: янтарь, лайм и сочный кокос',
    icon: '🎨',
    terminalBg: 'from-[#272822]/95 via-[#1e1f1c]/95 to-[#191917]/95',
    terminalBorder: 'border-[#a6e22e]/40 hover:border-[#a6e22e]/70',
    terminalGlow: 'rgba(166, 226, 46, 0.35)',
    codeColor: 'text-[#66d9ef]',
    promptColor: 'text-[#fd971f]',
    btnBg: 'bg-[#3e3d32] hover:bg-[#49483e]',
    btnBorder: 'border-[#a6e22e]/60',
    btnText: 'text-[#a6e22e]',
    btnShadow: 'shadow-[0_4px_0_#587919]',
    btnActiveBg: 'bg-[#a6e22e] text-[#272822] border-[#e6db74]',
    popupColor: 'text-[#e6db74]',
    crtScanline: false
  },
  dracula: {
    id: 'dracula',
    name: 'Dracula Dark',
    tagline: 'Классическая темная вампирская тема: пурпур, розовый и циан',
    icon: '🧛',
    terminalBg: 'from-[#282a36]/95 via-[#1e1f29]/95 to-[#16171e]/95',
    terminalBorder: 'border-[#bd93f9]/40 hover:border-[#bd93f9]/70',
    terminalGlow: 'rgba(189, 147, 249, 0.35)',
    codeColor: 'text-[#ff79c6]',
    promptColor: 'text-[#50fa7b]',
    btnBg: 'bg-[#44475a] hover:bg-[#52556b]',
    btnBorder: 'border-[#bd93f9]/60',
    btnText: 'text-[#bd93f9]',
    btnShadow: 'shadow-[0_4px_0_#6272a4]',
    btnActiveBg: 'bg-[#bd93f9] text-[#282a36] border-[#ff79c6]',
    popupColor: 'text-[#50fa7b]',
    crtScanline: false
  },
  matrix: {
    id: 'matrix',
    name: 'Retro Matrix CRT',
    tagline: 'Зеленый монохромный люминофор лампового терминала 1980-х',
    icon: '📟',
    terminalBg: 'from-black via-[#001400]/95 to-black',
    terminalBorder: 'border-emerald-500/50 hover:border-emerald-400',
    terminalGlow: 'rgba(16, 185, 129, 0.45)',
    codeColor: 'text-emerald-400',
    promptColor: 'text-emerald-300',
    btnBg: 'bg-[#03200a] hover:bg-[#063311]',
    btnBorder: 'border-emerald-500/70',
    btnText: 'text-emerald-300',
    btnShadow: 'shadow-[0_4px_0_#065f46]',
    btnActiveBg: 'bg-emerald-500 text-black border-emerald-300',
    popupColor: 'text-emerald-300',
    crtScanline: true
  }
};

export const DEFAULT_THEME_ID = 'cyberpunk';
