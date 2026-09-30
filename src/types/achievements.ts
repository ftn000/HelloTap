export type AchievementCategory = 'clicks' | 'code' | 'economy' | 'upgrades' | 'systems' | 'prestige' | 'special';

export type AchievementTier = 0 | 1 | 2 | 3;

export interface AchievementTierDef {
  tier: 1 | 2 | 3;
  target: number;
  rewardDescRu: string;
  rewardDescEn: string;
  bonusMultiplier: number;
}

export interface AchievementDef {
  id: string;
  icon: string;
  category: AchievementCategory;
  titleRu: string;
  titleEn: string;
  descRu: string;
  descEn: string;
  tiers: [AchievementTierDef, AchievementTierDef, AchievementTierDef];
  statKey: string;
}

export interface UnlockedAchievementData {
  tier: AchievementTier; // 0 = not started, 1 = star 1, 2 = star 2, 3 = maxed (completed)
  unlockedAt?: string;
}
