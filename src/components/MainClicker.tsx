import React, { useState, useEffect } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Flame, Sparkles, Terminal, Cpu, Zap, Bug } from 'lucide-react';
import confetti from 'canvas-confetti';
import { IDE_THEMES } from '../utils/themesList';
import { StudioDecor } from './StudioDecor';
import { sounds } from '../utils/soundEffects';

interface Popup {
  id: number;
  text: string;
  x: number;
  y: number;
  isCrit: boolean;
}

interface CodeBug {
  id: string;
  title: string;
  timeLeft: number;
}

const BUG_TYPES = [
  'NullReferenceException: Object not set at Line 42',
  'Deadlock: ThreadPool exhaustion in Worker #3',
  'IndexOutOfRangeException: Array buffer overflow',
  'SyntaxError: Missing semicolon in production build'
];

const CODE_SNIPPETS = [
  'async function deployProduction() {',
  '  await neuralCore.train(weights);',
  '  const release = await git.commit();',
  '  quantumEngine.optimize(threads);',
  '  return server.publish("v2.0.0");',
  '}'
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
    overclockRemainingSec,
    triggerOverclock,
    unlockedSkills,
    t 
  } = useGame();
  const currentTheme = IDE_THEMES[themeId] || IDE_THEMES['cyberpunk'];
  const [popups, setPopups] = useState<Popup[]>([]);
  const [activeKey, setActiveKey] = useState<boolean>(false);
  const [snippetIndex, setSnippetIndex] = useState<number>(0);
  const [activeBug, setActiveBug] = useState<CodeBug | null>(null);

  const onPointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const { isCrit, codeAdded } = handleClick(e.clientX, e.clientY);
    setActiveKey(true);
    setSnippetIndex(i => (i + 1) % CODE_SNIPPETS.length);

    if (isCrit) {
      confetti({
        particleCount: 25,
        spread: 45,
        origin: { x: e.clientX / window.innerWidth, y: e.clientY / window.innerHeight },
        colors: ['#F59E0B', '#10B981', '#06B6D4']
      });
    }

    const newPopup: Popup = {
      id: Date.now() + Math.random(),
      text: isCrit ? `${t.critText} +${formatNumber(codeAdded)}` : `+${formatNumber(codeAdded)}`,
      x,
      y,
      isCrit
    };

    setPopups(prev => [...prev.slice(-15), newPopup]);

    setTimeout(() => {
      setPopups(prev => prev.filter(p => p.id !== newPopup.id));
    }, 850);
  };

  // Периодический спавн багов (раз в 45-60 секунд)
  useEffect(() => {
    const interval = setInterval(() => {
      setActiveBug(prev => {
        if (prev) return prev;
        const randomTitle = BUG_TYPES[Math.floor(Math.random() * BUG_TYPES.length)];
        return {
          id: `bug_${Date.now()}`,
          title: randomTitle,
          timeLeft: 7
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

  const handleFixBug = (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!activeBug) return;

    const bountyMultiplier = 1.0 + (unlockedSkills['skill_bug_bounty_hunter'] || 0) * 0.50;
    const bonusCode = Math.round(Math.max(500, codePerSec * 40) * bountyMultiplier);

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
      text: `🐞 БАГ ИСПРАВЛЕН! +${formatNumber(bonusCode)} C# [OVERCLOCK x3.0]`,
      x: 80,
      y: 80,
      isCrit: true
    };
    setPopups(prev => [...prev.slice(-15), newPopup]);
    setActiveBug(null);
  };

  const onPointerUp = () => {
    setActiveKey(false);
  };

  return (
    <div className="flex flex-col items-center justify-center p-4 w-full max-w-md mx-auto">
      {/* Интерактивный декор и питомцы студии */}
      <StudioDecor />

      {/* Шкала Комбо «В Потоке» и индикатор Overclock */}
      <div className="w-full mb-3">
        <div className="flex items-center justify-between text-xs font-mono mb-1.5 px-1">
          <div className="flex items-center gap-1.5 text-slate-300">
            <Flame className={`w-3.5 h-3.5 ${isInFlow ? 'text-amber-400 animate-bounce' : 'text-slate-500'}`} />
            <span className={isInFlow ? 'text-amber-400 font-bold' : ''}>
              {isInFlow ? t.flowMode : t.focusBar}
            </span>

            {overclockRemainingSec > 0 && (
              <span className="px-2 py-0.5 rounded-full bg-red-500/20 text-red-400 border border-red-500/40 text-[10px] font-bold animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.4)]">
                🔥 OVERCLOCK x3.0 ({overclockRemainingSec}с)
              </span>
            )}
          </div>
          <span className="text-slate-400">{(comboEnergy * 100).toFixed(0)}%</span>
        </div>
        <div className="h-2.5 w-full bg-slate-900 rounded-full overflow-hidden p-0.5 border border-slate-800">
          <div
            className={`h-full rounded-full transition-all duration-100 ${
              overclockRemainingSec > 0
                ? 'bg-gradient-to-r from-red-500 via-orange-500 to-amber-400 shadow-[0_0_12px_rgba(239,68,68,0.8)] animate-pulse'
                : isInFlow
                ? 'bg-gradient-to-r from-amber-500 via-orange-500 to-yellow-400 shadow-[0_0_10px_rgba(245,158,11,0.7)] animate-pulse'
                : 'bg-gradient-to-r from-cyan-500 to-blue-500'
            }`}
            style={{ width: `${Math.min(100, comboEnergy * 100)}%` }}
          />
        </div>
      </div>

      {/* Интерактивная Клавиатура / IDE Терминал */}
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
        className={`relative w-full aspect-[4/3] rounded-3xl p-6 flex flex-col justify-between cursor-pointer select-none transition-all duration-75 overflow-hidden border ${
          activeKey
            ? `scale-[0.98] ${currentTheme.terminalBorder} bg-gradient-to-b ${currentTheme.terminalBg}`
            : isInFlow
            ? 'border-amber-500/60 shadow-[0_0_35px_rgba(245,158,11,0.3)] bg-gradient-to-b from-slate-900 to-slate-950'
            : `${currentTheme.terminalBorder} bg-gradient-to-b ${currentTheme.terminalBg}`
        }`}
      >
        {/* Ретро CRT Scanlines оверлей */}
        {currentTheme.crtScanline && (
          <div className="absolute inset-0 pointer-events-none bg-[linear-gradient(rgba(18,16,16,0)_50%,rgba(0,0,0,0.35)_50%)] bg-[length:100%_4px] opacity-40 z-10" />
        )}

        {/* Верхняя строка терминала */}
        <div className="relative z-20 flex items-center justify-between border-b border-slate-800/80 pb-3">
          <div className="flex items-center gap-2">
            <div className="flex gap-1.5">
              <div className="w-2.5 h-2.5 rounded-full bg-red-500/80" />
              <div className="w-2.5 h-2.5 rounded-full bg-amber-500/80" />
              <div className="w-2.5 h-2.5 rounded-full bg-emerald-500/80" />
            </div>
            <span className="text-xs font-mono text-slate-400 ml-2 flex items-center gap-1">
              <Terminal className="w-3.5 h-3.5 text-cyan-400" />
              StudioEditor.cs
            </span>
          </div>

          <div className="flex items-center gap-2">
            {hasAutoClicker && (
              <div className="flex items-center gap-1 text-[10px] font-mono text-cyan-300 bg-cyan-500/15 px-2 py-0.5 rounded-full border border-cyan-500/40 shadow-[0_0_8px_rgba(6,182,212,0.3)] animate-pulse">
                <Zap className="w-3 h-3 text-cyan-400" />
                <span>10 CPS</span>
              </div>
            )}
            <div className="flex items-center gap-1.5 text-[11px] font-mono text-cyan-400/80 bg-cyan-950/40 px-2 py-0.5 rounded-md border border-cyan-800/30">
              <Cpu className="w-3 h-3" />
              <span>+{formatNumber(codePerClick)} C#/клик</span>
            </div>
          </div>
        </div>

        {/* ОХОТА НА БАГИ: Интерактивная строка бага */}
        {activeBug && (
          <button
            onClick={handleFixBug}
            className="relative z-30 w-full p-2.5 my-1 rounded-2xl bg-red-950/90 border-2 border-red-500 shadow-[0_0_20px_rgba(239,68,68,0.7)] text-left flex items-center justify-between gap-2 animate-bounce cursor-pointer"
          >
            <div className="flex items-center gap-2 overflow-hidden">
              <Bug className="w-4 h-4 text-red-400 animate-spin shrink-0" />
              <span className="text-xs font-bold text-red-200 truncate font-mono">
                {activeBug.title}
              </span>
            </div>
            <span className="px-2.5 py-1 rounded-xl bg-red-500 text-black text-[10px] font-black shrink-0 tracking-wider">
              ДЕБАЖИТЬ! ({activeBug.timeLeft}с)
            </span>
          </button>
        )}

        {/* Кодовая строка / Визуализатор IDE */}
        <div className="relative z-20 my-auto py-3 font-mono text-sm sm:text-base space-y-1.5">
          <div className={`${currentTheme.codeColor} font-semibold flex items-center gap-2`}>
            <span className={currentTheme.promptColor}>&gt;</span>
            <span>{CODE_SNIPPETS[snippetIndex]}</span>
          </div>
          <div className="text-xs text-slate-500 italic">
            {t.clickInstruction}
          </div>
        </div>

        {/* Большая кнопка пробела / свитча */}
        <div className={`relative z-20 w-full py-3.5 rounded-2xl border text-center font-mono font-bold tracking-wider text-sm transition-all duration-75 flex items-center justify-center gap-2 shadow-inner ${
          activeKey
            ? `${currentTheme.btnActiveBg} translate-y-1`
            : isInFlow
            ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-[0_4px_0_#b45309]'
            : `${currentTheme.btnBg} ${currentTheme.btnText} ${currentTheme.btnBorder} ${currentTheme.btnShadow}`
        }`}>
          <span>{t.compileBtn}</span>
          {isInFlow && <Sparkles className="w-4 h-4 animate-spin" />}
        </div>

        {/* Всплывающие партиклы урона/кода */}
        {popups.map(p => (
          <div
            key={p.id}
            style={{ left: `${p.x}px`, top: `${p.y}px` }}
            className={`absolute float-popup font-mono font-bold text-sm pointer-events-none select-none ${
              p.isCrit ? 'text-amber-400 text-base drop-shadow-[0_0_8px_rgba(245,158,11,0.8)]' : 'text-cyan-300 drop-shadow-[0_0_6px_rgba(6,182,212,0.8)]'
            }`}
          >
            {p.text}
          </div>
        ))}
      </div>
    </div>
  );
};
