import React, { useState, useEffect, useRef } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Flame, Sparkles, Terminal, Cpu, Zap, Bug, GitBranch, GitMerge, CheckCircle2, Lock, Volume2, AlertTriangle } from 'lucide-react';
import confetti from 'canvas-confetti';
import { IDE_THEMES } from '../utils/themesList';
import { StudioDecor } from './StudioDecor';
import { CICDPipeline } from './CICDPipeline';
import { CODE_PROJECT_FILES, CodeProjectFile, tokenizeCodeLine, getTokenColor, getUnlockRequirement } from '../utils/codeProjects';
import { sounds } from '../utils/soundEffects';

interface Popup {
  id: number;
  text: string;
  tag: string;
  x: number;
  y: number;
  isCrit: boolean;
}

interface CodeBug {
  id: string;
  title: string;
  timeLeft: number;
}

export interface LinterWarning {
  id: string;
  fileId: string;
  lineIdx: number;
  tokenIdx: number;
  originalText: string;
  errorText: string;
  message: string;
  timeLeft: number;
}

const LINTER_ERROR_TEMPLATES = [
  { typo: (s: string) => s + ';;', msg: 'SyntaxError: Extra semicolon or invalid token' },
  { typo: (s: string) => s.slice(0, -1) + '??', msg: 'SyntaxError: Invalid optional chaining' },
  { typo: (s: string) => s + '()()', msg: 'TypeError: Expression is not callable' },
  { typo: (s: string) => s + '<T_Err>', msg: 'TypeError: Missing generic type argument' },
  { typo: (s: string) => s + '!null', msg: 'TS2531: Object is possibly null' },
  { typo: (s: string) => 'typo_' + s, msg: 'ReferenceError: Undefined variable name' }
];

const BUG_TYPES = [
  'NullReferenceException: Object not set at Line 42',
  'Deadlock: ThreadPool exhaustion in Worker #3',
  'IndexOutOfRangeException: Array buffer overflow',
  'SyntaxError: Missing semicolon in production build',
  'MemoryLeak: Unclosed WebSocket stream in EventBus'
];

const CODE_PARTICLES = [
  'git push', 'npm i', 'const bug = null', 'console.log("⚡")',
  'loc += 1;', '200 OK', 'async/await', '✨ Clean Code',
  'refactor()', 'docker up', '🚀 deploy', '418 Teapot',
  'TODO: sleep', 'return win;'
];

const COMPILE_LOGS = [
  '✓ [BUILD] bundle emitted in 8ms',
  '⚡ [HMR] fast refresh complete',
  '✓ [TYPECHECK] 0 errors, 0 warnings',
  '🚀 [VITE] optimized chunk dependencies',
  '✓ [TESTS] 48 passed in 12ms',
  '📦 [ASSETS] compressed gzip stream'
];

const SWITCH_OPTIONS = [
  { id: 'blue', name: 'Blue', label: 'Clicky', icon: '🔵' },
  { id: 'red', name: 'Red', label: 'Linear', icon: '🔴' },
  { id: 'brown', name: 'Brown', label: 'Tactile', icon: '🟤' },
  { id: 'laser', name: 'Laser', label: 'Cyber', icon: '⚡' },
  { id: 'typewriter', name: 'Typewriter', label: 'Vintage Ding', icon: '📜' },
] as const;

const REFACTOR_TAGS = [
  '⚡ [DRY: Deduplicate]',
  '⚡ [SIMD: Vectorize]',
  '⚡ [ASYNC: Non-block]',
  '⚡ [ZERO-COPY: Buffer]',
  '⚡ [O(1): HashMap]'
];

