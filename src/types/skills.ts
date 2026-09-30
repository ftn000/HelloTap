export type SkillBranch = 'frontend' | 'backend' | 'business';

export interface SkillNode {
  id: string;
  name: string;
  branch: SkillBranch;
  icon: string;
  description: string;
  maxLevel: number;
  costPerLevel: number;
  effectType: 
    | 'crit_chance'        // +% к шансу критического клика
    | 'crit_power'         // + к множителю критического удара
    | 'flow_gain'          // ускоренное накопление шкалы Flow
    | 'passive_boost'      // +% ко всему пассивному доходу C#
    | 'timewarp_cdr'       // -% к кулдауну Time Warp
    | 'shop_discount'      // -% к стоимости улучшений
    | 'money_boost'        // +% к заработку рублей ₽
    | 'bug_reward_boost';  // +% к награде за дебаггинг багов
  valuePerLevel: number;
  reqSkillId?: string;     // Требуемый предыдущий навык в ветке
}
