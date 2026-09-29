import React, { useState } from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { X, Building2, Zap, Save, Copy, Check, AlertTriangle, ArrowRight, RefreshCw } from 'lucide-react';
import confetti from 'canvas-confetti';

interface StudioHubModalProps {
  isOpen: boolean;
  onClose: () => void;
  defaultTab?: 'systems' | 'digest' | 'save';
}

export const StudioHubModal: React.FC<StudioHubModalProps> = ({ isOpen, onClose, defaultTab = 'systems' }) => {
  const [activeTab, setActiveTab] = useState<'systems' | 'digest' | 'save'>(defaultTab);
  const {
    systems,
    upgradeSystem,
    codeLines,
    claimDailyDigest,
    dailyDigestClaims,
    triggerTimeWarp,
    timeWarpRemainingSec,
    exportSaveBase64,
    importSaveBase64,
    hardReset
  } = useGame();

  const [copied, setCopied] = useState<boolean>(false);
  const [importInput, setImportInput] = useState<string>('');
  const [importMsg, setImportMsg] = useState<string>('');
  const [resetConfirm, setResetConfirm] = useState<boolean>(false);

  if (!isOpen) return null;

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

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="relative w-full max-w-lg bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-cyber-border bg-slate-900/60">
          <div className="flex items-center gap-2">
            <Building2 className="w-5 h-5 text-cyan-400" />
            <h2 className="text-base font-bold text-slate-100 font-mono">STUDIO OS — ЦЕНТР УПРАВЛЕНИЯ</h2>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Tabs */}
        <div className="grid grid-cols-3 gap-1 p-2 bg-slate-950/60 border-b border-cyber-border text-xs font-mono">
          <button
            onClick={() => setActiveTab('systems')}
            className={`py-2 rounded-xl font-semibold transition-all ${
              activeTab === 'systems' ? 'bg-cyan-500/20 text-cyan-400 border border-cyan-500/40 shadow-sm' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            🏢 Системы Офиса
          </button>
          <button
            onClick={() => setActiveTab('digest')}
            className={`py-2 rounded-xl font-semibold transition-all ${
              activeTab === 'digest' ? 'bg-amber-500/20 text-amber-400 border border-amber-500/40 shadow-sm' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            📋 Дайджест & Warp
          </button>
          <button
            onClick={() => setActiveTab('save')}
            className={`py-2 rounded-xl font-semibold transition-all ${
              activeTab === 'save' ? 'bg-purple-500/20 text-purple-400 border border-purple-500/40 shadow-sm' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            💾 Сохранения
          </button>
        </div>

        {/* Content Body */}
        <div className="p-4 overflow-y-auto space-y-4">
          {/* TAB 1: СИСТЕМЫ ОФИСА */}
          {activeTab === 'systems' && (
            <div className="space-y-3">
              <div className="text-xs text-slate-400">
                Модернизируйте инфраструктуру студии для получения глобальных множителей дохода:
              </div>

              {systems.map(s => {
                const cost = Math.floor(s.reqCode * Math.pow(1.5, s.level));
                const canAfford = codeLines >= cost && s.level < s.maxLevel;
                const isMax = s.level >= s.maxLevel;

                return (
                  <div
                    key={s.id}
                    className="p-3 rounded-2xl bg-slate-900/80 border border-slate-800 flex items-center justify-between gap-3"
                  >
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-xl bg-slate-800 flex items-center justify-center text-xl shrink-0">
                        {s.icon}
                      </div>
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="font-semibold text-sm text-slate-200">{s.title}</span>
                          <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-slate-800 text-slate-400">
                            ур. {s.level}/{s.maxLevel}
                          </span>
                        </div>
                        <p className="text-xs text-slate-400 mt-0.5">{s.description}</p>
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
          )}

          {/* TAB 2: ДАЙДЖЕСТ И TIME WARP */}
          {activeTab === 'digest' && (
            <div className="space-y-4 font-mono">
              {/* Daily Digest Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-amber-950/30 to-slate-900 border border-amber-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">📋</span>
                    <div>
                      <h3 className="text-sm font-bold text-amber-300">УТРЕННИЙ ДАЙДЖЕСТ СТУДИИ</h3>
                      <p className="text-xs text-slate-400">Собрано отчетов: {dailyDigestClaims}</p>
                    </div>
                  </div>
                  <span className="text-xs text-amber-400 bg-amber-500/10 px-2 py-1 rounded-md border border-amber-500/30">
                    +100% FLOW (x3.0)
                  </span>
                </div>

                <p className="text-xs text-slate-300 font-sans">
                  Активирует единый сводный сбор дивидендов студии, начисляет мгновенный финансовый бонус и переводит команду в Режим Потока!
                </p>

                <button
                  onClick={handleClaim}
                  className="w-full py-3 rounded-xl bg-gradient-to-r from-amber-500 to-yellow-500 hover:from-amber-400 hover:to-yellow-400 text-slate-950 font-bold text-xs uppercase tracking-wider shadow-[0_0_20px_rgba(245,158,11,0.4)] transition-all active:scale-98"
                >
                  💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ
                </button>
              </div>

              {/* Time Warp Box */}
              <div className="p-4 rounded-2xl bg-gradient-to-br from-cyan-950/30 to-slate-900 border border-cyan-500/30 space-y-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <span className="text-2xl">⚡</span>
                    <div>
                      <h3 className="text-sm font-bold text-cyan-300">TIME WARP: СИМУЛЯТОР СМЕНЫ</h3>
                      <p className="text-xs text-slate-400">Мгновенная автономная выработка за 2 часа</p>
                    </div>
                  </div>
                </div>

                <p className="text-xs text-slate-300 font-sans">
                  Ускоряет время студии на 7200 секунд. Начисляет 2 часа пассивного дохода команды и AI-отдела.
                </p>

                <button
                  onClick={triggerTimeWarp}
                  disabled={timeWarpRemainingSec > 0}
                  className={`w-full py-3 rounded-xl font-bold text-xs uppercase tracking-wider transition-all ${
                    timeWarpRemainingSec > 0
                      ? 'bg-slate-800 text-slate-500 cursor-not-allowed border border-slate-700'
                      : 'bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white shadow-[0_0_20px_rgba(6,182,212,0.4)] active:scale-98'
                  }`}
                >
                  {timeWarpRemainingSec > 0
                    ? `⏳ ЗАРЯДКА: ${Math.floor(timeWarpRemainingSec / 60)}:${(timeWarpRemainingSec % 60).toString().padStart(2, '0')}`
                    : '⚡ ЗАПУСТИТЬ TIME WARP (2 ЧАСА)'}
                </button>
              </div>
            </div>
          )}

          {/* TAB 3: ОБЛАКО И СОХРАНЕНИЯ */}
          {activeTab === 'save' && (
            <div className="space-y-4 font-mono text-xs">
              {/* Экспорт */}
              <div className="p-3.5 rounded-2xl bg-slate-900 border border-slate-800 space-y-2.5">
                <div className="flex items-center gap-2 text-cyan-400 font-bold">
                  <Copy className="w-4 h-4" />
                  <span>ЭКСПОРТ СОХРАНЕНИЯ (BASE64)</span>
                </div>
                <p className="text-slate-400 text-[11px] font-sans">
                  Скопируйте ключ сохранения для переноса прогресса между браузерами и устройствами:
                </p>
                <button
                  onClick={handleCopy}
                  className="w-full py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 border border-slate-700 flex items-center justify-center gap-2 font-semibold transition-all"
                >
                  {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
                  <span>{copied ? 'СКОПИРОВАНО В БУФЕР!' : 'КОПИРОВАТЬ КЛЮЧ СОХРАНЕНИЯ'}</span>
                </button>
              </div>

              {/* Импорт */}
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

              {/* Сброс */}
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
