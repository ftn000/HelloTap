import React from 'react';
import { useGame } from '../context/GameContext';
import { Terminal, GitBranch, Award, Trophy, Building2 } from 'lucide-react';

interface BottomNavProps {
  onOpenHub: () => void;
  onOpenAchievements: () => void;
  onOpenTechTree: () => void;
  onOpenLeaderboard: () => void;
}

export const BottomNav: React.FC<BottomNavProps> = ({
  onOpenHub,
  onOpenAchievements,
  onOpenTechTree,
  onOpenLeaderboard,
}) => {
  const { lang, achievements, skillPoints } = useGame();

  const totalAchStars = Object.values(achievements).reduce((sum, tier) => sum + tier, 0);

  const scrollToCode = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <nav className="sm:hidden fixed bottom-0 left-0 right-0 z-40 bg-slate-950/95 backdrop-blur-xl border-t border-slate-800/80 px-2 py-1.5 shadow-[0_-4px_25px_rgba(0,0,0,0.6)]">
      <div className="flex items-center justify-around max-w-md mx-auto">
        {/* Кнопка Код / Наверх */}
        <button
          onClick={scrollToCode}
          className="flex flex-col items-center justify-center py-1 px-2.5 rounded-xl text-slate-300 hover:text-cyan-400 active:scale-95 transition-all group"
        >
          <div className="p-1 rounded-lg bg-cyan-500/10 group-hover:bg-cyan-500/20 text-cyan-400 border border-cyan-500/20">
            <Terminal className="w-4 h-4" />
          </div>
          <span className="text-[10px] font-mono font-medium mt-1 text-slate-400 group-hover:text-cyan-300">
            {lang === 'ru' ? 'Код' : lang === 'tr' ? 'Kod' : 'Code'}
          </span>
        </button>

        {/* Кнопка Навыки (Tech Tree) */}
        <button
          onClick={onOpenTechTree}
          className="relative flex flex-col items-center justify-center py-1 px-2.5 rounded-xl text-slate-300 hover:text-indigo-400 active:scale-95 transition-all group"
        >
          <div className={`p-1 rounded-lg border transition-all ${
            skillPoints > 0
              ? 'bg-indigo-500/25 border-indigo-500/60 text-indigo-300 shadow-[0_0_12px_rgba(99,102,241,0.4)] animate-pulse'
              : 'bg-indigo-500/10 border-indigo-500/20 text-indigo-400 group-hover:bg-indigo-500/20'
          }`}>
            <GitBranch className="w-4 h-4" />
          </div>
          {skillPoints > 0 && (
            <span className="absolute top-0.5 right-2 px-1 min-w-[15px] h-3.5 rounded-full bg-indigo-500 text-white font-mono font-bold text-[9px] flex items-center justify-center shadow">
              {skillPoints}
            </span>
          )}
          <span className="text-[10px] font-mono font-medium mt-1 text-slate-400 group-hover:text-indigo-300">
            {lang === 'ru' ? 'Навыки' : lang === 'tr' ? 'Yetenekler' : 'Skills'}
          </span>
        </button>

        {/* Кнопка Ачивки */}
        <button
          onClick={onOpenAchievements}
          className="relative flex flex-col items-center justify-center py-1 px-2.5 rounded-xl text-slate-300 hover:text-amber-400 active:scale-95 transition-all group"
        >
          <div className="p-1 rounded-lg bg-amber-500/10 group-hover:bg-amber-500/20 text-amber-400 border border-amber-500/20">
            <Award className="w-4 h-4" />
          </div>
          {totalAchStars > 0 && (
            <span className="absolute top-0.5 right-2 px-1 min-w-[15px] h-3.5 rounded-full bg-amber-500 text-slate-950 font-mono font-bold text-[9px] flex items-center justify-center shadow">
              {totalAchStars}
            </span>
          )}
          <span className="text-[10px] font-mono font-medium mt-1 text-slate-400 group-hover:text-amber-300">
            {lang === 'ru' ? 'Ачивки' : lang === 'tr' ? 'Başarımlar' : 'Badges'}
          </span>
        </button>

        {/* Кнопка Лидерборд */}
        <button
          onClick={onOpenLeaderboard}
          className="flex flex-col items-center justify-center py-1 px-2.5 rounded-xl text-slate-300 hover:text-amber-400 active:scale-95 transition-all group"
        >
          <div className="p-1 rounded-lg bg-amber-500/10 group-hover:bg-amber-500/20 text-amber-400 border border-amber-500/20">
            <Trophy className="w-4 h-4" />
          </div>
          <span className="text-[10px] font-mono font-medium mt-1 text-slate-400 group-hover:text-amber-300">
            {lang === 'ru' ? 'Топ' : lang === 'tr' ? 'Sıralama' : 'Ranks'}
          </span>
        </button>

        {/* Кнопка Hub Студии */}
        <button
          onClick={onOpenHub}
          className="flex flex-col items-center justify-center py-1 px-2.5 rounded-xl text-white active:scale-95 transition-all group"
        >
          <div className="p-1 rounded-lg bg-gradient-to-r from-cyan-500 to-blue-600 text-white shadow-[0_0_12px_rgba(6,182,212,0.35)] group-hover:from-cyan-400 group-hover:to-blue-500">
            <Building2 className="w-4 h-4" />
          </div>
          <span className="text-[10px] font-mono font-bold mt-1 text-cyan-300">
            {lang === 'ru' ? 'Хаб OS' : lang === 'tr' ? 'Merkez OS' : 'Hub OS'}
          </span>
        </button>
      </div>
    </nav>
  );
};
