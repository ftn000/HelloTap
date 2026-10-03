import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { X, Building2, Zap, Save, Copy, Check, AlertTriangle, Tv, TrendingUp, Sliders, Crown, Sparkles, Palette, Volume2, VolumeX, Music, Bell, Play, Pause, SkipForward, Headphones } from 'lucide-react';
import confetti from 'canvas-confetti';
import { HubCategoryType } from '../types/game';
import { IN_APP_PRODUCTS } from '../utils/yandexPayments';
import { IDE_THEMES, getThemeTagline, getThemeSoundPresetName } from '../utils/themesList';
import { ThemeId } from '../types/themes';
import { sounds } from '../utils/soundEffects';
import { yandexSdk } from '../utils/yandexSdk';

interface StudioHubModalProps {
  isOpen: boolean;
  onClose: () => void;
  defaultTab?: 'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save';
}

export const StudioHubModal: React.FC<StudioHubModalProps> = ({ isOpen, onClose, defaultTab = 'systems' }) => {
  const [activeTab, setActiveTab] = useState<'systems' | 'shop' | 'digest' | 'prestige' | 'custom' | 'save'>(defaultTab);
  const [categoryFilter, setCategoryFilter] = useState<HubCategoryType | 'all'>('all');

  const {
    systems,
    upgradeSystem,
    codeLines,
    totalCodeEver,
    prestigeCount,
    prestigeTokens,
    claimDailyDigest,
    dailyDigestClaims,
    dailyDigestRemainingSec,
    triggerTimeWarp,
    timeWarpRemainingSec,
    adBoostRemainingSec,
    watchAdForDoubleBoost,
    watchAdForTimeWarpReset,
    triggerPrestigeIPO,
    switchType,
    setSwitchType,
    themeId,
    setThemeId,
    soundProfile,
    setSoundProfile,
    sfxVolume,
    setSfxVolume,
    musicVolume,
    setMusicVolume,
    isMusicPlaying,
    toggleMusic,
    currentTrackName,
    nextMusicTrack,
    hasVipX2,
    hasAutoClicker,
    hasNoAds,
    buyInAppProduct,
    exportSaveBase64,
    importSaveBase64,
    hardReset,
    lang,
    t
  } = useGame();

  const [copied, setCopied] = useState<boolean>(false);
  const [importInput, setImportInput] = useState<string>('');
  const [importMsg, setImportMsg] = useState<string>('');
  const [resetConfirm, setResetConfirm] = useState<boolean>(false);
  const [purchasingId, setPurchasingId] = useState<string | null>(null);
  const [purchaseStatus, setPurchaseStatus] = useState<string | null>(null);

  if (!isOpen) return null;

  const potentialTokens = Math.floor(Math.sqrt(totalCodeEver / 80000));

  const handleCopy = () => {
    const code = exportSaveBase64();
    navigator.clipboard.writeText(code);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleImport = () => {
    if (!importInput.trim()) return;
    const success = importSaveBase64(importInput);
    if (success) {
      setImportMsg(t.importSuccess);
      setTimeout(() => {
        window.location.reload();
      }, 700);
    } else {
      setImportMsg(t.importError);
    }
  };

  const handleClaim = () => {
    claimDailyDigest();
    confetti({
      particleCount: 50,
      spread: 60,
      origin: { y: 0.7 }
    });
  };

  const handleIPO = () => {
    if (potentialTokens <= 0) return;
    const res = triggerPrestigeIPO();
    if (res.gainedTokens > 0) {
      confetti({
        particleCount: 80,
        spread: 90,
        origin: { y: 0.6 },
        colors: ['#F59E0B', '#10B981', '#06B6D4', '#8B5CF6']
      });
      onClose();
    }
  };

  const filteredSystems = categoryFilter === 'all' 
    ? systems 
    : systems.filter(s => s.category === categoryFilter);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/85 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="relative w-full max-w-xl bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-cyber-border bg-slate-900/80">
          <div className="flex items-center gap-2">
            <Building2 className="w-5 h-5 text-cyan-400" />
            <h2 className="text-sm sm:text-base font-bold text-slate-100 font-mono tracking-wide">
              {t.hubHeader}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Tabs Bar */}
        <div className="grid grid-cols-3 sm:grid-cols-6 gap-1 p-1.5 bg-slate-950/70 border-b border-cyber-border text-[10px] sm:text-[11px] font-mono">
          <button
            onClick={() => setActiveTab('systems')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'systems' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t.tabSystems}
          </button>
          <button
            onClick={() => setActiveTab('shop')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'shop' ? 'bg-amber-500/20 text-amber-300 border border-amber-500/40 shadow-[0_0_10px_rgba(245,158,11,0.2)]' : 'text-amber-400/80 hover:text-amber-200'
            }`}
          >
            {t.tabShop}
          </button>
          <button
            onClick={() => setActiveTab('digest')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'digest' ? 'bg-indigo-500/20 text-indigo-300 border border-indigo-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t.tabBoosts}
          </button>
          <button
            onClick={() => setActiveTab('prestige')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'prestige' ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t.tabIPO}
          </button>
          <button
            onClick={() => setActiveTab('custom')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'custom' ? 'bg-blue-500/20 text-blue-300 border border-blue-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t.tabSwitches}
          </button>
          <button
            onClick={() => setActiveTab('save')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'save' ? 'bg-purple-500/20 text-purple-300 border border-purple-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t.tabSaves}
          </button>
        </div>

        {/* Content Body */}
        <div className="p-4 overflow-y-auto space-y-4">
          {/* TAB 1: СИСТЕМЫ СТУДИИ */}
          {activeTab === 'systems' && (
            <div className="space-y-3">
              {/* Category Pills */}
              <div className="flex items-center gap-1.5 overflow-x-auto pb-1 text-xs font-mono">
                {(['all', 'office', 'business', 'tech', 'culture'] as const).map(cat => (
                  <button
                    key={cat}
                    onClick={() => setCategoryFilter(cat)}
                    className={`px-3 py-1 rounded-lg capitalize transition-all ${
                      categoryFilter === cat
                        ? 'bg-cyan-500 text-slate-950 font-bold'
                        : 'bg-slate-800 text-slate-400 hover:bg-slate-700'
                    }`}
                  >
                    {cat === 'all' ? t.catAll : cat === 'office' ? t.catOffice : cat === 'business' ? t.catBusiness : cat === 'tech' ? t.catTech : t.catCulture}
                  </button>
                ))}
              </div>

              <div className="space-y-2.5">
                {filteredSystems.map(s => {
                  const cost = Math.floor(s.reqCode * Math.pow(1.5, s.level));
                  const canAfford = codeLines >= cost && s.level < s.maxLevel;
                  const isMax = s.level >= s.maxLevel;

                  const sTitle = lang === 'ru' ? (s.titleRu || s.title) : lang === 'tr' ? (s.titleTr || s.titleEn || s.title) : (s.titleEn || s.title);
                  const sDesc = lang === 'ru' ? (s.descriptionRu || s.description) : lang === 'tr' ? (s.descriptionTr || s.descriptionEn || s.description) : (s.descriptionEn || s.description);
                  const sBonus = lang === 'ru' ? (s.bonusDescRu || s.bonusDesc) : lang === 'tr' ? (s.bonusDescTr || s.bonusDescEn || s.bonusDesc) : (s.bonusDescEn || s.bonusDesc);

                  return (
                    <div
                      key={s.id}
                      className="p-3 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center justify-between gap-3"
                    >
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center text-xl shrink-0">
                          {s.icon}
                        </div>
                        <div className="min-w-0">
                          <div className="flex items-center gap-2">
                            <span className="font-semibold text-sm text-slate-200 truncate">{sTitle}</span>
                            <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-400 shrink-0">
                              {t.lvlPrefix} {s.level}/{s.maxLevel}
                            </span>
                          </div>
                          <p className="text-xs text-slate-400 truncate mt-0.5">{sDesc}</p>
                          <span className="text-[11px] text-cyan-400 font-mono">{sBonus}</span>
                        </div>
                      </div>

                      <button
                        onClick={() => upgradeSystem(s.id)}
                        disabled={!canAfford}
                        className={`px-3 py-1.5 rounded-xl text-xs font-mono font-bold shrink-0 transition-all ${
                          isMax
                            ? 'bg-slate-800 text-slate-500 cursor-not-allowed'
                            : canAfford
                            ? 'bg-cyan-500 hover:bg-cyan-400 text-slate-950 shadow-[0_0_12px_rgba(6,182,212,0.3)] active:scale-95'
                            : 'bg-slate-800/80 text-slate-500 cursor-not-allowed'
                        }`}
                      >
                        {isMax ? t.maxBtn : `${formatNumber(cost)} C#`}
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB: VIP МАРКЕТПЛЕЙС И ДОНАТ */}
          {activeTab === 'shop' && (
            <div className="space-y-3 font-mono">
              {/* VIP Store Banner */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-purple-950/40 to-slate-900 border border-purple-500/30 space-y-2">
                <div className="flex items-center gap-2">
                  <Crown className="w-5 h-5 text-amber-400" />
                  <h3 className="text-sm font-bold text-amber-300 tracking-wide">{t.vipStoreTitle}</h3>
                </div>
                <p className="text-xs text-slate-300 font-sans">{t.vipStoreDesc}</p>

                {/* Active Perks summary */}
                {(hasVipX2 || hasAutoClicker || hasNoAds) && (
                  <div className="pt-2 border-t border-purple-500/20 space-y-1">
                    <div className="text-[11px] text-amber-400/90 font-bold">{t.vipActivePerks}</div>
                    <div className="flex flex-wrap gap-1.5 text-[10px]">
                      {hasVipX2 && (
                        <span className="px-2 py-0.5 rounded-md bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
                          {t.vipPerkMultiplier}
                        </span>
                      )}
                      {hasAutoClicker && (
                        <span className="px-2 py-0.5 rounded-md bg-cyan-500/20 text-cyan-300 border border-cyan-500/30">
                          {t.vipPerkAutoclicker}
                        </span>
                      )}
                      {hasNoAds && (
                        <span className="px-2 py-0.5 rounded-md bg-amber-500/20 text-amber-300 border border-amber-500/30">
                          {t.vipPerkNoAds}
                        </span>
                      )}
                    </div>
                  </div>
                )}

                {purchaseStatus && (
                  <div className="p-2 rounded-lg bg-slate-950 text-center text-xs text-emerald-400 font-bold border border-emerald-500/40 animate-fade-in">
                    {purchaseStatus}
                  </div>
                )}
              </div>

              {/* Products list */}
              <div className="space-y-2.5">
                {IN_APP_PRODUCTS.map((prod) => {
                  const isOwned = 
                    (prod.id === 'codetap_vip_x2' && hasVipX2) ||
                    (prod.id === 'codetap_autoclicker' && hasAutoClicker) ||
                    (prod.id === 'codetap_noads' && hasNoAds);

                  const title = lang === 'ru' ? prod.titleRu : lang === 'tr' ? (prod.titleTr || prod.titleEn) : prod.titleEn;
                  const desc = lang === 'ru' ? prod.descRu : lang === 'tr' ? (prod.descTr || prod.descEn) : prod.descEn;
                  const isBusy = purchasingId === prod.id;

                  return (
                    <div
                      key={prod.id}
                      className={`p-3.5 rounded-2xl border transition-all flex flex-col sm:flex-row sm:items-center justify-between gap-3 ${
                        isOwned
                          ? 'bg-slate-900/60 border-emerald-500/30 opacity-80'
                          : 'bg-slate-900/90 border-slate-800 hover:border-purple-500/40'
                      }`}
                    >
                      <div className="flex items-start gap-3">
                        <span className="text-3xl p-2 rounded-xl bg-slate-800/80 border border-slate-700/60 flex-shrink-0">
                          {prod.icon}
                        </span>
                        <div className="space-y-1">
                          <div className="flex items-center gap-2 flex-wrap">
                            <span className="text-sm font-bold text-slate-100">{title}</span>
                            <span
                              className={`text-[9px] px-1.5 py-0.5 rounded font-mono font-semibold ${
                                prod.isConsumable
                                  ? 'bg-amber-500/20 text-amber-300 border border-amber-500/30'
                                  : 'bg-purple-500/20 text-purple-300 border border-purple-500/30'
                              }`}
                            >
                              {prod.isConsumable ? t.consumableBadge : t.permanentBadge}
                            </span>
                          </div>
                          <p className="text-xs text-slate-400 font-sans leading-relaxed">{desc}</p>
                        </div>
                      </div>

                      <div className="flex-shrink-0 sm:self-center">
                        {isOwned ? (
                          <div className="px-4 py-2 rounded-xl bg-emerald-950/40 border border-emerald-500/40 text-emerald-400 text-xs font-bold text-center">
                            {t.alreadyOwned}
                          </div>
                        ) : (
                          <button
                            disabled={isBusy}
                            onClick={async () => {
                              setPurchasingId(prod.id);
                              const ok = await buyInAppProduct(prod.id);
                              setPurchasingId(null);
                              if (ok) {
                                setPurchaseStatus(t.purchaseSuccess);
                                confetti({ particleCount: 70, spread: 70, origin: { y: 0.6 } });
                              } else {
                                setPurchaseStatus(t.purchaseFailed);
                              }
                              setTimeout(() => setPurchaseStatus(null), 3500);
                            }}
                            className={`w-full sm:w-auto px-4 py-2.5 rounded-xl font-bold text-xs uppercase tracking-wide transition-all shadow-md active:scale-98 flex items-center justify-center gap-1.5 ${
                              isBusy
                                ? 'bg-slate-800 text-slate-500 cursor-wait'
                                : 'bg-gradient-to-r from-amber-500 to-yellow-500 hover:from-amber-400 hover:to-yellow-400 text-slate-950 shadow-[0_0_12px_rgba(245,158,11,0.3)]'
                            }`}
                          >
                            <Sparkles className="w-3.5 h-3.5" />
                            <span>
                              {isBusy
                                ? '⏳...'
                                : t.buyForYans.replace('{0}', prod.priceYans.toString())}
                            </span>
                          </button>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB 3: ДАЙДЖЕСТ, TIME WARP И ЯНДЕКС РЕКЛАМА */}
          {activeTab === 'digest' && (
            <div className="space-y-3 font-mono">
              {/* Рекламные бусты Яндекс Игр */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-indigo-950/40 to-slate-900 border border-indigo-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Tv className="w-5 h-5 text-indigo-400" />
                    <div>
                      <h3 className="text-sm font-bold text-indigo-300">{t.yandexBonusTitle}</h3>
                      <p className="text-xs text-slate-400">{t.yandexBonusDesc}</p>
                    </div>
                  </div>
                  {adBoostRemainingSec > 0 && (
                    <span className="text-xs text-emerald-400 bg-emerald-500/10 px-2 py-1 rounded-md border border-emerald-500/30 animate-pulse">
                      {t.adActive} ({adBoostRemainingSec}с)
                    </span>
                  )}
                </div>

                {hasNoAds && (
                  <div className="p-2 rounded-xl bg-amber-500/10 border border-amber-500/30 text-amber-300 text-xs font-bold text-center tracking-wide">
                    {t.noAdsActiveBadge}
                  </div>
                )}

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <button
                    onClick={watchAdForDoubleBoost}
                    className="p-3 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 text-white font-bold text-xs flex flex-col items-center justify-center gap-1 shadow-md transition-all active:scale-98"
                  >
                    <span>{t.yandexAdDouble}</span>
                    <span className="text-[10px] font-normal opacity-90">{t.yandexAdDoubleDesc}</span>
                  </button>

                  <button
                    onClick={watchAdForTimeWarpReset}
                    className="p-3 rounded-xl bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-500 hover:to-blue-500 text-white font-bold text-xs flex flex-col items-center justify-center gap-1 shadow-md transition-all active:scale-98"
                  >
                    <span>{t.yandexAdTimeWarp}</span>
                    <span className="text-[10px] font-normal opacity-90">{t.yandexAdTimeWarpDesc}</span>
                  </button>
                </div>
              </div>

              {/* Daily Digest Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-amber-950/30 to-slate-900 border border-amber-500/30 space-y-2.5">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">📋</span>
                    <div>
                      <h3 className="text-sm font-bold text-amber-300">{t.digestTitle}</h3>
                      <p className="text-xs text-slate-400">{t.digestClaimsCount}: {dailyDigestClaims}</p>
                    </div>
                  </div>
                  <span className="text-xs text-amber-400 bg-amber-500/10 px-2 py-0.5 rounded border border-amber-500/30">
                    +100% FLOW (x3.0)
                  </span>
                </div>
                <button
                  onClick={handleClaim}
                  className="w-full py-2.5 rounded-xl bg-gradient-to-r from-amber-500 to-yellow-500 hover:from-amber-400 text-slate-950 font-bold text-xs uppercase tracking-wider shadow-[0_0_15px_rgba(245,158,11,0.3)] transition-all active:scale-98"
                >
                  {t.digestClaimBtn}
                </button>
              </div>

              {/* Time Warp Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-cyan-950/30 to-slate-900 border border-cyan-500/30 space-y-2.5">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">⚡</span>
                    <div>
                      <h3 className="text-sm font-bold text-cyan-300">{t.timeWarpTitle}</h3>
                      <p className="text-xs text-slate-400">{t.timeWarpDesc}</p>
                    </div>
                  </div>
                </div>
                <button
                  onClick={triggerTimeWarp}
                  disabled={timeWarpRemainingSec > 0}
                  className={`w-full py-2.5 rounded-xl font-bold text-xs uppercase tracking-wider transition-all ${
                    timeWarpRemainingSec > 0
                      ? 'bg-slate-800 text-slate-500 cursor-not-allowed border border-slate-700'
                      : 'bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 text-white shadow-[0_0_15px_rgba(6,182,212,0.3)] active:scale-98'
                  }`}
                >
                  {timeWarpRemainingSec > 0
                    ? `${t.timeWarpCharging}: ${Math.floor(timeWarpRemainingSec / 60)}:${(timeWarpRemainingSec % 60).toString().padStart(2, '0')}`
                    : t.timeWarpBtn}
                </button>
              </div>
            </div>
          )}

          {/* TAB 3: ПРЕСТИЖ / ВЫХОД НА IPO */}
          {activeTab === 'prestige' && (
            <div className="space-y-4 font-mono">
              <div className="p-4 rounded-2xl bg-gradient-to-br from-emerald-950/40 to-slate-900 border border-emerald-500/30 space-y-3">
                <div className="flex items-center gap-2.5">
                  <TrendingUp className="w-6 h-6 text-emerald-400" />
                  <div>
                    <h3 className="text-sm font-bold text-emerald-300">{t.ipoTitle}</h3>
                    <p className="text-xs text-slate-400">IPO: {prestigeCount}</p>
                  </div>
                </div>

                <p className="text-xs text-slate-300 font-sans leading-relaxed">
                  {t.ipoDesc}
                </p>

                <div className="p-3 rounded-xl bg-slate-950/80 border border-slate-800 flex items-center justify-between text-xs">
                  <span className="text-slate-400">{t.ipoShares}</span>
                  <span className="text-emerald-400 font-bold">{prestigeTokens} {t.ipoTokensInPortfolio} (+{(prestigeTokens * 5)}% {t.boostTag})</span>
                </div>

                <div className="p-3 rounded-xl bg-slate-950/80 border border-slate-800 flex items-center justify-between text-xs">
                  <span className="text-slate-400">{t.ipoWillGain}</span>
                  <span className="text-amber-400 font-bold">+{potentialTokens} {t.ipoTokensGain}</span>
                </div>

                <button
                  onClick={handleIPO}
                  disabled={potentialTokens <= 0}
                  className={`w-full py-3 rounded-xl font-bold text-xs uppercase tracking-wider transition-all ${
                    potentialTokens > 0
                      ? 'bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-400 text-slate-950 shadow-[0_0_20px_rgba(16,185,129,0.4)] active:scale-98'
                      : 'bg-slate-800 text-slate-500 cursor-not-allowed border border-slate-700'
                  }`}
                >
                  {potentialTokens > 0 ? `${t.ipoBtn} (+${potentialTokens})` : t.ipoNotEnough}
                </button>
              </div>
            </div>
          )}

          {/* TAB 4: КАСТОМИЗАЦИЯ СВИТЧЕЙ И ЗВУКА */}
          {activeTab === 'custom' && (
            <div className="space-y-4 font-mono text-xs">
              {/* 1. РЕГУЛЯТОР ГРОМКОСТИ ЭФФЕКТОВ И ПРОФИЛИ ЗВУКА */}
              <div className="p-3.5 rounded-2xl bg-gradient-to-br from-slate-900 to-indigo-950/40 border border-indigo-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-indigo-300 font-bold">
                    <Volume2 className="w-4 h-4 text-indigo-400" />
                    <span>{t.sfxTitle}</span>
                  </div>
                  <span className="text-[11px] font-bold text-indigo-300 px-2 py-0.5 rounded bg-indigo-950/60 border border-indigo-800/40">
                    {soundProfile === 'mute' ? 'Muted' : `${Math.round(sfxVolume * 100)}%`}
                  </span>
                </div>

                {/* Ползунок громкости SFX */}
                <div className="space-y-1">
                  <div className="flex items-center justify-between text-[10px] text-slate-400 font-sans">
                    <span>{t.sfxVolumeLabel}</span>
                    <button
                      type="button"
                      onClick={() => sounds.playKeyClick(false)}
                      className="text-indigo-400 hover:text-indigo-300 underline font-mono cursor-pointer"
                    >
                      {t.sfxTestClick}
                    </button>
                  </div>
                  <input
                    type="range"
                    min="0"
                    max="100"
                    step="1"
                    value={soundProfile === 'mute' ? 0 : Math.round(sfxVolume * 100)}
                    onChange={(e) => {
                      const val = parseInt(e.target.value) / 100;
                      setSfxVolume(val);
                      if (soundProfile === 'mute' && val > 0) {
                        setSoundProfile('asmr');
                      }
                    }}
                    className="w-full accent-indigo-500 cursor-pointer h-1.5 bg-slate-800 rounded-lg appearance-none"
                  />
                </div>

                {/* Переключатель профилей звука */}
                <div className="space-y-1.5">
                  <span className="text-[10px] text-slate-400 font-sans block">
                    {t.sfxProfilesLabel}
                  </span>
                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-1.5">
                    {[
                      { id: 'asmr', name: 'Soft ASMR', icon: '🍃', desc: t.profileAsmrDesc },
                      { id: 'classic', name: 'Classic', icon: '⌨️', desc: t.profileClassicDesc },
                      { id: 'cyber', name: 'Cyber', icon: '⚡', desc: t.profileCyberDesc },
                      { id: 'mute', name: 'Mute', icon: '🔇', desc: t.profileMuteDesc },
                    ].map((p) => {
                      const isActive = soundProfile === p.id;
                      return (
                        <button
                          key={p.id}
                          type="button"
                          onClick={() => {
                            setSoundProfile(p.id as any);
                            if (p.id !== 'mute') {
                              setTimeout(() => sounds.playKeyClick(false), 40);
                            }
                          }}
                          className={`p-2 rounded-xl border text-left flex flex-col justify-between transition-all cursor-pointer ${
                            isActive
                              ? 'bg-indigo-950/80 border-indigo-400 text-white shadow-[0_0_12px_rgba(99,102,241,0.4)] ring-1 ring-indigo-400'
                              : 'bg-slate-900/80 border-slate-800 text-slate-400 hover:border-slate-700 hover:text-slate-200'
                          }`}
                        >
                          <div className="flex items-center justify-between w-full">
                            <span className="text-sm">{p.icon}</span>
                            {isActive && <Check className="w-3.5 h-3.5 text-indigo-400" />}
                          </div>
                          <span className="font-bold text-[11px] mt-1 text-slate-200">{p.name}</span>
                          <span className="text-[9px] text-slate-400 font-sans truncate">{p.desc}</span>
                        </button>
                      );
                    })}
                  </div>
                </div>

                {/* Звуковое оповещение о критическом клике и Flow */}
                <div className="pt-2 border-t border-slate-800/80 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-2">
                  <div className="flex items-center gap-1.5 text-[11px] text-slate-300">
                    <Bell className="w-3.5 h-3.5 text-amber-400 shrink-0" />
                    <span>{t.sfxBellLabel}</span>
                  </div>
                  <div className="flex items-center gap-1.5 w-full sm:w-auto">
                    <button
                      type="button"
                      onClick={() => sounds.playGlassBell('crit')}
                      className="px-2.5 py-1 rounded-lg bg-amber-500/20 hover:bg-amber-500/30 border border-amber-500/40 text-amber-300 text-[10px] flex items-center gap-1 font-mono transition-colors cursor-pointer"
                      title={t.sfxBellCrit}
                    >
                      <span>{t.sfxBellCrit}</span>
                    </button>
                    <button
                      type="button"
                      onClick={() => sounds.playGlassBell('flow')}
                      className="px-2.5 py-1 rounded-lg bg-cyan-500/20 hover:bg-cyan-500/30 border border-cyan-500/40 text-cyan-300 text-[10px] flex items-center gap-1 font-mono transition-colors cursor-pointer"
                      title={t.sfxBellFlow}
                    >
                      <span>{t.sfxBellFlow}</span>
                    </button>
                  </div>
                </div>
              </div>

              {/* 2. ФОНОВАЯ ЭМБИЕНТ-МУЗЫКА (LO-FI CODER BEATS) */}
              <div className="p-3.5 rounded-2xl bg-gradient-to-br from-slate-900 to-cyan-950/40 border border-cyan-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-cyan-300 font-bold">
                    <Music className="w-4 h-4 text-cyan-400" />
                    <span>{t.musicSectionTitle}</span>
                  </div>
                  <span className="text-[11px] font-bold text-cyan-400 px-2 py-0.5 rounded bg-cyan-950/60 border border-cyan-800/40">
                    {Math.round(musicVolume * 100)}%
                  </span>
                </div>

                <div className="flex items-center justify-between gap-2 p-2.5 rounded-xl bg-slate-950/70 border border-slate-800">
                  <div className="min-w-0 flex items-center gap-2">
                    <div className={`w-2.5 h-2.5 rounded-full shrink-0 ${isMusicPlaying ? 'bg-cyan-400 animate-ping' : 'bg-slate-600'}`} />
                    <div className="min-w-0">
                      <span className="font-bold text-xs text-slate-200 block truncate">
                        {currentTrackName || 'Lo-Fi Coder Beats'}
                      </span>
                      <span className="text-[9px] text-slate-400 font-sans block">
                        {t.musicSectionDesc}
                      </span>
                    </div>
                  </div>

                  <div className="flex items-center gap-1.5 shrink-0">
                    <button
                      type="button"
                      onClick={toggleMusic}
                      className={`px-3 py-1.5 rounded-xl font-bold text-[11px] flex items-center gap-1 transition-all cursor-pointer ${
                        isMusicPlaying
                           ? 'bg-cyan-500 text-slate-950 shadow-[0_0_12px_rgba(6,182,212,0.4)]'
                          : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
                      }`}
                    >
                      {isMusicPlaying ? <Pause className="w-3 h-3" /> : <Play className="w-3 h-3" />}
                      <span>{isMusicPlaying ? t.musicPauseBtn : t.musicPlayBtn}</span>
                    </button>
                    <button
                      type="button"
                      onClick={nextMusicTrack}
                      className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 border border-slate-700/60 transition-colors cursor-pointer"
                      title={t.musicNextBtn}
                    >
                      <SkipForward className="w-3.5 h-3.5" />
                    </button>
                  </div>
                </div>

                {/* Ползунок громкости музыки */}
                <div className="space-y-1">
                  <div className="flex items-center justify-between text-[10px] text-slate-400 font-sans">
                    <span>{t.musicVolumeLabel}</span>
                    <span>{Math.round(musicVolume * 100)}%</span>
                  </div>
                  <input
                    type="range"
                    min="0"
                    max="100"
                    step="1"
                    value={Math.round(musicVolume * 100)}
                    onChange={(e) => setMusicVolume(parseInt(e.target.value) / 100)}
                    className="w-full accent-cyan-400 cursor-pointer h-1.5 bg-slate-800 rounded-lg appearance-none"
                  />
                </div>
              </div>

              {/* 3. СВИТЧИ КЛАВИАТУРЫ */}
              <div className="flex items-center gap-2 text-indigo-400 font-bold mb-1">
                <Sliders className="w-4 h-4" />
                <span>{t.switchTitle}</span>
              </div>
              <p className="text-slate-400 font-sans">
                {t.switchDesc}
              </p>

              <div className="grid grid-cols-2 gap-2">
                {[
                  { id: 'blue', name: 'Blue Clicky', desc: t.switchBlueDesc, icon: '🔵' },
                  { id: 'red', name: 'Red Linear', desc: t.switchRedDesc, icon: '🔴' },
                  { id: 'brown', name: 'Brown Tactile', desc: t.switchBrownDesc, icon: '🟤' },
                  { id: 'laser', name: 'Cyber Laser', desc: t.switchLaserDesc, icon: '⚡' },
                ].map(sw => (
                  <button
                    key={sw.id}
                    onClick={() => setSwitchType(sw.id as 'blue' | 'red' | 'brown' | 'laser')}
                    className={`p-3 rounded-2xl border text-left flex flex-col gap-1 transition-all ${
                      switchType === sw.id
                        ? 'bg-indigo-950/60 border-indigo-500 text-white shadow-[0_0_12px_rgba(99,102,241,0.3)]'
                        : 'bg-slate-900 border-slate-800 text-slate-400 hover:border-slate-700'
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-lg">{sw.icon}</span>
                      {switchType === sw.id && <Check className="w-4 h-4 text-indigo-400" />}
                    </div>
                    <span className="font-bold text-slate-200 mt-1">{sw.name}</span>
                    <span className="text-[10px] text-slate-400 font-sans">{sw.desc}</span>
                  </button>
                ))}
              </div>

              {/* СЕКЦИЯ: ТЕМЫ ОФОРМЛЕНИЯ IDE */}
              <div className="pt-3 border-t border-slate-800 space-y-2">
                <div className="flex items-center gap-2 text-cyan-400 font-bold">
                  <Palette className="w-4 h-4" />
                  <span>{t.themesSectionTitle}</span>
                </div>
                <p className="text-slate-400 font-sans">
                  {t.themesSectionDesc}
                </p>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {Object.values(IDE_THEMES).map(th => (
                    <button
                      key={th.id}
                      onClick={() => setThemeId(th.id as ThemeId)}
                      className={`p-3 rounded-2xl border text-left flex flex-col gap-1 transition-all ${
                        themeId === th.id
                          ? 'bg-cyan-950/40 border-cyan-400 text-white shadow-[0_0_12px_rgba(6,182,212,0.3)]'
                          : 'bg-slate-900 border-slate-800 text-slate-400 hover:border-slate-700'
                      }`}
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-1.5">
                          <span className="text-base">{th.icon}</span>
                          <span className="font-bold text-slate-200">{th.name}</span>
                        </div>
                        {themeId === th.id && <Check className="w-4 h-4 text-cyan-400" />}
                      </div>
                      <span className="text-[10px] text-slate-400 font-sans leading-tight mt-0.5">
                        {getThemeTagline(th, lang)}
                      </span>
                      <div className="flex items-center gap-1.5 mt-1">
                        <span className="text-[10px] font-mono text-cyan-400 bg-cyan-950/60 px-1.5 py-0.5 rounded border border-cyan-800/40">
                          🔊 {t.themeSoundLabel}: {getThemeSoundPresetName(th, lang)}
                        </span>
                      </div>
                    </button>
                  ))}
                </div>
              </div>
            </div>
          )}

          {/* TAB 5: СОХРАНЕНИЯ */}
          {activeTab === 'save' && (
            <div className="space-y-4 font-mono text-xs">
                            {/* ЯНДЕКС ИГРЫ: ОЦЕНКА И ЯРЛЫК */}
              <div className="p-3.5 rounded-2xl bg-gradient-to-r from-amber-950/30 via-slate-900 to-indigo-950/30 border border-amber-500/30 space-y-2.5">
                <div className="flex items-center gap-2 text-amber-300 font-bold">
                  <span>⭐</span>
                  <span>Яндекс Игры / Platform</span>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <button
                    onClick={async () => {
                      const reviewed = await yandexSdk.requestReview();
                      if (reviewed) {
                        sounds.playPurchaseSuccess();
                      }
                    }}
                    className="p-2.5 rounded-xl bg-amber-500/20 hover:bg-amber-500/30 border border-amber-500/50 text-amber-200 text-left flex flex-col gap-0.5 transition-all cursor-pointer shadow-sm active:scale-98"
                  >
                    <span className="font-bold text-xs">{t.rateGameBtn}</span>
                    <span className="text-[10px] text-amber-300/80 font-sans">{t.rateGameDesc}</span>
                  </button>
                  <button
                    onClick={async () => {
                      const added = await yandexSdk.showShortcut();
                      if (added) {
                        sounds.playPurchaseSuccess();
                      }
                    }}
                    className="p-2.5 rounded-xl bg-indigo-500/20 hover:bg-indigo-500/30 border border-indigo-500/50 text-indigo-200 text-left flex flex-col gap-0.5 transition-all cursor-pointer shadow-sm active:scale-98"
                  >
                    <span className="font-bold text-xs">{t.addShortcutBtn}</span>
                    <span className="text-[10px] text-indigo-300/80 font-sans">{t.addShortcutDesc}</span>
                  </button>
                </div>
              </div>

              <div className="p-3.5 rounded-2xl bg-slate-900 border border-slate-800 space-y-2.5">
                <div className="flex items-center gap-2 text-cyan-400 font-bold">
                  <Copy className="w-4 h-4" />
                  <span>{t.saveExportTitle}</span>
                </div>
                <p className="text-slate-400 text-[11px] font-sans">
                  {t.saveExportDesc}
                </p>
                <button
                  onClick={handleCopy}
                  className="w-full py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 flex items-center justify-center gap-2 font-semibold transition-all"
                >
                  {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
                  <span>{copied ? t.saveCopied : t.saveCopyBtn}</span>
                </button>
              </div>

              <div className="p-3.5 rounded-2xl bg-slate-900 border border-slate-800 space-y-2.5">
                <div className="flex items-center gap-2 text-emerald-400 font-bold">
                  <Save className="w-4 h-4" />
                  <span>{t.saveImportTitle}</span>
                </div>
                <input
                  type="text"
                  placeholder={t.saveImportPlaceholder}
                  value={importInput}
                  onChange={(e) => setImportInput(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-950 border border-slate-800 text-slate-200 focus:outline-none focus:border-cyan-500 font-mono text-xs"
                />
                <button
                  onClick={handleImport}
                  className="w-full py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-slate-950 font-bold tracking-wide transition-all shadow-[0_0_12px_rgba(16,185,129,0.3)]"
                >
                  {t.saveImportBtn}
                </button>
                {importMsg && <div className="text-center mt-1">{importMsg}</div>}
              </div>

              <div className="p-3.5 rounded-2xl bg-red-950/20 border border-red-900/40 space-y-2">
                <div className="flex items-center gap-2 text-red-400 font-bold">
                  <AlertTriangle className="w-4 h-4" />
                  <span>{t.saveDangerZone}</span>
                </div>
                <button
                  onClick={() => {
                    if (resetConfirm) {
                      hardReset();
                    } else {
                      setResetConfirm(true);
                    }
                  }}
                  className="w-full py-2 rounded-xl bg-red-950/50 hover:bg-red-900/60 text-red-300 border border-red-800/60 font-semibold transition-all"
                >
                  {resetConfirm ? t.saveResetConfirm : t.saveResetBtn}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
