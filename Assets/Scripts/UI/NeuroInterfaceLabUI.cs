using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Научно-Исследовательский Институт Нейроинтерфейсов («Brain-Computer Interface Lab»):
/// - Разработка прямого подключения мыслей разработчика и игроков к кодовой базе и движку
/// - Прямая печать кода силой мысли со сверхсветовой скоростью
/// - 4 уровня нейроинтерфейсов:
///   1. 🧠 Non-Invasive EEG Headset (+30 C#/сек, +15% к клику)
///   2. ⚡ Subdural Cortical Mesh (+120 C#/сек, +20% к клику)
///   3. 🔬 Synaptic Optical Optogenetics (+450 C#/сек, +25% к клику)
///   4. 🌌 Direct Cortex Quantum Telepathy (+1800 C#/сек, x1.40 ко всему коду и клику)
/// - Интерактивное действие: «🧠 НЕЙРО-СИНХРОНИЗАЦИЯ МОЗГА» (100% комбо x3.0 и чистый поток кода)
/// - Мощный пассивный поток C#/сек и невероятный множитель к силе клика
/// </summary>
public class NeuroInterfaceLabUI : MonoBehaviour
{
    private static NeuroInterfaceLabUI instance;
    public static NeuroInterfaceLabUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<NeuroInterfaceLabUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(NeuroInterfaceLabUI));
                    instance = go.AddComponent<NeuroInterfaceLabUI>();
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
    public class NeuroTechTier
    {
        public string id;
        public string title;
        public string neuralInfo;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double codePerSecBonus;
        public float clickMultiplierBonus;
        public float incomeMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public double GetTotalCps() => codePerSecBonus * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openNeuroBtn;

    [Header("Панель нейролаборатории")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button syncBtn;
    [SerializeField] private TMP_Text syncBtnTxt;
    [SerializeField] private Transform techContainer;

    [Header("Нейро-модули")]
    [SerializeField] private List<NeuroTechTier> techTiers = new List<NeuroTechTier>();

    private const string PrefSyncCount = "Neuro_Sync_Count";
    private const string PrefTechLvlPrefix = "Neuro_Tech_Lvl_";

    private int neuralSyncsDoneCount = 0;
    private bool isSyncing = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitTechList();
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

    private void InitTechList()
    {
        if (techTiers != null && techTiers.Count > 0) return;

        techTiers = new List<NeuroTechTier>
        {
            new NeuroTechTier
            {
                id = "neuro_eeg_headset",
                title = "Non-Invasive EEG Headset",
                neuralInfo = "64 сенсора считывания альфа- и тета-волн коры мозга",
                icon = "🧠",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 9000.0,
                baseCostCode = 5500.0,
                codePerSecBonus = 30.0,
                clickMultiplierBonus = 0.15f,
                incomeMultiplierBonus = 0.08f
            },
            new NeuroTechTier
            {
                id = "neuro_cortical_mesh",
                title = "Subdural Cortical Mesh",
                neuralInfo = "Гибкая графеновая сетка для распознавания замысла кода",
                icon = "⚡",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 40000.0,
                baseCostCode = 26000.0,
                codePerSecBonus = 120.0,
                clickMultiplierBonus = 0.20f,
                incomeMultiplierBonus = 0.12f
            },
            new NeuroTechTier
            {
                id = "neuro_optogenetics",
                title = "Synaptic Optical Optogenetics",
                neuralInfo = "Лазерная стимуляция синапсов для мгновенной генерации фичей",
                icon = "🔬",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 180000.0,
                baseCostCode = 115000.0,
                codePerSecBonus = 450.0,
                clickMultiplierBonus = 0.25f,
                incomeMultiplierBonus = 0.18f
            },
            new NeuroTechTier
            {
                id = "neuro_quantum_telepathy",
                title = "Direct Cortex Telepathy",
                neuralInfo = "Прямой квантовый мост между мыслями и компилятором C#",
                icon = "🌌",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 750000.0,
                baseCostCode = 520000.0,
                codePerSecBonus = 1800.0,
                clickMultiplierBonus = 0.40f,
                incomeMultiplierBonus = 0.30f
            }
        };
    }

    private void LoadData()
    {
        neuralSyncsDoneCount = PlayerPrefs.GetInt(PrefSyncCount, 0);
        foreach (var t in techTiers)
        {
            if (PlayerPrefs.HasKey(PrefTechLvlPrefix + t.id))
            {
                t.level = PlayerPrefs.GetInt(PrefTechLvlPrefix + t.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSyncCount, neuralSyncsDoneCount);
        foreach (var t in techTiers)
        {
            PlayerPrefs.SetInt(PrefTechLvlPrefix + t.id, t.level);
        }
        PlayerPrefs.Save();
    }

    public double GetNeuroIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var t in techTiers)
        {
            mult += t.level * t.incomeMultiplierBonus;
        }
        return mult;
    }

    public double GetNeuroClickMultiplier()
    {
        double mult = 1.0;
        foreach (var t in techTiers)
        {
            mult += t.level * t.clickMultiplierBonus;
        }
        mult += Math.Min(neuralSyncsDoneCount * 0.02, 0.70);
        return mult;
    }

    public double GetNeuroCodePerSec()
    {
        double sum = 0;
        foreach (var t in techTiers)
        {
            sum += t.GetTotalCps();
        }
        return sum;
    }

    public void TriggerNeuralSync()
    {
        if (isSyncing) return;
        StartCoroutine(SyncRoutine());
    }

    private IEnumerator SyncRoutine()
    {
        isSyncing = true;
        if (syncBtn != null) syncBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (syncBtnTxt != null)
        {
            syncBtnTxt.text = "🧠 НЕЙРО-СИНХРОНИЗАЦИЯ... ПРЯМОЙ ПОТОК В НЕОКОРТЕКС!";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        neuralSyncsDoneCount++;
        double baseCode = Math.Max(40000.0, GetNeuroCodePerSec() * 60.0);
        double rewardCode = Math.Floor(baseCode * GetNeuroClickMultiplier());
        double rewardMoney = Math.Floor(rewardCode * 0.50 * GetNeuroIncomeMultiplier());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddComboEnergy(1.0f); // 100% комбо В Потоке (x3.0)
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🧠 НЕЙРО-РЕЗОНАНС!\nКОМБО 100% (x3.0)! <b>+{NumberFormatter.Format(rewardCode)} C#</b> (+{NumberFormatter.Format(rewardMoney)} ₽)", transform.position, new Color(0.85f, 0.4f, 1f), true);
        }

        isSyncing = false;
        if (syncBtn != null) syncBtn.interactable = true;
        if (syncBtnTxt != null) syncBtnTxt.text = "🧠 НЕЙРО-СИНХРОНИЗАЦИЯ МОЗГА";
    }

    public void UpgradeTech(string techId)
    {
        var tier = techTiers.Find(t => t.id == techId);
        if (tier == null) return;

        if (tier.level >= tier.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Нейроинтерфейс достиг максимальной точности!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = tier.GetCostMoney();
        double costCode = tier.GetCostCode();

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

        tier.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🧠 {tier.title} УРОВЕНЬ {tier.level}!\n+{tier.codePerSecBonus:F0} C#/сек • +{(tier.clickMultiplierBonus * 100):F0}% Клик", transform.position, new Color(0.85f, 0.45f, 1f), true);
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
            double totalCps = GetNeuroCodePerSec();
            double clkMult = GetNeuroClickMultiplier();
            double incMult = GetNeuroIncomeMultiplier();
            statsSummaryTxt.text = $"🧠 Нейро-генерация: <b>+{NumberFormatter.Format(totalCps)} C#/сек</b>\nСила клика: <b>x{clkMult:F2}</b> • Множитель дохода: <b>x{incMult:F2}</b> (Синхронизаций: {neuralSyncsDoneCount})";
        }

        if (techContainer != null)
        {
            for (int i = 0; i < techContainer.childCount; i++)
            {
                Transform child = techContainer.GetChild(i);
                if (i < techTiers.Count)
                {
                    UpdateTechCard(child, techTiers[i]);
                }
            }
        }
    }

    private void UpdateTechCard(Transform card, NeuroTechTier tier)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text infoTxt = card.Find("Header/InfoTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{tier.title} (Ур. {tier.level}/{tier.maxLevel})";
        if (infoTxt != null) infoTxt.text = tier.neuralInfo;
        if (iconTxt != null) iconTxt.text = tier.icon;
        if (descTxt != null) descTxt.text = $"+{tier.codePerSecBonus:F0} C#/сек • +{(tier.clickMultiplierBonus * 100):F0}% Клик • +{(tier.incomeMultiplierBonus * 100):F0}% Доход";
        if (statsTxt != null) statsTxt.text = $"Прямой поток: +{NumberFormatter.Format(tier.GetTotalCps())} C#/сек";

        if (upBtn != null && upBtnTxt != null)
        {
            if (tier.level >= tier.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = tier.GetCostMoney();
                double costCode = tier.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Имплантировать\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openNeuroBtn != null) openNeuroBtn.onClick.AddListener(OpenModal);
        if (syncBtn != null) syncBtn.onClick.AddListener(TriggerNeuralSync);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("NeuroLabModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.08f, 0.02f, 0.12f, 0.85f);
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
        cardImg.color = new Color(0.18f, 0.08f, 0.26f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "🧠 BRAIN-COMPUTER INTERFACE LAB";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.85f, 0.45f, 1f);
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
        subTxt.text = "Прямой нейроинтерфейс связи мозга с кодовой базой движка";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.9f, 0.8f, 1f);
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
        summaryBox.GetComponent<Image>().color = new Color(0.24f, 0.12f, 0.35f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.85f, 0.45f, 1f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Нейро-синхронизация)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.28f, 0.14f, 0.40f, 1f);

        GameObject syncBtnObj = new GameObject("SyncBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        syncBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform sbr = syncBtnObj.GetComponent<RectTransform>();
        sbr.anchorMin = Vector2.zero;
        sbr.anchorMax = Vector2.one;
        sbr.offsetMin = new Vector2(8, 6);
        sbr.offsetMax = new Vector2(-8, -6);
        syncBtnObj.GetComponent<Image>().color = new Color(0.75f, 0.30f, 0.95f, 1f);
        syncBtn = syncBtnObj.GetComponent<Button>();

        GameObject syncTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        syncTxtObj.transform.SetParent(syncBtnObj.transform, false);
        syncBtnTxt = syncTxtObj.GetComponent<TextMeshProUGUI>();
        syncBtnTxt.text = "🧠 НЕЙРО-СИНХРОНИЗАЦИЯ МОЗГА";
        syncBtnTxt.fontSize = 14;
        syncBtnTxt.fontStyle = FontStyles.Bold;
        syncBtnTxt.alignment = TextAlignmentOptions.Center;
        syncBtnTxt.color = Color.white;
        RectTransform str = syncTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = Vector2.zero;
        str.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("TechScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.12f, 0.05f, 0.18f, 0.5f);

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
        techContainer = content.transform;
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
        foreach (var t in techTiers)
        {
            CreateTechCardUI(content.transform, t);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.36f, 0.18f, 0.48f, 1f);
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
        GameObject launchBtnObj = new GameObject("NeuroLabLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, -154);
        launchBtnObj.GetComponent<Image>().color = new Color(0.75f, 0.30f, 0.95f, 0.95f);
        openNeuroBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "🧠";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateTechCardUI(Transform parent, NeuroTechTier tier)
    {
        GameObject card = new GameObject("TechCard_" + tier.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.24f, 0.12f, 0.32f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.36f, 0.18f, 0.48f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = tier.icon;
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

        GameObject info = new GameObject("InfoTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        info.transform.SetParent(header.transform, false);
        TextMeshProUGUI iTxt = info.GetComponent<TextMeshProUGUI>();
        iTxt.fontSize = 10;
        iTxt.color = new Color(0.85f, 0.55f, 1f);
        RectTransform ir = info.GetComponent<RectTransform>();
        ir.anchorMin = new Vector2(0f, 0f);
        ir.anchorMax = new Vector2(1f, 0.5f);
        ir.offsetMin = Vector2.zero;
        ir.offsetMax = Vector2.zero;

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
        dTxt.color = new Color(0.95f, 0.88f, 1f);
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
        stTxt.color = new Color(0.85f, 0.45f, 1f);
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
        upBtn.GetComponent<Image>().color = new Color(0.70f, 0.25f, 0.90f, 1f);

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

        string currentTechId = tier.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeTech(currentTechId));
    }
}
