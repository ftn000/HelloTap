import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { ACHIEVEMENTS } from '../utils/achievementsList';
import { AchievementCategory } from '../types/achievements';
import { X, Award, CheckCircle2, Star, Sparkles, Filter } from 'lucide-react';

interface AchievementsModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const AchievementsModal: React.FC<AchievementsModalProps> = ({ isOpen, onClose }) => {
  const { achievements, getAchievementProgress, achievementBonusMultiplier, lang, t } = useGame();
  const [selectedCat, setSelectedCat] = useState<AchievementCategory | 'all'>('all');

  if (!isOpen) return null;

  // Подсчет общего количества заработанных звезд (максимум 24 * 3 = 72)
  const totalStars = Object.values(achievements).reduce((sum, tier) => sum + tier, 0);
  const maxStars = ACHIEVEMENTS.length * 3;
  const completedCount = Object.values(achievements).filter(tier => tier === 3).length;

  const filtered = selectedCat === 'all'
    ? ACHIEVEMENTS
    : ACHIEVEMENTS.filter(a => a.category === selectedCat);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/85 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="relative w-full max-w-2xl bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-cyber-border bg-slate-900/80">
          <div className="flex items-center gap-2">
            <Award className="w-5 h-5 text-amber-400" />
            <h2 className="text-sm sm:text-base font-bold text-slate-100 font-mono tracking-wide">
              {lang === 'ru' ? 'ДОСТИЖЕНИЯ И НАГРАДЫ' : 'ACHIEVEMENTS & TROPHIES'}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Top Summary Banner */}
        <div className="p-4 bg-gradient-to-r from-amber-950/30 via-slate-900 to-indigo-950/30 border-b border-cyber-border space-y-2.5 font-mono">
          <div className="flex items-center justify-between flex-wrap gap-2 text-xs">
            <div className="flex items-center gap-2">
              <span className="text-amber-400 font-bold text-sm">★ {totalStars} / {maxStars}</span>
              <span className="text-slate-400 font-sans">
                ({completedCount} {lang === 'ru' ? `из ${ACHIEVEMENTS.length} закрыто` : `of ${ACHIEVEMENTS.length} mastered`})
              </span>
            </div>
            <div className="px-2.5 py-1 rounded-xl bg-emerald-500/20 text-emerald-300 border border-emerald-500/30 text-xs font-bold flex items-center gap-1.5 shadow-[0_0_10px_rgba(16,185,129,0.2)]">
              <Sparkles className="w-3.5 h-3.5" />
              <span>+{((achievementBonusMultiplier - 1) * 100).toFixed(0)}% {lang === 'ru' ? 'к общему множителю' : 'Total Boost'}</span>
            </div>
          </div>

          {/* Progress bar */}
          <div className="h-2 w-full bg-slate-950 rounded-full overflow-hidden p-0.5 border border-slate-800">
            <div
              className="h-full rounded-full bg-gradient-to-r from-amber-500 via-orange-500 to-yellow-400 transition-all duration-300 shadow-[0_0_8px_rgba(245,158,11,0.5)]"
              style={{ width: `${Math.min(100, (totalStars / maxStars) * 100)}%` }}
            />
          </div>

          {/* Category Filter Pills */}
          <div className="flex items-center gap-1.5 overflow-x-auto pt-1 pb-0.5 text-[11px] scrollbar-none">
            {(['all', 'clicks', 'code', 'economy', 'upgrades', 'systems', 'prestige'] as const).map(cat => {
              const label = {
                all: lang === 'ru' ? `Все (${ACHIEVEMENTS.length})` : lang === 'tr' ? `Tümü (${ACHIEVEMENTS.length})` : `All (${ACHIEVEMENTS.length})`,
                clicks: lang === 'ru' ? 'Клики' : lang === 'tr' ? 'Tıklamalar' : 'Clicks',
                code: lang === 'ru' ? 'Код' : lang === 'tr' ? 'Kod' : 'Code',
                economy: lang === 'ru' ? 'Экономика' : lang === 'tr' ? 'Ekonomi' : 'Economy',
                upgrades: lang === 'ru' ? 'Оборудование' : lang === 'tr' ? 'Ekipman' : 'Gear',
                systems: lang === 'ru' ? 'Системы' : lang === 'tr' ? 'Sistemler' : 'Systems',
                prestige: lang === 'ru' ? 'Престиж' : lang === 'tr' ? 'Prestij' : 'Prestige'
              }[cat];

              return (
                <button
                  key={cat}
                  onClick={() => setSelectedCat(cat)}
                  className={`px-2.5 py-1 rounded-lg shrink-0 font-medium transition-all ${
                    selectedCat === cat
                      ? 'bg-amber-500 text-slate-950 font-bold shadow-[0_0_10px_rgba(245,158,11,0.3)]'
                      : 'bg-slate-800/80 text-slate-400 hover:bg-slate-700'
                  }`}
                >
                  {label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Achievements List */}
        <div className="p-4 overflow-y-auto space-y-3 font-mono">
          {filtered.map(ach => {
            const currentTier = achievements[ach.id] || 0;
            const isCompleted = currentTier >= 3;
            const progress = getAchievementProgress(ach.id);
            const currentVal = progress.current;
            const nextTarget = progress.nextTarget;
            const percent = progress.percent;

            const title = lang === 'ru' ? ach.titleRu : lang === 'tr' ? (ach.titleTr || ach.titleEn) : ach.titleEn;
            const desc = lang === 'ru' ? ach.descRu : lang === 'tr' ? (ach.descTr || ach.descEn) : ach.descEn;

            return (
              <div
                key={ach.id}
                className={`p-3.5 rounded-2xl border transition-all space-y-2.5 ${
                  isCompleted
                    ? 'bg-gradient-to-r from-amber-950/25 to-slate-900/80 border-amber-500/40 shadow-[0_0_12px_rgba(245,158,11,0.15)]'
                    : currentTier > 0
                    ? 'bg-slate-900/80 border-cyan-500/30'
                    : 'bg-slate-900/40 border-slate-800/80 opacity-75'
                }`}
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex items-start gap-3">
                    <span className="text-3xl p-2 rounded-xl bg-slate-800/80 border border-slate-700/60 shrink-0">
                      {ach.icon}
                    </span>
                    <div>
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-sm font-bold text-slate-100">{title}</span>
                        {isCompleted ? (
                          <span className="text-[10px] px-2 py-0.5 rounded font-bold bg-amber-500 text-slate-950">
                            ★★★ {lang === 'ru' ? 'ЗАКРЫТО' : lang === 'tr' ? 'TAMAMLANDI' : 'MASTERED'}
                          </span>
                        ) : (
                          <span className="text-[10px] px-1.5 py-0.5 rounded font-bold bg-slate-800 text-amber-400 border border-slate-700">
                            {currentTier === 0 ? '☆☆☆' : currentTier === 1 ? '★☆☆' : '★★☆'}
                          </span>
                        )}
                      </div>
                      <p className="text-xs text-slate-400 font-sans mt-0.5">{desc}</p>
                    </div>
                  </div>

                  {/* Текущий уровень бонуса */}
                  <div className="text-right shrink-0">
                    <span className="text-xs font-bold text-emerald-400">
                      {currentTier > 0
                        ? `+${(ach.tiers.slice(0, currentTier).reduce((acc, t) => acc + t.bonusMultiplier, 0) * 100).toFixed(0)}%`
                        : '+0%'}
                    </span>
                  </div>
                </div>

                {/* 3 Трёхуровневых индикатора */}
                <div className="grid grid-cols-3 gap-1.5 text-[10px] font-sans">
                  {ach.tiers.map((tDef, idx) => {
                    const isTierUnlocked = currentTier >= tDef.tier;
                    const isCurrentGoal = currentTier === idx;

                    return (
                      <div
                        key={tDef.tier}
                        className={`p-2 rounded-xl border flex flex-col justify-between gap-1 transition-all ${
                          isTierUnlocked
                            ? 'bg-amber-500/15 border-amber-500/40 text-amber-200'
                            : isCurrentGoal
                            ? 'bg-cyan-950/30 border-cyan-500/40 text-cyan-200'
                            : 'bg-slate-950/40 border-slate-800 text-slate-500'
                        }`}
                      >
                        <div className="flex items-center justify-between font-mono font-bold">
                          <span>{tDef.tier === 1 ? `★ ${t.tierPrefix}1` : tDef.tier === 2 ? `★★ ${t.tierPrefix}2` : `★★★ ${t.tierPrefix}3`}</span>
                          {isTierUnlocked && <CheckCircle2 className="w-3 h-3 text-amber-400" />}
                        </div>
                        <div className="text-[10px] font-mono text-slate-300">
                          {formatNumber(tDef.target)}
                        </div>
                        <div className="text-[9px] text-emerald-400 leading-tight">
                          {lang === 'ru' ? tDef.rewardDescRu : lang === 'tr' ? (tDef.rewardDescTr || tDef.rewardDescEn) : tDef.rewardDescEn}
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Прогресс-бар текущего уровня */}
                {!isCompleted && (
                  <div className="space-y-1 pt-1">
                    <div className="flex items-center justify-between text-[10px] text-slate-400">
                      <span>{lang === 'ru' ? 'Прогресс до след. звезды:' : lang === 'tr' ? 'Sonraki yıldıza ilerleme:' : 'Progress to next star:'}</span>
                      <span className="font-mono text-cyan-300">{formatNumber(currentVal)} / {formatNumber(nextTarget)} ({percent.toFixed(0)}%)</span>
                    </div>
                    <div className="h-1.5 w-full bg-slate-950 rounded-full overflow-hidden border border-slate-800">
                      <div
                        className="h-full rounded-full bg-cyan-500 transition-all duration-200"
                        style={{ width: `${Math.min(100, percent)}%` }}
                      />
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};
