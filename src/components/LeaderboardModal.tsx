import React, { useState, useEffect } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { yandexSdk, LeaderboardEntry } from '../utils/yandexSdk';
import { X, Trophy, RefreshCw, User, Medal } from 'lucide-react';

interface LeaderboardModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const LeaderboardModal: React.FC<LeaderboardModalProps> = ({ isOpen, onClose }) => {
  const { totalCodeEver, lang, t } = useGame();
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  const fetchScores = async () => {
    setIsLoading(true);
    try {
      const data = await yandexSdk.getLeaderboardEntries(10, totalCodeEver);
      setEntries(data);
    } catch (err) {
      console.error("Failed to load leaderboard:", err);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (isOpen) {
      fetchScores();
    }
  }, [isOpen, totalCodeEver]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/85 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="relative w-full max-w-lg bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-cyber-border bg-slate-900/80">
          <div className="flex items-center gap-2">
            <Trophy className="w-5 h-5 text-amber-400" />
            <h2 className="text-sm sm:text-base font-bold text-slate-100 font-mono tracking-wide">
              {t.leaderboardTitle}
            </h2>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={fetchScores}
              disabled={isLoading}
              className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
              title={lang === 'ru' ? 'Обновить' : lang === 'tr' ? 'Yenile' : 'Refresh'}
            >
              <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
            </button>
            <button
              onClick={onClose}
              className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
            >
              <X className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Content Body */}
        <div className="p-4 overflow-y-auto space-y-3 font-mono">
          <p className="text-xs text-slate-400 font-sans leading-relaxed">
            {t.leaderboardDesc}
          </p>

          {isLoading ? (
            <div className="py-12 flex flex-col items-center justify-center gap-2 text-cyan-400 text-xs">
              <RefreshCw className="w-6 h-6 animate-spin text-cyan-400" />
              <span>{t.leaderboardLoading}</span>
            </div>
          ) : (
            <div className="space-y-1.5">
              {entries.map((entry) => {
                const isTop1 = entry.rank === 1;
                const isTop2 = entry.rank === 2;
                const isTop3 = entry.rank === 3;

                return (
                  <div
                    key={`${entry.rank}-${entry.name}`}
                    className={`flex items-center justify-between p-3 rounded-2xl border transition-all ${
                      entry.isPlayer
                        ? 'bg-cyan-950/40 border-cyan-500/60 shadow-[0_0_15px_rgba(6,182,212,0.2)]'
                        : isTop1
                        ? 'bg-amber-950/20 border-amber-500/40'
                        : isTop2
                        ? 'bg-slate-800/50 border-slate-600/50'
                        : isTop3
                        ? 'bg-orange-950/20 border-orange-500/30'
                        : 'bg-slate-900/60 border-slate-800'
                    }`}
                  >
                    <div className="flex items-center gap-3">
                      {/* Номер ранга */}
                      <div className="w-7 text-center font-bold text-sm">
                        {isTop1 ? (
                          <span className="text-amber-400 text-base">🥇</span>
                        ) : isTop2 ? (
                          <span className="text-slate-300 text-base">🥈</span>
                        ) : isTop3 ? (
                          <span className="text-orange-400 text-base">🥉</span>
                        ) : (
                          <span className="text-slate-400 text-xs">#{entry.rank}</span>
                        )}
                      </div>

                      {/* Аватарка или иконка */}
                      <div className="w-8 h-8 rounded-full bg-slate-800 border border-slate-700 overflow-hidden flex items-center justify-center shrink-0">
                        {entry.photo ? (
                          <img src={entry.photo} alt={entry.name} className="w-full h-full object-cover" />
                        ) : (
                          <User className="w-4 h-4 text-slate-400" />
                        )}
                      </div>

                      {/* Имя */}
                      <div className="flex items-center gap-2">
                        <span className={`text-xs sm:text-sm font-semibold truncate max-w-[140px] sm:max-w-[200px] ${
                          entry.isPlayer ? 'text-cyan-300 font-bold' : 'text-slate-200'
                        }`}>
                          {entry.name}
                        </span>
                        {entry.isPlayer && (
                          <span className="px-1.5 py-0.5 rounded text-[9px] font-bold bg-cyan-500 text-slate-950">
                            {t.leaderboardYouBadge}
                          </span>
                        )}
                      </div>
                    </div>

                    {/* Очки кода */}
                    <div className="text-right">
                      <div className="text-xs sm:text-sm font-bold text-cyan-400 tracking-tight">
                        {formatNumber(entry.score)}
                      </div>
                      <div className="text-[10px] text-slate-500 font-sans">
                        {t.leaderboardScore}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
