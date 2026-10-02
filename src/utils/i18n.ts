export type Language = 'ru' | 'en';

export interface TranslationDictionary {
  // HUD & Header
  codePerSec: string;
  moneyPerSec: string;
  codePerClick: string;
  flowMode: string;
  hubBtn: string;
  digestBtn: string;
  saveBtn: string;
  adActive: string;
  soundBtn: string;
  musicTurnOn: string;
  musicNowPlaying: string;
  musicNextTrack: string;
  techTreeBtn: string;
  achievementsBtn: string;
  heatmapBtn: string;
  codePerSecShort: string;
  moneyPerSecShort: string;
  secShort: string;

  // Clicker & IDE
  terminalFile: string;
  compileBtn: string;
  clickInstruction: string;
  critText: string;
  focusBar: string;
  locPerClick: string;
  locPerSec: string;
  rubPerSec: string;
  fastSwitchHint: string;
  fileOpenedLog: string;
  bugFixedToast: string;
  bugButton: string;
  refactorBlitzBanner: string;
  refactorBlitzReward: string;
  quickFixTooltip: string;
  quickFixButton: string;
  quickFixInline: string;
  quickFixTooltipAttr: string;
  mergePrTooltip: string;
  createBranchTooltip: string;
  cmdPaletteTooltip: string;
  heatmapTooltip: string;
  linterErrorTooltip: string;
  switchesHeader: string;
  themePreset: string;
  themeBadge: string;

  // Upgrades
  upgradesTitle: string;
  buyBtn: string;
  maxBtn: string;
  lvlPrefix: string;

  // Hub Tabs
  tabSystems: string;
  tabShop: string;
  tabBoosts: string;
  tabIPO: string;
  tabSwitches: string;
  tabSaves: string;

  // Studio Decor
  catTitle: string;
  catPurr: string;
  catName: string;
  catSleeping: string;
  coffeeTooltip: string;

  // Studio OS Hub Modal
  hubHeader: string;
  importSuccess: string;
  importError: string;
  digestClaimsCount: string;
  ipoTokensGain: string;
  ipoTokensInPortfolio: string;
  sfxTitle: string;
  sfxVolumeLabel: string;
  sfxTestClick: string;
  sfxProfilesLabel: string;
  sfxBellLabel: string;
  sfxBellCrit: string;
  sfxBellFlow: string;
  musicSectionTitle: string;
  musicSectionDesc: string;
  musicVolumeLabel: string;
  musicPauseBtn: string;
  musicPlayBtn: string;
  musicNextBtn: string;
  themesSectionTitle: string;
  themesSectionDesc: string;
  themeSoundLabel: string;

  // CI/CD Pipeline
  brokenBuildMessage: string;
  brokenBuildFixTooltip: string;
  runPipelineTooltip: string;
  fixBrokenStepTooltip: string;

  // Command Palette
  cmdPlaceholder: string;
  cmdNotFound: string;
  navHint: string;
  selectHint: string;
  closeHint: string;

  // GitHub Heatmap
  rankLabel: string;
  totalContributionsLabel: string;
  streakLabel: string;
  streakDays: string;
  pipelinesLabel: string;
  commitBtn: string;
  hoverDetails: string;
  contributionsCount: string;
  contributionsTitle: string;
  legendLess: string;
  legendMore: string;

  // Offline Progress Modal
  offlineWelcome: string;
  offlineAwayDesc: string;
  offlineCodeLabel: string;
  offlineRevenueLabel: string;
  offlineDoubleBtn: string;
  offlineClaimBtn: string;

  // Random Events Modal
  eventHint: string;
  eventSkip: string;

  // Tech Tree Modal
  techTreeTitle: string;
  techTreeSubtitle: string;
  pointsCount: string;
  allBranches: string;
  resetSkillsBtn: string;
  resetSkillsConfirm: string;
  resetSkillsTooltip: string;
  reqBaseSkill: string;
  maxedSkill: string;
  upgradeSkill: string;
  levelPrefix: string;

  // Achievements Modal
  tierPrefix: string;

  // VIP Shop
  vipStoreTitle: string;
  vipStoreDesc: string;
  vipActivePerks: string;
  vipPerkMultiplier: string;
  vipPerkAutoclicker: string;
  vipPerkNoAds: string;
  buyForYans: string;
  alreadyOwned: string;
  permanentBadge: string;
  consumableBadge: string;
  purchaseSuccess: string;
  purchaseFailed: string;
  noAdsActiveBadge: string;

