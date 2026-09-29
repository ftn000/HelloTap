export interface ShopUpgrade {
  id: number;
  name: string;
  category: 'click' | 'idle' | 'synergy';
  icon: string;
  description: string;
  level: number;
  maxLevel: number;
  baseCostCode: number;
  baseCostMoney: number;
  costMultiplier: number;
  codePerClickBonus: number;
  codePerSecBonus: number;
  moneyPerSecBonus: number;
  multiplierBonus: number;
}

export interface StudioSystem {
  id: string;
  title: string;
  icon: string;
  category: 'office' | 'business' | 'tech' | 'culture';
  description: string;
  level: number;
  maxLevel: number;
  reqCode: number;
  bonusDesc: string;
}

export interface GameSaveData {
  game: string;
  version: string;
  timestamp: string;
  codeLines: number;
  money: number;
  totalCodeEver: number;
  prestigeCount: number;
  prestigeTokens: number;
  upgrades: Record<number, number>;
  systems: Record<string, number>;
  lastSeenTime: number;
  timeWarpCooldown: number;
  dailyDigestClaims: number;
}
