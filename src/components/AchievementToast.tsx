import React from 'react';
import { Award, Star } from 'lucide-react';

export interface ToastItem {
  id: string;
  icon: string;
  title: string;
  tier: 1 | 2 | 3;
  rewardDesc: string;
}

interface AchievementToastProps {
  toasts: ToastItem[];
  onDismiss: (id: string) => void;
}

export const AchievementToast: React.FC<AchievementToastProps> = ({ toasts, onDismiss }) => {
  if (toasts.length === 0) return null;

  return (
    <div className="fixed top-16 right-4 z-50 flex flex-col gap-2 pointer-events-none max-w-sm w-full">
      {toasts.map(toast => {
        const isMaster = toast.tier === 3;

        return (
          <div
            key={toast.id}
            onClick={() => onDismiss(toast.id)}
            className={`pointer-events-auto cursor-pointer p-3.5 rounded-2xl border shadow-2xl backdrop-blur-md flex items-center gap-3 transition-all animate-in slide-in-from-right-5 fade-in duration-200 ${
              isMaster
                ? 'bg-gradient-to-r from-amber-950/90 via-slate-900/95 to-slate-900/95 border-amber-400/70 shadow-[0_0_20px_rgba(245,158,11,0.4)]'
                : 'bg-slate-900/95 border-cyan-500/50 shadow-[0_0_15px_rgba(6,182,212,0.3)]'
            }`}
          >
            <div className="text-3xl p-2 rounded-xl bg-slate-800/80 border border-slate-700/60 shrink-0">
              {toast.icon}
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-1.5 mb-0.5">
                <span className="text-[10px] uppercase font-mono font-bold px-1.5 py-0.5 rounded bg-amber-500/20 text-amber-300 border border-amber-500/30 flex items-center gap-1">
                  <Award className="w-3 h-3" />
                  {isMaster ? '★ ★ ★ ЗАКРЫТО' : `УРОВЕНЬ ${toast.tier}`}
                </span>
              </div>
              <div className="text-xs sm:text-sm font-bold text-slate-100 truncate font-mono">
                {toast.title}
              </div>
              <div className="text-[11px] text-emerald-400 font-sans truncate">
                {toast.rewardDesc}
              </div>
            </div>

            <div className="flex items-center text-amber-400 text-xs shrink-0">
              {toast.tier === 1 && '★☆☆'}
              {toast.tier === 2 && '★★☆'}
              {toast.tier === 3 && '★★★'}
            </div>
          </div>
        );
      })}
    </div>
  );
};