  // Categories
  catAll: string;
  catOffice: string;
  catBusiness: string;
  catTech: string;
  catCulture: string;

  // Yandex Ads
  yandexBonusTitle: string;
  yandexBonusDesc: string;
  yandexAdDouble: string;
  yandexAdDoubleDesc: string;
  yandexAdTimeWarp: string;
  yandexAdTimeWarpDesc: string;

  // Digest & Time Warp
  digestTitle: string;
  digestDesc: string;
  digestClaimBtn: string;
  timeWarpTitle: string;
  timeWarpDesc: string;
  timeWarpBtn: string;
  timeWarpCharging: string;

  // IPO
  ipoTitle: string;
  ipoDesc: string;
  ipoShares: string;
  ipoWillGain: string;
  ipoBtn: string;
  ipoNotEnough: string;

  // Switches
  switchTitle: string;
  switchDesc: string;

  // Saves
  saveExportTitle: string;
  saveExportDesc: string;
  saveCopyBtn: string;
  saveCopied: string;
  saveImportTitle: string;
  saveImportPlaceholder: string;
  saveImportBtn: string;
  saveDangerZone: string;
  saveResetBtn: string;
  saveResetConfirm: string;

  // Leaderboard & HUD
  leaderboardBtn: string;
  leaderboardTitle: string;
  leaderboardDesc: string;
  leaderboardRank: string;
  leaderboardPlayer: string;
  leaderboardScore: string;
  leaderboardYouBadge: string;
  leaderboardLoading: string;
  autoClickerActive: string;
}

