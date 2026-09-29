import React, { createContext, useContext, useState, useEffect, useRef, useCallback } from 'react';
import { ShopUpgrade, StudioSystem, GameSaveData } from '../types/game';
import { sounds } from '../utils/soundEffects';

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
    codePerSecBonus: 8,
    moneyPerSecBonus: 12,
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
    codePerSecBonus: 32,
    moneyPerSecBonus: 45,
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
    codePerSecBonus: 140,
    moneyPerSecBonus: 210,
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
    codePerClickBonus: 20,
    codePerSecBonus: 650,
    moneyPerSecBonus: 980,
    multiplierBonus: 0.20
  }
];

const INITIAL_SYSTEMS: StudioSystem[] = [
  {
    id: "sys_dailydigest",
    title: "Утренний Дайджест и Сбор Доходов",
    icon: "📋",
    category: "office",
    description: "Сводный отчет за сессию и сбор наград студии в 1 клик",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Автоматический сбор всех бонусов студии"
  },
  {
    id: "sys_saveexport",
    title: "Облако и Экспорт Сохранений",
    icon: "💾",
    category: "office",
    description: "Резервное копирование и перенос прогресса в Base64",
    level: 1,
    maxLevel: 1,
    reqCode: 0,
    bonusDesc: "Синхронизация между устройствами"
  },
  {
    id: "sys_cathaven",
    title: "Офисный Котоприют",
    icon: "🐱",
    category: "culture",
    description: "Котики-талисманы, антистресс и постоянный пассивный буст",
    level: 0,
    maxLevel: 10,
    reqCode: 2500,
    bonusDesc: "+5% ко всем доходам за каждого котика"
  },
  {
    id: "sys_realestate",
    title: "Студийная Недвижимость",
    icon: "🏢",
    category: "office",
    description: "Переезд из гаража в open-space лофт и небоскреб Silicon Tower",
    level: 0,
    maxLevel: 5,
    reqCode: 8000,
    bonusDesc: "Новый офис и глобальный множитель x1.5 за уровень"
  }
];

const LOCAL_STORAGE_KEY = "HELLOTAP_WEB_SAVE_V2";

interface GameContextType {
  codeLines: number;
  money: number;
  totalCodeEver: number;
  prestigeCount: number;
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
  handleClick: (clientX?: number, clientY?: number) => { isCrit: boolean; codeAdded: number; moneyAdded: number };
  buyUpgrade: (id: number) => boolean;
  upgradeSystem: (id: string) => boolean;
  claimDailyDigest: () => { bonusCode: number; bonusMoney: number };
  triggerTimeWarp: () => boolean;
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
  const [comboEnergy, setComboEnergy] = useState<number>(0);
  const [upgrades, setUpgrades] = useState<ShopUpgrade[]>(INITIAL_UPGRADES);
  const [systems, setSystems] = useState<StudioSystem[]>(INITIAL_SYSTEMS);
  const [dailyDigestClaims, setDailyDigestClaims] = useState<number>(0);
  const [timeWarpCooldown, setTimeWarpCooldown] = useState<number>(0);

  const isInFlow = comboEnergy >= 1.0;

  // Расчет множителей и доходов
  const globalMultiplier = 1.0 + (prestigeCount * 0.25) + 
    upgrades.reduce((acc, u) => acc + (u.level * u.multiplierBonus), 0) +
    (systems.find(s => s.id === 'sys_cathaven')?.level || 0) * 0.05 +
    (systems.find(s => s.id === 'sys_realestate')?.level || 0) * 0.5;

  const flowMultiplier = isInFlow ? 3.0 : 1.0;

  const baseCpc = 1 + upgrades.reduce((acc, u) => acc + (u.level * u.codePerClickBonus), 0);
  const codePerClick = baseCpc * globalMultiplier * flowMultiplier;

  const baseCps = upgrades.reduce((acc, u) => acc + (u.level * u.codePerSecBonus), 0);
  const codePerSec = baseCps * globalMultiplier * flowMultiplier;

  const baseMps = upgrades.reduce((acc, u) => acc + (u.level * u.moneyPerSecBonus), 0);
  const moneyPerSec = baseMps * globalMultiplier * flowMultiplier;

