export type Language = 'ru' | 'en' | 'tr';

export interface TranslationDictionary {
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
  upgradesTitle: string;
  buyBtn: string;
  maxBtn: string;
  lvlPrefix: string;
  tabSystems: string;
  tabShop: string;
  tabBoosts: string;
  tabIPO: string;
  tabSwitches: string;
  tabSaves: string;
  catTitle: string;
  catPurr: string;
  catName: string;
  catSleeping: string;
  coffeeTooltip: string;
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
  brokenBuildMessage: string;
  brokenBuildFixTooltip: string;
  runPipelineTooltip: string;
  fixBrokenStepTooltip: string;
  cmdPlaceholder: string;
  cmdNotFound: string;
  navHint: string;
  selectHint: string;
  closeHint: string;
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
  offlineWelcome: string;
  offlineAwayDesc: string;
  offlineCodeLabel: string;
  offlineRevenueLabel: string;
  offlineDoubleBtn: string;
  offlineClaimBtn: string;
  eventHint: string;
  eventSkip: string;
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
  tierPrefix: string;
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
  catAll: string;
  catOffice: string;
  catBusiness: string;
  catTech: string;
  catCulture: string;
  yandexBonusTitle: string;
  yandexBonusDesc: string;
  yandexAdDouble: string;
  yandexAdDoubleDesc: string;
  yandexAdTimeWarp: string;
  yandexAdTimeWarpDesc: string;
  digestTitle: string;
  digestDesc: string;
  digestClaimBtn: string;
  timeWarpTitle: string;
  timeWarpDesc: string;
  timeWarpBtn: string;
  timeWarpCharging: string;
  ipoTitle: string;
  ipoDesc: string;
  ipoShares: string;
  ipoWillGain: string;
  ipoBtn: string;
  ipoNotEnough: string;
  switchTitle: string;
  switchDesc: string;
  rateGameBtn: string;
  rateGameDesc: string;
  addShortcutBtn: string;
  addShortcutDesc: string;
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
  leaderboardBtn: string;
  leaderboardTitle: string;
  leaderboardDesc: string;
  leaderboardRank: string;
  leaderboardPlayer: string;
  leaderboardScore: string;
  leaderboardYouBadge: string;
  leaderboardLoading: string;
  autoClickerActive: string;
  hourShort: string;
  minShort: string;
  branchBusiness: string;
  branchAI: string;
  ptsShort: string;
  refactorDoneTag: string;
  prMergedPopup: string;
  refactoredLine: string;
  profileAsmrDesc: string;
  profileClassicDesc: string;
  profileCyberDesc: string;
  profileMuteDesc: string;
  switchBlueDesc: string;
  switchRedDesc: string;
  switchBrownDesc: string;
  switchLaserDesc: string;
  boostTag: string;
}

