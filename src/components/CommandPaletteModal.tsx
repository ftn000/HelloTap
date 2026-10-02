import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useGame } from '../context/GameContext';
import { IDE_THEMES, getThemeTagline, getThemeSoundPresetName } from '../utils/themesList';
import { ThemeId } from '../types/themes';
import { CODE_PROJECT_FILES, getUnlockRequirement } from '../utils/codeProjects';
import { sounds } from '../utils/soundEffects';
import { 
  Terminal, 
  GitBranch, 
  GitMerge, 
  Zap, 
  Flame, 
  Music, 
  Volume2, 
  Palette, 
  FileCode, 
  Coffee, 
  Sparkles, 
  Search, 
  X, 
  CornerDownLeft,
  RotateCw 
} from 'lucide-react';

interface PaletteCommand {
  id: string;
  category: string;
  title: string;
  subtitle?: string;
  icon: React.ReactNode;
  shortcutHint?: string;
  badge?: string;
  action: () => void;
}

interface CommandPaletteModalProps {
  onSelectFile?: (fileId: string) => void;
}

export const CommandPaletteModal: React.FC<CommandPaletteModalProps> = ({ onSelectFile }) => {
  const { 
    isCommandPaletteOpen, 
    setIsCommandPaletteOpen, 
    gitBranch, 
    branchCodeLines,
    createBranch, 
    mergePullRequest, 
    triggerRefactorBlitz, 
    triggerOverclock,
    nextMusicTrack, 
    toggleMusic, 
    isMusicPlaying,
    setThemeId, 
    handleClick, 
    upgrades, 
    buyUpgrade, 
    totalCodeEver,
    lang 
  } = useGame();

  const [search, setSearch] = useState<string>('');
  const [selectedIdx, setSelectedIdx] = useState<number>(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  // Фокус на инпут при открытии
  useEffect(() => {
    if (isCommandPaletteOpen) {
      setSearch('');
      setSelectedIdx(0);
      setTimeout(() => {
        inputRef.current?.focus();
      }, 50);
    }
  }, [isCommandPaletteOpen]);

  // Глобальный перехватчик клавиш для Command Palette: Ctrl+Shift+P, Cmd+Shift+P, F1
  useEffect(() => {
    const handleGlobalKeyDown = (e: KeyboardEvent) => {
      const isPaletteShortcut = 
        ((e.ctrlKey || e.metaKey) && e.shiftKey && e.key.toLowerCase() === 'p') || 
        e.key === 'F1';

      if (isPaletteShortcut) {
        e.preventDefault();
        setIsCommandPaletteOpen(prev => !prev);
      } else if (e.key === 'Escape' && isCommandPaletteOpen) {
        e.preventDefault();
        setIsCommandPaletteOpen(false);
      }
    };

    window.addEventListener('keydown', handleGlobalKeyDown);
    return () => window.removeEventListener('keydown', handleGlobalKeyDown);
  }, [isCommandPaletteOpen, setIsCommandPaletteOpen]);

  // Генерация доступных команд
  const commands = useMemo<PaletteCommand[]>(() => {
    const list: PaletteCommand[] = [];

    // --- 1. Git команды ---
    if (gitBranch !== 'main') {
      list.push({
        id: 'git_merge_pr',
        category: 'Git',
        title: `Git: Merge Pull Request (${gitBranch})`,
        subtitle: lang === 'ru' 
          ? `Слить ветку в main и забрать денежный грант (+${Math.round(branchCodeLines)} LOC staged)` 
          : `Merge branch into main and claim cash grant (+${Math.round(branchCodeLines)} LOC staged)`,
        icon: <GitMerge className="w-4 h-4 text-emerald-400" />,
        badge: 'READY PR',
        action: () => {
          mergePullRequest();
        }
      });
      list.push({
        id: 'git_checkout_main',
        category: 'Git',
        title: 'Git: Checkout main',
        subtitle: lang === 'ru' ? 'Вернуться на главную ветку main' : 'Switch back to main branch',
        icon: <GitBranch className="w-4 h-4 text-cyan-400" />,
        action: () => {
          createBranch('main');
        }
      });
    }

    list.push({
      id: 'git_new_branch',
      category: 'Git',
      title: lang === 'ru' ? 'Git: Создать новую feature-ветку' : 'Git: Create New Feature Branch',
      subtitle: lang === 'ru' ? 'Создать новую ветку разработки (+25% к выработке кода)' : 'Create new development branch (+25% LOC boost)',
      icon: <GitBranch className="w-4 h-4 text-cyan-400" />,
      badge: '+25% BOOST',
      action: () => {
        createBranch();
      }
    });

    list.push({
      id: 'git_activity_heatmap',
      category: 'Git',
      title: lang === 'ru' ? 'GitHub: Открыть график контрибуций (Heatmap)' : 'GitHub: Open Contribution Heatmap',
      subtitle: lang === 'ru' ? '365-дневная сетка коммитов и расчет репутации' : '365-day commit grid and dev score',
      icon: <GitBranch className="w-4 h-4 text-emerald-400" />,
      badge: 'PROFILE',
      action: () => {
        window.dispatchEvent(new CustomEvent('codetap_open_heatmap'));
      }
    });

    // --- 2. CI/CD & GitHub Actions ---
    list.push({
      id: 'cicd_run_pipeline',
      category: 'CI/CD',
      title: lang === 'ru' ? 'CI/CD: Запустить полный пайплайн сборки' : 'CI/CD: Run Full Build Pipeline',
      subtitle: lang === 'ru' ? 'Принудительный запуск 4 шагов GitHub Actions' : 'Force trigger all 4 GitHub Actions steps',
      icon: <RotateCw className="w-4 h-4 text-cyan-400" />,
      badge: 'RUN',
      action: () => {
        window.dispatchEvent(new CustomEvent('codetap_run_pipeline'));
      }
    });

    list.push({
      id: 'cicd_fix_build',
      category: 'CI/CD',
      title: lang === 'ru' ? 'CI/CD: Исправить Broken Build (Hotfix)' : 'CI/CD: Fix Broken Build (Hotfix)',
      subtitle: lang === 'ru' ? 'Устранение упавшего шага сборки с наградой' : 'Resolve failed build step with reward',
      icon: <Zap className="w-4 h-4 text-red-400" />,
      badge: 'HOTFIX',
      action: () => {
        window.dispatchEvent(new CustomEvent('codetap_fix_pipeline'));
      }
    });

    // --- 3. IDE & Рефакторинг ---
    list.push({
      id: 'ide_refactor_blitz',
      category: 'IDE',
      title: lang === 'ru' ? 'IDE: Запустить Refactor Blitz (10x Буст)' : 'IDE: Start Refactor Blitz (10x Boost)',
      subtitle: lang === 'ru' ? 'Активировать 15-секундную фазу ускоренного рефакторинга' : 'Activate 15-second fast refactor phase',
      icon: <Zap className="w-4 h-4 text-amber-400" />,
      badge: '10x MULT',
      action: () => {
        window.dispatchEvent(new CustomEvent('codetap_start_blitz'));
      }
    });

    list.push({
      id: 'ide_overclock',
      category: 'IDE',
      title: lang === 'ru' ? 'System: Включить Turbo Overclock x3.0' : 'System: Enable Turbo Overclock x3.0',
      subtitle: lang === 'ru' ? 'Принудительный форсаж процессора кликов на 14 секунд' : 'Boost click engine for 14 seconds',
      icon: <Flame className="w-4 h-4 text-red-400" />,
      badge: 'x3.0 CLICKS',
      action: () => {
        triggerOverclock(14);
      }
    });

    list.push({
      id: 'code_quick_tap',
      category: 'Code',
      title: lang === 'ru' ? 'Code: Ручной коммит и компиляция' : 'Code: Manual Commit & Compile',
      subtitle: lang === 'ru' ? 'Совершить мгновенный цикл компиляции кода' : 'Trigger instant code compilation cycle',
      icon: <Terminal className="w-4 h-4 text-cyan-300" />,
      action: () => {
        handleClick();
      }
    });

    // --- 3. Аудио команды ---
    list.push({
      id: 'audio_next_track',
      category: 'Audio',
      title: lang === 'ru' ? 'Audio: Следующий Lo-Fi саундтрек' : 'Audio: Next Lo-Fi Soundtrack',
      subtitle: lang === 'ru' ? 'Переключить чиптюн-синтезатор на следующую тему' : 'Switch chiptune synth to next theme',
      icon: <Music className="w-4 h-4 text-fuchsia-400" />,
      action: () => {
        nextMusicTrack();
      }
    });

    list.push({
      id: 'audio_toggle',
      category: 'Audio',
      title: isMusicPlaying 
        ? (lang === 'ru' ? 'Audio: Поставить музыку на паузу' : 'Audio: Pause Lo-Fi Music') 
        : (lang === 'ru' ? 'Audio: Возобновить Lo-Fi музыку' : 'Audio: Resume Lo-Fi Music'),
      subtitle: isMusicPlaying 
        ? (lang === 'ru' ? 'Выключить встроенный синтезатор' : 'Mute ambient synth') 
        : (lang === 'ru' ? 'Включить атмосферный 8-bit саундтрек' : 'Play atmospheric 8-bit soundtrack'),
      icon: <Volume2 className="w-4 h-4 text-fuchsia-400" />,
      action: () => {
        toggleMusic();
      }
    });

    // --- 4. Быстрый апгрейд ---
    const nextAvailableUpgrade = upgrades.find(u => u.level < u.maxLevel);
    if (nextAvailableUpgrade) {
      const uName = lang === 'ru' ? (nextAvailableUpgrade.nameRu || nextAvailableUpgrade.name) : (nextAvailableUpgrade.nameEn || nextAvailableUpgrade.name);
      const uDesc = lang === 'ru' ? (nextAvailableUpgrade.descriptionRu || nextAvailableUpgrade.description) : (nextAvailableUpgrade.descriptionEn || nextAvailableUpgrade.description);
      list.push({
        id: 'upgrade_buy_first',
        category: 'Gear',
        title: `Gear: ${lang === 'ru' ? 'Улучшить' : 'Upgrade'} ${uName} (Lvl ${nextAvailableUpgrade.level + 1})`,
        subtitle: uDesc,
        icon: <Coffee className="w-4 h-4 text-amber-300" />,
        action: () => {
          buyUpgrade(nextAvailableUpgrade.id);
        }
      });
    }

    // --- 5. Темы оформления ---
    Object.values(IDE_THEMES).forEach(th => {
      list.push({
        id: `theme_${th.id}`,
        category: 'Themes',
        title: `Theme: ${th.name} (${th.icon})`,
        subtitle: `${getThemeTagline(th, lang)} • ${lang === 'ru' ? 'Звук' : 'Sound'}: ${getThemeSoundPresetName(th, lang)}`,
        icon: <Palette className="w-4 h-4 text-indigo-400" />,
        action: () => {
          setThemeId(th.id as ThemeId);
        }
      });
    });

    // --- 6. Быстрый переход по файлам ---
    CODE_PROJECT_FILES.forEach(file => {
      const isUnlocked = totalCodeEver >= file.requiredCodeLines;
      list.push({
        id: `file_${file.id}`,
        category: 'Files',
        title: `File: ${file.filename} (${file.language})`,
        subtitle: isUnlocked ? (lang === 'ru' ? 'Открыть файл в редакторе' : 'Open file in editor') : `${lang === 'ru' ? 'Требуется' : 'Requires'}: ${getUnlockRequirement(file, lang)}`,
        icon: <FileCode className="w-4 h-4 text-blue-400" />,
        badge: isUnlocked ? undefined : 'LOCKED',
        action: () => {
          if (isUnlocked && onSelectFile) {
            onSelectFile(file.id);
          }
        }
      });
    });

    return list;
  }, [
    gitBranch, 
    branchCodeLines, 
    mergePullRequest, 
    createBranch, 
    triggerRefactorBlitz, 
    triggerOverclock, 
    handleClick, 
    nextMusicTrack, 
    isMusicPlaying, 
    toggleMusic, 
    upgrades, 
    buyUpgrade, 
    setThemeId, 
    totalCodeEver, 
    onSelectFile,
    lang
  ]);

  // Фильтрация команд по поисковой строке
  const filteredCommands = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return commands;
    return commands.filter(cmd => 
      cmd.title.toLowerCase().includes(q) || 
      cmd.category.toLowerCase().includes(q) || 
      (cmd.subtitle && cmd.subtitle.toLowerCase().includes(q))
    );
  }, [commands, search]);

  // Обработка клавиш навигации внутри палитры
  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setSelectedIdx(prev => (prev + 1) % Math.max(1, filteredCommands.length));
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setSelectedIdx(prev => (prev - 1 + filteredCommands.length) % Math.max(1, filteredCommands.length));
    } else if (e.key === 'Enter') {
      e.preventDefault();
      if (filteredCommands[selectedIdx]) {
        executeCommand(filteredCommands[selectedIdx]);
      }
    }
  };

  const executeCommand = (cmd: PaletteCommand) => {
    sounds.playAutoClickTick();
    cmd.action();
    setIsCommandPaletteOpen(false);
  };

  if (!isCommandPaletteOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 flex items-start justify-center p-3 sm:p-4 bg-black/80 backdrop-blur-md animate-in fade-in duration-100"
      onClick={() => setIsCommandPaletteOpen(false)}
    >
      <div 
        className="relative w-full max-w-xl mt-12 sm:mt-20 bg-slate-900 border border-slate-700/80 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.8)] overflow-hidden flex flex-col max-h-[75vh]"
        onClick={e => e.stopPropagation()}
      >
        {/* Поисковая строка Command Palette */}
        <div className="flex items-center gap-2.5 px-4 py-3 border-b border-slate-800 bg-slate-950/90">
          <span className="text-cyan-400 font-mono text-sm font-bold select-none">&gt;</span>
          <Search className="w-4 h-4 text-slate-500" />
          <input
            ref={inputRef}
            type="text"
            value={search}
            onChange={e => {
              setSearch(e.target.value);
              setSelectedIdx(0);
            }}
            onKeyDown={handleKeyDown}
            placeholder={lang === 'ru' ? "Введите команду или действие... (напр. branch, merge, blitz, theme)" : "Type a command or action... (e.g. branch, merge, blitz, theme)"}
            className="w-full bg-transparent text-sm text-slate-100 placeholder-slate-500 font-mono focus:outline-none"
          />
          <button
            onClick={() => setIsCommandPaletteOpen(false)}
            className="p-1 rounded-lg text-slate-500 hover:text-slate-300 hover:bg-slate-800 transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Список команд */}
        <div 
          ref={listRef}
          className="flex-1 overflow-y-auto p-1.5 space-y-0.5 no-scrollbar font-mono text-xs"
        >
          {filteredCommands.length === 0 ? (
            <div className="p-6 text-center text-slate-500">
              {lang === 'ru' ? 'Команда не найдена. Попробуйте другой запрос.' : 'No commands found. Try another query.'}
            </div>
          ) : (
            filteredCommands.map((cmd, idx) => {
              const isSelected = idx === selectedIdx;
              return (
                <button
                  key={cmd.id}
                  onClick={() => executeCommand(cmd)}
                  onMouseEnter={() => setSelectedIdx(idx)}
                  className={`w-full px-3 py-2 rounded-xl text-left flex items-center justify-between gap-3 transition-colors ${
                    isSelected 
                      ? 'bg-cyan-500/20 text-cyan-200 border border-cyan-500/40 shadow-sm' 
                      : 'text-slate-300 hover:bg-slate-800/60 border border-transparent'
                  }`}
                >
                  <div className="flex items-center gap-2.5 overflow-hidden">
                    <span className="shrink-0">{cmd.icon}</span>
                    <div className="flex flex-col truncate">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold truncate">{cmd.title}</span>
                        {cmd.badge && (
                          <span className="px-1.5 py-0.2 rounded text-[9px] font-bold bg-amber-500/20 text-amber-300 border border-amber-500/40">
                            {cmd.badge}
                          </span>
                        )}
                      </div>
                      {cmd.subtitle && (
                        <span className="text-[10px] text-slate-400 font-sans truncate">
                          {cmd.subtitle}
                        </span>
                      )}
                    </div>
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    <span className="text-[10px] text-slate-500 font-sans hidden sm:inline">
                      {cmd.category}
                    </span>
                    {isSelected && (
                      <CornerDownLeft className="w-3.5 h-3.5 text-cyan-400 animate-pulse" />
                    )}
                  </div>
                </button>
              );
            })
          )}
        </div>

        {/* Подвал с подсказками клавиш */}
        <div className="px-4 py-2 border-t border-slate-800/80 bg-slate-950/60 flex items-center justify-between text-[10px] font-mono text-slate-500">
          <div className="flex items-center gap-3">
            <span>↑↓ {lang === 'ru' ? 'Навигация' : 'Navigate'}</span>
            <span>↵ {lang === 'ru' ? 'Выбор' : 'Select'}</span>
            <span>ESC {lang === 'ru' ? 'Закрыть' : 'Close'}</span>
          </div>
          <span className="text-cyan-400/80">VS Code Palette [Ctrl+Shift+P / F1]</span>
        </div>
      </div>
    </div>
  );
};
