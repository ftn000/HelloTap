import React from 'react';
import { useGame } from '../context/GameContext';
import { formatNumber } from '../utils/numberFormatter';
import { Flame, Sparkles, Volume2, VolumeX, Building2, Save, FileText, Crown, Trophy, Music, Award, GitBranch } from 'lucide-react';
import { sounds } from '../utils/soundEffects';

interface HeaderHUDProps {
  onOpenHub: () => void;
  onOpenShop: () => void;
  onOpenDigest: () => void;
  onOpenSave: () => void;
  onOpenLeaderboard: () => void;
  onOpenAchievements: () => void;
  onOpenTechTree: () => void;
  onOpenHeatmap?: () => void;
}

export const HeaderHUD: React.FC<HeaderHUDProps> = ({ onOpenHub, onOpenShop, onOpenDigest, onOpenSave, onOpenLeaderboard, onOpenAchievements, onOpenTechTree, onOpenHeatmap }) => {
  const { codeLines, money, codePerSec, moneyPerSec, globalMultiplier, isInFlow, comboEnergy, adBoostRemainingSec, lang, setLang, t, isMusicPlaying, toggleMusic, currentTrackName, nextMusicTrack, achievements, skillPoints, currentStreak } = useGame();
  const [muted, setMuted] = React.useState(sounds.isMuted);

  const totalAchStars = Object.values(achievements).reduce((sum, tier) => sum + tier, 0);

  const toggleMute = () => {
    sounds.isMuted = !sounds.isMuted;
    setMuted(sounds.isMuted);
  };

  return (
    <header className="sticky top-0 z-30 bg-cyber-bg/95 backdrop-blur-md border-b border-cyber-border px-4 py-3">
      <div className="max-w-4xl mx-auto flex items-center justify-between gap-3">
        {/* Баланс Кода и Денег */}
        <div className="flex items-center gap-4">
          <div className="flex flex-col">
            <div className="flex items-center gap-1.5">
              <span className="text-xl">💻</span>
              <span className="text-xl sm:text-2xl font-bold font-mono tracking-tight text-cyber-neonBlue">
                {formatNumber(codeLines)}
              </span>
              <span className="text-xs text-cyan-400/80 font-mono">C#</span>
            </div>
            <span className="text-[11px] text-slate-400 font-mono">
              +{formatNumber(codePerSec)}/сек
            </span>
          </div>

          <div className="h-8 w-[1px] bg-slate-800" />

          <div className="flex flex-col">
            <div className="flex items-center gap-1.5">
              <span className="text-xl">💰</span>
              <span className="text-xl sm:text-2xl font-bold font-mono tracking-tight text-emerald-400">
                {formatNumber(money)}
              </span>
              <span className="text-xs text-emerald-500/80 font-mono">₽</span>
            </div>
            <span className="text-[11px] text-slate-400 font-mono">
              +{formatNumber(moneyPerSec)}/сек
            </span>
          </div>
        </div>

        {/* Множитель и Быстрые Кнопки */}
        <div className="flex items-center gap-2">
          {/* Индикатор Активного Яндекс Буста x2 */}
          {adBoostRemainingSec > 0 && (
            <div className="flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold font-mono bg-indigo-500/20 text-indigo-300 border border-indigo-500/50 animate-pulse">
              <span className="text-xs">📺</span>
              <span>x2 ({adBoostRemainingSec}с)</span>
            </div>
          )}

          {/* Индикатор Комбо «В Потоке» */}
          <div className={`hidden sm:flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold font-mono transition-all ${
            isInFlow 
              ? 'bg-amber-500/20 text-amber-400 border border-amber-500/50 shadow-[0_0_12px_rgba(245,158,11,0.3)] animate-pulse' 
              : 'bg-slate-800/60 text-slate-400 border border-slate-700/50'
          }`}>
            <Flame className={`w-3.5 h-3.5 ${isInFlow ? 'text-amber-400 animate-bounce' : 'text-slate-500'}`} />
            <span>x{(globalMultiplier * (isInFlow ? 3.0 : 1.0)).toFixed(1)}</span>
            {isInFlow && <span className="text-[10px] text-amber-300 uppercase tracking-wider">FLOW</span>}
          </div>

          {/* Кнопка смены языка */}
          <button
            onClick={() => setLang(lang === 'ru' ? 'en' : 'ru')}
            className="px-2 py-1 rounded-xl bg-slate-800/80 hover:bg-slate-700/80 border border-slate-700 text-xs font-mono font-bold text-slate-200 transition-colors"
            title="Язык / Language"
          >
            {lang === 'ru' ? '🇷🇺 RU' : '🇬🇧 EN'}
          </button>

          {/* Кнопка Mute */}
          <button
            onClick={toggleMute}
            className="p-2 rounded-xl bg-slate-800/60 hover:bg-slate-700/60 border border-slate-700/60 text-slate-300 transition-colors"
            title="Звук"
          >
            {muted ? <VolumeX className="w-4 h-4 text-red-400" /> : <Volume2 className="w-4 h-4 text-slate-300" />}
          </button>

          {/* Кнопка Дайджест */}
          <button
            onClick={onOpenDigest}
            className="p-2 rounded-xl bg-amber-500/10 hover:bg-amber-500/20 border border-amber-500/30 text-amber-400 transition-all shadow-[0_0_10px_rgba(245,158,11,0.15)]"
            title={t.digestBtn}
          >
            <FileText className="w-4 h-4" />
          </button>

          {/* Lo-Fi / Synthwave Музыкальный плеер */}
          <div className="flex items-center gap-1 bg-slate-800/60 p-1 rounded-xl border border-slate-700/60">
            <button
              onClick={toggleMusic}
              className={`p-1.5 rounded-lg border transition-all ${
                isMusicPlaying
                  ? 'bg-purple-500/25 text-purple-300 border-purple-500/50 shadow-[0_0_12px_rgba(168,85,247,0.4)] animate-pulse'
                  : 'bg-transparent text-slate-400 border-transparent hover:text-slate-200'
              }`}
              title={isMusicPlaying ? `Сейчас играет: ${currentTrackName} (Клик для паузы)` : (lang === 'ru' ? 'Включить Lo-Fi Synthwave Музыку' : 'Play Lo-Fi Ambient')}
            >
              <Music className="w-3.5 h-3.5" />
            </button>
            {isMusicPlaying && (
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  nextMusicTrack();
                }}
                className="px-1 py-0.5 rounded text-[10px] text-purple-300 hover:text-white hover:bg-purple-500/20 font-mono transition-colors"
                title={`След. трек (${currentTrackName})`}
              >
                ⏭️
              </button>
            )}
          </div>

          {/* Кнопка Достижений (на мобильных перенесена в нижний бар) */}
          <button
            onClick={onOpenAchievements}
            className="hidden sm:flex relative p-2 rounded-xl bg-amber-500/10 hover:bg-amber-500/20 border border-amber-500/30 text-amber-400 transition-all shadow-[0_0_10px_rgba(245,158,11,0.15)]"
            title={lang === 'ru' ? 'Достижения и Награды' : 'Achievements'}
          >
            <Award className="w-4 h-4" />
            {totalAchStars > 0 && (
              <span className="absolute -top-1 -right-1 px-1 min-w-[15px] h-3.5 rounded-full bg-amber-500 text-slate-950 font-mono font-bold text-[9px] flex items-center justify-center">
                {totalAchStars}
              </span>
            )}
          </button>

          {/* Кнопка Дерева Талантов (на мобильных перенесена в нижний бар) */}
          <button
            onClick={onOpenTechTree}
            className={`hidden sm:flex relative p-2 rounded-xl border transition-all ${
              skillPoints > 0
                ? 'bg-indigo-500/20 text-indigo-300 border-indigo-500/50 shadow-[0_0_12px_rgba(99,102,241,0.3)] animate-pulse'
                : 'bg-slate-800/60 text-slate-400 border-slate-700/60 hover:text-indigo-300'
            }`}
            title="Дерево IT-навыков и талантов [K]"
          >
            <GitBranch className="w-4 h-4" />
            {skillPoints > 0 && (
              <span className="absolute -top-1 -right-1 px-1 min-w-[15px] h-3.5 rounded-full bg-indigo-500 text-white font-mono font-bold text-[9px] flex items-center justify-center shadow-md">
                {skillPoints}
              </span>
            )}
          </button>

          {/* Кнопка Рейтинга Лидерборда (на мобильных перенесена в нижний бар) */}
          <button
            onClick={onOpenLeaderboard}
            className="hidden sm:flex p-2 rounded-xl bg-amber-500/10 hover:bg-amber-500/20 border border-amber-500/30 text-amber-400 transition-all shadow-[0_0_10px_rgba(245,158,11,0.15)]"
            title={t.leaderboardBtn}
          >
            <Trophy className="w-4 h-4 text-amber-400" />
          </button>

          {/* Кнопка GitHub Heatmap */}
          {onOpenHeatmap && (
            <button
              onClick={onOpenHeatmap}
              className="relative p-2 rounded-xl bg-emerald-500/10 hover:bg-emerald-500/20 border border-emerald-500/30 text-emerald-400 transition-all shadow-[0_0_10px_rgba(16,185,129,0.15)]"
              title="GitHub Contribution Heatmap (График вкладов)"
            >
              <span className="text-sm">🐙</span>
              {currentStreak > 0 && (
                <span className="absolute -top-1 -right-1 px-1 min-w-[15px] h-3.5 rounded-full bg-emerald-500 text-slate-950 font-mono font-bold text-[9px] flex items-center justify-center">
                  {currentStreak}🔥
                </span>
              )}
            </button>
          )}

          {/* Кнопка Сохранений */}
          <button
            onClick={onOpenSave}
            className="p-2 rounded-xl bg-cyan-500/10 hover:bg-cyan-500/20 border border-cyan-500/30 text-cyan-400 transition-all"
            title={t.saveBtn}
          >
            <Save className="w-4 h-4" />
          </button>

          {/* Кнопка VIP Донат */}
          <button
            onClick={onOpenShop}
            className="flex items-center gap-1 p-2 sm:px-2.5 sm:py-2 rounded-xl bg-amber-500/15 hover:bg-amber-500/25 border border-amber-500/40 text-amber-300 font-mono text-xs font-bold transition-all shadow-[0_0_12px_rgba(245,158,11,0.2)] active:scale-95"
            title={t.tabShop}
          >
            <Crown className="w-4 h-4 text-amber-400" />
            <span className="hidden sm:inline">{t.tabShop}</span>
          </button>

          {/* Кнопка Hub Студии (на мобильных перенесена в нижний бар) */}
          <button
            onClick={onOpenHub}
            className="hidden sm:flex items-center gap-1.5 px-3 py-2 rounded-xl bg-gradient-to-r from-cyan-500 to-blue-600 hover:from-cyan-400 hover:to-blue-500 text-white font-medium text-xs shadow-[0_0_15px_rgba(6,182,212,0.3)] transition-all active:scale-95"
          >
            <Building2 className="w-4 h-4" />
            <span className="hidden sm:inline">{t.hubBtn}</span>
          </button>
        </div>
      </div>
    </header>
  );
};
