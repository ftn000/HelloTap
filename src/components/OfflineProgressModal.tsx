import React from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Moon, Sparkles } from 'lucide-react';
import confetti from 'canvas-confetti';
import { sounds } from '../utils/soundEffects';

interface OfflineProgressModalProps {
  isOpen: boolean;
  offlineSec: number;
  codeEarned: number;
  moneyEarned: number;
  onClaim: (double: boolean) => void;
}

export const OfflineProgressModal: React.FC<OfflineProgressModalProps> = ({
  isOpen,
  offlineSec,
  codeEarned,
  moneyEarned,
  onClaim
}) => {
  const { lang, t, hasNoAds } = useGame();
  if (!isOpen) return null;

  const hours = Math.floor(offlineSec / 3600);
  const minutes = Math.floor((offlineSec % 3600) / 60);
  const seconds = Math.floor(offlineSec % 60);

  const hUnit = `${t.hourShort} `;
  const mUnit = `${t.minShort} `;
  const sUnit = t.secShort;
  const timeString = `${hours > 0 ? `${hours}${hUnit}` : ''}${minutes}${mUnit}${seconds}${sUnit}`;

  const handleClaimNormal = () => {
    confetti({
      particleCount: 40,
      spread: 60,
      origin: { y: 0.6 }
    });
    sounds.playOfflineReward(false);
    onClaim(false);
  };

  const handleClaimDouble = () => {
    confetti({
      particleCount: 80,
      spread: 90,
      origin: { y: 0.5 },
      colors: ['#F59E0B', '#10B981', '#06B6D4', '#8B5CF6']
    });
    sounds.playOfflineReward(true);
    onClaim(true);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-200 font-mono">
      <div className="relative w-full max-w-md bg-gradient-to-b from-slate-900 via-slate-950 to-slate-900 border border-cyan-500/40 rounded-3xl p-6 shadow-[0_0_50px_rgba(6,182,212,0.25)] text-slate-100 flex flex-col gap-4 text-center">
        {/* Фоновое свечение */}
        <div className="absolute -top-16 -left-16 w-32 h-32 bg-cyan-500/20 rounded-full blur-3xl pointer-events-none" />

        <div className="w-14 h-14 mx-auto rounded-2xl bg-cyan-500/10 border border-cyan-500/30 flex items-center justify-center text-cyan-400 shadow-[0_0_20px_rgba(6,182,212,0.3)]">
          <Moon className="w-7 h-7" />
        </div>

        <div>
          <h2 className="text-lg sm:text-xl font-bold text-white tracking-wide">
            {t.offlineWelcome}
          </h2>
          <p className="text-xs text-slate-400 font-sans mt-1">
            {t.offlineAwayDesc.replace('{0}', timeString)}
          </p>
        </div>

        {/* Карточки начисленных ресурсов */}
        <div className="grid grid-cols-2 gap-2.5 my-1">
          <div className="p-3.5 rounded-2xl bg-slate-950/80 border border-slate-800 flex flex-col items-center justify-center gap-1">
            <span className="text-[10px] text-slate-400 uppercase tracking-wider">{t.offlineCodeLabel}</span>
            <span className="text-base font-bold text-cyan-400">+{formatNumber(codeEarned)}</span>
          </div>

          <div className="p-3.5 rounded-2xl bg-slate-950/80 border border-slate-800 flex flex-col items-center justify-center gap-1">
            <span className="text-[10px] text-slate-400 uppercase tracking-wider">{t.offlineRevenueLabel}</span>
            <span className="text-base font-bold text-emerald-400">+{formatNumber(moneyEarned)} ₽</span>
          </div>
        </div>

        {/* Кнопки действий */}
        <div className="flex flex-col gap-2 pt-1">
          <button
            onClick={handleClaimDouble}
            className="w-full py-3.5 rounded-2xl bg-gradient-to-r from-amber-500 via-yellow-500 to-amber-600 hover:from-amber-400 text-slate-950 font-bold text-xs uppercase tracking-wider flex items-center justify-center gap-2 shadow-[0_0_25px_rgba(245,158,11,0.4)] active:scale-98 transition-all"
          >
            <Sparkles className="w-4 h-4 animate-spin" />
            <span>{t.offlineDoubleBtn}</span>
          </button>

          <button
            onClick={handleClaimNormal}
            className="w-full py-2.5 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-300 font-semibold text-xs tracking-wider border border-slate-700 active:scale-98 transition-all"
          >
            {t.offlineClaimBtn}
          </button>
        </div>
      </div>
    </div>
  );
};
