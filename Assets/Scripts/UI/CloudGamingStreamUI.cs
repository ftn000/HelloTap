using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальная Стриминговая Облачная Платформа Студии («Cloud Gaming Stream Fabric»):
/// - Собственная облачная игровая платформа по подписке (без скачивания игр)
/// - Серверные GPU-фермы и стриминг 4K/120fps с задержкой 2ms
/// - 4 уровня облачной инфраструктуры:
///   1. ☁️ Edge EdgePOP Relay (+250 ₽/сек, +8% к доходу)
///   2. 🎮 Multi-GPU Virtual Rig (+900 ₽/сек, +12% к клику)
///   3. 🚀 Neural AV1 Compression Engine (+3000 ₽/сек, +20% к доходу)
///   4. 🌐 Global Hyperscale Cloud Mesh (+10000 ₽/сек, x1.35 ко всему доходу)
/// - Интерактивное действие: «☁️ БЕСПЛАТНЫЙ ОБЛАЧНЫЙ УИКЕНД» (наплыв подписчиков, комбо +35% и кассовый сбор)
/// - Пассивный доход в рублях в секунду от ежемесячной абонентской платы игроков
/// </summary>
public class CloudGamingStreamUI : MonoBehaviour
{
    private static CloudGamingStreamUI instance;
    public static CloudGamingStreamUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CloudGamingStreamUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CloudGamingStreamUI));
                    instance = go.AddComponent<CloudGamingStreamUI>();
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
    public class CloudFabricTier
    {
        public string id;
        public string title;
        public string techSpecs;
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
    [SerializeField] private Button openCloudBtn;

    [Header("Панель облака")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button freeWeekendBtn;
    [SerializeField] private TMP_Text freeWeekendBtnTxt;
    [SerializeField] private Transform fabricContainer;

    [Header("Уровни облака")]
    [SerializeField] private List<CloudFabricTier> tiers = new List<CloudFabricTier>();

    private const string PrefWeekendsCount = "Cloud_Weekends_Count";
    private const string PrefTierLvlPrefix = "Cloud_Tier_Lvl_";

    private int freeWeekendsHostedCount = 0;
    private bool isHostingWeekend = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitTiersList();
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

    private void InitTiersList()
    {
        if (tiers != null && tiers.Count > 0) return;

        tiers = new List<CloudFabricTier>
        {
            new CloudFabricTier
            {
                id = "cloud_edge_pop",
                title = "EdgePOP Relay Nodes",
                techSpecs = "Точки присутствия в 40 дата-центрах • Пинг < 15ms",
                icon = "☁️",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 8500.0,
                baseCostCode = 5000.0,
                moneyPerSecBonus = 250.0,
                incomeMultiplierBonus = 0.08f,
                clickMultiplierBonus = 0.06f
            },
            new CloudFabricTier
            {
                id = "cloud_multi_gpu",
                title = "Multi-GPU Virtual Rig",
                techSpecs = "Кластеры RTX 5090 с аппаратной трассировкой пути",
                icon = "🎮",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 38000.0,
                baseCostCode = 24000.0,
                moneyPerSecBonus = 900.0,
                incomeMultiplierBonus = 0.12f,
                clickMultiplierBonus = 0.12f
            },
            new CloudFabricTier
            {
                id = "cloud_neural_av1",
                title = "Neural AV1 Encoder Mesh",
                techSpecs = "Нейросетевое сжатие видеопотока 4K 120fps без потерь",
                icon = "🚀",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 165000.0,
                baseCostCode = 105000.0,
                moneyPerSecBonus = 3000.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.16f
            },
            new CloudFabricTier
            {
                id = "cloud_hyperscale",
                title = "Hyperscale Cloud Fabric",
                techSpecs = "Глобальная распределенная сеть на 10 000 000 стримеров",
                icon = "🌐",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 700000.0,
                baseCostCode = 480000.0,
                moneyPerSecBonus = 10000.0,
                incomeMultiplierBonus = 0.35f,
                clickMultiplierBonus = 0.25f
            }
        };
    }

    private void LoadData()
    {
        freeWeekendsHostedCount = PlayerPrefs.GetInt(PrefWeekendsCount, 0);
        foreach (var t in tiers)
        {
            if (PlayerPrefs.HasKey(PrefTierLvlPrefix + t.id))
            {
                t.level = PlayerPrefs.GetInt(PrefTierLvlPrefix + t.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefWeekendsCount, freeWeekendsHostedCount);
        foreach (var t in tiers)
        {
            PlayerPrefs.SetInt(PrefTierLvlPrefix + t.id, t.level);
        }
        PlayerPrefs.Save();
    }

    public double GetCloudStreamIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var t in tiers)
        {
            mult += t.level * t.incomeMultiplierBonus;
        }
        mult += Math.Min(freeWeekendsHostedCount * 0.015, 0.60);
        return mult;
    }

    public double GetCloudStreamClickMultiplier()
    {
        double mult = 1.0;
        foreach (var t in tiers)
        {
            mult += t.level * t.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetCloudStreamMoneyPerSec()
    {
        double sum = 0;
        foreach (var t in tiers)
        {
            sum += t.GetTotalMps();
        }
        return sum;
    }

    public void HostFreeWeekend()
    {
        if (isHostingWeekend) return;
        StartCoroutine(FreeWeekendRoutine());
    }

    private IEnumerator FreeWeekendRoutine()
    {
        isHostingWeekend = true;
        if (freeWeekendBtn != null) freeWeekendBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (freeWeekendBtnTxt != null)
        {
            freeWeekendBtnTxt.text = "☁️ МИЛЛИОНЫ ИГРОКОВ В ОБЛАКЕ... НАПЛЫВ ПОДПИСЧИКОВ!";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        freeWeekendsHostedCount++;
        double baseRevenue = Math.Max(50000.0, GetCloudStreamMoneyPerSec() * 60.0);
        double rewardMoney = Math.Floor(baseRevenue * GetCloudStreamIncomeMultiplier());
        double rewardCode = Math.Floor(rewardMoney * 0.40 * GetCloudStreamClickMultiplier());

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
            ClickJuice.Instance.SpawnCustomPopup($"☁️ ОБЛАЧНЫЙ СТРИМИНГ ВЗОРВАЛ СЕТЬ!\nПодписки: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        isHostingWeekend = false;
        if (freeWeekendBtn != null) freeWeekendBtn.interactable = true;
        if (freeWeekendBtnTxt != null) freeWeekendBtnTxt.text = "☁️ ЗАПУСК БЕСПЛАТНЫХ ВЫХОДНЫХ В ОБЛАКЕ";
    }

    public void UpgradeTier(string tierId)
    {
        var tier = tiers.Find(t => t.id == tierId);
        if (tier == null) return;

        if (tier.level >= tier.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Облачный узел развит до предела!", transform.position, Color.yellow, false);
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
            ClickJuice.Instance.SpawnCustomPopup($"☁️ {tier.title} УРОВЕНЬ {tier.level}!\n+{tier.moneyPerSecBonus:F0} ₽/сек • +{(tier.incomeMultiplierBonus * 100):F0}% Доход", transform.position, new Color(0.3f, 0.85f, 1f), true);
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
            double totalMps = GetCloudStreamMoneyPerSec();
            double incMult = GetCloudStreamIncomeMultiplier();
            double clkMult = GetCloudStreamClickMultiplier();
            statsSummaryTxt.text = $"☁️ Облачные подписки: <b>+{NumberFormatter.Format(totalMps)} ₽/сек</b>\nМножитель дохода: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b> (Промо-акций: {freeWeekendsHostedCount})";
        }

        if (fabricContainer != null)
        {
            for (int i = 0; i < fabricContainer.childCount; i++)
            {
                Transform child = fabricContainer.GetChild(i);
                if (i < tiers.Count)
                {
                    UpdateFabricCard(child, tiers[i]);
                }
            }
        }
    }

    private void UpdateFabricCard(Transform card, CloudFabricTier tier)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text specsTxt = card.Find("Header/SpecsTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{tier.title} (Ур. {tier.level}/{tier.maxLevel})";
        if (specsTxt != null) specsTxt.text = tier.techSpecs;
        if (iconTxt != null) iconTxt.text = tier.icon;
        if (descTxt != null) descTxt.text = $"+{tier.moneyPerSecBonus:F0} ₽/сек • +{(tier.incomeMultiplierBonus * 100):F0}% Доход • +{(tier.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущая абонплата: +{NumberFormatter.Format(tier.GetTotalMps())} ₽/сек";

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
                upBtnTxt.text = $"Развернуть\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openCloudBtn != null) openCloudBtn.onClick.AddListener(OpenModal);
        if (freeWeekendBtn != null) freeWeekendBtn.onClick.AddListener(HostFreeWeekend);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("CloudGamingModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.01f, 0.06f, 0.10f, 0.85f);
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
        cardImg.color = new Color(0.06f, 0.14f, 0.24f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "☁️ CLOUD GAMING STREAM FABRIC";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.2f, 0.85f, 1f);
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
        subTxt.text = "Облачная платформа потокового гейминга с GPU-нодами по всему миру";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.75f, 0.90f, 1f);
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
        summaryBox.GetComponent<Image>().color = new Color(0.10f, 0.20f, 0.35f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.2f, 0.85f, 1f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Запуск бесплатного уикенда)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.12f, 0.24f, 0.40f, 1f);

        GameObject wBtnObj = new GameObject("FreeWeekendBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        wBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform wbR = wBtnObj.GetComponent<RectTransform>();
        wbR.anchorMin = Vector2.zero;
        wbR.anchorMax = Vector2.one;
        wbR.offsetMin = new Vector2(8, 6);
        wbR.offsetMax = new Vector2(-8, -6);
        wBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.60f, 0.95f, 1f);
        freeWeekendBtn = wBtnObj.GetComponent<Button>();

        GameObject wTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        wTxtObj.transform.SetParent(wBtnObj.transform, false);
        freeWeekendBtnTxt = wTxtObj.GetComponent<TextMeshProUGUI>();
        freeWeekendBtnTxt.text = "☁️ ЗАПУСК БЕСПЛАТНЫХ ВЫХОДНЫХ В ОБЛАКЕ";
        freeWeekendBtnTxt.fontSize = 14;
        freeWeekendBtnTxt.fontStyle = FontStyles.Bold;
        freeWeekendBtnTxt.alignment = TextAlignmentOptions.Center;
        freeWeekendBtnTxt.color = Color.white;
        RectTransform wtr = wTxtObj.GetComponent<RectTransform>();
        wtr.anchorMin = Vector2.zero;
        wtr.anchorMax = Vector2.one;
        wtr.offsetMin = Vector2.zero;
        wtr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("FabricScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.5f);

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
        fabricContainer = content.transform;
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
        foreach (var t in tiers)
        {
            CreateFabricCardUI(content.transform, t);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.32f, 0.48f, 1f);
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
        GameObject launchBtnObj = new GameObject("CloudFabricLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, -104);
        launchBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.60f, 0.95f, 0.95f);
        openCloudBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "☁️";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateFabricCardUI(Transform parent, CloudFabricTier tier)
    {
        GameObject card = new GameObject("FabricCard_" + tier.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.10f, 0.20f, 0.32f, 0.9f);

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
        iconBox.GetComponent<Image>().color = new Color(0.16f, 0.30f, 0.48f, 1f);

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

        GameObject specs = new GameObject("SpecsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        specs.transform.SetParent(header.transform, false);
        TextMeshProUGUI sTxt = specs.GetComponent<TextMeshProUGUI>();
        sTxt.fontSize = 10;
        sTxt.color = new Color(0.35f, 0.85f, 1f);
        RectTransform sr = specs.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 0f);
        sr.anchorMax = new Vector2(1f, 0.5f);
        sr.offsetMin = Vector2.zero;
        sr.offsetMax = Vector2.zero;

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
        dTxt.color = new Color(0.85f, 0.92f, 1f);
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
        stTxt.color = new Color(0.2f, 0.9f, 1f);
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
        upBtn.GetComponent<Image>().color = new Color(0.18f, 0.55f, 0.88f, 1f);

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

        string currentTierId = tier.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeTier(currentTierId));
    }
}
