export type SkillBranch = 'frontend' | 'backend' | 'business' | 'ai' | 'devops';

export interface SkillNode {
  id: string;
  name: string;
  nameRu?: string;
  nameEn?: string;
  nameTr?: string;
  branch: SkillBranch;
  icon: string;
  description: string;
  descriptionRu?: string;
  descriptionEn?: string;
  descriptionTr?: string;
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
    | 'bug_reward_boost'   // +% к награде за дебаггинг багов
    | 'click_boost'        // +% к коду за клик
    | 'singularity'        // глобальный множитель ко всему
    | 'pr_boost'           // +% к награде за слияние PR
    | 'blitz_duration'     // +сек к длительности Refactor Blitz
    | 'pipeline_haste';    // ускорение CI/CD и награда за Broken Build fix
  valuePerLevel: number;
  reqSkillId?: string;     // Требуемый предыдущий навык в ветке
}
