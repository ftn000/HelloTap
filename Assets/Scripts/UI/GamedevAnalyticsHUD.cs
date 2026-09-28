using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Аналитический дашборд студии (Gamedev Analytics HUD):
/// 1. 📈 Продажи игр (Game Sales & Revenue): Выручка, копии, темп продаж по проектам
/// 2. 👥 Аудитория и удержание (DAU/MAU, Retention D1/D7/D30, отзывы в Steam 96%)
/// 3. 💹 Биржа акций инди-разработчиков ($TAP Stock Exchange): котировки, график, инвестиции
/// </summary>
public class GamedevAnalyticsHUD : MonoBehaviour
{
    private static GamedevAnalyticsHUD instance;
    public static GamedevAnalyticsHUD Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<GamedevAnalyticsHUD>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openDashboardBtn;
    [SerializeField] private TMP_Text openDashboardBtnText;
    [SerializeField] private Button closeDashboardBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Вкладки дашборда")]
    [SerializeField] private Button tabSalesBtn;
    [SerializeField] private Button tabAudienceBtn;
    [SerializeField] private Button tabStockBtn;
    [SerializeField] private GameObject salesPanelView;
    [SerializeField] private GameObject audiencePanelView;
    [SerializeField] private GameObject stockPanelView;

    [Header("Вкладка 1: Продажи")]
    [SerializeField] private TMP_Text totalRevenueText;
    [SerializeField] private TMP_Text totalCopiesText;
    [SerializeField] private TMP_Text bestSellerText;
    [SerializeField] private RectTransform[] salesBars;
    [SerializeField] private TMP_Text[] salesBarLabels;

    [Header("Вкладка 2: Аудитория")]
    [SerializeField] private TMP_Text dauText;
    [SerializeField] private TMP_Text mauText;
    [SerializeField] private TMP_Text retentionD1Text;
    [SerializeField] private TMP_Text retentionD7Text;
    [SerializeField] private TMP_Text steamRatingText;

    [Header("Вкладка 3: Биржа $TAP")]
    [SerializeField] private TMP_Text stockPriceText;
    [SerializeField] private TMP_Text stockTrendText;
    [SerializeField] private TMP_Text playerPortfolioText;
    [SerializeField] private Button buy10SharesBtn;
    [SerializeField] private Button buy100SharesBtn;
    [SerializeField] private Button sellAllSharesBtn;
    [SerializeField] private RectTransform[] stockChartPoints;

    private int activeTab = 0; // 0=Sales, 1=Audience, 2=Stock

    // Биржа акций $TAP
    private double currentSharePrice = 45.0; // ₽
    private int playerOwnedShares = 0;
    private double totalSpentOnShares = 0;
    private readonly float[] stockPriceHistory = new float[10];
    private float stockTickerTimer = 0f;

