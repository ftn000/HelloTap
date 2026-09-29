using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Биржа Криптовалют и Студийный Майнинг («Studio Crypto Exchange & Rig»):
/// - Майнинг-ферма студии: апгрейд видеокарт (GTX 1060, RTX 3080, RTX 4090, Quantum ASIC Rig)
/// - Автоматический фарм внутриигрового токена «TapCoin» на основе суммарного хешрейта (MH/s)
/// - Динамический курс TapCoin к Рублю с биржевыми колебаниями (быстрая спекуляция)
/// - Интерактивный оверклокинг фермы («⚡ РАЗОГНАТЬ МАЙНИНГ-РИГ»)
/// - Множитель к доходу студии от капитализации криптовалютного кошелька
/// </summary>
public class StudioCryptoMiningUI : MonoBehaviour
{
    private static StudioCryptoMiningUI instance;
    public static StudioCryptoMiningUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StudioCryptoMiningUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(StudioCryptoMiningUI));
                    instance = go.AddComponent<StudioCryptoMiningUI>();
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
    public class MiningHardware
    {
        public string id;
        public string title;
        public string icon;
        public int hashrateMhs;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public int GetCurrentHashrate() => hashrateMhs * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openCryptoBtn;

    [Header("Биржа и баланс кошелька")]
    [SerializeField] private TMP_Text cryptoStatsSummaryTxt;
    [SerializeField] private TMP_Text exchangeRateTxt;
    [SerializeField] private Button overclockBtn;
    [SerializeField] private TMP_Text overclockBtnTxt;
    [SerializeField] private Button sellAllCoinsBtn;
    [SerializeField] private Button buyCoinsBtn;
    [SerializeField] private Transform hardwareContainer;

    private readonly List<MiningHardware> rigs = new List<MiningHardware>();
    private double tapCoinBalance = 0.0;
    private double currentCoinPriceRub = 1200.0;
    private float priceUpdateTimer = 0f;
    private const float PriceUpdateInterval = 4.0f;

    private const string PrefTapCoinBalance = "Crypto_TapCoinBalance";
    private const string PrefHardwarePrefix = "Crypto_HardwareLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public double TapCoinBalance => tapCoinBalance;
    public double CurrentCoinPrice => currentCoinPriceRub;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeRigs();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeRigs()
    {
        if (rigs.Count > 0) return;

        rigs.Add(new MiningHardware
        {
            id = "rig_gtx1060",
            title = "GPU GTX 1060 6GB",
            icon = "📼",
            hashrateMhs = 25,
            level = 1,
            maxLevel = 10,
            baseCostMoney = 35000,
            baseCostCode = 15000
        });

        rigs.Add(new MiningHardware
        {
            id = "rig_rtx3080",
            title = "GPU RTX 3080 10GB",
            icon = "⚡",
            hashrateMhs = 100,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 140000,
            baseCostCode = 70000
        });

        rigs.Add(new MiningHardware
        {
            id = "rig_rtx4090",
            title = "GPU RTX 4090 24GB",
            icon = "🔥",
            hashrateMhs = 260,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 450000,
            baseCostCode = 220000
        });

        rigs.Add(new MiningHardware
        {
            id = "rig_asic_quantum",
            title = "Quantum ASIC Rig 2nm",
            icon = "💎",
            hashrateMhs = 850,
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1600000,
            baseCostCode = 800000
        });
    }

    private void LoadData()
    {
        tapCoinBalance = PlayerPrefs.GetFloat(PrefTapCoinBalance, 0.5f);
        foreach (var r in rigs)
        {
            if (PlayerPrefs.HasKey(PrefHardwarePrefix + r.id))
            {
                r.level = PlayerPrefs.GetInt(PrefHardwarePrefix + r.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat(PrefTapCoinBalance, (float)tapCoinBalance);
        foreach (var r in rigs)
        {
            PlayerPrefs.SetInt(PrefHardwarePrefix + r.id, r.level);
        }
        PlayerPrefs.Save();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Пассивный майнинг токенов в секунду
        int totalHashrate = GetTotalHashrateMhs();
        if (totalHashrate > 0)
        {
            double minedCoins = totalHashrate * 0.00008 * dt;
            tapCoinBalance += minedCoins;
        }

        // 2. Колебание курса TapCoin
        priceUpdateTimer += dt;
        if (priceUpdateTimer >= PriceUpdateInterval)
        {
            priceUpdateTimer = 0f;
            FluctuatePrice();
            if (IsModalOpen) RefreshUI();
        }
    }

    private void FluctuatePrice()
    {
        // Курс колеблется в диапазоне 700 - 3200 ₽
        float deltaPercent = UnityEngine.Random.Range(-0.12f, 0.14f);
        currentCoinPriceRub = Math.Max(700.0, Math.Min(3500.0, currentCoinPriceRub * (1.0 + deltaPercent)));
    }

    public int GetTotalHashrateMhs()
    {
        int sum = 0;
        foreach (var r in rigs)
        {
            sum += r.GetCurrentHashrate();
        }
        return sum;
    }

    public double GetCryptoIncomeMultiplier()
    {
        // До 60% бонус к общему доходу от баланса крипты
        return 1.0 + Math.Min(tapCoinBalance * 0.015, 0.60);
    }

    public void OverclockRigs()
    {
        int hashrate = Math.Max(10, GetTotalHashrateMhs());
        double burstCoins = Math.Max(0.05, hashrate * 0.0005);
        tapCoinBalance += burstCoins;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.15f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"⚡ ОВЕРКЛОКИНГ РИГОВ!\n+{burstCoins:0.000} TapCoin намайнено!", transform.position, new Color(0.2f, 0.9f, 1f), true);
        }
    }

    public void SellAllCoins()
    {
        if (tapCoinBalance < 0.01)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Кошелек пуст! Намайните или купите TapCoin.", transform.position, Color.yellow, false);
            }
            return;
        }

        double earnedMoney = Math.Floor(tapCoinBalance * currentCoinPriceRub);
        double soldCoins = tapCoinBalance;
        tapCoinBalance = 0.0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(earnedMoney);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 ПРОДАНО {soldCoins:0.00} TapCoin!\nПолучено: <b>+{NumberFormatter.Format(earnedMoney)} ₽</b>", transform.position, new Color(0.2f, 1f, 0.4f), true);
        }
    }

    public void BuyCoins()
    {
        double cost = currentCoinPriceRub * 2.0; // Покупка 2 монет
        double curMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;

        if (curMoney < cost)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(cost)} ₽ для покупки 2 TapCoin!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMoney(cost);
        }

        tapCoinBalance += 2.0;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💎 КУПЛЕНО 2.0 TapCoin!\nИнвестиция по курсу {NumberFormatter.Format(currentCoinPriceRub)} ₽", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    public void UpgradeRig(string rigId)
    {
        var rig = rigs.Find(r => r.id == rigId);
        if (rig == null) return;

        if (rig.level >= rig.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Риг уже прокачан до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = rig.GetCostMoney();
        double costCode = rig.GetCostCode();

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

        rig.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"⚡ {rig.title} улучшен до Ур.{rig.level}!\nХешрейт: +{rig.hashrateMhs} MH/s", transform.position, new Color(0.2f, 1f, 0.5f), true);
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
        if (overclockBtn != null)
        {
            overclockBtn.onClick.RemoveAllListeners();
            overclockBtn.onClick.AddListener(OverclockRigs);
        }
        if (sellAllCoinsBtn != null)
        {
            sellAllCoinsBtn.onClick.RemoveAllListeners();
            sellAllCoinsBtn.onClick.AddListener(SellAllCoins);
        }
        if (buyCoinsBtn != null)
        {
            buyCoinsBtn.onClick.RemoveAllListeners();
            buyCoinsBtn.onClick.AddListener(BuyCoins);
        }
        if (openCryptoBtn != null)
        {
            openCryptoBtn.onClick.RemoveAllListeners();
            openCryptoBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (cryptoStatsSummaryTxt != null)
        {
            int totalHs = GetTotalHashrateMhs();
            double mult = GetCryptoIncomeMultiplier();
            cryptoStatsSummaryTxt.text = $"Баланс: <color=#FFD700><b>{tapCoinBalance:0.000} TapCoin</b></color> (≈ {NumberFormatter.Format(tapCoinBalance * currentCoinPriceRub)} ₽)\nХешрейт фермы: <color=#00FFAA><b>{totalHs} MH/s</b></color> | Бонус дохода: <color=#00FFAA>x{mult:0.00}</color>";
        }

        if (exchangeRateTxt != null)
        {
            exchangeRateTxt.text = $"Курс TapCoin: <color=#00FFAA><b>{NumberFormatter.Format(currentCoinPriceRub)} ₽</b></color>";
        }

        if (hardwareContainer == null) return;

        for (int i = 0; i < rigs.Count; i++)
        {
            var rig = rigs[i];
            Transform child = i < hardwareContainer.childCount ? hardwareContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{rig.icon} {rig.title} (Ур. {rig.level}/{rig.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = $"Производительность: <b>{rig.GetCurrentHashrate()} MH/s</b> (+{rig.hashrateMhs} за ур.)";
            }
            if (statsTxt != null)
            {
                statsTxt.text = $"Фарм: <color=#00FFAA>+{(rig.GetCurrentHashrate() * 0.00008 * 60):0.00} TapCoin/мин</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (rig.level >= rig.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = rig.GetCostMoney();
                    double c = rig.GetCostCode();
                    upBtnTxt.text = $"Купить / Апгрейд\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        GameObject root = new GameObject("StudioCryptoMining_ModalRoot", typeof(RectTransform));
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
        GameObject card = new GameObject("CryptoCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.12f, 0.12f, 0.18f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.18f, 0.16f, 0.28f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🪙 БИРЖА TAPCOIN & КРИПТО-МАЙНИНГ";
        tTxt.fontSize = 19;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(1f, 0.82f, 0.2f);
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
        GameObject statsObj = new GameObject("CryptoStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        cryptoStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        cryptoStatsSummaryTxt.fontSize = 13;
        cryptoStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        cryptoStatsSummaryTxt.color = new Color(0.92f, 0.92f, 1f);

        // Exchange Action Panel (Buy / Sell / Overclock)
        GameObject exPanel = new GameObject("ExPanel", typeof(RectTransform), typeof(Image));
        exPanel.transform.SetParent(card.transform, false);
        RectTransform epRect = exPanel.GetComponent<RectTransform>();
        epRect.anchorMin = new Vector2(0f, 1f);
        epRect.anchorMax = new Vector2(1f, 1f);
        epRect.pivot = new Vector2(0.5f, 1f);
        epRect.sizeDelta = new Vector2(-30, 66);
        epRect.anchoredPosition = new Vector2(0, -126);
        exPanel.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.25f, 1f);

        // Overclock Button
        GameObject ocBtnObj = new GameObject("OverclockBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        ocBtnObj.transform.SetParent(exPanel.transform, false);
        RectTransform ocRect = ocBtnObj.GetComponent<RectTransform>();
        ocRect.anchorMin = new Vector2(0.02f, 0.15f);
        ocRect.anchorMax = new Vector2(0.38f, 0.85f);
        ocRect.offsetMin = Vector2.zero;
        ocRect.offsetMax = Vector2.zero;
        ocBtnObj.GetComponent<Image>().color = new Color(0.20f, 0.55f, 0.90f, 1f);
        overclockBtn = ocBtnObj.GetComponent<Button>();

        GameObject ocTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        ocTxtObj.transform.SetParent(ocBtnObj.transform, false);
        overclockBtnTxt = ocTxtObj.GetComponent<TextMeshProUGUI>();
        overclockBtnTxt.text = "⚡ РАЗОГНАТЬ РИГ";
        overclockBtnTxt.fontSize = 11;
        overclockBtnTxt.fontStyle = FontStyles.Bold;
        overclockBtnTxt.alignment = TextAlignmentOptions.Center;
        overclockBtnTxt.color = Color.white;
        RectTransform octr = ocTxtObj.GetComponent<RectTransform>();
        octr.anchorMin = Vector2.zero;
        octr.anchorMax = Vector2.one;
        octr.offsetMin = Vector2.zero;
        octr.offsetMax = Vector2.zero;

        // Buy 2 Coins Button
        GameObject bBtnObj = new GameObject("BuyCoinsBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        bBtnObj.transform.SetParent(exPanel.transform, false);
        RectTransform bbRect = bBtnObj.GetComponent<RectTransform>();
        bbRect.anchorMin = new Vector2(0.40f, 0.15f);
        bbRect.anchorMax = new Vector2(0.68f, 0.85f);
        bbRect.offsetMin = Vector2.zero;
        bbRect.offsetMax = Vector2.zero;
        bBtnObj.GetComponent<Image>().color = new Color(0.22f, 0.65f, 0.35f, 1f);
        buyCoinsBtn = bBtnObj.GetComponent<Button>();

        GameObject bTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bTxtObj.transform.SetParent(bBtnObj.transform, false);
        TextMeshProUGUI bt = bTxtObj.GetComponent<TextMeshProUGUI>();
        bt.text = "КУПИТЬ 2 TC";
        bt.fontSize = 11;
        bt.fontStyle = FontStyles.Bold;
        bt.alignment = TextAlignmentOptions.Center;
        bt.color = Color.white;
        RectTransform btr = bTxtObj.GetComponent<RectTransform>();
        btr.anchorMin = Vector2.zero;
        btr.anchorMax = Vector2.one;
        btr.offsetMin = Vector2.zero;
        btr.offsetMax = Vector2.zero;

        // Sell All Button
        GameObject sBtnObj = new GameObject("SellCoinsBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        sBtnObj.transform.SetParent(exPanel.transform, false);
        RectTransform sbRect = sBtnObj.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0.70f, 0.15f);
        sbRect.anchorMax = new Vector2(0.98f, 0.85f);
        sbRect.offsetMin = Vector2.zero;
        sbRect.offsetMax = Vector2.zero;
        sBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.15f, 1f);
        sellAllCoinsBtn = sBtnObj.GetComponent<Button>();

        GameObject sTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sTxtObj.transform.SetParent(sBtnObj.transform, false);
        TextMeshProUGUI stt = sTxtObj.GetComponent<TextMeshProUGUI>();
        stt.text = "ПРОДАТЬ ВСЁ";
        stt.fontSize = 11;
        stt.fontStyle = FontStyles.Bold;
        stt.alignment = TextAlignmentOptions.Center;
        stt.color = Color.white;
        RectTransform str = sTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = Vector2.zero;
        str.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("RigScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -200);
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
        hardwareContainer = content.transform;
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

        foreach (var rig in rigs)
        {
            CreateRigCardUI(content.transform, rig);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.24f, 0.24f, 0.35f, 1f);
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

    private void CreateRigCardUI(Transform parent, MiningHardware rig)
    {
        GameObject card = new GameObject("RigCard_" + rig.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 90);
        card.GetComponent<Image>().color = new Color(0.14f, 0.15f, 0.22f, 1f);

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
        upBtn.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.88f, 1f);

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

        string currentRigId = rig.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeRig(currentRigId));
    }
}
