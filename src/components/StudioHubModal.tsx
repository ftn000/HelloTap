import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { X, Building2, Zap, Save, Copy, Check, AlertTriangle, Tv, TrendingUp, Sliders } from 'lucide-react';
import confetti from 'canvas-confetti';
import { HubCategoryType } from '../types/game';

interface StudioHubModalProps {
  isOpen: boolean;
  onClose: () => void;
  defaultTab?: 'systems' | 'digest' | 'prestige' | 'custom' | 'save';
}

export const StudioHubModal: React.FC<StudioHubModalProps> = ({ isOpen, onClose, defaultTab = 'systems' }) => {
  const [activeTab, setActiveTab] = useState<'systems' | 'digest' | 'prestige' | 'custom' | 'save'>(defaultTab);
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
    triggerTimeWarp,
    timeWarpRemainingSec,
    adBoostRemainingSec,
    watchAdForDoubleBoost,
    watchAdForTimeWarpReset,
    triggerPrestigeIPO,
    switchType,
    setSwitchType,
    exportSaveBase64,
    importSaveBase64,
    hardReset
  } = useGame();

  const [copied, setCopied] = useState<boolean>(false);
  const [importInput, setImportInput] = useState<string>('');
  const [importMsg, setImportMsg] = useState<string>('');
  const [resetConfirm, setResetConfirm] = useState<boolean>(false);

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
      setImportMsg('✓ Прогресс успешно загружен!');
      setTimeout(() => {
        window.location.reload();
      }, 700);
    } else {
      setImportMsg('❌ Ошибка: неверный формат ключа сохранения!');
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
              STUDIO OS — СИСТЕМЫ И ПРЕСТИЖ
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
        <div className="grid grid-cols-5 gap-1 p-1.5 bg-slate-950/70 border-b border-cyber-border text-[11px] font-mono">
          <button
            onClick={() => setActiveTab('systems')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'systems' ? 'bg-cyan-500/20 text-cyan-300 border border-cyan-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            🏢 Системы
          </button>
          <button
            onClick={() => setActiveTab('digest')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'digest' ? 'bg-amber-500/20 text-amber-300 border border-amber-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            ⚡ Бусты & Реклама
          </button>
          <button
            onClick={() => setActiveTab('prestige')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'prestige' ? 'bg-emerald-500/20 text-emerald-300 border border-emerald-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            📈 IPO
          </button>
          <button
            onClick={() => setActiveTab('custom')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'custom' ? 'bg-indigo-500/20 text-indigo-300 border border-indigo-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            ⌨️ Свитчи
          </button>
          <button
            onClick={() => setActiveTab('save')}
            className={`py-2 px-1 rounded-xl font-semibold transition-all text-center ${
              activeTab === 'save' ? 'bg-purple-500/20 text-purple-300 border border-purple-500/40' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            💾 Сейвы
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
                    {cat === 'all' ? 'Все' : cat === 'office' ? 'Офис' : cat === 'business' ? 'Бизнес' : cat === 'tech' ? 'Технологии' : 'Культура'}
                  </button>
                ))}
              </div>

              <div className="space-y-2.5">
                {filteredSystems.map(s => {
                  const cost = Math.floor(s.reqCode * Math.pow(1.5, s.level));
                  const canAfford = codeLines >= cost && s.level < s.maxLevel;
                  const isMax = s.level >= s.maxLevel;

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
                            <span className="font-semibold text-sm text-slate-200 truncate">{s.title}</span>
                            <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-400 shrink-0">
                              ур. {s.level}/{s.maxLevel}
                            </span>
                          </div>
                          <p className="text-xs text-slate-400 truncate mt-0.5">{s.description}</p>
                          <span className="text-[11px] text-cyan-400 font-mono">{s.bonusDesc}</span>
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
                        {isMax ? 'MAX' : `${formatNumber(cost)} C#`}
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          {/* TAB 2: ДАЙДЖЕСТ, TIME WARP И ЯНДЕКС РЕКЛАМА */}
          {activeTab === 'digest' && (
            <div className="space-y-3 font-mono">
              {/* Рекламные бусты Яндекс Игр */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-indigo-950/40 to-slate-900 border border-indigo-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Tv className="w-5 h-5 text-indigo-400" />
                    <div>
                      <h3 className="text-sm font-bold text-indigo-300">БОНУСЫ ЯНДЕКС ИГР</h3>
                      <p className="text-xs text-slate-400">Просмотр короткого ролика за супер-буст</p>
                    </div>
                  </div>
                  {adBoostRemainingSec > 0 && (
                    <span className="text-xs text-emerald-400 bg-emerald-500/10 px-2 py-1 rounded-md border border-emerald-500/30 animate-pulse">
                      x2 АКТИВЕН ({adBoostRemainingSec}с)
                    </span>
                  )}
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  <button
                    onClick={watchAdForDoubleBoost}
                    className="p-3 rounded-xl bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 text-white font-bold text-xs flex flex-col items-center justify-center gap-1 shadow-md transition-all active:scale-98"
                  >
                    <span>📺 x2 ДОХОД НА 3 МИНУТЫ</span>
                    <span className="text-[10px] font-normal opacity-90">Удваивает весь C# и рубли</span>
                  </button>

                  <button
                    onClick={watchAdForTimeWarpReset}
                    className="p-3 rounded-xl bg-gradient-to-r from-cyan-600 to-blue-600 hover:from-cyan-500 hover:to-blue-500 text-white font-bold text-xs flex flex-col items-center justify-center gap-1 shadow-md transition-all active:scale-98"
                  >
                    <span>📺 СБРОСИТЬ TIME WARP</span>
                    <span className="text-[10px] font-normal opacity-90">Мгновенная зарядка 2h варпа</span>
                  </button>
                </div>
              </div>

              {/* Daily Digest Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-amber-950/30 to-slate-900 border border-amber-500/30 space-y-2.5">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">📋</span>
                    <div>
                      <h3 className="text-sm font-bold text-amber-300">УТРЕННИЙ ДАЙДЖЕСТ</h3>
                      <p className="text-xs text-slate-400">Сборов: {dailyDigestClaims}</p>
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
                  💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ
                </button>
              </div>

              {/* Time Warp Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-cyan-950/30 to-slate-900 border border-cyan-500/30 space-y-2.5">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">⚡</span>
                    <div>
                      <h3 className="text-sm font-bold text-cyan-300">TIME WARP: СИМУЛЯТОР СМЕНЫ</h3>
                      <p className="text-xs text-slate-400">Мгновенная автономная выработка за 2 часа</p>
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
                    ? `⏳ ЗАРЯДКА: ${Math.floor(timeWarpRemainingSec / 60)}:${(timeWarpRemainingSec % 60).toString().padStart(2, '0')}`
                    : '⚡ ЗАПУСТИТЬ TIME WARP (2 ЧАСА)'}
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
                    <h3 className="text-sm font-bold text-emerald-300">ВЫХОД НА IPO (ПРЕСТИЖ)</h3>
                    <p className="text-xs text-slate-400">Проведено IPO: {prestigeCount} раз</p>
                  </div>
                </div>

                <p className="text-xs text-slate-300 font-sans leading-relaxed">
                  Продайте акции компании инвесторам на бирже. Строки кода и базовые апгрейды сбрасываются, но вы получаете <b>Токены Акций</b> и <b>постоянный множитель x1.5</b> на все будущие сессии!
                </p>

                <div className="p-3 rounded-xl bg-slate-950/80 border border-slate-800 flex items-center justify-between text-xs">
                  <span className="text-slate-400">Акции в портфеле:</span>
                  <span className="text-emerald-400 font-bold">{prestigeTokens} шт. (+{(prestigeTokens * 5)}% буст)</span>
                </div>

                <div className="p-3 rounded-xl bg-slate-950/80 border border-slate-800 flex items-center justify-between text-xs">
                  <span className="text-slate-400">Будет начислено при IPO:</span>
                  <span className="text-amber-400 font-bold">+{potentialTokens} Токенов</span>
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
                  {potentialTokens > 0 ? `🚀 ПРОВЕСТИ IPO (+${potentialTokens} ТОКЕНОВ)` : 'ТРЕБУЕТСЯ БОЛЬШЕ КОДА ДЛЯ IPO'}
                </button>
              </div>
            </div>
          )}

          {/* TAB 4: КАСТОМИЗАЦИЯ СВИТЧЕЙ И ЗВУКА */}
          {activeTab === 'custom' && (
            <div className="space-y-3 font-mono text-xs">
              <div className="flex items-center gap-2 text-indigo-400 font-bold mb-1">
                <Sliders className="w-4 h-4" />
                <span>МЕХАНИЧЕСКИЕ ПЕРЕКЛЮЧАТЕЛИ КЛАВИАТУРЫ</span>
              </div>
              <p className="text-slate-400 font-sans">
                Выберите тип механических свитчей для изменения звукового профиля синтезатора:
              </p>

              <div className="grid grid-cols-2 gap-2">
                {[
                  { id: 'blue', name: 'Blue Clicky', desc: 'Громкий звонкий щелчок', icon: '🔵' },
                  { id: 'red', name: 'Red Linear', desc: 'Тихий мягкий ход', icon: '🔴' },
                  { id: 'brown', name: 'Brown Tactile', desc: 'Четкий тактильный бумп', icon: '🟤' },
                  { id: 'laser', name: 'Cyber Laser', desc: 'Синтезаторный лазер', icon: '⚡' },
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
            </div>
          )}

          {/* TAB 5: СОХРАНЕНИЯ */}
          {activeTab === 'save' && (
            <div className="space-y-4 font-mono text-xs">
              <div className="p-3.5 rounded-2xl bg-slate-900 border border-slate-800 space-y-2.5">
                <div className="flex items-center gap-2 text-cyan-400 font-bold">
                  <Copy className="w-4 h-4" />
                  <span>ЭКСПОРТ СОХРАНЕНИЯ (BASE64)</span>
                </div>
                <button
                  onClick={handleCopy}
                  className="w-full py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 flex items-center justify-center gap-2 font-semibold transition-all"
                >
                  {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
                  <span>{copied ? 'СКОПИРОВАНО В БУФЕР!' : 'КОПИРОВАТЬ КЛЮЧ СОХРАНЕНИЯ'}</span>
                </button>
              </div>

              <div className="p-3.5 rounded-2xl bg-slate-900 border border-slate-800 space-y-2.5">
                <div className="flex items-center gap-2 text-emerald-400 font-bold">
                  <Save className="w-4 h-4" />
                  <span>ИМПОРТ СОХРАНЕНИЯ</span>
                </div>
                <input
                  type="text"
                  placeholder="Вставьте код HELLOTAP_SAVE_V2:..."
                  value={importInput}
                  onChange={(e) => setImportInput(e.target.value)}
                  className="w-full px-3 py-2 rounded-xl bg-slate-950 border border-slate-800 text-slate-200 focus:outline-none focus:border-cyan-500 font-mono text-xs"
                />
                <button
                  onClick={handleImport}
                  className="w-full py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-slate-950 font-bold tracking-wide transition-all shadow-[0_0_12px_rgba(16,185,129,0.3)]"
                >
                  ЗАГРУЗИТЬ ПРОГРЕСС
                </button>
                {importMsg && <div className="text-center mt-1">{importMsg}</div>}
              </div>

              <div className="p-3.5 rounded-2xl bg-red-950/20 border border-red-900/40 space-y-2">
                <div className="flex items-center gap-2 text-red-400 font-bold">
                  <AlertTriangle className="w-4 h-4" />
                  <span>ОПАСНАЯ ЗОНА</span>
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
                  {resetConfirm ? '❓ ТОЧНО СБРОСИТЬ? НАЖМИТЕ ЕЩЁ РАЗ' : '⚠️ СБРОСИТЬ ВЕСЬ ПРОГРЕСС'}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
