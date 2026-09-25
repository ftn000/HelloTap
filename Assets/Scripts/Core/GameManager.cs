using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный менеджер игры "GameDev Clicker".
/// Управляет балансом, ресурсами, комбо-потоком, охотой на баги, оффлайн-доходом,
/// достижениями, системой Престижа (IPO), улучшениями, проектами и сохранением.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private bool isInitialized = false;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameManager>();
                if (instance != null)
                {
                    instance.EnsureInitialized();
                }
            }
            return instance;
        }
    }

    private const string SaveKeyPrefix = "DevGameSave_";

    [Header("Ресурсы")]
    [SerializeField] private double codeLines = 0;
    [SerializeField] private double totalCodeWritten = 0;
    [SerializeField] private double money = 0;
    [SerializeField] private double totalMoneyEarned = 0;

    [Header("Престиж и Статистика")]
    [SerializeField] private int prestigeLevel = 0;
    [SerializeField] private int bugsFixedCount = 0;

    [Header("Базовые параметры")]
    [SerializeField] private double baseCodePerClick = 1.0;
    [SerializeField] private float critChance = 0.08f;
    [SerializeField] private double critMultiplier = 3.0;

    [Header("Шкала В Потоке (Combo)")]
    private float comboEnergy = 0f; // 0.0 .. 1.0
    private const float ComboGainPerClick = 0.085f;
    private const float ComboDecayPerSecond = 0.15f;

    [Header("Бустеры (Энергетик)")]
    private float energyBoostTimeRemaining = 0f;
    private double activeBoostMultiplier = 1.0;

    [Header("Оффлайн-доход")]
    private float pendingOfflineSeconds = 0f;
    private double pendingOfflineCode = 0;
    private double pendingOfflineMoney = 0;

    [Header("Улучшения, Проекты и Достижения")]
    [SerializeField] private List<UpgradeItem> upgrades = new List<UpgradeItem>();
    [SerializeField] private List<GameProjectData> projects = new List<GameProjectData>();
    [SerializeField] private List<AchievementData> achievements = new List<AchievementData>();

    // События для подписчиков (UI, звуки, визуальные эффекты)
    public event Action OnCurrenciesChanged;
    public event Action<double, bool, Vector2> OnCodeClicked;
    public event Action<UpgradeItem> OnUpgradePurchased;
    public event Action<GameProjectData> OnProjectCompleted;
    public event Action<AchievementData> OnAchievementUnlocked;
    public event Action<int> OnPrestigeCompleted;
    public event Action OnOfflineEarningsReady;

    public double CodeLines => codeLines;
    public double TotalCodeWritten => totalCodeWritten;
    public double Money => money;
    public double TotalMoneyEarned => totalMoneyEarned;
    public int PrestigeLevel => prestigeLevel;
    public int BugsFixedCount => bugsFixedCount;
    public float ComboEnergy => comboEnergy;
    public bool IsBoostActive => energyBoostTimeRemaining > 0f;
    public float BoostTimeRemaining => energyBoostTimeRemaining;

    public bool HasPendingOfflineEarnings => pendingOfflineSeconds >= 15f && (pendingOfflineCode >= 1 || pendingOfflineMoney >= 1);
    public float PendingOfflineSeconds => pendingOfflineSeconds;
    public double PendingOfflineCode => pendingOfflineCode;
    public double PendingOfflineMoney => pendingOfflineMoney;

    public IReadOnlyList<UpgradeItem> Upgrades => upgrades;
    public IReadOnlyList<GameProjectData> Projects => projects;
    public IReadOnlyList<AchievementData> Achievements => achievements;

    private float autoSaveTimer = 0f;
    private const float AutoSaveInterval = 5f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;
        InitializeDefaultDataIfEmpty();
        LoadGame();
    }

    private void Start()
    {
        EnsureInitialized();
        OnCurrenciesChanged?.Invoke();
        if (HasPendingOfflineEarnings)
        {
            OnOfflineEarningsReady?.Invoke();
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        bool stateChanged = false;

        // 1. Остывание шкалы комбо "В Потоке"
        if (comboEnergy > 0f)
        {
            float prevMultiplier = (float)GetComboMultiplier();
            comboEnergy = Mathf.Max(0f, comboEnergy - ComboDecayPerSecond * dt);
            if (Mathf.Abs((float)GetComboMultiplier() - prevMultiplier) > 0.01f)
            {
                stateChanged = true;
            }
        }

        // 2. Таймер буста энергетика
        if (energyBoostTimeRemaining > 0f)
        {
            energyBoostTimeRemaining -= dt;
            if (energyBoostTimeRemaining <= 0f)
            {
                energyBoostTimeRemaining = 0f;
                activeBoostMultiplier = 1.0;
                stateChanged = true;
            }
        }

        // 3. Пассивный доход строк кода и денег в секунду
        double passiveCode = GetCodePerSecond() * dt;
        double passiveMoney = GetMoneyPerSecond() * dt;

        if (passiveCode > 0)
        {
            codeLines += passiveCode;
            totalCodeWritten += passiveCode;
            stateChanged = true;
        }

        if (passiveMoney > 0)
        {
            money += passiveMoney;
            totalMoneyEarned += passiveMoney;
            stateChanged = true;
        }

        if (stateChanged)
        {
            CheckAchievements();
            OnCurrenciesChanged?.Invoke();
        }

        // 4. Автосохранение каждые N секунд
        autoSaveTimer += dt;
        if (autoSaveTimer >= AutoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveGame();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    #region Баланс, Ранги и Множители

    /// <summary>
    /// Множитель от шкалы комбо "В Потоке" (x1.0 -> x1.5 -> x2.0 -> x3.0)
    /// </summary>
    public double GetComboMultiplier()
    {
        if (comboEnergy >= 0.90f) return 3.0;
        if (comboEnergy >= 0.65f) return 2.0;
        if (comboEnergy >= 0.35f) return 1.5;
        return 1.0;
    }

    /// <summary>
    /// Множитель от уровня Престижа (IPO): +50% за каждый выход на IPO
    /// </summary>
    public double GetPrestigeMultiplier()
    {
        return 1.0 + (prestigeLevel * 0.5);
    }

    /// <summary>
    /// Суммарный множитель от открытых достижений
    /// </summary>
    public double GetAchievementMultiplier()
    {
        double bonus = 0.0;
        foreach (var ach in achievements)
        {
            if (ach.IsUnlocked)
            {
                bonus += ach.BonusFraction;
            }
        }
        return 1.0 + bonus;
    }

    /// <summary>
    /// Глобальный множитель (Престиж * Достижения * Буст энергетика)
    /// </summary>
    public double GetGlobalMultiplier()
    {
        return GetPrestigeMultiplier() * GetAchievementMultiplier() * activeBoostMultiplier;
    }

    /// <summary>
    /// Расчет строк кода за один клик (базовый + улучшения железа) * Глобальный множитель * Комбо
    /// </summary>
    public double GetCodePerClick()
    {
        double hardwareBonus = 0;
        foreach (var upg in upgrades)
        {
            if (upg.Category == UpgradeCategory.Hardware)
            {
                hardwareBonus += upg.GetTotalPower();
            }
        }
        return (baseCodePerClick + hardwareBonus) * GetGlobalMultiplier() * GetComboMultiplier();
    }

    /// <summary>
    /// Расчет пассивных строк кода в секунду (боты, помощники, джуны)
    /// </summary>
    public double GetCodePerSecond()
    {
        double staffBonus = 0;
        foreach (var upg in upgrades)
        {
            if (upg.Category == UpgradeCategory.PassiveStaff)
            {
                staffBonus += upg.GetTotalPower();
            }
        }
        return staffBonus * GetGlobalMultiplier();
    }

    /// <summary>
    /// Расчет пассивных денег в секунду (выпущенные игры на продаже)
    /// </summary>
    public double GetMoneyPerSecond()
    {
        double moneyIncome = 0;
        foreach (var prj in projects)
        {
            if (prj.IsCompleted)
            {
                moneyIncome += prj.PassiveMoneyIncomePerSec;
            }
        }
        return moneyIncome * GetPrestigeMultiplier() * GetAchievementMultiplier();
    }

    /// <summary>
    /// Возвращает текущий ранг разработчика в зависимости от написанного кода и Престижа
    /// </summary>
    public string GetDeveloperRankTitle()
    {
        string rank;
        if (totalCodeWritten < 100) rank = "Первокурсник";
        else if (totalCodeWritten < 800) rank = "Стажёр (Intern)";
        else if (totalCodeWritten < 5000) rank = "Junior Разработчик";
        else if (totalCodeWritten < 30000) rank = "Middle Разработчик";
        else if (totalCodeWritten < 150000) rank = "Senior Архитектор";
        else rank = "Техлид / Инди-Легенда";

        if (prestigeLevel > 0)
        {
            return $"{rank} [IPO x{prestigeLevel}]";
        }
        return rank;
    }

    /// <summary>
    /// Возвращает ближайший невыполненный проект для прогресс-бара на главном экране
    /// </summary>
    public GameProjectData GetNextTargetProject()
    {
        foreach (var prj in projects)
        {
            if (!prj.IsCompleted) return prj;
        }
        return null;
    }

    public int GetUpgradeLevel(string upgradeId)
    {
        var upg = upgrades.Find(u => u.Id == upgradeId);
        return upg != null ? upg.CurrentLevel : 0;
    }

    #endregion

    #region Действия Игрока

    /// <summary>
    /// Нажатие на экран / печать кода
    /// </summary>
    public void ClickCode(Vector2 clickPosition = default)
    {
        // Повышаем шкалу комбо "В Потоке"
        comboEnergy = Mathf.Clamp01(comboEnergy + ComboGainPerClick);

        bool isCrit = UnityEngine.Random.value < critChance;
        double amount = GetCodePerClick() * (isCrit ? critMultiplier : 1.0);

        codeLines += amount;
        totalCodeWritten += amount;

        CheckAchievements();
        OnCodeClicked?.Invoke(amount, isCrit, clickPosition);
        OnCurrenciesChanged?.Invoke();
    }

    /// <summary>
    /// Награда за обезвреживание бага на мониторе
    /// </summary>
    public void ClaimBugFixReward(Vector2 screenPos, out double bonusCode, out double bonusMoney)
    {
        bugsFixedCount++;
        bonusCode = Math.Floor(Math.Max(15, GetCodePerClick() * 12));
        bonusMoney = Math.Floor(Math.Max(50, GetMoneyPerSecond() * 10 + 50));

        codeLines += bonusCode;
        totalCodeWritten += bonusCode;
        money += bonusMoney;
        totalMoneyEarned += bonusMoney;

        CheckAchievements();
        OnCurrenciesChanged?.Invoke();
        SaveGame();
    }

    /// <summary>
    /// Покупка улучшения
    /// </summary>
    public bool BuyUpgrade(string upgradeId)
    {
        UpgradeItem upg = upgrades.Find(u => u.Id == upgradeId);
        if (upg == null) return false;

        if (upg.Buy(ref money))
        {
            CheckAchievements();
            OnUpgradePurchased?.Invoke(upg);
            OnCurrenciesChanged?.Invoke();
            SaveGame();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Релиз проекта (требует накопленные строки кода)
    /// </summary>
    public bool TryReleaseProject(string projectId)
    {
        GameProjectData prj = projects.Find(p => p.Id == projectId);
        if (prj == null || prj.IsCompleted) return false;

        if (codeLines >= prj.RequiredCodeLines)
        {
            codeLines -= prj.RequiredCodeLines;
            prj.IsCompleted = true;

            double reward = Math.Floor(prj.RewardMoney * GetPrestigeMultiplier() * GetAchievementMultiplier());
            money += reward;
            totalMoneyEarned += reward;

            CheckAchievements();
            OnProjectCompleted?.Invoke(prj);
            OnCurrenciesChanged?.Invoke();
            SaveGame();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Активация буста (например, энергетик за просмотр рекламы или кнопку)
    /// </summary>
    public void ActivateEnergyBoost(float durationSeconds, double multiplier = 2.0)
    {
        energyBoostTimeRemaining = durationSeconds;
        activeBoostMultiplier = multiplier;
        OnCurrenciesChanged?.Invoke();
    }

    /// <summary>
    /// Забрать накопленный оффлайн-доход (с множителем x1 или x2)
    /// </summary>
    public void ClaimOfflineEarnings(double multiplier = 1.0)
    {
        if (!HasPendingOfflineEarnings) return;

        double addCode = Math.Floor(pendingOfflineCode * multiplier);
        double addMoney = Math.Floor(pendingOfflineMoney * multiplier);

        codeLines += addCode;
        totalCodeWritten += addCode;
        money += addMoney;
        totalMoneyEarned += addMoney;

        pendingOfflineSeconds = 0f;
        pendingOfflineCode = 0;
        pendingOfflineMoney = 0;

        CheckAchievements();
        OnCurrenciesChanged?.Invoke();
        SaveGame();
    }

    /// <summary>
    /// Проверка доступности Престижа (Выход на IPO)
    /// Требуется минимум 10 000 всего написанных строк кода или релиз хотя бы 2 проектов
    /// </summary>
    public bool CanPrestige()
    {
        int completedProjects = 0;
        foreach (var prj in projects)
        {
            if (prj.IsCompleted) completedProjects++;
        }
        return totalCodeWritten >= 10000 || completedProjects >= 2;
    }

    /// <summary>
    /// Выход на IPO (Сброс текущего забега ради постоянного множителя +50% к доходу)
    /// </summary>
    public bool PerformPrestige()
    {
        if (!CanPrestige()) return false;

        prestigeLevel++;
        codeLines = 0;
        money = 0;
        comboEnergy = 0f;

        foreach (var upg in upgrades)
        {
            upg.CurrentLevel = 0;
        }

        foreach (var prj in projects)
        {
            prj.IsCompleted = false;
        }

        CheckAchievements();
        SaveGame();
        OnPrestigeCompleted?.Invoke(prestigeLevel);
        OnCurrenciesChanged?.Invoke();
        return true;
    }

    #endregion

    #region Достижения

    private void CheckAchievements()
    {
        TryUnlockAchievement("ach_hello_world", totalCodeWritten >= 50);
        TryUnlockAchievement("ach_flow_state", comboEnergy >= 0.90f);
        TryUnlockAchievement("ach_bug_hunter", bugsFixedCount >= 1);

        var labProj = projects.Find(p => p.Id == "proj_lab");
        TryUnlockAchievement("ach_first_release", labProj != null && labProj.IsCompleted);

        var catUpg = upgrades.Find(u => u.Id == "staff_cat");
        TryUnlockAchievement("ach_cat_lover", catUpg != null && catUpg.CurrentLevel >= 1);

        bool hasLvl10Hw = false;
        foreach (var u in upgrades)
        {
            if (u.CurrentLevel >= 10) { hasLvl10Hw = true; break; }
        }
        TryUnlockAchievement("ach_hardware_pro", hasLvl10Hw);

        TryUnlockAchievement("ach_indie_rich", totalMoneyEarned >= 50000);
        TryUnlockAchievement("ach_ipo", prestigeLevel >= 1);
    }

    private void TryUnlockAchievement(string id, bool condition)
    {
        if (!condition) return;
        AchievementData ach = achievements.Find(a => a.Id == id);
        if (ach != null && !ach.IsUnlocked)
        {
            ach.IsUnlocked = true;
            OnAchievementUnlocked?.Invoke(ach);
        }
    }

    #endregion

    #region Данные по Умолчанию и Сохранение

    private void InitializeDefaultDataIfEmpty()
    {
        if (upgrades.Count == 0)
        {
            // Категория: Железо (Клик)
            upgrades.Add(new UpgradeItem("hw_keyboard", "Механическая клавиатура", "+1 строка за клик", UpgradeCategory.Hardware, 50, 1.0, 1.15));
            upgrades.Add(new UpgradeItem("hw_mouse", "Игровая мышь с макросом", "+3 строки за клик", UpgradeCategory.Hardware, 250, 3.0, 1.16));
            upgrades.Add(new UpgradeItem("hw_monitor2", "Второй монитор (для доки)", "+10 строк за клик", UpgradeCategory.Hardware, 1200, 10.0, 1.17));
            upgrades.Add(new UpgradeItem("hw_pc_rig", "Мощный ПК с RTX", "+35 строк за клик", UpgradeCategory.Hardware, 6000, 35.0, 1.18));
            upgrades.Add(new UpgradeItem("hw_chair", "Эргономичное кресло", "+120 строк за клик", UpgradeCategory.Hardware, 25000, 120.0, 1.20));

            // Категория: Персонал и Автоматизация (Пассив)
            upgrades.Add(new UpgradeItem("staff_script", "Bash / Python скрипт", "+1 строка кода/сек", UpgradeCategory.PassiveStaff, 100, 1.0, 1.15));
            upgrades.Add(new UpgradeItem("staff_cat", "Кот на клавиатуре", "+4 строки кода/сек", UpgradeCategory.PassiveStaff, 500, 4.0, 1.16));
            upgrades.Add(new UpgradeItem("staff_gpt", "ChatGPT подписка", "+15 строк кода/сек", UpgradeCategory.PassiveStaff, 2500, 15.0, 1.17));
            upgrades.Add(new UpgradeItem("staff_intern", "Студент-стажёр", "+60 строк кода/сек", UpgradeCategory.PassiveStaff, 12000, 60.0, 1.18));
            upgrades.Add(new UpgradeItem("staff_senior", "Senior ментор на час", "+250 строк кода/сек", UpgradeCategory.PassiveStaff, 60000, 250.0, 1.20));
        }

        if (projects.Count == 0)
        {
            projects.Add(new GameProjectData("proj_lab", "Лабораторная по Unity", "Сдать лабу преподавателю вовремя", 100, 300, 2));
            projects.Add(new GameProjectData("proj_tg_bot", "Бот в Telegram", "Простой кликер с мемами", 600, 2000, 10));
            projects.Add(new GameProjectData("proj_yandex", "Игра для Яндекс Игр", "Гиперказуальный хит в топе каталога", 3500, 15000, 50));
            projects.Add(new GameProjectData("proj_steam", "Инди-игра в Steam", "Мрачный рогалик с пиксель-артом", 20000, 100000, 250));
            projects.Add(new GameProjectData("proj_mmo", "MMO-убийца Ведьмака", "Проект всей вашей жизни", 150000, 1000000, 2000));
        }

        if (achievements.Count == 0)
        {
            achievements.Add(new AchievementData("ach_hello_world", "Hello, World!", "Написать первые 50 строк кода", 0.05));
            achievements.Add(new AchievementData("ach_flow_state", "В Потоке!", "Развить максимальный темп печати x3.0", 0.05));
            achievements.Add(new AchievementData("ach_bug_hunter", "Гроза багов", "Исправить критический баг на мониторе", 0.05));
            achievements.Add(new AchievementData("ach_first_release", "Зачёт получен!", "Сдать Лабораторную по Unity", 0.10));
            achievements.Add(new AchievementData("ach_cat_lover", "Пушистый кодер", "Взять кота на клавиатуру", 0.10));
            achievements.Add(new AchievementData("ach_hardware_pro", "Техно-энтузиаст", "Прокачать любое улучшение до 10 уровня", 0.10));
            achievements.Add(new AchievementData("ach_indie_rich", "Успешный инди", "Заработать суммарно 50 000 руб.", 0.15));
            achievements.Add(new AchievementData("ach_ipo", "Выход на IPO", "Совершить первый Престиж студии", 0.20));
        }
    }

    public void SaveGame()
    {
        PlayerPrefs.SetString(SaveKeyPrefix + "Code", codeLines.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "TotalCode", totalCodeWritten.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "Money", money.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "TotalMoney", totalMoneyEarned.ToString("R"));
        PlayerPrefs.SetInt(SaveKeyPrefix + "Prestige", prestigeLevel);
        PlayerPrefs.SetInt(SaveKeyPrefix + "BugsFixed", bugsFixedCount);
        PlayerPrefs.SetString(SaveKeyPrefix + "LastUtcTicks", DateTime.UtcNow.Ticks.ToString());

        foreach (var upg in upgrades)
        {
            PlayerPrefs.SetInt(SaveKeyPrefix + "Upg_" + upg.Id, upg.CurrentLevel);
        }

        foreach (var prj in projects)
        {
            PlayerPrefs.SetInt(SaveKeyPrefix + "Prj_" + prj.Id, prj.IsCompleted ? 1 : 0);
        }

        foreach (var ach in achievements)
        {
            PlayerPrefs.SetInt(SaveKeyPrefix + "Ach_" + ach.Id, ach.IsUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKeyPrefix + "Code"))
        {
            if (double.TryParse(PlayerPrefs.GetString(SaveKeyPrefix + "Code"), out double loadedCode))
                codeLines = loadedCode;

            if (double.TryParse(PlayerPrefs.GetString(SaveKeyPrefix + "TotalCode"), out double loadedTotCode))
                totalCodeWritten = loadedTotCode;

            if (double.TryParse(PlayerPrefs.GetString(SaveKeyPrefix + "Money"), out double loadedMoney))
                money = loadedMoney;

            if (double.TryParse(PlayerPrefs.GetString(SaveKeyPrefix + "TotalMoney"), out double loadedTotMoney))
                totalMoneyEarned = loadedTotMoney;

            prestigeLevel = PlayerPrefs.GetInt(SaveKeyPrefix + "Prestige", 0);
            bugsFixedCount = PlayerPrefs.GetInt(SaveKeyPrefix + "BugsFixed", 0);

            foreach (var upg in upgrades)
            {
                upg.CurrentLevel = PlayerPrefs.GetInt(SaveKeyPrefix + "Upg_" + upg.Id, 0);
            }

            foreach (var prj in projects)
            {
                prj.IsCompleted = PlayerPrefs.GetInt(SaveKeyPrefix + "Prj_" + prj.Id, 0) == 1;
            }

            foreach (var ach in achievements)
            {
                ach.IsUnlocked = PlayerPrefs.GetInt(SaveKeyPrefix + "Ach_" + ach.Id, 0) == 1;
            }

            // Расчет оффлайн-дохода ("Пока вас не было")
            if (PlayerPrefs.HasKey(SaveKeyPrefix + "LastUtcTicks"))
            {
                if (long.TryParse(PlayerPrefs.GetString(SaveKeyPrefix + "LastUtcTicks"), out long lastTicks))
                {
                    DateTime lastTime = new DateTime(lastTicks, DateTimeKind.Utc);
                    double elapsedSec = (DateTime.UtcNow - lastTime).TotalSeconds;
                    if (elapsedSec >= 15.0)
                    {
                        float clampedSec = Mathf.Clamp((float)elapsedSec, 15f, 86400f); // Максимум 24 часа
                        double codeRate = GetCodePerSecond();
                        double moneyRate = GetMoneyPerSecond();

                        if (codeRate > 0 || moneyRate > 0)
                        {
                            pendingOfflineSeconds = clampedSec;
                            pendingOfflineCode = Math.Floor(clampedSec * codeRate * 0.5); // 50% эффективности оффлайн
                            pendingOfflineMoney = Math.Floor(clampedSec * moneyRate * 0.5);
                        }
                    }
                }
            }
        }
    }

    public void ResetAllData()
    {
        codeLines = 0;
        totalCodeWritten = 0;
        money = 0;
        totalMoneyEarned = 0;
        prestigeLevel = 0;
        bugsFixedCount = 0;
        comboEnergy = 0f;
        energyBoostTimeRemaining = 0;
        activeBoostMultiplier = 1.0;
        pendingOfflineSeconds = 0f;
        pendingOfflineCode = 0;
        pendingOfflineMoney = 0;

        foreach (var upg in upgrades)
        {
            upg.CurrentLevel = 0;
        }

        foreach (var prj in projects)
        {
            prj.IsCompleted = false;
        }

        foreach (var ach in achievements)
        {
            ach.IsUnlocked = false;
        }

        SaveGame();
        OnCurrenciesChanged?.Invoke();
    }

    #endregion
}
