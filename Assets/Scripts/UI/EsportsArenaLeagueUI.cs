using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Киберспортивная Арена и Лига Студии («Colosseum Esports Arena & Major League»):
/// - Брендовый киберспортивный стадион на 50 000 зрителей
/// - Продажа билетов, прав на трансляции и фирменного мерча
/// - 4 уровня развития арены и лиги:
///   1. 🏟️ CyberDome Spectator Hall (+200 ₽/сек, +10% к доходу)
///   2. 🎮 Pro-Gamer Soundproof Booths (+750 ₽/сек, +15% к клику)
///   3. 📺 Global 8K Broadcast Studio (+2600 ₽/сек, +20% к доходу)
///   4. 🏆 World Championship Major Trophy (+9000 ₽/сек, x1.35 ко всему доходу)
/// - Интерактивное действие: «🏆 ПРОВЕСТИ ГРАНД-ФИНАЛ МЭЙДЖОРА» (аншлаг, комбо +35% и выручка от билетов)
/// - Непрерывный пассивный поток денег в секунду от киберспортивной франшизы
/// </summary>
public class EsportsArenaLeagueUI : MonoBehaviour
{
    private static EsportsArenaLeagueUI instance;
    public static EsportsArenaLeagueUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<EsportsArenaLeagueUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(EsportsArenaLeagueUI));
                    instance = go.AddComponent<EsportsArenaLeagueUI>();
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
    public class ArenaFacilityTier
    {
        public string id;
        public string title;
        public string capacityInfo;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double moneyPerSecBonus;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public double GetTotalMps() => moneyPerSecBonus * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openArenaBtn;

    [Header("Панель арены")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button hostMajorBtn;
    [SerializeField] private TMP_Text hostMajorBtnTxt;
    [SerializeField] private Transform facilitiesContainer;

    [Header("Модули арены")]
    [SerializeField] private List<ArenaFacilityTier> facilities = new List<ArenaFacilityTier>();

    private const string PrefMajorsCount = "Arena_Majors_Count";
    private const string PrefFacilityLvlPrefix = "Arena_Facility_Lvl_";

    private int majorsHostedCount = 0;
    private bool isHostingMajor = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitFacilitiesList();
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

    private void InitFacilitiesList()
    {
        if (facilities != null && facilities.Count > 0) return;

        facilities = new List<ArenaFacilityTier>
        {
            new ArenaFacilityTier
            {
                id = "fac_cyber_dome",
                title = "CyberDome Spectator Hall",
                capacityInfo = "Трибуны на 15 000 фанатов • Лазерное шоу",
                icon = "🏟️",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 7500.0,
                baseCostCode = 4500.0,
                moneyPerSecBonus = 200.0,
                incomeMultiplierBonus = 0.10f,
                clickMultiplierBonus = 0.08f
            },
            new ArenaFacilityTier
            {
                id = "fac_soundproof_booths",
                title = "Pro Soundproof Booths",
                capacityInfo = "Звукоизолированные кабины с 360-Гц мониторами",
                icon = "🎮",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 35000.0,
                baseCostCode = 22000.0,
                moneyPerSecBonus = 750.0,
                incomeMultiplierBonus = 0.15f,
                clickMultiplierBonus = 0.15f
            },
            new ArenaFacilityTier
            {
                id = "fac_8k_broadcast",
                title = "Global 8K Broadcast Studio",
                capacityInfo = "Трансляция финала на Twitch/YouTube с 50 языками",
                icon = "📺",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 150000.0,
                baseCostCode = 95000.0,
                moneyPerSecBonus = 2600.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.18f
            },
            new ArenaFacilityTier
            {
                id = "fac_world_championship",
                title = "Major World Championship",
                capacityInfo = "Главный кубок года • Мировой призовой фонд $10M+",
                icon = "🏆",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 650000.0,
                baseCostCode = 450000.0,
                moneyPerSecBonus = 9000.0,
                incomeMultiplierBonus = 0.35f,
                clickMultiplierBonus = 0.25f
            }
        };
    }

    private void LoadData()
    {
        majorsHostedCount = PlayerPrefs.GetInt(PrefMajorsCount, 0);
        foreach (var fac in facilities)
        {
            if (PlayerPrefs.HasKey(PrefFacilityLvlPrefix + fac.id))
            {
                fac.level = PlayerPrefs.GetInt(PrefFacilityLvlPrefix + fac.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefMajorsCount, majorsHostedCount);
        foreach (var fac in facilities)
        {
            PlayerPrefs.SetInt(PrefFacilityLvlPrefix + fac.id, fac.level);
        }
        PlayerPrefs.Save();
    }

    public double GetArenaIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var fac in facilities)
        {
            mult += fac.level * fac.incomeMultiplierBonus;
        }
        mult += Math.Min(majorsHostedCount * 0.015, 0.60);
        return mult;
    }

    public double GetArenaClickMultiplier()
    {
        double mult = 1.0;
        foreach (var fac in facilities)
        {
            mult += fac.level * fac.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetArenaMoneyPerSec()
    {
        double sum = 0;
        foreach (var fac in facilities)
        {
            sum += fac.GetTotalMps();
        }
        return sum;
    }

    public void HostMajorGrandFinals()
    {
        if (isHostingMajor) return;
        StartCoroutine(MajorRoutine());
    }

    private IEnumerator MajorRoutine()
    {
        isHostingMajor = true;
        if (hostMajorBtn != null) hostMajorBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (hostMajorBtnTxt != null)
        {
            hostMajorBtnTxt.text = "🏆 АНШЛАГ НА АРЕНЕ... ГРАНД-ФИНАЛ В РАЗГАРЕ!";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        majorsHostedCount++;
        double baseRevenue = Math.Max(45000.0, GetArenaMoneyPerSec() * 60.0);
        double rewardMoney = Math.Floor(baseRevenue * GetArenaIncomeMultiplier());
        double rewardCode = Math.Floor(rewardMoney * 0.40 * GetArenaClickMultiplier());

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
            ClickJuice.Instance.SpawnCustomPopup($"🏆 ЧЕМПИОНЫ ПОДНЯЛИ КУБОК!\nВыручка турнира: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isHostingMajor = false;
        if (hostMajorBtn != null) hostMajorBtn.interactable = true;
        if (hostMajorBtnTxt != null) hostMajorBtnTxt.text = "🏆 ПРОВЕСТИ ГРАНД-ФИНАЛ МЭЙДЖОРА";
    }

    public void UpgradeFacility(string facId)
    {
        var fac = facilities.Find(f => f.id == facId);
        if (fac == null) return;

        if (fac.level >= fac.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Модуль арены достиг максимального ранга!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = fac.GetCostMoney();
        double costCode = fac.GetCostCode();

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

        fac.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏆 {fac.title} УРОВЕНЬ {fac.level}!\n+{fac.moneyPerSecBonus:F0} ₽/сек • +{(fac.incomeMultiplierBonus * 100):F0}% Доход", transform.position, new Color(1f, 0.8f, 0.2f), true);
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
            double totalMps = GetArenaMoneyPerSec();
            double incMult = GetArenaIncomeMultiplier();
            double clkMult = GetArenaClickMultiplier();
            statsSummaryTxt.text = $"🏆 Арена и билеты: <b>+{NumberFormatter.Format(totalMps)} ₽/сек</b>\nМножитель дохода: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b> (Мэйджоров: {majorsHostedCount})";
        }

        if (facilitiesContainer != null)
        {
            for (int i = 0; i < facilitiesContainer.childCount; i++)
            {
                Transform child = facilitiesContainer.GetChild(i);
                if (i < facilities.Count)
                {
                    UpdateFacilityCard(child, facilities[i]);
                }
            }
        }
    }

    private void UpdateFacilityCard(Transform card, ArenaFacilityTier fac)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text capTxt = card.Find("Header/CapTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{fac.title} (Ур. {fac.level}/{fac.maxLevel})";
        if (capTxt != null) capTxt.text = fac.capacityInfo;
        if (iconTxt != null) iconTxt.text = fac.icon;
        if (descTxt != null) descTxt.text = $"+{fac.moneyPerSecBonus:F0} ₽/сек • +{(fac.incomeMultiplierBonus * 100):F0}% Доход • +{(fac.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущая касса: +{NumberFormatter.Format(fac.GetTotalMps())} ₽/сек";

        if (upBtn != null && upBtnTxt != null)
        {
            if (fac.level >= fac.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = fac.GetCostMoney();
                double costCode = fac.GetCostCode();
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
        if (openArenaBtn != null) openArenaBtn.onClick.AddListener(OpenModal);
        if (hostMajorBtn != null) hostMajorBtn.onClick.AddListener(HostMajorGrandFinals);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("EsportsArenaModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.08f, 0.05f, 0.01f, 0.85f);
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
        cardImg.color = new Color(0.18f, 0.12f, 0.04f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "🏆 COLOSSEUM ESPORTS ARENA";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(1f, 0.85f, 0.25f);
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
        subTxt.text = "Киберспортивный стадион студии и организация мировых мэйджоров";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(1f, 0.92f, 0.75f);
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
        summaryBox.GetComponent<Image>().color = new Color(0.24f, 0.16f, 0.05f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(1f, 0.85f, 0.25f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Провести Гранд-Финал)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.26f, 0.18f, 0.06f, 1f);

        GameObject majorBtnObj = new GameObject("MajorBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        majorBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform mbR = majorBtnObj.GetComponent<RectTransform>();
        mbR.anchorMin = Vector2.zero;
        mbR.anchorMax = Vector2.one;
        mbR.offsetMin = new Vector2(8, 6);
        mbR.offsetMax = new Vector2(-8, -6);
        majorBtnObj.GetComponent<Image>().color = new Color(0.90f, 0.65f, 0.15f, 1f);
        hostMajorBtn = majorBtnObj.GetComponent<Button>();

        GameObject majorTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        majorTxtObj.transform.SetParent(majorBtnObj.transform, false);
        hostMajorBtnTxt = majorTxtObj.GetComponent<TextMeshProUGUI>();
        hostMajorBtnTxt.text = "🏆 ПРОВЕСТИ ГРАНД-ФИНАЛ МЭЙДЖОРА";
        hostMajorBtnTxt.fontSize = 14;
        hostMajorBtnTxt.fontStyle = FontStyles.Bold;
        hostMajorBtnTxt.alignment = TextAlignmentOptions.Center;
        hostMajorBtnTxt.color = new Color(0.12f, 0.08f, 0.02f);
        RectTransform mtr = majorTxtObj.GetComponent<RectTransform>();
        mtr.anchorMin = Vector2.zero;
        mtr.anchorMax = Vector2.one;
        mtr.offsetMin = Vector2.zero;
        mtr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("FacilitiesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.10f, 0.06f, 0.02f, 0.5f);

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
        facilitiesContainer = content.transform;
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
        foreach (var fac in facilities)
        {
            CreateFacilityCardUI(content.transform, fac);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.36f, 0.24f, 0.08f, 1f);
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
        GameObject launchBtnObj = new GameObject("EsportsArenaLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, -4);
        launchBtnObj.GetComponent<Image>().color = new Color(0.90f, 0.65f, 0.15f, 0.95f);
        openArenaBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "🏟️";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateFacilityCardUI(Transform parent, ArenaFacilityTier fac)
    {
        GameObject card = new GameObject("FacCard_" + fac.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.24f, 0.16f, 0.06f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.36f, 0.24f, 0.08f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = fac.icon;
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

        GameObject cap = new GameObject("CapTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cap.transform.SetParent(header.transform, false);
        TextMeshProUGUI cTxt = cap.GetComponent<TextMeshProUGUI>();
        cTxt.fontSize = 10;
        cTxt.color = new Color(1f, 0.85f, 0.35f);
        RectTransform cr = cap.GetComponent<RectTransform>();
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
        dTxt.color = new Color(1f, 0.92f, 0.80f);
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
        stTxt.color = new Color(1f, 0.85f, 0.25f);
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
        upBtn.GetComponent<Image>().color = new Color(0.85f, 0.60f, 0.15f, 1f);

        GameObject upTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        upTxt.transform.SetParent(upBtn.transform, false);
        TextMeshProUGUI ut = upTxt.GetComponent<TextMeshProUGUI>();
        ut.fontSize = 11;
        ut.fontStyle = FontStyles.Bold;
        ut.alignment = TextAlignmentOptions.Center;
        ut.color = new Color(0.12f, 0.08f, 0.02f);
        RectTransform utr = upTxt.GetComponent<RectTransform>();
        utr.anchorMin = Vector2.zero;
        utr.anchorMax = Vector2.one;
        utr.offsetMin = Vector2.zero;
        utr.offsetMax = Vector2.zero;

        string currentFacId = fac.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeFacility(currentFacId));
    }
}
