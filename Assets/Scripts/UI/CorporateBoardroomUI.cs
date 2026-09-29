using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Совет Директоров и Враждебные Поглощения («Hostile Takeover Boardroom»):
/// - Совет директоров холдинга, скупка контрольных пакетов конкурирующих студий и издательств
/// - 4 уровня стратегических поглощений:
///   1. 📑 Indie Mobile Studio Acquisition (+15% к доходу, +10% к клику)
///   2. 🏢 Mid-Tier AA Publisher Merger (+20% к доходу, +15% к клику)
///   3. 🌐 Global Triple-A Megacorp Buyout (+30% к доходу, +25% к клику)
///   4. 👑 Full Monopoly Market Domination (x1.45 ко всему доходу холдинга)
/// - Интерактивное действие: «🤝 ЗАКЛЮЧИТЬ МЕГАСДЕЛКУ ПОГЛОЩЕНИЯ» (взрыв капитализации, 100% комбо x3.0 и выплата)
/// - Колоссальные множители к пассивному доходу и клику разработчика
/// </summary>
public class CorporateBoardroomUI : MonoBehaviour
{
    private static CorporateBoardroomUI instance;
    public static CorporateBoardroomUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CorporateBoardroomUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CorporateBoardroomUI));
                    instance = go.AddComponent<CorporateBoardroomUI>();
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
    public class TakeoverTargetTier
    {
        public string id;
        public string title;
        public string dealTerms;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.52, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.45, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openBoardroomBtn;

    [Header("Панель поглощений")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button executeDealBtn;
    [SerializeField] private TMP_Text executeDealBtnTxt;
    [SerializeField] private Transform takeoversContainer;

    [Header("Цели поглощения")]
    [SerializeField] private List<TakeoverTargetTier> takeovers = new List<TakeoverTargetTier>();

    private const string PrefDealsCount = "Boardroom_Deals_Count";
    private const string PrefTakeoverLvlPrefix = "Boardroom_Lvl_";

    private int dealsClosedCount = 0;
    private bool isExecutingDeal = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitTakeoversList();
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

    private void InitTakeoversList()
    {
        if (takeovers != null && takeovers.Count > 0) return;

        takeovers = new List<TakeoverTargetTier>
        {
            new TakeoverTargetTier
            {
                id = "deal_indie_mobile",
                title = "Indie Mobile Studio Acquisition",
                dealTerms = "Поглощение 51% гиперказуальной студии • Каталог из 12 игр",
                icon = "📑",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 8000.0,
                baseCostCode = 5000.0,
                incomeMultiplierBonus = 0.15f,
                clickMultiplierBonus = 0.10f
            },
            new TakeoverTargetTier
            {
                id = "deal_aa_publisher",
                title = "Mid-Tier AA Publisher Merger",
                dealTerms = "Слияние издательских сетей и дистрибуции в Европе и Азии",
                icon = "🏢",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 38000.0,
                baseCostCode = 24000.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.15f
            },
            new TakeoverTargetTier
            {
                id = "deal_aaa_megacorp",
                title = "Global AAA Megacorp Buyout",
                dealTerms = "Враждебный выкуп акций гиганта индустрии на бирже Nasdaq",
                icon = "🌐",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 160000.0,
                baseCostCode = 100000.0,
                incomeMultiplierBonus = 0.30f,
                clickMultiplierBonus = 0.25f
            },
            new TakeoverTargetTier
            {
                id = "deal_full_monopoly",
                title = "Full Monopoly Market Domination",
                dealTerms = "Абсолютная рыночная монополия • Контроль мирового гейминга",
                icon = "👑",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 750000.0,
                baseCostCode = 500000.0,
                incomeMultiplierBonus = 0.45f,
                clickMultiplierBonus = 0.35f
            }
        };
    }

    private void LoadData()
    {
        dealsClosedCount = PlayerPrefs.GetInt(PrefDealsCount, 0);
        foreach (var t in takeovers)
        {
            if (PlayerPrefs.HasKey(PrefTakeoverLvlPrefix + t.id))
            {
                t.level = PlayerPrefs.GetInt(PrefTakeoverLvlPrefix + t.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefDealsCount, dealsClosedCount);
        foreach (var t in takeovers)
        {
            PlayerPrefs.SetInt(PrefTakeoverLvlPrefix + t.id, t.level);
        }
        PlayerPrefs.Save();
    }

    public double GetBoardroomIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var t in takeovers)
        {
            mult += t.level * t.incomeMultiplierBonus;
        }
        mult += Math.Min(dealsClosedCount * 0.02, 0.70);
        return mult;
    }

    public double GetBoardroomClickMultiplier()
    {
        double mult = 1.0;
        foreach (var t in takeovers)
        {
            mult += t.level * t.clickMultiplierBonus;
        }
        return mult;
    }

    public void ExecuteMegaDeal()
    {
        if (isExecutingDeal) return;
        StartCoroutine(DealRoutine());
    }

    private IEnumerator DealRoutine()
    {
        isExecutingDeal = true;
        if (executeDealBtn != null) executeDealBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (executeDealBtnTxt != null)
        {
            executeDealBtnTxt.text = "🤝 ПОДПИСАНИЕ СДЕЛКИ СОВЕТОМ ДИРЕКТОРОВ...";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        dealsClosedCount++;
        double rewardMoney = 60000.0 * GetBoardroomIncomeMultiplier();
        double rewardCode = 35000.0 * GetBoardroomClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(1.0f); // 100% комбо В Потоке (x3.0)
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🤝 МЕГАСДЕЛКА ЗАКРЫТА!\nКОМБО 100% (x3.0)! <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.95f, 0.75f, 0.2f), true);
        }

        isExecutingDeal = false;
        if (executeDealBtn != null) executeDealBtn.interactable = true;
        if (executeDealBtnTxt != null) executeDealBtnTxt.text = "🤝 ЗАКЛЮЧИТЬ МЕГАСДЕЛКУ ПОГЛОЩЕНИЯ";
    }

    public void UpgradeTakeover(string targetId)
    {
        var target = takeovers.Find(t => t.id == targetId);
        if (target == null) return;

        if (target.level >= target.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Контрольный пакет уже выкуплен полностью!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = target.GetCostMoney();
        double costCode = target.GetCostCode();

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

        target.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🤝 {target.title} РАНГ {target.level}!\n+{(target.incomeMultiplierBonus * 100):F0}% Доход • +{(target.clickMultiplierBonus * 100):F0}% Клик", transform.position, new Color(0.95f, 0.75f, 0.2f), true);
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
            double incMult = GetBoardroomIncomeMultiplier();
            double clkMult = GetBoardroomClickMultiplier();
            statsSummaryTxt.text = $"🤝 Множитель холдинга: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b>\n(Закрыто мегасделок: {dealsClosedCount})";
        }

        if (takeoversContainer != null)
        {
            for (int i = 0; i < takeoversContainer.childCount; i++)
            {
                Transform child = takeoversContainer.GetChild(i);
                if (i < takeovers.Count)
                {
                    UpdateTakeoverCard(child, takeovers[i]);
                }
            }
        }
    }

    private void UpdateTakeoverCard(Transform card, TakeoverTargetTier target)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text termsTxt = card.Find("Header/TermsTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{target.title} (Ур. {target.level}/{target.maxLevel})";
        if (termsTxt != null) termsTxt.text = target.dealTerms;
        if (iconTxt != null) iconTxt.text = target.icon;
        if (descTxt != null) descTxt.text = $"+{(target.incomeMultiplierBonus * 100):F0}% Доход Холдинга • +{(target.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущая доля: +{(target.level * target.incomeMultiplierBonus * 100):F0}% доход / +{(target.level * target.clickMultiplierBonus * 100):F0}% клик";

        if (upBtn != null && upBtnTxt != null)
        {
            if (target.level >= target.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = target.GetCostMoney();
                double costCode = target.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Выкупить\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openBoardroomBtn != null) openBoardroomBtn.onClick.AddListener(OpenModal);
        if (executeDealBtn != null) executeDealBtn.onClick.AddListener(ExecuteMegaDeal);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("CorporateBoardroomModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.04f, 0.04f, 0.06f, 0.85f);
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
        cardImg.color = new Color(0.12f, 0.12f, 0.16f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "🤝 CORPORATE BOARDROOM";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.95f, 0.80f, 0.35f);
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
        subTxt.text = "Совет директоров, враждебные поглощения и консолидация рынка";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.9f, 0.9f, 0.95f);
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
        summaryBox.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.24f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.95f, 0.80f, 0.35f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Заключить сделку поглощения)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.20f, 0.20f, 0.28f, 1f);

        GameObject dealBtnObj = new GameObject("DealBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        dealBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform dbR = dealBtnObj.GetComponent<RectTransform>();
        dbR.anchorMin = Vector2.zero;
        dbR.anchorMax = Vector2.one;
        dbR.offsetMin = new Vector2(8, 6);
        dbR.offsetMax = new Vector2(-8, -6);
        dealBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.70f, 0.20f, 1f);
        executeDealBtn = dealBtnObj.GetComponent<Button>();

        GameObject dealTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        dealTxtObj.transform.SetParent(dealBtnObj.transform, false);
        executeDealBtnTxt = dealTxtObj.GetComponent<TextMeshProUGUI>();
        executeDealBtnTxt.text = "🤝 ЗАКЛЮЧИТЬ МЕГАСДЕЛКУ ПОГЛОЩЕНИЯ";
        executeDealBtnTxt.fontSize = 14;
        executeDealBtnTxt.fontStyle = FontStyles.Bold;
        executeDealBtnTxt.alignment = TextAlignmentOptions.Center;
        executeDealBtnTxt.color = new Color(0.12f, 0.10f, 0.04f);
        RectTransform dtr = dealTxtObj.GetComponent<RectTransform>();
        dtr.anchorMin = Vector2.zero;
        dtr.anchorMax = Vector2.one;
        dtr.offsetMin = Vector2.zero;
        dtr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("TakeoversScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.5f);

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
        takeoversContainer = content.transform;
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
        foreach (var t in takeovers)
        {
            CreateTakeoverCardUI(content.transform, t);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.36f, 1f);
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
        GameObject launchBtnObj = new GameObject("CorporateBoardroomLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, -54);
        launchBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.70f, 0.20f, 0.95f);
        openBoardroomBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "🤝";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateTakeoverCardUI(Transform parent, TakeoverTargetTier target)
    {
        GameObject card = new GameObject("TakeoverCard_" + target.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.24f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.28f, 0.28f, 0.36f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = target.icon;
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

        GameObject terms = new GameObject("TermsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        terms.transform.SetParent(header.transform, false);
        TextMeshProUGUI tmTxt = terms.GetComponent<TextMeshProUGUI>();
        tmTxt.fontSize = 10;
        tmTxt.color = new Color(0.95f, 0.85f, 0.45f);
        RectTransform tmr = terms.GetComponent<RectTransform>();
        tmr.anchorMin = new Vector2(0f, 0f);
        tmr.anchorMax = new Vector2(1f, 0.5f);
        tmr.offsetMin = Vector2.zero;
        tmr.offsetMax = Vector2.zero;

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
        dTxt.color = new Color(0.95f, 0.95f, 0.95f);
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
        stTxt.color = new Color(0.95f, 0.80f, 0.35f);
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
        upBtn.GetComponent<Image>().color = new Color(0.80f, 0.65f, 0.20f, 1f);

        GameObject upTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        upTxt.transform.SetParent(upBtn.transform, false);
        TextMeshProUGUI ut = upTxt.GetComponent<TextMeshProUGUI>();
        ut.fontSize = 11;
        ut.fontStyle = FontStyles.Bold;
        ut.alignment = TextAlignmentOptions.Center;
        ut.color = new Color(0.12f, 0.10f, 0.04f);
        RectTransform utr = upTxt.GetComponent<RectTransform>();
        utr.anchorMin = Vector2.zero;
        utr.anchorMax = Vector2.one;
        utr.offsetMin = Vector2.zero;
        utr.offsetMax = Vector2.zero;

        string currentTargetId = target.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeTakeover(currentTargetId));
    }
}
