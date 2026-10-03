import React from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { ShoppingBag, ArrowUpRight, Check } from 'lucide-react';

export const ShopPanel: React.FC = () => {
  const { upgrades, buyUpgrade, codeLines, money, unlockedSkills, lang, t } = useGame();

  const discountMultiplier = Math.max(0.65, 1.0 - ((unlockedSkills['skill_negotiation'] || 0) * 0.05 + (unlockedSkills['skill_unicorn_status'] || 0) * 0.08));

  return (
    <div className="w-full max-w-md mx-auto px-4 pb-20">
      <div className="flex items-center gap-2 mb-3">
        <ShoppingBag className="w-4 h-4 text-cyan-400" />
        <h2 className="text-sm font-bold uppercase tracking-wider text-slate-300 font-mono">
          {t.upgradesTitle}
        </h2>
      </div>

      <div className="space-y-2.5">
        {upgrades.map(u => {
          const costCode = Math.floor(u.baseCostCode * Math.pow(u.costMultiplier, u.level) * discountMultiplier);
          const costMoney = Math.floor(u.baseCostMoney * Math.pow(u.costMultiplier, u.level) * discountMultiplier);
          const canAfford = codeLines >= costCode && money >= costMoney && u.level < u.maxLevel;
          const isMax = u.level >= u.maxLevel;

          const uName = lang === 'ru' ? (u.nameRu || u.name) : lang === 'tr' ? (u.nameTr || u.nameEn || u.name) : (u.nameEn || u.name);
          const uDesc = lang === 'ru' ? (u.descriptionRu || u.description) : lang === 'tr' ? (u.descriptionTr || u.descriptionEn || u.description) : (u.descriptionEn || u.description);

          return (
            <div
              key={u.id}
              className={`p-3 rounded-2xl border transition-all duration-150 flex items-center justify-between gap-3 ${
                isMax
                  ? 'bg-slate-900/40 border-slate-800/40 opacity-70'
                  : canAfford
                  ? 'bg-slate-900/90 border-slate-800 hover:border-cyan-500/50 hover:bg-slate-850'
                  : 'bg-slate-950/60 border-slate-900 opacity-60'
              }`}
            >
              {/* Иконка и Описание */}
              <div className="flex items-center gap-3 min-w-0">
                <div className="w-11 h-11 rounded-xl bg-slate-800 flex items-center justify-center text-xl shrink-0 border border-slate-700/50">
                  {u.icon}
                </div>
                <div className="min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="font-semibold text-sm text-slate-200 truncate">{uName}</span>
                    <span className="text-[11px] font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-400 shrink-0">
                      {t.lvlPrefix} {u.level}/{u.maxLevel}
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 truncate mt-0.5">{uDesc}</p>
                  
                  {/* Бонусы апгрейда */}
                  <div className="flex items-center gap-2 mt-1 text-[11px] font-mono text-cyan-400">
                    {u.codePerClickBonus > 0 && <span>+{u.codePerClickBonus} {t.locPerClick}</span>}
                    {u.codePerSecBonus > 0 && <span>+{u.codePerSecBonus} {t.locPerSec}</span>}
                    {u.moneyPerSecBonus > 0 && <span className="text-emerald-400">+{u.moneyPerSecBonus} {t.rubPerSec}</span>}
                  </div>
                </div>
              </div>

              {/* Кнопка покупки */}
              <button
                onClick={() => buyUpgrade(u.id)}
                disabled={!canAfford}
                className={`px-3.5 py-2 rounded-xl text-xs font-mono font-bold shrink-0 flex flex-col items-center justify-center transition-all ${
                  isMax
                    ? 'bg-slate-800 text-slate-500 cursor-not-allowed'
                    : canAfford
                    ? 'bg-gradient-to-r from-emerald-500 to-teal-600 hover:from-emerald-400 hover:to-teal-500 text-slate-950 shadow-[0_0_15px_rgba(16,185,129,0.3)] active:scale-95'
                    : 'bg-slate-800/80 text-slate-500 cursor-not-allowed'
                }`}
              >
                {isMax ? (
                  <span className="flex items-center gap-1">
                    <Check className="w-3.5 h-3.5" /> {t.maxBtn}
                  </span>
                ) : (
                  <>
                    <span className="flex items-center gap-1">
                      {t.buyBtn} <ArrowUpRight className="w-3 h-3" />
                    </span>
                    <span className="text-[10px] font-normal opacity-90">
                      {formatNumber(costCode)} C# / {formatNumber(costMoney)} ₽
                    </span>
                  </>
                )}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
};
