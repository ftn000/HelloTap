import React, { useState, useMemo } from 'react';
import { useGame } from '../context/GameContext';
import { X, GitCommit, Flame, Trophy, Sparkles, CheckCircle2, ShieldCheck, Zap } from 'lucide-react';
import confetti from 'canvas-confetti';
import { sounds } from '../utils/soundEffects';
import { getContributionLevel, getContributionColor, getDevRank, calculateStreak } from '../utils/githubHeatmap';

interface GitHubHeatmapModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const GitHubHeatmapModal: React.FC<GitHubHeatmapModalProps> = ({ isOpen, onClose }) => {
  const { 
    contributions, 
    recordContribution, 
    totalContributions, 
    currentStreak, 
    devReputationBonus, 
    pipelinesPassed,
    lang 
  } = useGame();

  const [hoveredDay, setHoveredDay] = useState<{ date: string; count: number } | null>(null);

  const { longestStreak } = useMemo(() => calculateStreak(contributions), [contributions]);
  const devRank = useMemo(() => getDevRank(totalContributions), [totalContributions]);

  // Генерируем 52 недели по 7 дней
  const heatmapData = useMemo(() => {
    const today = new Date();
    const weeks: { date: string; count: number; level: 0 | 1 | 2 | 3 | 4 }[][] = [];
    
    // Находим воскресенье 52 недели назад
    const startDate = new Date(today);
    startDate.setDate(today.getDate() - (52 * 7 - 1));
    // Выравниваем на начало недели (воскресенье)
    startDate.setDate(startDate.getDate() - startDate.getDay());

    const cur = new Date(startDate);
    for (let w = 0; w < 53; w++) {
      const week: { date: string; count: number; level: 0 | 1 | 2 | 3 | 4 }[] = [];
      for (let d = 0; d < 7; d++) {
        const dateKey = cur.toISOString().split('T')[0];
        const isFuture = cur > today;
        const count = isFuture ? 0 : (contributions[dateKey] || 0);
        const level = isFuture ? 0 : getContributionLevel(count);
        week.push({ date: dateKey, count, level });
        cur.setDate(cur.getDate() + 1);
      }
      weeks.push(week);
    }
    return weeks;
  }, [contributions]);

  // Названия месяцев над сеткой
  const monthLabels = useMemo(() => {
    const months = ['Янв', 'Фев', 'Мар', 'Апр', 'Май', 'Июн', 'Июл', 'Авг', 'Сен', 'Окт', 'Ноя', 'Дек'];
    const monthsEn = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const monthsTr = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara'];
    const labels: { name: string; weekIdx: number }[] = [];
    
    let lastMonth = -1;
    heatmapData.forEach((week, wIdx) => {
      const firstDay = new Date(week[0].date);
      const m = firstDay.getMonth();
      if (m !== lastMonth && wIdx > 0 && wIdx < 50) {
        labels.push({
          name: lang === 'ru' ? months[m] : lang === 'tr' ? monthsTr[m] : monthsEn[m],
          weekIdx: wIdx
        });
        lastMonth = m;
      }
    });
    return labels;
  }, [heatmapData, lang]);

  if (!isOpen) return null;

  const handleManualCommit = () => {
    recordContribution(1);
    sounds.playKeyClick(true);
    sounds.triggerHaptic('medium');

    confetti({
      particleCount: 25,
      spread: 45,
      origin: { y: 0.6 },
      colors: ['#26a641', '#39d353', '#006d32', '#F59E0B']
    });
  };

