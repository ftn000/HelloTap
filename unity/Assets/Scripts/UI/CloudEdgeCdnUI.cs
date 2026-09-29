using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Интерактивная Глобальная Карта Серверов («Cloud Edge CDN & Data Centers»):
/// - 5 глобальных локаций дата-центров по всему миру:
///   1. Франкфурт (EU-Central-1) - Надежность и европейский трафик
///   2. Токио (AP-Northeast-1) - Минимальный пинг и азиатский рынок
///   3. Вирджиния (US-East-1) - Крупнейший серверный кластер с высокой пропускной способностью
///   4. Сингапур (AP-Southeast-1) - Высокоскоростной азиатский шлюз
///   5. Сан-Паулу (SA-East-1) - Латиноамериканский развивающийся хаб
/// - Апгрейд узлов: расширение пропускной способности (Gbps), оптимизация сетевых маршрутов
/// - Интерактивный пинг-тест («ПРОВЕРИТЬ СЕТЕВОЙ ПИНГ / МАРШРУТИЗАЦИЯ»): сбор телеметрии, сетевой буст
/// - Постоянные множители к пассивному доходу, силе клика и расчету оффлайн-прибыли
/// </summary>
public class CloudEdgeCdnUI : MonoBehaviour
{
    private static CloudEdgeCdnUI instance;
    public static CloudEdgeCdnUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CloudEdgeCdnUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CloudEdgeCdnUI));
                    instance = go.AddComponent<CloudEdgeCdnUI>();
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
    public class DataCenterNode
    {
        public string id;
        public string regionName;
        public string city;
        public string icon;
        public int basePingMs;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonusPerLevel;
        public float clickMultiplierBonusPerLevel;
        public float offlineMultiplierBonusPerLevel;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public int GetCurrentPing() => Math.Max(2, basePingMs - (level * 3));
        public int GetBandwidthGbps() => 10 + (level * 25);
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openCdnBtn;

    [Header("Сетевой статус и пинг")]
    [SerializeField] private TMP_Text cdnStatsSummaryTxt;
    [SerializeField] private Button pingTestBtn;
    [SerializeField] private TMP_Text pingTestBtnTxt;
    [SerializeField] private Transform nodesContainer;

    private readonly List<DataCenterNode> nodes = new List<DataCenterNode>();
    private bool isTestingPing = false;

    private const string PrefNodeLevelPrefix = "Cdn_NodeLvl_";
    private const string PrefPingTestsCount = "Cdn_PingTestsCount";
    private int pingTestsCompleted = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeNodes();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeNodes()
    {
        if (nodes.Count > 0) return;

        nodes.Add(new DataCenterNode
        {
            id = "node_frankfurt",
            regionName = "EU-Central-1",
            city = "Франкфурт",
            icon = "🇩🇪",
            basePingMs = 28,
            level = 1,
            maxLevel = 10,
            baseCostMoney = 40000,
            baseCostCode = 18000,
            incomeMultiplierBonusPerLevel = 0.08f,
            clickMultiplierBonusPerLevel = 0.04f,
            offlineMultiplierBonusPerLevel = 0.06f
        });

        nodes.Add(new DataCenterNode
        {
            id = "node_tokyo",
            regionName = "AP-Northeast-1",
            city = "Токио",
            icon = "🇯🇵",
            basePingMs = 38,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 120000,
            baseCostCode = 60000,
            incomeMultiplierBonusPerLevel = 0.12f,
            clickMultiplierBonusPerLevel = 0.08f,
            offlineMultiplierBonusPerLevel = 0.08f
        });

        nodes.Add(new DataCenterNode
        {
            id = "node_virginia",
            regionName = "US-East-1",
            city = "Вирджиния",
            icon = "🇺🇸",
            basePingMs = 32,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 350000,
            baseCostCode = 175000,
            incomeMultiplierBonusPerLevel = 0.18f,
            clickMultiplierBonusPerLevel = 0.12f,
            offlineMultiplierBonusPerLevel = 0.15f
        });

        nodes.Add(new DataCenterNode
        {
            id = "node_singapore",
            regionName = "AP-Southeast-1",
            city = "Сингапур",
            icon = "🇸🇬",
            basePingMs = 45,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 900000,
            baseCostCode = 450000,
            incomeMultiplierBonusPerLevel = 0.22f,
            clickMultiplierBonusPerLevel = 0.15f,
            offlineMultiplierBonusPerLevel = 0.18f
        });

        nodes.Add(new DataCenterNode
        {
            id = "node_saopaulo",
            regionName = "SA-East-1",
            city = "Сан-Паулу",
            icon = "🇧🇷",
            basePingMs = 55,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 2400000,
            baseCostCode = 1200000,
            incomeMultiplierBonusPerLevel = 0.30f,
            clickMultiplierBonusPerLevel = 0.20f,
            offlineMultiplierBonusPerLevel = 0.25f
        });
    }

    private void LoadData()
    {
        pingTestsCompleted = PlayerPrefs.GetInt(PrefPingTestsCount, 0);
        foreach (var node in nodes)
        {
            if (PlayerPrefs.HasKey(PrefNodeLevelPrefix + node.id))
            {
                node.level = PlayerPrefs.GetInt(PrefNodeLevelPrefix + node.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefPingTestsCount, pingTestsCompleted);
        foreach (var node in nodes)
        {
            PlayerPrefs.SetInt(PrefNodeLevelPrefix + node.id, node.level);
        }
        PlayerPrefs.Save();
    }

    public double GetCdnIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var node in nodes)
        {
            mult += node.level * node.incomeMultiplierBonusPerLevel;
        }
        return mult;
    }

    public double GetCdnClickMultiplier()
    {
        double mult = 1.0;
        foreach (var node in nodes)
        {
            mult += node.level * node.clickMultiplierBonusPerLevel;
        }
        return mult;
    }

    public double GetCdnOfflineMultiplier()
    {
        double mult = 1.0;
        foreach (var node in nodes)
        {
            mult += node.level * node.offlineMultiplierBonusPerLevel;
        }
        return mult;
    }

    public int GetTotalBandwidthGbps()
    {
        int total = 0;
        foreach (var node in nodes)
        {
            if (node.level > 0)
            {
                total += node.GetBandwidthGbps();
            }
        }
        return total;
    }

    public int GetAveragePing()
    {
        int activeCount = 0;
        int sum = 0;
        foreach (var node in nodes)
        {
            if (node.level > 0)
            {
                activeCount++;
                sum += node.GetCurrentPing();
            }
        }
        return activeCount > 0 ? (sum / activeCount) : 99;
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
        if (pingTestBtn != null)
        {
            pingTestBtn.onClick.RemoveAllListeners();
            pingTestBtn.onClick.AddListener(OnPingTestClicked);
        }
        if (openCdnBtn != null)
        {
            openCdnBtn.onClick.RemoveAllListeners();
            openCdnBtn.onClick.AddListener(OpenModal);
        }
    }

    private void OnPingTestClicked()
    {
        if (isTestingPing) return;
        StartCoroutine(PingTestRoutine());
    }

    private IEnumerator PingTestRoutine()
    {
        isTestingPing = true;
        if (pingTestBtn != null) pingTestBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (pingTestBtnTxt != null)
        {
            pingTestBtnTxt.text = "📡 СКАНИРОВАНИЕ BGP И ПРОВЕРКА ПИНГА...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        pingTestsCompleted++;
        int avgPing = GetAveragePing();
        int totalGbps = GetTotalBandwidthGbps();

        double baseReward = Math.Max(15000.0, totalGbps * 120.0);
        double rewardMoney = Math.Floor(baseReward * GetCdnIncomeMultiplier());
        double rewardCode = Math.Floor(baseReward * 0.35 * GetCdnClickMultiplier());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.25f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🌐 ТЕСТ CDN ЗАВЕРШЕН!\nСр. пинг: <b>{avgPing} мс</b> | Сеть: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        isTestingPing = false;
        if (pingTestBtn != null) pingTestBtn.interactable = true;
        if (pingTestBtnTxt != null) pingTestBtnTxt.text = "🌐 ПРОВЕРИТЬ СЕТЕВОЙ ПИНГ / МАРШРУТЫ";
    }

    public void UpgradeNode(string nodeId)
    {
        var node = nodes.Find(n => n.id == nodeId);
        if (node == null) return;

        if (node.level >= node.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Узел CDN уже прокачан до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = node.GetCostMoney();
        double costCode = node.GetCostCode();

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

        node.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"⚡ Дата-центр {node.city} улучшен до Ур.{node.level}!\nКанал: {node.GetBandwidthGbps()} Gbps | Пинг: {node.GetCurrentPing()} мс", transform.position, new Color(0.2f, 1f, 0.5f), true);
        }
    }

    public void RefreshUI()
    {
        if (cdnStatsSummaryTxt != null)
        {
            double incMult = GetCdnIncomeMultiplier();
            double clkMult = GetCdnClickMultiplier();
            double offMult = GetCdnOfflineMultiplier();
            int avgPing = GetAveragePing();
            int totalGbps = GetTotalBandwidthGbps();

            cdnStatsSummaryTxt.text = $"Глобальная сеть: <color=#00FFAA>{totalGbps} Gbps</color> | Ср. Пинг: <color=#FFD700>{avgPing} мс</color>\nДоход: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color> | Оффлайн: <color=#38B6FF>x{offMult:0.00}</color>";
        }

        if (pingTestBtnTxt != null && !isTestingPing)
        {
            pingTestBtnTxt.text = "🌐 ПРОВЕРИТЬ СЕТЕВОЙ ПИНГ / МАРШРУТЫ";
        }

        if (nodesContainer == null) return;

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            Transform child = i < nodesContainer.childCount ? nodesContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{node.icon} {node.city} ({node.regionName}) - Ур. {node.level}/{node.maxLevel}";
            }
            if (descTxt != null)
            {
                descTxt.text = $"Канал связи: <b>{node.GetBandwidthGbps()} Gbps</b> | Задержка: <b>{node.GetCurrentPing()} мс</b>";
            }
            if (statsTxt != null)
            {
                float totalInc = node.level * node.incomeMultiplierBonusPerLevel * 100f;
                float totalOff = node.level * node.offlineMultiplierBonusPerLevel * 100f;
                statsTxt.text = $"Бонус: <color=#00FFAA>+{totalInc:0}% доход</color> | <color=#38B6FF>+{totalOff:0}% оффлайн</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (node.level >= node.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = node.GetCostMoney();
                    double c = node.GetCostCode();
                    upBtnTxt.text = $"Аренда / Апгрейд\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        GameObject root = new GameObject("CloudEdgeCdn_ModalRoot", typeof(RectTransform));
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
        bImg.color = new Color(0.03f, 0.05f, 0.09f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("CdnCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.10f, 0.13f, 0.20f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.14f, 0.20f, 0.32f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🌐 ОБЛАЧНАЯ СЕТЬ CDN & ДАТА-ЦЕНТРЫ";
        tTxt.fontSize = 19;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(0.35f, 0.85f, 1f);
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
        GameObject statsObj = new GameObject("CdnStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        cdnStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        cdnStatsSummaryTxt.fontSize = 13;
        cdnStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        cdnStatsSummaryTxt.color = new Color(0.88f, 0.92f, 1f);

        // Ping test action button
        GameObject pingPanel = new GameObject("PingPanel", typeof(RectTransform), typeof(Image));
        pingPanel.transform.SetParent(card.transform, false);
        RectTransform ppRect = pingPanel.GetComponent<RectTransform>();
        ppRect.anchorMin = new Vector2(0f, 1f);
        ppRect.anchorMax = new Vector2(1f, 1f);
        ppRect.pivot = new Vector2(0.5f, 1f);
        ppRect.sizeDelta = new Vector2(-30, 52);
        ppRect.anchoredPosition = new Vector2(0, -126);
        pingPanel.GetComponent<Image>().color = new Color(0.13f, 0.19f, 0.30f, 1f);

        GameObject pBtnObj = new GameObject("PingTestBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pBtnObj.transform.SetParent(pingPanel.transform, false);
        RectTransform pbRect = pBtnObj.GetComponent<RectTransform>();
        pbRect.anchorMin = Vector2.zero;
        pbRect.anchorMax = Vector2.one;
        pbRect.offsetMin = new Vector2(8, 6);
        pbRect.offsetMax = new Vector2(-8, -6);
        pBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.58f, 0.88f, 1f);
        pingTestBtn = pBtnObj.GetComponent<Button>();

        GameObject ptTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        ptTxtObj.transform.SetParent(pBtnObj.transform, false);
        pingTestBtnTxt = ptTxtObj.GetComponent<TextMeshProUGUI>();
        pingTestBtnTxt.text = "🌐 ПРОВЕРИТЬ СЕТЕВОЙ ПИНГ / МАРШРУТЫ";
        pingTestBtnTxt.fontSize = 14;
        pingTestBtnTxt.fontStyle = FontStyles.Bold;
        pingTestBtnTxt.alignment = TextAlignmentOptions.Center;
        pingTestBtnTxt.color = Color.white;
        RectTransform ptr = ptTxtObj.GetComponent<RectTransform>();
        ptr.anchorMin = Vector2.zero;
        ptr.anchorMax = Vector2.one;
        ptr.offsetMin = Vector2.zero;
        ptr.offsetMax = Vector2.zero;

        // Nodes Scroll View
        GameObject scrollObj = new GameObject("NodesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
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
        nodesContainer = content.transform;
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

        foreach (var node in nodes)
        {
            CreateNodeCardUI(content.transform, node);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.26f, 0.38f, 1f);
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

    private void CreateNodeCardUI(Transform parent, DataCenterNode node)
    {
        GameObject card = new GameObject("NodeCard_" + node.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 92);
        card.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.25f, 1f);

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
        tTxt.color = new Color(0.35f, 0.88f, 1f);
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
        upBtn.GetComponent<Image>().color = new Color(0.20f, 0.50f, 0.85f, 1f);

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

        string currentNodeId = node.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeNode(currentNodeId));
    }
}