export const TRANSLATIONS: Record<Language, TranslationDictionary> = {
  ru: {
    // HUD & Header
    codePerSec: "сек",
    moneyPerSec: "сек",
    codePerClick: "C#/клик",
    flowMode: "РЕЖИМ В ПОТОКЕ (x3.0 МНОЖИТЕЛЬ)!",
    hubBtn: "Hub Студии",
    digestBtn: "Ежедневный Дайджест",
    saveBtn: "Облако и Сохранения",
    adActive: "x2 АКТИВЕН",
    soundBtn: "Звук",
    musicTurnOn: "Включить Lo-Fi Synthwave Музыку",
    musicNowPlaying: "Сейчас играет: {0} (Клик для паузы)",
    musicNextTrack: "След. трек ({0})",
    techTreeBtn: "Дерево IT-навыков и талантов [K]",
    achievementsBtn: "Достижения и Награды [A]",
    heatmapBtn: "GitHub Contribution Heatmap (График активности)",
    codePerSecShort: "/сек",
    moneyPerSecShort: "₽/сек",
    secShort: "с",

    // Clicker & IDE
    terminalFile: "StudioEditor.cs",
    compileBtn: "⌨️ КОМПИЛИРОВАТЬ КОД",
    clickInstruction: "// Нажимайте для компиляции и генерации дохода",
    critText: "🔥 КРИТ",
    focusBar: "Шкала фокуса разработчика",
    locPerClick: "C#/кл",
    locPerSec: "C#/сек",
    rubPerSec: "₽/сек",
    fastSwitchHint: "Быстрое переключение по Tab или Ctrl+P",
    fileOpenedLog: "✓ [{0}] Открыт: {1} ({2})",
    bugFixedToast: "🐛 Баг устранен! +{0} C# [OVERCLOCK x3.0]",
    bugButton: "ОТЛАДИТЬ! ({0}с)",
    refactorBlitzBanner: "⚡ REFACTOR BLITZ! Кликните 5 строк: ({0}/5)",
    refactorBlitzReward: "Награда: 10x Boost на 20 секунд!",
    quickFixTooltip: "— Кликните для Quick Fix!",
    quickFixButton: "💡 Quick Fix (+бонус)",
    quickFixInline: "💡 Fix ({0}с)",
    quickFixTooltipAttr: "Кликните для автоисправления ошибки",
    mergePrTooltip: "Слить Pull Request в main и получить награду",
    createBranchTooltip: "Создать feature-ветку (+25% к коду)",
    cmdPaletteTooltip: "Открыть командную строку VS Code (Ctrl+Shift+P / F1)",
    heatmapTooltip: "Открыть GitHub Contribution Heatmap",
    linterErrorTooltip: "Синтаксическая ошибка: кликните для Quick Fix",
    switchesHeader: "Свитчи:",
    themePreset: "(Пресет темы)",
    themeBadge: "Тема",

    // Upgrades
    upgradesTitle: "Оборудование и Улучшения",
    buyBtn: "КУПИТЬ",
    maxBtn: "МАКС",
    lvlPrefix: "ур.",

    // Hub Tabs
    tabSystems: "🏢 Системы",
    tabShop: "💎 Донат",
    tabBoosts: "⚡ Бусты & Реклама",
    tabIPO: "📈 IPO",
    tabSwitches: "⌨️ Свитчи",
    tabSaves: "💾 Сейвы",

    // Studio Decor
    catTitle: "Офисный кот-талисман (кликни погладить!)",
    catPurr: "Мурр! ❤️",
    catName: "Барсик",
    catSleeping: "Котик спит (прокачай приют в Hub)",
    coffeeTooltip: "Кофе программиста",

    // Studio OS Hub Modal
    hubHeader: "STUDIO OS — СИСТЕМЫ И ПРЕСТИЖ",
    importSuccess: "✓ Прогресс успешно загружен!",
    importError: "❌ Ошибка: неверный формат ключа сохранения!",
    digestClaimsCount: "Сборов: {0}",
    ipoTokensGain: "+{0} Токенов",
    ipoTokensInPortfolio: "{0} шт. (+{1}% буст)",
    sfxTitle: "Громкость SFX & Профили звука",
    sfxVolumeLabel: "Громкость кликов и эффектов интерфейса:",
    sfxTestClick: "Тест клика 🔊",
    sfxProfilesLabel: "Переключатель профилей звука:",
    sfxBellLabel: "Стеклянный колокольчик при критах и Flow:",
    sfxBellCrit: "🔔 Крит",
    sfxBellFlow: "✨ Flow",
    musicSectionTitle: "Фоновая Lo-Fi музыка (Coder Beats)",
    musicSectionDesc: "Генеративный Lo-Fi синт-фон для глубокого кодинга",
    musicVolumeLabel: "Громкость музыки:",
    musicPauseBtn: "Пауза",
    musicPlayBtn: "Слушать",
    musicNextBtn: "Следующий трек",
    themesSectionTitle: "Темы IDE и Терминала",
    themesSectionDesc: "Цветовые схемы кода, подсветка синтаксиса и неоновое свечение рабочей среды.",
    themeSoundLabel: "Звук",

    // CI/CD Pipeline
    brokenBuildMessage: "❌ Broken Build: Pipeline #{0} упал на [{1}] — кликните для починки!",
    brokenBuildFixTooltip: "Устранить аварию CI/CD и получить награду за хотфикс",
    runPipelineTooltip: "Запустить ручной прогон CI/CD пайплайна",
    fixBrokenStepTooltip: "Кликните здесь, чтобы устранить сбой сборки!",

    // Command Palette
    cmdPlaceholder: "Введите команду или действие... (например, branch, merge, blitz, theme)",
    cmdNotFound: "Команда не найдена. Попробуйте другой запрос.",
    navHint: "↑↓ Навигация",
    selectHint: "↵ Выбор",
    closeHint: "ESC Закрыть",

    // GitHub Heatmap
    rankLabel: "Ранг разработчика",
    totalContributionsLabel: "Всего вкладов (365д)",
    streakLabel: "Активный стрик дней",
    streakDays: "{0} дн. (макс: {1})",
    pipelinesLabel: "CI/CD Пайплайнов",
    commitBtn: "Совершить коммит активности (+1 вклад)",
    hoverDetails: "Наведите на ячейку для просмотра деталей",
    contributionsCount: "{0} вкладов в код",
    contributionsTitle: "{0}: {1} контрибуций",
    legendLess: "Меньше",
    legendMore: "Больше",

    // Offline Progress Modal
    offlineWelcome: "С ВОЗВРАЩЕНИЕМ В ОФИС!",
    offlineAwayDesc: "Пока вас не было ({0}), серверы студии продолжали компилировать код!",
    offlineCodeLabel: "Код C#",
    offlineRevenueLabel: "Выручка",
    offlineDoubleBtn: "УДВОИТЬ БОНУС (x2)",
    offlineClaimBtn: "Забрать обычный бонус",

    // Random Events Modal
    eventHint: "Быстрый выбор дает преимущество студии",
    eventSkip: "Пропустить",

    // Tech Tree Modal
    techTreeTitle: "ДЕРЕВО IT-НАВЫКОВ И ТАЛАНТОВ",
    techTreeSubtitle: "Очки талантов начисляются при выходе на IPO и закрытии ачивок",
    pointsCount: "Очков: {0}",
    allBranches: "Все ветки",
    resetSkillsBtn: "Сброс",
    resetSkillsConfirm: "Сбросить все вложенные очки талантов и вернуть их на баланс?",
    resetSkillsTooltip: "Сбросить все навыки и вернуть очки",
    reqBaseSkill: "🔒 Требуется открыть базовый навык ветки",
    maxedSkill: "МАКСИМУМ",
    upgradeSkill: "Прокачать ({0} очк.)",
    levelPrefix: "Уровень",

    // Achievements Modal
    tierPrefix: "Ур.",

    // VIP Shop
    vipStoreTitle: "VIP МАРКЕТПЛЕЙС (ЯНДЕКС ИГРЫ)",
    vipStoreDesc: "Покупки за игровую валюту Ян (Yandex In-App Purchases). Вечные улучшения сохраняются навсегда!",
    vipActivePerks: "АКТИВНЫЕ ПРИВИЛЕГИИ:",
    vipPerkMultiplier: "👑 Вечный x2 множитель дохода",
    vipPerkAutoclicker: "⚡ Авто-кликер Bot Pro (10 CPS)",
    vipPerkNoAds: "🚫 Реклама отключена (бонусы мгновенно)",
    buyForYans: "КУПИТЬ ЗА {0} ЯН",
    alreadyOwned: "КУПЛЕНО ✓",
    permanentBadge: "ВЕЧНО",
    consumableBadge: "МГНОВЕННО",
    purchaseSuccess: "🎉 Покупка успешно совершена!",
    purchaseFailed: "❌ Ошибка покупки или действие отменено",
    noAdsActiveBadge: "👑 VIP NO-ADS: РЕКЛАМА ОТКЛЮЧЕНА (НАГРАДА МГНОВЕННО)",

    // Categories
    catAll: "Все",
    catOffice: "Офис",
    catBusiness: "Бизнес",
    catTech: "Технологии",
    catCulture: "Культура",

    // Yandex Ads
    yandexBonusTitle: "БОНУСЫ ЯНДЕКС ИГР",
    yandexBonusDesc: "Просмотр короткого ролика за супер-буст",
    yandexAdDouble: "📺 x2 ДОХОД НА 3 МИНУТЫ",
    yandexAdDoubleDesc: "Удваивает весь C# и рубли",
    yandexAdTimeWarp: "📺 СБРОСИТЬ TIME WARP",
    yandexAdTimeWarpDesc: "Мгновенная зарядка 2h варпа",

    // Digest & Time Warp
    digestTitle: "УТРЕННИЙ ДАЙДЖЕСТ",
    digestDesc: "Сводный операционный центр и автоматический сбор наград",
    digestClaimBtn: "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ",
    timeWarpTitle: "TIME WARP: СИМУЛЯТОР СМЕНЫ",
    timeWarpDesc: "Мгновенная автономная выработка за 2 часа",
    timeWarpBtn: "⚡ ЗАПУСТИТЬ TIME WARP (2 ЧАСА)",
    timeWarpCharging: "⏳ ЗАРЯДКА",

    // IPO
    ipoTitle: "ВЫХОД НА IPO (ПРЕСТИЖ)",
    ipoDesc: "Продайте акции компании инвесторам на бирже. Все накопленные деньги, команда и купленное железо сохраняются! Вы получаете инвестиционный грант, Токены Акций и постоянный множитель x1.5 на все будущие сессии!",
    ipoShares: "Акции в портфеле:",
    ipoWillGain: "Будет начислено при IPO:",
    ipoBtn: "🚀 ПРОВЕСТИ IPO",
    ipoNotEnough: "ТРЕБУЕТСЯ БОЛЬШЕ КОДА ДЛЯ IPO",

    // Switches
    switchTitle: "МЕХАНИЧЕСКИЕ ПЕРЕКЛЮЧАТЕЛИ КЛАВИАТУРЫ",
    switchDesc: "Выберите тип механических свитчей для изменения звукового профиля синтезатора:",

    // Saves
    saveExportTitle: "ЭКСПОРТ СОХРАНЕНИЯ (BASE64)",
    saveExportDesc: "Скопируйте ключ сохранения для переноса прогресса между браузерами и устройствами:",
    saveCopyBtn: "КОПИРОВАТЬ КЛЮЧ СОХРАНЕНИЯ",
    saveCopied: "СКОПИРОВАНО В БУФЕР!",
    saveImportTitle: "ИМПОРТ СОХРАНЕНИЯ",
    saveImportPlaceholder: "Вставьте код HELLOTAP_SAVE_V2:...",
    saveImportBtn: "ЗАГРУЗИТЬ ПРОГРЕСС",
    saveDangerZone: "ОПАСНАЯ ЗОНА",
    saveResetBtn: "⚠️ СБРОСИТЬ ВЕСЬ ПРОГРЕСС",
    saveResetConfirm: "❓ ТОЧНО СБРОСИТЬ? НАЖМИТЕ ЕЩЁ РАЗ",

    // Leaderboard & HUD
    leaderboardBtn: "Рейтинг",
    leaderboardTitle: "ТОП РАЗРАБОТЧИКОВ (РЕЙТИНГ)",
    leaderboardDesc: "Глобальный зал славы Яндекс Игр по количеству скомпилированных строк кода за все время",
    leaderboardRank: "#",
    leaderboardPlayer: "Разработчик",
    leaderboardScore: "Строк C#",
    leaderboardYouBadge: "ВЫ",
    leaderboardLoading: "Загрузка топа...",
    autoClickerActive: "⚡ АВТОКЛИКЕР (10 CPS)"
  },
  en: {
    // HUD & Header
    codePerSec: "sec",
    moneyPerSec: "sec",
    codePerClick: "C#/click",
    flowMode: "FLOW STATE ACTIVE (x3.0 MULTIPLIER)!",
    hubBtn: "Studio Hub",
    digestBtn: "Daily Digest",
    saveBtn: "Cloud & Saves",
    adActive: "x2 ACTIVE",
    soundBtn: "Sound",
    musicTurnOn: "Play Lo-Fi Ambient",
    musicNowPlaying: "Now playing: {0} (Click to pause)",
    musicNextTrack: "Next track ({0})",
    techTreeBtn: "IT Skills & Talent Tree [K]",
    achievementsBtn: "Achievements & Badges [A]",
    heatmapBtn: "GitHub Contribution Heatmap",
    codePerSecShort: "/sec",
    moneyPerSecShort: "₽/sec",
    secShort: "s",

    // Clicker & IDE
    terminalFile: "StudioEditor.cs",
    compileBtn: "⌨️ COMPILE CODE",
    clickInstruction: "// Tap or click to compile and earn revenue",
    critText: "🔥 CRIT",
    focusBar: "Developer Focus Bar",
    locPerClick: "C#/click",
    locPerSec: "C#/sec",
    rubPerSec: "₽/sec",
    fastSwitchHint: "Quick switch with Tab or Ctrl+P",
    fileOpenedLog: "✓ [{0}] Opened: {1} ({2})",
    bugFixedToast: "🐛 Bug resolved! +{0} C# [OVERCLOCK x3.0]",
    bugButton: "DEBUG! ({0}s)",
    refactorBlitzBanner: "⚡ REFACTOR BLITZ! Click 5 lines: ({0}/5)",
    refactorBlitzReward: "Reward: 10x Boost for 20 seconds!",
    quickFixTooltip: "— Click for Quick Fix!",
    quickFixButton: "💡 Quick Fix (+bonus)",
    quickFixInline: "💡 Fix ({0}s)",
    quickFixTooltipAttr: "Click to auto-fix code error",
    mergePrTooltip: "Merge Pull Request into main and claim bounty",
    createBranchTooltip: "Create feature branch (+25% code boost)",
    cmdPaletteTooltip: "Open VS Code Command Palette (Ctrl+Shift+P / F1)",
    heatmapTooltip: "Open GitHub Contribution Heatmap",
    linterErrorTooltip: "Syntax error: click for Quick Fix",
    switchesHeader: "Switches:",
    themePreset: "(Theme preset)",
    themeBadge: "Theme",

    // Upgrades
    upgradesTitle: "Hardware & Upgrades",
    buyBtn: "BUY",
    maxBtn: "MAX",
    lvlPrefix: "lvl",

    // Hub Tabs
    tabSystems: "🏢 Systems",
    tabShop: "💎 VIP Store",
    tabBoosts: "⚡ Boosts & Ads",
    tabIPO: "📈 IPO",
    tabSwitches: "⌨️ Switches",
    tabSaves: "💾 Saves",

    // Studio Decor
    catTitle: "Office Mascot Cat (Click to pet!)",
    catPurr: "Purr! ❤️",
    catName: "Pixel",
    catSleeping: "Kitty is asleep (upgrade Haven in Hub)",
    coffeeTooltip: "Developer's Coffee",

    // Studio OS Hub Modal
    hubHeader: "STUDIO OS — SYSTEMS & PRESTIGE",
    importSuccess: "✓ Progress loaded successfully!",
    importError: "❌ Error: invalid save key format!",
    digestClaimsCount: "Claims: {0}",
    ipoTokensGain: "+{0} Tokens",
    ipoTokensInPortfolio: "{0} pcs. (+{1}% boost)",
    sfxTitle: "SFX Volume & Sound Profiles",
    sfxVolumeLabel: "Click & UI sound effect volume:",
    sfxTestClick: "Test click 🔊",
    sfxProfilesLabel: "Sound profile selector:",
    sfxBellLabel: "Glass chime for crits & Flow:",
    sfxBellCrit: "🔔 Crit",
    sfxBellFlow: "✨ Flow",
    musicSectionTitle: "Background Lo-Fi Music (Coder Beats)",
    musicSectionDesc: "Generative Lo-Fi synth audio for deep coding",
    musicVolumeLabel: "Music volume:",
    musicPauseBtn: "Pause",
    musicPlayBtn: "Play",
    musicNextBtn: "Next track",
    themesSectionTitle: "IDE & Terminal Themes",
    themesSectionDesc: "Color palettes, syntax tokenization, and neon ambient glows.",
    themeSoundLabel: "Sound",

    // CI/CD Pipeline
    brokenBuildMessage: "❌ Broken Build: Pipeline #{0} failed at [{1}] — click to fix!",
    brokenBuildFixTooltip: "Fix CI/CD broken build and claim hotfix reward",
    runPipelineTooltip: "Trigger manual CI/CD pipeline run",
    fixBrokenStepTooltip: "Click here to fix the broken build step!",

    // Command Palette
    cmdPlaceholder: "Type a command or action... (e.g. branch, merge, blitz, theme)",
    cmdNotFound: "No commands found. Try another query.",
    navHint: "↑↓ Navigate",
    selectHint: "↵ Select",
    closeHint: "ESC Close",

    // GitHub Heatmap
    rankLabel: "Developer Rank",
    totalContributionsLabel: "Total Contributions (365d)",
    streakLabel: "Active Day Streak",
    streakDays: "{0} days (max: {1})",
    pipelinesLabel: "CI/CD Pipelines",
    commitBtn: "Commit Activity (+1 contribution)",
    hoverDetails: "Hover over a cell to view details",
    contributionsCount: "{0} code contributions",
    contributionsTitle: "{0}: {1} contributions",
    legendLess: "Less",
    legendMore: "More",

    // Offline Progress Modal
    offlineWelcome: "WELCOME BACK TO THE OFFICE!",
    offlineAwayDesc: "While you were away ({0}), studio servers kept compiling code!",
    offlineCodeLabel: "C# Code",
    offlineRevenueLabel: "Revenue",
    offlineDoubleBtn: "DOUBLE REWARD (x2)",
    offlineClaimBtn: "Claim Regular Reward",

    // Random Events Modal
    eventHint: "Quick decisions empower the studio",
    eventSkip: "Skip",

    // Tech Tree Modal
    techTreeTitle: "TECH SKILL TREE & TALENTS",
    techTreeSubtitle: "Talent points are awarded on IPO and unlocking achievements",
    pointsCount: "Points: {0}",
    allBranches: "All Branches",
    resetSkillsBtn: "Reset",
    resetSkillsConfirm: "Reset all spent talent points and refund them to your balance?",
    resetSkillsTooltip: "Reset all skills and refund points",
    reqBaseSkill: "🔒 Requires base branch skill",
    maxedSkill: "MAXED",
    upgradeSkill: "Upgrade ({0} pts)",
    levelPrefix: "Level",

    // Achievements Modal
    tierPrefix: "Tier",

    // VIP Shop
    vipStoreTitle: "VIP MARKETPLACE (YANDEX GAMES)",
    vipStoreDesc: "Purchases with Yan currency (Yandex In-App Purchases). Permanent upgrades stay with you forever!",
    vipActivePerks: "ACTIVE PRIVILEGES:",
    vipPerkMultiplier: "👑 Permanent x2 income multiplier",
    vipPerkAutoclicker: "⚡ Auto-Clicker Bot Pro (10 CPS)",
    vipPerkNoAds: "🚫 Ads disabled (instant bonus rewards)",
    buyForYans: "BUY FOR {0} YAN",
    alreadyOwned: "PURCHASED ✓",
    permanentBadge: "PERMANENT",
    consumableBadge: "INSTANT",
    purchaseSuccess: "🎉 Purchase completed successfully!",
    purchaseFailed: "❌ Purchase error or cancelled",
    noAdsActiveBadge: "👑 VIP NO-ADS: ADS SKIPPED (INSTANT REWARD)",

    // Categories
    catAll: "All",
    catOffice: "Office",
    catBusiness: "Business",
    catTech: "Tech",
    catCulture: "Culture",

    // Yandex Ads
    yandexBonusTitle: "YANDEX GAMES BOOSTS",
    yandexBonusDesc: "Watch a short ad for a studio super-boost",
    yandexAdDouble: "📺 x2 INCOME FOR 3 MIN",
    yandexAdDoubleDesc: "Doubles all C# and Money earnings",
    yandexAdTimeWarp: "📺 RESET TIME WARP",
    yandexAdTimeWarpDesc: "Instantly recharge 2-hour Time Warp",

    // Digest & Time Warp
    digestTitle: "DAILY DIGEST",
    digestDesc: "Studio operations hub and automated reward collection",
    digestClaimBtn: "💰 CLAIM ALL REWARDS & DIVIDENDS",
    timeWarpTitle: "TIME WARP: SHIFT SIMULATOR",
    timeWarpDesc: "Instant autonomous studio progress for 2 hours",
    timeWarpBtn: "⚡ LAUNCH TIME WARP (2 HOURS)",
    timeWarpCharging: "⏳ CHARGING",

    // IPO
    ipoTitle: "GO PUBLIC (IPO PRESTIGE)",
    ipoDesc: "Sell company shares to market investors. All money, studio gear, and team members are preserved! You gain an IPO cash grant, Stock Tokens and a permanent x1.5 multiplier for all future sessions!",
    ipoShares: "Portfolio shares:",
    ipoWillGain: "Tokens on IPO:",
    ipoBtn: "🚀 EXECUTE IPO",
    ipoNotEnough: "MORE CODE NEEDED FOR IPO",

    // Switches
    switchTitle: "MECHANICAL KEYBOARD SWITCHES",
    switchDesc: "Select switch type to customize real-time Web Audio sound profile:",

    // Saves
    saveExportTitle: "EXPORT SAVE (BASE64)",
    saveExportDesc: "Copy your save key to transfer progress between browsers and devices:",
    saveCopyBtn: "COPY SAVE KEY",
    saveCopied: "COPIED TO CLIPBOARD!",
    saveImportTitle: "IMPORT SAVE",
    saveImportPlaceholder: "Paste HELLOTAP_SAVE_V2:... code",
    saveImportBtn: "LOAD PROGRESS",
    saveDangerZone: "DANGER ZONE",
    saveResetBtn: "⚠️ HARD RESET ALL PROGRESS",
    saveResetConfirm: "❓ ARE YOU SURE? CLICK AGAIN",

    // Leaderboard & HUD
    leaderboardBtn: "Rankings",
    leaderboardTitle: "DEVELOPER HALL OF FAME",
    leaderboardDesc: "Global Yandex Games leaderboard of all-time compiled code lines",
    leaderboardRank: "#",
    leaderboardPlayer: "Developer",
    leaderboardScore: "C# Lines",
    leaderboardYouBadge: "YOU",
    leaderboardLoading: "Loading leaderboard...",
    autoClickerActive: "⚡ AUTOCLICKER (10 CPS)"
  }
};

export function detectInitialLanguage(): Language {
  try {
    const saved = localStorage.getItem("HELLOTAP_LANG");
    if (saved === 'ru' || saved === 'en') return saved;

    const navLang = navigator.language?.toLowerCase() || '';
    if (navLang.startsWith('ru') || navLang.startsWith('be') || navLang.startsWith('uk') || navLang.startsWith('kz')) {
      return 'ru';
    }
  } catch {
    // fallback
  }
  return 'ru';
}
