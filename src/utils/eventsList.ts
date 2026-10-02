import { GameRandomEvent } from '../types/events';
import { Language } from './i18n';

export const generateRandomEvent = (codePerSec: number, moneyPerSec: number, lang: Language = 'ru'): GameRandomEvent => {
  const effectiveCps = Math.max(10, codePerSec);
  const effectiveMps = Math.max(5, moneyPerSec);
  const isRu = lang === 'ru';

  const eventPool: GameRandomEvent[] = [
    {
      id: `hacker_${Date.now()}`,
      type: 'hacker_attack',
      title: isRu ? '🚨 ДДОС-АТАКА НА ПРОДАКШН!' : '🚨 PRODUCTION DDOS ATTACK!',
      subtitle: isRu ? 'Система безопасности зафиксировала терабит SYN-флуда' : 'Security systems logged 1 Tbps of SYN flood traffic',
      description: isRu 
        ? 'Боты атакуют ваши шлюзы API. Серверы начинают греться! Что предпринимаем?' 
        : 'Botnets are flooding API gateways. Servers are overheating! What should we do?',
      icon: '👾',
      gradient: 'from-red-950/80 via-slate-900 to-red-950/40',
      borderColor: 'border-red-500/60',
      glowColor: 'rgba(239, 68, 68, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_waf',
          title: isRu ? '🛡️ Развернуть Cloudflare WAF' : '🛡️ Deploy Cloudflare WAF',
          description: isRu 
            ? `Отразить атаку и получить компенсацию от страховки: +${Math.round(effectiveMps * 60)} ₽`
            : `Deflect attack and claim cyber insurance payout: +${Math.round(effectiveMps * 60)} ₽`,
          badge: isRu ? '+Рубли' : '+Revenue',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(500, Math.round(effectiveMps * 60))
        },
        {
          id: 'opt_counter_hack',
          title: isRu ? '⚡ Встречный кибер-удар' : '⚡ Counter Cyber Strike',
          description: isRu 
            ? `Захватить ботнет: +${Math.round(effectiveCps * 45)} строк чистого кода C#!`
            : `Hijack botnet nodes: +${Math.round(effectiveCps * 45)} clean C# lines!`,
          badge: isRu ? '+Код C#' : '+C# Code',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'grant_code',
          value: Math.max(300, Math.round(effectiveCps * 45))
        }
      ]
    },
    {
      id: `angel_${Date.now()}`,
      type: 'angel_investor',
      title: isRu ? '💼 ВИЗИТ ВЕНЧУРНОГО АНГЕЛА' : '💼 VENTURE ANGEL ARRIVAL',
      subtitle: isRu ? 'Инвестор из Кремниевой Долины заглянул на ваш кофе-брейк' : 'Silicon Valley angel investor stopped by your coffee station',
      description: isRu 
        ? 'Он впечатлен скоростью коммитов вашей команды и предлагает сделку прямо сейчас.' 
        : 'They are blown away by your team commit velocity and are offering a term sheet right now.',
      icon: '💎',
      gradient: 'from-amber-950/80 via-slate-900 to-amber-950/40',
      borderColor: 'border-amber-500/60',
      glowColor: 'rgba(245, 158, 11, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_seed_check',
          title: isRu ? '💰 Взять Seed-инвестиции' : '💰 Take Seed Check',
          description: isRu 
            ? `Мгновенный чек в капитал студии: +${Math.round(effectiveMps * 90)} ₽`
            : `Instant wire transfer to studio capital: +${Math.round(effectiveMps * 90)} ₽`,
          badge: isRu ? 'Крупный чек ₽' : 'Fat Check ₽',
          badgeColor: 'bg-amber-500/20 text-amber-300 border-amber-500/40',
          actionType: 'grant_money',
          value: Math.max(1000, Math.round(effectiveMps * 90))
        },
        {
          id: 'opt_bootstrap_pride',
          title: isRu ? '🔥 Отказаться и работать в потоке' : '🔥 Decline & Stay in Flow',
          description: isRu 
            ? 'Мгновенный вход в режим гиперфокуса (Flow State x2.5) на 30 секунд!'
            : 'Instant entry into hyperfocus (Flow State x2.5) for 30 seconds!',
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
      title: isRu ? '🐞 0-DAY КРИТИЧЕСКИЙ ЭКСПЛОЙТ' : '🐞 0-DAY CRITICAL EXPLOIT',
      subtitle: isRu ? 'Стажер нашел уязвимость переполнения буфера в ядре' : 'Intern discovered a buffer overflow in the core engine',
      description: isRu 
        ? 'Дыра в безопасности позволяет либо быстро пропатчить систему, либо сдать ее в Bug Bounty.' 
        : 'Vulnerability allows either an immediate production patch or a lucrative Bug Bounty payout.',
      icon: '🔬',
      gradient: 'from-purple-950/80 via-slate-900 to-purple-950/40',
      borderColor: 'border-purple-500/60',
      glowColor: 'rgba(168, 85, 247, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_patch',
          title: isRu ? '🛠️ Экстренный хотфикс в Production' : '🛠️ Hotfix to Production',
          description: isRu 
            ? `Код оптимизирован: +${Math.round(effectiveCps * 60)} строк C# мгновенно!`
            : `Engine optimized: +${Math.round(effectiveCps * 60)} lines of C# instantly!`,
          badge: isRu ? 'Хотфикс C#' : 'C# Hotfix',
          badgeColor: 'bg-purple-500/20 text-purple-300 border-purple-500/40',
          actionType: 'grant_code',
          value: Math.max(500, Math.round(effectiveCps * 60))
        },
        {
          id: 'opt_bounty_reward',
          title: isRu ? '🏆 Сдать отчет в Bug Bounty' : '🏆 Submit Bug Bounty Report',
          description: isRu 
            ? `Премия от корпорации за ответственное раскрытие: +${Math.round(effectiveMps * 75)} ₽`
            : `Corporate bounty award for responsible disclosure: +${Math.round(effectiveMps * 75)} ₽`,
          badge: isRu ? '+Баунти ₽' : '+Bounty ₽',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(800, Math.round(effectiveMps * 75))
        }
      ]
    },
    {
      id: `coffee_${Date.now()}`,
      type: 'coffee_rush',
      title: isRu ? '☕ СПЕЦИАЛЬНЫЙ ТРОЙНОЙ COLD BREW' : '☕ TRIPLE NITRO COLD BREW',
      subtitle: isRu ? 'Курьер доставил фирменный крафтовый кофе для разработчиков' : 'Courier delivered specialty craft coffee for the engineering floor',
      description: isRu 
        ? 'Аромат свежей арабики и двойная доза кофеина заряжают весь офис невероятной энергией!' 
        : 'Fresh arabica aroma and potent caffeine supercharge the entire studio with raw focus!',
      icon: '⚡',
      gradient: 'from-cyan-950/80 via-slate-900 to-cyan-950/40',
      borderColor: 'border-cyan-500/60',
      glowColor: 'rgba(6, 182, 212, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_drink_solo',
          title: isRu ? '🚀 Выпить залпом в одиночку' : '🚀 Chug it solo',
          description: isRu 
            ? '100% шкалы комбо и моментальный Flow State на 35 секунд!'
            : 'Instant 100% combo gauge and full Flow State for 35 seconds!',
          badge: isRu ? 'Супер-Фокус' : 'Hyper-Focus',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'boost_flow',
          value: 35
        },
        {
          id: 'opt_share_team',
          title: isRu ? '👥 Разделить кофе с командой' : '👥 Share with the squad',
          description: isRu 
            ? `Все серверы пишут код быстрее: бонус +${Math.round(effectiveCps * 50)} строк C#!`
            : `All squad servers compile faster: bonus +${Math.round(effectiveCps * 50)} C# lines!`,
          badge: isRu ? 'Буст команды' : 'Squad Boost',
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
