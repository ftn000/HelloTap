import { GameRandomEvent } from '../types/events';
import { Language } from './i18n';

export const generateRandomEvent = (codePerSec: number, moneyPerSec: number, lang: Language = 'ru'): GameRandomEvent => {
  const effectiveCps = Math.max(10, codePerSec);
  const effectiveMps = Math.max(5, moneyPerSec);
  const isRu = lang === 'ru';
  const isTr = lang === 'tr';

  const eventPool: GameRandomEvent[] = [
    {
      id: `hacker_${Date.now()}`,
      type: 'hacker_attack',
      title: isRu ? '🚨 ДДОС-АТАКА НА ПРОДАКШН!' : isTr ? '🚨 CANLI SUNUCUYA DDOS SALDIRISI!' : '🚨 PRODUCTION DDOS ATTACK!',
      subtitle: isRu 
        ? 'Система безопасности зафиксировала терабит SYN-флуда' 
        : isTr 
        ? 'Güvenlik sistemleri 1 Tbps SYN seli kaydetti' 
        : 'Security systems logged 1 Tbps of SYN flood traffic',
      description: isRu 
        ? 'Боты атакуют ваши шлюзы API. Серверы начинают греться! Что предпринимаем?' 
        : isTr 
        ? 'Botnet ağları API ağ geçitlerine saldırıyor. Sunucular ısınıyor! Ne yapıyoruz?' 
        : 'Botnets are flooding API gateways. Servers are overheating! What should we do?',
      icon: '👾',
      gradient: 'from-red-950/80 via-slate-900 to-red-950/40',
      borderColor: 'border-red-500/60',
      glowColor: 'rgba(239, 68, 68, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_waf',
          title: isRu ? '🛡️ Развернуть Cloudflare WAF' : isTr ? '🛡️ Cloudflare WAF Dağıt' : '🛡️ Deploy Cloudflare WAF',
          description: isRu 
            ? `Отразить атаку и получить компенсацию от страховки: +${Math.round(effectiveMps * 60)} ₽`
            : isTr 
            ? `Saldırıyı püskürt ve siber sigorta tazminatı al: +${Math.round(effectiveMps * 60)} ₽`
            : `Deflect attack and claim cyber insurance payout: +${Math.round(effectiveMps * 60)} ₽`,
          badge: isRu ? '+Рубли' : isTr ? '+Gelir' : '+Revenue',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(500, Math.round(effectiveMps * 60))
        },
        {
          id: 'opt_counter_hack',
          title: isRu ? '⚡ Встречный кибер-удар' : isTr ? '⚡ Karşı Siber Taarruz' : '⚡ Counter Cyber Strike',
          description: isRu 
            ? `Захватить ботнет: +${Math.round(effectiveCps * 45)} строк чистого кода C#!`
            : isTr 
            ? `Botnet düğümlerini ele geçir: +${Math.round(effectiveCps * 45)} temiz C# satırı!`
            : `Hijack botnet nodes: +${Math.round(effectiveCps * 45)} clean C# lines!`,
          badge: isRu ? '+Код C#' : isTr ? '+C# Kodu' : '+C# Code',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'grant_code',
          value: Math.max(300, Math.round(effectiveCps * 45))
        }
      ]
    },
    {
      id: `angel_${Date.now()}`,
      type: 'angel_investor',
      title: isRu ? '💼 ВИЗИТ ВЕНЧУРНОГО АНГЕЛА' : isTr ? '💼 MELEK YATIRIMCI ZİYARETİ' : '💼 VENTURE ANGEL ARRIVAL',
      subtitle: isRu 
        ? 'Инвестор из Кремниевой Долины заглянул на ваш кофе-брейк' 
        : isTr 
        ? 'Silikon Vadisi melek yatırımcısı kahve molanıza uğradı' 
        : 'Silicon Valley angel investor stopped by your coffee station',
      description: isRu 
        ? 'Он впечатлен скоростью коммитов вашей команды и предлагает сделку прямо сейчас.' 
        : isTr 
        ? 'Ekibinizin commit hızından çok etkilendi ve hemen bir yatırım teklif ediyor.' 
        : 'They are blown away by your team commit velocity and are offering a term sheet right now.',
      icon: '💎',
      gradient: 'from-amber-950/80 via-slate-900 to-amber-950/40',
      borderColor: 'border-amber-500/60',
      glowColor: 'rgba(245, 158, 11, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_seed_check',
          title: isRu ? '💰 Взять Seed-инвестиции' : isTr ? '💰 Tohum Yatırımını Al' : '💰 Take Seed Check',
          description: isRu 
            ? `Мгновенный чек в капитал студии: +${Math.round(effectiveMps * 90)} ₽`
            : isTr 
            ? `Stüdyo sermayesine anında nakit: +${Math.round(effectiveMps * 90)} ₽`
            : `Instant wire transfer to studio capital: +${Math.round(effectiveMps * 90)} ₽`,
          badge: isRu ? 'Крупный чек ₽' : isTr ? 'Büyük Yatırım ₽' : 'Fat Check ₽',
          badgeColor: 'bg-amber-500/20 text-amber-300 border-amber-500/40',
          actionType: 'grant_money',
          value: Math.max(1000, Math.round(effectiveMps * 90))
        },
        {
          id: 'opt_bootstrap_pride',
          title: isRu ? '🔥 Отказаться и работать в потоке' : isTr ? '🔥 Reddet ve Flow Modunda Kal' : '🔥 Decline & Stay in Flow',
          description: isRu 
            ? 'Мгновенный вход в режим гиперфокуса (Flow State x2.5) на 30 секунд!'
            : isTr 
            ? '30 saniye boyunca anında hiper odaklanma (Flow State x2.5) moduna gir!'
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
      title: isRu ? '🐞 0-DAY КРИТИЧЕСКИЙ ЭКСПЛОЙТ' : isTr ? '🐞 0-DAY KRİTİK AÇIK' : '🐞 0-DAY CRITICAL EXPLOIT',
      subtitle: isRu 
        ? 'Стажер нашел уязвимость переполнения буфера в ядре' 
        : isTr 
        ? 'Stajyer çekirdekte bir arabellek taşması açığı buldu' 
        : 'Intern discovered a buffer overflow in the core engine',
      description: isRu 
        ? 'Дыра в безопасности позволяет либо быстро пропатчить систему, либо сдать ее в Bug Bounty.' 
        : isTr 
        ? 'Güvenlik açığı, sistemi hemen yamalamayı veya Bug Bounty programına bildirmeyi mümkün kılıyor.' 
        : 'Vulnerability allows either an immediate production patch or a lucrative Bug Bounty payout.',
      icon: '🔬',
      gradient: 'from-purple-950/80 via-slate-900 to-purple-950/40',
      borderColor: 'border-purple-500/60',
      glowColor: 'rgba(168, 85, 247, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_patch',
          title: isRu ? '🛠️ Экстренный хотфикс в Production' : isTr ? '🛠️ Canlıya Acil Hotfix Geç' : '🛠️ Hotfix to Production',
          description: isRu 
            ? `Код оптимизирован: +${Math.round(effectiveCps * 60)} строк C# мгновенно!`
            : isTr 
            ? `Motor optimize edildi: anında +${Math.round(effectiveCps * 60)} C# satırı!`
            : `Engine optimized: +${Math.round(effectiveCps * 60)} lines of C# instantly!`,
          badge: isRu ? 'Хотфикс C#' : isTr ? 'C# Hotfix' : 'C# Hotfix',
          badgeColor: 'bg-purple-500/20 text-purple-300 border-purple-500/40',
          actionType: 'grant_code',
          value: Math.max(500, Math.round(effectiveCps * 60))
        },
        {
          id: 'opt_bounty_reward',
          title: isRu ? '🏆 Сдать отчет в Bug Bounty' : isTr ? '🏆 Bug Bounty Raporu Gönder' : '🏆 Submit Bug Bounty Report',
          description: isRu 
            ? `Премия от корпорации за ответственное раскрытие: +${Math.round(effectiveMps * 75)} ₽`
            : isTr 
            ? `Sorumlu bildirim için şirketten ödül: +${Math.round(effectiveMps * 75)} ₽`
            : `Corporate bounty award for responsible disclosure: +${Math.round(effectiveMps * 75)} ₽`,
          badge: isRu ? '+Баунти ₽' : isTr ? '+Ödül ₽' : '+Bounty ₽',
          badgeColor: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40',
          actionType: 'grant_money',
          value: Math.max(800, Math.round(effectiveMps * 75))
        }
      ]
    },
    {
      id: `coffee_${Date.now()}`,
      type: 'coffee_rush',
      title: isRu ? '☕ СПЕЦИАЛЬНЫЙ ТРОЙНОЙ COLD BREW' : isTr ? '☕ ÖZEL ÜÇLÜ COLD BREW' : '☕ TRIPLE NITRO COLD BREW',
      subtitle: isRu 
        ? 'Курьер доставил фирменный крафтовый кофе для разработчиков' 
        : isTr 
        ? 'Kurye yazılımcılar için özel el yapımı kahve teslim etti' 
        : 'Courier delivered specialty craft coffee for the engineering floor',
      description: isRu 
        ? 'Аромат свежей арабики и двойная доза кофеина заряжают весь офис невероятной энергией!' 
        : isTr 
        ? 'Taze arabica aroması ve çifte kafein tüm ofisi olağanüstü bir odaklanma ile şarj ediyor!' 
        : 'Fresh arabica aroma and potent caffeine supercharge the entire studio with raw focus!',
      icon: '⚡',
      gradient: 'from-cyan-950/80 via-slate-900 to-cyan-950/40',
      borderColor: 'border-cyan-500/60',
      glowColor: 'rgba(6, 182, 212, 0.4)',
      timeoutSec: 15,
      options: [
        {
          id: 'opt_drink_solo',
          title: isRu ? '🚀 Выпить залпом в одиночку' : isTr ? '🚀 Tek Başına Dik' : '🚀 Chug it solo',
          description: isRu 
            ? '100% шкалы комбо и моментальный Flow State на 35 секунд!'
            : isTr 
            ? '%100 kombo çubuğu ve 35 saniye boyunca anında Flow State!'
            : 'Instant 100% combo gauge and full Flow State for 35 seconds!',
          badge: isRu ? 'Супер-Фокус' : isTr ? 'Süper Odak' : 'Hyper-Focus',
          badgeColor: 'bg-cyan-500/20 text-cyan-300 border-cyan-500/40',
          actionType: 'boost_flow',
          value: 35
        },
        {
          id: 'opt_share_team',
          title: isRu ? '👥 Разделить кофе с командой' : isTr ? '👥 Ekiple Paylaş' : '👥 Share with the squad',
          description: isRu 
            ? `Все серверы пишут код быстрее: бонус +${Math.round(effectiveCps * 50)} строк C#!`
            : isTr 
            ? `Tüm sunucular daha hızlı kod derler: +${Math.round(effectiveCps * 50)} C# satırı!`
            : `All squad servers compile faster: bonus +${Math.round(effectiveCps * 50)} C# lines!`,
          badge: isRu ? 'Буст команды' : isTr ? 'Ekip Güçlendirmesi' : 'Squad Boost',
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
