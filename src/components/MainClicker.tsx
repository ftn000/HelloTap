import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Flame, Sparkles, Terminal, Cpu } from 'lucide-react';
import confetti from 'canvas-confetti';

interface Popup {
  id: number;
  text: string;
  x: number;
  y: number;
  isCrit: boolean;
}

const CODE_SNIPPETS = [
  'async function deployProduction() {',
  '  await neuralCore.train(weights);',
  '  const release = await git.commit();',
  '  quantumEngine.optimize(threads);',
  '  return server.publish("v2.0.0");',
  '}'
];

export const MainClicker: React.FC = () => {
  const { handleClick, comboEnergy, isInFlow, codePerClick, t } = useGame();
  const [popups, setPopups] = useState<Popup[]>([]);
  const [activeKey, setActiveKey] = useState<boolean>(false);
  const [snippetIndex, setSnippetIndex] = useState<number>(0);

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

  const onPointerUp = () => {
    setActiveKey(false);
  };

  return (
    <div className="flex flex-col items-center justify-center p-4 w-full max-w-md mx-auto">
      {/* Шкала Комбо «В Потоке» */}
      <div className="w-full mb-3">
        <div className="flex items-center justify-between text-xs font-mono mb-1.5 px-1">
          <div className="flex items-center gap-1 text-slate-300">
            <Flame className={`w-3.5 h-3.5 ${isInFlow ? 'text-amber-400 animate-bounce' : 'text-slate-500'}`} />
            <span className={isInFlow ? 'text-amber-400 font-bold' : ''}>
              {isInFlow ? t.flowMode : t.focusBar}
            </span>
          </div>
          <span className="text-slate-400">{(comboEnergy * 100).toFixed(0)}%</span>
        </div>
        <div className="h-2.5 w-full bg-slate-900 rounded-full overflow-hidden p-0.5 border border-slate-800">
          <div
            className={`h-full rounded-full transition-all duration-100 ${
              isInFlow
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
        className={`relative w-full aspect-[4/3] rounded-3xl p-6 flex flex-col justify-between cursor-pointer select-none transition-all duration-75 overflow-hidden border ${
          activeKey
            ? 'scale-[0.98] border-cyan-400 shadow-[0_0_30px_rgba(6,182,212,0.5)] bg-slate-900/90'
            : isInFlow
            ? 'border-amber-500/60 shadow-[0_0_35px_rgba(245,158,11,0.3)] bg-gradient-to-b from-slate-900 to-slate-950'
            : 'border-slate-800 shadow-[0_10px_30px_rgba(0,0,0,0.5)] bg-gradient-to-b from-slate-900/90 to-slate-950/90 hover:border-slate-700'
        }`}
      >
        {/* Верхняя строка терминала */}
        <div className="flex items-center justify-between border-b border-slate-800/80 pb-3">
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

          <div className="flex items-center gap-1.5 text-[11px] font-mono text-cyan-400/80 bg-cyan-950/40 px-2 py-0.5 rounded-md border border-cyan-800/30">
            <Cpu className="w-3 h-3" />
            <span>+{formatNumber(codePerClick)} C#/клик</span>
          </div>
        </div>

        {/* Кодовая строка / Визуализатор IDE */}
        <div className="my-auto py-4 font-mono text-sm sm:text-base space-y-1.5">
          <div className="text-cyan-400 font-semibold flex items-center gap-2">
            <span className="text-emerald-400">&gt;</span>
            <span>{CODE_SNIPPETS[snippetIndex]}</span>
          </div>
          <div className="text-xs text-slate-500 italic">
            {t.clickInstruction}
          </div>
        </div>

        {/* Большая кнопка пробела / свитча */}
        <div className={`w-full py-3.5 rounded-2xl border text-center font-mono font-bold tracking-wider text-sm transition-all duration-75 flex items-center justify-center gap-2 shadow-inner ${
          activeKey
            ? 'bg-cyan-500 text-slate-950 border-cyan-300 translate-y-1'
            : isInFlow
            ? 'bg-amber-500 text-slate-950 border-amber-400 shadow-[0_4px_0_#b45309]'
            : 'bg-slate-800/80 text-cyan-300 border-slate-700 shadow-[0_4px_0_#1e293b] hover:bg-slate-800'
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
