using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Коллекционные карточки Steam и крафт значков (Steam Trading Cards & Badges):
/// - 5 уникальных коллекционных карточек геймдева (Rubber Duck, Blue Switch, Bug #404, Midnight Flow, Quantum Core)
/// - Выпадение карточек за релизы игр, достижения и престижи
/// - Механика крафта значков (Badge Level 1–5):
///   - Расходует полный сет из 5 карточек
///   - Даёт постоянный бонус к скорости и доходу (+5% за каждый уровень)
///   - Дарит памятный смайлик и бонусный опыт
/// - Интерактивная витрина карточек с прогрессом сета
/// </summary>
public class SteamTradingCardsUI : MonoBehaviour
{
    private static SteamTradingCardsUI instance;
    public static SteamTradingCardsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SteamTradingCardsUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class TradingCard
    {
        public string id;
        public string title;
        public string icon;
        public string description;
        public Color accentColor;
        public int countOwned;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openCardsBtn;
    [SerializeField] private TMP_Text openCardsBtnText;
    [SerializeField] private Button closeCardsBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Витрина значка")]
    [SerializeField] private TMP_Text badgeTitleText;
    [SerializeField] private TMP_Text badgeLevelText;
    [SerializeField] private TMP_Text badgeBonusText;
    [SerializeField] private Button craftBadgeBtn;
    [SerializeField] private TMP_Text craftBadgeBtnText;

    [Header("Контейнер карточек")]
    [SerializeField] private Transform cardsGridContainer;

    private readonly List<TradingCard> cards = new List<TradingCard>();
    private int badgeLevel = 0;

    private const string PrefCardPrefix = "Steam_Card_";
    private const string PrefBadgeLevel = "Steam_BadgeLevel";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int BadgeLevel => badgeLevel;

    public double GetBadgeMultiplier()
    {
        return 1.0 + (badgeLevel * 0.05); // +5% за каждый уровень значка
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCards();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
            GameManager.Instance.OnPrestigeCompleted += HandlePrestigeCompleted;
            GameManager.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
            GameManager.Instance.OnPrestigeCompleted -= HandlePrestigeCompleted;
            GameManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
        }
    }

    private void HandleProjectCompleted(GameProjectData proj)
    {
        DropRandomCard("Успешный релиз игры");
    }

    private void HandlePrestigeCompleted(int prestigeLvl)
    {
        DropRandomCard("Выход студии на IPO");
        DropRandomCard("Юбилейный раунд IPO");
    }

    private void HandleAchievementUnlocked(AchievementData ach)
    {
        if (UnityEngine.Random.value < 0.6f)
        {
            DropRandomCard($"Достижение '{ach.Title}'");
        }
    }

    private void InitializeCards()
    {
        cards.Clear();

        cards.Add(new TradingCard
        {
            id = "card_duck",
            title = "Rubber Duck Debugger",
            icon = "🦆",
            description = "Главный собеседник и ментор инди-разработчика при поиске утечек памяти.",
            accentColor = new Color(1f, 0.85f, 0.2f),
            countOwned = 0
        });

        cards.Add(new TradingCard
        {
            id = "card_switch",
            title = "Blue Click Switch",
            icon = "⌨️",
            description = "Легендарный тактильный клик, сводящий с ума соседей и ускоряющий набор кода.",
            accentColor = new Color(0.2f, 0.75f, 1f),
            countOwned = 0
        });

        cards.Add(new TradingCard
        {
            id = "card_glitch",
            title = "Fatal Bug #404",
            icon = "👾",
            description = "Таинственный артефакт, живущий в недрах репозитория до первого релиза.",
            accentColor = new Color(1f, 0.35f, 0.5f),
            countOwned = 0
        });

        cards.Add(new TradingCard
        {
            id = "card_midnight",
            title = "Midnight Flow",
            icon = "☕",
            description = "3 часа ночи, кружка крепкого кофе, пустой мессенджер и чистый дзен разработки.",
            accentColor = new Color(0.7f, 0.45f, 1f),
            countOwned = 0
        });

        cards.Add(new TradingCard
        {
            id = "card_quantum",
            title = "Quantum Core Node",
            icon = "🌌",
            description = "Сверхмощный кристаллический кластер для нейро-майнинга и компиляции шейдеров.",
            accentColor = new Color(0.2f, 1f, 0.75f),
            countOwned = 0
        });
    }

