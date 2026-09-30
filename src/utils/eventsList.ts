import { GameRandomEvent } from '../types/events';

export const generateRandomEvent = (codePerSec: number, moneyPerSec: number): GameRandomEvent => {
  const effectiveCps = Math.max(10, codePerSec);
  const effectiveMps = Math.max(5, moneyPerSec);

  const eventPool: GameRandomEvent[] = [
    {
      id: `hacker_${Date.now()}`,
      type: 'hacker_attack',
      title: '🚨 ДДОС-АТАКА НА ПРОДАКШН!',
      subtitle: 'Система безопасности зафиксировала терабит SYN-флуда',
      description: 'Боты атакуют ваши шлюзы API. Серверы начинают греться! Что предпринимаем?',
      icon: '👾',
      gradient: 'from-red-950/80 via-slate-900 to-red-950/40',
      borderColor: 'border-red-500/60',
      glowColor: 'rgba(239, 68, 68, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_waf',
          title: '🛡️ Развернуть Cloudflare WAF',
          description: `Отразить атаку и получить компенсацию от страховки: +${Math.round(effectiveMps * 60)} ₽`,
          badge: '+Рубли',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(500, Math.round(effectiveMps * 60))
        },
        {
          id: 'opt_counter_hack',
          title: '⚡ Встречный кибер-удар',
          description: `Захватить ботнет: +${Math.round(effectiveCps * 45)} строк чистого кода C#!`,
          badge: '+Код C#',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'grant_code',
          value: Math.max(300, Math.round(effectiveCps * 45))
        }
      ]
    },
    {
      id: `angel_${Date.now()}`,
      type: 'angel_investor',
      title: '💼 ВИЗИТ ВЕНЧУРНОГО АНГЕЛА',
      subtitle: 'Инвестор из Кремниевой Долины заглянул на ваш кофе-брейк',
      description: 'Он впечатлен скоростью коммитов вашей команды и предлагает сделку прямо сейчас.',
      icon: '💎',
      gradient: 'from-amber-950/80 via-slate-900 to-amber-950/40',
      borderColor: 'border-amber-500/60',
      glowColor: 'rgba(245, 158, 11, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_seed_check',
          title: '💰 Взять Seed-инвестиции',
          description: `Мгновенный чек в капитал студии: +${Math.round(effectiveMps * 90)} ₽`,
          badge: 'Крупный чек ₽',
          badgeColor: 'bg-amber-500/20 text-amber-300 border-amber-500/40',
          actionType: 'grant_money',
          value: Math.max(1000, Math.round(effectiveMps * 90))
        },
        {
          id: 'opt_bootstrap_pride',
          title: '🔥 Отказаться и работать в потоке',
          description: 'Мгновенный вход в режим гиперфокуса (Flow State x2.5) на 30 секунд!',
          badge: 'Flow State x2.5',
          badgeColor: 'bg-indigo-500/20 text-indigo-300 border-indigo-500/40',
          actionType: 'boost_flow',
          value: 30
        }
      ]
    },
    {
      id: `bug_bounty_${Date.now()}`,
      type: 'bug_bounty',
      title: '🐞 0-DAY КРИТИЧЕСКИЙ ЭКСПЛОЙТ',
      subtitle: 'Стажер нашел уязвимость переполнения буфера в ядре',
      description: 'Дыра в безопасности позволяет либо быстро пропатчить систему, либо сдать ее в Bug Bounty.',
      icon: '🔬',
      gradient: 'from-purple-950/80 via-slate-900 to-purple-950/40',
      borderColor: 'border-purple-500/60',
      glowColor: 'rgba(168, 85, 247, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_patch',
          title: '🛠️ Экстренный хотфикс в Production',
          description: `Код оптимизирован: +${Math.round(effectiveCps * 60)} строк C# мгновенно!`,
          badge: 'Хотфикс C#',
          badgeColor: 'bg-purple-500/20 text-purple-300 border-purple-500/40',
          actionType: 'grant_code',
          value: Math.max(500, Math.round(effectiveCps * 60))
        },
        {
          id: 'opt_bounty_reward',
          title: '🏆 Сдать отчет в Bug Bounty',
          description: `Премия от корпорации за ответственное раскрытие: +${Math.round(effectiveMps * 75)} ₽`,
          badge: '+Баунти ₽',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(800, Math.round(effectiveMps * 75))
        }
      ]
    },
    {
      id: `coffee_${Date.now()}`,
      type: 'coffee_rush',
      title: '☕ СПЕЦИАЛЬНЫЙ ТРОЙНОЙ COLD BREW',
      subtitle: 'Курьер доставил фирменный крафтовый кофе для разработчиков',
      description: 'Аромат свежей арабики и двойная доза кофеина заряжают весь офис невероятной энергией!',
      icon: '⚡',
      gradient: 'from-cyan-950/80 via-slate-900 to-cyan-950/40',
      borderColor: 'border-cyan-500/60',
      glowColor: 'rgba(6, 182, 212, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_drink_solo',
          title: '🚀 Выпить залпом в одиночку',
          description: '100% шкалы комбо и моментальный Flow State на 35 секунд!',
          badge: 'Супер-Фокус',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'boost_flow',
          value: 35
        },
        {
          id: 'opt_share_team',
          title: '👥 Разделить кофе с командой',
          description: `Все серверы пишут код быстрее: бонус +${Math.round(effectiveCps * 50)} строк C#!`,
          badge: 'Буст команды',
          badgeColor: 'bg-amber-500/20 text-amber-300 border-amber-500/40',
          actionType: 'grant_code',
          value: Math.max(400, Math.round(effectiveCps * 50))
        }
      ]
    }
  ];

  const randomIndex = Math.floor(Math.random() * eventPool.length);
  return eventPool[randomIndex];
};
