using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Межпланетная Экспансия на Марс («Mars Colony Indie Division»):
/// - Первый межпланетный филиал игровой студии в куполе Olympus Mons
/// - Разработка научно-фантастических игр в условиях 0.38g марсианской гравитации
/// - 4 уровня марсианского подразделения:
///   1. 🔴 Pressurized Habitat Dome (+15% к доходу, +10% к клику)
///   2. 🛰️ Earth-Mars Laser Uplink (+20% к доходу, +15% к клику)
///   3. ⛏️ Olympus Mons Dev Biosphere (+30% к доходу, +20% к клику)
///   4. 🪐 Interplanetary Gaming Federation (x1.45 ко всему доходу студии)
/// - Интерактивное действие: «🔴 ПРИНЯТЬ МАРСИАНСКИЙ ТЕЛЕМЕТРИЧЕСКИЙ ПАКЕТ» (комбо +40% и технологии Марса)
/// - Космические множители ко всем финансовым потокам студии на Земле и Марсе
/// </summary>
public class MarsColonyStudioUI : MonoBehaviour
{
    private static MarsColonyStudioUI instance;
    public static MarsColonyStudioUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MarsColonyStudioUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(MarsColonyStudioUI));
                    instance = go.AddComponent<MarsColonyStudioUI>();
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
    public class MarsModuleTier
    {
        public string id;
        public string title;
        public string habitatSpecs;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.50, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.44, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openMarsBtn;

    [Header("Панель марсианского купола")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button telemetryBtn;
    [SerializeField] private TMP_Text telemetryBtnTxt;
    [SerializeField] private Transform modulesContainer;

    [Header("Модули Марса")]
    [SerializeField] private List<MarsModuleTier> modules = new List<MarsModuleTier>();

    private const string PrefPacketsCount = "Mars_Packets_Count";
    private const string PrefModuleLvlPrefix = "Mars_Module_Lvl_";

    private int telemetryPacketsCount = 0;
    private bool isReceivingPacket = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitModulesList();
        LoadData();
    }

    private void Start()
    {
        if (modalRoot == null)
        {
            BuildUI();
        }
        else
        {
            WireButtons();
            RefreshUI();
            if (modalRoot != null) modalRoot.SetActive(false);
        }
    }

    private void InitModulesList()
    {
        if (modules != null && modules.Count > 0) return;

        modules = new List<MarsModuleTier>
        {
            new MarsModuleTier
            {
                id = "mars_habitat_dome",
                title = "Pressurized Habitat Dome",
                habitatSpecs = "Герметичный геодезический купол на равнине Элизия • Кислородный сад",
                icon = "🔴",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 9500.0,
                baseCostCode = 6000.0,
                incomeMultiplierBonus = 0.15f,
                clickMultiplierBonus = 0.10f
            },
            new MarsModuleTier
            {
                id = "mars_laser_uplink",
                title = "Earth-Mars Laser Uplink",
                habitatSpecs = "Оптическая межпланетная линия связи Земля-Марс • Скорость 10 Gbps",
                icon = "🛰️",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 42000.0,
                baseCostCode = 28000.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.15f
            },
            new MarsModuleTier
            {
                id = "mars_olympus_biosphere",
                title = "Olympus Mons Biosphere",
                habitatSpecs = "Научный кластер на вершине вулкана Олимп • Нулевые космические шумы",
                icon = "⛏️",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 190000.0,
                baseCostCode = 125000.0,
                incomeMultiplierBonus = 0.30f,
                clickMultiplierBonus = 0.20f
            },
            new MarsModuleTier
            {
                id = "mars_interplanetary_fed",
                title = "Interplanetary Gaming Fed",
                habitatSpecs = "Межпланетная федерация киберспорта и гейминга Солнечной системы",
                icon = "🪐",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 800000.0,
                baseCostCode = 550000.0,
                incomeMultiplierBonus = 0.45f,
                clickMultiplierBonus = 0.30f
            }
        };
    }

    private void LoadData()
    {
        telemetryPacketsCount = PlayerPrefs.GetInt(PrefPacketsCount, 0);
        foreach (var m in modules)
        {
            if (PlayerPrefs.HasKey(PrefModuleLvlPrefix + m.id))
            {
                m.level = PlayerPrefs.GetInt(PrefModuleLvlPrefix + m.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefPacketsCount, telemetryPacketsCount);
        foreach (var m in modules)
        {
            PlayerPrefs.SetInt(PrefModuleLvlPrefix + m.id, m.level);
        }
        PlayerPrefs.Save();
    }

    public double GetMarsIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var m in modules)
        {
            mult += m.level * m.incomeMultiplierBonus;
        }
        mult += Math.Min(telemetryPacketsCount * 0.02, 0.70);
        return mult;
    }

    public double GetMarsClickMultiplier()
    {
        double mult = 1.0;
        foreach (var m in modules)
        {
            mult += m.level * m.clickMultiplierBonus;
        }
        return mult;
    }

    public void ReceiveTelemetryPacket()
    {
        if (isReceivingPacket) return;
        StartCoroutine(PacketRoutine());
    }

    private IEnumerator PacketRoutine()
    {
        isReceivingPacket = true;
        if (telemetryBtn != null) telemetryBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (telemetryBtnTxt != null)
        {
            telemetryBtnTxt.text = "🔴 ПРИЕМ ЛАЗЕРНОГО СИГНАЛА С ОЛИМПА МАРСА...";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        telemetryPacketsCount++;
        double rewardMoney = 65000.0 * GetMarsIncomeMultiplier();
        double rewardCode = 40000.0 * GetMarsClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.40f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🔴 ПАКЕТ С МАРСА ДОСТАВЛЕН!\nДанные колонии: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(1f, 0.45f, 0.25f), true);
        }

        isReceivingPacket = false;
        if (telemetryBtn != null) telemetryBtn.interactable = true;
        if (telemetryBtnTxt != null) telemetryBtnTxt.text = "🔴 ПРИНЯТЬ МАРСИАНСКИЙ ТЕЛЕМЕТРИЧЕСКИЙ ПАКЕТ";
    }

    public void UpgradeModule(string modId)
    {
        var mod = modules.Find(m => m.id == modId);
        if (mod == null) return;

        if (mod.level >= mod.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Марсианский модуль развернут на полную мощность!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = mod.GetCostMoney();
        double costCode = mod.GetCostCode();

        double curMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;
        double curCode = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;

        if (curMoney < costMoney || curCode < costCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(costMoney)} ₽ и {NumberFormatter.Format(costCode)} C#!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMoney(costMoney);
            GameManager.Instance.SpendLinesOfCode(costCode);
        }

        mod.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🔴 {mod.title} УРОВЕНЬ {mod.level}!\n+{(mod.incomeMultiplierBonus * 100):F0}% Доход • +{(mod.clickMultiplierBonus * 100):F0}% Клик", transform.position, new Color(1f, 0.5f, 0.25f), true);
        }
    }

    public void OpenModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            RefreshUI();
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = Vector3.one * 0.85f;
                StopAllCoroutines();
                StartCoroutine(AnimateModalOpen());
            }
            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null && modalRoot.activeSelf)
        {
            StartCoroutine(AnimateModalClose());
            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    private IEnumerator AnimateModalOpen()
    {
        float timer = 0f;
        float duration = 0.18f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float scale = Mathf.Lerp(0.85f, 1.0f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateModalClose()
    {
        float timer = 0f;
        float duration = 0.14f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float scale = Mathf.Lerp(1.0f, 0.85f, t);
            if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (modalRoot != null) modalRoot.SetActive(false);
    }

    public void RefreshUI()
    {
        if (statsSummaryTxt != null)
        {
            double incMult = GetMarsIncomeMultiplier();
            double clkMult = GetMarsClickMultiplier();
            statsSummaryTxt.text = $"🔴 Множитель Марса: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b>\n(Принято телепакетов: {telemetryPacketsCount})";
        }

        if (modulesContainer != null)
        {
            for (int i = 0; i < modulesContainer.childCount; i++)
            {
                Transform child = modulesContainer.GetChild(i);
                if (i < modules.Count)
                {
                    UpdateModuleCard(child, modules[i]);
                }
            }
        }
    }

    private void UpdateModuleCard(Transform card, MarsModuleTier mod)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text specsTxt = card.Find("Header/SpecsTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{mod.title} (Ур. {mod.level}/{mod.maxLevel})";
        if (specsTxt != null) specsTxt.text = mod.habitatSpecs;
        if (iconTxt != null) iconTxt.text = mod.icon;
        if (descTxt != null) descTxt.text = $"+{(mod.incomeMultiplierBonus * 100):F0}% Доход Студии • +{(mod.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущий вклад: +{(mod.level * mod.incomeMultiplierBonus * 100):F0}% доход / +{(mod.level * mod.clickMultiplierBonus * 100):F0}% клик";

        if (upBtn != null && upBtnTxt != null)
        {
            if (mod.level >= mod.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = mod.GetCostMoney();
                double costCode = mod.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Построить\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openMarsBtn != null) openMarsBtn.onClick.AddListener(OpenModal);
        if (telemetryBtn != null) telemetryBtn.onClick.AddListener(ReceiveTelemetryPacket);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("MarsColonyModalRoot", typeof(RectTransform));
        modalRoot.transform.SetParent(canvas.transform, false);
        RectTransform rtRoot = modalRoot.GetComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.offsetMin = Vector2.zero;
        rtRoot.offsetMax = Vector2.zero;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(modalRoot.transform, false);
        RectTransform rtBackdrop = backdrop.GetComponent<RectTransform>();
        rtBackdrop.anchorMin = Vector2.zero;
        rtBackdrop.anchorMax = Vector2.one;
        rtBackdrop.offsetMin = Vector2.zero;
        rtBackdrop.offsetMax = Vector2.zero;
        Image bgImg = backdrop.GetComponent<Image>();
        bgImg.color = new Color(0.12f, 0.04f, 0.02f, 0.85f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(modalRoot.transform, false);
        modalCardTransform = card.transform;
        RectTransform rtCard = card.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(480, 680);
        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.24f, 0.10f, 0.06f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "🔴 MARS COLONY INDIE DIVISION";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(1f, 0.45f, 0.25f);
        RectTransform hRect = headerObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(-40, 36);
        hRect.anchoredPosition = new Vector2(0, -14);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI subTxt = subObj.GetComponent<TextMeshProUGUI>();
        subTxt.text = "Марсианский филиал студии в куполе Olympus Mons и лазерный линк";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(1f, 0.85f, 0.75f);
        RectTransform sRect = subObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 1f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.sizeDelta = new Vector2(-40, 22);
        sRect.anchoredPosition = new Vector2(0, -48);

        // Close 'X' Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(card.transform, false);
        RectTransform xRect = closeXObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 1f);
        xRect.anchorMax = new Vector2(1f, 1f);
        xRect.pivot = new Vector2(1f, 1f);
        xRect.sizeDelta = new Vector2(34, 34);
        xRect.anchoredPosition = new Vector2(-12, -12);
        closeXObj.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(closeXObj.transform, false);
        TextMeshProUGUI xt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xt.text = "✕";
        xt.fontSize = 18;
        xt.fontStyle = FontStyles.Bold;
        xt.alignment = TextAlignmentOptions.Center;
        xt.color = Color.white;
        RectTransform xtr = xTxtObj.GetComponent<RectTransform>();
        xtr.anchorMin = Vector2.zero;
        xtr.anchorMax = Vector2.one;
        xtr.offsetMin = Vector2.zero;
        xtr.offsetMax = Vector2.zero;

        // Stats Summary Box
        GameObject summaryBox = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        summaryBox.transform.SetParent(card.transform, false);
        RectTransform sbRect = summaryBox.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0f, 1f);
        sbRect.anchorMax = new Vector2(1f, 1f);
        sbRect.pivot = new Vector2(0.5f, 1f);
        sbRect.sizeDelta = new Vector2(-30, 48);
        sbRect.anchoredPosition = new Vector2(0, -74);
        summaryBox.GetComponent<Image>().color = new Color(0.32f, 0.14f, 0.08f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(1f, 0.55f, 0.35f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Принять телеметрический пакет)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.34f, 0.16f, 0.10f, 1f);

        GameObject telBtnObj = new GameObject("TelemetryBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        telBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform tbR = telBtnObj.GetComponent<RectTransform>();
        tbR.anchorMin = Vector2.zero;
        tbR.anchorMax = Vector2.one;
        tbR.offsetMin = new Vector2(8, 6);
        tbR.offsetMax = new Vector2(-8, -6);
        telBtnObj.GetComponent<Image>().color = new Color(0.95f, 0.40f, 0.20f, 1f);
        telemetryBtn = telBtnObj.GetComponent<Button>();

        GameObject telTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        telTxtObj.transform.SetParent(telBtnObj.transform, false);
        telemetryBtnTxt = telTxtObj.GetComponent<TextMeshProUGUI>();
        telemetryBtnTxt.text = "🔴 ПРИНЯТЬ МАРСИАНСКИЙ ТЕЛЕМЕТРИЧЕСКИЙ ПАКЕТ";
        telemetryBtnTxt.fontSize = 13;
        telemetryBtnTxt.fontStyle = FontStyles.Bold;
        telemetryBtnTxt.alignment = TextAlignmentOptions.Center;
        telemetryBtnTxt.color = Color.white;
        RectTransform ttr = telTxtObj.GetComponent<RectTransform>();
        ttr.anchorMin = Vector2.zero;
        ttr.anchorMax = Vector2.one;
        ttr.offsetMin = Vector2.zero;
        ttr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("ModulesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.14f, 0.06f, 0.04f, 0.5f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vRect = viewport.GetComponent<RectTransform>();
        vRect.anchorMin = Vector2.zero;
        vRect.anchorMax = Vector2.one;
        vRect.offsetMin = Vector2.zero;
        vRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = Color.white;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        modulesContainer = content.transform;
        RectTransform cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0f, 1f);
        cRect.anchorMax = new Vector2(1f, 1f);
        cRect.pivot = new Vector2(0.5f, 1f);
        cRect.offsetMin = Vector2.zero;
        cRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = cRect;
        sr.viewport = vRect;
        sr.horizontal = false;
        sr.vertical = true;

        // Instantiate cards
        foreach (var m in modules)
        {
            CreateModuleCardUI(content.transform, m);
        }

        // Bottom Close Button
        GameObject botCloseObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 42);
        bcRect.anchoredPosition = new Vector2(0, 12);
        botCloseObj.GetComponent<Image>().color = new Color(0.40f, 0.20f, 0.12f, 1f);
        closeBtn = botCloseObj.GetComponent<Button>();

        GameObject bcTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bcTxtObj.transform.SetParent(botCloseObj.transform, false);
        TextMeshProUGUI bct = bcTxtObj.GetComponent<TextMeshProUGUI>();
        bct.text = "ЗАКРЫТЬ";
        bct.fontSize = 14;
        bct.fontStyle = FontStyles.Bold;
        bct.alignment = TextAlignmentOptions.Center;
        bct.color = Color.white;
        RectTransform bctr = bcTxtObj.GetComponent<RectTransform>();
        bctr.anchorMin = Vector2.zero;
        bctr.anchorMax = Vector2.one;
        bctr.offsetMin = Vector2.zero;
        bctr.offsetMax = Vector2.zero;

        // Launcher Floating Button
        CreateLauncherButton(canvas.transform);

        WireButtons();
        RefreshUI();
        modalRoot.SetActive(false);
    }

    private void CreateLauncherButton(Transform canvasTransform)
    {
        GameObject launchBtnObj = new GameObject("MarsColonyLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, -204);
        launchBtnObj.GetComponent<Image>().color = new Color(0.95f, 0.40f, 0.20f, 0.95f);
        openMarsBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "🔴";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateModuleCardUI(Transform parent, MarsModuleTier mod)
    {
        GameObject card = new GameObject("ModCard_" + mod.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.26f, 0.12f, 0.08f, 0.9f);

        LayoutElement le = card.GetComponent<LayoutElement>();
        le.preferredHeight = 94;
        le.minHeight = 94;

        // Icon Box
        GameObject iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image));
        iconBox.transform.SetParent(card.transform, false);
        RectTransform ibRect = iconBox.GetComponent<RectTransform>();
        ibRect.anchorMin = new Vector2(0f, 0.5f);
        ibRect.anchorMax = new Vector2(0f, 0.5f);
        ibRect.pivot = new Vector2(0f, 0.5f);
        ibRect.sizeDelta = new Vector2(50, 50);
        ibRect.anchoredPosition = new Vector2(8, 0);
        iconBox.GetComponent<Image>().color = new Color(0.40f, 0.20f, 0.12f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = mod.icon;
        it.fontSize = 26;
        it.alignment = TextAlignmentOptions.Center;
        RectTransform itr = iconTxt.GetComponent<RectTransform>();
        itr.anchorMin = Vector2.zero;
        itr.anchorMax = Vector2.one;
        itr.offsetMin = Vector2.zero;
        itr.offsetMax = Vector2.zero;

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform));
        header.transform.SetParent(card.transform, false);
        RectTransform hr = header.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(0.68f, 1f);
        hr.pivot = new Vector2(0f, 1f);
        hr.anchoredPosition = new Vector2(66, -6);
        hr.sizeDelta = new Vector2(0, 36);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 13;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = Color.white;
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0.5f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        GameObject specs = new GameObject("SpecsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        specs.transform.SetParent(header.transform, false);
        TextMeshProUGUI spTxt = specs.GetComponent<TextMeshProUGUI>();
        spTxt.fontSize = 10;
        spTxt.color = new Color(1f, 0.65f, 0.40f);
        RectTransform spr = specs.GetComponent<RectTransform>();
        spr.anchorMin = new Vector2(0f, 0f);
        spr.anchorMax = new Vector2(1f, 0.5f);
        spr.offsetMin = Vector2.zero;
        spr.offsetMax = Vector2.zero;

        // Body
        GameObject body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(card.transform, false);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(0.68f, 1f);
        br.offsetMin = new Vector2(66, 6);
        br.offsetMax = new Vector2(0, -42);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.color = new Color(1f, 0.90f, 0.85f);
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = new Vector2(0f, 0.45f);
        dr.anchorMax = new Vector2(1f, 1f);
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;

        GameObject stats = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stats.transform.SetParent(body.transform, false);
        TextMeshProUGUI stTxt = stats.GetComponent<TextMeshProUGUI>();
        stTxt.fontSize = 11;
        stTxt.fontStyle = FontStyles.Bold;
        stTxt.color = new Color(1f, 0.60f, 0.35f);
        RectTransform strr = stats.GetComponent<RectTransform>();
        strr.anchorMin = new Vector2(0f, 0f);
        strr.anchorMax = new Vector2(1f, 0.45f);
        strr.offsetMin = Vector2.zero;
        strr.offsetMax = Vector2.zero;

        // Action
        GameObject act = new GameObject("Action", typeof(RectTransform));
        act.transform.SetParent(card.transform, false);
        RectTransform ar = act.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.68f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 8);
        ar.offsetMax = new Vector2(-8, -8);

        GameObject upBtn = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        upBtn.transform.SetParent(act.transform, false);
        RectTransform ubr = upBtn.GetComponent<RectTransform>();
        ubr.anchorMin = Vector2.zero;
        ubr.anchorMax = Vector2.one;
        ubr.offsetMin = Vector2.zero;
        ubr.offsetMax = Vector2.zero;
        upBtn.GetComponent<Image>().color = new Color(0.90f, 0.40f, 0.20f, 1f);

        GameObject upTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        upTxt.transform.SetParent(upBtn.transform, false);
        TextMeshProUGUI ut = upTxt.GetComponent<TextMeshProUGUI>();
        ut.fontSize = 11;
        ut.fontStyle = FontStyles.Bold;
        ut.alignment = TextAlignmentOptions.Center;
        ut.color = Color.white;
        RectTransform utr = upTxt.GetComponent<RectTransform>();
        utr.anchorMin = Vector2.zero;
        utr.anchorMax = Vector2.one;
        utr.offsetMin = Vector2.zero;
        utr.offsetMax = Vector2.zero;

        string currentModId = mod.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeModule(currentModId));
    }
}
