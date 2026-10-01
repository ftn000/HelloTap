import React, { createContext, useContext, useState, useEffect, useCallback, useRef } from 'react';
import { ShopUpgrade, StudioSystem, GameSaveData, HubCategoryType } from '../types/game';
import { sounds } from '../utils/soundEffects';
import { yandexSdk } from '../utils/yandexSdk';
import { yandexPayments } from '../utils/yandexPayments';
import { Language, TranslationDictionary, TRANSLATIONS, detectInitialLanguage } from '../utils/i18n';
import { ACHIEVEMENTS } from '../utils/achievementsList';
import { ToastItem } from '../components/AchievementToast';
import { musicSynth } from '../utils/musicSynth';
import { ThemeId } from '../types/themes';
import { DEFAULT_THEME_ID } from '../utils/themesList';
import { GameRandomEvent, GameEventOption } from '../types/events';
import { generateRandomEvent } from '../utils/eventsList';
import { SKILL_NODES } from '../utils/skillsList';

const INITIAL_UPGRADES: ShopUpgrade[] = [
  {
    id: 1,
    name: "Эспрессо из Кении",
    category: "click",
    icon: "☕",
    description: "Двойной шот кофеина повышает скорость набора кода",
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
    category: "click",
    icon: "⌨️",
    description: "Тактильные свитчи Lubed Tealios для сверхбыстрого ввода",
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
    category: "idle",
    icon: "🤖",
    description: "Автоматическая сборка релизов и непрерывный деплой",
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
    category: "idle",
    icon: "🧠",
    description: "Нейросеть автодополняет функции и рефакторит спагетти",
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
    category: "idle",
    icon: "🖧",
    description: "Собственный микросерверный кластер студии",
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
    category: "synergy",
    icon: "⚛️",
    description: "Квантовая суперпозиция параллельных вычислений",
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
    category: "idle",
    icon: "🧠",
    description: "LLM-ассистент генерирует микросервисы и пишет тесты",
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
    category: "idle",
    icon: "⚡",
    description: "Стойка из 8x H100 с жидкостным охлаждением для обучения моделей",
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
    category: "idle",
    icon: "🤖",
    description: "Рой AI-агентов закрывает тикеты на GitHub и рефакторит код 24/7",
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
    category: "synergy",
    icon: "💎",
    description: "Гранты и донаты от IT-гигантов за открытые библиотеки студии",
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
    category: "synergy",
    icon: "🔮",
    description: "Квантовая суперпозиция компилирует миллиарды комбинаций кода мгновенно",
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
    category: "synergy",
    icon: "🛰️",
    description: "Космический лазерный канал связи: глобальное покрытие планеты без задержек",
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
    icon: "📋",
    category: "office",
    description: "Сводный отчет за сессию и быстрый сбор наград студии в 1 клик",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Сбор всех дивидендов студии"
  },
  {
    id: "sys_saveexport",
    title: "Облако и Экспорт Сохранений",
    icon: "💾",
    category: "office",
    description: "Резервное копирование и перенос прогресса между устройствами",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Поддержка Яндекс Облака"
  },
  {
    id: "sys_realestate",
    title: "Студийная Недвижимость",
    icon: "🏢",
    category: "office",
    description: "Переезд из гаража в open-space лофт и небоскреб Silicon Tower",
    level: 0,
    maxLevel: 5,
    reqCode: 6000,
    bonusDesc: "+40% к глобальному множителю за уровень"
  },

  // --- 2. БИЗНЕС И РЫНОК ---
  {
    id: "sys_assetstore",
    title: "Маркетплейс Ассетов",
    icon: "🏪",
    category: "business",
    description: "Публикация шейдеров, 3D-моделей и C#-скриптов на маркетплейс",
    level: 0,
    maxLevel: 10,
    reqCode: 3500,
    bonusDesc: "+150 ₽/сек пассивных роялти за уровень"
  },
  {
    id: "sys_venture",
    title: "Венчурные Инвестиции",
    icon: "💼",
    category: "business",
    description: "Питч-сессии перед венчурными фондами Кремниевой Долины",
    level: 0,
    maxLevel: 5,
    reqCode: 15000,
    bonusDesc: "Гранты инвесторов и +25% к дивидендам"
  },
  {
    id: "sys_merch",
    title: "Студийный Мерч-Стор",
    icon: "👕",
    category: "business",
    description: "Худи, механические кейкапы и коллекционные фигурки маскотов",
    level: 0,
    maxLevel: 8,
    reqCode: 8500,
    bonusDesc: "+80 ₽/сек и +5% к клику"
  },

  // --- 3. ТЕХНОЛОГИИ И ИНФРАСТРУКТУРА ---
  {
    id: "sys_satellite",
    title: "Орбитальный Спутник Uplink",
    icon: "🛰️",
    category: "tech",
    description: "Низкоорбитальная спутниковая связь с минимальным пингом",
    level: 0,
    maxLevel: 5,
    reqCode: 25000,
    bonusDesc: "+300 C#/сек и ускорение комбо"
  },
  {
    id: "sys_cybersec",
    title: "Кибербезопасность & Защита",
    icon: "🛡️",
    category: "tech",
    description: "Античит, аппаратный файрвол и аудит уязвимостей смарт-контрактов",
    level: 0,
    maxLevel: 6,
    reqCode: 12000,
    bonusDesc: "+15% к защите от багов и стабильности"
  },

  // --- 4. КУЛЬТУРА И КОМАНДА ---
  {
    id: "sys_cathaven",
    title: "Офисный Котоприют",
    icon: "🐱",
    category: "culture",
    description: "Котики-талисманы, антистресс и постоянный пассивный буст",
    level: 0,
    maxLevel: 10,
    reqCode: 2000,
    bonusDesc: "+6% ко всем доходам за каждого котика"
  },
  {
    id: "sys_esports",
    title: "Киберспортивная Арена",
    icon: "🏆",
    category: "culture",
    description: "Организация мировых чемпионатов по играм вашей студии",
    level: 0,
    maxLevel: 5,
    reqCode: 35000,
    bonusDesc: "+500 ₽/сек и +15% к силе клика"
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

  const setLang = (l: Language) => {
    setLangState(l);
    try {
      localStorage.setItem("HELLOTAP_LANG", l);
    } catch {}
  };

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

  const setThemeId = (t: ThemeId) => {
    setThemeIdState(t);
  };

  const dismissEvent = () => {
    setActiveEvent(null);
  };

  const handleEventOption = (option: GameEventOption) => {
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
  };

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
    (unlockedSkills['skill_ai_agents'] || 0) * 0.40;

  const moneyTalentMult = 1.0 + 
    (unlockedSkills['skill_venture_network'] || 0) * 0.25 + 
    (unlockedSkills['skill_unicorn_status'] || 0) * 0.40;

  const clickTalentMult = 1.0 +
    (unlockedSkills['skill_prompt_engineering'] || 0) * 0.20;

  const singularityMult = 1.0 +
    (unlockedSkills['skill_agi_singularity'] || 0) * 1.50;

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
  ) * adBoostMultiplier * vipMultiplier * achievementBonusMultiplier * overclockMultiplier * singularityMult;

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
        achievements
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
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, themeId, skillPoints, unlockedSkills, hasVipX2, hasAutoClicker, hasNoAds, manualClicks, critClicks, flowEnters, timeWarpsUsed, testedSwitches, achievements]);

  // Периодический спавн случайных мини-событий (каждые 90-120 секунд)
  useEffect(() => {
    const timer = setInterval(() => {
      setActiveEvent(prev => {
        if (prev) return prev;
        return generateRandomEvent(codePerSec, moneyPerSec);
      });
    }, 95000);

    return () => clearInterval(timer);
  }, [codePerSec, moneyPerSec]);

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
  }, [codePerSec, moneyPerSec, hasAutoClicker, codePerClick]);

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

    setManualClicks(c => c + 1);
    if (isCrit) setCritClicks(c => c + 1);

    const flowStep = (isCrit ? 0.15 : 0.06) * (1.0 + (unlockedSkills['skill_hot_reload'] || 0) * 0.25);
    setComboEnergy(e => {
      const next = Math.min(1.0, e + flowStep);
      if (next >= 1.0 && e < 1.0) {
        setFlowEnters(f => f + 1);
      }
      return next;
    });

    sounds.playKeyClick(isCrit);
    sounds.triggerHaptic(isCrit ? 'heavy' : 'light');

    return { isCrit, codeAdded, moneyAdded };
  }, [codePerClick, unlockedSkills]);

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

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return { bonusCode, bonusMoney };
  }, [globalMultiplier]);

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
      default: return 0;
    }
  }, [manualClicks, critClicks, flowEnters, totalCodeEver, codePerSec, money, moneyPerSec, upgrades, systems, prestigeCount, prestigeTokens, timeWarpsUsed, testedSwitches]);

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
            title: lang === 'ru' ? ach.titleRu : ach.titleEn,
            tier: newTier,
            rewardDesc: lang === 'ru' ? nextTierDef.rewardDescRu : nextTierDef.rewardDescEn
          }
        ]);

        sounds.playUpgrade();
        sounds.triggerHaptic('success');

        setTimeout(() => {
          setAchievementToasts(prev => prev.filter(t => t.id !== toastId));
        }, 2800);
      }
    });
  }, [manualClicks, critClicks, flowEnters, totalCodeEver, codePerSec, money, moneyPerSec, upgrades, systems, prestigeCount, prestigeTokens, timeWarpsUsed, testedSwitches, getStatValue, lang]);

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
      achievements
    };
    const json = JSON.stringify(data);
    return "HELLOTAP_SAVE_V2:" + btoa(unescape(encodeURIComponent(json)));
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, themeId, skillPoints, unlockedSkills, hasVipX2, hasAutoClicker, hasNoAds, manualClicks, critClicks, flowEnters, timeWarpsUsed, testedSwitches, achievements]);

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
