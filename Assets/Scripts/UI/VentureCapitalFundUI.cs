using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Венчурный Фонд Инкубации Инди-Стартапов («Venture Capital Indie Fund»):
/// - Инвестирование избыточного капитала студии в перспективные молодые инди-команды
/// - 4 уровня инвестиционных программ:
///   1. 🐣 Garage Game Jam Seed (+20 C#/сек, +10% к доходу)
///   2. 🚀 Early-Stage Indie Accelerator (+80 C#/сек, +15% к клику)
///   3. 🦄 Series-A VR/AI Scaleup (+300 C#/сек, +20% к доходу)
///   4. 🏛️ Global Unicorn Games Syndicate (+1200 C#/сек, x1.40 ко всему доходу)
/// - Интерактивное действие: «💼 ВЫПЛАТИТЬ ВЕНЧУРНЫЕ ДИВИДЕНДЫ» (IPO стартапов, комбо +35% и солидный куш)
/// - Пассивный приток строк кода C# от финансируемых студий и глобальные множители
/// </summary>
public class VentureCapitalFundUI : MonoBehaviour
{
    private static VentureCapitalFundUI instance;
    public static VentureCapitalFundUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<VentureCapitalFundUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(VentureCapitalFundUI));
                    instance = go.AddComponent<VentureCapitalFundUI>();
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
    public class StartupProgramTier
    {
        public string id;
        public string title;
        public string cohortInfo;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double codePerSecBonus;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

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
    [SerializeField] private Button openFundBtn;

    [Header("Панель фонда")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button dividendsBtn;
    [SerializeField] private TMP_Text dividendsBtnTxt;
    [SerializeField] private Transform programsContainer;

    [Header("Инвестиционные программы")]
    [SerializeField] private List<StartupProgramTier> programs = new List<StartupProgramTier>();

    private const string PrefDividendsCount = "Venture_Dividends_Count";
    private const string PrefProgramLvlPrefix = "Venture_Program_Lvl_";

    private int dividendsCollectedCount = 0;
    private bool isCollectingDividends = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitProgramsList();
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

    private void InitProgramsList()
    {
        if (programs != null && programs.Count > 0) return;

        programs = new List<StartupProgramTier>
        {
            new StartupProgramTier
            {
                id = "prog_gamejam_seed",
                title = "Garage Game Jam Seed",
                cohortInfo = "Студенческие команды • Посевные гранты на прототипы",
                icon = "🐣",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 5000.0,
                baseCostCode = 3000.0,
                codePerSecBonus = 20.0,
                incomeMultiplierBonus = 0.10f,
                clickMultiplierBonus = 0.08f
            },
            new StartupProgramTier
            {
                id = "prog_indie_accelerator",
                title = "Early Indie Accelerator",
                cohortInfo = "3-месячный буткемп • Менторство и аудит архитектуры",
                icon = "🚀",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 25000.0,
                baseCostCode = 16000.0,
                codePerSecBonus = 80.0,
                incomeMultiplierBonus = 0.15f,
                clickMultiplierBonus = 0.15f
            },
            new StartupProgramTier
            {
                id = "prog_series_a_scaleup",
                title = "Series-A VR/AI Scaleup",
                cohortInfo = "Масштабирование инди-хитов • Выход на глобальные сторы",
                icon = "🦄",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 110000.0,
                baseCostCode = 70000.0,
                codePerSecBonus = 300.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.18f
            },
            new StartupProgramTier
            {
                id = "prog_unicorn_syndicate",
                title = "Global Unicorn Syndicate",
                cohortInfo = "Синдикат игровых единорогов • Международные IPO",
                icon = "🏛️",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 500000.0,
                baseCostCode = 350000.0,
                codePerSecBonus = 1200.0,
                incomeMultiplierBonus = 0.40f,
                clickMultiplierBonus = 0.25f
            }
        };
    }

    private void LoadData()
    {
        dividendsCollectedCount = PlayerPrefs.GetInt(PrefDividendsCount, 0);
        foreach (var prog in programs)
        {
            if (PlayerPrefs.HasKey(PrefProgramLvlPrefix + prog.id))
            {
                prog.level = PlayerPrefs.GetInt(PrefProgramLvlPrefix + prog.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefDividendsCount, dividendsCollectedCount);
        foreach (var prog in programs)
        {
            PlayerPrefs.SetInt(PrefProgramLvlPrefix + prog.id, prog.level);
        }
        PlayerPrefs.Save();
    }

    public double GetVentureIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var prog in programs)
        {
            mult += prog.level * prog.incomeMultiplierBonus;
        }
        mult += Math.Min(dividendsCollectedCount * 0.015, 0.60);
        return mult;
    }

    public double GetVentureClickMultiplier()
    {
        double mult = 1.0;
        foreach (var prog in programs)
        {
            mult += prog.level * prog.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetVentureCodePerSec()
    {
        double sum = 0;
        foreach (var prog in programs)
        {
            sum += prog.GetTotalCps();
        }
        return sum;
    }

    public void CollectDividends()
    {
        if (isCollectingDividends) return;
        StartCoroutine(DividendsRoutine());
    }

    private IEnumerator DividendsRoutine()
    {
        isCollectingDividends = true;
        if (dividendsBtn != null) dividendsBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (dividendsBtnTxt != null)
        {
            dividendsBtnTxt.text = "💼 ВЫХОД СТАРТАПА НА IPO... ВЫПЛАТА ДИВИДЕНДОВ!";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        dividendsCollectedCount++;
        double baseMoney = Math.Max(40000.0, GetVentureIncomeMultiplier() * 30000.0);
        double rewardMoney = Math.Floor(baseMoney * GetVentureIncomeMultiplier());
        double baseCode = Math.Max(25000.0, GetVentureCodePerSec() * 60.0);
        double rewardCode = Math.Floor(baseCode * GetVentureClickMultiplier());

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
            ClickJuice.Instance.SpawnCustomPopup($"💼 ДИВИДЕНДЫ ПОЛУЧЕНЫ!\nКапитализация портфеля: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isCollectingDividends = false;
        if (dividendsBtn != null) dividendsBtn.interactable = true;
        if (dividendsBtnTxt != null) dividendsBtnTxt.text = "💼 ВЫПЛАТИТЬ ВЕНЧУРНЫЕ ДИВИДЕНДЫ";
    }

    public void UpgradeProgram(string progId)
    {
        var prog = programs.Find(p => p.id == progId);
        if (prog == null) return;

        if (prog.level >= prog.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Инвестиционная программа достигла предела масштабирования!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = prog.GetCostMoney();
        double costCode = prog.GetCostCode();

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

        prog.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💼 {prog.title} УРОВЕНЬ {prog.level}!\n+{prog.codePerSecBonus:F0} C#/сек • +{(prog.incomeMultiplierBonus * 100):F0}% Доход", transform.position, new Color(0.2f, 1f, 0.6f), true);
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
            double totalCps = GetVentureCodePerSec();
            double incMult = GetVentureIncomeMultiplier();
            double clkMult = GetVentureClickMultiplier();
            statsSummaryTxt.text = $"💼 Стартап-поток: <b>+{NumberFormatter.Format(totalCps)} C#/сек</b>\nМножитель дохода: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b> (IPO дивидендов: {dividendsCollectedCount})";
        }

        if (programsContainer != null)
        {
            for (int i = 0; i < programsContainer.childCount; i++)
            {
                Transform child = programsContainer.GetChild(i);
                if (i < programs.Count)
                {
                    UpdateProgramCard(child, programs[i]);
                }
            }
        }
    }

    private void UpdateProgramCard(Transform card, StartupProgramTier prog)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text cohortTxt = card.Find("Header/CohortTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{prog.title} (Ур. {prog.level}/{prog.maxLevel})";
        if (cohortTxt != null) cohortTxt.text = prog.cohortInfo;
        if (iconTxt != null) iconTxt.text = prog.icon;
        if (descTxt != null) descTxt.text = $"+{prog.codePerSecBonus:F0} C#/сек • +{(prog.incomeMultiplierBonus * 100):F0}% Доход • +{(prog.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущая отдача: +{NumberFormatter.Format(prog.GetTotalCps())} C#/сек";

        if (upBtn != null && upBtnTxt != null)
        {
            if (prog.level >= prog.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = prog.GetCostMoney();
                double costCode = prog.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Инвестировать\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openFundBtn != null) openFundBtn.onClick.AddListener(OpenModal);
        if (dividendsBtn != null) dividendsBtn.onClick.AddListener(CollectDividends);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("VentureCapitalModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.04f, 0.08f, 0.05f, 0.85f);
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
        cardImg.color = new Color(0.08f, 0.16f, 0.12f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "💼 VENTURE CAPITAL INDIE FUND";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.3f, 1f, 0.6f);
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
        subTxt.text = "Инкубатор перспективных инди-стартапов и сбор дивидендов";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.75f, 0.95f, 0.85f);
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
        summaryBox.GetComponent<Image>().color = new Color(0.10f, 0.22f, 0.16f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.3f, 1f, 0.6f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Выплатить венчурные дивиденды)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.12f, 0.26f, 0.18f, 1f);

        GameObject divBtnObj = new GameObject("DividendsBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        divBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform dbRect = divBtnObj.GetComponent<RectTransform>();
        dbRect.anchorMin = Vector2.zero;
        dbRect.anchorMax = Vector2.one;
        dbRect.offsetMin = new Vector2(8, 6);
        dbRect.offsetMax = new Vector2(-8, -6);
        divBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.65f, 0.35f, 1f);
        dividendsBtn = divBtnObj.GetComponent<Button>();

        GameObject divTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        divTxtObj.transform.SetParent(divBtnObj.transform, false);
        dividendsBtnTxt = divTxtObj.GetComponent<TextMeshProUGUI>();
        dividendsBtnTxt.text = "💼 ВЫПЛАТИТЬ ВЕНЧУРНЫЕ ДИВИДЕНДЫ";
        dividendsBtnTxt.fontSize = 14;
        dividendsBtnTxt.fontStyle = FontStyles.Bold;
        dividendsBtnTxt.alignment = TextAlignmentOptions.Center;
        dividendsBtnTxt.color = Color.white;
        RectTransform dtr = divTxtObj.GetComponent<RectTransform>();
        dtr.anchorMin = Vector2.zero;
        dtr.anchorMax = Vector2.one;
        dtr.offsetMin = Vector2.zero;
        dtr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("ProgramsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.10f, 0.07f, 0.5f);

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
        programsContainer = content.transform;
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
        foreach (var prog in programs)
        {
            CreateProgramCardUI(content.transform, prog);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.18f, 0.35f, 0.25f, 1f);
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
        GameObject launchBtnObj = new GameObject("VentureFundLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, 146);
        launchBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.65f, 0.35f, 0.95f);
        openFundBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "💼";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateProgramCardUI(Transform parent, StartupProgramTier prog)
    {
        GameObject card = new GameObject("ProgCard_" + prog.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.12f, 0.22f, 0.16f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.18f, 0.35f, 0.25f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = prog.icon;
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

        GameObject cohort = new GameObject("CohortTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cohort.transform.SetParent(header.transform, false);
        TextMeshProUGUI cTxt = cohort.GetComponent<TextMeshProUGUI>();
        cTxt.fontSize = 10;
        cTxt.color = new Color(0.4f, 1f, 0.7f);
        RectTransform cr = cohort.GetComponent<RectTransform>();
        cr.anchorMin = new Vector2(0f, 0f);
        cr.anchorMax = new Vector2(1f, 0.5f);
        cr.offsetMin = Vector2.zero;
        cr.offsetMax = Vector2.zero;

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
        dTxt.color = new Color(0.85f, 0.95f, 0.90f);
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
        sTxt.color = new Color(0.3f, 1f, 0.6f);
        RectTransform sr = stats.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 0f);
        sr.anchorMax = new Vector2(1f, 0.45f);
        sr.offsetMin = Vector2.zero;
        sr.offsetMax = Vector2.zero;

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
        upBtn.GetComponent<Image>().color = new Color(0.18f, 0.60f, 0.35f, 1f);

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

        string currentProgId = prog.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeProgram(currentProgId));
    }
}