  const weekDays = lang === 'ru' 
    ? ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'] 
    : ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-3 sm:p-4 bg-black/85 backdrop-blur-md animate-in fade-in duration-150 font-mono">
      <div className="relative w-full max-w-3xl bg-cyber-card border border-cyber-border rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[94vh]">
        {/* Хедер */}
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-cyber-border bg-slate-900/90">
          <div className="flex items-center gap-2.5">
            <span className="text-2xl">🐙</span>
            <div>
              <h2 className="text-sm sm:text-base font-bold text-white tracking-wide flex items-center gap-2">
                <span>GITHUB CONTRIBUTION HEATMAP</span>
                <span className="text-[10px] px-2 py-0.5 rounded-full bg-emerald-500/20 text-emerald-300 border border-emerald-500/40">
                  +{(devReputationBonus * 100).toFixed(1)}% REPUTATION
                </span>
              </h2>
              <p className="text-[10px] text-slate-400 font-sans">
                {lang === 'ru' 
                  ? 'График активности разработчика и коммитов в репозиторий за последние 365 дней'
                  : 'Developer activity graph and commits to the repository over the last 365 days'}
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="p-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Профиль разработчика и сводка */}
        <div className="p-4 bg-slate-950/60 border-b border-slate-800/80 grid grid-cols-2 sm:grid-cols-4 gap-2.5">
          <div className="p-2.5 rounded-2xl bg-slate-900/80 border border-slate-800 flex flex-col">
            <span className="text-[10px] text-slate-400">{lang === 'ru' ? 'Ранг разработчика' : lang === 'tr' ? 'Geliştirici rütbesi' : 'Developer rank'}</span>
            <div className="flex items-center gap-1.5 mt-0.5">
              <span className="text-base">{devRank.badge}</span>
              <span className={`text-xs font-bold truncate ${devRank.color}`}>{devRank.title}</span>
            </div>
          </div>

          <div className="p-2.5 rounded-2xl bg-slate-900/80 border border-slate-800 flex flex-col">
            <span className="text-[10px] text-slate-400">{lang === 'ru' ? 'Всего вкладов (365д)' : lang === 'tr' ? 'Toplam katkı (365g)' : 'Total contributions (365d)'}</span>
            <div className="flex items-center gap-1.5 mt-0.5">
              <GitCommit className="w-4 h-4 text-emerald-400" />
              <span className="text-sm font-bold text-emerald-300">{totalContributions} commits</span>
            </div>
          </div>

          <div className="p-2.5 rounded-2xl bg-slate-900/80 border border-slate-800 flex flex-col">
            <span className="text-[10px] text-slate-400">{lang === 'ru' ? 'Активный стрик дней' : lang === 'tr' ? 'Aktif seri günleri' : 'Active day streak'}</span>
            <div className="flex items-center gap-1.5 mt-0.5">
              <Flame className="w-4 h-4 text-amber-400" />
              <span className="text-sm font-bold text-amber-300">{currentStreak} {lang === 'ru' ? 'дн.' : lang === 'tr' ? 'gün' : 'days'} ({lang === 'ru' ? 'макс' : lang === 'tr' ? 'maks' : 'max'}: {longestStreak})</span>
            </div>
          </div>

          <div className="p-2.5 rounded-2xl bg-slate-900/80 border border-slate-800 flex flex-col">
            <span className="text-[10px] text-slate-400">{lang === 'ru' ? 'CI/CD Пайплайнов' : lang === 'tr' ? 'CI/CD İş Akışları' : 'CI/CD Pipelines'}</span>
            <div className="flex items-center gap-1.5 mt-0.5">
              <CheckCircle2 className="w-4 h-4 text-cyan-400" />
              <span className="text-sm font-bold text-cyan-300">{pipelinesPassed} passed</span>
            </div>
          </div>
        </div>

        {/* Сетка Heatmap */}
        <div className="p-4 sm:p-5 overflow-y-auto flex-1 flex flex-col items-center">
          <div className="w-full overflow-x-auto pb-2 no-scrollbar">
            <div className="min-w-[690px] mx-auto">
              {/* Месяцы */}
              <div className="flex text-[9px] text-slate-400 mb-1 ml-6 relative h-4">
                {monthLabels.map((m, idx) => (
                  <span
                    key={idx}
                    style={{ left: `${m.weekIdx * 12.8}px` }}
                    className="absolute"
                  >
                    {m.name}
                  </span>
                ))}
              </div>

