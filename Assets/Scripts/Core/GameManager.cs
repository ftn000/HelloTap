using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Главный менеджер игры "GameDev Clicker".
/// Управляет балансом, ресурсами, улучшениями, проектами и сохранением.
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

    [Header("Базовые параметры")]
    [SerializeField] private double baseCodePerClick = 1.0;
    [SerializeField] private float critChance = 0.08f;
    [SerializeField] private double critMultiplier = 3.0;

    [Header("Бустеры (Энергетик)")]
    private float energyBoostTimeRemaining = 0f;
    private double activeBoostMultiplier = 1.0;

    [Header("Улучшения и Проекты")]
    [SerializeField] private List<UpgradeItem> upgrades = new List<UpgradeItem>();
    [SerializeField] private List<GameProjectData> projects = new List<GameProjectData>();

    // События для подписчиков (UI, звуки, визуальные эффекты)
    public event Action OnCurrenciesChanged;
    public event Action<double, bool, Vector2> OnCodeClicked;
    public event Action<UpgradeItem> OnUpgradePurchased;
    public event Action<GameProjectData> OnProjectCompleted;

    public double CodeLines => codeLines;
    public double TotalCodeWritten => totalCodeWritten;
    public double Money => money;
    public double TotalMoneyEarned => totalMoneyEarned;
    public bool IsBoostActive => energyBoostTimeRemaining > 0f;
    public float BoostTimeRemaining => energyBoostTimeRemaining;
    public IReadOnlyList<UpgradeItem> Upgrades => upgrades;
    public IReadOnlyList<GameProjectData> Projects => projects;

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
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Таймер буста энергетика
        if (energyBoostTimeRemaining > 0f)
        {
            energyBoostTimeRemaining -= dt;
            if (energyBoostTimeRemaining <= 0f)
            {
                energyBoostTimeRemaining = 0f;
                activeBoostMultiplier = 1.0;
                OnCurrenciesChanged?.Invoke();
            }
        }

        // Пассивный доход строк кода и денег в секунду
        double passiveCode = GetCodePerSecond() * dt;
        double passiveMoney = GetMoneyPerSecond() * dt;

        if (passiveCode > 0)
        {
            codeLines += passiveCode;
            totalCodeWritten += passiveCode;
        }

        if (passiveMoney > 0)
        {
            money += passiveMoney;
            totalMoneyEarned += passiveMoney;
        }

        if (passiveCode > 0 || passiveMoney > 0)
        {
            OnCurrenciesChanged?.Invoke();
        }

        // Автосохранение каждые N секунд
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

    #region Баланс и Расчеты

    /// <summary>
    /// Расчет строк кода за один клик (базовый + улучшения железа) * буст
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
        return (baseCodePerClick + hardwareBonus) * activeBoostMultiplier;
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
        return staffBonus * activeBoostMultiplier;
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
        return moneyIncome;
    }

    #endregion

    #region Действия Игрока

    /// <summary>
    /// Нажатие на экран / печать кода
    /// </summary>
    public void ClickCode(Vector2 clickPosition = default)
    {
        bool isCrit = UnityEngine.Random.value < critChance;
        double amount = GetCodePerClick() * (isCrit ? critMultiplier : 1.0);

        codeLines += amount;
        totalCodeWritten += amount;

        OnCodeClicked?.Invoke(amount, isCrit, clickPosition);
        OnCurrenciesChanged?.Invoke();
    }

    /// <summary>
    /// Покупка улучшения
    /// </summary>
    public bool BuyUpgrade(string upgradeId)
    {
        UpgradeItem upg = upgrades.Find(u => u.Id == upgradeId);
        if (upg == null) return false;

        // Улучшения покупаются за деньги (если хватает) или за строки кода
        // Железо и персонал берется за деньги
        if (upg.Buy(ref money))
        {
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
            money += prj.RewardMoney;
            totalMoneyEarned += prj.RewardMoney;

            OnProjectCompleted?.Invoke(prj);
            OnCurrenciesChanged?.Invoke();
            SaveGame();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Активация буста (например, энергетик за просмотр рекламы или покупку)
    /// </summary>
    public void ActivateEnergyBoost(float durationSeconds, double multiplier = 2.0)
    {
        energyBoostTimeRemaining = durationSeconds;
        activeBoostMultiplier = multiplier;
        OnCurrenciesChanged?.Invoke();
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
    }

    public void SaveGame()
    {
        PlayerPrefs.SetString(SaveKeyPrefix + "Code", codeLines.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "TotalCode", totalCodeWritten.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "Money", money.ToString("R"));
        PlayerPrefs.SetString(SaveKeyPrefix + "TotalMoney", totalMoneyEarned.ToString("R"));

        foreach (var upg in upgrades)
        {
            PlayerPrefs.SetInt(SaveKeyPrefix + "Upg_" + upg.Id, upg.CurrentLevel);
        }

        foreach (var prj in projects)
        {
            PlayerPrefs.SetInt(SaveKeyPrefix + "Prj_" + prj.Id, prj.IsCompleted ? 1 : 0);
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

            foreach (var upg in upgrades)
            {
                upg.CurrentLevel = PlayerPrefs.GetInt(SaveKeyPrefix + "Upg_" + upg.Id, 0);
            }

            foreach (var prj in projects)
            {
                prj.IsCompleted = PlayerPrefs.GetInt(SaveKeyPrefix + "Prj_" + prj.Id, 0) == 1;
            }
        }
    }

    public void ResetAllData()
    {
        codeLines = 0;
        totalCodeWritten = 0;
        money = 0;
        totalMoneyEarned = 0;
        energyBoostTimeRemaining = 0;
        activeBoostMultiplier = 1.0;

        foreach (var upg in upgrades)
        {
            upg.CurrentLevel = 0;
        }

        foreach (var prj in projects)
        {
            prj.IsCompleted = false;
        }

        SaveGame();
        OnCurrenciesChanged?.Invoke();
    }

    #endregion
}