    private const string PrefShares = "Studio_Stock_Shares";
    private const string PrefSpent = "Studio_Stock_Spent";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public double CurrentSharePrice => currentSharePrice;
    public int PlayerOwnedShares => playerOwnedShares;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadStockData();
        InitializeStockHistory();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        SelectTab(0);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Колебания котировок акций $TAP каждые 8 секунд
        stockTickerTimer += dt;
        if (stockTickerTimer >= 8f)
        {
            stockTickerTimer = 0f;
            UpdateStockPrice();
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            RefreshActiveTabUI();
        }
    }

    private void LoadStockData()
    {
        playerOwnedShares = PlayerPrefs.GetInt(PrefShares, 0);
        if (double.TryParse(PlayerPrefs.GetString(PrefSpent, "0"), out double spent))
        {
            totalSpentOnShares = spent;
        }
    }

    private void SaveStockData()
    {
        PlayerPrefs.SetInt(PrefShares, playerOwnedShares);
        PlayerPrefs.SetString(PrefSpent, totalSpentOnShares.ToString("R"));
        PlayerPrefs.Save();
    }

    private void InitializeStockHistory()
    {
        // Базовая цена зависит от достигнутого престижа и релизов
        double baseVal = 25.0;
        if (GameManager.Instance != null)
        {
            baseVal += GameManager.Instance.PrestigeLevel * 30.0;
            baseVal += GameManager.Instance.TotalReleasesCount * 15.0;
        }
        currentSharePrice = Math.Max(15.0, baseVal);

        for (int i = 0; i < stockPriceHistory.Length; i++)
        {
            stockPriceHistory[i] = (float)currentSharePrice;
        }
    }

    private void UpdateStockPrice()
    {
        // Базис влияния: клики, релизы, престиж и волатильность рынка
        float marketSentiment = UnityEngine.Random.Range(-0.06f, 0.08f);

        // Если активен буст "В потоке", акции показывают ралли
        if (GameManager.Instance != null && GameManager.Instance.IsBoostActive)
        {
            marketSentiment += 0.05f;
        }

        currentSharePrice = Math.Max(10.0, currentSharePrice * (1.0 + marketSentiment));

        // Сдвигаем историю
        for (int i = 0; i < stockPriceHistory.Length - 1; i++)
        {
            stockPriceHistory[i] = stockPriceHistory[i + 1];
        }
        stockPriceHistory[stockPriceHistory.Length - 1] = (float)currentSharePrice;

        UpdateStockUI();
    }

    public void BuyShares(int count)
    {
        double totalCost = currentSharePrice * count;
        if (GameManager.Instance == null || GameManager.Instance.Money < totalCost)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно рублей для покупки {count} акций!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(totalCost);
        playerOwnedShares += count;
        totalSpentOnShares += totalCost;
        SaveStockData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📈 КУПЛЕНО {count} АКЦИЙ $TAP!\n-{NumberFormatter.Format(totalCost)} ₽", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateStockUI();
    }

    public void SellAllShares()
    {
        if (playerOwnedShares <= 0) return;

        double totalPayout = currentSharePrice * playerOwnedShares;
        double profit = totalPayout - totalSpentOnShares;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(totalPayout);
        }

        int soldCount = playerOwnedShares;
        playerOwnedShares = 0;
        totalSpentOnShares = 0;
        SaveStockData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        string profitStr = profit >= 0 ? $"+{NumberFormatter.Format(profit)} ₽ прибыли" : $"{NumberFormatter.Format(profit)} ₽ убытка";
        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 ПРОДАНО {soldCount} АКЦИЙ $TAP!\n+{NumberFormatter.Format(totalPayout)} ₽ ({profitStr})", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateStockUI();
    }

    public void SelectTab(int tabIndex)
    {
        activeTab = tabIndex;
        if (salesPanelView != null) salesPanelView.SetActive(activeTab == 0);
        if (audiencePanelView != null) audiencePanelView.SetActive(activeTab == 1);
        if (stockPanelView != null) stockPanelView.SetActive(activeTab == 2);

        // Подсветка табов
        UpdateTabButtonsColor();
        RefreshActiveTabUI();
    }

    private void UpdateTabButtonsColor()
    {
        Color activeColor = new Color(0f, 0.75f, 1f);
        Color normalColor = new Color(0.12f, 0.16f, 0.22f);

        if (tabSalesBtn != null)
        {
            var img = tabSalesBtn.GetComponent<Image>();
            if (img != null) img.color = activeTab == 0 ? activeColor : normalColor;
        }
        if (tabAudienceBtn != null)
        {
            var img = tabAudienceBtn.GetComponent<Image>();
            if (img != null) img.color = activeTab == 1 ? activeColor : normalColor;
        }
        if (tabStockBtn != null)
        {
            var img = tabStockBtn.GetComponent<Image>();
            if (img != null) img.color = activeTab == 2 ? activeColor : normalColor;
        }
    }

    private void RefreshActiveTabUI()
    {
        if (activeTab == 0) UpdateSalesUI();
        else if (activeTab == 1) UpdateAudienceUI();
        else if (activeTab == 2) UpdateStockUI();
    }

    private void UpdateSalesUI()
    {
        if (GameManager.Instance == null) return;

        double totalRevenue = GameManager.Instance.TotalMoneyEarned;
        int completedReleases = GameManager.Instance.TotalReleasesCount;
        double estimatedCopies = completedReleases * 12500 + totalRevenue / 15.0;

        if (totalRevenueText != null) totalRevenueText.text = $"💰 Общая выручка: <b>{NumberFormatter.Format(totalRevenue)} ₽</b>";
        if (totalCopiesText != null) totalCopiesText.text = $"🎮 Продано копий / инсталлов: <b>{estimatedCopies:N0}</b>";

        string best = "Нет релизов";
        if (GameManager.Instance.Projects != null)
        {
            for (int i = GameManager.Instance.Projects.Count - 1; i >= 0; i--)
            {
                var p = GameManager.Instance.Projects[i];
                if (p != null && p.IsCompleted)
                {
                    best = p.Title;
                    break;
                }
            }
        }
        if (bestSellerText != null) bestSellerText.text = $"🏆 Главный хит: <b>{best}</b>";

        // Обновляем столбцы графика продаж
        if (salesBars != null && salesBars.Length > 0)
        {
            for (int i = 0; i < salesBars.Length; i++)
            {
                if (salesBars[i] == null) continue;
                float heightFactor = Mathf.Clamp01(0.2f + (i * 0.15f) + (completedReleases * 0.1f));
                salesBars[i].sizeDelta = new Vector2(salesBars[i].sizeDelta.x, 140f * heightFactor);
            }
        }
    }

    private void UpdateAudienceUI()
    {
        if (GameManager.Instance == null) return;

        int releases = GameManager.Instance.TotalReleasesCount;
        int prestige = GameManager.Instance.PrestigeLevel;

        int dau = 120 + releases * 450 + prestige * 2200;
        int mau = dau * 8;

        if (dauText != null) dauText.text = $"👥 Активные игроки в день (DAU): <b>{dau:N0}</b>";
        if (mauText != null) mauText.text = $"🌐 Ежемесячная аудитория (MAU): <b>{mau:N0}</b>";
        if (retentionD1Text != null) retentionD1Text.text = $"📊 Удержание D1: <b>{42 + Math.Min(15, releases * 2)}%</b>";
        if (retentionD7Text != null) retentionD7Text.text = $"📈 Удержание D7: <b>{18 + Math.Min(12, releases)}%</b> | D30: <b>9.4%</b>";
        if (steamRatingText != null) steamRatingText.text = $"⭐ Рейтинг в Steam: <b>96%</b> (Крайне положительные)";
    }

    private void UpdateStockUI()
    {
        if (stockPriceText != null)
        {
            stockPriceText.text = $"🏷️ Акция $TAP: <b>{currentSharePrice:F2} ₽</b>";
        }

        if (stockTrendText != null && stockPriceHistory.Length > 1)
        {
            float prev = stockPriceHistory[stockPriceHistory.Length - 2];
            float cur = stockPriceHistory[stockPriceHistory.Length - 1];
            float diffPercent = prev > 0 ? ((cur - prev) / prev) * 100f : 0f;
            string arrow = diffPercent >= 0 ? "▲" : "▼";
            Color trendCol = diffPercent >= 0 ? new Color(0.2f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);
            stockTrendText.text = $"Тренд: <color=#{ColorUtility.ToHtmlStringRGB(trendCol)}>{arrow} {diffPercent:+0.0;-0.0}%</color>";
        }

        if (playerPortfolioText != null)
        {
            double curVal = playerOwnedShares * currentSharePrice;
            double pnl = curVal - totalSpentOnShares;
            string pnlStr = pnl >= 0 ? $"+{NumberFormatter.Format(pnl)} ₽" : $"{NumberFormatter.Format(pnl)} ₽";
            playerPortfolioText.text = $"💼 Портфель: <b>{playerOwnedShares} шт.</b> (~{NumberFormatter.Format(curVal)} ₽) | PnL: <b>{pnlStr}</b>";
        }

        if (buy10SharesBtn != null)
        {
            double cost10 = currentSharePrice * 10;
            buy10SharesBtn.interactable = GameManager.Instance != null && GameManager.Instance.Money >= cost10;
            var t = buy10SharesBtn.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = $"+10 ({NumberFormatter.Format(cost10)} ₽)";
        }

        if (buy100SharesBtn != null)
        {
            double cost100 = currentSharePrice * 100;
            buy100SharesBtn.interactable = GameManager.Instance != null && GameManager.Instance.Money >= cost100;
            var t = buy100SharesBtn.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = $"+100 ({NumberFormatter.Format(cost100)} ₽)";
        }

        if (sellAllSharesBtn != null)
        {
            sellAllSharesBtn.interactable = playerOwnedShares > 0;
            var t = sellAllSharesBtn.GetComponentInChildren<TMP_Text>();
            if (t != null) t.text = $"ПРОДАТЬ ВСЁ (~{NumberFormatter.Format(playerOwnedShares * currentSharePrice)} ₽)";
        }

        // Обновляем визуальные точки графика
        if (stockChartPoints != null && stockChartPoints.Length > 0)
        {
            float minPrice = float.MaxValue;
            float maxPrice = float.MinValue;
            for (int i = 0; i < stockPriceHistory.Length; i++)
            {
                if (stockPriceHistory[i] < minPrice) minPrice = stockPriceHistory[i];
                if (stockPriceHistory[i] > maxPrice) maxPrice = stockPriceHistory[i];
            }
            float range = Mathf.Max(5f, maxPrice - minPrice);

            for (int i = 0; i < stockChartPoints.Length && i < stockPriceHistory.Length; i++)
            {
                if (stockChartPoints[i] == null) continue;
                float norm = (stockPriceHistory[i] - minPrice) / range;
                stockChartPoints[i].anchoredPosition = new Vector2(stockChartPoints[i].anchoredPosition.x, 20f + norm * 80f);
            }
        }
    }

    private void BindButtons()
    {
        if (openDashboardBtn != null)
        {
            openDashboardBtn.onClick.RemoveAllListeners();
            openDashboardBtn.onClick.AddListener(OpenModal);
        }
        if (closeDashboardBtn != null)
        {
            closeDashboardBtn.onClick.RemoveAllListeners();
            closeDashboardBtn.onClick.AddListener(CloseModal);
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

        if (tabSalesBtn != null)
        {
            tabSalesBtn.onClick.RemoveAllListeners();
            tabSalesBtn.onClick.AddListener(() => SelectTab(0));
        }
        if (tabAudienceBtn != null)
        {
            tabAudienceBtn.onClick.RemoveAllListeners();
            tabAudienceBtn.onClick.AddListener(() => SelectTab(1));
        }
        if (tabStockBtn != null)
        {
            tabStockBtn.onClick.RemoveAllListeners();
            tabStockBtn.onClick.AddListener(() => SelectTab(2));
        }

        if (buy10SharesBtn != null)
        {
            buy10SharesBtn.onClick.RemoveAllListeners();
            buy10SharesBtn.onClick.AddListener(() => BuyShares(10));
        }
        if (buy100SharesBtn != null)
        {
            buy100SharesBtn.onClick.RemoveAllListeners();
            buy100SharesBtn.onClick.AddListener(() => BuyShares(100));
        }
        if (sellAllSharesBtn != null)
        {
            sellAllSharesBtn.onClick.RemoveAllListeners();
            sellAllSharesBtn.onClick.AddListener(SellAllShares);
        }
    }

    public void OpenModal()
    {
        EnsureUIExists();
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.88f, 0.88f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }
        }
        SelectTab(activeTab);
        HapticFeedback.Vibrate(20);
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.Vibrate(15);
    }

    private IEnumerator PopCardAnim(Transform card)
    {
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.18f);
            float scale = Mathf.Lerp(0.88f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("GamedevAnalyticsHUDModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        // Backdrop
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.80f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(490, 620);
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.8f, 1f, 0.45f);
        outline.effectDistance = new Vector2(2, -2);

        // Header
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -18);
        headerRt.sizeDelta = new Vector2(-40, 36);
        TMP_Text headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "📊 АНАЛИТИКА СТУДИИ & $TAP";
        headerTxt.fontSize = 20;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 0.9f, 1f);

        // Close X
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeXRt = closeXObj.GetComponent<RectTransform>();
        closeXRt.anchorMin = new Vector2(1, 1);
        closeXRt.anchorMax = new Vector2(1, 1);
        closeXRt.anchoredPosition = new Vector2(-25, -25);
        closeXRt.sizeDelta = new Vector2(34, 34);
        closeXObj.GetComponent<Image>().color = new Color(0.25f, 0.1f, 0.12f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject closeXTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeXTxtObj.transform.SetParent(closeXObj.transform, false);
        TMP_Text cTxt = closeXTxtObj.GetComponent<TextMeshProUGUI>();
        cTxt.text = "✕";
        cTxt.fontSize = 16;
        cTxt.alignment = TextAlignmentOptions.Center;
        cTxt.color = Color.white;

        // Tabs Header
        GameObject tabsRow = new GameObject("TabsRow", typeof(RectTransform));
        tabsRow.transform.SetParent(cardObj.transform, false);
        RectTransform tabsRt = tabsRow.GetComponent<RectTransform>();
        tabsRt.anchorMin = new Vector2(0, 1);
        tabsRt.anchorMax = new Vector2(1, 1);
        tabsRt.pivot = new Vector2(0.5f, 1);
        tabsRt.anchoredPosition = new Vector2(0, -60);
        tabsRt.sizeDelta = new Vector2(-30, 38);

        tabSalesBtn = CreateTabButton(tabsRow.transform, "📈 Продажи", 0);
        tabAudienceBtn = CreateTabButton(tabsRow.transform, "👥 Аудитория", 1);
        tabStockBtn = CreateTabButton(tabsRow.transform, "💹 Биржа $TAP", 2);

        // View 1: Sales
        salesPanelView = CreateSubPanel(cardObj.transform, "SalesView");
        totalRevenueText = CreatePanelText(salesPanelView.transform, "💰 Выручка: 0 ₽", -16, 14);
        totalCopiesText = CreatePanelText(salesPanelView.transform, "🎮 Продано копий: 0", -46, 13);
        bestSellerText = CreatePanelText(salesPanelView.transform, "🏆 Главный хит: --", -76, 13);

        // View 2: Audience
        audiencePanelView = CreateSubPanel(cardObj.transform, "AudienceView");
        dauText = CreatePanelText(audiencePanelView.transform, "👥 DAU: 0", -16, 14);
        mauText = CreatePanelText(audiencePanelView.transform, "🌐 MAU: 0", -46, 14);
        retentionD1Text = CreatePanelText(audiencePanelView.transform, "📊 Удержание D1: 42%", -76, 13);
        retentionD7Text = CreatePanelText(audiencePanelView.transform, "📈 Удержание D7: 18%", -106, 13);
        steamRatingText = CreatePanelText(audiencePanelView.transform, "⭐ Steam: 96% (Крайне положит.)", -136, 13);

        // View 3: Stock $TAP
        stockPanelView = CreateSubPanel(cardObj.transform, "StockView");
        stockPriceText = CreatePanelText(stockPanelView.transform, "🏷️ Акция $TAP: 45.00 ₽", -16, 16, FontStyles.Bold);
        stockTrendText = CreatePanelText(stockPanelView.transform, "Тренд: ▲ +0.0%", -46, 13);
        playerPortfolioText = CreatePanelText(stockPanelView.transform, "💼 Портфель: 0 шт.", -76, 13);

        // Stock Action Buttons
        buy10SharesBtn = CreateActionButton(stockPanelView.transform, "+10", new Vector2(-110, -135), new Vector2(100, 36), new Color(0.12f, 0.55f, 0.45f));
        buy100SharesBtn = CreateActionButton(stockPanelView.transform, "+100", new Vector2(0, -135), new Vector2(100, 36), new Color(0.15f, 0.65f, 0.35f));
        sellAllSharesBtn = CreateActionButton(stockPanelView.transform, "ПРОДАТЬ ВСЁ", new Vector2(110, -135), new Vector2(110, 36), new Color(0.7f, 0.25f, 0.25f));

        // Bottom Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 16);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeDashboardBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero;
        closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "ЗАКРЫТЬ ДАШБОРД";
        closeTxt.fontSize = 14;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private Button CreateTabButton(Transform parent, string title, int index)
    {
        GameObject bObj = new GameObject($"Tab_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
        bObj.transform.SetParent(parent, false);
        RectTransform brt = bObj.GetComponent<RectTransform>();
        float step = 1f / 3f;
        brt.anchorMin = new Vector2(index * step, 0);
        brt.anchorMax = new Vector2((index + 1) * step, 1);
        brt.offsetMin = new Vector2(2, 0);
        brt.offsetMax = new Vector2(-2, 0);
        bObj.GetComponent<Image>().color = index == 0 ? new Color(0f, 0.75f, 1f) : new Color(0.12f, 0.16f, 0.22f);

        GameObject tObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        tObj.transform.SetParent(bObj.transform, false);
        RectTransform trt = tObj.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TMP_Text txt = tObj.GetComponent<TextMeshProUGUI>();
        txt.text = title;
        txt.fontSize = 12;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        return bObj.GetComponent<Button>();
    }

    private GameObject CreateSubPanel(Transform parent, string name)
    {
        GameObject p = new GameObject(name, typeof(RectTransform), typeof(Image));
        p.transform.SetParent(parent, false);
        RectTransform prt = p.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 0);
        prt.anchorMax = new Vector2(1, 1);
        prt.offsetMin = new Vector2(20, 75);
        prt.offsetMax = new Vector2(-20, -110);
        p.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.85f);
        return p;
    }

    private TMP_Text CreatePanelText(Transform parent, string initial, float posY, float fontSize, FontStyles style = FontStyles.Normal)
    {
        GameObject o = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        o.transform.SetParent(parent, false);
        RectTransform rt = o.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(12, posY);
        rt.sizeDelta = new Vector2(-24, 26);
        TMP_Text t = o.GetComponent<TextMeshProUGUI>();
        t.text = initial;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.color = new Color(0.9f, 0.95f, 1f);
        return t;
    }

    private Button CreateActionButton(Transform parent, string label, Vector2 pos, Vector2 size, Color col)
    {
        GameObject b = new GameObject("ActBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TMP_Text t = to.GetComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 11;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        return b.GetComponent<Button>();
    }
}
