import React, { useState, useEffect, useRef } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Flame, Sparkles, Terminal, Cpu, Zap, Bug, Radio, Volume2 } from 'lucide-react';
import confetti from 'canvas-confetti';
import { IDE_THEMES } from '../utils/themesList';
import { StudioDecor } from './StudioDecor';

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

const CODE_BLOCKS = [
  [
    'import { neuralCore } from "@ai/tensor";',
    'const model = await neuralCore.load("gpt-quantum");',
    'const result = await model.generate({ stream: true });',
    'return <Game title="HelloTap CodeTap" active={true} />;'
  ],
  [
    'async function deployProduction() {',
    '  await git.commit("-m", "feat: hyper code v2.11");',
    '  const server = await k8s.scale({ replicas: 128 });',
    '  return server.publish("https://109.69.17.170/");',
  ],
  [
    'class QuantumCompiler extends Engine {',
    '  optimize(bytecode) { return bytecode.vectorize(); }',
    '  compile() { return WebAssembly.instantiate(this); }',
    '  run() { console.log("🚀 0 errors, 0 warnings!"); }'
  ],
  [
    'const developer = new IndieHacker({ coffee: 100 });',
    'while (developer.isCoding()) {',
    '  developer.writeLines(Math.floor(Math.random() * 50));',
    '  await developer.shipNextFeature();',
  ]
];