    private void LoadData()
    {
        badgeLevel = PlayerPrefs.GetInt(PrefBadgeLevel, 0);

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].countOwned = PlayerPrefs.GetInt(PrefCardPrefix + cards[i].id, 0);
        }

        // Даем 2 стартовые карточки новым игрокам
        if (PlayerPrefs.GetInt("Steam_HasInitialCards", 0) == 0)
        {
            PlayerPrefs.SetInt("Steam_HasInitialCards", 1);
            cards[0].countOwned += 1;
            cards[1].countOwned += 1;
            SaveData();
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefBadgeLevel, badgeLevel);

        for (int i = 0; i < cards.Count; i++)
        {
            PlayerPrefs.SetInt(PrefCardPrefix + cards[i].id, cards[i].countOwned);
        }
        PlayerPrefs.Save();
    }

    public void DropRandomCard(string reason)
    {
        if (cards.Count == 0) return;
        int idx = UnityEngine.Random.Range(0, cards.Count);
        var card = cards[idx];
        card.countOwned++;
        SaveData();

        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🃏 ВЫПАЛА КАРТОЧКА STEAM!\n{card.icon} {card.title}\n({reason})", transform.position, card.accentColor, true);
        }

        if (IsModalOpen)
        {
            UpdateModalUI();
        }
    }

    public bool CanCraftBadge()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].countOwned <= 0) return false;
        }
        return true;
    }

    public void CraftBadge()
    {
        if (!CanCraftBadge())
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("❌ Не хватает карточек для полного набора (5/5)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        // Списываем по 1 карточке каждого вида
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].countOwned--;
        }

        badgeLevel++;
        SaveData();

        // Награда за крафт: +50 000 ₽ + 10 000 строк кода
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(50000.0);
            GameManager.Instance.AddLinesOfCode(10000.0);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎖️ ЗНАЧОК СКРАФЧЕН! Ур. {badgeLevel}\nПостоянный бонус: +{badgeLevel * 5}% ко всем доходам!\n+50 000 ₽, +10 000 строк", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openCardsBtn != null)
        {
            openCardsBtn.onClick.RemoveAllListeners();
            openCardsBtn.onClick.AddListener(OpenModal);
        }
        if (closeCardsBtn != null)
        {
            closeCardsBtn.onClick.RemoveAllListeners();
            closeCardsBtn.onClick.AddListener(CloseModal);
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

        if (craftBadgeBtn != null)
        {
            craftBadgeBtn.onClick.RemoveAllListeners();
            craftBadgeBtn.onClick.AddListener(CraftBadge);
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
        if (badgeTitleText != null)
        {
            badgeTitleText.text = badgeLevel > 0 
                ? $"🎖️ Значок 'Легенда Инди-Геймдева' (Ур. {badgeLevel})" 
                : "🎖️ Значок ещё не скрафчен";
        }

        if (badgeBonusText != null)
        {
            badgeBonusText.text = $"Текущий постоянный бонус: <b><color=#00FF88>+{badgeLevel * 5}% к доходу</color></b>";
        }

        bool canCraft = CanCraftBadge();
        if (craftBadgeBtn != null)
        {
            craftBadgeBtn.interactable = canCraft;
            var img = craftBadgeBtn.GetComponent<Image>();
            if (img != null) img.color = canCraft ? new Color(0.1f, 0.7f, 0.4f) : new Color(0.2f, 0.25f, 0.32f);
        }

        if (craftBadgeBtnText != null)
        {
            craftBadgeBtnText.text = canCraft ? "✨ СКРАФТИТЬ ЗНАЧОК (5/5)" : "Собрано не все карточки";
        }

        RefreshCardsGrid();
    }

    private void RefreshCardsGrid()
    {
        if (cardsGridContainer == null) return;

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            Transform cardTr = cardsGridContainer.Find($"CardItem_{i}");
            if (cardTr == null) continue;

            TMP_Text countTxt = cardTr.Find("CountBadge/Text")?.GetComponent<TMP_Text>();
            if (countTxt != null)
            {
                countTxt.text = $"x{card.countOwned}";
                countTxt.color = card.countOwned > 0 ? Color.white : new Color(0.6f, 0.6f, 0.65f);
            }

            Image bg = cardTr.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = card.countOwned > 0 
                    ? new Color(0.12f, 0.16f, 0.24f, 0.95f) 
                    : new Color(0.06f, 0.08f, 0.11f, 0.7f);
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("SteamTradingCardsModal", typeof(RectTransform));
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
        cardRt.sizeDelta = new Vector2(490, 640);
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.6f, 1f, 0.5f);
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
        headerTxt.text = "🃏 КАРТОЧКИ STEAM & КРАФТ ЗНАЧКОВ";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.35f, 0.8f, 1f);

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

        // Badge Preview Panel
        GameObject badgePanel = new GameObject("BadgePanel", typeof(RectTransform), typeof(Image));
        badgePanel.transform.SetParent(cardObj.transform, false);
        RectTransform badgeRt = badgePanel.GetComponent<RectTransform>();
        badgeRt.anchorMin = new Vector2(0, 1);
        badgeRt.anchorMax = new Vector2(1, 1);
        badgeRt.pivot = new Vector2(0.5f, 1);
        badgeRt.anchoredPosition = new Vector2(0, -60);
        badgeRt.sizeDelta = new Vector2(-40, 68);
        badgePanel.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

        GameObject bTitleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        bTitleObj.transform.SetParent(badgePanel.transform, false);
        RectTransform btrt = bTitleObj.GetComponent<RectTransform>();
        btrt.anchorMin = new Vector2(0, 0.5f);
        btrt.anchorMax = new Vector2(1, 1);
        btrt.offsetMin = new Vector2(12, 0);
        btrt.offsetMax = new Vector2(-12, -4);
        badgeTitleText = bTitleObj.GetComponent<TextMeshProUGUI>();
        badgeTitleText.fontSize = 14;
        badgeTitleText.fontStyle = FontStyles.Bold;

        GameObject bBonusObj = new GameObject("Bonus", typeof(RectTransform), typeof(TextMeshProUGUI));
        bBonusObj.transform.SetParent(badgePanel.transform, false);
        RectTransform bbrt = bBonusObj.GetComponent<RectTransform>();
        bbrt.anchorMin = new Vector2(0, 0);
        bbrt.anchorMax = new Vector2(1, 0.5f);
        bbrt.offsetMin = new Vector2(12, 4);
        bbrt.offsetMax = new Vector2(-12, 0);
        badgeBonusText = bBonusObj.GetComponent<TextMeshProUGUI>();
        badgeBonusText.fontSize = 12;

        // Cards Grid (5 items)
        GameObject gridObj = new GameObject("CardsGrid", typeof(RectTransform), typeof(VerticalLayoutGroup));
        gridObj.transform.SetParent(cardObj.transform, false);
        cardsGridContainer = gridObj.transform;
        RectTransform gridRt = gridObj.GetComponent<RectTransform>();
        gridRt.anchorMin = new Vector2(0, 0);
        gridRt.anchorMax = new Vector2(1, 1);
        gridRt.offsetMin = new Vector2(20, 120);
        gridRt.offsetMax = new Vector2(-20, -135);

        var vlg = gridObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;

        for (int i = 0; i < cards.Count; i++)
        {
            CreateCardListItem(cardsGridContainer, cards[i], i);
        }

        // Craft Badge Button
        GameObject craftBtnObj = new GameObject("CraftBadgeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        craftBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform craftRt = craftBtnObj.GetComponent<RectTransform>();
        craftRt.anchorMin = new Vector2(0, 0);
        craftRt.anchorMax = new Vector2(1, 0);
        craftRt.pivot = new Vector2(0.5f, 0);
        craftRt.anchoredPosition = new Vector2(0, 68);
        craftRt.sizeDelta = new Vector2(-40, 46);
        craftBadgeBtn = craftBtnObj.GetComponent<Button>();

        GameObject craftTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        craftTxtObj.transform.SetParent(craftBtnObj.transform, false);
        RectTransform craftTxtRt = craftTxtObj.GetComponent<RectTransform>();
        craftTxtRt.anchorMin = Vector2.zero;
        craftTxtRt.anchorMax = Vector2.one;
        craftTxtRt.sizeDelta = Vector2.zero;
        craftBadgeBtnText = craftTxtObj.GetComponent<TextMeshProUGUI>();
        craftBadgeBtnText.text = "✨ СКРАФТИТЬ ЗНАЧОК (5/5)";
        craftBadgeBtnText.fontSize = 14;
        craftBadgeBtnText.fontStyle = FontStyles.Bold;
        craftBadgeBtnText.alignment = TextAlignmentOptions.Center;
        craftBadgeBtnText.color = Color.white;

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 16);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeCardsBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero;
        closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "ЗАКРЫТЬ";
        closeTxt.fontSize = 14;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private void CreateCardListItem(Transform parent, TradingCard card, int index)
    {
        GameObject item = new GameObject($"CardItem_{index}", typeof(RectTransform), typeof(Image));
        item.transform.SetParent(parent, false);
        RectTransform itemRt = item.GetComponent<RectTransform>();
        itemRt.sizeDelta = new Vector2(430, 64);
        item.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.95f);

        // Icon
        GameObject ico = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
        ico.transform.SetParent(item.transform, false);
        RectTransform icoRt = ico.GetComponent<RectTransform>();
        icoRt.anchorMin = new Vector2(0, 0.5f);
        icoRt.anchorMax = new Vector2(0, 0.5f);
        icoRt.anchoredPosition = new Vector2(25, 0);
        icoRt.sizeDelta = new Vector2(40, 40);
        TMP_Text icot = ico.GetComponent<TextMeshProUGUI>();
        icot.text = card.icon;
        icot.fontSize = 24;
        icot.alignment = TextAlignmentOptions.Center;

        // Title & Desc
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(item.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(55, -6);
        trt.sizeDelta = new Vector2(-120, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"<color=#{ColorUtility.ToHtmlStringRGB(card.accentColor)}>{card.title}</color>";
        tt.fontSize = 13;
        tt.fontStyle = FontStyles.Bold;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(item.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(1, 1);
        drt.pivot = new Vector2(0, 0);
        drt.offsetMin = new Vector2(55, 6);
        drt.offsetMax = new Vector2(-65, -28);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = card.description;
        dt.fontSize = 10;
        dt.color = new Color(0.7f, 0.75f, 0.85f);

        // Count Badge
        GameObject badge = new GameObject("CountBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(item.transform, false);
        RectTransform brt = badge.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-30, 0);
        brt.sizeDelta = new Vector2(45, 28);
        badge.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.9f);

        GameObject countTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        countTxtObj.transform.SetParent(badge.transform, false);
        RectTransform ctrt = countTxtObj.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.sizeDelta = Vector2.zero;
        TMP_Text ct = countTxtObj.GetComponent<TextMeshProUGUI>();
        ct.text = $"x{card.countOwned}";
        ct.fontSize = 12;
        ct.fontStyle = FontStyles.Bold;
        ct.alignment = TextAlignmentOptions.Center;
    }
}