export const MainClicker: React.FC = () => {
  const { 
    handleClick, 
    comboEnergy, 
    isInFlow, 
    codePerClick, 
    codePerSec, 
    hasAutoClicker, 
    themeId, 
    totalCodeEver, 
    overclockRemainingSec, 
    triggerOverclock, 
    unlockedSkills, 
    switchType, 
    setSwitchType, 
    recordCommit, 
    gitBranch, 
    branchCodeLines, 
    mergedPrCount, 
    createBranch, 
    mergePullRequest, 
    blitzRemainingSec, 
    isBlitzActive, 
    triggerRefactorBlitz, 
    setIsCommandPaletteOpen, 
    currentStreak, 
    lang,
    t 
  } = useGame();

  const currentTheme = IDE_THEMES[themeId] || IDE_THEMES['cyberpunk'];
  const [popups, setPopups] = useState<Popup[]>([]);
  const [activeKey, setActiveKey] = useState<boolean>(false);
  const [activeBug, setActiveBug] = useState<CodeBug | null>(null);
  const [linterWarning, setLinterWarning] = useState<LinterWarning | null>(null);

  // Мини-игра Refactor Blitz
  const [blitzGameActive, setBlitzGameActive] = useState<boolean>(false);
  const [blitzTimeLeft, setBlitzTimeLeft] = useState<number>(0);
  const [refactoredLineIndices, setRefactoredLineIndices] = useState<number[]>([]);

  // Динамические файлы проектов: выбор активного файла
  const [activeFileId, setActiveFileId] = useState<string>('ts_starter');
  const [lineIdx, setLineIdx] = useState<number>(0);
  const [gitStatus, setGitStatus] = useState<string>('git: (main)*');
  const [compileLog, setCompileLog] = useState<string>('✓ Ready to build');

  // Комбо-стрик по скорости клика
  const clickTimesRef = useRef<number[]>([]);
  const [currentCps, setCurrentCps] = useState<number>(0);

  // Вычисление доступных файлов по прогрессу игрока
  const activeFile: CodeProjectFile = 
    CODE_PROJECT_FILES.find(f => f.id === activeFileId) || CODE_PROJECT_FILES[0];

  // Вычисление динамического комбо-множителя от CPS
  const getComboMultiplier = (cpsVal: number): { mult: number; label: string; color: string } => {
    if (cpsVal >= 12) return { mult: 3.0, label: '🌟 HYPER SPEED x3.0!', color: 'text-fuchsia-400 bg-fuchsia-500/20 border-fuchsia-400/50 shadow-[0_0_15px_rgba(217,70,239,0.5)]' };
    if (cpsVal >= 9) return { mult: 2.0, label: '⚡ SYNTH OVERDRIVE x2.0!', color: 'text-amber-300 bg-amber-500/20 border-amber-400/50 shadow-[0_0_12px_rgba(245,158,11,0.5)]' };
    if (cpsVal >= 6) return { mult: 1.5, label: '🔥 GODLIKE FLOW x1.5', color: 'text-orange-400 bg-orange-500/20 border-orange-400/40' };
    if (cpsVal >= 3) return { mult: 1.2, label: '⚡ FAST TYPING x1.2', color: 'text-cyan-400 bg-cyan-500/15 border-cyan-400/30' };
    return { mult: 1.0, label: '', color: '' };
  };

  const comboInfo = getComboMultiplier(currentCps);

  // Каждые 200 мс пересчитываем CPS за скользящее окно 1.2 секунды
  useEffect(() => {
    const timer = setInterval(() => {
      const now = Date.now();
      clickTimesRef.current = clickTimesRef.current.filter(ts => now - ts < 1200);
      const calculated = clickTimesRef.current.length / 1.2;
      setCurrentCps(Math.round(calculated * 10) / 10);
    }, 200);
    return () => clearInterval(timer);
  }, []);

  // Горячие клавиши Tab и Ctrl+P для мгновенной смены активного файла в IDE
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target as HTMLElement | null;
      if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable)) {
        return;
      }

      const isTab = e.key === 'Tab';
      const isCtrlP = (e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'p';

      if (isTab || isCtrlP) {
        e.preventDefault();

        const unlocked = CODE_PROJECT_FILES.filter(f => totalCodeEver >= f.requiredCodeLines);
        if (unlocked.length <= 1) return;

        const currentIdx = unlocked.findIndex(f => f.id === activeFileId);
        let nextIdx: number;

        if (e.shiftKey && isTab) {
          nextIdx = (currentIdx - 1 + unlocked.length) % unlocked.length;
        } else {
          nextIdx = (currentIdx + 1) % unlocked.length;
        }

        const nextFile = unlocked[nextIdx];
        setActiveFileId(nextFile.id);
        setLineIdx(0);
        sounds.playAutoClickTick();
        setCompileLog(`📂 [${isTab ? 'TAB' : 'Ctrl+P'}] ${lang === 'ru' ? 'Открыт' : 'Opened'}: ${nextFile.filename} (${nextFile.language})`);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [totalCodeEver, activeFileId]);

  const onPointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    clickTimesRef.current.push(Date.now());

    // Базовый клик
    const { isCrit, codeAdded } = handleClick(e.clientX, e.clientY);
    setActiveKey(true);

    // Продвигаем строку живого кода в текущем файле
    setLineIdx(prevLine => {
      if (prevLine >= activeFile.codeLines.length - 1) {
        // Полный цикл файла пройден: совершаем коммит и триггерим языковую ачивку!
        recordCommit(activeFile.langKey);
        setGitStatus(`git: commit [${activeFile.filename}] 🚀`);
        setTimeout(() => {
          setGitStatus('git: (main)*');
        }, 750);
        return 0; // Зацикливание текущего скрипта
      }
      return prevLine + 1;
    });

    // Тактильный эффект сборки: обновление лога компиляции и Git-статуса
    const randomLog = COMPILE_LOGS[Math.floor(Math.random() * COMPILE_LOGS.length)];
    setCompileLog(`${randomLog} (+${formatNumber(codeAdded)} C#)`);
    if (gitStatus === 'git: (main)*') {
      setGitStatus(`git: (shipping +${formatNumber(codeAdded)})`);
      setTimeout(() => {
        setGitStatus('git: (main)*');
      }, 400);
    }

    if (isCrit) {
      confetti({
        particleCount: 28,
        spread: 50,
        origin: { x: e.clientX / window.innerWidth, y: e.clientY / window.innerHeight },
        colors: ['#F59E0B', '#10B981', '#06B6D4', '#EC4899']
      });
    }

    const randomTag = CODE_PARTICLES[Math.floor(Math.random() * CODE_PARTICLES.length)];
    const newPopup: Popup = {
      id: Date.now() + Math.random(),
      text: isCrit ? `✨ ${t.critText} +${formatNumber(codeAdded)}` : `+${formatNumber(codeAdded)}`,
      tag: randomTag,
      x,
      y,
      isCrit
    };

    setPopups(prev => [...prev.slice(-14), newPopup]);

    setTimeout(() => {
      setPopups(prev => prev.filter(p => p.id !== newPopup.id));
    }, 850);
  };

  // Периодический спавн багов (раз в 45 секунд)
  useEffect(() => {
    const interval = setInterval(() => {
      setActiveBug(prev => {
        if (prev) return prev;
        const randomTitle = BUG_TYPES[Math.floor(Math.random() * BUG_TYPES.length)];
        return {
          id: `bug_${Date.now()}`,
          title: randomTitle,
          timeLeft: 8
        };
      });
    }, 45000);

    return () => clearInterval(interval);
  }, []);

  // Таймер обратного отсчета активного бага
  useEffect(() => {
    if (!activeBug) return;
    const timer = setInterval(() => {
      setActiveBug(prev => {
        if (!prev) return null;
        if (prev.timeLeft <= 1) return null;
        return { ...prev, timeLeft: prev.timeLeft - 1 };
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [activeBug]);

  // Периодический спавн синтаксических ошибок Linter Warning (каждые 20-25 секунд)
  useEffect(() => {
    const interval = setInterval(() => {
      setLinterWarning(prev => {
        if (prev) return prev;

        const targetLineIdx = Math.floor(Math.random() * activeFile.codeLines.length);
        const line = activeFile.codeLines[targetLineIdx];
        const tokens = tokenizeCodeLine(line);

        const candidates = tokens
          .map((tok, idx) => ({ tok, idx }))
          .filter(({ tok }) => tok.type !== 'punctuation' && tok.type !== 'comment' && tok.text.trim().length > 1);

        if (candidates.length === 0) return null;

        const chosen = candidates[Math.floor(Math.random() * candidates.length)];
        const template = LINTER_ERROR_TEMPLATES[Math.floor(Math.random() * LINTER_ERROR_TEMPLATES.length)];

        return {
          id: `linter_${Date.now()}`,
          fileId: activeFile.id,
          lineIdx: targetLineIdx,
          tokenIdx: chosen.idx,
          originalText: chosen.tok.text,
          errorText: template.typo(chosen.tok.text),
          message: template.msg,
          timeLeft: 12
        };
      });
    }, 22000);

    return () => clearInterval(interval);
  }, [activeFile.id, activeFile.codeLines]);

  // Таймер обратного отсчета ошибки линтера
  useEffect(() => {
    if (!linterWarning) return;
    const timer = setInterval(() => {
      setLinterWarning(prev => {
        if (!prev) return null;
        if (prev.timeLeft <= 1) return null;
        return { ...prev, timeLeft: prev.timeLeft - 1 };
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [linterWarning]);

  // Мгновенный фикс синтаксической ошибки линтера с бонусом к коду
  const handleFixLinter = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!linterWarning) return;

    const bountyMultiplier = 1.0 + (unlockedSkills['skill_clean_coder'] || 0) * 0.40;
    const bonusCode = Math.round(Math.max(300, codePerClick * 25, codePerSec * 4) * bountyMultiplier);

    handleClick();
    sounds.playQuickFix();
    sounds.triggerHaptic('success');

    confetti({
      particleCount: 35,
      spread: 60,
      origin: { y: 0.55 },
      colors: ['#EF4444', '#10B981', '#38BDF8', '#F59E0B']
    });

    const newPopup: Popup = {
      id: Date.now() + Math.random(),
      text: `🔧 QUICK FIX! +${formatNumber(bonusCode)} C#`,
      tag: '0 ERRORS',
      x: 100,
      y: 90,
      isCrit: true
    };

    setPopups(prev => [...prev.slice(-14), newPopup]);
    setCompileLog(`✓ [LINTER] Hotfix applied: ${linterWarning.originalText} (+${formatNumber(bonusCode)} C#)`);
    setLinterWarning(null);
  };

  const handleFixBug = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!activeBug) return;

    const bountyMultiplier = 1.0 + (unlockedSkills['skill_bug_bounty_hunter'] || 0) * 0.50;
    const bonusCode = Math.round(Math.max(500, codePerSec * 45) * bountyMultiplier);

    handleClick();
    triggerOverclock(14);

    confetti({
      particleCount: 50,
      spread: 75,
      origin: { y: 0.5 },
      colors: ['#EF4444', '#10B981', '#06B6D4', '#F59E0B']
    });

    const newPopup: Popup = {
      id: Date.now() + Math.random(),
      text: t.bugFixedToast.replace('{0}', formatNumber(bonusCode)),
      tag: 'HOTFIX',
      x: 80,
      y: 80,
      isCrit: true
    };
    setPopups(prev => [...prev.slice(-14), newPopup]);
    setActiveBug(null);
  };

  // Периодический спавн мини-игры Refactor Blitz (каждые 10 минут)
  useEffect(() => {
    const interval = setInterval(() => {
      if (!blitzGameActive && !isBlitzActive) {
        setBlitzGameActive(true);
        setBlitzTimeLeft(15);
        setRefactoredLineIndices([]);
      }
    }, 600000);

    return () => clearInterval(interval);
  }, [blitzGameActive, isBlitzActive]);

  // Слушатель кастомного события для мгновенного старта мини-игры из Command Palette
  useEffect(() => {
    const handleStartBlitz = () => {
      setBlitzGameActive(true);
      setBlitzTimeLeft(15);
      setRefactoredLineIndices([]);
    };
    window.addEventListener('codetap_start_blitz', handleStartBlitz);
    return () => window.removeEventListener('codetap_start_blitz', handleStartBlitz);
  }, []);

  // Таймер обратного отсчета для активной мини-игры Refactor Blitz
  useEffect(() => {
    if (!blitzGameActive) return;
    const timer = setInterval(() => {
      setBlitzTimeLeft(prev => {
        if (prev <= 1) {
          setBlitzGameActive(false);
          setRefactoredLineIndices([]);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [blitzGameActive]);

  // Клик по целевой строке рефакторинга в мини-игре Refactor Blitz
  const handleRefactorLine = (e: React.MouseEvent, targetIdx: number) => {
    e.stopPropagation();
    if (!blitzGameActive || refactoredLineIndices.includes(targetIdx)) return;

    const nextIndices = [...refactoredLineIndices, targetIdx];
    setRefactoredLineIndices(nextIndices);
    sounds.playQuickFix();
    sounds.triggerHaptic('medium');

    const neededLines = Math.min(5, activeFile.codeLines.length);
    if (nextIndices.length >= neededLines) {
      setBlitzGameActive(false);
      setRefactoredLineIndices([]);
      triggerRefactorBlitz(20);

      confetti({
        particleCount: 65,
        spread: 85,
        origin: { y: 0.5 },
        colors: ['#F59E0B', '#10B981', '#38BDF8', '#EC4899', '#A855F7']
      });

      const newPopup: Popup = {
        id: Date.now() + Math.random(),
        text: `🚀 REFACTOR BLITZ! x10 BOOST (20${t.secShort})`,
        tag: t.refactorDoneTag,
        x: 100,
        y: 80,
        isCrit: true
      };
      setPopups(prev => [...prev.slice(-14), newPopup]);
      setCompileLog('⚡ [BLITZ] 5x Refactors done! 10x multiplier active!');
    }
  };

  // Слияние Pull Request
  const handleMergePR = (e: React.MouseEvent) => {
    e.stopPropagation();
    const result = mergePullRequest();
    if (result) {
      confetti({
        particleCount: 50,
        spread: 70,
        origin: { y: 0.6 },
        colors: ['#10B981', '#38BDF8', '#F59E0B']
      });

      const newPopup: Popup = {
        id: Date.now() + Math.random(),
        text: `🔀 ${t.prMergedPopup} +₽${formatNumber(result.rewardMoney)} +${formatNumber(result.rewardCode)} C#`,
        tag: `PR #${mergedPrCount + 1}`,
        x: 90,
        y: 80,
        isCrit: true
      };
      setPopups(prev => [...prev.slice(-14), newPopup]);
      setCompileLog(`✓ [GIT] PR merged into main: +₽${formatNumber(result.rewardMoney)}`);
    }
  };

  const onPointerUp = () => {
    setActiveKey(false);
  };

  return (
    <div className="flex flex-col items-center justify-center p-3 sm:p-4 w-full max-w-md mx-auto">
      {/* Интерактивный декор и питомцы студии */}
      <StudioDecor />

      {/* Шкала Комбо «В Потоке», CPS Стрик и Overclock */}
      <div className="w-full mb-2.5">
        <div className="flex items-center justify-between text-xs font-mono mb-1 px-1">
          <div className="flex items-center gap-1.5 text-slate-300">
            <Flame className={`w-3.5 h-3.5 ${isInFlow ? 'text-amber-400 animate-bounce' : 'text-slate-500'}`} />
            <span className={isInFlow ? 'text-amber-400 font-bold' : ''}>
              {isInFlow ? t.flowMode : t.focusBar}
            </span>

            {overclockRemainingSec > 0 && (
              <span className="px-2 py-0.5 rounded-full bg-red-500/20 text-red-400 border border-red-500/40 text-[10px] font-bold animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.4)]">
                🔥 OVERCLOCK x3.0 ({overclockRemainingSec}{t.secShort})
              </span>
            )}

            {isBlitzActive && (
              <span className="px-2 py-0.5 rounded-full bg-gradient-to-r from-amber-400 to-yellow-500 text-slate-950 border border-yellow-300 text-[10px] font-black animate-pulse shadow-[0_0_12px_rgba(245,158,11,0.8)]">
                ⚡ 10x BLITZ ({blitzRemainingSec}{t.secShort})
              </span>
            )}

            {comboInfo.label && (
              <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border animate-pulse ${comboInfo.color}`}>
                {comboInfo.label}
              </span>
            )}
          </div>
          <div className="flex items-center gap-2">
            {currentCps > 0 && (
              <span className="text-[11px] text-cyan-400 font-bold">{currentCps} CPS</span>
            )}
            <span className="text-slate-400">{(comboEnergy * 100).toFixed(0)}%</span>
          </div>
        </div>

        <div className="h-2 w-full bg-slate-900 rounded-full overflow-hidden p-0.5 border border-slate-800">
          <div
            className={`h-full rounded-full transition-all duration-100 ${
              isBlitzActive
                ? 'bg-gradient-to-r from-amber-400 via-yellow-300 to-amber-500 shadow-[0_0_14px_rgba(245,158,11,0.9)] animate-pulse'
                : overclockRemainingSec > 0
                ? 'bg-gradient-to-r from-red-500 via-orange-500 to-amber-400 shadow-[0_0_12px_rgba(239,68,68,0.8)] animate-pulse'
                : isInFlow
                ? 'bg-gradient-to-r from-amber-500 via-orange-500 to-yellow-400 shadow-[0_0_10px_rgba(245,158,11,0.7)] animate-pulse'
                : comboInfo.mult > 1.0
                ? 'bg-gradient-to-r from-cyan-400 via-teal-400 to-emerald-400'
                : 'bg-gradient-to-r from-cyan-500 to-blue-500'
            }`}
            style={{ width: `${Math.min(100, comboEnergy * 100)}%` }}
          />
        </div>
      </div>

      {/* Интерактивная IDE-Консоль с вкладками файлов и честной подсветкой синтаксиса */}
      <div
        onPointerDown={onPointerDown}
        onPointerUp={onPointerUp}
        onPointerLeave={onPointerUp}
        style={{
          boxShadow: activeKey
            ? `0 0 35px ${currentTheme.terminalGlow}`
            : isInFlow
            ? '0 0 35px rgba(245,158,11,0.35)'
            : `0 10px 30px rgba(0,0,0,0.5), 0 0 18px ${currentTheme.terminalGlow}`
        }}
        className={`relative w-full rounded-3xl p-4 sm:p-5 flex flex-col justify-between cursor-pointer select-none transition-all duration-75 overflow-hidden border ${
          activeKey
            ? `scale-[0.985] ${currentTheme.terminalBorder} bg-gradient-to-b ${currentTheme.terminalBg}`
            : isInFlow
            ? 'border-amber-500/60 shadow-[0_0_35px_rgba(245,158,11,0.3)] bg-gradient-to-b from-slate-900 to-slate-950'
            : `${currentTheme.terminalBorder} bg-gradient-to-b ${currentTheme.terminalBg}`
        }`}
      >
        {/* Ретро CRT Scanlines оверлей */}
        {currentTheme.crtScanline && (
          <div className="absolute inset-0 pointer-events-none bg-[linear-gradient(rgba(18,16,16,0)_50%,rgba(0,0,0,0.35)_50%)] bg-[length:100%_4px] opacity-40 z-10" />
        )}

        {/* ШАГ 1: Вкладки файлов проекта в шапке редактора (IDE Tabs) */}
        <div className="relative z-20 flex items-center justify-between border-b border-slate-800/80 pb-2 mb-2 gap-2">
          {/* Кнопки светофора macOS / Linux */}
          <div className="flex items-center gap-1.5 shrink-0">
            <div className="w-2.5 h-2.5 rounded-full bg-red-500/80" />
            <div className="w-2.5 h-2.5 rounded-full bg-amber-500/80" />
            <div className="w-2.5 h-2.5 rounded-full bg-emerald-500/80" />
          </div>

          {/* Скроллируемые вкладки открытых файлов */}
          <div className="flex items-center gap-1 overflow-x-auto no-scrollbar py-0.5">
            {CODE_PROJECT_FILES.map(file => {
              const isUnlocked = totalCodeEver >= file.requiredCodeLines;
              const isSelected = activeFileId === file.id;

              return (
                <button
                  key={file.id}
                  onClick={(e) => {
                    e.stopPropagation();
                    if (isUnlocked) {
                      setActiveFileId(file.id);
                      setLineIdx(0);
                    }
                  }}
                  disabled={!isUnlocked}
                  className={`px-2 py-0.5 rounded-lg text-[10px] font-mono flex items-center gap-1 transition-all whitespace-nowrap ${
                    isSelected
                      ? 'bg-slate-800 text-cyan-300 border border-cyan-500/40 shadow-sm'
                      : isUnlocked
                      ? 'bg-slate-900/60 text-slate-400 hover:text-slate-200 border border-transparent'
                      : 'bg-slate-950/40 text-slate-600 border border-transparent cursor-not-allowed'
                  }`}
                  title={isUnlocked ? `${file.filename} (${file.language}) [Tab / Ctrl+P]` : `${file.filename}: ${getUnlockRequirement(file, lang)}`}
                >
                  <span>{file.langIcon}</span>
                  <span>{file.filename}</span>
                  {!isUnlocked && <Lock className="w-2.5 h-2.5 text-slate-600 ml-0.5" />}
                </button>
              );
            })}
            <span className="hidden sm:inline text-[9px] text-slate-500 font-mono ml-1" title={t.fastSwitchHint}>
              [Tab/Ctrl+P]
            </span>
          </div>

          {/* Доход за клик */}
          <div className="flex items-center gap-1.5 shrink-0">
            {hasAutoClicker && (
              <div className="hidden sm:flex items-center gap-1 text-[10px] font-mono text-cyan-300 bg-cyan-500/15 px-1.5 py-0.5 rounded-full border border-cyan-500/40 shadow-[0_0_8px_rgba(6,182,212,0.3)] animate-pulse">
                <Zap className="w-2.5 h-2.5 text-cyan-400" />
                <span>10 CPS</span>
              </div>
            )}
            <div className="flex items-center gap-1 text-[10px] sm:text-[11px] font-mono text-cyan-400/90 bg-cyan-950/50 px-2 py-0.5 rounded-md border border-cyan-800/40">
              <Cpu className="w-3 h-3 text-cyan-400" />
              <span>+{formatNumber(codePerClick)} C#</span>
            </div>
          </div>
        </div>

        {/* ОХОТА НА БАГИ: Интерактивная строка бага */}
        {activeBug && (
          <button
            onClick={handleFixBug}
            className="relative z-30 w-full p-2.5 my-2 rounded-2xl bg-red-950/90 border-2 border-red-500 shadow-[0_0_20px_rgba(239,68,68,0.7)] text-left flex items-center justify-between gap-2 animate-bounce cursor-pointer"
          >
            <div className="flex items-center gap-2 overflow-hidden">
              <Bug className="w-4 h-4 text-red-400 animate-spin shrink-0" />
              <span className="text-xs font-bold text-red-200 truncate font-mono">
                {activeBug.title}
              </span>
            </div>
            <span className="px-2.5 py-1 rounded-xl bg-red-500 text-black text-[10px] font-black shrink-0 tracking-wider">
              {t.bugButton.replace('{0}', activeBug.timeLeft.toString())}
            </span>
          </button>
        )}

        {/* МИНИ-ИГРА: REFACTOR BLITZ БАННЕР */}
        {blitzGameActive && (
          <div className="relative z-30 w-full p-2.5 my-2 rounded-2xl bg-amber-950/90 border-2 border-amber-400 shadow-[0_0_20px_rgba(245,158,11,0.7)] text-left flex items-center justify-between gap-2 animate-pulse">
            <div className="flex items-center gap-2 overflow-hidden">
              <Zap className="w-4 h-4 text-amber-400 animate-bounce shrink-0" />
              <div>
                <div className="text-xs font-bold text-amber-200 font-mono">
                  ⚡ {t.refactorBlitzBanner.replace('{0}', refactoredLineIndices.length.toString())}
                </div>
                <div className="text-[10px] text-amber-300/80 font-mono">
                  {t.refactorBlitzReward}
                </div>
              </div>
            </div>
            <span className="px-2.5 py-1 rounded-xl bg-amber-400 text-slate-950 text-[10px] font-black shrink-0 tracking-wider">
              ⏱️ {blitzTimeLeft}{t.secShort}
            </span>
          </div>
        )}

        {/* ШАГ 2: МНОГОСТРОЧНЫЙ IDE-РЕДАКТОР С ЧЕСТНОЙ ПОДСВЕТКОЙ СИНТАКСИСА И ЛИНТЕРОМ */}
        <div className="relative z-20 my-2 p-3 rounded-2xl bg-black/55 border border-slate-800/80 font-mono text-xs sm:text-[13px] space-y-1 overflow-hidden min-h-[105px] flex flex-col justify-center shadow-inner">
          {activeFile.codeLines.map((lineText, idx) => {
            const isCurrent = idx === lineIdx;
            const isPassed = idx < lineIdx;
            const tokens = tokenizeCodeLine(lineText);
            const hasLinterErrorOnLine = linterWarning && linterWarning.fileId === activeFile.id && linterWarning.lineIdx === idx;

            return (
              <div 
                key={idx} 
                className={`flex items-center gap-2.5 transition-opacity ${
                  isCurrent ? 'opacity-100' : isPassed ? 'opacity-70' : 'opacity-40'
                }`}
              >
                <span className="text-slate-600 select-none text-[10px] sm:text-xs w-4 text-right">
                  {idx + 1}
                </span>
                <div className="flex items-center flex-wrap truncate">
                  {tokens.map((tok, tIdx) => {
                    const isErrorToken = hasLinterErrorOnLine && linterWarning.tokenIdx === tIdx;

                    if (isErrorToken) {
                      return (
                        <span
                          key={tIdx}
                          onClick={handleFixLinter}
                          title={`${linterWarning.message} ${t.quickFixTooltip}`}
                          className="relative inline-flex items-center group cursor-pointer z-30 mx-0.5"
                        >
                          <span className="underline decoration-wavy decoration-red-500 decoration-2 underline-offset-4 text-red-400 bg-red-500/20 px-1 py-0.5 rounded font-bold animate-pulse hover:bg-red-500/35 transition-all">
                            {linterWarning.errorText}
                          </span>
                          {/* VS Code Quick Fix Lightbulb Tooltip */}
                          <span className="absolute -top-7 left-0 z-40 hidden group-hover:flex items-center gap-1 px-2 py-0.5 rounded-lg bg-amber-400 text-slate-950 text-[10px] font-bold shadow-lg whitespace-nowrap animate-bounce">
                            {t.quickFixButton}
                          </span>
                        </span>
                      );
                    }

                    return (
                      <span
                        key={tIdx}
                        style={{ color: getTokenColor(tok, currentTheme) }}
                        className="whitespace-pre font-medium"
                      >
                        {tok.text}
                      </span>
                    );
                  })}
                  {isCurrent && (
                    <span 
                      style={{ backgroundColor: currentTheme.syntaxFunction }} 
                      className="ml-1 w-2 h-3.5 animate-pulse inline-block" 
                    />
                  )}
                  {/* Кнопка Quick Fix в строке с ошибкой линтера */}
                  {hasLinterErrorOnLine && (
                    <button
                      onClick={handleFixLinter}
                      className="ml-2 px-1.5 py-0.5 rounded bg-red-500/25 hover:bg-red-500/40 border border-red-500/60 text-[10px] text-red-200 font-mono inline-flex items-center gap-1 animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.5)] cursor-pointer"
                      title={t.quickFixTooltipAttr}
                    >
                      <span>💡 Fix ({linterWarning.timeLeft}{t.secShort})</span>
                    </button>
                  )}
                  {/* Кнопка рефакторинга целевой строки в мини-игре Refactor Blitz */}
                  {blitzGameActive && idx < 5 && (
                    <button
                      onClick={(e) => handleRefactorLine(e, idx)}
                      className={`ml-2 px-1.5 py-0.5 rounded text-[10px] font-mono inline-flex items-center gap-1 transition-all cursor-pointer ${
                        refactoredLineIndices.includes(idx)
                          ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/50 line-through opacity-70'
                          : 'bg-amber-500/30 text-amber-200 border border-amber-400 font-bold animate-bounce shadow-[0_0_8px_rgba(245,158,11,0.6)] hover:bg-amber-400 hover:text-black'
                      }`}
                    >
                      {refactoredLineIndices.includes(idx) ? t.refactoredLine : REFACTOR_TAGS[idx]}
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {/* ШАГ 4: ТАКТИЛЬНАЯ КНОПКА КОМПИЛЯЦИИ С БЕГУЩИМ ЛОГОМ СБОРКИ */}
        <div className="relative z-20 w-full mt-1">
          <div className={`w-full py-3 sm:py-3.5 rounded-2xl border text-center font-mono font-bold tracking-wider text-xs sm:text-sm transition-all duration-75 flex flex-col items-center justify-center gap-0.5 shadow-inner ${
            activeKey
              ? `${currentTheme.btnActiveBg} translate-y-1`
              : isInFlow
              ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-[0_4px_0_#b45309]'
              : `${currentTheme.btnBg} ${currentTheme.btnText} ${currentTheme.btnBorder} ${currentTheme.btnShadow}`
          }`}>
            <div className="flex items-center gap-1.5">
              <span>{t.compileBtn}</span>
              {isInFlow && <Sparkles className="w-3.5 h-3.5 animate-spin text-slate-950" />}
            </div>
            {/* Микро-лог компиляции */}
            <span className="text-[10px] font-normal opacity-80 tracking-normal font-mono truncate max-w-[90%]">
              {compileLog}
            </span>
          </div>
        </div>

        {/* ШАГ 5: CI/CD PIPELINE & GITHUB ACTIONS */}
        <div className="relative z-20 w-full">
          <CICDPipeline />
        </div>

        {/* ШАГ 3: СТАТУСНАЯ СТРОКА IDE (IDE Status Bar) */}
        <div className="relative z-20 mt-2.5 pt-2 border-t border-slate-800/80 flex items-center justify-between text-[10px] font-mono text-slate-400 select-none flex-wrap gap-1">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="flex items-center gap-1 text-slate-300 hover:text-cyan-400 transition-colors">
              <GitBranch className="w-3 h-3 text-cyan-400" />
              <span className={gitBranch !== 'main' ? 'text-amber-300 font-semibold' : ''}>
                {gitBranch !== 'main' ? gitBranch : gitStatus}
              </span>
            </span>

            {/* Git Branch Management & PR Merge */}
            {gitBranch !== 'main' ? (
              <div className="flex items-center gap-1">
                <span className="text-[9px] px-1.5 py-0.2 rounded bg-amber-500/20 text-amber-300 border border-amber-500/40 font-mono">
                  +25% LOC ({Math.floor(branchCodeLines)})
                </span>
                <button
                  onClick={handleMergePR}
                  disabled={branchCodeLines < 1}
                  className={`px-1.5 py-0.5 rounded border text-[9px] flex items-center gap-0.5 transition-all cursor-pointer ${
                    branchCodeLines >= 1
                      ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/60 hover:bg-emerald-500/30 font-bold animate-pulse shadow-[0_0_8px_rgba(16,185,129,0.4)]'
                      : 'bg-slate-800 text-slate-500 border-slate-700 cursor-not-allowed'
                  }`}
                  title={t.mergePrTooltip}
                >
                  <GitMerge className="w-2.5 h-2.5" />
                  <span>Merge PR</span>
                </button>
              </div>
            ) : (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  createBranch();
                }}
                className="px-1.5 py-0.5 rounded bg-cyan-950/80 hover:bg-cyan-900 border border-cyan-700/50 text-[9px] text-cyan-300 flex items-center gap-0.5 transition-colors cursor-pointer"
                title={t.createBranchTooltip}
              >
                <span>+Branch</span>
              </button>
            )}

            {/* Кнопка вызова палитры команд (Command Palette) */}
            <button
              onClick={(e) => {
                e.stopPropagation();
                setIsCommandPaletteOpen(true);
              }}
              className="px-1.5 py-0.5 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-[9px] text-slate-300 hover:text-cyan-300 flex items-center gap-1 transition-colors cursor-pointer"
              title={t.cmdPaletteTooltip}
            >
              <Terminal className="w-2.5 h-2.5 text-cyan-400" />
              <span>Palette</span>
            </button>

            {/* Кнопка вызова GitHub Heatmap */}
            <button
              onClick={(e) => {
                e.stopPropagation();
                window.dispatchEvent(new CustomEvent('codetap_open_heatmap'));
              }}
              className="px-1.5 py-0.5 rounded bg-emerald-950/80 hover:bg-emerald-900 border border-emerald-700/50 text-[9px] text-emerald-300 flex items-center gap-1 transition-colors cursor-pointer"
              title={t.heatmapTooltip}
            >
              <span>🐙</span>
              <span>Heatmap</span>
              {currentStreak > 0 && <span className="text-amber-400 font-bold">{currentStreak}🔥</span>}
            </button>

            {linterWarning ? (
              <button
                onClick={handleFixLinter}
                className="flex items-center gap-1 text-red-400 hover:text-red-300 animate-pulse font-bold cursor-pointer"
                title={t.linterErrorTooltip}
              >
                <AlertTriangle className="w-3 h-3 text-red-400" />
                <span>1 error (Fix)</span>
              </button>
            ) : (
              <span className="hidden sm:flex items-center gap-1 text-emerald-400">
                <CheckCircle2 className="w-3 h-3" />
                <span>0 errors</span>
              </span>
            )}
          </div>

          <div className="flex items-center gap-2.5">
            <span className="text-slate-500">Ln {lineIdx + 1}, Col {(lineIdx + 1) * 14}</span>
            <span className="hidden sm:inline text-slate-500">UTF-8</span>
            <span className={`px-1.5 py-0.2 rounded bg-slate-800/80 border border-slate-700/60 font-semibold ${activeFile.langColor}`}>
              {activeFile.language}
            </span>
          </div>
        </div>

        {/* Парящие частицы кода и комбо */}
        {popups.map(p => (
          <div
            key={p.id}
            style={{ left: `${p.x}px`, top: `${p.y}px` }}
            className={`absolute float-popup font-mono pointer-events-none select-none flex flex-col items-center z-40 ${
              p.isCrit 
                ? 'text-amber-400 drop-shadow-[0_0_10px_rgba(245,158,11,0.9)]' 
                : 'text-cyan-300 drop-shadow-[0_0_8px_rgba(6,182,212,0.8)]'
            }`}
          >
            <span className="text-sm font-black">{p.text}</span>
            <span className="text-[10px] font-semibold opacity-85 px-1.5 py-0.2 rounded bg-black/60 border border-slate-700/60">
              {p.tag}
            </span>
          </div>
        ))}
      </div>

      {/* СЕЛЕКТОР ПРОФИЛЕЙ СВИТЧЕЙ КЛАВИАТУРЫ */}
      <div className="w-full mt-2.5 flex items-center justify-between gap-1 p-1.5 rounded-2xl bg-slate-900/80 border border-slate-800 text-[11px] font-mono">
        <span className="text-slate-400 text-[10px] px-1.5 flex items-center gap-1 shrink-0">
          <Volume2 className="w-3.5 h-3.5 text-cyan-400" />
          {t.switchesHeader}
        </span>
        <div className="flex items-center gap-1 overflow-x-auto no-scrollbar">
          {SWITCH_OPTIONS.map(sw => {
            const isSelected = switchType === sw.id;
            const isThemePreset = currentTheme.soundPreset === sw.id;
            return (
              <button
                key={sw.id}
                onClick={(e) => {
                  e.stopPropagation();
                  setSwitchType(sw.id);
                }}
                className={`px-2 py-1 rounded-xl transition-all flex items-center gap-1 font-semibold whitespace-nowrap cursor-pointer ${
                  isSelected 
                    ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-400/50 shadow-[0_0_10px_rgba(6,182,212,0.3)]' 
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60 border border-transparent'
                }`}
                title={`${sw.name} - ${sw.label}${isThemePreset ? ` (${t.themePreset})` : ''}`}
              >
                <span>{sw.icon}</span>
                <span>{sw.name}</span>
                {isThemePreset && (
                  <span className="text-[9px] px-1 py-0.2 rounded bg-cyan-950/80 text-cyan-400 border border-cyan-800/60 font-mono">
                    {t.themeBadge}
                  </span>
                )}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};
