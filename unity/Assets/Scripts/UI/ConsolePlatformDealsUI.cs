using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Игровые Консоли и Платформенные Эксклюзивы (Console Platform Deals):
/// - 4 платформенных вендора: PlayBox 5, Nintap Switchy, Microstation Series X, SteamDeck Handheld
/// - Лицензирование DevKit и аудит качества проектов студии
/// - Авансовые выплаты (Upfront Cash Grants) при подписании контрактов
/// - Роялти и перманентные глобальные бонусы к доходам от релизов и пассивному потоку
/// </summary>
public class ConsolePlatformDealsUI : MonoBehaviour
{
    private static ConsolePlatformDealsUI instance;
    public static ConsolePlatformDealsUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ConsolePlatformDealsUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(ConsolePlatformDealsUI));
                    instance = go.AddComponent<ConsolePlatformDealsUI>();
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
    public class PlatformDeal
    {
        public string id;
        public string platformName;
        public string icon;
        public string vendorTitle;
        public string description;
        public string requirementDesc;
        public double devKitCostMoney;
        public double devKitCostCode;
        public double upfrontGrantMoney;
        public string perkDescription;
        public double incomeBonusMultiplier;
        public bool isSigned;
        public Color themeColor;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openDealsBtn;

    [Header("Информационный заголовок")]
    [SerializeField] private TMP_Text dealsSummaryHeaderTxt;
    [SerializeField] private Transform dealsContainer;

    private readonly List<PlatformDeal> deals = new List<PlatformDeal>();

    private const string PrefDealPrefix = "PlatformDeal_Signed_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<PlatformDeal> Deals => deals;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeDeals();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeDeals()
    {
        deals.Clear();

        deals.Add(new PlatformDeal
        {
            id = "playbox",
            platformName = "PlayBox 5",
            icon = "🎮",
            vendorTitle = "PlayBox Interactive Corp",
            description = "Флагманская домашняя консоль с 65 млн активных геймеров и любовью к блокбастерам.",
            requirementDesc = "TapEngine v2.0+ и минимум 3 завершенных проекта",
            devKitCostMoney = 40000,
            devKitCostCode = 8000,
            upfrontGrantMoney = 150000,
            perkDescription = "+25% ко всем доходам от релизов проектов",
            incomeBonusMultiplier = 0.25,
            isSigned = false,
            themeColor = new Color(0.2f, 0.5f, 1f)
        });

        deals.Add(new PlatformDeal
        {
            id = "nintap",
            platformName = "Nintap Switchy",
            icon = "🔴",
            vendorTitle = "Nintap Entertainment",
            description = "Портативная консоль-хит. Идеальная аудитория для уютных инди-шедевров.",
            requirementDesc = "Написать более 20 000 строк кода",
            devKitCostMoney = 25000,
            devKitCostCode = 5000,
            upfrontGrantMoney = 90000,
            perkDescription = "+30% к оффлайн-доходу и скорости релизов",
            incomeBonusMultiplier = 0.20,
            isSigned = false,
            themeColor = new Color(1f, 0.3f, 0.3f)
        });

        deals.Add(new PlatformDeal
        {
            id = "microstation",
            platformName = "Microstation Series X",
            icon = "🟢",
            vendorTitle = "Microstation Cloud Gaming",
            description = "Подписочный сервис DevPass. Гарантированный поток доходов сотен тысяч игроков.",
            requirementDesc = "Офис уровня Коворкинг или выше",
            devKitCostMoney = 60000,
            devKitCostCode = 12000,
            upfrontGrantMoney = 220000,
            perkDescription = "+20% к пассивному доходу студии в секунду",
            incomeBonusMultiplier = 0.20,
            isSigned = false,
            themeColor = new Color(0.2f, 0.85f, 0.3f)
        });

        deals.Add(new PlatformDeal
        {
            id = "steamdeck",
            platformName = "SteamDeck Handheld",
            icon = "💻",
            vendorTitle = "Valve Open Hardware",
            description = "Открытая экосистема ПК-гейминга в кармане с поддержкой модов сообщества.",
            requirementDesc = "Написать более 10 000 строк кода",
            devKitCostMoney = 15000,
            devKitCostCode = 3000,
            upfrontGrantMoney = 60000,
            perkDescription = "+20% к силе клика и времени комбо",
            incomeBonusMultiplier = 0.15,
            isSigned = false,
            themeColor = new Color(0.9f, 0.45f, 1f)
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < deals.Count; i++)
        {
            deals[i].isSigned = PlayerPrefs.GetInt(PrefDealPrefix + deals[i].id, 0) == 1;
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < deals.Count; i++)
        {
            PlayerPrefs.SetInt(PrefDealPrefix + deals[i].id, deals[i].isSigned ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public double GetPlatformDealsMultiplier()
    {
        double mult = 1.0;
        for (int i = 0; i < deals.Count; i++)
        {
            if (deals[i].isSigned)
            {
                mult += deals[i].incomeBonusMultiplier;
            }
        }
        return mult;
    }

    public int GetSignedDealsCount()
    {
        int c = 0;
        for (int i = 0; i < deals.Count; i++)
        {
            if (deals[i].isSigned) c++;
        }
        return c;
    }

    public bool CheckEligibility(PlatformDeal deal)
    {
        if (deal.isSigned) return true;
        if (GameManager.Instance == null) return false;

        switch (deal.id)
        {
            case "playbox":
                int engVer = CustomGameEngineUI.Instance != null ? CustomGameEngineUI.Instance.CurrentMajorVersion : 1;
                int complPrj = 0;
                foreach (var p in GameManager.Instance.Projects)
                {
                    if (p.IsCompleted) complPrj++;
                }
                return engVer >= 2 && complPrj >= 3;

            case "nintap":
                return GameManager.Instance.TotalCodeWritten >= 20000;

            case "microstation":
                int office = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.CurrentTierIndex : 0;
                return office >= 1;

            case "steamdeck":
                return GameManager.Instance.TotalCodeWritten >= 10000;

            default:
                return false;
        }
    }

    public bool TrySignContract(string dealId)
    {
        var deal = deals.Find(d => d.id == dealId);
        if (deal == null || deal.isSigned) return false;

        if (!CheckEligibility(deal))
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"⚠️ Условия вендора не выполнены!\n{deal.requirementDesc}", transform.position, Color.yellow, false);
            }
            return false;
        }

        if (GameManager.Instance == null || GameManager.Instance.Money < deal.devKitCostMoney || GameManager.Instance.CodeLines < deal.devKitCostCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(deal.devKitCostMoney)} ₽ и {NumberFormatter.Format(deal.devKitCostCode)} кода за DevKit!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendMoney(deal.devKitCostMoney);
        GameManager.Instance.SpendLinesOfCode(deal.devKitCostCode);

        // Начисляем авансовый грант
        GameManager.Instance.AddMoney(deal.upfrontGrantMoney);

        deal.isSigned = true;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📜 КОНТРАКТ ПОДПИСАН!\n{deal.icon} {deal.platformName}\nВыплачен аванс: <b>+{NumberFormatter.Format(deal.upfrontGrantMoney)} ₽</b>\n<color=#00FF88>{deal.perkDescription}</color>", transform.position, deal.themeColor, true);
        }

        UpdateModalUI();
        return true;
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

    private void BindButtons()
    {
        if (openDealsBtn != null)
        {
            openDealsBtn.onClick.RemoveAllListeners();
            openDealsBtn.onClick.AddListener(OpenModal);
        }
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
    }

    private void UpdateModalUI()
    {
        int signed = GetSignedDealsCount();
        if (dealsSummaryHeaderTxt != null)
        {
            double bonusPct = (GetPlatformDealsMultiplier() - 1.0) * 100.0;
            dealsSummaryHeaderTxt.text = $"Партнерских контрактов: <b><color=#00E5FF>{signed}/{deals.Count}</color></b> | Суммарный бонус: <b><color=#00FF88>+{bonusPct:F0}%</color></b> к доходу студии";
        }

        RefreshDealCards();
    }

    private void RefreshDealCards()
    {
        if (dealsContainer == null) return;

        for (int i = 0; i < deals.Count; i++)
        {
            var deal = deals[i];
            Transform cardTr = dealsContainer.Find($"DealCard_{deal.id}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actionBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = actionBtn != null ? actionBtn.GetComponentInChildren<TMP_Text>() : null;

            bool isEligible = CheckEligibility(deal);

            if (bg != null)
            {
                bg.color = deal.isSigned 
                    ? new Color(0.12f, 0.22f, 0.16f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.92f);
            }

            if (actionBtn != null && btnTxt != null)
            {
                string dId = deal.id;
                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() => TrySignContract(dId));

                if (deal.isSigned)
                {
                    actionBtn.interactable = false;
                    btnTxt.text = "⭐ ПАРТНЕР ПЛАТФОРМЫ";
                }
                else if (!isEligible)
                {
                    actionBtn.interactable = false;
                    btnTxt.text = "ТРЕБОВАНИЯ НЕ ВЫПОЛНЕНЫ";
                }
                else
                {
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= deal.devKitCostMoney &&
                                     GameManager.Instance.CodeLines >= deal.devKitCostCode;
                    actionBtn.interactable = canAfford;
                    btnTxt.text = $"ПОДПИСАТЬ ({NumberFormatter.Format(deal.devKitCostMoney)} ₽)\nАванс: +{NumberFormatter.Format(deal.upfrontGrantMoney)} ₽";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        EnsureFloatingButton(canvas);

        // Корневой объект модального окна
        GameObject root = new GameObject("ConsoleDealsModal", typeof(RectTransform));
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
        bgObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(500, 680);
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -14);
        headerRt.sizeDelta = new Vector2(-40, 32);
        var headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "🎮 КОНСОЛИ И ЭКСКЛЮЗИВНЫЕ СДЕЛКИ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.3f, 0.75f, 1f);

        // Close X Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeXRt = closeXObj.GetComponent<RectTransform>();
        closeXRt.anchorMin = new Vector2(1, 1);
        closeXRt.anchorMax = new Vector2(1, 1);
        closeXRt.pivot = new Vector2(1, 1);
        closeXRt.anchoredPosition = new Vector2(-12, -12);
        closeXRt.sizeDelta = new Vector2(32, 32);
        closeXObj.GetComponent<Image>().color = new Color(0.2f, 0.25f, 0.35f, 0.9f);
        closeXBtn = closeXObj.GetComponent<Button>();
        GameObject xTxtObj = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(closeXObj.transform, false);
        RectTransform xTxtRt = xTxtObj.GetComponent<RectTransform>();
        xTxtRt.anchorMin = Vector2.zero; xTxtRt.anchorMax = Vector2.one; xTxtRt.sizeDelta = Vector2.zero;
        var xt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xt.text = "✕"; xt.fontSize = 16; xt.fontStyle = FontStyles.Bold; xt.alignment = TextAlignmentOptions.Center; xt.color = Color.white;

        // Deals Summary Box
        GameObject sumObj = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumObj.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 42);
        sumObj.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.9f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumObj.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        dealsSummaryHeaderTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        dealsSummaryHeaderTxt.fontSize = 11;
        dealsSummaryHeaderTxt.alignment = TextAlignmentOptions.Center;
        dealsSummaryHeaderTxt.color = new Color(0.9f, 0.95f, 1f);

        // Scroll Container for Deals
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -104);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;
        dealsContainer = contentObj.transform;

        for (int i = 0; i < deals.Count; i++)
        {
            CreateDealCardTemplate(dealsContainer, deals[i]);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 12);
        closeBtnRt.sizeDelta = new Vector2(-40, 38);
        closeBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.3f, 0.95f);
        closeBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one; closeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "ЗАКРЫТЬ";
        closeTxt.fontSize = 13;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
        BindButtons();
    }

    private void CreateDealCardTemplate(Transform parent, PlatformDeal deal)
    {
        GameObject card = new GameObject($"DealCard_{deal.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 84);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        // Title and icon
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(0.65f, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(0, 20);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{deal.icon} <b>{deal.platformName}</b> <color=#90B0D0>({deal.vendorTitle})</color>";
        tt.fontSize = 12;
        tt.color = deal.themeColor;

        // Requirement & perk text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.62f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Условие: <color=#FFD700>{deal.requirementDesc}</color>\nБонус: <color=#00FF88>{deal.perkDescription}</color>";
        dt.fontSize = 10;

        // Action button
        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, 0);
        brt.sizeDelta = new Vector2(150, 42);
        b.GetComponent<Image>().color = new Color(0.15f, 0.45f, 0.65f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ПОДПИСАТЬ";
        btxt.fontSize = 9;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openDealsBtn != null) return;

        Transform hubBar = canvas.transform.Find("StudioHubBar");
        if (hubBar == null)
        {
            GameObject hubObj = new GameObject("StudioHubBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hubObj.transform.SetParent(canvas.transform, false);
            RectTransform hubRt = hubObj.GetComponent<RectTransform>();
            hubRt.anchorMin = new Vector2(0.5f, 1f);
            hubRt.anchorMax = new Vector2(0.5f, 1f);
            hubRt.pivot = new Vector2(0.5f, 1f);
            hubRt.anchoredPosition = new Vector2(0, -96);
            hubRt.sizeDelta = new Vector2(460, 32);

            var hlg = hubObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hubBar = hubObj.transform;
        }

        GameObject btnGo = new GameObject("DealsHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.12f, 0.35f, 0.55f, 0.9f);
        openDealsBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🎮 Консоли";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
