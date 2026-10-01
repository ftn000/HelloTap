import React from 'react';
import { Award, X } from 'lucide-react';

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

  // Показываем максимум 2 последних тоста одновременно, чтобы не перекрывать экран
  const visibleToasts = toasts.slice(-2);

  return (
    <div className="fixed top-14 left-1/2 -translate-x-1/2 w-[calc(100%-1.5rem)] max-w-sm sm:left-auto sm:right-4 sm:translate-x-0 z-50 flex flex-col gap-1.5 pointer-events-none">
      {visibleToasts.map(toast => {
        const isMaster = toast.tier === 3;

        return (
          <div
            key={toast.id}
            onClick={() => onDismiss(toast.id)}
            className={`pointer-events-auto cursor-pointer px-3 py-2 rounded-xl border shadow-2xl backdrop-blur-md flex items-center gap-2.5 transition-all animate-in slide-in-from-top-3 fade-in duration-200 ${
              isMaster
                ? 'bg-gradient-to-r from-amber-950/95 via-slate-900/95 to-slate-900/95 border-amber-400/80 shadow-[0_0_16px_rgba(245,158,11,0.35)]'
                : 'bg-slate-900/95 border-cyan-500/60 shadow-[0_0_14px_rgba(6,182,212,0.25)]'
            }`}
          >
            <div className="text-xl p-1.5 rounded-lg bg-slate-800/80 border border-slate-700/60 shrink-0">
              {toast.icon}
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-1.5">
                <span className="text-[9px] uppercase font-mono font-bold px-1 py-0.2 rounded bg-amber-500/20 text-amber-300 border border-amber-500/30 flex items-center gap-0.5">
                  <Award className="w-2.5 h-2.5" />
                  {isMaster ? 'MAX' : `LVL ${toast.tier}`}
                </span>
                <span className="text-xs font-bold text-slate-100 truncate font-mono">
                  {toast.title}
                </span>
              </div>
              <div className="text-[10px] text-emerald-400 font-sans truncate mt-0.5">
                {toast.rewardDesc}
              </div>
            </div>

            <div className="flex items-center gap-1 shrink-0">
              <span className="text-[10px] text-amber-400 font-mono">
                {toast.tier === 1 && '★☆☆'}
                {toast.tier === 2 && '★★☆'}
                {toast.tier === 3 && '★★★'}
              </span>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onDismiss(toast.id);
                }}
                className="text-slate-400 hover:text-slate-200 p-0.5"
              >
                <X className="w-3 h-3" />
              </button>
            </div>
          </div>
        );
      })}
    </div>
  );
};
