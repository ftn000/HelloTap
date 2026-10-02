import { SkillNode } from '../types/skills';

export const SKILL_NODES: SkillNode[] = [
  // ВЕТКА: FRONTEND & UX
  {
    id: 'skill_clean_code',
    name: 'Чистая архитектура',
    branch: 'frontend',
    icon: '✨',
    description: 'Изящный код без запахов: повышает шанс критического клика на +3%',
    maxLevel: 5,
    costPerLevel: 1,
    effectType: 'crit_chance',
    valuePerLevel: 0.03
  },
  {
    id: 'skill_hot_reload',
    name: 'Vite Fast HMR',
    branch: 'frontend',
    icon: '⚡',
    description: 'Мгновенная горячая замена модулей: комбо Flow State копится на +25% быстрее',
    maxLevel: 3,
    costPerLevel: 1,
    effectType: 'flow_gain',
    valuePerLevel: 0.25,
    reqSkillId: 'skill_clean_code'
  },
  {
    id: 'skill_pixel_perfect',
    name: 'Pixel Perfect Eye',
    branch: 'frontend',
    icon: '🎯',
    description: 'Точность верстки до субпикселя: увеличивает множитель крита на +1.0x',
    maxLevel: 4,
    costPerLevel: 2,
    effectType: 'crit_power',
    valuePerLevel: 1.0,
    reqSkillId: 'skill_clean_code'
  },
  {
    id: 'skill_wasm_speed',
    name: 'WebAssembly Core',
    branch: 'frontend',
    icon: '🚀',
    description: 'Бинарная скорость нативного кода: криты наносят еще на +1.5x больше кода',
    maxLevel: 3,
    costPerLevel: 3,
    effectType: 'crit_power',
    valuePerLevel: 1.5,
    reqSkillId: 'skill_pixel_perfect'
  },

  // ВЕТКА: BACKEND & DEVOPS
  {
    id: 'skill_async_io',
    name: 'Асинхронный Event Loop',
    branch: 'backend',
    icon: '🔄',
    description: 'Неблокирующий ввод-вывод: весь пассивный доход C#/сек вырастает на +20%',
    maxLevel: 5,
    costPerLevel: 1,
    effectType: 'passive_boost',
    valuePerLevel: 0.20
  },
  {
    id: 'skill_redis_cache',
    name: 'Redis In-Memory Cache',
    branch: 'backend',
    icon: '⚡',
    description: 'Кэш в оперативной памяти: сокращает перезарядку Time Warp на 15%',
    maxLevel: 3,
    costPerLevel: 2,
    effectType: 'timewarp_cdr',
    valuePerLevel: 0.15,
    reqSkillId: 'skill_async_io'
  },
  {
    id: 'skill_k8s_autoscaling',
    name: 'Kubernetes Автоскейлинг',
    branch: 'backend',
    icon: '☸️',
    description: 'Оркестрация микросервисов: буст пассивного кода студии еще на +35%',
    maxLevel: 4,
    costPerLevel: 2,
    effectType: 'passive_boost',
    valuePerLevel: 0.35,
    reqSkillId: 'skill_async_io'
  },
  {
    id: 'skill_quantum_threads',
    name: 'Квантовая многопоточность',
    branch: 'backend',
    icon: '⚛️',
    description: 'Суперпозиция потоков: сверх-буст пассивного кода C#/сек на +50%',
    maxLevel: 3,
    costPerLevel: 3,
    effectType: 'passive_boost',
    valuePerLevel: 0.50,
    reqSkillId: 'skill_k8s_autoscaling'
  },

  // ВЕТКА: STARTUP MANAGEMENT & BUSINESS
  {
    id: 'skill_negotiation',
    name: 'Искусство питчинга',
    branch: 'business',
    icon: '🤝',
    description: 'Договоренности с вендорами: скидка на все улучшения в магазине 5%',
    maxLevel: 5,
    costPerLevel: 1,
    effectType: 'shop_discount',
    valuePerLevel: 0.05
  },
  {
    id: 'skill_bug_bounty_hunter',
    name: 'Bug Bounty Охотник',
    branch: 'business',
    icon: '🐞',
    description: 'Выплаты за уязвимости: за поимку багов в коде дается на +50% больше строк',
    maxLevel: 3,
    costPerLevel: 1,
    effectType: 'bug_reward_boost',
    valuePerLevel: 0.50,
    reqSkillId: 'skill_negotiation'
  },
  {
    id: 'skill_venture_network',
    name: 'Венчурные связи',
    branch: 'business',
    icon: '💼',
    description: 'Инвестиции бизнес-ангелов: постоянный бонус +25% ко всем доходам в рублях ₽',
    maxLevel: 4,
    costPerLevel: 2,
    effectType: 'money_boost',
    valuePerLevel: 0.25,
    reqSkillId: 'skill_negotiation'
  },
  {
    id: 'skill_unicorn_status',
    name: 'Статус IT-Единорога',
    branch: 'business',
    icon: '🦄',
    description: 'Оценка стартапа в $1 млрд: еще -8% скидки на оборудование и +40% к деньгам',
    maxLevel: 3,
    costPerLevel: 3,
    effectType: 'money_boost',
    valuePerLevel: 0.40,
    reqSkillId: 'skill_venture_network'
  },

  // ВЕТКА: AI & НЕЙРОСЕТИ
  {
    id: 'skill_prompt_engineering',
    name: 'Промпт-инжиниринг LLM',
    branch: 'ai',
    icon: '💬',
    description: 'Ювелирные промпты: нейросети ускоряют генерацию кода за клик на +20%',
    maxLevel: 5,
    costPerLevel: 1,
    effectType: 'click_boost',
    valuePerLevel: 0.20
  },
  {
    id: 'skill_ai_agents',
    name: 'Мультиагентный рой',
    branch: 'ai',
    icon: '🤖',
    description: 'Сотни микроагентов автономно пишут фичи: пассивный доход C#/сек +40%',
    maxLevel: 4,
    costPerLevel: 2,
    effectType: 'passive_boost',
    valuePerLevel: 0.40,
    reqSkillId: 'skill_prompt_engineering'
  },
  {
    id: 'skill_fine_tuning',
    name: 'LoRA & Файн-тюнинг весов',
    branch: 'ai',
    icon: '🧠',
    description: 'Кастомные веса модели: шанс критического клика +5% и множитель +2.0x',
    maxLevel: 3,
    costPerLevel: 3,
    effectType: 'crit_chance',
    valuePerLevel: 0.05,
    reqSkillId: 'skill_ai_agents'
  },
  {
    id: 'skill_agi_singularity',
    name: 'AGI Сингулярность',
    branch: 'ai',
    icon: '🌟',
    description: 'Искусственный общий интеллект: грандиозный буст всего прогресса студии x2.5!',
    maxLevel: 2,
    costPerLevel: 5,
    effectType: 'singularity',
    valuePerLevel: 1.5,
    reqSkillId: 'skill_fine_tuning'
  },

  // ВЕТКА: DEVOPS & CI/CD
  {
    id: 'skill_gitops',
    name: 'GitOps & Trunk-Based Dev',
    branch: 'devops',
    icon: '🔀',
    description: 'Непрерывная интеграция: слияние Pull Request приносит на +50% больше денег ₽ и кода',
    maxLevel: 5,
    costPerLevel: 1,
    effectType: 'pr_boost',
    valuePerLevel: 0.50
  },
  {
    id: 'skill_github_actions',
    name: 'GitHub Actions Matrix',
    branch: 'devops',
    icon: '⚙️',
    description: 'Автоматизированный CI/CD: ускоряет пайплайн тестов и увеличивает награду за Broken Build на +50%',
    maxLevel: 4,
    costPerLevel: 2,
    effectType: 'pipeline_haste',
    valuePerLevel: 0.50,
    reqSkillId: 'skill_gitops'
  },
  {
    id: 'skill_blitz_compiler',
    name: 'LLVM JIT-Оптимизатор',
    branch: 'devops',
    icon: '⚡',
    description: 'Векторизация машинного кода: продлевает действие 10x Refactor Blitz на +5 секунд за уровень',
    maxLevel: 3,
    costPerLevel: 2,
    effectType: 'blitz_duration',
    valuePerLevel: 5,
    reqSkillId: 'skill_gitops'
  },
  {
    id: 'skill_auto_deploy',
    name: 'Zero-Downtime Blue-Green',
    branch: 'devops',
    icon: '🚀',
    description: 'Бесшовный автодеплой без простоя: пассивный доход +30% и дивиденд при каждом успешном билде',
    maxLevel: 3,
    costPerLevel: 3,
    effectType: 'passive_boost',
    valuePerLevel: 0.30,
    reqSkillId: 'skill_github_actions'
  }
];