              {/* Дни недели и сетка */}
              <div className="flex items-start gap-1.5">
                {/* Лейблы дней недели */}
                <div className="flex flex-col gap-[3px] text-[8px] text-slate-500 pt-0.5 w-5 text-right select-none">
                  {weekDays.map((dName, dIdx) => (
                    <span key={dIdx} className="h-[10px] leading-[10px]">{dName}</span>
                  ))}
                </div>

                {/* 52 колонки недель */}
                <div className="flex gap-[3px]">
                  {heatmapData.map((week, wIdx) => (
                    <div key={wIdx} className="flex flex-col gap-[3px]">
                      {week.map((day, dIdx) => {
                        const colorClass = getContributionColor(day.level);
                        return (
                          <div
                            key={dIdx}
                            onMouseEnter={() => setHoveredDay({ date: day.date, count: day.count })}
                            onMouseLeave={() => setHoveredDay(null)}
                            className={`w-[10px] h-[10px] rounded-[2px] border transition-transform hover:scale-125 cursor-pointer ${colorClass}`}
                            title={lang === 'ru' ? `${day.date}: ${day.count} контрибуций` : lang === 'tr' ? `${day.date}: ${day.count} katkı` : `${day.date}: ${day.count} contributions`}
                          />
                        );
                      })}
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          {/* Подвал сетки: Тултип & Легенда */}
          <div className="w-full flex items-center justify-between mt-3 text-[10px] text-slate-400 px-2 flex-wrap gap-2">
            <div>
              {hoveredDay ? (
                <span className="text-cyan-300 font-semibold">
                  📌 {hoveredDay.date}: <strong className="text-white">{hoveredDay.count}</strong> {lang === 'ru' ? 'вкладов в код' : lang === 'tr' ? 'kod katkısı' : 'code contributions'}
                </span>
              ) : (
                <span className="text-slate-500">{lang === 'ru' ? 'Наведите на ячейку для просмотра деталей' : lang === 'tr' ? 'Ayrıntıları görmek için hücrenin üzerine gelin' : 'Hover over a cell to view details'}</span>
              )}
            </div>

            {/* Легенда цветов */}
            <div className="flex items-center gap-1 font-sans">
              <span className="text-[10px] text-slate-500 mr-1">{lang === 'ru' ? 'Меньше' : lang === 'tr' ? 'Az' : 'Less'}</span>
              <div className="w-2.5 h-2.5 rounded-[2px] bg-slate-900 border border-slate-800" />
              <div className="w-2.5 h-2.5 rounded-[2px] bg-[#0e4429]" />
              <div className="w-2.5 h-2.5 rounded-[2px] bg-[#006d32]" />
              <div className="w-2.5 h-2.5 rounded-[2px] bg-[#26a641]" />
              <div className="w-2.5 h-2.5 rounded-[2px] bg-[#39d353]" />
              <span className="text-[10px] text-slate-500 ml-1">{lang === 'ru' ? 'Больше' : lang === 'tr' ? 'Çok' : 'More'}</span>
            </div>
          </div>

          {/* Интерактивная кнопка коммита */}
          <div className="mt-5 w-full flex items-center justify-center gap-3">
            <button
              onClick={handleManualCommit}
              className="px-4 py-2.5 rounded-2xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs flex items-center gap-2 shadow-[0_0_15px_rgba(16,185,129,0.4)] active:scale-95 transition-all cursor-pointer"
            >
              <GitCommit className="w-4 h-4" />
              <span>{lang === 'ru' ? 'Совершить коммит активности (+1 вклад)' : lang === 'tr' ? 'Aktivite commiti yap (+1 katkı)' : 'Make activity commit (+1 contribution)'}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