  // Загрузка сохранения из LocalStorage
  useEffect(() => {
    try {
      const raw = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (raw) {
        const data: GameSaveData = JSON.parse(raw);
        if (data.codeLines) setCodeLines(data.codeLines);
        if (data.money) setMoney(data.money);
        if (data.totalCodeEver) setTotalCodeEver(data.totalCodeEver);
        if (data.prestigeCount) setPrestigeCount(data.prestigeCount);
        if (data.dailyDigestClaims) setDailyDigestClaims(data.dailyDigestClaims);
        if (data.timeWarpCooldown) setTimeWarpCooldown(data.timeWarpCooldown);

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

        // Оффлайн доход
        if (data.lastSeenTime) {
          const offlineSec = Math.min((Date.now() - data.lastSeenTime) / 1000, 43200); // макс 12 часов
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

  // Сохранение в LocalStorage
  useEffect(() => {
    const save = () => {
      const data: GameSaveData = {
        game: "HelloTap",
        version: "2.0.0",
        timestamp: new Date().toISOString(),
        codeLines,
        money,
        totalCodeEver,
        prestigeCount,
        prestigeTokens: 0,
        upgrades: upgrades.reduce((acc, u) => ({ ...acc, [u.id]: u.level }), {}),
        systems: systems.reduce((acc, s) => ({ ...acc, [s.id]: s.level }), {}),
        lastSeenTime: Date.now(),
        timeWarpCooldown,
        dailyDigestClaims
      };
      localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(data));
    };

    const interval = setInterval(save, 5000);
    window.addEventListener("beforeunload", save);
    return () => {
      clearInterval(interval);
      window.removeEventListener("beforeunload", save);
    };
  }, [codeLines, money, totalCodeEver, prestigeCount, upgrades, systems, timeWarpCooldown, dailyDigestClaims]);

  // Основной игровой цикл (10 тиков в секунду)
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

      // Спад комбо
      setComboEnergy(energy => {
        if (energy <= 0) return 0;
        const decayRate = energy >= 1.0 ? 0.08 : 0.04;
        return Math.max(0, energy - decayRate * dt);
      });
    }, 100);

    return () => clearInterval(interval);
  }, [codePerSec, moneyPerSec]);

  // Клик игрока
  const handleClick = useCallback((_clientX?: number, _clientY?: number) => {
    const isCrit = Math.random() < 0.12;
    const critMult = isCrit ? 4.0 : 1.0;
    const codeAdded = codePerClick * critMult;
    const moneyAdded = Math.max(0.5, codeAdded * 0.35);

    setCodeLines(c => c + codeAdded);
    setTotalCodeEver(t => t + codeAdded);
    setMoney(m => m + moneyAdded);

    // Добавляем энергию комбо (+6% за обычный клик, +15% за крит)
    setComboEnergy(e => Math.min(1.0, e + (isCrit ? 0.15 : 0.06)));

    sounds.playKeyClick(isCrit);
    sounds.triggerHaptic(isCrit ? 'heavy' : 'light');

    return { isCrit, codeAdded, moneyAdded };
  }, [codePerClick]);

  // Покупка улучшения
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

  // Прокачка системы студии
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
    const bonusCode = 45000 * globalMultiplier;
    const bonusMoney = 75000 * globalMultiplier;

    setCodeLines(c => c + bonusCode);
    setMoney(m => m + bonusMoney);
    setComboEnergy(1.0); // 100% комбо В Потоке
    setDailyDigestClaims(d => d + 1);

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return { bonusCode, bonusMoney };
  }, [globalMultiplier]);

  // Time Warp (2 часа)
  const triggerTimeWarp = useCallback((): boolean => {
    const now = Date.now();
    if (now < timeWarpCooldown) return false;

    const simulatedCode = Math.max(25000 * globalMultiplier, codePerSec * 7200 * 0.7);
    const simulatedMoney = Math.max(45000 * globalMultiplier, moneyPerSec * 7200 * 0.7);

    setCodeLines(c => c + simulatedCode);
    setMoney(m => m + simulatedMoney);
    setComboEnergy(1.0);
    setTimeWarpCooldown(now + 1800 * 1000); // 30 минут кулдаун

    sounds.playRelease();
    sounds.triggerHaptic('success');
    return true;
  }, [timeWarpCooldown, globalMultiplier, codePerSec, moneyPerSec]);

  // Экспорт сохранения в Base64
  const exportSaveBase64 = useCallback((): string => {
    const data: GameSaveData = {
      game: "HelloTap",
      version: "2.0.0",
      timestamp: new Date().toISOString(),
      codeLines,
      money,
      totalCodeEver,
      prestigeCount,
      prestigeTokens: 0,
      upgrades: upgrades.reduce((acc, u) => ({ ...acc, [u.id]: u.level }), {}),
      systems: systems.reduce((acc, s) => ({ ...acc, [s.id]: s.level }), {}),
      lastSeenTime: Date.now(),
      timeWarpCooldown,
      dailyDigestClaims
    };
    const json = JSON.stringify(data);
    return "HELLOTAP_SAVE_V2:" + btoa(unescape(encodeURIComponent(json)));
  }, [codeLines, money, totalCodeEver, prestigeCount, upgrades, systems, timeWarpCooldown, dailyDigestClaims]);

  // Импорт сохранения из Base64
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

  return (
    <GameContext.Provider
      value={{
        codeLines,
        money,
        totalCodeEver,
        prestigeCount,
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
        handleClick,
        buyUpgrade,
        upgradeSystem,
        claimDailyDigest,
        triggerTimeWarp,
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
