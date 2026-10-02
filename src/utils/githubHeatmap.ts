export interface DayContribution {
  date: string;       // YYYY-MM-DD
  count: number;
  level: 0 | 1 | 2 | 3 | 4;
}

export const getContributionLevel = (count: number): 0 | 1 | 2 | 3 | 4 => {
  if (count <= 0) return 0;
  if (count <= 2) return 1;
  if (count <= 5) return 2;
  if (count <= 9) return 3;
  return 4;
};

export const getContributionColor = (level: 0 | 1 | 2 | 3 | 4): string => {
  switch (level) {
    case 0: return 'bg-slate-900 border-slate-800/80';
    case 1: return 'bg-[#0e4429] border-[#0e4429]/80 shadow-[0_0_4px_rgba(14,68,41,0.4)]';
    case 2: return 'bg-[#006d32] border-[#006d32]/80 shadow-[0_0_6px_rgba(0,109,50,0.5)]';
    case 3: return 'bg-[#26a641] border-[#26a641]/80 shadow-[0_0_8px_rgba(38,166,65,0.6)]';
    case 4: return 'bg-[#39d353] border-[#39d353]/80 shadow-[0_0_10px_rgba(57,211,83,0.8)]';
  }
};

export const getTodayKey = (): string => {
  return new Date().toISOString().split('T')[0];
};

export const generateBaselineContributions = (): Record<string, number> => {
  const result: Record<string, number> = {};
  const today = new Date();
  
  // 52 недели = 364 дня назад
  for (let i = 363; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const key = d.toISOString().split('T')[0];
    const dayOfWeek = d.getDay(); // 0 = вс, 6 = сб
    const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
    
    // Псевдослучайный паттерн с более высокой активностью в будни
    const seed = Math.sin(i * 12.9898 + 78.233) * 43758.5453;
    const rand = seed - Math.floor(seed);
    
    if (rand < 0.28) {
      result[key] = 0;
    } else if (isWeekend) {
      result[key] = Math.floor(rand * 3) + 1;
    } else {
      result[key] = Math.floor(rand * 8) + 1;
    }
  }
  
  // Сегодня гарантированно активный день
  result[getTodayKey()] = (result[getTodayKey()] || 0) + 3;
  return result;
};

export const calculateStreak = (contributions: Record<string, number>): { currentStreak: number; longestStreak: number; total: number } => {
  let total = 0;
  const today = new Date();
  
  // Считаем сумму
  for (let i = 363; i >= 0; i--) {
    const d = new Date(today);
    d.setDate(d.getDate() - i);
    const key = d.toISOString().split('T')[0];
    total += contributions[key] || 0;
  }
  
  // Считаем текущий стрик от вчера/сегодня назад
  let currentStreak = 0;
  let d = new Date(today);
  const todayKey = d.toISOString().split('T')[0];
  
  // Если сегодня есть коммит, начинаем с сегодня, иначе с вчера
  if ((contributions[todayKey] || 0) > 0) {
    currentStreak++;
    d.setDate(d.getDate() - 1);
  } else {
    d.setDate(d.getDate() - 1);
    const yesterdayKey = d.toISOString().split('T')[0];
    if ((contributions[yesterdayKey] || 0) === 0) {
      currentStreak = 0;
    }
  }
  
  while (true) {
    const key = d.toISOString().split('T')[0];
    if ((contributions[key] || 0) > 0) {
      currentStreak++;
      d.setDate(d.getDate() - 1);
    } else {
      break;
    }
  }
  
  // Считаем максимальный стрик
  let longestStreak = 0;
  let tempStreak = 0;
  for (let i = 363; i >= 0; i--) {
    const cur = new Date(today);
    cur.setDate(cur.getDate() - i);
    const key = cur.toISOString().split('T')[0];
    if ((contributions[key] || 0) > 0) {
      tempStreak++;
      if (tempStreak > longestStreak) longestStreak = tempStreak;
    } else {
      tempStreak = 0;
    }
  }
  
  return { currentStreak, longestStreak: Math.max(longestStreak, currentStreak), total };
};

export const getDevRank = (totalContributions: number): { title: string; badge: string; color: string } => {
  if (totalContributions >= 2000) return { title: 'Octocat Legend', badge: '🐙', color: 'text-emerald-400' };
  if (totalContributions >= 1400) return { title: 'Staff Architect', badge: '🏛️', color: 'text-purple-400' };
  if (totalContributions >= 900) return { title: 'Lead Engineer', badge: '💎', color: 'text-cyan-400' };
  if (totalContributions >= 500) return { title: 'Senior Developer', badge: '⚡', color: 'text-amber-400' };
  if (totalContributions >= 200) return { title: 'Middle Developer', badge: '🔥', color: 'text-blue-400' };
  return { title: 'Junior Coder', badge: '🌱', color: 'text-slate-400' };
};
