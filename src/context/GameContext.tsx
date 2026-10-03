import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import { ShopUpgrade, StudioSystem, GameSaveData, HubCategoryType } from '../types/game';
import { sounds, SoundProfile } from '../utils/soundEffects';
import { yandexSdk } from '../utils/yandexSdk';
import { yandexPayments } from '../utils/yandexPayments';
import { Language, TranslationDictionary, TRANSLATIONS, detectInitialLanguage } from '../utils/i18n';
import { ACHIEVEMENTS } from '../utils/achievementsList';
import { ToastItem } from '../components/AchievementToast';
import { musicSynth } from '../utils/musicSynth';
import { ThemeId } from '../types/themes';
import { DEFAULT_THEME_ID, IDE_THEMES } from '../utils/themesList';
import { GameRandomEvent, GameEventOption } from '../types/events';
import { generateRandomEvent } from '../utils/eventsList';
import { SKILL_NODES } from '../utils/skillsList';
import { generateBaselineContributions, getTodayKey, calculateStreak } from '../utils/githubHeatmap';

const INITIAL_UPGRADES: ShopUpgrade[] = [
  {
    id: 1,
    name: "Эспрессо из Кении",
    nameRu: "Эспрессо из Кении",
    nameEn: "Kenyan Espresso",
    nameTr: "Kenya Espressosu",
    category: "click",
    icon: "☕",
    description: "Двойной шот кофеина повышает скорость набора кода",
    descriptionRu: "Двойной шот кофеина повышает скорость набора кода",
    descriptionEn: "Double espresso shot boosts typing speed",
    descriptionTr: "Çift doz kafein kod yazma hızını artırır",
    level: 0,
    maxLevel: 50,
    baseCostCode: 15,
    baseCostMoney: 50,
    costMultiplier: 1.15,
    codePerClickBonus: 1,
    codePerSecBonus: 0,
    moneyPerSecBonus: 0,
    multiplierBonus: 0.02
  },
  {
    id: 2,
    name: "Механика Custom 75%",
    nameRu: "Механика Custom 75%",
    nameEn: "Custom 75% Mechanical",
    nameTr: "Özel %75 Mekanik Klavye",
    category: "click",
    icon: "⌨️",
    description: "Тактильные свитчи Lubed Tealios для сверхбыстрого ввода",
    descriptionRu: "Тактильные свитчи Lubed Tealios для сверхбыстрого ввода",
    descriptionEn: "Lubed Tealios tactile switches for ultra-fast typing",
    descriptionTr: "Ultra hızlı yazım için dokunsal Lubed Tealios anahtarları",
    level: 0,
    maxLevel: 40,
    baseCostCode: 80,
    baseCostMoney: 250,
    costMultiplier: 1.18,
    codePerClickBonus: 4,
    codePerSecBonus: 0,
    moneyPerSecBonus: 0,
    multiplierBonus: 0.05
  },
  {
    id: 3,
    name: "Авто-CI/CD Робот",
    nameRu: "Авто-CI/CD Робот",
    nameEn: "Auto CI/CD Bot",
    nameTr: "Otomatik CI/CD Robotu",
    category: "idle",
    icon: "🤖",
    description: "Автоматическая сборка релизов и непрерывный деплой",
    descriptionRu: "Автоматическая сборка релизов и непрерывный деплой",
    descriptionEn: "Automated release builds and continuous deployment",
    descriptionTr: "Otomatik sürüm derlemeleri ve sürekli dağıtım",
    level: 0,
    maxLevel: 40,
    baseCostCode: 350,
    baseCostMoney: 900,
    costMultiplier: 1.20,
    codePerClickBonus: 0,
    codePerSecBonus: 12,
    moneyPerSecBonus: 18,
    multiplierBonus: 0.04
  },
  {
    id: 4,
    name: "AI Copilot Ассистент",
    nameRu: "AI Copilot Ассистент",
    nameEn: "AI Copilot Assistant",
    nameTr: "AI Copilot Asistanı",
    category: "idle",
    icon: "🧠",
    description: "Нейросеть автодополняет функции и рефакторит спагетти",
    descriptionRu: "Нейросеть автодополняет функции и рефакторит спагетти",
    descriptionEn: "Neural assistant autocompletes functions and refactors spaghetti code",
    descriptionTr: "Yapay zeka fonksiyonları otomatik tamamlar ve spagetti kodu düzenler",
    level: 0,
    maxLevel: 35,
    baseCostCode: 1500,
    baseCostMoney: 3800,
    costMultiplier: 1.22,
    codePerClickBonus: 0,
    codePerSecBonus: 45,
    moneyPerSecBonus: 65,
    multiplierBonus: 0.08
  },
  {
    id: 5,
    name: "Серверная Стойка Ubuntu",
    nameRu: "Серверная Стойка Ubuntu",
    nameEn: "Ubuntu Server Rack",
    nameTr: "Ubuntu Sunucu Kabini",
    category: "idle",
    icon: "🖧",
    description: "Собственный микросерверный кластер студии",
    descriptionRu: "Собственный микросерверный кластер студии",
    descriptionEn: "Dedicated studio microserver cluster",
    descriptionTr: "Stüdyoya özel mikro sunucu kümesi",
    level: 0,
    maxLevel: 30,
    baseCostCode: 7500,
    baseCostMoney: 18000,
    costMultiplier: 1.25,
    codePerClickBonus: 0,
    codePerSecBonus: 180,
    moneyPerSecBonus: 280,
    multiplierBonus: 0.12
  },
  {
    id: 6,
    name: "Квантовый Дата-Центр",
    nameRu: "Квантовый Дата-Центр",
    nameEn: "Quantum Data Center",
    nameTr: "Kuantum Veri Merkezi",
    category: "synergy",
    icon: "⚛️",
    description: "Квантовая суперпозиция параллельных вычислений",
    descriptionRu: "Квантовая суперпозиция параллельных вычислений",
    descriptionEn: "Quantum superposition of parallel computations",
    descriptionTr: "Paralel hesaplamaların kuantum süperpozisyonu",
    level: 0,
    maxLevel: 25,
    baseCostCode: 38000,
    baseCostMoney: 95000,
    costMultiplier: 1.28,
    codePerClickBonus: 25,
    codePerSecBonus: 850,
    moneyPerSecBonus: 1250,
    multiplierBonus: 0.20
  },
  {
    id: 7,
    name: "Нейросеть AI Copilot Pro",
    nameRu: "Нейросеть AI Copilot Pro",
    nameEn: "AI Copilot Pro LLM",
    nameTr: "AI Copilot Pro LLM",
    category: "idle",
    icon: "🧠",
    description: "LLM-ассистент генерирует микросервисы и пишет тесты",
    descriptionRu: "LLM-ассистент генерирует микросервисы и пишет тесты",
    descriptionEn: "LLM assistant generates microservices and writes tests",
    descriptionTr: "LLM asistanı mikroservisler üretir ve testler yazar",
    level: 0,
    maxLevel: 25,
    baseCostCode: 150000,
    baseCostMoney: 380000,
    costMultiplier: 1.28,
    codePerClickBonus: 0,
    codePerSecBonus: 3200,
    moneyPerSecBonus: 4800,
    multiplierBonus: 0.25
  },
  {
    id: 8,
    name: "Тензорный GPU Кластер H100",
    nameRu: "Тензорный GPU Кластер H100",
    nameEn: "Tensor GPU Cluster H100",
    nameTr: "Tensor GPU Kümesi H100",
    category: "idle",
    icon: "⚡",
    description: "Стойка из 8x H100 с жидкостным охлаждением для обучения моделей",
    descriptionRu: "Стойка из 8x H100 с жидкостным охлаждением для обучения моделей",
    descriptionEn: "Liquid-cooled 8x H100 rack for massive AI model training",
    descriptionTr: "Büyük yapay zeka modelleri eğitmek için sıvı soğutmalı 8x H100 kabini",
    level: 0,
    maxLevel: 20,
    baseCostCode: 750000,
    baseCostMoney: 1900000,
    costMultiplier: 1.30,
    codePerClickBonus: 0,
    codePerSecBonus: 14500,
    moneyPerSecBonus: 22000,
    multiplierBonus: 0.35
  },
  {
    id: 9,
    name: "Автономный Дев-Рой Агентов",
    nameRu: "Автономный Дев-Рой Агентов",
    nameEn: "Autonomous Dev Agent Swarm",
    nameTr: "Otonom Geliştirici Ajan Sürüsü",
    category: "idle",
    icon: "🤖",
    description: "Рой AI-агентов закрывает тикеты на GitHub и рефакторит код 24/7",
    descriptionRu: "Рой AI-агентов закрывает тикеты на GitHub и рефакторит код 24/7",
    descriptionEn: "AI agent swarm resolves GitHub issues and refactors code 24/7",
    descriptionTr: "Yapay zeka ajan sürüsü GitHub sorunlarını çözer ve 7/24 kod düzenler",
    level: 0,
    maxLevel: 20,
    baseCostCode: 3800000,
    baseCostMoney: 9500000,
    costMultiplier: 1.32,
    codePerClickBonus: 0,
    codePerSecBonus: 68000,
    moneyPerSecBonus: 105000,
    multiplierBonus: 0.50
  },
  {
    id: 10,
    name: "Open Source Спонсорство",
    nameRu: "Open Source Спонсорство",
    nameEn: "Open Source Sponsorship",
    nameTr: "Açık Kaynak Sponsorluğu",
    category: "synergy",
    icon: "💎",
    description: "Гранты и донаты от IT-гигантов за открытые библиотеки студии",
    descriptionRu: "Гранты и донаты от IT-гигантов за открытые библиотеки студии",
    descriptionEn: "Grants and sponsorships from tech giants for open-source libraries",
    descriptionTr: "Stüdyonun açık kaynak kütüphaneleri için teknoloji devlerinden bağış ve hibeler",
    level: 0,
    maxLevel: 15,
    baseCostCode: 18000000,
    baseCostMoney: 45000000,
    costMultiplier: 1.35,
    codePerClickBonus: 120,
    codePerSecBonus: 280000,
    moneyPerSecBonus: 520000,
    multiplierBonus: 0.75
  },
  {
    id: 11,
    name: "Квантовый Процессор Qubit-128",
    nameRu: "Квантовый Процессор Qubit-128",
    nameEn: "Qubit-128 Quantum Processor",
    nameTr: "Qubit-128 Kuantum İşlemcisi",
    category: "synergy",
    icon: "🔮",
    description: "Квантовая суперпозиция компилирует миллиарды комбинаций кода мгновенно",
    descriptionRu: "Квантовая суперпозиция компилирует миллиарды комбинаций кода мгновенно",
    descriptionEn: "Quantum superposition compiles billions of code paths simultaneously",
    descriptionTr: "Kuantum süperpozisyonu milyarlarca kod kombinasyonunu anında derler",
    level: 0,
    maxLevel: 10,
    baseCostCode: 90000000,
    baseCostMoney: 230000000,
    costMultiplier: 1.40,
    codePerClickBonus: 600,
    codePerSecBonus: 1200000,
    moneyPerSecBonus: 2400000,
    multiplierBonus: 1.20
  },
  {
    id: 12,
    name: "Орбитальный Спутниковый Даталинк",
    nameRu: "Орбитальный Спутниковый Даталинк",
    nameEn: "Orbital Satellite Datalink",
    nameTr: "Yörünge Uydu Veri Bağlantısı",
    category: "synergy",
    icon: "🛰️",
    description: "Космический лазерный канал связи: глобальное покрытие планеты без задержек",
    descriptionRu: "Космический лазерный канал связи: глобальное покрытие планеты без задержек",
    descriptionEn: "Space laser uplink: zero-latency global planet coverage",
    descriptionTr: "Uzay lazer bağlantısı: gecikmesiz küresel gezegen kapsama alanı",
    level: 0,
    maxLevel: 10,
    baseCostCode: 500000000,
    baseCostMoney: 1200000000,
    costMultiplier: 1.45,
    codePerClickBonus: 3000,
    codePerSecBonus: 5500000,
    moneyPerSecBonus: 11000000,
    multiplierBonus: 2.00
  }
];

