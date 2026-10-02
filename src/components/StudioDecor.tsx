import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { Coffee, Server, Heart } from 'lucide-react';
import { sounds } from '../utils/soundEffects';

export const StudioDecor: React.FC = () => {
  const { systems, isInFlow, t } = useGame();
  const catLevel = systems.find(s => s.id === 'sys_cathaven')?.level || 0;
  const serverLevel = systems.find(s => s.id === 'sys_satellite')?.level || 0;

  const [catPurring, setCatPurring] = useState<boolean>(false);
  const [coffeeSips, setCoffeeSips] = useState<number>(0);

  const handleCatClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    setCatPurring(true);
    sounds.playKeyClick(true);
    sounds.triggerHaptic('light');
    setTimeout(() => setCatPurring(false), 1500);
  };

  const handleCoffeeClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    setCoffeeSips(s => s + 1);
    sounds.playUpgrade();
    sounds.triggerHaptic('medium');
  };

  return (
    <div className="w-full flex items-center justify-between px-3 py-1.5 mb-2 bg-slate-900/60 border border-slate-800/80 rounded-2xl backdrop-blur-sm text-xs font-mono select-none">
      {/* СЕРВЕРНАЯ СТОЙКА СО СВЕТОДИОДАМИ */}
      <div className="flex items-center gap-2 text-slate-400">
        <Server className="w-4 h-4 text-cyan-400 animate-pulse" />
        <div className="flex items-center gap-1">
          <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-ping" />
          <span className="w-1.5 h-1.5 rounded-full bg-cyan-400" />
          <span className={`w-1.5 h-1.5 rounded-full ${isInFlow ? 'bg-amber-400 animate-pulse' : 'bg-slate-700'}`} />
          <span className="text-[10px] text-slate-400 ml-1">
            {serverLevel > 0 ? `SVR: ONLINE (${serverLevel}x)` : 'LOCAL NODE'}
          </span>
        </div>
      </div>

      {/* ЦЕНТР / ПИТОМЕЦ: РЫЖИЙ КОТИК */}
      <div className="relative flex items-center">
        {catLevel > 0 ? (
          <button
            onClick={handleCatClick}
            className="group relative flex items-center gap-1.5 px-2.5 py-1 rounded-xl bg-amber-950/30 border border-amber-500/30 hover:border-amber-400 transition-all active:scale-95"
            title={t.catTitle}
          >
            <span className="text-base group-hover:scale-110 transition-transform">🐱</span>
            <span className="text-[11px] font-bold text-amber-300">
              {catPurring ? t.catPurr : t.catName}
            </span>
            {catPurring && (
              <Heart className="absolute -top-3 right-0 w-3.5 h-3.5 text-pink-400 animate-bounce" />
            )}
          </button>
        ) : (
          <div className="text-[10px] text-slate-500 italic">
            {t.catSleeping}
          </div>
        )}
      </div>

      {/* КРУЖКА ГОРЯЧЕГО КОФЕ */}
      <button
        onClick={handleCoffeeClick}
        className="flex items-center gap-1 px-2 py-1 rounded-xl bg-slate-800/50 hover:bg-slate-800 border border-slate-700 text-slate-300 hover:text-amber-300 transition-all active:scale-95"
        title={t.coffeeTooltip}
      >
        <Coffee className={`w-3.5 h-3.5 ${isInFlow ? 'text-amber-400 animate-bounce' : 'text-slate-400'}`} />
        <span className="text-[10px] text-slate-400">
          {coffeeSips > 0 ? `x${coffeeSips}` : '100%'}
        </span>
      </button>
    </div>
  );
};
