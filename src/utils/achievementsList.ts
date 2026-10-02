import { AchievementDef } from '../types/achievements';

export const ACHIEVEMENTS: AchievementDef[] = [
  // --- 1. КЛИКИ И ФОКУС ---
  {
    id: "ach_manual_clicks",
    icon: "⌨️",
    category: "clicks",
    titleRu: "Ручной труд",
    titleEn: "Manual Craft",
    descRu: "Совершите клики по клавиатуре для компиляции кода",
    descEn: "Perform manual keyboard taps to compile code",
    statKey: "manualClicks",
    tiers: [
      { tier: 1, target: 50, rewardDescRu: "+2% к общему множителю", rewardDescEn: "+2% Global Multiplier", bonusMultiplier: 0.02 },
      { tier: 2, target: 500, rewardDescRu: "+3% к общему множителю", rewardDescEn: "+3% Global Multiplier", bonusMultiplier: 0.03 },
      { tier: 3, target: 2500, rewardDescRu: "+5% к общему множителю (МАСТЕР)", rewardDescEn: "+5% Global Multiplier (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_crits",
    icon: "🔥",
    category: "clicks",
    titleRu: "Оверклокер",
    titleEn: "Overclocker",
    descRu: "Выбейте критические клики с 4-кратным кодом",
    descEn: "Land critical clicks with 4x code burst",
    statKey: "critClicks",
    tiers: [
      { tier: 1, target: 10, rewardDescRu: "+2% к силе клика", rewardDescEn: "+2% Click Power", bonusMultiplier: 0.02 },
      { tier: 2, target: 100, rewardDescRu: "+3% к силе клика", rewardDescEn: "+3% Click Power", bonusMultiplier: 0.03 },
      { tier: 3, target: 500, rewardDescRu: "+5% к силе клика (МАСТЕР)", rewardDescEn: "+5% Click Power (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_flow_enter",
    icon: "⚡",
    category: "clicks",
    titleRu: "В Потоке",
    titleEn: "Flow State",
    descRu: "Активируйте режим «В Потоке» (Flow Mode x3.0)",
    descEn: "Trigger the intense x3.0 Flow Mode combo",
    statKey: "flowEnters",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к множителю", rewardDescEn: "+2% Multiplier", bonusMultiplier: 0.02 },
      { tier: 2, target: 10, rewardDescRu: "+3% к множителю", rewardDescEn: "+3% Multiplier", bonusMultiplier: 0.03 },
      { tier: 3, target: 35, rewardDescRu: "+5% к множителю (МАСТЕР)", rewardDescEn: "+5% Multiplier (MASTER)", bonusMultiplier: 0.05 }
    ]
  },

  // --- 2. КОД И ПРОИЗВОДИТЕЛЬНОСТЬ ---
  {
    id: "ach_code_total",
    icon: "💻",
    category: "code",
    titleRu: "Фабрика Кода",
    titleEn: "Code Factory",
    descRu: "Скомпилируйте строки кода на C# за всё время",
    descEn: "Compile all-time total C# code lines",
    statKey: "totalCodeEver",
    tiers: [
      { tier: 1, target: 1000, rewardDescRu: "+2% к пассивному доходу", rewardDescEn: "+2% Passive Income", bonusMultiplier: 0.02 },
      { tier: 2, target: 100000, rewardDescRu: "+3% к пассивному доходу", rewardDescEn: "+3% Passive Income", bonusMultiplier: 0.03 },
      { tier: 3, target: 5000000, rewardDescRu: "+5% к пассивному доходу (МАСТЕР)", rewardDescEn: "+5% Passive Income (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_code_ps",
    icon: "🚀",
    category: "code",
    titleRu: "Вычислительный Узел",
    titleEn: "Computing Node",
    descRu: "Достигните высокой скорости выработки C#/сек",
    descEn: "Reach high C# production speed per second",
    statKey: "codePerSec",
    tiers: [
      { tier: 1, target: 50, rewardDescRu: "+2% к скорости кода", rewardDescEn: "+2% Code Speed", bonusMultiplier: 0.02 },
      { tier: 2, target: 1000, rewardDescRu: "+3% к скорости кода", rewardDescEn: "+3% Code Speed", bonusMultiplier: 0.03 },
      { tier: 3, target: 20000, rewardDescRu: "+5% к скорости кода (МАСТЕР)", rewardDescEn: "+5% Code Speed (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_lang_python",
    icon: "🐍",
    category: "code",
    titleRu: "Python Пионер",
    titleEn: "Python Pioneer",
    descRu: "Сделайте коммиты в модуль нейросети на Python",
    descEn: "Make commits to the Python neural network module",
    statKey: "commits_python",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+3% к силе клика (ПЕРВЫЙ КОММИТ)", rewardDescEn: "+3% Click Power (FIRST COMMIT)", bonusMultiplier: 0.03 },
      { tier: 2, target: 10, rewardDescRu: "+5% к силе клика", rewardDescEn: "+5% Click Power", bonusMultiplier: 0.05 },
      { tier: 3, target: 50, rewardDescRu: "+8% к силе клика (СЕНЬОР PYTHON)", rewardDescEn: "+8% Click Power (SENIOR PYTHON)", bonusMultiplier: 0.08 }
    ]
  },
  {
    id: "ach_lang_cpp",
    icon: "⚡",
    category: "code",
    titleRu: "Магия Указателей",
    titleEn: "Pointer Magic (C++)",
    descRu: "Сделайте коммиты в Vulkan-движок на C++23",
    descEn: "Make commits to the Vulkan engine on C++23",
    statKey: "commits_cpp",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+3% к выработке кода (ПЕРВЫЙ КОММИТ)", rewardDescEn: "+3% Code Output (FIRST COMMIT)", bonusMultiplier: 0.03 },
      { tier: 2, target: 10, rewardDescRu: "+5% к выработке кода", rewardDescEn: "+5% Code Output", bonusMultiplier: 0.05 },
      { tier: 3, target: 50, rewardDescRu: "+8% к выработке кода (АРХИТЕКТОР C++)", rewardDescEn: "+8% Code Output (C++ ARCHITECT)", bonusMultiplier: 0.08 }
    ]
  },
  {
    id: "ach_lang_solidity",
    icon: "💎",
    category: "code",
    titleRu: "Смарт-Контракт",
    titleEn: "Smart Contract (Solidity)",
    descRu: "Сделайте коммиты смарт-контракта на Solidity",
    descEn: "Make commits to the Solidity smart contract",
    statKey: "commits_solidity",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+3% к доходу в рублях (ПЕРВЫЙ КОММИТ)", rewardDescEn: "+3% Money Income (FIRST COMMIT)", bonusMultiplier: 0.03 },
      { tier: 2, target: 10, rewardDescRu: "+5% к доходу в рублях", rewardDescEn: "+5% Money Income", bonusMultiplier: 0.05 },
      { tier: 3, target: 50, rewardDescRu: "+8% к доходу в рублях (WEB3 КИТ)", rewardDescEn: "+8% Money Income (WEB3 WHALE)", bonusMultiplier: 0.08 }
    ]
  },
  {
    id: "ach_lang_rust",
    icon: "🦀",
    category: "code",
    titleRu: "Borrow Checker Укрощен",
    titleEn: "Borrow Checker Tamed (Rust)",
    descRu: "Сделайте коммиты квантового ядра на Rust",
    descEn: "Make commits to the Rust quantum core",
    statKey: "commits_rust",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+4% к общему множителю (ПЕРВЫЙ КОММИТ)", rewardDescEn: "+4% Global Multiplier (FIRST COMMIT)", bonusMultiplier: 0.04 },
      { tier: 2, target: 10, rewardDescRu: "+6% к общему множителю", rewardDescEn: "+6% Global Multiplier", bonusMultiplier: 0.06 },
      { tier: 3, target: 50, rewardDescRu: "+10% к общему множителю (RUSTACEAN GOD)", rewardDescEn: "+10% Global Multiplier (RUSTACEAN GOD)", bonusMultiplier: 0.10 }
    ]
  },

  // --- 3. ЭКОНОМИКА И ДОХОД ---
  {
    id: "ach_money_earned",
    icon: "💰",
    category: "economy",
    titleRu: "Первый Капитал",
    titleEn: "Initial Capital",
    descRu: "Заработайте рубли на разработке софта",
    descEn: "Earn revenue by shipping commercial software",
    statKey: "money",
    tiers: [
      { tier: 1, target: 1000, rewardDescRu: "+2% к доходу в рублях", rewardDescEn: "+2% Money Income", bonusMultiplier: 0.02 },
      { tier: 2, target: 100000, rewardDescRu: "+3% к доходу в рублях", rewardDescEn: "+3% Money Income", bonusMultiplier: 0.03 },
      { tier: 3, target: 10000000, rewardDescRu: "+5% к доходу в рублях (МАСТЕР)", rewardDescEn: "+5% Money Income (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_money_ps",
    icon: "📈",
    category: "economy",
    titleRu: "Денежный Поток",
    titleEn: "Cash Flow",
    descRu: "Разгоните пассивный денежный поток ₽/сек",
    descEn: "Boost passive money stream per second",
    statKey: "moneyPerSec",
    tiers: [
      { tier: 1, target: 25, rewardDescRu: "+2% к доходам", rewardDescEn: "+2% Income", bonusMultiplier: 0.02 },
      { tier: 2, target: 500, rewardDescRu: "+3% к доходам", rewardDescEn: "+3% Income", bonusMultiplier: 0.03 },
      { tier: 3, target: 10000, rewardDescRu: "+5% к доходам (МАСТЕР)", rewardDescEn: "+5% Income (MASTER)", bonusMultiplier: 0.05 }
    ]
  },

  // --- 4. ОБОРУДОВАНИЕ СТУДИИ ---
  {
    id: "ach_espresso",
    icon: "☕",
    category: "upgrades",
    titleRu: "Кофеиновый Маньяк",
    titleEn: "Caffeine Fiend",
    descRu: "Прокачайте уровень Кенийского Эспрессо",
    descEn: "Upgrade Kenyan Espresso level",
    statKey: "upgrade_1",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% к силе клика", rewardDescEn: "+2% Click Power", bonusMultiplier: 0.02 },
      { tier: 2, target: 20, rewardDescRu: "+3% к силе клика", rewardDescEn: "+3% Click Power", bonusMultiplier: 0.03 },
      { tier: 3, target: 50, rewardDescRu: "+5% к силе клика (МАКС)", rewardDescEn: "+5% Click Power (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_custom_mech",
    icon: "⌨️",
    category: "upgrades",
    titleRu: "Кастомная Механика",
    titleEn: "Custom Keeb",
    descRu: "Прокачайте клавиатуру Custom 75%",
    descEn: "Upgrade Custom 75% Keyboard level",
    statKey: "upgrade_2",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% к кликам", rewardDescEn: "+2% Clicks", bonusMultiplier: 0.02 },
      { tier: 2, target: 20, rewardDescRu: "+3% к кликам", rewardDescEn: "+3% Clicks", bonusMultiplier: 0.03 },
      { tier: 3, target: 40, rewardDescRu: "+5% к кликам (МАКС)", rewardDescEn: "+5% Clicks (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_cicd_robot",
    icon: "🤖",
    category: "upgrades",
    titleRu: "Авто-CI/CD",
    titleEn: "Automated CI/CD",
    descRu: "Прокачайте Авто-CI/CD Робота студии",
    descEn: "Upgrade Studio CI/CD Robot level",
    statKey: "upgrade_3",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% к автодоходу", rewardDescEn: "+2% Idle Income", bonusMultiplier: 0.02 },
      { tier: 2, target: 20, rewardDescRu: "+3% к автодоходу", rewardDescEn: "+3% Idle Income", bonusMultiplier: 0.03 },
      { tier: 3, target: 40, rewardDescRu: "+5% к автодоходу (МАКС)", rewardDescEn: "+5% Idle Income (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_ai_copilot",
    icon: "🧠",
    category: "upgrades",
    titleRu: "Сингулярность Кода",
    titleEn: "Code Singularity",
    descRu: "Прокачайте нейросеть AI Copilot",
    descEn: "Upgrade AI Copilot assistant level",
    statKey: "upgrade_4",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% ко всем показателям", rewardDescEn: "+2% All Stats", bonusMultiplier: 0.02 },
      { tier: 2, target: 15, rewardDescRu: "+3% ко всем показателям", rewardDescEn: "+3% All Stats", bonusMultiplier: 0.03 },
      { tier: 3, target: 35, rewardDescRu: "+5% ко всем показателям (МАКС)", rewardDescEn: "+5% All Stats (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_ubuntu_rack",
    icon: "🖧",
    category: "upgrades",
    titleRu: "Серверный Кластер",
    titleEn: "Server Cluster",
    descRu: "Прокачайте Стойку Ubuntu серверов",
    descEn: "Upgrade Ubuntu Server Rack level",
    statKey: "upgrade_5",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% к C#/сек", rewardDescEn: "+2% C#/sec", bonusMultiplier: 0.02 },
      { tier: 2, target: 15, rewardDescRu: "+3% к C#/сек", rewardDescEn: "+3% C#/sec", bonusMultiplier: 0.03 },
      { tier: 3, target: 30, rewardDescRu: "+5% к C#/сек (МАКС)", rewardDescEn: "+5% C#/sec (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_quantum_dc",
    icon: "⚛️",
    category: "upgrades",
    titleRu: "Квантовый Прорыв",
    titleEn: "Quantum Leap",
    descRu: "Прокачайте Квантовый Дата-Центр",
    descEn: "Upgrade Quantum Data Center level",
    statKey: "upgrade_6",
    tiers: [
      { tier: 1, target: 3, rewardDescRu: "+3% к общему множителю", rewardDescEn: "+3% Multiplier", bonusMultiplier: 0.03 },
      { tier: 2, target: 12, rewardDescRu: "+4% к общему множителю", rewardDescEn: "+4% Multiplier", bonusMultiplier: 0.04 },
      { tier: 3, target: 25, rewardDescRu: "+6% к общему множителю (МАКС)", rewardDescEn: "+6% Multiplier (MAX)", bonusMultiplier: 0.06 }
    ]
  },
  {
    id: "ach_total_upgrades",
    icon: "🛠️",
    category: "upgrades",
    titleRu: "Сетап Мечты",
    titleEn: "Dream Setup",
    descRu: "Суммарное количество уровней всего оборудования",
    descEn: "Total cumulative levels across all gear",
    statKey: "totalUpgradeLevels",
    tiers: [
      { tier: 1, target: 25, rewardDescRu: "+2% к множителю", rewardDescEn: "+2% Multiplier", bonusMultiplier: 0.02 },
      { tier: 2, target: 100, rewardDescRu: "+4% к множителю", rewardDescEn: "+4% Multiplier", bonusMultiplier: 0.04 },
      { tier: 3, target: 220, rewardDescRu: "+6% к множителю (МАСТЕР)", rewardDescEn: "+6% Multiplier (MASTER)", bonusMultiplier: 0.06 }
    ]
  },

  // --- 5. СИСТЕМЫ И ИНФРАСТРУКТУРА СТУДИИ ---
  {
    id: "ach_systems_count",
    icon: "🏢",
    category: "systems",
    titleRu: "Архитектура Студии",
    titleEn: "Studio Architecture",
    descRu: "Разблокируйте передовые системы в Hub Студии",
    descEn: "Unlock advanced studio management systems",
    statKey: "unlockedSystemsCount",
    tiers: [
      { tier: 1, target: 2, rewardDescRu: "+2% к выработке", rewardDescEn: "+2% Production", bonusMultiplier: 0.02 },
      { tier: 2, target: 5, rewardDescRu: "+3% к выработке", rewardDescEn: "+3% Production", bonusMultiplier: 0.03 },
      { tier: 3, target: 10, rewardDescRu: "+5% к выработке (МАКС)", rewardDescEn: "+5% Production (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_cats",
    icon: "🐱",
    category: "systems",
    titleRu: "Кошачий Синдикат",
    titleEn: "Cat Syndicate",
    descRu: "Улучшите Офисный Котоприют",
    descEn: "Level up Studio Cat Haven",
    statKey: "sys_cathaven",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к уюту и доходу", rewardDescEn: "+2% Comfort & Income", bonusMultiplier: 0.02 },
      { tier: 2, target: 5, rewardDescRu: "+3% к уюту и доходу", rewardDescEn: "+3% Comfort & Income", bonusMultiplier: 0.03 },
      { tier: 3, target: 10, rewardDescRu: "+5% к уюту и доходу (МАКС)", rewardDescEn: "+5% Comfort & Income (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_esports",
    icon: "🏆",
    category: "systems",
    titleRu: "Киберспорт",
    titleEn: "Esports Legend",
    descRu: "Улучшите Киберспортивную Арену студии",
    descEn: "Level up Studio Esports Arena",
    statKey: "sys_esports",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к силе клика", rewardDescEn: "+2% Click Power", bonusMultiplier: 0.02 },
      { tier: 2, target: 3, rewardDescRu: "+3% к силе клика", rewardDescEn: "+3% Click Power", bonusMultiplier: 0.03 },
      { tier: 3, target: 5, rewardDescRu: "+5% к силе клика (МАКС)", rewardDescEn: "+5% Click Power (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_realestate",
    icon: "🏙️",
    category: "systems",
    titleRu: "Силиконовая Долина",
    titleEn: "Silicon Valley",
    descRu: "Приобретите офисную недвижимость",
    descEn: "Acquire prime commercial real estate",
    statKey: "sys_realestate",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+3% к капиталу", rewardDescEn: "+3% Capital", bonusMultiplier: 0.03 },
      { tier: 2, target: 2, rewardDescRu: "+4% к капиталу", rewardDescEn: "+4% Capital", bonusMultiplier: 0.04 },
      { tier: 3, target: 4, rewardDescRu: "+6% к капиталу (МАКС)", rewardDescEn: "+6% Capital (MAX)", bonusMultiplier: 0.06 }
    ]
  },
  {
    id: "ach_asset_store",
    icon: "📦",
    category: "systems",
    titleRu: "Пассивные Роялти",
    titleEn: "Passive Royalties",
    descRu: "Публикуйте ассеты в Asset Store",
    descEn: "Publish reusable studio assets in Store",
    statKey: "sys_assetstore",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к роялти", rewardDescEn: "+2% Royalties", bonusMultiplier: 0.02 },
      { tier: 2, target: 4, rewardDescRu: "+3% к роялти", rewardDescEn: "+3% Royalties", bonusMultiplier: 0.03 },
      { tier: 3, target: 8, rewardDescRu: "+5% к роялти (МАКС)", rewardDescEn: "+5% Royalties (MAX)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_merch",
    icon: "👕",
    category: "systems",
    titleRu: "Культ Бренда",
    titleEn: "Brand Cult",
    descRu: "Развивайте фирменный Мерч-шоп студии",
    descEn: "Scale studio branded merch merchandise store",
    statKey: "sys_merch",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к продажам", rewardDescEn: "+2% Sales", bonusMultiplier: 0.02 },
      { tier: 2, target: 3, rewardDescRu: "+3% к продажам", rewardDescEn: "+3% Sales", bonusMultiplier: 0.03 },
      { tier: 3, target: 6, rewardDescRu: "+5% к продажам (МАКС)", rewardDescEn: "+5% Sales (MAX)", bonusMultiplier: 0.05 }
    ]
  },

  // --- 6. ПРЕСТИЖ, ВАЛЮТА И ОСОБЫЕ ---
  {
    id: "ach_ipo_count",
    icon: "🚀",
    category: "prestige",
    titleRu: "Волк с Уолл-стрит",
    titleEn: "Wall Street Wolf",
    descRu: "Проведите выходы на биржу (IPO Престиж)",
    descEn: "Execute successful company IPO prestige resets",
    statKey: "prestigeCount",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+3% к постоянному множителю", rewardDescEn: "+3% Permanent Multiplier", bonusMultiplier: 0.03 },
      { tier: 2, target: 3, rewardDescRu: "+5% к постоянному множителю", rewardDescEn: "+5% Permanent Multiplier", bonusMultiplier: 0.05 },
      { tier: 3, target: 10, rewardDescRu: "+8% к постоянному множителю (МАСТЕР)", rewardDescEn: "+8% Permanent Multiplier (MASTER)", bonusMultiplier: 0.08 }
    ]
  },
  {
    id: "ach_ipo_tokens",
    icon: "🪙",
    category: "prestige",
    titleRu: "Портфель Инвестора",
    titleEn: "Investor Portfolio",
    descRu: "Накопите суммарные Токены Акций IPO",
    descEn: "Accumulate total IPO stock investment tokens",
    statKey: "prestigeTokens",
    tiers: [
      { tier: 1, target: 5, rewardDescRu: "+2% к силе токенов", rewardDescEn: "+2% Token Power", bonusMultiplier: 0.02 },
      { tier: 2, target: 50, rewardDescRu: "+4% к силе токенов", rewardDescEn: "+4% Token Power", bonusMultiplier: 0.04 },
      { tier: 3, target: 250, rewardDescRu: "+6% к силе токенов (МАСТЕР)", rewardDescEn: "+6% Token Power (MASTER)", bonusMultiplier: 0.06 }
    ]
  },
  {
    id: "ach_time_warps",
    icon: "⏳",
    category: "special",
    titleRu: "Варп Времени",
    titleEn: "Time Warp",
    descRu: "Используйте ускорение смены Time Warp (2 часа)",
    descEn: "Trigger instant 2-hour Time Warp shift",
    statKey: "timeWarpsUsed",
    tiers: [
      { tier: 1, target: 1, rewardDescRu: "+2% к выработке", rewardDescEn: "+2% Production", bonusMultiplier: 0.02 },
      { tier: 2, target: 5, rewardDescRu: "+3% к выработке", rewardDescEn: "+3% Production", bonusMultiplier: 0.03 },
      { tier: 3, target: 15, rewardDescRu: "+5% к выработке (МАСТЕР)", rewardDescEn: "+5% Production (MASTER)", bonusMultiplier: 0.05 }
    ]
  },
  {
    id: "ach_switches",
    icon: "🎧",
    category: "special",
    titleRu: "Сомелье Свитчей",
    titleEn: "Switch Sommelier",
    descRu: "Опробуйте различные механические свитчи",
    descEn: "Try out different mechanical keyboard switches",
    statKey: "switchesTestedCount",
    tiers: [
      { tier: 1, target: 2, rewardDescRu: "+2% к ритму", rewardDescEn: "+2% Rhythm", bonusMultiplier: 0.02 },
      { tier: 2, target: 3, rewardDescRu: "+3% к ритму", rewardDescEn: "+3% Rhythm", bonusMultiplier: 0.03 },
      { tier: 3, target: 4, rewardDescRu: "+5% к ритму (ВСЕ 4 СВИТЧА)", rewardDescEn: "+5% Rhythm (ALL 4 SWITCHES)", bonusMultiplier: 0.05 }
    ]
  }
];
