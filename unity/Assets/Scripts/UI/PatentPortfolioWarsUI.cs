using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальные Патентные Войны («Patent Portfolio & Tech Monopolies»):
/// - Регистрация и монетизация 4 ключевых патентов студии:
///   1. ⚡ Quantum LZ4 Texture Streaming (Сжатие текстур)
///   2. 🌐 Deterministic Rollback Netcode (Сетевой неткод без лагов)
///   3. 🌌 Infinite Voxel Geometry Culling (Потоковый рендер)
///   4. 🧠 Motion Neural Physics Blend (Нейро-физика анимаций)
/// - Лицензионные роялти: постоянные отчисления с продаж сторонних проектов по лицензии
/// - Интерактивное действие: «⚖️ ВЫИГРАТЬ ПАТЕНТНЫЙ ИСК У ПИРАТОВ» (судебная компенсация + комбо)
/// - Перманентный множитель технологической монополии к доходу и силе клика
/// </summary>
public class PatentPortfolioWarsUI : MonoBehaviour
{
    private static PatentPortfolioWarsUI instance;
    public static PatentPortfolioWarsUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PatentPortfolioWarsUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(PatentPortfolioWarsUI));
                    instance = go.AddComponent<PatentPortfolioWarsUI>();
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
    public class StudioPatent
    {
        public string id;
        public string title;
        public string techField;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openPatentsBtn;

    [Header("Судебные иски и патенты")]
    [SerializeField] private TMP_Text patentStatsSummaryTxt;
    [SerializeField] private Button lawsuitBtn;
    [SerializeField] private TMP_Text lawsuitBtnTxt;
    [SerializeField] private Transform patentsContainer;

    private readonly List<StudioPatent> patents = new List<StudioPatent>();
    private bool isLawsuitActive = false;
    private int wonLawsuitsCount = 0;

    private const string PrefPatentLvlPrefix = "Patent_Lvl_";
    private const string PrefLawsuitsCount = "Patent_LawsuitsCount";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializePatents();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializePatents()
    {
        if (patents.Count > 0) return;

        patents.Add(new StudioPatent
        {
            id = "patent_lz4_textures",
            title = "Quantum LZ4 Texture Streaming",
            techField = "Сжатие и потоковая загрузка ассетов",
            icon = "⚡",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 45000,
            baseCostCode = 20000,
            incomeMultiplierBonus = 0.08f,
            clickMultiplierBonus = 0.05f
        });

        patents.Add(new StudioPatent
        {
            id = "patent_rollback_netcode",
            title = "Deterministic Rollback Netcode",
            techField = "Сетевой мультиплеер нулевой задержки",
            icon = "🌐",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 150000,
            baseCostCode = 75000,
            incomeMultiplierBonus = 0.15f,
            clickMultiplierBonus = 0.10f
        });

        patents.Add(new StudioPatent
        {
            id = "patent_infinite_voxel",
            title = "Infinite Voxel Geometry Culling",
            techField = "Процедурная оптимизация геометрии",
            icon = "🌌",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 480000,
            baseCostCode = 240000,
            incomeMultiplierBonus = 0.22f,
            clickMultiplierBonus = 0.15f
        });

        patents.Add(new StudioPatent
        {
            id = "patent_neural_physics",
            title = "Motion Neural Physics Blend",
            techField = "Нейросетевая физика процедурных анимаций",
            icon = "🧠",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1500000,
            baseCostCode = 750000,
            incomeMultiplierBonus = 0.32f,
            clickMultiplierBonus = 0.25f
        });
    }

    private void LoadData()
    {
        wonLawsuitsCount = PlayerPrefs.GetInt(PrefLawsuitsCount, 0);
        foreach (var p in patents)
        {
            if (PlayerPrefs.HasKey(PrefPatentLvlPrefix + p.id))
            {
                p.level = PlayerPrefs.GetInt(PrefPatentLvlPrefix + p.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefLawsuitsCount, wonLawsuitsCount);
        foreach (var p in patents)
        {
            PlayerPrefs.SetInt(PrefPatentLvlPrefix + p.id, p.level);
        }
        PlayerPrefs.Save();
    }

    public double GetPatentIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var p in patents)
        {
            mult += p.level * p.incomeMultiplierBonus;
        }
        mult += Math.Min(wonLawsuitsCount * 0.02, 0.80);
        return mult;
    }

    public double GetPatentClickMultiplier()
    {
        double mult = 1.0;
        foreach (var p in patents)
        {
            mult += p.level * p.clickMultiplierBonus;
        }
        return mult;
    }

    public void DefendPatentsInCourt()
    {
        if (isLawsuitActive) return;
        StartCoroutine(CourtLawsuitRoutine());
    }

    private IEnumerator CourtLawsuitRoutine()
    {
        isLawsuitActive = true;
        if (lawsuitBtn != null) lawsuitBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (lawsuitBtnTxt != null)
        {
            lawsuitBtnTxt.text = "⚖️ СУДЕБНОЕ ЗАСЕДАНИЕ: РАССМОТРЕНИЕ УЛИК ПЛАГИАТА...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        wonLawsuitsCount++;
        double rewardMoney = 35000.0 * GetPatentIncomeMultiplier();
        double rewardCode = 16000.0 * GetPatentClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.35f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"⚖️ ИСК ВЫИГРАН!\nПираты выплатили штраф: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.2f, 1f, 0.5f), true);
        }

        isLawsuitActive = false;
        if (lawsuitBtn != null) lawsuitBtn.interactable = true;
        if (lawsuitBtnTxt != null) lawsuitBtnTxt.text = "⚖️ ВЫИГРАТЬ СУДЕБНЫЙ ИСК У ПИРАТОВ (Штраф)";
    }

    public void UpgradePatent(string patentId)
    {
        var patent = patents.Find(p => p.id == patentId);
        if (patent == null) return;

        if (patent.level >= patent.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Патент уже расширен до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = patent.GetCostMoney();
        double costCode = patent.GetCostCode();

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

        patent.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Патент '{patent.title}' расширен до Ур.{patent.level}!\nРоялти: +{(patent.incomeMultiplierBonus * 100):0}%", transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
    }

    public void OpenModal()
    {
        EnsureUIExists();
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            RefreshUI();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();
            HapticFeedback.LightImpact();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    public void ToggleModal()
    {
        if (IsModalOpen) CloseModal();
        else OpenModal();
    }

    private void BindButtons()
    {
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
        if (lawsuitBtn != null)
        {
            lawsuitBtn.onClick.RemoveAllListeners();
            lawsuitBtn.onClick.AddListener(DefendPatentsInCourt);
        }
        if (openPatentsBtn != null)
        {
            openPatentsBtn.onClick.RemoveAllListeners();
            openPatentsBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (patentStatsSummaryTxt != null)
        {
            double incMult = GetPatentIncomeMultiplier();
            double clkMult = GetPatentClickMultiplier();
            patentStatsSummaryTxt.text = $"Патентный доход и роялти: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>\nВыиграно судебных дел: <b>{wonLawsuitsCount}</b>";
        }

        if (lawsuitBtnTxt != null && !isLawsuitActive)
        {
            lawsuitBtnTxt.text = "⚖️ ВЫИГРАТЬ СУДЕБНЫЙ ИСК У ПИРАТОВ (Штраф)";
        }

        if (patentsContainer == null) return;

        for (int i = 0; i < patents.Count; i++)
        {
            var p = patents[i];
            Transform child = i < patentsContainer.childCount ? patentsContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{p.icon} {p.title} (Ур. {p.level}/{p.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = p.techField;
            }
            if (statsTxt != null)
            {
                float totalInc = p.level * p.incomeMultiplierBonus * 100f;
                float totalClk = p.level * p.clickMultiplierBonus * 100f;
                statsTxt.text = $"Роялти: <color=#00FFAA>+{totalInc:0}% доход</color> | <color=#FFD700>+{totalClk:0}% клик</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (p.level >= p.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = p.GetCostMoney();
                    double c = p.GetCostCode();
                    upBtnTxt.text = $"Продлить\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
                    upBtn.interactable = true;
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("PatentPortfolio_ModalRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        modalRoot = root;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(root.transform, false);
        RectTransform bRect = backdrop.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        Image bImg = backdrop.GetComponent<Image>();
        bImg.color = new Color(0.04f, 0.04f, 0.07f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Card
        GameObject card = new GameObject("PatentCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.12f, 0.13f, 0.19f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.30f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "⚖️ ПАТЕНТНОЕ ПОРТФОЛИО & СУДЫ";
        tTxt.fontSize = 19;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(1f, 0.85f, 0.25f);
        RectTransform tRect = titleObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = new Vector2(20, 0);
        tRect.offsetMax = new Vector2(-70, 0);

        // Close X
        GameObject xBtnObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        xBtnObj.transform.SetParent(header.transform, false);
        RectTransform xRect = xBtnObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 0.5f);
        xRect.anchorMax = new Vector2(1f, 0.5f);
        xRect.pivot = new Vector2(1f, 0.5f);
        xRect.sizeDelta = new Vector2(38, 38);
        xRect.anchoredPosition = new Vector2(-15, 0);
        xBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.85f);
        closeXBtn = xBtnObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(xBtnObj.transform, false);
        TextMeshProUGUI xTxt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xTxt.text = "✕";
        xTxt.fontSize = 20;
        xTxt.fontStyle = FontStyles.Bold;
        xTxt.alignment = TextAlignmentOptions.Center;
        xTxt.color = Color.white;
        RectTransform xtRect = xTxtObj.GetComponent<RectTransform>();
        xtRect.anchorMin = Vector2.zero;
        xtRect.anchorMax = Vector2.one;
        xtRect.offsetMin = Vector2.zero;
        xtRect.offsetMax = Vector2.zero;

        // Subheader Stats
        GameObject statsObj = new GameObject("PatentStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        patentStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        patentStatsSummaryTxt.fontSize = 13;
        patentStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        patentStatsSummaryTxt.color = new Color(0.90f, 0.94f, 1f);

        // Action Panel (Lawsuit Button)
        GameObject actPanel = new GameObject("ActPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.26f, 1f);

        GameObject lBtnObj = new GameObject("LawsuitBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        lBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform lbRect = lBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = Vector2.zero;
        lbRect.anchorMax = Vector2.one;
        lbRect.offsetMin = new Vector2(8, 6);
        lbRect.offsetMax = new Vector2(-8, -6);
        lBtnObj.GetComponent<Image>().color = new Color(0.75f, 0.25f, 0.25f, 1f);
        lawsuitBtn = lBtnObj.GetComponent<Button>();

        GameObject ltTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        ltTxtObj.transform.SetParent(lBtnObj.transform, false);
        lawsuitBtnTxt = ltTxtObj.GetComponent<TextMeshProUGUI>();
        lawsuitBtnTxt.text = "⚖️ ВЫИГРАТЬ СУДЕБНЫЙ ИСК У ПИРАТОВ (Штраф)";
        lawsuitBtnTxt.fontSize = 14;
        lawsuitBtnTxt.fontStyle = FontStyles.Bold;
        lawsuitBtnTxt.alignment = TextAlignmentOptions.Center;
        lawsuitBtnTxt.color = Color.white;
        RectTransform ltr = ltTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("PatentsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.5f);

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
        patentsContainer = content.transform;
        RectTransform ctnRect = content.GetComponent<RectTransform>();
        ctnRect.anchorMin = new Vector2(0f, 1f);
        ctnRect.anchorMax = new Vector2(1f, 1f);
        ctnRect.pivot = new Vector2(0.5f, 1f);
        ctnRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.viewport = vRect;
        sr.content = ctnRect;
        sr.horizontal = false;
        sr.vertical = true;

        foreach (var patent in patents)
        {
            CreatePatentCardUI(content.transform, patent);
        }

        // Close Bottom Button
        GameObject botCloseObj = new GameObject("CloseBottomBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 42);
        bcRect.anchoredPosition = new Vector2(0, 10);
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.24f, 0.35f, 1f);
        closeBtn = botCloseObj.GetComponent<Button>();

        GameObject bcTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bcTxtObj.transform.SetParent(botCloseObj.transform, false);
        TextMeshProUGUI bcTxt = bcTxtObj.GetComponent<TextMeshProUGUI>();
        bcTxt.text = "ЗАКРЫТЬ";
        bcTxt.fontSize = 14;
        bcTxt.fontStyle = FontStyles.Bold;
        bcTxt.alignment = TextAlignmentOptions.Center;
        bcTxt.color = Color.white;
        RectTransform bctRect = bcTxtObj.GetComponent<RectTransform>();
        bctRect.anchorMin = Vector2.zero;
        bctRect.anchorMax = Vector2.one;
        bctRect.offsetMin = Vector2.zero;
        bctRect.offsetMax = Vector2.zero;

        modalRoot.SetActive(false);
    }

    private void CreatePatentCardUI(Transform parent, StudioPatent patent)
    {
        GameObject card = new GameObject("PatentCard_" + patent.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 92);
        card.GetComponent<Image>().color = new Color(0.14f, 0.17f, 0.24f, 1f);

        // Header
        GameObject hdr = new GameObject("Header", typeof(RectTransform));
        hdr.transform.SetParent(card.transform, false);
        RectTransform hr = hdr.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(1f, 1f);
        hr.pivot = new Vector2(0.5f, 1f);
        hr.sizeDelta = new Vector2(-20, 24);
        hr.anchoredPosition = new Vector2(10, -6);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(hdr.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 14;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = new Color(1f, 0.85f, 0.3f);
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        // Body
        GameObject body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(card.transform, false);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(0.66f, 1f);
        br.offsetMin = new Vector2(10, 8);
        br.offsetMax = new Vector2(0, -30);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.color = new Color(0.85f, 0.90f, 1f);
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = new Vector2(0f, 0.45f);
        dr.anchorMax = new Vector2(1f, 1f);
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;

        GameObject stats = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stats.transform.SetParent(body.transform, false);
        TextMeshProUGUI sTxt = stats.GetComponent<TextMeshProUGUI>();
        sTxt.fontSize = 11;
        sTxt.fontStyle = FontStyles.Bold;
        sTxt.color = new Color(0.2f, 1f, 0.6f);
        RectTransform sr = stats.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 0f);
        sr.anchorMax = new Vector2(1f, 0.45f);
        sr.offsetMin = Vector2.zero;
        sr.offsetMax = Vector2.zero;

        // Action
        GameObject act = new GameObject("Action", typeof(RectTransform));
        act.transform.SetParent(card.transform, false);
        RectTransform ar = act.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.67f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 8);
        ar.offsetMax = new Vector2(-10, -10);

        GameObject upBtn = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        upBtn.transform.SetParent(act.transform, false);
        RectTransform ubr = upBtn.GetComponent<RectTransform>();
        ubr.anchorMin = Vector2.zero;
        ubr.anchorMax = Vector2.one;
        ubr.offsetMin = Vector2.zero;
        ubr.offsetMax = Vector2.zero;
        upBtn.GetComponent<Image>().color = new Color(0.20f, 0.55f, 0.85f, 1f);

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

        string currentPatentId = patent.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradePatent(currentPatentId));
    }
}