const SWITCH_OPTIONS = [
  { id: 'blue', name: 'Blue', label: 'Clicky', icon: '🔵' },
  { id: 'red', name: 'Red', label: 'Linear', icon: '🔴' },
  { id: 'brown', name: 'Brown', label: 'Tactile', icon: '🟤' },
  { id: 'laser', name: 'Laser', label: 'Cyber', icon: '⚡' },
  { id: 'typewriter', name: 'Typewriter', label: 'Vintage Ding', icon: '📜' },
] as const;

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
    switchType,
    setSwitchType,
    t 
  } = useGame();

  const currentTheme = IDE_THEMES[themeId] || IDE_THEMES['cyberpunk'];
  const [popups, setPopups] = useState<Popup[]>([]);
  const [activeKey, setActiveKey] = useState<boolean>(false);
  const [activeBug, setActiveBug] = useState<CodeBug | null>(null);

  // Живой IDE-терминал: блок кода и прогресс печати
  const [programIdx, setProgramIdx] = useState<number>(0);
  const [lineIdx, setLineIdx] = useState<number>(0);

  // Комбо-стрик по скорости клика
  const clickTimesRef = useRef<number[]>([]);
  const [currentCps, setCurrentCps] = useState<number>(0);

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

  const onPointerDown = (e: React.PointerEvent<HTMLDivElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    clickTimesRef.current.push(Date.now());

    // Базовый клик
    const { isCrit, codeAdded } = handleClick(e.clientX, e.clientY);
    setActiveKey(true);

    // Продвигаем строку живого кода в терминале
    setLineIdx(prevLine => {
      if (prevLine >= CODE_BLOCKS[programIdx].length - 1) {
        setProgramIdx(p => (p + 1) % CODE_BLOCKS.length);
        return 0;
      }
      return prevLine + 1;
    });

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
      text: `🐞 БАГ ИСПРАВЛЕН! +${formatNumber(bonusCode)} C# [OVERCLOCK x3.0]`,
      tag: 'HOTFIX',
      x: 80,
      y: 80,
      isCrit: true
    };
    setPopups(prev => [...prev.slice(-14), newPopup]);
    setActiveBug(null);
  };

  const onPointerUp = () => {
    setActiveKey(false);
  };

  const currentProgram = CODE_BLOCKS[programIdx];

  return (
    <div className="flex flex-col items-center justify-center p-3 sm:p-4 w-full max-w-md mx-auto">
      {/* Интерактивный декор и питомцы студии */}
      <StudioDecor />

      {/* Шкала Комбо «В Потоке», CPS Стрик и Overclock */}
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

        <div className="h-2.5 w-full bg-slate-900 rounded-full overflow-hidden p-0.5 border border-slate-800">
          <div
            className={`h-full rounded-full transition-all duration-100 ${
              overclockRemainingSec > 0
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
        className={`relative w-full rounded-3xl p-5 flex flex-col justify-between cursor-pointer select-none transition-all duration-75 overflow-hidden border ${
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

        {/* Верхняя строка терминала */}
        <div className="relative z-20 flex items-center justify-between border-b border-slate-800/80 pb-2.5">
          <div className="flex items-center gap-2">
            <div className="flex gap-1.5">
              <div className="w-2.5 h-2.5 rounded-full bg-red-500/80" />
              <div className="w-2.5 h-2.5 rounded-full bg-amber-500/80" />
              <div className="w-2.5 h-2.5 rounded-full bg-emerald-500/80" />
            </div>
            <span className="text-xs font-mono text-slate-400 ml-2 flex items-center gap-1">
              <Terminal className="w-3.5 h-3.5 text-cyan-400" />
              AppKernel.ts
            </span>
          </div>

          <div className="flex items-center gap-2">
            {hasAutoClicker && (
              <div className="flex items-center gap-1 text-[10px] font-mono text-cyan-300 bg-cyan-500/15 px-2 py-0.5 rounded-full border border-cyan-500/40 shadow-[0_0_8px_rgba(6,182,212,0.3)] animate-pulse">
                <Zap className="w-3 h-3 text-cyan-400" />
                <span>10 CPS</span>
              </div>
            )}
            <div className="flex items-center gap-1.5 text-[11px] font-mono text-cyan-400/90 bg-cyan-950/40 px-2 py-0.5 rounded-md border border-cyan-800/30">
              <Cpu className="w-3 h-3 text-cyan-400" />
              <span>+{formatNumber(codePerClick)} C#/клик</span>
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
              ДЕБАЖИТЬ! ({activeBug.timeLeft}с)
            </span>
          </button>
        )}

        {/* МНОГОСТРОЧНЫЙ ИНТЕРАКТИВНЫЙ IDE-РЕДАКТОР С НОМЕРАМИ СТРОК */}
        <div className="relative z-20 my-3.5 p-3 rounded-2xl bg-black/45 border border-slate-800/80 font-mono text-xs sm:text-sm space-y-1 overflow-hidden min-h-[96px] flex flex-col justify-center shadow-inner">
          {currentProgram.map((lineText, idx) => {
            const isCurrent = idx === lineIdx;
            const isPassed = idx < lineIdx;

            return (
              <div 
                key={idx} 
                className={`flex items-center gap-2.5 transition-opacity ${
                  isCurrent ? 'opacity-100' : isPassed ? 'opacity-70' : 'opacity-35'
                }`}
              >
                <span className="text-slate-600 select-none text-[10px] sm:text-xs w-4 text-right">
                  {idx + 1}
                </span>
                <div className="flex items-center truncate">
                  <span className={`${isCurrent ? currentTheme.codeColor : 'text-slate-300'} font-medium`}>
                    {lineText}
                  </span>
                  {isCurrent && (
                    <span className="ml-1 w-2 h-4 bg-cyan-400 animate-pulse inline-block" />
                  )}
                </div>
              </div>
            );
          })}
        </div>

        {/* Большая кнопка компиляции / свитча */}
        <div className={`relative z-20 w-full py-3.5 rounded-2xl border text-center font-mono font-bold tracking-wider text-sm transition-all duration-75 flex items-center justify-center gap-2 shadow-inner ${
          activeKey
            ? `${currentTheme.btnActiveBg} translate-y-1`
            : isInFlow
            ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-[0_4px_0_#b45309]'
            : `${currentTheme.btnBg} ${currentTheme.btnText} ${currentTheme.btnBorder} ${currentTheme.btnShadow}`
        }`}>
          <span>{t.compileBtn}</span>
          {isInFlow && <Sparkles className="w-4 h-4 animate-spin text-slate-950" />}
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
      <div className="w-full mt-3 flex items-center justify-between gap-1 p-1.5 rounded-2xl bg-slate-900/80 border border-slate-800 text-[11px] font-mono">
        <span className="text-slate-400 text-[10px] px-1.5 flex items-center gap-1 shrink-0">
          <Volume2 className="w-3.5 h-3.5 text-cyan-400" />
          Свитчи:
        </span>
        <div className="flex items-center gap-1 overflow-x-auto no-scrollbar">
          {SWITCH_OPTIONS.map(sw => {
            const isSelected = switchType === sw.id;
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
                title={`${sw.name} - ${sw.label}`}
              >
                <span>{sw.icon}</span>
                <span>{sw.name}</span>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};