const INITIAL_SYSTEMS: StudioSystem[] = [
  // --- 1. ОФИС И КОМАНДА ---
  {
    id: "sys_dailydigest",
    title: "Утренний Дайджест и Сбор Доходов",
    titleRu: "Утренний Дайджест и Сбор Доходов",
    titleEn: "Morning Digest & Yield Claim",
    titleTr: "Sabah Özeti ve Gelir Toplama",
    icon: "📋",
    category: "office",
    description: "Сводный отчет за сессию и быстрый сбор наград студии в 1 клик",
    descriptionRu: "Сводный отчет за сессию и быстрый сбор наград студии в 1 клик",
    descriptionEn: "Summary session report and quick 1-click reward collection",
    descriptionTr: "Oturum özeti raporu ve tek tıkla stüdyo ödüllerini toplama",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Сбор всех дивидендов студии",
    bonusDescRu: "Сбор всех дивидендов студии",
    bonusDescEn: "Claim all studio dividends",
    bonusDescTr: "Tüm stüdyo temettülerini topla"
  },
  {
    id: "sys_saveexport",
    title: "Облако и Экспорт Сохранений",
    titleRu: "Облако и Экспорт Сохранений",
    titleEn: "Cloud & Save Export",
    titleTr: "Bulut ve Kayıt Dışa Aktarma",
    icon: "💾",
    category: "office",
    description: "Резервное копирование и перенос прогресса между устройствами",
    descriptionRu: "Резервное копирование и перенос прогресса между устройствами",
    descriptionEn: "Backup and cross-device progress transfer",
    descriptionTr: "Cihazlar arası yedekleme ve ilerleme aktarımı",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Поддержка Яндекс Облака",
    bonusDescRu: "Поддержка Яндекс Облака",
    bonusDescEn: "Yandex Cloud Backup",
    bonusDescTr: "Yandex Bulut Desteği"
  },
  {
    id: "sys_realestate",
    title: "Студийная Недвижимость",
    titleRu: "Студийная Недвижимость",
    titleEn: "Studio Real Estate",
    titleTr: "Stüdyo Gayrimenkulü",
    icon: "🏢",
    category: "office",
    description: "Переезд из гаража в open-space лофт и небоскреб Silicon Tower",
    descriptionRu: "Переезд из гаража в open-space лофт и небоскреб Silicon Tower",
    descriptionEn: "Move from garage to open-space loft and Silicon Tower skyscraper",
    descriptionTr: "Garajdan açık ofis loftuna ve Silicon Tower gökdelenine taşınma",
    level: 0,
    maxLevel: 5,
    reqCode: 6000,
    bonusDesc: "+40% к глобальному множителю за уровень",
    bonusDescRu: "+40% к глобальному множителю за уровень",
    bonusDescEn: "+40% Global Multiplier per level",
    bonusDescTr: "Seviye başına +%40 genel çarpan"
  },

  // --- 2. БИЗНЕС И РЫНОК ---
  {
    id: "sys_assetstore",
    title: "Маркетплейс Ассетов",
    titleRu: "Маркетплейс Ассетов",
    titleEn: "Asset Marketplace",
    titleTr: "Varlık Pazarı (Asset Store)",
    icon: "🏪",
    category: "business",
    description: "Публикация шейдеров, 3D-моделей и C#-скриптов на маркетплейс",
    descriptionRu: "Публикация шейдеров, 3D-моделей и C#-скриптов на маркетплейс",
    descriptionEn: "Publish shaders, 3D assets, and C# packages to marketplaces",
    descriptionTr: "Mağazada gölgelendiriciler, 3D modeller ve C# paketleri yayınlayın",
    level: 0,
    maxLevel: 10,
    reqCode: 3500,
    bonusDesc: "+150 ₽/сек пассивных роялти за уровень",
    bonusDescRu: "+150 ₽/сек пассивных роялти за уровень",
    bonusDescEn: "+150 ₽/sec passive royalties per level",
    bonusDescTr: "Seviye başına +150 ₽/sn pasif telif hakkı"
  },
  {
    id: "sys_venture",
    title: "Венчурные Инвестиции",
    titleRu: "Венчурные Инвестиции",
    titleEn: "Venture Investments",
    titleTr: "Girişim Yatırımları",
    icon: "💼",
    category: "business",
    description: "Питч-сессии перед венчурными фондами Кремниевой Долины",
    descriptionRu: "Питч-сессии перед венчурными фондами Кремниевой Долины",
    descriptionEn: "Pitch sessions with Silicon Valley venture capital firms",
    descriptionTr: "Silikon Vadisi girişim sermayesi fonlarına sunum seansları",
    level: 0,
    maxLevel: 5,
    reqCode: 15000,
    bonusDesc: "Гранты инвесторов и +25% к дивидендам",
    bonusDescRu: "Гранты инвесторов и +25% к дивидендам",
    bonusDescEn: "Investor grants and +25% dividends",
    bonusDescTr: "Yatırımcı hibeleri ve temettülere +%25"
  },
  {
    id: "sys_merch",
    title: "Студийный Мерч-Стор",
    titleRu: "Студийный Мерч-Стор",
    titleEn: "Studio Merch Store",
    titleTr: "Stüdyo Ürün Mağazası",
    icon: "👕",
    category: "business",
    description: "Худи, механические кейкапы и коллекционные фигурки маскотов",
    descriptionRu: "Худи, механические кейкапы и коллекционные фигурки маскотов",
    descriptionEn: "Hoodies, artisan keycaps, and mascot figurines",
    descriptionTr: "Kapüşonlular, özel mekanik tuş başlıkları ve maskot figürleri",
    level: 0,
    maxLevel: 8,
    reqCode: 8500,
    bonusDesc: "+80 ₽/сек и +5% к клику",
    bonusDescRu: "+80 ₽/сек и +5% к клику",
    bonusDescEn: "+80 ₽/sec and +5% click power",
    bonusDescTr: "+80 ₽/sn ve tıklamaya +%5 güç"
  },

  // --- 3. ТЕХНОЛОГИИ И ИНФРАСТРУКТУРА ---
  {
    id: "sys_satellite",
    title: "Орбитальный Спутник Uplink",
    titleRu: "Орбитальный Спутник Uplink",
    titleEn: "Orbital Satellite Uplink",
    titleTr: "Yörünge Uydusu Uplink",
    icon: "🛰️",
    category: "tech",
    description: "Низкоорбитальная спутниковая связь с минимальным пингом",
    descriptionRu: "Низкоорбитальная спутниковая связь с минимальным пингом",
    descriptionEn: "Low-orbit satellite communication with ultra-low latency",
    descriptionTr: "Ultra düşük gecikmeli alçak yörünge uydu iletişimi",
    level: 0,
    maxLevel: 5,
    reqCode: 25000,
    bonusDesc: "+300 C#/сек и ускорение комбо",
    bonusDescRu: "+300 C#/сек и ускорение комбо",
    bonusDescEn: "+300 C#/sec and faster combo",
    bonusDescTr: "+300 C#/sn ve daha hızlı kombo"
  },
  {
    id: "sys_cybersec",
    title: "Кибербезопасность & Защита",
    titleRu: "Кибербезопасность & Защита",
    titleEn: "Cybersecurity & Defense",
    titleTr: "Siber Güvenlik ve Savunma",
    icon: "🛡️",
    category: "tech",
    description: "Античит, аппаратный файрвол и аудит уязвимостей смарт-контрактов",
    descriptionRu: "Античит, аппаратный файрвол и аудит уязвимостей смарт-контрактов",
    descriptionEn: "Anti-cheat, hardware firewall, and smart contract vulnerability audits",
    descriptionTr: "Hile karşıtı koruma, donanım güvenlik duvarı ve akıllı sözleşme denetimleri",
    level: 0,
    maxLevel: 6,
    reqCode: 12000,
    bonusDesc: "+15% к защите от багов и стабильности",
    bonusDescRu: "+15% к защите от багов и стабильности",
    bonusDescEn: "+15% bug resistance & stability",
    bonusDescTr: "Hatalara karşı +%15 direnç ve kararlılık"
  },

  // --- 4. КУЛЬТУРА И КОМАНДА ---
  {
    id: "sys_cathaven",
    title: "Офисный Котоприют",
    titleRu: "Офисный Котоприют",
    titleEn: "Office Cat Haven",
    titleTr: "Ofis Kedi Yuvası",
    icon: "🐱",
    category: "culture",
    description: "Котики-талисманы, антистресс и постоянный пассивный буст",
    descriptionRu: "Котики-талисманы, антистресс и постоянный пассивный буст",
    descriptionEn: "Mascot kitties, anti-stress comfort, and permanent passive boost",
    descriptionTr: "Maskot kediler, anti-stres ve kalıcı pasif güçlendirme",
    level: 0,
    maxLevel: 10,
    reqCode: 2000,
    bonusDesc: "+6% ко всем доходам за каждого котика",
    bonusDescRu: "+6% ко всем доходам за каждого котика",
    bonusDescEn: "+6% to all earnings per cat",
    bonusDescTr: "Kedi başına tüm kazançlara +%6"
  },
  {
    id: "sys_esports",
    title: "Киберспортивная Арена",
    titleRu: "Киберспортивная Арена",
    titleEn: "Esports Championship Arena",
    titleTr: "E-Spor Şampiyona Arenası",
    icon: "🏆",
    category: "culture",
    description: "Организация мировых чемпионатов по играм вашей студии",
    descriptionRu: "Организация мировых чемпионатов по играм вашей студии",
    descriptionEn: "Hosting world championships for your studio's game titles",
    descriptionTr: "Stüdyonuzun oyunları için dünya şampiyonaları düzenleyin",
    level: 0,
    maxLevel: 5,
    reqCode: 35000,
    bonusDesc: "+500 ₽/сек и +15% к силе клика",
    bonusDescRu: "+500 ₽/сек и +15% к силе клика",
    bonusDescEn: "+500 ₽/sec and +15% click power",
    bonusDescTr: "+500 ₽/sn ve tıklama gücüne +%15"
  }
];

