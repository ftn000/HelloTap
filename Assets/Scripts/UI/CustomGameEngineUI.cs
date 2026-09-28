using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Собственный Движок Студии «TapEngine» (Custom Game Engine):
/// - 6 модулей движка: Рендеринг, Физика, ИИ, Сетевой стек, Аудиоядро, Шейдеры
/// - Эволюция версий: v1.0 ScriptBasic -> v2.0 PixelForge -> v3.0 VoxelCraft -> v4.0 HyperDrive -> v5.0 QuantumNext
/// - Полный отказ от сторонних лицензий и роялти (буст чистого дохода студии)
/// - Множитель x2 к скорости релизов и увеличение строк кода за клик
/// - Сохранение в PlayerPrefs, тактильный отклик и звуковое сопровождение
/// </summary>
public class CustomGameEngineUI : MonoBehaviour
{
    private static CustomGameEngineUI instance;
    public static CustomGameEngineUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CustomGameEngineUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CustomGameEngineUI));
                    instance = go.AddComponent<CustomGameEngineUI>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    public class EngineModule
    {
        public string id;
        public string title;
        public string icon;
        public string desc;
        public int level;
        public int maxLevel;
        public double baseCostCode;
        public double baseCostMoney;

        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.45, level));
        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.5, level));
    }

    [System.Serializable]
    public class EngineVersionInfo
    {
        public int versionNumber;
        public string versionName;
        public string codeName;
        public int minModuleLevelRequired;
        public double compileCodeCost;
        public double incomeBonusPercent;
        public double releaseSpeedBonusPercent;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openEngineBtn;

    [Header("Заголовки и статус движка")]
    [SerializeField] private TMP_Text engineVersionHeaderTxt;
    [SerializeField] private TMP_Text enginePerksSummaryTxt;
    [SerializeField] private Button compileVersionBtn;
    [SerializeField] private TMP_Text compileVersionBtnTxt;
    [SerializeField] private Transform modulesContainer;

    private readonly List<EngineModule> modules = new List<EngineModule>();
    private readonly List<EngineVersionInfo> versions = new List<EngineVersionInfo>();
    private int currentVersionIndex = 0; // 0 = v1.0, 1 = v2.0, ...

    private const string PrefEngineVersion = "TapEngine_CurrentVersion";
    private const string PrefModulePrefix = "TapEngine_ModLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CurrentMajorVersion => currentVersionIndex + 1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeModules();
        InitializeVersions();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeModules()
    {
        modules.Clear();

        modules.Add(new EngineModule
        {
            id = "renderer",
            title = "Рендеринг (Vulkan / Metal)",
            icon = "🎨",
            desc = "+5% к скорости релизов и сочности графики за уровень",
            level = 0,
            maxLevel = 10,
            baseCostCode = 120,
            baseCostMoney = 500
        });

        modules.Add(new EngineModule
        {
            id = "physics",
            title = "Физика (RigidBody / Ragdoll)",
            icon = "⚙️",
            desc = "+6% строк кода за клик за уровень",
            level = 0,
            maxLevel = 10,
            baseCostCode = 180,
            baseCostMoney = 800
        });

        modules.Add(new EngineModule
        {
            id = "ai",
            title = "Искусственный Интеллект (NavMesh/BT)",
            icon = "🤖",
            desc = "+4% к пассивному доходу строк кода в секунду",
            level = 0,
            maxLevel = 10,
            baseCostCode = 250,
            baseCostMoney = 1200
        });

        modules.Add(new EngineModule
        {
            id = "netcode",
            title = "Сетевой Стек (Rollback Netcode)",
            icon = "🌐",
            desc = "+5% чистого дохода студии (без серверных лагов)",
            level = 0,
            maxLevel = 10,
            baseCostCode = 350,
            baseCostMoney = 2000
        });

        modules.Add(new EngineModule
        {
            id = "audio",
            title = "Аудио DSP Ядро (Spatial Sound)",
            icon = "🎧",
            desc = "+4% к наградам за релизы и атмосфере студии",
            level = 0,
            maxLevel = 10,
            baseCostCode = 150,
            baseCostMoney = 750
        });

        modules.Add(new EngineModule
        {
            id = "shaders",
            title = "Шейдерная Кузница (Ray Tracing)",
            icon = "✨",
            desc = "+7% к оценкам критиков и финальным выплатам",
            level = 0,
            maxLevel = 10,
            baseCostCode = 400,
            baseCostMoney = 2500
        });
    }

    private void InitializeVersions()
    {
        versions.Clear();

        versions.Add(new EngineVersionInfo
        {
            versionNumber = 1,
            versionName = "v1.0 «ScriptBasic»",
            codeName = "Первые пробы архитектуры",
            minModuleLevelRequired = 0,
            compileCodeCost = 0,
            incomeBonusPercent = 10.0,
            releaseSpeedBonusPercent = 15.0
        });

        versions.Add(new EngineVersionInfo
        {
            versionNumber = 2,
            versionName = "v2.0 «PixelForge»",
            codeName = "Отказ от роялти Unity/Unreal",
            minModuleLevelRequired = 2,
            compileCodeCost = 1500,
            incomeBonusPercent = 25.0,
            releaseSpeedBonusPercent = 35.0
        });

        versions.Add(new EngineVersionInfo
        {
            versionNumber = 3,
            versionName = "v3.0 «VoxelCraft»",
            codeName = "Собственный воксельный пайплайн",
            minModuleLevelRequired = 4,
            compileCodeCost = 6000,
            incomeBonusPercent = 50.0,
            releaseSpeedBonusPercent = 60.0
        });

        versions.Add(new EngineVersionInfo
        {
            versionNumber = 4,
            versionName = "v4.0 «HyperDrive»",
            codeName = "Многопоточный ECS и безлаговый стек",
            minModuleLevelRequired = 6,
            compileCodeCost = 25000,
            incomeBonusPercent = 80.0,
            releaseSpeedBonusPercent = 85.0
        });

        versions.Add(new EngineVersionInfo
        {
            versionNumber = 5,
            versionName = "v5.0 «QuantumNext»",
            codeName = "Нейросетевой рендеринг Next-Gen",
            minModuleLevelRequired = 8,
            compileCodeCost = 100000,
            incomeBonusPercent = 120.0,
            releaseSpeedBonusPercent = 100.0 // x2.0 скорость релизов!
        });
    }

    private void LoadData()
    {
        currentVersionIndex = PlayerPrefs.GetInt(PrefEngineVersion, 0);
        currentVersionIndex = Mathf.Clamp(currentVersionIndex, 0, versions.Count - 1);

        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].level = PlayerPrefs.GetInt(PrefModulePrefix + modules[i].id, 0);
            modules[i].level = Mathf.Clamp(modules[i].level, 0, modules[i].maxLevel);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefEngineVersion, currentVersionIndex);
        for (int i = 0; i < modules.Count; i++)
        {
            PlayerPrefs.SetInt(PrefModulePrefix + modules[i].id, modules[i].level);
        }
        PlayerPrefs.Save();
    }

    public double GetEngineIncomeMultiplier()
    {
        var curVer = versions[Mathf.Clamp(currentVersionIndex, 0, versions.Count - 1)];
        double versionBonus = 1.0 + (curVer.incomeBonusPercent / 100.0);
        double netcodeBonus = 1.0 + (GetModuleLevel("netcode") * 0.05);
        double aiBonus = 1.0 + (GetModuleLevel("ai") * 0.04);
        return versionBonus * netcodeBonus * aiBonus;
    }

    public double GetEngineClickMultiplier()
    {
        double physicsBonus = 1.0 + (GetModuleLevel("physics") * 0.06);
        double renderBonus = 1.0 + (GetModuleLevel("renderer") * 0.03);
        return physicsBonus * renderBonus;
    }

    public double GetProjectReleaseSpeedMultiplier()
    {
        var curVer = versions[Mathf.Clamp(currentVersionIndex, 0, versions.Count - 1)];
        return 1.0 + (curVer.releaseSpeedBonusPercent / 100.0);
    }

    public double GetProjectRewardMultiplier()
    {
        double shaderBonus = 1.0 + (GetModuleLevel("shaders") * 0.07);
        double audioBonus = 1.0 + (GetModuleLevel("audio") * 0.04);
        return shaderBonus * audioBonus;
    }

    public int GetModuleLevel(string id)
    {
        var mod = modules.Find(m => m.id == id);
        return mod != null ? mod.level : 0;
    }

    public bool TryUpgradeModule(string id)
    {
        var mod = modules.Find(m => m.id == id);
        if (mod == null || mod.level >= mod.maxLevel) return false;

        double costCode = mod.GetCostCode();
        double costMoney = mod.GetCostMoney();

        if (GameManager.Instance == null) return false;

        if (GameManager.Instance.CodeLines < costCode || GameManager.Instance.Money < costMoney)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(costCode)} кода и {NumberFormatter.Format(costMoney)} ₽", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendLinesOfCode(costCode);
        GameManager.Instance.SpendMoney(costMoney);

        mod.level++;
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛠️ МОДУЛЬ ПРОКАЧАН!\n{mod.icon} {mod.title} -> ур. {mod.level}", transform.position, new Color(0.2f, 0.9f, 1f), true);
        }

        UpdateModalUI();
        return true;
    }

    public bool TryCompileNextVersion()
    {
        if (currentVersionIndex >= versions.Count - 1) return false;

        var nextVer = versions[currentVersionIndex + 1];

        // Проверяем минимальный уровень всех модулей
        foreach (var mod in modules)
        {
            if (mod.level < nextVer.minModuleLevelRequired)
            {
                HapticFeedback.LightImpact();
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"⚠️ Все модули должны быть минимум ур. {nextVer.minModuleLevelRequired}!", transform.position, Color.yellow, false);
                }
                return false;
            }
        }

        if (GameManager.Instance == null || GameManager.Instance.CodeLines < nextVer.compileCodeCost)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(nextVer.compileCodeCost)} строк кода для компиляции ядра!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendLinesOfCode(nextVer.compileCodeCost);
        currentVersionIndex++;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🚀 СКОМПИЛИРОВАН {nextVer.versionName}!\nСкорость релизов: +{nextVer.releaseSpeedBonusPercent:F0}%\nДоход студии: +{nextVer.incomeBonusPercent:F0}%", transform.position, new Color(0.2f, 1f, 0.5f), true);
        }

        UpdateModalUI();
        return true;
    }

    public void OpenModal()
    {
        EnsureUIExists();
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.88f, 0.88f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }
        }
        UpdateModalUI();
        HapticFeedback.Vibrate(20);
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.Vibrate(15);
    }

    private IEnumerator PopCardAnim(Transform card)
    {
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.18f);
            float scale = Mathf.Lerp(0.88f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    private void BindButtons()
    {
        if (openEngineBtn != null)
        {
            openEngineBtn.onClick.RemoveAllListeners();
            openEngineBtn.onClick.AddListener(OpenModal);
        }
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(CloseModal);
        }
        if (closeXBtn != null)
        {
            closeXBtn.onClick.RemoveAllListeners();
            closeXBtn.onClick.AddListener(CloseModal);
        }
        if (backdropBtn != null)
        {
            backdropBtn.onClick.RemoveAllListeners();
            backdropBtn.onClick.AddListener(CloseModal);
        }
        if (compileVersionBtn != null)
        {
            compileVersionBtn.onClick.RemoveAllListeners();
            compileVersionBtn.onClick.AddListener(() => TryCompileNextVersion());
        }
    }

    private void UpdateModalUI()
    {
        var curVer = versions[Mathf.Clamp(currentVersionIndex, 0, versions.Count - 1)];

        if (engineVersionHeaderTxt != null)
        {
            engineVersionHeaderTxt.text = $"⚙️ ДВИЖОК «TAPENGINE» <color=#00E5FF>{curVer.versionName}</color>";
        }

        if (enginePerksSummaryTxt != null)
        {
            double curIncome = (GetEngineIncomeMultiplier() - 1.0) * 100.0;
            double curSpeed = (GetProjectReleaseSpeedMultiplier() - 1.0) * 100.0;
            double curClick = (GetEngineClickMultiplier() - 1.0) * 100.0;
            enginePerksSummaryTxt.text = $"Кодовое имя: <i>{curVer.codeName}</i>\n" +
                                        $"Скорость релизов: <b><color=#00FF88>+{curSpeed:F0}%</color></b> | Доход: <b><color=#00E5FF>+{curIncome:F0}%</color></b> | Клик: <b><color=#FFD700>+{curClick:F0}%</color></b>";
        }

        if (compileVersionBtn != null && compileVersionBtnTxt != null)
        {
            if (currentVersionIndex >= versions.Count - 1)
            {
                compileVersionBtn.interactable = false;
                compileVersionBtnTxt.text = "⭐ ДВИЖОК ДОСТИГ ВЕРШИНЫ (NEXT-GEN)";
            }
            else
            {
                var nextVer = versions[currentVersionIndex + 1];
                bool canCompile = true;
                foreach (var mod in modules)
                {
                    if (mod.level < nextVer.minModuleLevelRequired) { canCompile = false; break; }
                }
                if (GameManager.Instance != null && GameManager.Instance.CodeLines < nextVer.compileCodeCost)
                {
                    canCompile = false;
                }

                compileVersionBtn.interactable = canCompile;
                compileVersionBtnTxt.text = $"СКОМПИЛИРОВАТЬ {nextVer.versionName} ({NumberFormatter.Format(nextVer.compileCodeCost)} Кода)\n<size=10>Требуется ур. {nextVer.minModuleLevelRequired} для всех модулей</size>";
            }
        }

        RefreshModuleCards();
    }

    private void RefreshModuleCards()
    {
        if (modulesContainer == null) return;

        for (int i = 0; i < modules.Count; i++)
        {
            var mod = modules[i];
            Transform cardTr = modulesContainer.Find($"ModuleCard_{mod.id}");
            if (cardTr == null) continue;

            TMP_Text levelTxt = cardTr.Find("LevelTxt")?.GetComponent<TMP_Text>();
            if (levelTxt != null)
            {
                levelTxt.text = mod.level >= mod.maxLevel ? "<color=#FFD700>МАКС</color>" : $"Ур. {mod.level}/{mod.maxLevel}";
            }

            Button upgBtn = cardTr.Find("UpgradeBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = upgBtn != null ? upgBtn.GetComponentInChildren<TMP_Text>() : null;
            if (upgBtn != null && btnTxt != null)
            {
                string modId = mod.id;
                upgBtn.onClick.RemoveAllListeners();
                upgBtn.onClick.AddListener(() => TryUpgradeModule(modId));

                if (mod.level >= mod.maxLevel)
                {
                    upgBtn.interactable = false;
                    btnTxt.text = "МАКС. УРОВЕНЬ";
                }
                else
                {
                    double costCode = mod.GetCostCode();
                    double costMoney = mod.GetCostMoney();
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.CodeLines >= costCode &&
                                     GameManager.Instance.Money >= costMoney;
                    upgBtn.interactable = canAfford;
                    btnTxt.text = $"ПРОКАЧАТЬ\n{NumberFormatter.Format(costCode)} <color=#00E5FF>Кода</color> | {NumberFormatter.Format(costMoney)} ₽";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Создаем кнопку открытия в плавающей панели студии, если нет
        EnsureFloatingButton(canvas);

        // Корневой объект модального окна
        GameObject root = new GameObject("TapEngineModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        // Backdrop
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(500, 680);
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.9f, 1f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -14);
        headerRt.sizeDelta = new Vector2(-40, 32);
        engineVersionHeaderTxt = headerObj.GetComponent<TextMeshProUGUI>();
        engineVersionHeaderTxt.text = "⚙️ ДВИЖОК «TAPENGINE»";
        engineVersionHeaderTxt.fontSize = 17;
        engineVersionHeaderTxt.fontStyle = FontStyles.Bold;
        engineVersionHeaderTxt.alignment = TextAlignmentOptions.Center;
        engineVersionHeaderTxt.color = new Color(0.1f, 0.9f, 1f);

        // Close X Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeXRt = closeXObj.GetComponent<RectTransform>();
        closeXRt.anchorMin = new Vector2(1, 1);
        closeXRt.anchorMax = new Vector2(1, 1);
        closeXRt.pivot = new Vector2(1, 1);
        closeXRt.anchoredPosition = new Vector2(-12, -12);
        closeXRt.sizeDelta = new Vector2(32, 32);
        closeXObj.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 0.9f);
        closeXBtn = closeXObj.GetComponent<Button>();
        GameObject xTxtObj = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(closeXObj.transform, false);
        RectTransform xTxtRt = xTxtObj.GetComponent<RectTransform>();
        xTxtRt.anchorMin = Vector2.zero; xTxtRt.anchorMax = Vector2.one; xTxtRt.sizeDelta = Vector2.zero;
        var xt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xt.text = "✕"; xt.fontSize = 16; xt.fontStyle = FontStyles.Bold; xt.alignment = TextAlignmentOptions.Center; xt.color = Color.white;

        // Perks Summary Box
        GameObject summaryObj = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        summaryObj.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = summaryObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 54);
        summaryObj.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.2f, 0.85f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryObj.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-16, -6);
        enginePerksSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        enginePerksSummaryTxt.fontSize = 11;
        enginePerksSummaryTxt.alignment = TextAlignmentOptions.Center;
        enginePerksSummaryTxt.color = new Color(0.9f, 0.92f, 0.95f);

        // Compile Next Version Button
        GameObject compObj = new GameObject("CompileBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        compObj.transform.SetParent(cardObj.transform, false);
        RectTransform compRt = compObj.GetComponent<RectTransform>();
        compRt.anchorMin = new Vector2(0, 1);
        compRt.anchorMax = new Vector2(1, 1);
        compRt.pivot = new Vector2(0.5f, 1);
        compRt.anchoredPosition = new Vector2(0, -114);
        compRt.sizeDelta = new Vector2(-36, 44);
        compObj.GetComponent<Image>().color = new Color(0.1f, 0.5f, 0.8f, 0.95f);
        compileVersionBtn = compObj.GetComponent<Button>();

        GameObject compTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        compTxtObj.transform.SetParent(compObj.transform, false);
        RectTransform compTxtRt = compTxtObj.GetComponent<RectTransform>();
        compTxtRt.anchorMin = Vector2.zero; compTxtRt.anchorMax = Vector2.one; compTxtRt.sizeDelta = Vector2.zero;
        compileVersionBtnTxt = compTxtObj.GetComponent<TextMeshProUGUI>();
        compileVersionBtnTxt.fontSize = 12;
        compileVersionBtnTxt.fontStyle = FontStyles.Bold;
        compileVersionBtnTxt.alignment = TextAlignmentOptions.Center;
        compileVersionBtnTxt.color = Color.white;

        // Scroll Container for Modules
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -168);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;
        modulesContainer = contentObj.transform;

        // Create cards for each module
        for (int i = 0; i < modules.Count; i++)
        {
            CreateModuleCardTemplate(modulesContainer, modules[i]);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 12);
        closeBtnRt.sizeDelta = new Vector2(-40, 38);
        closeBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.3f, 0.95f);
        closeBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one; closeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "ЗАКРЫТЬ";
        closeTxt.fontSize = 13;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
        BindButtons();
    }

    private void CreateModuleCardTemplate(Transform parent, EngineModule mod)
    {
        GameObject card = new GameObject($"ModuleCard_{mod.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 70);
        card.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

        // Title and icon
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(0.65f, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(0, 20);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{mod.icon} <b>{mod.title}</b>";
        tt.fontSize = 12;
        tt.color = new Color(0.9f, 0.95f, 1f);

        // Level text
        GameObject lvlObj = new GameObject("LevelTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlObj.transform.SetParent(card.transform, false);
        RectTransform lvlRt = lvlObj.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0.65f, 1);
        lvlRt.anchorMax = new Vector2(1, 1);
        lvlRt.pivot = new Vector2(1, 1);
        lvlRt.anchoredPosition = new Vector2(-10, -6);
        lvlRt.sizeDelta = new Vector2(0, 20);
        TMP_Text lt = lvlObj.GetComponent<TextMeshProUGUI>();
        lt.text = $"Ур. {mod.level}/{mod.maxLevel}";
        lt.fontSize = 11;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = new Color(0.2f, 0.9f, 1f);

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.6f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"<color=#80A0C0>{mod.desc}</color>";
        dt.fontSize = 10;

        // Upgrade button
        GameObject b = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, -4);
        brt.sizeDelta = new Vector2(160, 36);
        b.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.65f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ПРОКАЧАТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openEngineBtn != null) return;

        // Ищем контейнер быстрых кнопок или создаем
        Transform hubBar = canvas.transform.Find("StudioHubBar");
        if (hubBar == null)
        {
            GameObject hubObj = new GameObject("StudioHubBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hubObj.transform.SetParent(canvas.transform, false);
            RectTransform hubRt = hubObj.GetComponent<RectTransform>();
            hubRt.anchorMin = new Vector2(0.5f, 1f);
            hubRt.anchorMax = new Vector2(0.5f, 1f);
            hubRt.pivot = new Vector2(0.5f, 1f);
            hubRt.anchoredPosition = new Vector2(0, -96);
            hubRt.sizeDelta = new Vector2(460, 32);

            var hlg = hubObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hubBar = hubObj.transform;
        }

        GameObject btnGo = new GameObject("EngineHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.1f, 0.25f, 0.4f, 0.9f);
        openEngineBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "⚙️ TapEngine";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
