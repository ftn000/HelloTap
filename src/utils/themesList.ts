import { IdeTheme } from '../types/themes';
import { Language } from './i18n';

export const IDE_THEMES: Record<string, IdeTheme> = {
  cyberpunk: {
    id: 'cyberpunk',
    name: 'Cyberpunk Neon',
    tagline: 'Неоновый бирюзово-фиолетовый кибер-стиль будущего',
    taglineRu: 'Неоновый бирюзово-фиолетовый кибер-стиль будущего',
    taglineEn: 'Neon cyan & purple futuristic cyberpunk aesthetic',
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
    crtScanline: false,
    syntaxKeyword: '#c084fc',     // purple-400
    syntaxType: '#38bdf8',        // sky-400
    syntaxFunction: '#4ade80',    // green-400
    syntaxString: '#fde047',      // yellow-300
    syntaxNumber: '#fb923c',      // orange-400
    syntaxComment: '#64748b',     // slate-500
    syntaxPunctuation: '#94a3b8', // slate-400
    soundPreset: 'laser',
    soundPresetName: 'Неоновый лазер',
    soundPresetNameRu: 'Неоновый лазер',
    soundPresetNameEn: 'Neon Laser'
  },
  monokai: {
    id: 'monokai',
    name: 'Monokai Pro',
    tagline: 'Легендарная палитра Sublime Text: янтарь, лайм и сочный кокос',
    taglineRu: 'Легендарная палитра Sublime Text: янтарь, лайм и сочный кокос',
    taglineEn: 'Legendary Sublime Text palette: amber, lime, and rich coconut',
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
    crtScanline: false,
    syntaxKeyword: '#f92672',     // monokai magenta
    syntaxType: '#66d9ef',        // monokai cyan
    syntaxFunction: '#a6e22e',    // monokai lime green
    syntaxString: '#e6db74',      // monokai yellow
    syntaxNumber: '#ae81ff',      // monokai violet
    syntaxComment: '#75715e',     // monokai muted gray
    syntaxPunctuation: '#f8f8f2', // monokai white
    soundPreset: 'brown',
    soundPresetName: 'Мягкий тактильный',
    soundPresetNameRu: 'Мягкий тактильный',
    soundPresetNameEn: 'Soft Tactile'
  },
  dracula: {
    id: 'dracula',
    name: 'Dracula Dark',
    tagline: 'Классическая темная вампирская тема: пурпур, розовый и циан',
    taglineRu: 'Классическая темная вампирская тема: пурпур, розовый и циан',
    taglineEn: 'Classic dark vampire aesthetic: purple, pink, and cyan',
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
    crtScanline: false,
    syntaxKeyword: '#ff79c6',     // dracula pink
    syntaxType: '#8be9fd',        // dracula cyan
    syntaxFunction: '#50fa7b',    // dracula green
    syntaxString: '#f1fa8c',      // dracula yellow
    syntaxNumber: '#bd93f9',      // dracula purple
    syntaxComment: '#6272a4',     // dracula comment blue-gray
    syntaxPunctuation: '#f8f8f2', // dracula foreground
    soundPreset: 'blue',
    soundPresetName: 'Звонкий кликер',
    soundPresetNameRu: 'Звонкий кликер',
    soundPresetNameEn: 'Crisp Clicky'
  },
  matrix: {
    id: 'matrix',
    name: 'Retro Matrix CRT',
    tagline: 'Зеленый монохромный люминофор лампового терминала 1980-х',
    taglineRu: 'Зеленый монохромный люминофор лампового терминала 1980-х',
    taglineEn: 'Green monochrome phosphor of a 1980s CRT terminal',
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
    crtScanline: true,
    syntaxKeyword: '#34d399',     // emerald-400
    syntaxType: '#6ee7b7',        // emerald-300
    syntaxFunction: '#10b981',    // emerald-500
    syntaxString: '#a7f3d0',      // emerald-200
    syntaxNumber: '#059669',      // emerald-600
    syntaxComment: '#064e3b',     // dark green
    syntaxPunctuation: '#34d399', // emerald-400
    soundPreset: 'typewriter',
    soundPresetName: 'Винтажный стук',
    soundPresetNameRu: 'Винтажный стук',
    soundPresetNameEn: 'Vintage Mechanical'
  },
  nordic: {
    id: 'nordic',
    name: 'Nordic Frost',
    tagline: 'Арктическая прохладная палитра северного сияния и ледяного стекла',
    taglineRu: 'Арктическая прохладная палитра северного сияния и ледяного стекла',
    taglineEn: 'Arctic aurora borealis palette with frosted glacial glass',
    icon: '❄️',
    terminalBg: 'from-[#1e2530]/95 via-[#18202c]/95 to-[#131720]/95',
    terminalBorder: 'border-[#88c0d0]/40 hover:border-[#88c0d0]/80',
    terminalGlow: 'rgba(136, 192, 208, 0.35)',
    codeColor: 'text-[#88c0d0]',
    promptColor: 'text-[#81a1c1]',
    btnBg: 'bg-[#2e3440] hover:bg-[#3b4252]',
    btnBorder: 'border-[#88c0d0]/60',
    btnText: 'text-[#88c0d0]',
    btnShadow: 'shadow-[0_4px_0_#434c5e]',
    btnActiveBg: 'bg-[#88c0d0] text-[#2e3440] border-[#8fbcbb]',
    popupColor: 'text-[#88c0d0]',
    crtScanline: false,
    syntaxKeyword: '#81a1c1',     // nord blue
    syntaxType: '#8fbcbb',        // nord teal
    syntaxFunction: '#88c0d0',    // nord cyan
    syntaxString: '#a3be8c',      // nord green
    syntaxNumber: '#b48ead',      // nord magenta
    syntaxComment: '#4c566a',     // nord muted gray
    syntaxPunctuation: '#d8dee9', // nord snow white
    soundPreset: 'red',
    soundPresetName: 'Мягкий линейный',
    soundPresetNameRu: 'Мягкий линейный',
    soundPresetNameEn: 'Smooth Linear'
  },
  tokyonight: {
    id: 'tokyonight',
    name: 'Tokyo Night',
    tagline: 'Глубокая полночь неонового Сибуя: лазурь, маджента и сакура',
    taglineRu: 'Глубокая полночь неонового Сибуя: лазурь, маджента и сакура',
    taglineEn: 'Deep midnight neon of Shibuya: azure, magenta, and cherry blossom',
    icon: '🌸',
    terminalBg: 'from-[#1a1b26]/95 via-[#16161e]/95 to-[#13141c]/95',
    terminalBorder: 'border-[#7aa2f7]/40 hover:border-[#bb9af7]/70',
    terminalGlow: 'rgba(122, 162, 247, 0.35)',
    codeColor: 'text-[#7aa2f7]',
    promptColor: 'text-[#bb9af7]',
    btnBg: 'bg-[#24283b] hover:bg-[#2f3549]',
    btnBorder: 'border-[#bb9af7]/60',
    btnText: 'text-[#bb9af7]',
    btnShadow: 'shadow-[0_4px_0_#414868]',
    btnActiveBg: 'bg-[#bb9af7] text-[#1a1b26] border-[#7aa2f7]',
    popupColor: 'text-[#7dcfff]',
    crtScanline: false,
    syntaxKeyword: '#bb9af7',     // tokyo violet
    syntaxType: '#2ac3de',        // tokyo cyan
    syntaxFunction: '#7aa2f7',    // tokyo blue
    syntaxString: '#9ece6a',      // tokyo lime
    syntaxNumber: '#ff9e64',      // tokyo orange
    syntaxComment: '#565f89',     // tokyo muted slate
    syntaxPunctuation: '#c0caf5', // tokyo white
    soundPreset: 'laser',
    soundPresetName: 'Неоновый лазер',
    soundPresetNameRu: 'Неоновый лазер',
    soundPresetNameEn: 'Neon Laser'
  }
};

export const DEFAULT_THEME_ID = 'cyberpunk';

export function getThemeTagline(theme: IdeTheme, lang: Language): string {
  if (lang === 'ru') return theme.taglineRu || theme.tagline;
  return theme.taglineEn || theme.tagline;
}

export function getThemeSoundPresetName(theme: IdeTheme, lang: Language): string {
  if (lang === 'ru') return theme.soundPresetNameRu || theme.soundPresetName;
  return theme.soundPresetNameEn || theme.soundPresetName;
}
