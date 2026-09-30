import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { ShopUpgrade, StudioSystem, GameSaveData, HubCategoryType } from '../types/game';
import { sounds } from '../utils/soundEffects';
import { yandexSdk } from '../utils/yandexSdk';
import { yandexPayments } from '../utils/yandexPayments';
import { Language, TranslationDictionary, TRANSLATIONS, detectInitialLanguage } from '../utils/i18n';

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
  switchType: 'blue' | 'red' | 'brown' | 'laser';
  setSwitchType: (t: 'blue' | 'red' | 'brown' | 'laser') => void;
  hasVipX2: boolean;
  hasAutoClicker: boolean;
  hasNoAds: boolean;
  buyInAppProduct: (productId: string) => Promise<boolean>;
  handleClick: (clientX?: number, clientY?: number) => { isCrit: boolean; codeAdded: number; moneyAdded: number };
  buyUpgrade: (id: number) => boolean;
  upgradeSystem: (id: string) => boolean;
  claimDailyDigest: () => { bonusCode: number; bonusMoney: number };
  triggerTimeWarp: () => boolean;
  triggerPrestigeIPO: () => { gainedTokens: number };
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
  const [switchType, setSwitchTypeState] = useState<'blue' | 'red' | 'brown' | 'laser'>('blue');
  const [lang, setLangState] = useState<Language>(detectInitialLanguage());
  const [hasVipX2, setHasVipX2] = useState<boolean>(false);
  const [hasAutoClicker, setHasAutoClicker] = useState<boolean>(false);
  const [hasNoAds, setHasNoAds] = useState<boolean>(false);
  const t = TRANSLATIONS[lang];

  const setLang = (l: Language) => {
    setLangState(l);
    try {
      localStorage.setItem("HELLOTAP_LANG", l);
    } catch {}
  };

  const setSwitchType = (t: 'blue' | 'red' | 'brown' | 'laser') => {
    setSwitchTypeState(t);
    sounds.switchType = t;
    sounds.playKeyClick(true);
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
  ) * adBoostMultiplier * vipMultiplier;

  const flowMultiplier = isInFlow ? 3.0 : 1.0;

  const baseCpc = 1 + 
    upgrades.reduce((acc, u) => acc + (u.level * u.codePerClickBonus), 0) +
    (systems.find(s => s.id === 'sys_merch')?.level || 0) * 2;
  const codePerClick = baseCpc * globalMultiplier * flowMultiplier;

  const baseCps = upgrades.reduce((acc, u) => acc + (u.level * u.codePerSecBonus), 0) +
    (systems.find(s => s.id === 'sys_satellite')?.level || 0) * 300;
  const codePerSec = baseCps * globalMultiplier * flowMultiplier;

  const baseMps = upgrades.reduce((acc, u) => acc + (u.level * u.moneyPerSecBonus), 0) +
    (systems.find(s => s.id === 'sys_assetstore')?.level || 0) * 150 +
    (systems.find(s => s.id === 'sys_merch')?.level || 0) * 80 +
    (systems.find(s => s.id === 'sys_esports')?.level || 0) * 500;
  const moneyPerSec = baseMps * globalMultiplier * flowMultiplier;

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
          if (offlineSec > 10) {
            const offCode = baseCps * globalMultiplier * offlineSec * 0.45;
            const offMoney = baseMps * globalMultiplier * offlineSec * 0.40;
            if (offCode > 0) setCodeLines(c => c + offCode);
            if (offMoney > 0) setMoney(m => m + offMoney);
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
        version: "2.3.0",
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
        hasVipX2,
        hasAutoClicker,
        hasNoAds
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
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, hasVipX2, hasAutoClicker, hasNoAds]);

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
    const isCrit = Math.random() < 0.12;
    const critMult = isCrit ? 4.0 : 1.0;
    const codeAdded = codePerClick * critMult;
    const moneyAdded = Math.max(0.5, codeAdded * 0.35);

    setCodeLines(c => c + codeAdded);
    setTotalCodeEver(t => t + codeAdded);
    setMoney(m => m + moneyAdded);

    setComboEnergy(e => Math.min(1.0, e + (isCrit ? 0.15 : 0.06)));

    sounds.playKeyClick(isCrit);
    sounds.triggerHaptic(isCrit ? 'heavy' : 'light');

    return { isCrit, codeAdded, moneyAdded };
  }, [codePerClick]);

  // Покупка апгрейда
  const buyUpgrade = useCallback((id: number): boolean => {
    const up = upgrades.find(u => u.id === id);
    if (!up || up.level >= up.maxLevel) return false;

    const costCode = Math.floor(up.baseCostCode * Math.pow(up.costMultiplier, up.level));
    const costMoney = Math.floor(up.baseCostMoney * Math.pow(up.costMultiplier, up.level));

    if (codeLines < costCode || money < costMoney) return false;

    setCodeLines(c => c - costCode);
    setMoney(m => m - costMoney);
    setUpgrades(prev => prev.map(u => u.id === id ? { ...u, level: u.level + 1 } : u));

    sounds.playUpgrade();
    sounds.triggerHaptic('medium');
    return true;
  }, [upgrades, codeLines, money]);

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
      } else if (productId === "codetap_stocks_100") {
        setPrestigeTokens(t => t + 100);
      } else if (productId === "codetap_money_1m") {
        setMoney(m => m + 1000000);
      }

      sounds.playRelease();
      sounds.triggerHaptic('success');
      return true;
    } catch (err) {
      console.error("[GameContext] buyInAppProduct error:", err);
      return false;
    }
  }, []);

  // Престиж / Выход на IPO
  const triggerPrestigeIPO = useCallback(() => {
    const newTokens = Math.floor(Math.sqrt(totalCodeEver / 80000));
    if (newTokens <= 0) return { gainedTokens: 0 };

    setPrestigeCount(p => p + 1);
    setPrestigeTokens(t => t + newTokens);
    setCodeLines(0);
    setMoney(500 * (prestigeCount + 1));
    setUpgrades(INITIAL_UPGRADES);

    sounds.playRelease();
    sounds.triggerHaptic('success');
    yandexSdk.showInterstitial();
    return { gainedTokens: newTokens };
  }, [totalCodeEver, prestigeCount]);

  // Base64 Экспорт
  const exportSaveBase64 = useCallback((): string => {
    const data: GameSaveData = {
      game: "HelloTap",
      version: "2.3.0",
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
      hasVipX2,
      hasAutoClicker,
      hasNoAds
    };
    const json = JSON.stringify(data);
    return "HELLOTAP_SAVE_V2:" + btoa(unescape(encodeURIComponent(json)));
  }, [codeLines, money, totalCodeEver, prestigeCount, prestigeTokens, upgrades, systems, timeWarpCooldown, dailyDigestClaims, switchType, hasVipX2, hasAutoClicker, hasNoAds]);

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
        hasVipX2,
        hasAutoClicker,
        hasNoAds,
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