export const TRANSLATIONS: Record<Language, TranslationDictionary> = {
  ru: {
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
    upgradesTitle: "Оборудование и Улучшения",
    buyBtn: "КУПИТЬ",
    maxBtn: "МАКС",
    lvlPrefix: "ур.",
    tabSystems: "🏢 Системы",
    tabShop: "💎 Донат",
    tabBoosts: "⚡ Бусты & Реклама",
    tabIPO: "📈 IPO",
    tabSwitches: "⌨️ Свитчи",
    tabSaves: "💾 Сейвы",
    catTitle: "Офисный кот-талисман (кликни погладить!)",
    catPurr: "Мурр! ❤️",
    catName: "Барсик",
    catSleeping: "Котик спит (прокачай приют в Hub)",
    coffeeTooltip: "Кофе программиста",
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
    brokenBuildMessage: "❌ Broken Build: Pipeline #{0} упал на [{1}] — кликните для починки!",
    brokenBuildFixTooltip: "Устранить аварию CI/CD и получить награду за хотфикс",
    runPipelineTooltip: "Запустить ручной прогон CI/CD пайплайна",
    fixBrokenStepTooltip: "Кликните здесь, чтобы устранить сбой сборки!",
    cmdPlaceholder: "Введите команду или действие... (например, branch, merge, blitz, theme)",
    cmdNotFound: "Команда не найдена. Попробуйте другой запрос.",
    navHint: "↑↓ Навигация",
    selectHint: "↵ Выбор",
    closeHint: "ESC Закрыть",
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
    offlineWelcome: "С ВОЗВРАЩЕНИЕМ В ОФИС!",
    offlineAwayDesc: "Пока вас не было ({0}), серверы студии продолжали компилировать код!",
    offlineCodeLabel: "Код C#",
    offlineRevenueLabel: "Выручка",
    offlineDoubleBtn: "УДВОИТЬ БОНУС (x2)",
    offlineClaimBtn: "Забрать обычный бонус",
    eventHint: "Быстрый выбор дает преимущество студии",
    eventSkip: "Пропустить",
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
    tierPrefix: "Ур.",
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
    catAll: "Все",
    catOffice: "Офис",
    catBusiness: "Бизнес",
    catTech: "Технологии",
    catCulture: "Культура",
    yandexBonusTitle: "БОНУСЫ ЯНДЕКС ИГР",
    yandexBonusDesc: "Просмотр короткого ролика за супер-буст",
    yandexAdDouble: "📺 x2 ДОХОД НА 3 МИНУТЫ",
    yandexAdDoubleDesc: "Удваивает весь C# и рубли",
    yandexAdTimeWarp: "📺 СБРОСИТЬ TIME WARP",
    yandexAdTimeWarpDesc: "Мгновенная зарядка 2h варпа",
    digestTitle: "УТРЕННИЙ ДАЙДЖЕСТ",
    digestDesc: "Сводный операционный центр и автоматический сбор наград",
    digestClaimBtn: "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ",
    timeWarpTitle: "TIME WARP: СИМУЛЯТОР СМЕНЫ",
    timeWarpDesc: "Мгновенная автономная выработка за 2 часа",
    timeWarpBtn: "⚡ ЗАПУСТИТЬ TIME WARP (2 ЧАСА)",
    timeWarpCharging: "⏳ ЗАРЯДКА",
    ipoTitle: "ВЫХОД НА IPO (ПРЕСТИЖ)",
    ipoDesc: "Продайте акции компании инвесторам на бирже. Все накопленные деньги, команда и купленное железо сохраняются! Вы получаете инвестиционный грант, Токены Акций и постоянный множитель x1.5 на все будущие сессии!",
    ipoShares: "Акции в портфеле:",
    ipoWillGain: "Будет начислено при IPO:",
    ipoBtn: "🚀 ПРОВЕСТИ IPO",
    ipoNotEnough: "ТРЕБУЕТСЯ БОЛЬШЕ КОДА ДЛЯ IPO",
    switchTitle: "МЕХАНИЧЕСКИЕ ПЕРЕКЛЮЧАТЕЛИ КЛАВИАТУРЫ",
    switchDesc: "Выберите тип механических свитчей для изменения звукового профиля синтезатора:",
    rateGameBtn: "⭐ Оценить игру в Яндекс Играх",
    rateGameDesc: "Поставьте 5 звезд и поддержите команду разработчиков!",
    addShortcutBtn: "📱 Добавить ярлык на рабочий стол",
    addShortcutDesc: "Быстрый запуск игры с главного экрана в 1 клик",
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
    leaderboardBtn: "Рейтинг",
    leaderboardTitle: "ТОП РАЗРАБОТЧИКОВ (РЕЙТИНГ)",
    leaderboardDesc: "Глобальный зал славы Яндекс Игр по количеству скомпилированных строк кода за все время",
    leaderboardRank: "#",
    leaderboardPlayer: "Разработчик",
    leaderboardScore: "Строк C#",
    leaderboardYouBadge: "ВЫ",
    leaderboardLoading: "Загрузка топа...",
    autoClickerActive: "⚡ АВТОКЛИКЕР (10 CPS)",
    hourShort: "ч",
    minShort: "м",
    branchBusiness: "Стартап-магнат",
    branchAI: "ИИ и нейросети",
    ptsShort: "очк.",
    refactorDoneTag: "ВСЕ 5 СТРОК ОТРЕФАКТОРЕНЫ",
    prMergedPopup: "PR СЛИТ!",
    refactoredLine: "✓ Отрефакторено",
    profileAsmrDesc: "Мягкий и глубокий",
    profileClassicDesc: "Четкий звонкий",
    profileCyberDesc: "Неоновый синт",
    profileMuteDesc: "Без звука",
    switchBlueDesc: "Громкий звонкий щелчок",
    switchRedDesc: "Тихий мягкий ход",
    switchBrownDesc: "Четкий тактильный бумп",
    switchLaserDesc: "Синтезаторный лазер",
    boostTag: "буст"
  },
  en: {
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
    upgradesTitle: "Hardware & Upgrades",
    buyBtn: "BUY",
    maxBtn: "MAX",
    lvlPrefix: "lvl",
    tabSystems: "🏢 Systems",
    tabShop: "💎 VIP Store",
    tabBoosts: "⚡ Boosts & Ads",
    tabIPO: "📈 IPO",
    tabSwitches: "⌨️ Switches",
    tabSaves: "💾 Saves",
    catTitle: "Office Mascot Cat (Click to pet!)",
    catPurr: "Purr! ❤️",
    catName: "Pixel",
    catSleeping: "Kitty is asleep (upgrade Haven in Hub)",
    coffeeTooltip: "Developer's Coffee",
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
    brokenBuildMessage: "❌ Broken Build: Pipeline #{0} failed at [{1}] — click to fix!",
    brokenBuildFixTooltip: "Fix CI/CD broken build and claim hotfix reward",
    runPipelineTooltip: "Trigger manual CI/CD pipeline run",
    fixBrokenStepTooltip: "Click here to fix the broken build step!",
    cmdPlaceholder: "Type a command or action... (e.g. branch, merge, blitz, theme)",
    cmdNotFound: "No commands found. Try another query.",
    navHint: "↑↓ Navigate",
    selectHint: "↵ Select",
    closeHint: "ESC Close",
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
    offlineWelcome: "WELCOME BACK TO THE OFFICE!",
    offlineAwayDesc: "While you were away ({0}), studio servers kept compiling code!",
    offlineCodeLabel: "C# Code",
    offlineRevenueLabel: "Revenue",
    offlineDoubleBtn: "DOUBLE REWARD (x2)",
    offlineClaimBtn: "Claim Regular Reward",
    eventHint: "Quick decisions empower the studio",
    eventSkip: "Skip",
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
    tierPrefix: "Tier",
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
    catAll: "All",
    catOffice: "Office",
    catBusiness: "Business",
    catTech: "Tech",
    catCulture: "Culture",
    yandexBonusTitle: "YANDEX GAMES BOOSTS",
    yandexBonusDesc: "Watch a short ad for a studio super-boost",
    yandexAdDouble: "📺 x2 INCOME FOR 3 MIN",
    yandexAdDoubleDesc: "Doubles all C# and Money earnings",
    yandexAdTimeWarp: "📺 RESET TIME WARP",
    yandexAdTimeWarpDesc: "Instantly recharge 2-hour Time Warp",
    digestTitle: "DAILY DIGEST",
    digestDesc: "Studio operations hub and automated reward collection",
    digestClaimBtn: "💰 CLAIM ALL REWARDS & DIVIDENDS",
    timeWarpTitle: "TIME WARP: SHIFT SIMULATOR",
    timeWarpDesc: "Instant autonomous studio progress for 2 hours",
    timeWarpBtn: "⚡ LAUNCH TIME WARP (2 HOURS)",
    timeWarpCharging: "⏳ CHARGING",
    ipoTitle: "GO PUBLIC (IPO PRESTIGE)",
    ipoDesc: "Sell company shares to market investors. All money, studio gear, and team members are preserved! You gain an IPO cash grant, Stock Tokens and a permanent x1.5 multiplier for all future sessions!",
    ipoShares: "Portfolio shares:",
    ipoWillGain: "Tokens on IPO:",
    ipoBtn: "🚀 EXECUTE IPO",
    ipoNotEnough: "MORE CODE NEEDED FOR IPO",
    switchTitle: "MECHANICAL KEYBOARD SWITCHES",
    switchDesc: "Select switch type to customize real-time Web Audio sound profile:",
    rateGameBtn: "⭐ Rate Game on Yandex Games",
    rateGameDesc: "Leave 5 stars to support the dev team!",
    addShortcutBtn: "📱 Add Shortcut to Desktop",
    addShortcutDesc: "Instant 1-click launch from home screen",
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
    leaderboardBtn: "Rankings",
    leaderboardTitle: "DEVELOPER HALL OF FAME",
    leaderboardDesc: "Global Yandex Games leaderboard of all-time compiled code lines",
    leaderboardRank: "#",
    leaderboardPlayer: "Developer",
    leaderboardScore: "C# Lines",
    leaderboardYouBadge: "YOU",
    leaderboardLoading: "Loading leaderboard...",
    autoClickerActive: "⚡ AUTOCLICKER (10 CPS)",
    hourShort: "h",
    minShort: "m",
    branchBusiness: "Startup Tycoon",
    branchAI: "AI & Neural Nets",
    ptsShort: "pts",
    refactorDoneTag: "ALL 5 REFACTORED",
    prMergedPopup: "PR MERGED!",
    refactoredLine: "✓ Refactored",
    profileAsmrDesc: "Soft & deep",
    profileClassicDesc: "Crisp clicky",
    profileCyberDesc: "Neon synth",
    profileMuteDesc: "Muted",
    switchBlueDesc: "Loud tactile click",
    switchRedDesc: "Smooth quiet keystroke",
    switchBrownDesc: "Crisp tactile bump",
    switchLaserDesc: "Synth laser pew",
    boostTag: "boost"
  },
  tr: {
    codePerSec: "sn",
    moneyPerSec: "sn",
    codePerClick: "C#/tıklama",
    flowMode: "FLOW MODU AKTİF (x3.0 ÇARPAN)!",
    hubBtn: "Stüdyo Merkezi",
    digestBtn: "Günlük Özet",
    saveBtn: "Bulut ve Kayıtlar",
    adActive: "x2 AKTİF",
    soundBtn: "Ses",
    musicTurnOn: "Lo-Fi Müziği Başlat",
    musicNowPlaying: "Şu an çalıyor: {0} (Duraklatmak için tıkla)",
    musicNextTrack: "Sonraki parça ({0})",
    techTreeBtn: "BT Yetenek ve Beceri Ağacı [K]",
    achievementsBtn: "Başarımlar ve Rozetler [A]",
    heatmapBtn: "GitHub Katkı Isı Haritası",
    codePerSecShort: "/sn",
    moneyPerSecShort: "₽/sn",
    secShort: "sn",
    hourShort: "sa",
    minShort: "dk",
    terminalFile: "StudioEditor.cs",
    compileBtn: "⌨️ KODU DERLE",
    clickInstruction: "// Kod derlemek ve gelir elde etmek için dokunun veya tıklayın",
    critText: "🔥 KRİTİK",
    focusBar: "Geliştirici Odak Çubuğu",
    locPerClick: "C#/tıklama",
    locPerSec: "C#/sn",
    rubPerSec: "₽/sn",
    fastSwitchHint: "Tab veya Ctrl+P ile hızlı geçiş",
    fileOpenedLog: "✓ [{0}] Açıldı: {1} ({2})",
    bugFixedToast: "🐛 Hata çözüldü! +{0} C# [OVERCLOCK x3.0]",
    bugButton: "HATA AYIKLA! ({0}sn)",
    refactorBlitzBanner: "⚡ REFACTOR BLITZ! 5 satıra tıklayın: ({0}/5)",
    refactorBlitzReward: "Ödül: 20 saniye boyunca 10 kat artış!",
    quickFixTooltip: "— Hızlı Düzeltme için tıklayın!",
    quickFixButton: "💡 Hızlı Düzeltme (+bonus)",
    quickFixInline: "💡 Düzelt ({0}sn)",
    quickFixTooltipAttr: "Kod hatasını otomatik düzeltmek için tıklayın",
    mergePrTooltip: "Pull Request'i main dalına birleştir ve ödülü topla",
    createBranchTooltip: "Yeni özellik dalı oluştur (+%25 kod artışı)",
    cmdPaletteTooltip: "VS Code Komut Paletini Aç (Ctrl+Shift+P / F1)",
    heatmapTooltip: "GitHub Katkı Isı Haritasını Aç",
    linterErrorTooltip: "Sözdizimi hatası: Hızlı Düzeltme için tıklayın",
    switchesHeader: "Anahtarlar:",
    themePreset: "(Tema ön ayarı)",
    themeBadge: "Tema",
    refactorDoneTag: "5 SATIRIN HEPSİ YENİDEN DÜZENLENDİ",
    prMergedPopup: "PR BİRLEŞTİRİLDİ!",
    refactoredLine: "✓ Düzenlendi",
    upgradesTitle: "Donanım ve Yükseltmeler",
    buyBtn: "SATIN AL",
    maxBtn: "MAKS",
    lvlPrefix: "svy",
    tabSystems: "🏢 Sistemler",
    tabShop: "💎 VIP Mağaza",
    tabBoosts: "⚡ Güçlendirmeler ve Reklamlar",
    tabIPO: "📈 Halka Arz (IPO)",
    tabSwitches: "⌨️ Tuş Anahtarları",
    tabSaves: "💾 Kayıtlar",
    catTitle: "Ofis Maskotu Kedi (Sevmek için tıklayın!)",
    catPurr: "Mırrr! ❤️",
    catName: "Pixel",
    catSleeping: "Kediciğimiz uyuyor (Merkez'de Barınak yükseltin)",
    coffeeTooltip: "Geliştirici Kahvesi",
    hubHeader: "STUDIO OS — SİSTEMLER VE PRESTİJ",
    importSuccess: "✓ İlerleme başarıyla yüklendi!",
    importError: "❌ Hata: Geçersiz kayıt anahtarı biçimi!",
    digestClaimsCount: "Toplama sayısı: {0}",
    ipoTokensGain: "+{0} Jeton",
    ipoTokensInPortfolio: "{0} adet (+%{1} artış)",
    sfxTitle: "SFX Ses Düzeyi ve Ses Profilleri",
    sfxVolumeLabel: "Tıklama ve arayüz ses efektleri düzeyi:",
    sfxTestClick: "Tıklama testi 🔊",
    sfxProfilesLabel: "Ses profili seçici:",
    sfxBellLabel: "Kritikler ve Flow için cam çıngırak:",
    sfxBellCrit: "🔔 Kritik",
    sfxBellFlow: "✨ Flow",
    musicSectionTitle: "Arka Plan Lo-Fi Müziği (Coder Beats)",
    musicSectionDesc: "Derin kodlama için üretken Lo-Fi synth ortam müziği",
    musicVolumeLabel: "Müzik düzeyi:",
    musicPauseBtn: "Duraklat",
    musicPlayBtn: "Çal",
    musicNextBtn: "Sonraki parça",
    themesSectionTitle: "IDE ve Terminal Temaları",
    themesSectionDesc: "Renk paletleri, sözdizimi vurgulama ve neon ortam ışıkları.",
    themeSoundLabel: "Ses",
    profileAsmrDesc: "Yumuşak ve derin",
    profileClassicDesc: "Net ve tıkırtılı",
    profileCyberDesc: "Neon synth",
    profileMuteDesc: "Sessiz",
    switchBlueDesc: "Yüksek sesli dokunsal tıklama",
    switchRedDesc: "Pürüzsüz sessiz vuruş",
    switchBrownDesc: "Belirgin dokunsal his",
    switchLaserDesc: "Fütüristik synth lazer",
    boostTag: "artış",
    brokenBuildMessage: "❌ Bozuk Derleme: #{0} numaralı ardışık düzen [{1}] adımında başarısız oldu — düzeltmek için tıklayın!",
    brokenBuildFixTooltip: "Bozuk CI/CD derlemesini düzeltin ve düzeltme ödülünü alın",
    runPipelineTooltip: "Manuel CI/CD ardışık düzenini çalıştır",
    fixBrokenStepTooltip: "Bozuk derleme adımını düzeltmek için buraya tıklayın!",
    cmdPlaceholder: "Bir komut veya eylem yazın... (örn. dal, birleştir, blitz, tema)",
    cmdNotFound: "Komut bulunamadı. Başka bir arama deneyin.",
    navHint: "↑↓ Gezin",
    selectHint: "↵ Seç",
    closeHint: "ESC Kapat",
    rankLabel: "Geliştirici Kademesi",
    totalContributionsLabel: "Toplam Katkılar (365g)",
    streakLabel: "Aktif Gün Serisi",
    streakDays: "{0} gün (en çok: {1})",
    pipelinesLabel: "CI/CD Ardışık Düzenleri",
    commitBtn: "Aktiviteyi Commit Et (+1 katkı)",
    hoverDetails: "Ayrıntıları görmek için kutunun üzerine gelin",
    contributionsCount: "{0} kod katkısı",
    contributionsTitle: "{0}: {1} katkı",
    legendLess: "Az",
    legendMore: "Çok",
    offlineWelcome: "OFİSE TEKRAR HOŞ GELDİNİZ!",
    offlineAwayDesc: "Siz yokken ({0}), stüdyo sunucuları kod derlemeye devam etti!",
    offlineCodeLabel: "C# Kodu",
    offlineRevenueLabel: "Gelir",
    offlineDoubleBtn: "ÖDÜLÜ İKİYE KATLA (x2)",
    offlineClaimBtn: "Normal Ödülü Al",
    eventHint: "Hızlı kararlar stüdyoyu güçlendirir",
    eventSkip: "Geç",
    techTreeTitle: "BT YETENEK VE BECERİ AĞACI",
    techTreeSubtitle: "Yetenek puanları, Halka Arz ve başarım kilitleri açıldıkça verilir",
    pointsCount: "Puanlar: {0}",
    allBranches: "Tüm Dallar",
    resetSkillsBtn: "Sıfırla",
    resetSkillsConfirm: "Harcanan tüm yetenek puanları sıfırlanıp bakiyenize iade edilsin mi?",
    resetSkillsTooltip: "Tüm becerileri sıfırla ve puanları geri al",
    reqBaseSkill: "🔒 Dalın temel becerisi gereklidir",
    maxedSkill: "MAKS",
    upgradeSkill: "Yükselt ({0} puan)",
    levelPrefix: "Seviye",
    branchBusiness: "Girişimci Patron",
    branchAI: "Yapay Zeka ve Sinir Ağları",
    ptsShort: "puan",
    tierPrefix: "Aşama",
    vipStoreTitle: "VIP PAZAR YERİ (YANDEX OYUNLAR)",
    vipStoreDesc: "Yan para birimi ile satın alımlar (Yandex Uygulama İçi Satın Alma). Kalıcı yükseltmeler sonsuza kadar sizde kalır!",
    vipActivePerks: "AKTİF AYRICALIKLAR:",
    vipPerkMultiplier: "👑 Kalıcı x2 gelir çarpanı",
    vipPerkAutoclicker: "⚡ Otomatik Tıklayıcı Bot Pro (10 CPS)",
    vipPerkNoAds: "🚫 Reklamsız mod (anında bonus ödülleri)",
    buyForYans: "{0} YAN KARŞILIĞINDA AL",
    alreadyOwned: "SATIN ALINDI ✓",
    permanentBadge: "KALICI",
    consumableBadge: "ANINDA",
    purchaseSuccess: "🎉 Satın alma başarıyla tamamlandı!",
    purchaseFailed: "❌ Satın alma hatası veya iptal edildi",
    noAdsActiveBadge: "👑 VIP REKLAMSIZ: REKLAMLAR ATLANDI (ANINDA ÖDÜL)",
    catAll: "Tümü",
    catOffice: "Ofis",
    catBusiness: "İşletme",
    catTech: "Teknoloji",
    catCulture: "Kültür",
    yandexBonusTitle: "YANDEX OYUN GÜÇLENDİRMELERİ",
    yandexBonusDesc: "Stüdyoya süper güçlendirme için kısa bir reklam izleyin",
    yandexAdDouble: "📺 3 DAKİKA x2 GELİR",
    yandexAdDoubleDesc: "Tüm C# ve para kazancını ikiye katlar",
    yandexAdTimeWarp: "📺 ZAMAN BÜKMEYİ SIFIRLA",
    yandexAdTimeWarpDesc: "2 saatlik Zaman Bükmeyi anında yeniden doldurur",
    digestTitle: "GÜNLÜK ÖZET",
    digestDesc: "Stüdyo operasyon merkezi ve otomatik ödül toplama",
    digestClaimBtn: "💰 TÜM ÖDÜLLERİ VE TEMETTÜLERİ AL",
    timeWarpTitle: "ZAMAN BÜKME: VARDİYA SİMÜLATÖRÜ",
    timeWarpDesc: "2 saatlik anında otonom stüdyo ilerlemesi",
    timeWarpBtn: "⚡ ZAMAN BÜKMEYİ BAŞLAT (2 SAAT)",
    timeWarpCharging: "⏳ DOLUYOR",
    ipoTitle: "HALKA ARZ EDİL (IPO PRESTİJİ)",
    ipoDesc: "Şirket hisselerini piyasa yatırımcılarına satın. Tüm para, stüdyo ekipmanı ve çalışanlar KORUNUR! Bir IPO nakit hibesi, Hisse Jetonları ve gelecekteki tüm oturumlar için kalıcı bir x1.5 çarpanı kazanırsınız!",
    ipoShares: "Portföy hisseleri:",
    ipoWillGain: "Halka Arzda Jetonlar:",
    ipoBtn: "🚀 HALKA ARZI BAŞLAT",
    ipoNotEnough: "HALKA ARZ İÇİN DAHA FAZLA KOD GEREKİYOR",
    switchTitle: "MEKANİK KLAVYE ANAHTARLARI",
    switchDesc: "Gerçek zamanlı Web Audio ses profilini özelleştirmek için anahtar türünü seçin:",
    rateGameBtn: "⭐ Yandex'te Oyunu Değerlendir",
    rateGameDesc: "5 yıldız verin ve geliştirici ekibi destekleyin!",
    addShortcutBtn: "📱 Masaüstüne Kısayol Ekle",
    addShortcutDesc: "Ana ekrandan tek tıkla hızlı başlatma",
    saveExportTitle: "KAYDI DIŞA AKTAR (BASE64)",
    saveExportDesc: "İlerlemenizi tarayıcılar ve cihazlar arasında aktarmak için kayıt anahtarınızı kopyalayın:",
    saveCopyBtn: "KAYIT ANAHTARINI KOPYALA",
    saveCopied: "PANAYA KOPYALANDI!",
    saveImportTitle: "KAYDI İÇE AKTAR",
    saveImportPlaceholder: "HELLOTAP_SAVE_V2:... kodunu yapıştırın",
    saveImportBtn: "İLERLEMEYİ YÜKLE",
    saveDangerZone: "TEHLİKELİ BÖLGE",
    saveResetBtn: "⚠️ TÜM İLERLEMEYİ SIFIRLA",
    saveResetConfirm: "❓ EMİN MİSİNİZ? TEKRAR TIKLAYIN",
    leaderboardBtn: "Sıralama",
    leaderboardTitle: "GELİŞTİRİCİ ŞÖHRET LİSTESİ",
    leaderboardDesc: "Tüm zamanların derlenen kod satırlarına göre küresel Yandex Oyunları lider tablosu",
    leaderboardRank: "#",
    leaderboardPlayer: "Geliştirici",
    leaderboardScore: "C# Satırları",
    leaderboardYouBadge: "SEN",
    leaderboardLoading: "Lider tablosu yükleniyor...",
    autoClickerActive: "⚡ OTOMATİK TIKLAYICI (10 CPS)"
  }
};

export function detectInitialLanguage(): Language {
  try {
    const saved = localStorage.getItem("HELLOTAP_LANG") as Language;
    if (saved && (saved === 'ru' || saved === 'en' || saved === 'tr')) return saved;

    const anyWin = typeof window !== 'undefined' ? (window as unknown as { ysdk?: { environment?: { i18n?: { lang?: string } } } }) : null;
    const yaLang = anyWin?.ysdk?.environment?.i18n?.lang?.toLowerCase?.();
    if (yaLang) {
      if (yaLang.startsWith('tr')) return 'tr';
      if (yaLang.startsWith('ru') || yaLang.startsWith('be') || yaLang.startsWith('uk') || yaLang.startsWith('kk') || yaLang.startsWith('uz')) return 'ru';
      return 'en';
    }

    const navLang = navigator.language?.toLowerCase() || '';
    if (navLang.startsWith('ru') || navLang.startsWith('be') || navLang.startsWith('uk') || navLang.startsWith('kz')) {
      return 'ru';
    }
    if (navLang.startsWith('tr')) return 'tr';
    if (navLang.startsWith('en')) return 'en';
  } catch {
    // fallback
  }
  return 'ru';
}
