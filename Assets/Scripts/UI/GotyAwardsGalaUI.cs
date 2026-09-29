using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Международная Премия Игры Года («Game of the Year Awards & Gala»):
/// - Ежегодная престижная церемония награждения игровой индустрии
/// - 4 легендарные золотые статуэтки GOTY:
///   1. 🎮 Лучший Инди-Геймплей (+25% к доходу студии)
///   2. 🎵 Лучший Саундтрек и Аудио (+25% к силе клика)
///   3. ⚡ Технологический Прорыв Года (+30% к доходу, +20% к оффлайн)
///   4. 👑 Игра Года — Гран-При GOTY (x1.40 ко всему студийному продакшену)
/// - Интерактивное действие: «🎙️ ВЫСТУПИТЬ С РЕЧЬЮ ПОБЕДИТЕЛЯ GOTY» (100% комбо x3.0 + грант)
/// - Перманентные легендарные множители ко всем экономическим показателям студии
/// </summary>
public class GotyAwardsGalaUI : MonoBehaviour
{
    private static GotyAwardsGalaUI instance;
    public static GotyAwardsGalaUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GotyAwardsGalaUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(GotyAwardsGalaUI));
                    instance = go.AddComponent<GotyAwardsGalaUI>();
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
    public class GotyTrophyCategory
    {
        public string id;
        public string title;
        public string juryVerdict;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.50, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.45, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openGotyBtn;

    [Header("Церемония и статуэтки")]
    [SerializeField] private TMP_Text gotyStatsSummaryTxt;
    [SerializeField] private Button speechBtn;
    [SerializeField] private TMP_Text speechBtnTxt;
    [SerializeField] private Transform trophiesContainer;

    private readonly List<GotyTrophyCategory> trophies = new List<GotyTrophyCategory>();
    private bool isCeremonyActive = false;
    private int galaSpeechesGiven = 0;

    private const string PrefTrophyLvlPrefix = "Goty_TrophyLvl_";
    private const string PrefSpeechesCount = "Goty_SpeechesCount";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTrophies();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeTrophies()
    {
        if (trophies.Count > 0) return;

        trophies.Add(new GotyTrophyCategory
        {
            id = "goty_gameplay",
            title = "Лучший Инди-Геймплей",
            juryVerdict = "«Невероятная глубина механики клика и аддиктивный игровой цикл»",
            icon = "🎮",
            level = 1,
            maxLevel = 5,
            baseCostMoney = 50000,
            baseCostCode = 25000,
            incomeMultiplierBonus = 0.25f,
            clickMultiplierBonus = 0.10f
        });

        trophies.Add(new GotyTrophyCategory
        {
            id = "goty_soundtrack",
            title = "Лучший Саундтрек и Аудио",
            juryVerdict = "«Синхронизированные клики, Lo-Fi ритмы и сочный тактильный звук»",
            icon = "🎵",
            level = 0,
            maxLevel = 5,
            baseCostMoney = 180000,
            baseCostCode = 90000,
            incomeMultiplierBonus = 0.15f,
            clickMultiplierBonus = 0.25f
        });

        trophies.Add(new GotyTrophyCategory
        {
            id = "goty_tech_innovation",
            title = "Технологический Прорыв Года",
            juryVerdict = "«Революционная архитектура без GC и потоковый рендеринг геометрии»",
            icon = "⚡",
            level = 0,
            maxLevel = 5,
            baseCostMoney = 550000,
            baseCostCode = 275000,
            incomeMultiplierBonus = 0.30f,
            clickMultiplierBonus = 0.20f
        });

        trophies.Add(new GotyTrophyCategory
        {
            id = "goty_ultimate",
            title = "Игра Года — Гран-При GOTY",
            juryVerdict = "«Абсолютный триумф геймдева и выбор 100 миллионов игроков по всему миру»",
            icon = "👑",
            level = 0,
            maxLevel = 5,
            baseCostMoney = 2000000,
            baseCostCode = 1000000,
            incomeMultiplierBonus = 0.40f,
            clickMultiplierBonus = 0.30f
        });
    }

    private void LoadData()
    {
        galaSpeechesGiven = PlayerPrefs.GetInt(PrefSpeechesCount, 0);
        foreach (var t in trophies)
        {
            if (PlayerPrefs.HasKey(PrefTrophyLvlPrefix + t.id))
            {
                t.level = PlayerPrefs.GetInt(PrefTrophyLvlPrefix + t.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSpeechesCount, galaSpeechesGiven);
        foreach (var t in trophies)
        {
            PlayerPrefs.SetInt(PrefTrophyLvlPrefix + t.id, t.level);
        }
        PlayerPrefs.Save();
    }

    public double GetGotyIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var t in trophies)
        {
            mult += t.level * t.incomeMultiplierBonus;
        }
        mult += Math.Min(galaSpeechesGiven * 0.02, 0.80);
        return mult;
    }

    public double GetGotyClickMultiplier()
    {
        double mult = 1.0;
        foreach (var t in trophies)
        {
            mult += t.level * t.clickMultiplierBonus;
        }
        return mult;
    }

    public void GiveAcceptanceSpeech()
    {
        if (isCeremonyActive) return;
        StartCoroutine(SpeechRoutine());
    }

    private IEnumerator SpeechRoutine()
    {
        isCeremonyActive = true;
        if (speechBtn != null) speechBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (speechBtnTxt != null)
        {
            speechBtnTxt.text = "🎙️ ВЫСТУПЛЕНИЕ НА СЦЕНЕ... ЗОЛОТОЙ ДОЖДЬ И ОВАЦИИ ЗАЛА!";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        galaSpeechesGiven++;
        double rewardMoney = 50000.0 * GetGotyIncomeMultiplier();
        double rewardCode = 25000.0 * GetGotyClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(1.0f); // 100% комбо x3.0
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏆 ТРИУМФ НА ЦЕРЕМОНИИ GOTY!\nКомбо x3.0 (100%) + Грант: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        isCeremonyActive = false;
        if (speechBtn != null) speechBtn.interactable = true;
        if (speechBtnTxt != null) speechBtnTxt.text = "🎙️ ВЫСТУПИТЬ С РЕЧЬЮ ПОБЕДИТЕЛЯ GOTY";
    }

    public void UpgradeTrophy(string trophyId)
    {
        var t = trophies.Find(x => x.id == trophyId);
        if (t == null) return;

        if (t.level >= t.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Статуэтка уже инкрустирована до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = t.GetCostMoney();
        double costCode = t.GetCostCode();

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

        t.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Статуэтка '{t.title}' улучшена до Ур.{t.level}!\nМножитель студии: +{(t.incomeMultiplierBonus * 100):0}%", transform.position, new Color(1f, 0.85f, 0.25f), true);
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
        if (speechBtn != null)
        {
            speechBtn.onClick.RemoveAllListeners();
            speechBtn.onClick.AddListener(GiveAcceptanceSpeech);
        }
        if (openGotyBtn != null)
        {
            openGotyBtn.onClick.RemoveAllListeners();
            openGotyBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (gotyStatsSummaryTxt != null)
        {
            double incMult = GetGotyIncomeMultiplier();
            double clkMult = GetGotyClickMultiplier();
            gotyStatsSummaryTxt.text = $"Бонус мировой славы GOTY: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>\nВыступлений на сцене: <b>{galaSpeechesGiven}</b>";
        }

        if (speechBtnTxt != null && !isCeremonyActive)
        {
            speechBtnTxt.text = "🎙️ ВЫСТУПИТЬ С РЕЧЬЮ ПОБЕДИТЕЛЯ GOTY (100% комбо)";
        }

        if (trophiesContainer == null) return;

        for (int i = 0; i < trophies.Count; i++)
        {
            var t = trophies[i];
            Transform child = i < trophiesContainer.childCount ? trophiesContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{t.icon} {t.title} (Ур. {t.level}/{t.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = t.juryVerdict;
            }
            if (statsTxt != null)
            {
                float totalInc = t.level * t.incomeMultiplierBonus * 100f;
                float totalClk = t.level * t.clickMultiplierBonus * 100f;
                statsTxt.text = $"Эффект: <color=#00FFAA>+{totalInc:0}% доход</color> | <color=#FFD700>+{totalClk:0}% клик</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (t.level >= t.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = t.GetCostMoney();
                    double c = t.GetCostCode();
                    upBtnTxt.text = $"Инкрустация\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        GameObject root = new GameObject("GotyAwards_ModalRoot", typeof(RectTransform));
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
        bImg.color = new Color(0.04f, 0.04f, 0.06f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Card
        GameObject card = new GameObject("GotyCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.12f, 0.12f, 0.17f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.24f, 0.18f, 0.10f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🏆 МЕЖДУНАРОДНАЯ ПРЕМИЯ GOTY AWARDS";
        tTxt.fontSize = 18;
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
        GameObject statsObj = new GameObject("GotyStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        gotyStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        gotyStatsSummaryTxt.fontSize = 13;
        gotyStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        gotyStatsSummaryTxt.color = new Color(0.95f, 0.92f, 0.85f);

        // Action Panel
        GameObject actPanel = new GameObject("ActPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.20f, 0.17f, 0.12f, 1f);

        GameObject sBtnObj = new GameObject("SpeechBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        sBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform sbRect = sBtnObj.GetComponent<RectTransform>();
        sbRect.anchorMin = Vector2.zero;
        sbRect.anchorMax = Vector2.one;
        sbRect.offsetMin = new Vector2(8, 6);
        sbRect.offsetMax = new Vector2(-8, -6);
        sBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f, 1f);
        speechBtn = sBtnObj.GetComponent<Button>();

        GameObject stTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stTxtObj.transform.SetParent(sBtnObj.transform, false);
        speechBtnTxt = stTxtObj.GetComponent<TextMeshProUGUI>();
        speechBtnTxt.text = "🎙️ ВЫСТУПИТЬ С РЕЧЬЮ ПОБЕДИТЕЛЯ GOTY (100% комбо)";
        speechBtnTxt.fontSize = 13;
        speechBtnTxt.fontStyle = FontStyles.Bold;
        speechBtnTxt.alignment = TextAlignmentOptions.Center;
        speechBtnTxt.color = Color.white;
        RectTransform str = stTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = Vector2.zero;
        str.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("TrophiesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.06f, 0.06f, 0.08f, 0.5f);

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
        trophiesContainer = content.transform;
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

        foreach (var trophy in trophies)
        {
            CreateTrophyCardUI(content.transform, trophy);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.24f, 0.22f, 0.18f, 1f);
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

    private void CreateTrophyCardUI(Transform parent, GotyTrophyCategory trophy)
    {
        GameObject card = new GameObject("TrophyCard_" + trophy.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 92);
        card.GetComponent<Image>().color = new Color(0.16f, 0.15f, 0.20f, 1f);

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
        dTxt.color = new Color(0.90f, 0.90f, 0.95f);
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
        upBtn.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f, 1f);

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

        string currentTrophyId = trophy.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeTrophy(currentTrophyId));
    }
}
