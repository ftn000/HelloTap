export type Language = 'ru' | 'en';

export interface TranslationDictionary {
  // HUD
  codePerSec: string;
  moneyPerSec: string;
  codePerClick: string;
  flowMode: string;
  hubBtn: string;
  digestBtn: string;
  saveBtn: string;
  adActive: string;

  // Clicker
  terminalFile: string;
  compileBtn: string;
  clickInstruction: string;
  critText: string;
  focusBar: string;

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

    terminalFile: "StudioEditor.cs",
    compileBtn: "⌨️ КОМПИЛИРОВАТЬ КОД",
    clickInstruction: "// Нажимайте для компиляции и генерации дохода",
    critText: "🔥 КРИТ",
    focusBar: "Шкала фокуса разработчика",

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
    ipoDesc: "Продайте акции компании инвесторам на бирже. Строки кода и базовые апгрейды сбрасываются, но вы получаете Токены Акций и постоянный множитель x1.5 на все будущие сессии!",
    ipoShares: "Акции в портфеле:",
    ipoWillGain: "Будет начислено при IPO:",
    ipoBtn: "🚀 ПРОВЕСТИ IPO",
    ipoNotEnough: "ТРЕБУЕТСЯ БОЛЬШЕ КОДА ДЛЯ IPO",

    switchTitle: "МЕХАНИЧЕСКИЕ ПЕРЕКЛЮЧАТЕЛИ КЛАВИАТУРЫ",
    switchDesc: "Выберите тип механических свитчей для изменения звукового профиля синтезатора:",

    saveExportTitle: "ЭКСПОРТ СОХРАНЕНИЯ (BASE64)",
    saveExportDesc: "Скопируйте ключ сохранения для переноса прогресса между браузерами и устройствами:",
    saveCopyBtn: "КОПИРОВАТЬ КЛЮЧ СОХРАНЕНИЯ",
    saveCopied: "СКОПИРОВАНО В БУФЕР!",
    saveImportTitle: "ИМПОРТ СОХРАНЕНИЯ",
    saveImportPlaceholder: "Вставьте код HELLOTAP_SAVE_V2:...",
    saveImportBtn: "ЗАГРУЗИТЬ ПРОГРЕСС",
    saveDangerZone: "ОПАСНАЯ ЗОНА",
    saveResetBtn: "⚠️ СБРОСИТЬ ВЕСЬ ПРОГРЕСС",
    saveResetConfirm: "❓ ТОЧНО СБРОСИТЬ? НАЖМИТЕ ЕЩЁ РАЗ"
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

    terminalFile: "StudioEditor.cs",
    compileBtn: "⌨️ COMPILE CODE",
    clickInstruction: "// Tap or click to compile and earn revenue",
    critText: "🔥 CRIT",
    focusBar: "Developer Focus Bar",

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
    ipoDesc: "Sell company shares to market investors. Code and base gear reset, but you gain Stock Tokens and a permanent x1.5 multiplier for all future sessions!",
    ipoShares: "Portfolio shares:",
    ipoWillGain: "Tokens on IPO:",
    ipoBtn: "🚀 EXECUTE IPO",
    ipoNotEnough: "MORE CODE NEEDED FOR IPO",

    switchTitle: "MECHANICAL KEYBOARD SWITCHES",
    switchDesc: "Select switch type to customize real-time Web Audio sound profile:",

    saveExportTitle: "EXPORT SAVE (BASE64)",
    saveExportDesc: "Copy your save key to transfer progress between browsers and devices:",
    saveCopyBtn: "COPY SAVE KEY",
    saveCopied: "COPIED TO CLIPBOARD!",
    saveImportTitle: "IMPORT SAVE",
    saveImportPlaceholder: "Paste HELLOTAP_SAVE_V2:... code",
    saveImportBtn: "LOAD PROGRESS",
    saveDangerZone: "DANGER ZONE",
    saveResetBtn: "⚠️ HARD RESET ALL PROGRESS",
    saveResetConfirm: "❓ ARE YOU SURE? CLICK AGAIN"
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
