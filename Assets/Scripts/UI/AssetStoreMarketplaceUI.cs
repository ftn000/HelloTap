using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Аукцион Студийных Ассетов и 3D-Моделей («Global Asset Store Marketplace»):
/// - Разработка и публикация пакетов ассетов в мировой маркетплейс:
///   1. 🎨 ShaderGraph Master Collection (50 VFX шейдеров)
///   2. 🏰 Dungeon Procedural Architecture Tool (Процедурные уровни)
///   3. 🌆 Low-Poly Cyberpunk Mega-City Pack (3D-окружение и текстуры)
///   4. 🛰️ Distributed Netcode & Physics Suite (Сетевой фреймворк)
/// - Пассивный доход в рублях каждую секунду от продаж лицензий сторонним разработчикам
/// - Интерактивное действие: «📢 ЗАПУСТИТЬ РАСПРОДАЖУ BLACK FRIDAY» (Flash Sale)
/// - Множители пассивного дохода и силы клика от коммерческого успеха магазина
/// </summary>
public class AssetStoreMarketplaceUI : MonoBehaviour
{
    private static AssetStoreMarketplaceUI instance;
    public static AssetStoreMarketplaceUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AssetStoreMarketplaceUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(AssetStoreMarketplaceUI));
                    instance = go.AddComponent<AssetStoreMarketplaceUI>();
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
    public class MarketplaceAssetPackage
    {
        public string id;
        public string title;
        public string category;
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
    [SerializeField] private Button openMarketplaceBtn;

    [Header("Маркетплейс и распродажа")]
    [SerializeField] private TMP_Text marketplaceStatsSummaryTxt;
    [SerializeField] private Button flashSaleBtn;
    [SerializeField] private TMP_Text flashSaleBtnTxt;
    [SerializeField] private Transform packagesContainer;

    private readonly List<MarketplaceAssetPackage> packages = new List<MarketplaceAssetPackage>();
    private bool isFlashSaleActive = false;
    private int flashSalesCount = 0;

    private const string PrefPackageLvlPrefix = "AssetStore_PkgLvl_";
    private const string PrefSalesCount = "AssetStore_SalesCount";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializePackages();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializePackages()
    {
        if (packages.Count > 0) return;

        packages.Add(new MarketplaceAssetPackage
        {
            id = "pkg_shaders",
            title = "ShaderGraph Master Collection",
            category = "50 оптимизированных VFX шейдеров",
            icon = "🎨",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 35000,
            baseCostCode = 16000,
            moneyPerSecBonus = 150.0,
            incomeMultiplierBonus = 0.08f,
            clickMultiplierBonus = 0.05f
        });

        packages.Add(new MarketplaceAssetPackage
        {
            id = "pkg_proc_dungeon",
            title = "Dungeon Procedural Architecture Tool",
            category = "Генератор лабиринтов и комнат на C#",
            icon = "🏰",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 130000,
            baseCostCode = 65000,
            moneyPerSecBonus = 500.0,
            incomeMultiplierBonus = 0.14f,
            clickMultiplierBonus = 0.10f
        });

        packages.Add(new MarketplaceAssetPackage
        {
            id = "pkg_cyber_models",
            title = "Low-Poly Cyberpunk Mega-City Pack",
            category = "250 уникальных 3D-моделей и неоновых пропсов",
            icon = "🌆",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 420000,
            baseCostCode = 210000,
            moneyPerSecBonus = 1600.0,
            incomeMultiplierBonus = 0.22f,
            clickMultiplierBonus = 0.15f
        });

        packages.Add(new MarketplaceAssetPackage
        {
            id = "pkg_netcode_physics",
            title = "Distributed Netcode & Physics Suite",
            category = "Сетевая синхронизация и авто-балансировка лагов",
            icon = "🛰️",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1400000,
            baseCostCode = 700000,
            moneyPerSecBonus = 5000.0,
            incomeMultiplierBonus = 0.35f,
            clickMultiplierBonus = 0.25f
        });
    }

    private void LoadData()
    {
        flashSalesCount = PlayerPrefs.GetInt(PrefSalesCount, 0);
        foreach (var p in packages)
        {
            if (PlayerPrefs.HasKey(PrefPackageLvlPrefix + p.id))
            {
                p.level = PlayerPrefs.GetInt(PrefPackageLvlPrefix + p.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSalesCount, flashSalesCount);
        foreach (var p in packages)
        {
            PlayerPrefs.SetInt(PrefPackageLvlPrefix + p.id, p.level);
        }
        PlayerPrefs.Save();
    }

    public double GetMarketplaceIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var p in packages)
        {
            mult += p.level * p.incomeMultiplierBonus;
        }
        mult += Math.Min(flashSalesCount * 0.015, 0.60);
        return mult;
    }

    public double GetMarketplaceClickMultiplier()
    {
        double mult = 1.0;
        foreach (var p in packages)
        {
            mult += p.level * p.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetMarketplaceMoneyPerSec()
    {
        double sum = 0;
        foreach (var p in packages)
        {
            sum += p.GetTotalMps();
        }
        return sum;
    }

    public void LaunchFlashSale()
    {
        if (isFlashSaleActive) return;
        StartCoroutine(FlashSaleRoutine());
    }

    private IEnumerator FlashSaleRoutine()
    {
        isFlashSaleActive = true;
        if (flashSaleBtn != null) flashSaleBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (flashSaleBtnTxt != null)
        {
            flashSaleBtnTxt.text = "📢 BLACK FRIDAY SALE! АССЕТЫ ПРОДАЮТСЯ С 70% СКИДКОЙ...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        flashSalesCount++;
        double baseSales = Math.Max(30000.0, GetMarketplaceMoneyPerSec() * 60.0);
        double rewardMoney = Math.Floor(baseSales * GetMarketplaceIncomeMultiplier());
        double rewardCode = Math.Floor(rewardMoney * 0.35 * GetMarketplaceClickMultiplier());

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
            ClickJuice.Instance.SpawnCustomPopup($"📢 РАСПРОДАЖА ЗАВЕРШЕНА!\nВыручка магазина: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isFlashSaleActive = false;
        if (flashSaleBtn != null) flashSaleBtn.interactable = true;
        if (flashSaleBtnTxt != null) flashSaleBtnTxt.text = "📢 ЗАПУСТИТЬ РАСПРОДАЖУ BLACK FRIDAY";
    }

    public void UpgradePackage(string pkgId)
    {
        var pkg = packages.Find(p => p.id == pkgId);
        if (pkg == null) return;

        if (pkg.level >= pkg.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Пакет уже улучшен до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = pkg.GetCostMoney();
        double costCode = pkg.GetCostCode();

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

        pkg.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Пакет '{pkg.title}' обновлен до v{pkg.level}.0!\nПассивный доход: +{pkg.moneyPerSecBonus} ₽/сек", transform.position, new Color(0.2f, 1f, 0.5f), true);
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
        if (flashSaleBtn != null)
        {
            flashSaleBtn.onClick.RemoveAllListeners();
            flashSaleBtn.onClick.AddListener(LaunchFlashSale);
        }
        if (openMarketplaceBtn != null)
        {
            openMarketplaceBtn.onClick.RemoveAllListeners();
            openMarketplaceBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (marketplaceStatsSummaryTxt != null)
        {
            double mps = GetMarketplaceMoneyPerSec();
            double incMult = GetMarketplaceIncomeMultiplier();
            double clkMult = GetMarketplaceClickMultiplier();
            marketplaceStatsSummaryTxt.text = $"Роялти с маркетплейса: <color=#00FFAA><b>+{NumberFormatter.Format(mps)} ₽/сек</b></color> | Распродаж: <b>{flashSalesCount}</b>\nДоход: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>";
        }

        if (flashSaleBtnTxt != null && !isFlashSaleActive)
        {
            flashSaleBtnTxt.text = "📢 ЗАПУСТИТЬ РАСПРОДАЖУ BLACK FRIDAY";
        }

        if (packagesContainer == null) return;

        for (int i = 0; i < packages.Count; i++)
        {
            var pkg = packages[i];
            Transform child = i < packagesContainer.childCount ? packagesContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{pkg.icon} {pkg.title} (v{pkg.level}.0)";
            }
            if (descTxt != null)
            {
                descTxt.text = pkg.category;
            }
            if (statsTxt != null)
            {
                float totalInc = pkg.level * pkg.incomeMultiplierBonus * 100f;
                statsTxt.text = $"Продажи: <color=#00FFAA>+{pkg.GetTotalMps()} ₽/сек</color> | Бонус: <color=#FFD700>+{totalInc:0}% доход</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (pkg.level >= pkg.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = pkg.GetCostMoney();
                    double c = pkg.GetCostCode();
                    upBtnTxt.text = $"Релиз v{pkg.level + 1}.0\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        GameObject root = new GameObject("AssetStore_ModalRoot", typeof(RectTransform));
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
        bImg.color = new Color(0.04f, 0.04f, 0.08f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Card
        GameObject card = new GameObject("StoreCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.11f, 0.13f, 0.20f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.32f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🛍️ МАРКЕТПЛЕЙС АССЕТОВ & ПЛАГИНОВ";
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
        GameObject statsObj = new GameObject("MarketplaceStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        marketplaceStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        marketplaceStatsSummaryTxt.fontSize = 13;
        marketplaceStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        marketplaceStatsSummaryTxt.color = new Color(0.92f, 0.92f, 1f);

        // Action Panel
        GameObject actPanel = new GameObject("ActPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.28f, 1f);

        GameObject sBtnObj = new GameObject("FlashBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        sBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform sbRect = sBtnObj.GetComponent<RectTransform>();
        sbRect.anchorMin = Vector2.zero;
        sbRect.anchorMax = Vector2.one;
        sbRect.offsetMin = new Vector2(8, 6);
        sbRect.offsetMax = new Vector2(-8, -6);
        sBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.45f, 0.15f, 1f);
        flashSaleBtn = sBtnObj.GetComponent<Button>();

        GameObject stTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stTxtObj.transform.SetParent(sBtnObj.transform, false);
        flashSaleBtnTxt = stTxtObj.GetComponent<TextMeshProUGUI>();
        flashSaleBtnTxt.text = "📢 ЗАПУСТИТЬ РАСПРОДАЖУ BLACK FRIDAY";
        flashSaleBtnTxt.fontSize = 14;
        flashSaleBtnTxt.fontStyle = FontStyles.Bold;
        flashSaleBtnTxt.alignment = TextAlignmentOptions.Center;
        flashSaleBtnTxt.color = Color.white;
        RectTransform str = stTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = Vector2.zero;
        str.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("PackagesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
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
        packagesContainer = content.transform;
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

        foreach (var pkg in packages)
        {
            CreatePackageCardUI(content.transform, pkg);
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

    private void CreatePackageCardUI(Transform parent, MarketplaceAssetPackage pkg)
    {
        GameObject card = new GameObject("PkgCard_" + pkg.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 92);
        card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.24f, 1f);

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

        string currentPkgId = pkg.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradePackage(currentPkgId));
    }
}