const LOCAL_STORAGE_KEY = "HELLOTAP_WEB_SAVE_V2";

interface GameContextType {
  lang: Language;
  setLang: (l: Language) => void;
  t: TranslationDictionary;
  codeLines: number;
  money: number;
  totalCodeEver: number;
  prestigeCount: number;
  prestigeTokens: number;
  comboEnergy: number;
  isInFlow: boolean;
  codePerClick: number;
  codePerSec: number;
  moneyPerSec: number;
  globalMultiplier: number;
  upgrades: ShopUpgrade[];
  systems: StudioSystem[];
  dailyDigestClaims: number;
  timeWarpRemainingSec: number;
  adBoostRemainingSec: number;
  switchType: 'blue' | 'red' | 'brown' | 'laser' | 'typewriter';
  setSwitchType: (t: 'blue' | 'red' | 'brown' | 'laser' | 'typewriter') => void;
  themeId: ThemeId;
  setThemeId: (t: ThemeId) => void;
  activeEvent: GameRandomEvent | null;
  dismissEvent: () => void;
  handleEventOption: (option: GameEventOption) => void;
  hasVipX2: boolean;
  hasAutoClicker: boolean;
  hasNoAds: boolean;
  achievements: Record<string, number>;
  achievementBonusMultiplier: number;
  getAchievementProgress: (id: string) => { current: number; nextTarget: number; percent: number };
  achievementToasts: ToastItem[];
  dismissAchievementToast: (id: string) => void;
  isMusicPlaying: boolean;
  toggleMusic: () => boolean;
  currentTrackName: string;
  nextMusicTrack: () => string;
  buyInAppProduct: (productId: string) => Promise<boolean>;
  handleClick: (clientX?: number, clientY?: number) => { isCrit: boolean; codeAdded: number; moneyAdded: number };
  buyUpgrade: (id: number) => boolean;
  upgradeSystem: (id: string) => boolean;
  claimDailyDigest: () => { bonusCode: number; bonusMoney: number };
  triggerTimeWarp: () => boolean;
  triggerPrestigeIPO: () => { gainedTokens: number; gainedSkillPoints: number };
  langCommits: Record<string, number>;
  recordCommit: (langKey: string) => void;
  skillPoints: number;
  unlockedSkills: Record<string, number>;
  upgradeSkill: (skillId: string, cost: number) => boolean;
  resetSkills: () => void;
  overclockRemainingSec: number;
  triggerOverclock: (sec?: number) => void;
  offlineReport: { isOpen: boolean; seconds: number; codeEarned: number; moneyEarned: number } | null;
  claimOfflineEarnings: (double: boolean) => void;
  watchAdForDoubleBoost: () => void;
  watchAdForTimeWarpReset: () => void;
  exportSaveBase64: () => string;
  importSaveBase64: (code: string) => boolean;
  hardReset: () => void;
  gitBranch: string;
  branchCodeLines: number;
  mergedPrCount: number;
  createBranch: (name?: string) => void;
  mergePullRequest: () => { rewardMoney: number; rewardCode: number } | null;
  blitzRemainingSec: number;
  isBlitzActive: boolean;
  triggerRefactorBlitz: (sec?: number) => void;
  isCommandPaletteOpen: boolean;
  setIsCommandPaletteOpen: React.Dispatch<React.SetStateAction<boolean>>;
  contributions: Record<string, number>;
  recordContribution: (amount?: number) => void;
  totalContributions: number;
  currentStreak: number;
  devReputationBonus: number;
  pipelinesPassed: number;
  recordPipelinePass: () => void;
  recordPipelineFix: (bonusCode: number, bonusMoney: number) => void;
  soundProfile: SoundProfile;
  setSoundProfile: (p: SoundProfile) => void;
  sfxVolume: number;
  setSfxVolume: (v: number) => void;
  musicVolume: number;
  setMusicVolume: (v: number) => void;
}

const GameContext = createContext<GameContextType | undefined>(undefined);

