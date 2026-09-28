using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Магазин официального мерча студии (Studio Merch Store):
/// - Производство партий брендированных товаров студии
/// - 4 вида мерча: кружки с котиками, фирменные худи, кастомные клавиатуры, артбуки
/// - Механика складских запасов (Inventory), производства партий и авто-продаж
/// - Пассивный доход от продаж мерча фанатам по всему миру
/// </summary>
public class StudioMerchStoreUI : MonoBehaviour
{
    private static StudioMerchStoreUI instance;
    public static StudioMerchStoreUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<StudioMerchStoreUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class MerchItem
    {
        public string id;
        public string title;
        public string icon;
        public string description;
        public double unlockCost;
        public double batchCost;
        public int batchUnits;
        public double unitPrice;
        public int currentStock;
        public int totalSold;
        public bool isUnlocked;
        public float salesSpeedPerSec; // Сколько единиц продаётся в сек.
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openStoreBtn;
    [SerializeField] private TMP_Text openStoreBtnText;
    [SerializeField] private Button closeStoreBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка магазина мерча")]
    [SerializeField] private TMP_Text totalMerchRevenueText;
    [SerializeField] private TMP_Text merchIncomeRateText;
    [SerializeField] private TMP_Text brandReputationText;

    [Header("Контейнер товаров")]
    [SerializeField] private Transform itemsContainer;

    private readonly List<MerchItem> items = new List<MerchItem>();
    private double totalMerchEarnings = 0;
    private int brandPrestigeLevel = 1;

    private const string PrefUnlockedPrefix = "Merch_Unlocked_";
    private const string PrefStockPrefix = "Merch_Stock_";
    private const string PrefSoldPrefix = "Merch_Sold_";
    private const string PrefEarningsKey = "Merch_TotalEarnings";
    private const string PrefBrandLevelKey = "Merch_BrandLevel";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<MerchItem> Items => items;

    public double GetMerchIncomePerSec()
    {
        double income = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].isUnlocked && items[i].currentStock > 0)
            {
                income += items[i].salesSpeedPerSec * items[i].unitPrice * (1.0 + (brandPrestigeLevel - 1) * 0.2);
            }
        }
        return income;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCatalog();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void Update()
    {
        // Симуляция продаж со склада
        double frameIncome = 0;
        float dt = Time.deltaTime;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!item.isUnlocked || item.currentStock <= 0) continue;

            float soldUnitsFloat = item.salesSpeedPerSec * dt;
            if (soldUnitsFloat > 0)
            {
                // Вероятностное или дискретное списание при накоплении
                if (UnityEngine.Random.value < item.salesSpeedPerSec * dt)
                {
                    item.currentStock--;
                    item.totalSold++;
                    double profit = item.unitPrice * (1.0 + (brandPrestigeLevel - 1) * 0.2);
                    frameIncome += profit;
                    totalMerchEarnings += profit;
                }
            }
        }

        if (frameIncome > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(frameIncome);
        }

        if (IsModalOpen)
        {
            UpdateModalUI();
        }
    }

    private void InitializeCatalog()
    {
        items.Clear();

        items.Add(new MerchItem
        {
            id = "merch_cat_mug",
            title = "Кружка 'Coffee & Bugfix'",
            icon = "☕",
            description = "Керамическая кружка с принтом офисного кота. Обязательный атрибут ночного коддинга.",
            unlockCost = 25000.0,
            batchCost = 8000.0,
            batchUnits = 50,
            unitPrice = 450.0,
            currentStock = 0,
            totalSold = 0,
            isUnlocked = false,
            salesSpeedPerSec = 0.8f
        });

        items.Add(new MerchItem
        {
            id = "merch_hoodie",
            title = "Худи 'It Works On My Machine'",
            icon = "🧥",
            description = "Уютное оверсайз-худи из премиального хлопка. Культовая фраза разработчиков на спине.",
            unlockCost = 120000.0,
            batchCost = 45000.0,
            batchUnits = 40,
            unitPrice = 2800.0,
            currentStock = 0,
            totalSold = 0,
            isUnlocked = false,
            salesSpeedPerSec = 0.5f
        });

        items.Add(new MerchItem
        {
            id = "merch_keyboard",
            title = "Клавиатура 'Clicky Legend 60%'",
            icon = "⌨️",
            description = "Кастомная механика со смазанными свичами и RGB-подсветкой. Звук нажатия ласкает слух.",
            unlockCost = 450000.0,
            batchCost = 180000.0,
            batchUnits = 25,
            unitPrice = 12500.0,
            currentStock = 0,
            totalSold = 0,
            isUnlocked = false,
            salesSpeedPerSec = 0.25f
        });

        items.Add(new MerchItem
        {
            id = "merch_artbook",
            title = "Артбук 'The Art of HelloTap'",
            icon = "📖",
            description = "Лимитированное коллекционное издание с концепт-артами всех проектов студии и автографами.",
            unlockCost = 1500000.0,
            batchCost = 600000.0,
            batchUnits = 30,
            unitPrice = 35000.0,
            currentStock = 0,
            totalSold = 0,
            isUnlocked = false,
            salesSpeedPerSec = 0.15f
        });
    }

    private void LoadData()
    {
        totalMerchEarnings = double.Parse(PlayerPrefs.GetString(PrefEarningsKey, "0"));
        brandPrestigeLevel = PlayerPrefs.GetInt(PrefBrandLevelKey, 1);

        for (int i = 0; i < items.Count; i++)
        {
            items[i].isUnlocked = PlayerPrefs.GetInt(PrefUnlockedPrefix + items[i].id, 0) == 1;
            items[i].currentStock = PlayerPrefs.GetInt(PrefStockPrefix + items[i].id, 0);
            items[i].totalSold = PlayerPrefs.GetInt(PrefSoldPrefix + items[i].id, 0);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(PrefEarningsKey, totalMerchEarnings.ToString("R"));
        PlayerPrefs.GetInt(PrefBrandLevelKey, brandPrestigeLevel);

        for (int i = 0; i < items.Count; i++)
        {
            PlayerPrefs.SetInt(PrefUnlockedPrefix + items[i].id, items[i].isUnlocked ? 1 : 0);
            PlayerPrefs.SetInt(PrefStockPrefix + items[i].id, items[i].currentStock);
            PlayerPrefs.SetInt(PrefSoldPrefix + items[i].id, items[i].totalSold);
        }
        PlayerPrefs.Save();
    }

    public void UnlockMerch(int index)
    {
        if (index < 0 || index >= items.Count) return;
        var item = items[index];
        if (item.isUnlocked) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < item.unlockCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно денег ({NumberFormatter.Format(item.unlockCost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(item.unlockCost);
        item.isUnlocked = true;
        item.currentStock = item.batchUnits; // Стартовая тестовая партия в подарок
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛍️ МЕРЧ ЗАПУЩЕН В ПРОИЗВОДСТВО!\n{item.icon} {item.title}\nПервая партия ({item.batchUnits} шт) уже на складе!", transform.position, new Color(0.3f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    public void ProduceBatch(int index)
    {
        if (index < 0 || index >= items.Count) return;
        var item = items[index];
        if (!item.isUnlocked) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < item.batchCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств на партию ({NumberFormatter.Format(item.batchCost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(item.batchCost);
        item.currentStock += item.batchUnits;
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📦 ПАРТИЯ ГОТОВА!\n+{item.batchUnits} шт на склад ({item.title})", transform.position, new Color(0.4f, 0.8f, 1f), false);
        }

        UpdateModalUI();
    }

    public void UpgradeBrandPrestige()
    {
        double cost = 250000.0 * Math.Pow(2.5, brandPrestigeLevel - 1);
        if (GameManager.Instance == null || GameManager.Instance.Money < cost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Требуется {NumberFormatter.Format(cost)} ₽ для ребрендинга!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(cost);
        brandPrestigeLevel++;
        PlayerPrefs.SetInt(PrefBrandLevelKey, brandPrestigeLevel);
        PlayerPrefs.Save();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPrestige();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🌟 ПРЕСТИЖ БРЕНДА УР. {brandPrestigeLevel}!\nНаценка на весь мерч: +{((brandPrestigeLevel - 1) * 20)}% к выручке!", transform.position, new Color(1f, 0.85f, 0.3f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openStoreBtn != null)
        {
            openStoreBtn.onClick.RemoveAllListeners();
            openStoreBtn.onClick.AddListener(OpenModal);
        }
        if (closeStoreBtn != null)
        {
            closeStoreBtn.onClick.RemoveAllListeners();
            closeStoreBtn.onClick.AddListener(CloseModal);
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
        UpdateModalUI();
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

    private void UpdateModalUI()
    {
        if (totalMerchRevenueText != null)
        {
            totalMerchRevenueText.text = $"Всего заработано на мерче: <b><color=#00FF88>+{NumberFormatter.Format(totalMerchEarnings)} ₽</color></b>";
        }

        if (merchIncomeRateText != null)
        {
            double rate = GetMerchIncomePerSec();
            merchIncomeRateText.text = $"Текущие продажи: <b><color=#44D0FF>+{NumberFormatter.Format(rate)} ₽/сек</color></b>";
        }

        if (brandReputationText != null)
        {
            brandReputationText.text = $"Уровень бренда: <b><color=#FFD700>Ур. {brandPrestigeLevel}</color></b> (+{(brandPrestigeLevel - 1) * 20}% маржи)";
        }

        RefreshItemsList();
    }

    private void RefreshItemsList()
    {
        if (itemsContainer == null) return;

        double currentMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;

        for (int i = 0; i < items.Count; i++)
        {
            int idx = i;
            var it = items[i];
            Transform cardTr = itemsContainer.Find($"MerchCard_{idx}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text actBtnTxt = actBtn != null ? actBtn.GetComponentInChildren<TMP_Text>() : null;
            TMP_Text stockTxt = cardTr.Find("StockText")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = it.isUnlocked
                    ? new Color(0.12f, 0.16f, 0.22f, 0.95f)
                    : new Color(0.08f, 0.09f, 0.12f, 0.90f);
            }

            if (stockTxt != null)
            {
                if (it.isUnlocked)
                {
                    string stockColor = it.currentStock > 10 ? "#00FF88" : (it.currentStock > 0 ? "#FFCC00" : "#FF5555");
                    stockTxt.text = $"Склад: <color={stockColor}><b>{it.currentStock} шт.</b></color> (Продано: {it.totalSold})";
                }
                else
                {
                    stockTxt.text = "<color=#8899AA>Недоступно для производства</color>";
                }
            }

            if (actBtn != null && actBtnTxt != null)
            {
                actBtn.onClick.RemoveAllListeners();

                if (!it.isUnlocked)
                {
                    bool canUnlock = currentMoney >= it.unlockCost;
                    actBtn.interactable = canUnlock;
                    actBtnTxt.text = $"ОТКРЫТЬ ({NumberFormatter.Format(it.unlockCost)} ₽)";
                    actBtn.GetComponent<Image>().color = canUnlock ? new Color(0.2f, 0.7f, 0.4f) : new Color(0.25f, 0.25f, 0.28f);
                    actBtn.onClick.AddListener(() => UnlockMerch(idx));
                }
                else
                {
                    bool canBatch = currentMoney >= it.batchCost;
                    actBtn.interactable = canBatch;
                    actBtnTxt.text = $"ПАРТИЯ +{it.batchUnits} шт ({NumberFormatter.Format(it.batchCost)} ₽)";
                    actBtn.GetComponent<Image>().color = canBatch ? new Color(0.25f, 0.55f, 0.9f) : new Color(0.25f, 0.25f, 0.28f);
                    actBtn.onClick.AddListener(() => ProduceBatch(idx));
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("StudioMerchStoreModal", typeof(RectTransform));
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
        bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.82f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(490, 680);
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.7f, 1f, 0.5f);
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
        headerTxt.text = "🛍️ ОФИЦИАЛЬНЫЙ МЕРЧ СТУДИИ";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.4f, 0.85f, 1f);

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

        // Info Summary Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -60);
        infoRt.sizeDelta = new Vector2(-36, 75);
        infoPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.9f);

        GameObject revObj = new GameObject("RevenueText", typeof(RectTransform), typeof(TextMeshProUGUI));
        revObj.transform.SetParent(infoPanel.transform, false);
        RectTransform revRt = revObj.GetComponent<RectTransform>();
        revRt.anchorMin = new Vector2(0, 0.66f);
        revRt.anchorMax = new Vector2(1, 1);
        revRt.offsetMin = new Vector2(12, 0);
        revRt.offsetMax = new Vector2(-12, -4);
        totalMerchRevenueText = revObj.GetComponent<TextMeshProUGUI>();
        totalMerchRevenueText.fontSize = 12;

        GameObject rateObj = new GameObject("RateText", typeof(RectTransform), typeof(TextMeshProUGUI));
        rateObj.transform.SetParent(infoPanel.transform, false);
        RectTransform rateRt = rateObj.GetComponent<RectTransform>();
        rateRt.anchorMin = new Vector2(0, 0.33f);
        rateRt.anchorMax = new Vector2(1, 0.66f);
        rateRt.offsetMin = new Vector2(12, 0);
        rateRt.offsetMax = new Vector2(-12, 0);
        merchIncomeRateText = rateObj.GetComponent<TextMeshProUGUI>();
        merchIncomeRateText.fontSize = 12;

        GameObject repObj = new GameObject("RepText", typeof(RectTransform), typeof(TextMeshProUGUI));
        repObj.transform.SetParent(infoPanel.transform, false);
        RectTransform repRt = repObj.GetComponent<RectTransform>();
        repRt.anchorMin = new Vector2(0, 0);
        repRt.anchorMax = new Vector2(1, 0.33f);
        repRt.offsetMin = new Vector2(12, 4);
        repRt.offsetMax = new Vector2(-12, 0);
        brandReputationText = repObj.GetComponent<TextMeshProUGUI>();
        brandReputationText.fontSize = 12;

        // Scroll View Container
        GameObject scrollObj = new GameObject("ItemsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 70);
        scrollRt.offsetMax = new Vector2(-18, -145);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        itemsContainer = contentObj.transform;
        RectTransform contRt = contentObj.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0, 1);
        contRt.anchorMax = new Vector2(1, 1);
        contRt.pivot = new Vector2(0.5f, 1);
        contRt.sizeDelta = new Vector2(0, 400);

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contRt;
        sr.viewport = vpRt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        for (int i = 0; i < items.Count; i++)
        {
            CreateItemCard(i, items[i], itemsContainer);
        }

        // Bottom Bar: Brand Upgrade Button & Close
        GameObject bottomBar = new GameObject("BottomBar", typeof(RectTransform));
        bottomBar.transform.SetParent(cardObj.transform, false);
        RectTransform bbarRt = bottomBar.GetComponent<RectTransform>();
        bbarRt.anchorMin = new Vector2(0, 0);
        bbarRt.anchorMax = new Vector2(1, 0);
        bbarRt.pivot = new Vector2(0.5f, 0);
        bbarRt.anchoredPosition = new Vector2(0, 14);
        bbarRt.sizeDelta = new Vector2(-36, 46);

        GameObject brandUpBtnObj = new GameObject("BrandUpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        brandUpBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform brandRt = brandUpBtnObj.GetComponent<RectTransform>();
        brandRt.anchorMin = new Vector2(0, 0);
        brandRt.anchorMax = new Vector2(0.68f, 1);
        brandRt.offsetMin = Vector2.zero;
        brandRt.offsetMax = new Vector2(-6, 0);
        brandUpBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f);
        Button brandBtn = brandUpBtnObj.GetComponent<Button>();
        brandBtn.onClick.AddListener(UpgradeBrandPrestige);

        GameObject brandTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        brandTxtObj.transform.SetParent(brandUpBtnObj.transform, false);
        RectTransform btRt = brandTxtObj.GetComponent<RectTransform>();
        btRt.anchorMin = Vector2.zero;
        btRt.anchorMax = Vector2.one;
        btRt.sizeDelta = Vector2.zero;
        TMP_Text bt = brandTxtObj.GetComponent<TextMeshProUGUI>();
        bt.text = "⭐ ПРОКАЧАТЬ ПРЕСТИЖ БРЕНДА";
        bt.fontSize = 11;
        bt.fontStyle = FontStyles.Bold;
        bt.alignment = TextAlignmentOptions.Center;
        bt.color = Color.black;

        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform clRt = closeBtnObj.GetComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0.70f, 0);
        clRt.anchorMax = new Vector2(1, 1);
        clRt.offsetMin = Vector2.zero;
        clRt.offsetMax = Vector2.zero;
        closeBtnObj.GetComponent<Image>().color = new Color(0.28f, 0.32f, 0.38f);
        closeStoreBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform cltRt = closeTxtObj.GetComponent<RectTransform>();
        cltRt.anchorMin = Vector2.zero;
        cltRt.anchorMax = Vector2.one;
        cltRt.sizeDelta = Vector2.zero;
        TMP_Text clt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        clt.text = "ЗАКРЫТЬ";
        clt.fontSize = 11;
        clt.fontStyle = FontStyles.Bold;
        clt.alignment = TextAlignmentOptions.Center;
        clt.color = Color.white;

        modalRoot = root;
    }

    private void CreateItemCard(int index, MerchItem item, Transform parent)
    {
        GameObject card = new GameObject($"MerchCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 92);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{item.icon} <b>{item.title}</b> <color=#90C0FF>(Розничная цена: {NumberFormatter.Format(item.unitPrice)} ₽)</color>";
        tt.fontSize = 12;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.6f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"{item.description}";
        dt.fontSize = 10;
        dt.color = new Color(0.8f, 0.85f, 0.95f);

        GameObject stk = new GameObject("StockText", typeof(RectTransform), typeof(TextMeshProUGUI));
        stk.transform.SetParent(card.transform, false);
        RectTransform stkrt = stk.GetComponent<RectTransform>();
        stkrt.anchorMin = new Vector2(0.6f, 1);
        stkrt.anchorMax = new Vector2(1, 1);
        stkrt.pivot = new Vector2(1, 1);
        stkrt.anchoredPosition = new Vector2(-10, -8);
        stkrt.sizeDelta = new Vector2(170, 20);
        TMP_Text stkt = stk.GetComponent<TextMeshProUGUI>();
        stkt.fontSize = 11;
        stkt.alignment = TextAlignmentOptions.Right;

        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0);
        brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(1, 0);
        brt.anchoredPosition = new Vector2(-10, 8);
        brt.sizeDelta = new Vector2(170, 32);
        b.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.9f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ПАРТИЯ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }
}
