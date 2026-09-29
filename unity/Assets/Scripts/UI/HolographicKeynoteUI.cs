using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// VR/AR Голографический Театр и Презентации («Holographic Keynote Stage»):
/// - Проведение футуристических голографических кейноутов, презентаций движков и консолей
/// - 4 уровня голографических сцен:
///   1. 🔦 HoloProjector Stage v1 (+15% к клику, +10% к доходу)
///   2. 👓 Spatial Augmented Arena (+20% к клику, +15% к доходу)
///   3. 🌌 Neural Photonic Immersive Dome (+25% к клику, +20% к доходу)
///   4. 🪐 Omnipresence Quantum Hologram (+35% к клику, x1.35 ко всему доходу)
/// - Интерактивное действие: «✨ ПРОВЕСТИ ГОЛОГРАФИЧЕСКИЙ КЕЙНОУТ» (100% комбо x3.0 + грант фанатов)
/// - Мощные множители к силе клика и студийному доходу
/// </summary>
public class HolographicKeynoteUI : MonoBehaviour
{
    private static HolographicKeynoteUI instance;
    public static HolographicKeynoteUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<HolographicKeynoteUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(HolographicKeynoteUI));
                    instance = go.AddComponent<HolographicKeynoteUI>();
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
    public class HoloStageTier
    {
        public string id;
        public string title;
        public string techSpecs;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float clickMultiplierBonus;
        public float incomeMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.50, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.44, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openKeynoteBtn;

    [Header("Панель кейноутов")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button hostKeynoteBtn;
    [SerializeField] private TMP_Text hostKeynoteBtnTxt;
    [SerializeField] private Transform stagesContainer;

    [Header("Сцены кейноута")]
    [SerializeField] private List<HoloStageTier> stages = new List<HoloStageTier>();

    private const string PrefKeynotesCount = "Holo_Keynotes_Count";
    private const string PrefStageLvlPrefix = "Holo_Stage_Lvl_";

    private int keynotesHostedCount = 0;
    private bool isHostingKeynote = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitStagesList();
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

    private void InitStagesList()
    {
        if (stages != null && stages.Count > 0) return;

        stages = new List<HoloStageTier>
        {
            new HoloStageTier
            {
                id = "holo_stage_v1",
                title = "HoloProjector Stage v1",
                techSpecs = "Лазерный воксельный проектор • 1080p 3D-модели",
                icon = "🔦",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 6000.0,
                baseCostCode = 3500.0,
                clickMultiplierBonus = 0.15f,
                incomeMultiplierBonus = 0.10f
            },
            new HoloStageTier
            {
                id = "holo_arena",
                title = "Spatial Augmented Arena",
                techSpecs = "Смешанная реальность для зрителей • 8K рендеринг",
                icon = "👓",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 28000.0,
                baseCostCode = 18000.0,
                clickMultiplierBonus = 0.20f,
                incomeMultiplierBonus = 0.15f
            },
            new HoloStageTier
            {
                id = "holo_neural_dome",
                title = "Neural Photonic Dome",
                techSpecs = "Купольный фотонный интерферометр • Прямой нейро-хайп",
                icon = "🌌",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 120000.0,
                baseCostCode = 80000.0,
                clickMultiplierBonus = 0.25f,
                incomeMultiplierBonus = 0.20f
            },
            new HoloStageTier
            {
                id = "holo_omnipresence",
                title = "Omnipresence Quantum Hologram",
                techSpecs = "Глобальная телепортация аватара спикера на миллионы экранов",
                icon = "🪐",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 550000.0,
                baseCostCode = 380000.0,
                clickMultiplierBonus = 0.35f,
                incomeMultiplierBonus = 0.35f
            }
        };
    }

    private void LoadData()
    {
        keynotesHostedCount = PlayerPrefs.GetInt(PrefKeynotesCount, 0);
        foreach (var stg in stages)
        {
            if (PlayerPrefs.HasKey(PrefStageLvlPrefix + stg.id))
            {
                stg.level = PlayerPrefs.GetInt(PrefStageLvlPrefix + stg.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefKeynotesCount, keynotesHostedCount);
        foreach (var stg in stages)
        {
            PlayerPrefs.SetInt(PrefStageLvlPrefix + stg.id, stg.level);
        }
        PlayerPrefs.Save();
    }

    public double GetHoloIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var stg in stages)
        {
            mult += stg.level * stg.incomeMultiplierBonus;
        }
        mult += Math.Min(keynotesHostedCount * 0.015, 0.60);
        return mult;
    }

    public double GetHoloClickMultiplier()
    {
        double mult = 1.0;
        foreach (var stg in stages)
        {
            mult += stg.level * stg.clickMultiplierBonus;
        }
        mult += Math.Min(keynotesHostedCount * 0.02, 0.70);
        return mult;
    }

    public void HostKeynote()
    {
        if (isHostingKeynote) return;
        StartCoroutine(KeynoteRoutine());
    }

    private IEnumerator KeynoteRoutine()
    {
        isHostingKeynote = true;
        if (hostKeynoteBtn != null) hostKeynoteBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (hostKeynoteBtnTxt != null)
        {
            hostKeynoteBtnTxt.text = "✨ ЗАПУСК ГОЛОГРАММЫ... МИЛЛИОНЫ ЗРИТЕЛЕЙ В ОНЛАЙНЕ!";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        keynotesHostedCount++;
        double rewardMoney = 55000.0 * GetHoloIncomeMultiplier();
        double rewardCode = 30000.0 * GetHoloClickMultiplier();

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
            ClickJuice.Instance.SpawnCustomPopup($"✨ ГОЛОГРАФИЧЕСКИЙ ТРИУМФ!\nКОМБО 100% (x3.0)! <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.8f, 0.3f, 1f), true);
        }

        isHostingKeynote = false;
        if (hostKeynoteBtn != null) hostKeynoteBtn.interactable = true;
        if (hostKeynoteBtnTxt != null) hostKeynoteBtnTxt.text = "✨ ПРОВЕСТИ ГОЛОГРАФИЧЕСКИЙ КЕЙНОУТ";
    }

    public void UpgradeStage(string stageId)
    {
        var stg = stages.Find(s => s.id == stageId);
        if (stg == null) return;

        if (stg.level >= stg.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Голографическая сцена улучшена до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = stg.GetCostMoney();
        double costCode = stg.GetCostCode();

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

        stg.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ {stg.title} УРОВЕНЬ {stg.level}!\n+{(stg.clickMultiplierBonus * 100):F0}% Клик • +{(stg.incomeMultiplierBonus * 100):F0}% Доход", transform.position, new Color(0.85f, 0.4f, 1f), true);
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
            double incMult = GetHoloIncomeMultiplier();
            double clkMult = GetHoloClickMultiplier();
            statsSummaryTxt.text = $"✨ Множитель клика: <b>x{clkMult:F2}</b> • Множитель дохода: <b>x{incMult:F2}</b>\n(Проведено кейноутов: {keynotesHostedCount})";
        }

        if (stagesContainer != null)
        {
            for (int i = 0; i < stagesContainer.childCount; i++)
            {
                Transform child = stagesContainer.GetChild(i);
                if (i < stages.Count)
                {
                    UpdateStageCard(child, stages[i]);
                }
            }
        }
    }

    private void UpdateStageCard(Transform card, HoloStageTier stg)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text techTxt = card.Find("Header/TechTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{stg.title} (Ур. {stg.level}/{stg.maxLevel})";
        if (techTxt != null) techTxt.text = stg.techSpecs;
        if (iconTxt != null) iconTxt.text = stg.icon;
        if (descTxt != null) descTxt.text = $"+{(stg.clickMultiplierBonus * 100):F0}% Сила Клика • +{(stg.incomeMultiplierBonus * 100):F0}% Доход Студии";
        if (statsTxt != null) statsTxt.text = $"Текущий вклад: +{(stg.level * stg.clickMultiplierBonus * 100):F0}% клик / +{(stg.level * stg.incomeMultiplierBonus * 100):F0}% доход";

        if (upBtn != null && upBtnTxt != null)
        {
            if (stg.level >= stg.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = stg.GetCostMoney();
                double costCode = stg.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Апгрейд\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openKeynoteBtn != null) openKeynoteBtn.onClick.AddListener(OpenModal);
        if (hostKeynoteBtn != null) hostKeynoteBtn.onClick.AddListener(HostKeynote);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("HolographicKeynoteModalRoot", typeof(RectTransform));
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
        cardImg.color = new Color(0.16f, 0.08f, 0.24f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "✨ HOLOGRAPHIC KEYNOTE STAGE";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.9f, 0.45f, 1f);
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
        subTxt.text = "Голографический театр презентаций новых движков и релизов";
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
        summaryBox.GetComponent<Image>().color = new Color(0.22f, 0.12f, 0.32f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.9f, 0.45f, 1f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Провести голографический кейноут)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.26f, 0.14f, 0.38f, 1f);

        GameObject hostBtnObj = new GameObject("HostBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        hostBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform hbRect = hostBtnObj.GetComponent<RectTransform>();
        hbRect.anchorMin = Vector2.zero;
        hbRect.anchorMax = Vector2.one;
        hbRect.offsetMin = new Vector2(8, 6);
        hbRect.offsetMax = new Vector2(-8, -6);
        hostBtnObj.GetComponent<Image>().color = new Color(0.70f, 0.25f, 0.85f, 1f);
        hostKeynoteBtn = hostBtnObj.GetComponent<Button>();

        GameObject hostTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        hostTxtObj.transform.SetParent(hostBtnObj.transform, false);
        hostKeynoteBtnTxt = hostTxtObj.GetComponent<TextMeshProUGUI>();
        hostKeynoteBtnTxt.text = "✨ ПРОВЕСТИ ГОЛОГРАФИЧЕСКИЙ КЕЙНОУТ";
        hostKeynoteBtnTxt.fontSize = 14;
        hostKeynoteBtnTxt.fontStyle = FontStyles.Bold;
        hostKeynoteBtnTxt.alignment = TextAlignmentOptions.Center;
        hostKeynoteBtnTxt.color = Color.white;
        RectTransform htr = hostTxtObj.GetComponent<RectTransform>();
        htr.anchorMin = Vector2.zero;
        htr.anchorMax = Vector2.one;
        htr.offsetMin = Vector2.zero;
        htr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("StagesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.10f, 0.04f, 0.16f, 0.5f);

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
        stagesContainer = content.transform;
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
        foreach (var stg in stages)
        {
            CreateStageCardUI(content.transform, stg);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.35f, 0.18f, 0.45f, 1f);
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
        GameObject launchBtnObj = new GameObject("HoloKeynoteLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, 96);
        launchBtnObj.GetComponent<Image>().color = new Color(0.70f, 0.25f, 0.85f, 0.95f);
        openKeynoteBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "✨";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateStageCardUI(Transform parent, HoloStageTier stg)
    {
        GameObject card = new GameObject("StageCard_" + stg.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.20f, 0.12f, 0.28f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.35f, 0.18f, 0.45f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = stg.icon;
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

        GameObject tech = new GameObject("TechTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        tech.transform.SetParent(header.transform, false);
        TextMeshProUGUI tcTxt = tech.GetComponent<TextMeshProUGUI>();
        tcTxt.fontSize = 10;
        tcTxt.color = new Color(0.85f, 0.45f, 1f);
        RectTransform tcr = tech.GetComponent<RectTransform>();
        tcr.anchorMin = new Vector2(0f, 0f);
        tcr.anchorMax = new Vector2(1f, 0.5f);
        tcr.offsetMin = Vector2.zero;
        tcr.offsetMax = Vector2.zero;

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
        dTxt.color = new Color(0.95f, 0.85f, 1f);
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
        sTxt.color = new Color(0.9f, 0.45f, 1f);
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
        upBtn.GetComponent<Image>().color = new Color(0.65f, 0.25f, 0.85f, 1f);

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

        string currentStageId = stg.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeStage(currentStageId));
    }
}