export const GameProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [codeLines, setCodeLines] = useState<number>(0);
  const [money, setMoney] = useState<number>(100);
  const [totalCodeEver, setTotalCodeEver] = useState<number>(0);
  const [prestigeCount, setPrestigeCount] = useState<number>(0);
  const [prestigeTokens, setPrestigeTokens] = useState<number>(0);
  const [comboEnergy, setComboEnergy] = useState<number>(0);
  const [upgrades, setUpgrades] = useState<ShopUpgrade[]>(INITIAL_UPGRADES);
  const [systems, setSystems] = useState<StudioSystem[]>(INITIAL_SYSTEMS);
  const [dailyDigestClaims, setDailyDigestClaims] = useState<number>(0);
  const [timeWarpCooldown, setTimeWarpCooldown] = useState<number>(0);
  const [adBoostEndTime, setAdBoostEndTime] = useState<number>(0);
  const [switchType, setSwitchTypeState] = useState<'blue' | 'red' | 'brown' | 'laser' | 'typewriter'>('blue');
  const [lang, setLangState] = useState<Language>(detectInitialLanguage());
  const [hasVipX2, setHasVipX2] = useState<boolean>(false);
  const [hasAutoClicker, setHasAutoClicker] = useState<boolean>(false);
  const [hasNoAds, setHasNoAds] = useState<boolean>(false);
  const [manualClicks, setManualClicks] = useState<number>(0);
  const [critClicks, setCritClicks] = useState<number>(0);
  const [flowEnters, setFlowEnters] = useState<number>(0);
  const [timeWarpsUsed, setTimeWarpsUsed] = useState<number>(0);
  const [testedSwitches, setTestedSwitches] = useState<string[]>(['blue']);
  const [themeId, setThemeIdState] = useState<ThemeId>(DEFAULT_THEME_ID);
  const [langCommits, setLangCommits] = useState<Record<string, number>>({});
  const [gitBranch, setGitBranch] = useState<string>('main');
  const [branchCodeLines, setBranchCodeLines] = useState<number>(0);
  const [mergedPrCount, setMergedPrCount] = useState<number>(0);
  const [blitzEndTime, setBlitzEndTime] = useState<number>(0);
  const [soundProfile, setSoundProfileState] = useState<SoundProfile>(sounds.getProfile());
  const [sfxVolume, setSfxVolumeState] = useState<number>(sounds.getVolume());
  const [musicVolume, setMusicVolumeState] = useState<number>(musicSynth.getVolume());
  const [isCommandPaletteOpen, setIsCommandPaletteOpen] = useState<boolean>(false);
  const [contributions, setContributions] = useState<Record<string, number>>(generateBaselineContributions);
  const [pipelinesPassed, setPipelinesPassed] = useState<number>(0);
  const [activeEvent, setActiveEvent] = useState<GameRandomEvent | null>(null);
  const [skillPoints, setSkillPoints] = useState<number>(0);
  const [unlockedSkills, setUnlockedSkills] = useState<Record<string, number>>({});
  const [overclockEndTime, setOverclockEndTime] = useState<number>(0);
  const [offlineReport, setOfflineReport] = useState<{ isOpen: boolean; seconds: number; codeEarned: number; moneyEarned: number } | null>(null);
  const [achievements, setAchievements] = useState<Record<string, number>>({});
  const achievementsRef = useRef<Record<string, number>>({});
  const [achievementToasts, setAchievementToasts] = useState<ToastItem[]>([]);
  const [isMusicPlaying, setIsMusicPlaying] = useState<boolean>(musicSynth.getIsPlaying());
  const [currentTrackName, setCurrentTrackName] = useState<string>(musicSynth.getTrackName());
  const t = TRANSLATIONS[lang];

  useEffect(() => {
    achievementsRef.current = achievements;
  }, [achievements]);

  const isOverclocked = Date.now() < overclockEndTime;
  const overclockMultiplier = isOverclocked ? 3.0 : 1.0;
  const overclockRemainingSec = Math.max(0, Math.ceil((overclockEndTime - Date.now()) / 1000));

  const triggerOverclock = useCallback((sec: number = 12) => {
    setOverclockEndTime(Date.now() + sec * 1000);
    sounds.playPurchaseSuccess();
    sounds.triggerHaptic('success');
  }, []);

  const upgradeSkill = useCallback((skillId: string, cost: number): boolean => {
    if (skillPoints < cost) return false;
    setSkillPoints(sp => sp - cost);
    setUnlockedSkills(prev => ({
      ...prev,
      [skillId]: (prev[skillId] || 0) + 1
    }));
    return true;
  }, [skillPoints]);

  const resetSkills = useCallback(() => {
    const totalSpent = Object.entries(unlockedSkills).reduce((sum, [id, lvl]) => {
      const node = SKILL_NODES.find(n => n.id === id);
      return sum + (node ? node.costPerLevel * lvl : lvl);
    }, 0);
    setSkillPoints(sp => sp + totalSpent);
    setUnlockedSkills({});
  }, [unlockedSkills]);

  const claimOfflineEarnings = useCallback((double: boolean) => {
    if (!offlineReport) return;
    const mult = double ? 2 : 1;
    const finalCode = offlineReport.codeEarned * mult;
    const finalMoney = offlineReport.moneyEarned * mult;
    setCodeLines(c => c + finalCode);
    setTotalCodeEver(t => t + finalCode);
    setMoney(m => m + finalMoney);
    setOfflineReport(null);
  }, [offlineReport]);

  const TITLES: Record<Language, string> = {
    ru: 'CodeTap: Симулятор Программиста — Gamedev Clicker & Tycoon',
    en: 'CodeTap: Programmer Simulator — Gamedev Clicker & Tycoon',
    tr: 'CodeTap: Yazılımcı Simülatörü — Gamedev Clicker & Tycoon',
  };

  const setLang = (l: Language) => {
    setLangState(l);
    try {
      localStorage.setItem("HELLOTAP_LANG", l);
      document.documentElement.lang = l;
      document.title = TITLES[l] || TITLES.en;
    } catch {}
  };

  useEffect(() => {
    try {
      document.documentElement.lang = lang;
      document.title = TITLES[lang] || TITLES.en;
    } catch {}
  }, [lang]);

  const setSwitchType = (t: 'blue' | 'red' | 'brown' | 'laser' | 'typewriter') => {
    setSwitchTypeState(t);
    sounds.switchType = t;
    sounds.playKeyClick(true);
    setTestedSwitches(prev => prev.includes(t) ? prev : [...prev, t]);
  };

  const nextMusicTrack = useCallback(() => {
    const nextName = musicSynth.nextTrack();
    setCurrentTrackName(nextName);
    setIsMusicPlaying(true);
    return nextName;
  }, []);

  const recordContribution = useCallback((amount: number = 1) => {
    const today = getTodayKey();
    setContributions(prev => ({
      ...prev,
      [today]: (prev[today] || 0) + amount
    }));
  }, []);

  const recordPipelinePass = useCallback(() => {
    setPipelinesPassed(p => p + 1);
    recordContribution(1);
    const deployDividend = (unlockedSkills['skill_auto_deploy'] || 0) * 2000;
    if (deployDividend > 0) {
      setMoney(m => m + deployDividend);
    }
  }, [recordContribution, unlockedSkills]);

  const recordPipelineFix = useCallback((bonusCode: number, bonusMoney: number) => {
    setCodeLines(c => c + bonusCode);
    setTotalCodeEver(t => t + bonusCode);
    setMoney(m => m + bonusMoney);
    setPipelinesPassed(p => p + 1);
    recordContribution(2);
    const deployDividend = (unlockedSkills['skill_auto_deploy'] || 0) * 2000;
    if (deployDividend > 0) {
      setMoney(m => m + deployDividend);
    }
  }, [recordContribution, unlockedSkills]);

  const setSoundProfile = useCallback((profile: SoundProfile) => {
    sounds.setProfile(profile);
    setSoundProfileState(profile);
  }, []);

  const setSfxVolume = useCallback((val: number) => {
    sounds.setVolume(val);
    setSfxVolumeState(val);
  }, []);

  const setMusicVolume = useCallback((val: number) => {
    musicSynth.setVolume(val);
    setMusicVolumeState(val);
  }, []);

  const recordCommit = useCallback((langKey: string) => {
    setLangCommits(prev => ({
      ...prev,
      [langKey]: (prev[langKey] || 0) + 1
    }));
    recordContribution(1);
  }, [recordContribution]);

  const setThemeId = (t: ThemeId) => {
    setThemeIdState(t);
    const theme = IDE_THEMES[t];
    if (theme && theme.soundPreset) {
      setSwitchType(theme.soundPreset);
    }
  };

  const createBranch = useCallback((customName?: string) => {
    const BRANCH_PRESETS = [
      'feat/neural-opt',
      'feat/quantum-speedup',
      'fix/memory-leak',
      'feat/web3-staking',
      'refactor/vulkan-pipeline',
      'feat/microservice-workers',
      'feat/agi-alignment'
    ];
    const name = customName || BRANCH_PRESETS[Math.floor(Math.random() * BRANCH_PRESETS.length)];
    setGitBranch(name);
    setBranchCodeLines(0);
    sounds.playKeyClick(true);
    sounds.triggerHaptic('medium');
  }, []);

  const triggerRefactorBlitz = useCallback((sec: number = 20) => {
    const extraDuration = (unlockedSkills['skill_blitz_compiler'] || 0) * 5;
    setBlitzEndTime(Date.now() + (sec + extraDuration) * 1000);
    sounds.playBlitzSuccess();
    sounds.triggerHaptic('success');
  }, [unlockedSkills]);

  const dismissEvent = useCallback(() => {
    setActiveEvent(null);
  }, []);

  const handleEventOption = useCallback((option: GameEventOption) => {
    switch (option.actionType) {
      case 'grant_money':
        setMoney(m => m + option.value);
        break;
      case 'grant_code':
        setCodeLines(c => c + option.value);
        setTotalCodeEver(t => t + option.value);
        break;
      case 'boost_flow':
        setComboEnergy(1.0);
        setFlowEnters(f => f + 1);
        break;
      case 'boost_cps':
        setCodeLines(c => c + option.value);
        break;
      case 'grant_token':
        setPrestigeTokens(t => t + option.value);
        break;
    }
    setActiveEvent(null);
  }, []);

  const isInFlow = comboEnergy >= 1.0;
  const isAdBoostActive = Date.now() < adBoostEndTime;
  const adBoostMultiplier = isAdBoostActive ? 2.0 : 1.0;

  // Инициализация Yandex Games SDK & Платежей
  useEffect(() => {
    yandexSdk.init().then(() => {
      // Пытаемся загрузить облачные сохранения Яндекс Игр
      yandexSdk.loadFromCloud("hellotap_save").then((cloudData) => {
        if (cloudData && typeof cloudData === 'object') {
          const cd = cloudData as GameSaveData;
          if (cd.codeLines !== undefined && cd.codeLines > codeLines) {
            setCodeLines(cd.codeLines);
            if (cd.money) setMoney(cd.money);
            if (cd.totalCodeEver) setTotalCodeEver(cd.totalCodeEver);
            if (cd.prestigeCount) setPrestigeCount(cd.prestigeCount);
            if (cd.prestigeTokens) setPrestigeTokens(cd.prestigeTokens);
          }
          if (cd.hasVipX2) setHasVipX2(true);
          if (cd.hasAutoClicker) setHasAutoClicker(true);
          if (cd.hasNoAds) setHasNoAds(true);
          if (cd.manualClicks) setManualClicks(cd.manualClicks);
          if (cd.critClicks) setCritClicks(cd.critClicks);
          if (cd.flowEnters) setFlowEnters(cd.flowEnters);
          if (cd.timeWarpsUsed) setTimeWarpsUsed(cd.timeWarpsUsed);
          if (cd.testedSwitches) setTestedSwitches(cd.testedSwitches);
          if (cd.achievements) setAchievements(cd.achievements);
          if (cd.themeId) setThemeIdState(cd.themeId as ThemeId);
          if (cd.skillPoints !== undefined) setSkillPoints(cd.skillPoints);
          if (cd.unlockedSkills) setUnlockedSkills(cd.unlockedSkills);
          if (cd.langCommits) setLangCommits(cd.langCommits);
          if (cd.gitBranch) setGitBranch(cd.gitBranch);
          if (cd.branchCodeLines) setBranchCodeLines(cd.branchCodeLines);
          if (cd.mergedPrCount) setMergedPrCount(cd.mergedPrCount);
          if (cd.contributions) setContributions(cd.contributions);
          if (cd.pipelinesPassed) setPipelinesPassed(cd.pipelinesPassed);
        }
      });

      // Загружаем активные покупки Яндекс Игр
      yandexPayments.getActivePurchases().then(purchases => {
        if (purchases.includes("codetap_vip_x2")) setHasVipX2(true);
        if (purchases.includes("codetap_autoclicker")) setHasAutoClicker(true);
        if (purchases.includes("codetap_noads")) setHasNoAds(true);
      });
    });
  }, []);

  const vipMultiplier = hasVipX2 ? 2.0 : 1.0;

  // Бонус от всех разблокированных уровней достижений (24 ачивки по 3 уровня)
  const achievementBonusMultiplier = 1.0 + Object.entries(achievements).reduce((sum, [achId, tier]) => {
    const def = ACHIEVEMENTS.find(a => a.id === achId);
    if (!def || tier <= 0) return sum;
    const tierBonus = def.tiers.slice(0, tier).reduce((acc, t) => acc + t.bonusMultiplier, 0);
    return sum + tierBonus;
  }, 0);

  // Множители от дерева IT-талантов
  const passiveTalentMult = 1.0 + 
    (unlockedSkills['skill_async_io'] || 0) * 0.20 + 
    (unlockedSkills['skill_k8s_autoscaling'] || 0) * 0.35 + 
    (unlockedSkills['skill_quantum_threads'] || 0) * 0.50 +
    (unlockedSkills['skill_ai_agents'] || 0) * 0.40 +
    (unlockedSkills['skill_auto_deploy'] || 0) * 0.30;

  const moneyTalentMult = 1.0 + 
    (unlockedSkills['skill_venture_network'] || 0) * 0.25 + 
    (unlockedSkills['skill_unicorn_status'] || 0) * 0.40;

  const clickTalentMult = 1.0 +
    (unlockedSkills['skill_prompt_engineering'] || 0) * 0.20;

  const singularityMult = 1.0 +
    (unlockedSkills['skill_agi_singularity'] || 0) * 1.50;

  const isBlitzActive = Date.now() < blitzEndTime;
  const blitzMultiplier = isBlitzActive ? 10.0 : 1.0;
  const blitzRemainingSec = Math.max(0, Math.ceil((blitzEndTime - Date.now()) / 1000));
  const branchMultiplier = gitBranch !== 'main' ? 1.25 : 1.0;

  // Расчет статистики контрибуций GitHub и репутационного бонуса
  const { total: totalContributions, currentStreak } = calculateStreak(contributions);
  const devReputationBonus = Math.min(0.50, Math.floor(totalContributions / 20) * 0.01);
  const devReputationMultiplier = 1.0 + devReputationBonus;

  // Расчет множителей и доходов
  const globalMultiplier = (
    1.0 + 
    (prestigeCount * 0.5) + 
    (prestigeTokens * 0.05) +
    upgrades.reduce((acc, u) => acc + (u.level * u.multiplierBonus), 0) +
    (systems.find(s => s.id === 'sys_cathaven')?.level || 0) * 0.06 +
    (systems.find(s => s.id === 'sys_realestate')?.level || 0) * 0.40 +
    (systems.find(s => s.id === 'sys_esports')?.level || 0) * 0.15 +
    (systems.find(s => s.id === 'sys_cybersec')?.level || 0) * 0.10
  ) * adBoostMultiplier * vipMultiplier * achievementBonusMultiplier * overclockMultiplier * singularityMult * blitzMultiplier * branchMultiplier * devReputationMultiplier;

  const flowMultiplier = isInFlow ? 3.0 : 1.0;

  const baseCpc = 1 + 
    upgrades.reduce((acc, u) => acc + (u.level * u.codePerClickBonus), 0) +
    (systems.find(s => s.id === 'sys_merch')?.level || 0) * 2;
  const codePerClick = baseCpc * globalMultiplier * flowMultiplier * clickTalentMult;

  const baseCps = upgrades.reduce((acc, u) => acc + (u.level * u.codePerSecBonus), 0) +
    (systems.find(s => s.id === 'sys_satellite')?.level || 0) * 300;
  const codePerSec = baseCps * globalMultiplier * flowMultiplier * passiveTalentMult;

  const baseMps = upgrades.reduce((acc, u) => acc + (u.level * u.moneyPerSecBonus), 0) +
    (systems.find(s => s.id === 'sys_assetstore')?.level || 0) * 150 +
    (systems.find(s => s.id === 'sys_merch')?.level || 0) * 80 +
    (systems.find(s => s.id === 'sys_esports')?.level || 0) * 500;
  const moneyPerSec = baseMps * globalMultiplier * flowMultiplier * moneyTalentMult;

  const mergePullRequest = useCallback(() => {
    if (gitBranch === 'main' || branchCodeLines < 1) return null;

    const prSkillBonus = 1.0 + (unlockedSkills['skill_gitops'] || 0) * 0.50;
    const rewardMoney = Math.round((branchCodeLines * Math.max(5, moneyPerSec * 0.25) + 1000) * prSkillBonus);
    const rewardCode = Math.round((branchCodeLines * 2.5 + 500) * prSkillBonus);

    setMoney(m => m + rewardMoney);
    setCodeLines(c => c + rewardCode);
    setTotalCodeEver(t => t + rewardCode);
    setMergedPrCount(c => c + 1);
    recordContribution(3);

    sounds.playBranchMerge();
    sounds.triggerHaptic('success');

    const result = { rewardMoney, rewardCode };
    setGitBranch('main');
    setBranchCodeLines(0);
    return result;
  }, [gitBranch, branchCodeLines, moneyPerSec, unlockedSkills, recordContribution]);

  // Загрузка локальных сохранений
  useEffect(() => {
    try {
      const raw = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (raw) {
        const data: GameSaveData = JSON.parse(raw);
        if (data.codeLines) setCodeLines(data.codeLines);
        if (data.money) setMoney(data.money);
        if (data.totalCodeEver) setTotalCodeEver(data.totalCodeEver);
        if (data.prestigeCount) setPrestigeCount(data.prestigeCount);
        if (data.prestigeTokens) setPrestigeTokens(data.prestigeTokens);
        if (data.dailyDigestClaims) setDailyDigestClaims(data.dailyDigestClaims);
        if (data.timeWarpCooldown) setTimeWarpCooldown(data.timeWarpCooldown);
        if (data.hasVipX2) setHasVipX2(true);
        if (data.hasAutoClicker) setHasAutoClicker(true);
        if (data.hasNoAds) setHasNoAds(true);
        if (data.manualClicks) setManualClicks(data.manualClicks);
        if (data.critClicks) setCritClicks(data.critClicks);
        if (data.flowEnters) setFlowEnters(data.flowEnters);
        if (data.timeWarpsUsed) setTimeWarpsUsed(data.timeWarpsUsed);
        if (data.testedSwitches) setTestedSwitches(data.testedSwitches);
        if (data.achievements) setAchievements(data.achievements);
        if (data.themeId) setThemeIdState(data.themeId as ThemeId);
        if (data.skillPoints !== undefined) setSkillPoints(data.skillPoints);
        if (data.unlockedSkills) setUnlockedSkills(data.unlockedSkills);
        if (data.langCommits) setLangCommits(data.langCommits);
        if (data.gitBranch) setGitBranch(data.gitBranch);
        if (data.branchCodeLines) setBranchCodeLines(data.branchCodeLines);
        if (data.mergedPrCount) setMergedPrCount(data.mergedPrCount);
        if (data.contributions) setContributions(data.contributions);
        if (data.pipelinesPassed) setPipelinesPassed(data.pipelinesPassed);
        if (data.switchType) {
          setSwitchTypeState(data.switchType);
          sounds.switchType = data.switchType;
        }

        if (data.upgrades) {
          setUpgrades(prev => prev.map(u => ({
            ...u,
            level: data.upgrades[u.id] ?? u.level
          })));
        }
        if (data.systems) {
          setSystems(prev => prev.map(s => ({
            ...s,
            level: data.systems[s.id] ?? s.level
          })));
        }

        if (data.lastSeenTime) {
          const offlineSec = Math.min((Date.now() - data.lastSeenTime) / 1000, 43200);
          if (offlineSec > 25) {
            const offCode = Math.floor(Math.max(10, baseCps * globalMultiplier * offlineSec * 0.45));
            const offMoney = Math.floor(Math.max(5, baseMps * globalMultiplier * offlineSec * 0.40));
            if (offCode > 0 || offMoney > 0) {
              setOfflineReport({
                isOpen: true,
                seconds: Math.floor(offlineSec),
                codeEarned: offCode,
                moneyEarned: offMoney
              });
            }
          }
        }
      }
    } catch (e) {
      console.error("Save load error:", e);
    }
  }, []);

  // Сохранение в LocalStorage и Яндекс Облако
  useEffect(() => {
    const save = () => {
      const data: GameSaveData = {
        game: "HelloTap",
        version: "2.8.2",
        timestamp: new Date().toISOString(),
        codeLines,
        money,
        totalCodeEver,
        prestigeCount,
        prestigeTokens,
        upgrades: upgrades.reduce((acc, u) => ({ ...acc, [u.id]: u.level }), {}),
        systems: systems.reduce((acc, s) => ({ ...acc, [s.id]: s.level }), {}),
        lastSeenTime: Date.now(),
        timeWarpCooldown,
        dailyDigestClaims,
        switchType,
        themeId,
        skillPoints,
        unlockedSkills,
        hasVipX2,
        hasAutoClicker,
        hasNoAds,
        manualClicks,
        critClicks,
        flowEnters,
        timeWarpsUsed,
        testedSwitches,
        achievements,
        langCommits,
        gitBranch,
        branchCodeLines,
        mergedPrCount,
        contributions,
        pipelinesPassed
      };
      localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(data));
      yandexSdk.saveToCloud("hellotap_save", data);
      yandexSdk.submitLeaderboardScore(totalCodeEver);
    };

    const interval = setInterval(save, 5000);
    window.addEventListener("beforeunload", save);
    return () => {
      clearInterval(interval);
      window.removeEventListener("beforeunload", save);
    };
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, themeId, skillPoints, unlockedSkills, hasVipX2, hasAutoClicker, hasNoAds, manualClicks, critClicks, flowEnters, timeWarpsUsed, testedSwitches, achievements, langCommits, gitBranch, branchCodeLines, mergedPrCount, contributions, pipelinesPassed]);

  // Периодический спавн случайных мини-событий (каждые 90-120 секунд)
  useEffect(() => {
    const timer = setInterval(() => {
      setActiveEvent(prev => {
        if (prev) return prev;
        return generateRandomEvent(codePerSec, moneyPerSec, lang);
      });
    }, 95000);

    return () => clearInterval(timer);
  }, [codePerSec, moneyPerSec, lang]);

  // Основной цикл
  useEffect(() => {
    const interval = setInterval(() => {
      const dt = 0.1;
      if (codePerSec > 0) {
        setCodeLines(c => c + codePerSec * dt);
        setTotalCodeEver(t => t + codePerSec * dt);
      }
      if (moneyPerSec > 0) {
        setMoney(m => m + moneyPerSec * dt);
      }

      // Автокликер Bot Pro (10 CPS)
      if (hasAutoClicker && codePerClick > 0) {
        const autoCode = codePerClick;
        const autoMoney = codePerClick * 0.25;
        setCodeLines(c => c + autoCode);
        setTotalCodeEver(t => t + autoCode);
        setMoney(m => m + autoMoney);
      }

      // Накопление изменений в Git-ветке при активной разработке
      if (gitBranch !== 'main' && codePerSec > 0) {
        setBranchCodeLines(b => b + Math.max(0.05, codePerSec * dt * 0.04));
      }

      setComboEnergy(energy => {
        if (hasAutoClicker) {
          return Math.min(1.0, energy + 0.005);
        }
        if (energy <= 0) return 0;
        const decayRate = energy >= 1.0 ? 0.08 : 0.04;
        return Math.max(0, energy - decayRate * dt);
      });
    }, 100);

    return () => clearInterval(interval);
  }, [codePerSec, moneyPerSec, hasAutoClicker, codePerClick, gitBranch]);

  // Клик
  const handleClick = useCallback((_clientX?: number, _clientY?: number) => {
    const critChance = Math.min(0.50, 0.12 + (unlockedSkills['skill_clean_code'] || 0) * 0.03 + (unlockedSkills['skill_fine_tuning'] || 0) * 0.05);
    const isCrit = Math.random() < critChance;
    const critMult = isCrit ? (4.0 + (unlockedSkills['skill_pixel_perfect'] || 0) * 1.0 + (unlockedSkills['skill_wasm_speed'] || 0) * 1.5 + (unlockedSkills['skill_fine_tuning'] || 0) * 2.0) : 1.0;
    const codeAdded = codePerClick * critMult;
    const moneyAdded = Math.max(0.5, codeAdded * 0.35);

    setCodeLines(c => c + codeAdded);
    setTotalCodeEver(t => t + codeAdded);
    setMoney(m => m + moneyAdded);

    if (gitBranch !== 'main') {
      setBranchCodeLines(b => b + 1);
    }

    setManualClicks(c => c + 1);
    if (isCrit) setCritClicks(c => c + 1);

    const flowStep = (isCrit ? 0.15 : 0.06) * (1.0 + (unlockedSkills['skill_hot_reload'] || 0) * 0.25);
    setComboEnergy(e => {
      const next = Math.min(1.0, e + flowStep);
      if (next >= 1.0 && e < 1.0) {
        setFlowEnters(f => f + 1);
        sounds.playGlassBell('flow');
      }
      return next;
    });

    sounds.playKeyClick(isCrit);
    if (isCrit) {
      sounds.playGlassBell('crit');
    }
    sounds.triggerHaptic(isCrit ? 'heavy' : 'light');

    return { isCrit, codeAdded, moneyAdded };
  }, [codePerClick, unlockedSkills, gitBranch]);

  // Покупка апгрейда
  const buyUpgrade = useCallback((id: number): boolean => {
    const up = upgrades.find(u => u.id === id);
    if (!up || up.level >= up.maxLevel) return false;

    const discountMultiplier = Math.max(0.65, 1.0 - ((unlockedSkills['skill_negotiation'] || 0) * 0.05 + (unlockedSkills['skill_unicorn_status'] || 0) * 0.08));
    const costCode = Math.floor(up.baseCostCode * Math.pow(up.costMultiplier, up.level) * discountMultiplier);
    const costMoney = Math.floor(up.baseCostMoney * Math.pow(up.costMultiplier, up.level) * discountMultiplier);

    if (codeLines < costCode || money < costMoney) return false;

    setCodeLines(c => c - costCode);
    setMoney(m => m - costMoney);
    setUpgrades(prev => prev.map(u => u.id === id ? { ...u, level: u.level + 1 } : u));

    sounds.playUpgrade();
    sounds.triggerHaptic('medium');
    return true;
  }, [upgrades, codeLines, money, unlockedSkills]);

  // Прокачка системы
  const upgradeSystem = useCallback((id: string): boolean => {
    const sys = systems.find(s => s.id === id);
    if (!sys || sys.level >= sys.maxLevel) return false;

    const cost = Math.floor(sys.reqCode * Math.pow(1.5, sys.level));
    if (codeLines < cost) return false;

    setCodeLines(c => c - cost);
    setSystems(prev => prev.map(s => s.id === id ? { ...s, level: s.level + 1 } : s));

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return true;
  }, [systems, codeLines]);

  // Daily Digest
  const claimDailyDigest = useCallback(() => {
    const bonusCode = 55000 * globalMultiplier;
    const bonusMoney = 85000 * globalMultiplier;

    setCodeLines(c => c + bonusCode);
    setMoney(m => m + bonusMoney);
    setComboEnergy(1.0);
    setDailyDigestClaims(d => d + 1);
    recordContribution(5);

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return { bonusCode, bonusMoney };
  }, [globalMultiplier, recordContribution]);

  // Time Warp
  const triggerTimeWarp = useCallback((): boolean => {
    const now = Date.now();
    if (now < timeWarpCooldown) return false;

    const simulatedCode = Math.max(30000 * globalMultiplier, codePerSec * 7200 * 0.75);
    const simulatedMoney = Math.max(50000 * globalMultiplier, moneyPerSec * 7200 * 0.75);

    setCodeLines(c => c + simulatedCode);
    setMoney(m => m + simulatedMoney);
    setComboEnergy(1.0);
    setTimeWarpCooldown(now + 1800 * 1000);
    setTimeWarpsUsed(w => w + 1);

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return true;
  }, [timeWarpCooldown, globalMultiplier, codePerSec, moneyPerSec]);

  // Яндекс Реклама: Буст x2 на 3 минуты (или мгновенно с No-Ads)
  const watchAdForDoubleBoost = useCallback(() => {
    if (hasNoAds) {
      setAdBoostEndTime(Date.now() + 180 * 1000);
      sounds.playRelease();
      sounds.triggerHaptic('success');
      return;
    }
    yandexSdk.showRewardedVideo(() => {
      setAdBoostEndTime(Date.now() + 180 * 1000);
      sounds.playRelease();
      sounds.triggerHaptic('success');
    });
  }, [hasNoAds]);

  // Яндекс Реклама: Сброс кулдауна Time Warp (или мгновенно с No-Ads)
  const watchAdForTimeWarpReset = useCallback(() => {
    if (hasNoAds) {
      setTimeWarpCooldown(0);
      sounds.playRelease();
      sounds.triggerHaptic('success');
      return;
    }
    yandexSdk.showRewardedVideo(() => {
      setTimeWarpCooldown(0);
      sounds.playRelease();
      sounds.triggerHaptic('success');
    });
  }, [hasNoAds]);

  // Покупка In-App товара Яндекс Игр
  const buyInAppProduct = useCallback(async (productId: string): Promise<boolean> => {
    try {
      const res = await yandexPayments.buyProduct(productId);
      if (!res.success) return false;

      if (productId === "codetap_vip_x2") {
        setHasVipX2(true);
      } else if (productId === "codetap_autoclicker") {
        setHasAutoClicker(true);
      } else if (productId === "codetap_noads") {
        setHasNoAds(true);
        yandexSdk.hideStickyBanner();
      } else if (productId === "codetap_stocks_100") {
        setPrestigeTokens(t => t + 100);
      } else if (productId === "codetap_money_1m") {
        setMoney(m => m + 1000000);
      }

      sounds.playPurchaseSuccess();
      sounds.triggerHaptic('success');
      return true;
    } catch (err) {
      console.error("[GameContext] buyInAppProduct error:", err);
      return false;
    }
  }, []);

  // Синхронизация RTB Sticky-баннера Яндекс Игр
  useEffect(() => {
    if (hasNoAds) {
      yandexSdk.hideStickyBanner();
    } else {
      yandexSdk.showStickyBanner();
    }
  }, [hasNoAds]);

  // Престиж / Выход на IPO
  const triggerPrestigeIPO = useCallback(() => {
    const newTokens = Math.floor(Math.sqrt(totalCodeEver / 80000));
    if (newTokens <= 0) return { gainedTokens: 0, gainedSkillPoints: 0 };

    const gainedSkillPoints = 3 + Math.floor(newTokens / 15);
    const ipoCashBonus = Math.max(5000 * (prestigeCount + 1), Math.floor(money * 0.15));
    setPrestigeCount(p => p + 1);
    setPrestigeTokens(t => t + newTokens);
    setSkillPoints(sp => sp + gainedSkillPoints);
    setCodeLines(0);
    // Деньги и купленные апгрейды сохраняются! Начисляем инвестиционный грант IPO
    setMoney(m => m + ipoCashBonus);

    sounds.playRelease();
    sounds.triggerHaptic('success');
    yandexSdk.showInterstitial();
    return { gainedTokens: newTokens, gainedSkillPoints };
  }, [totalCodeEver, prestigeCount, money]);

  // Получение актуального значения прогресса для любого типа ачивки
  const getStatValue = useCallback((statKey: string): number => {
    switch (statKey) {
      case 'manualClicks': return manualClicks;
      case 'critClicks': return critClicks;
      case 'flowEnters': return flowEnters;
      case 'totalCodeEver': return totalCodeEver;
      case 'codePerSec': return codePerSec;
      case 'money': return money;
      case 'moneyPerSec': return moneyPerSec;
      case 'upgrade_1': return upgrades.find(u => u.id === 1)?.level || 0;
      case 'upgrade_2': return upgrades.find(u => u.id === 2)?.level || 0;
      case 'upgrade_3': return upgrades.find(u => u.id === 3)?.level || 0;
      case 'upgrade_4': return upgrades.find(u => u.id === 4)?.level || 0;
      case 'upgrade_5': return upgrades.find(u => u.id === 5)?.level || 0;
      case 'upgrade_6': return upgrades.find(u => u.id === 6)?.level || 0;
      case 'totalUpgradeLevels': return upgrades.reduce((acc, u) => acc + u.level, 0);
      case 'unlockedSystemsCount': return systems.filter(s => s.level > 0).length;
      case 'sys_cathaven': return systems.find(s => s.id === 'sys_cathaven')?.level || 0;
      case 'sys_esports': return systems.find(s => s.id === 'sys_esports')?.level || 0;
      case 'sys_realestate': return systems.find(s => s.id === 'sys_realestate')?.level || 0;
      case 'sys_assetstore': return systems.find(s => s.id === 'sys_assetstore')?.level || 0;
      case 'sys_merch': return systems.find(s => s.id === 'sys_merch')?.level || 0;
      case 'prestigeCount': return prestigeCount;
      case 'prestigeTokens': return prestigeTokens;
      case 'timeWarpsUsed': return timeWarpsUsed;
      case 'switchesTestedCount': return testedSwitches.length;
      case 'commits_python': return langCommits['python'] || 0;
      case 'commits_cpp': return langCommits['cpp'] || 0;
      case 'commits_solidity': return langCommits['solidity'] || 0;
      case 'commits_rust': return langCommits['rust'] || 0;
      default: return 0;
    }
  }, [manualClicks, critClicks, flowEnters, totalCodeEver, codePerSec, money, moneyPerSec, upgrades, systems, prestigeCount, prestigeTokens, timeWarpsUsed, testedSwitches, langCommits]);

  const getAchievementProgress = useCallback((id: string) => {
    const def = ACHIEVEMENTS.find(a => a.id === id);
    if (!def) return { current: 0, nextTarget: 1, percent: 0 };
    const currentTier = achievements[id] || 0;
    const currentVal = getStatValue(def.statKey);
    if (currentTier >= 3) {
      return { current: currentVal, nextTarget: def.tiers[2].target, percent: 100 };
    }
    const nextTarget = def.tiers[currentTier].target;
    const percent = Math.min(100, (currentVal / nextTarget) * 100);
    return { current: currentVal, nextTarget, percent };
  }, [achievements, getStatValue]);

  // Проверка прогресса ачивок и создание тостов без дублирования
  useEffect(() => {
    ACHIEVEMENTS.forEach(ach => {
      const currentTier = achievementsRef.current[ach.id] || 0;
      if (currentTier >= 3) return;

      const val = getStatValue(ach.statKey);
      const nextTierDef = ach.tiers[currentTier];

      if (val >= nextTierDef.target) {
        const newTier = (currentTier + 1) as 1 | 2 | 3;
        // Мгновенная синхронная блокировка во избежание дублирования тостов при высоком CPS
        achievementsRef.current[ach.id] = newTier;
        setAchievements(prev => ({ ...prev, [ach.id]: newTier }));

        const toastId = `${ach.id}_${newTier}_${Date.now()}`;
        setAchievementToasts(prev => [
          ...prev.slice(-2), // Храним не более 2 активных тостов в очереди
          {
            id: toastId,
            icon: ach.icon,
            title: lang === 'ru' ? ach.titleRu : lang === 'tr' ? (ach.titleTr || ach.titleEn) : ach.titleEn,
            tier: newTier,
            rewardDesc: lang === 'ru' ? nextTierDef.rewardDescRu : lang === 'tr' ? (nextTierDef.rewardDescTr || nextTierDef.rewardDescEn) : nextTierDef.rewardDescEn
          }
        ]);

        sounds.playUpgrade();
        sounds.triggerHaptic('success');

        setTimeout(() => {
          setAchievementToasts(prev => prev.filter(t => t.id !== toastId));
        }, 2800);
      }
    });
  }, [manualClicks, critClicks, flowEnters, totalCodeEver, codePerSec, money, moneyPerSec, upgrades, systems, prestigeCount, prestigeTokens, timeWarpsUsed, testedSwitches, langCommits, getStatValue, lang]);

  const dismissAchievementToast = useCallback((id: string) => {
    setAchievementToasts(prev => prev.filter(t => t.id !== id));
  }, []);

  const toggleMusic = useCallback(() => {
    const playing = musicSynth.toggle();
    setIsMusicPlaying(playing);
    return playing;
  }, []);

  // Base64 Экспорт
  const exportSaveBase64 = useCallback((): string => {
    const data: GameSaveData = {
      game: "HelloTap",
      version: "2.8.2",
      timestamp: new Date().toISOString(),
      codeLines,
      money,
      totalCodeEver,
      prestigeCount,
      prestigeTokens,
      upgrades: upgrades.reduce((acc, u) => ({ ...acc, [u.id]: u.level }), {}),
      systems: systems.reduce((acc, s) => ({ ...acc, [s.id]: s.level }), {}),
      lastSeenTime: Date.now(),
      timeWarpCooldown,
      dailyDigestClaims,
      switchType,
      themeId,
      skillPoints,
      unlockedSkills,
      hasVipX2,
      hasAutoClicker,
      hasNoAds,
      manualClicks,
      critClicks,
      flowEnters,
      timeWarpsUsed,
      testedSwitches,
      achievements,
      langCommits,
      gitBranch,
      branchCodeLines,
      mergedPrCount,
      contributions,
      pipelinesPassed
    };
    const json = JSON.stringify(data);
    return "HELLOTAP_SAVE_V2:" + btoa(unescape(encodeURIComponent(json)));
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, themeId, skillPoints, unlockedSkills, hasVipX2, hasAutoClicker, hasNoAds, manualClicks, critClicks, flowEnters, timeWarpsUsed, testedSwitches, achievements, langCommits, gitBranch, branchCodeLines, mergedPrCount, contributions, pipelinesPassed]);

  // Base64 Импорт
  const importSaveBase64 = useCallback((code: string): boolean => {
    try {
      let b64 = code.trim();
      if (b64.startsWith("HELLOTAP_SAVE_V2:")) b64 = b64.substring("HELLOTAP_SAVE_V2:".length);
      else if (b64.startsWith("HELLOTAP_SAVE_V1:")) b64 = b64.substring("HELLOTAP_SAVE_V1:".length);
      const json = decodeURIComponent(escape(atob(b64)));
      const data = JSON.parse(json);
      if (data.game !== "HelloTap") return false;

      if (data.codeLines) setCodeLines(data.codeLines);
      if (data.money) setMoney(data.money);
      if (data.totalCodeEver) setTotalCodeEver(data.totalCodeEver);
      if (data.prestigeCount) setPrestigeCount(data.prestigeCount);
      if (data.prestigeTokens) setPrestigeTokens(data.prestigeTokens);
      if (data.hasVipX2) setHasVipX2(true);
      if (data.hasAutoClicker) setHasAutoClicker(true);
      if (data.hasNoAds) setHasNoAds(true);
      if (data.manualClicks) setManualClicks(data.manualClicks);
      if (data.critClicks) setCritClicks(data.critClicks);
      if (data.flowEnters) setFlowEnters(data.flowEnters);
      if (data.timeWarpsUsed) setTimeWarpsUsed(data.timeWarpsUsed);
      if (data.testedSwitches) setTestedSwitches(data.testedSwitches);
      if (data.achievements) setAchievements(data.achievements);
      if (data.themeId) setThemeIdState(data.themeId as ThemeId);
      if (data.skillPoints !== undefined) setSkillPoints(data.skillPoints);
      if (data.unlockedSkills) setUnlockedSkills(data.unlockedSkills);
      if (data.langCommits) setLangCommits(data.langCommits);
      if (data.gitBranch) setGitBranch(data.gitBranch);
      if (data.branchCodeLines) setBranchCodeLines(data.branchCodeLines);
      if (data.mergedPrCount) setMergedPrCount(data.mergedPrCount);
      if (data.contributions) setContributions(data.contributions);
      if (data.pipelinesPassed) setPipelinesPassed(data.pipelinesPassed);
      return true;
    } catch {
      return false;
    }
  }, []);

  const hardReset = useCallback(() => {
    localStorage.removeItem(LOCAL_STORAGE_KEY);
    window.location.reload();
  }, []);

  const timeWarpRemainingSec = Math.max(0, Math.ceil((timeWarpCooldown - Date.now()) / 1000));
  const adBoostRemainingSec = Math.max(0, Math.ceil((adBoostEndTime - Date.now()) / 1000));

  return (
    <GameContext.Provider
      value={{
        lang,
        setLang,
        t,
        codeLines,
        money,
        totalCodeEver,
        prestigeCount,
        prestigeTokens,
        comboEnergy,
        isInFlow,
        codePerClick,
        codePerSec,
        moneyPerSec,
        globalMultiplier,
        upgrades,
        systems,
        dailyDigestClaims,
        timeWarpRemainingSec,
        adBoostRemainingSec,
        switchType,
        setSwitchType,
        themeId,
        setThemeId,
        activeEvent,
        dismissEvent,
        handleEventOption,
        skillPoints,
        unlockedSkills,
        upgradeSkill,
        resetSkills,
        overclockRemainingSec,
        triggerOverclock,
        offlineReport,
        claimOfflineEarnings,
        hasVipX2,
        hasAutoClicker,
        hasNoAds,
        achievements,
        achievementBonusMultiplier,
        getAchievementProgress,
        achievementToasts,
        dismissAchievementToast,
        isMusicPlaying,
        toggleMusic,
        currentTrackName,
        nextMusicTrack,
        buyInAppProduct,
        handleClick,
        buyUpgrade,
        upgradeSystem,
        claimDailyDigest,
        triggerTimeWarp,
        triggerPrestigeIPO,
        langCommits,
        recordCommit,
        gitBranch,
        branchCodeLines,
        mergedPrCount,
        createBranch,
        mergePullRequest,
        blitzRemainingSec,
        isBlitzActive,
        triggerRefactorBlitz,
        isCommandPaletteOpen,
        setIsCommandPaletteOpen,
        contributions,
        recordContribution,
        totalContributions,
        currentStreak,
        devReputationBonus,
        pipelinesPassed,
        recordPipelinePass,
        recordPipelineFix,
        soundProfile,
        setSoundProfile,
        sfxVolume,
        setSfxVolume,
        musicVolume,
        setMusicVolume,
        watchAdForDoubleBoost,
        watchAdForTimeWarpReset,
        exportSaveBase64,
        importSaveBase64,
        hardReset
      }}
    >
      {children}
    </GameContext.Provider>
  );
};

export const useGame = () => {
  const context = useContext(GameContext);
  if (!context) throw new Error("useGame must be used within GameProvider");
  return context;
};
