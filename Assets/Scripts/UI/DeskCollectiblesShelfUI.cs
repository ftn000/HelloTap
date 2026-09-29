using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Физические Коллекционные Фигурки на Полку («Desk Toys & Collectibles Shelf»):
/// - 5 уникальных коллекционных фигурок геймдева над столом разработчика
/// - Интерактивное взаимодействие: осмотр фигурки, анимация покачивания, звуки клика и микро-бонусы
/// - Перманентные пассивные баффы к доходу компании, силе клика, критам и скорости релизов
/// - Сохранение в PlayerPrefs, тактильный отклик и праздничные эффекты
/// </summary>
public class DeskCollectiblesShelfUI : MonoBehaviour
{
    private static DeskCollectiblesShelfUI instance;
    public static DeskCollectiblesShelfUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DeskCollectiblesShelfUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(DeskCollectiblesShelfUI));
                    instance = go.AddComponent<DeskCollectiblesShelfUI>();
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
    public class DeskToy
    {
        public string id;
        public string title;
        public string icon;
        public string lore;
        public string perkDesc;
        public double costMoney;
        public double costCode;
        public bool isOwned;
        public bool isDisplayedOnShelf;
        public Color themeColor;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openShelfBtn;

    [Header("Информационный заголовок и полка")]
    [SerializeField] private TMP_Text shelfSummaryTxt;
    [SerializeField] private Button pokeShelfBtn;
    [SerializeField] private TMP_Text pokeShelfBtnTxt;
    [SerializeField] private Transform toysContainer;

    private readonly List<DeskToy> toys = new List<DeskToy>();

    private const string PrefToyOwnedPrefix = "DeskToy_Owned_";
    private const string PrefToyShelfPrefix = "DeskToy_OnShelf_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<DeskToy> Toys => toys;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeToys();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeToys()
    {
        toys.Clear();

        toys.Add(new DeskToy
        {
            id = "toy_nes",
            title = "Ретро-картридж 'Golden 8-bit'",
            icon = "🕹️",
            lore = "Легендарный золотой картридж. Если подуть в контакты, код начинает писаться сам собой.",
            perkDesc = "+20% к строкам кода за каждый клик",
            costMoney = 18000,
            costCode = 3500,
            isOwned = false,
            isDisplayedOnShelf = true,
            themeColor = new Color(1f, 0.85f, 0.2f)
        });

        toys.Add(new DeskToy
        {
            id = "toy_switch",
            title = "Механический свитч Cherry MX Blue",
            icon = "⌨️",
            lore = "Истинный звук инди-разработки. Звонкий клик ласкает уши и разгоняет тактовую частоту пальцев.",
            perkDesc = "+25% к шансу и множителю критического клика",
            costMoney = 45000,
            costCode = 9000,
            isOwned = false,
            isDisplayedOnShelf = true,
            themeColor = new Color(0.2f, 0.6f, 1f)
        });

        toys.Add(new DeskToy
        {
            id = "toy_bughunter",
            title = "Робот-дroid 'Bug Hunter Bot'",
            icon = "🤖",
            lore = "Автономный охотник за багами. Сканирует прод на наличие регрессий и утечек памяти.",
            perkDesc = "+20% к скорости релизов игровых проектов",
            costMoney = 85000,
            costCode = 18000,
            isOwned = false,
            isDisplayedOnShelf = true,
            themeColor = new Color(0.2f, 0.9f, 0.5f)
        });

        toys.Add(new DeskToy
        {
            id = "toy_floppy",
            title = "Неоновая дискета 1.44 MB",
            icon = "💾",
            lore = "Священная дискета с чистым ассемблером. Надежно хранит наработки студии в оффлайне.",
            perkDesc = "+25% к оффлайн доходу студии",
            costMoney = 140000,
            costCode = 30000,
            isOwned = false,
            isDisplayedOnShelf = true,
            themeColor = new Color(0.9f, 0.3f, 0.9f)
        });

        toys.Add(new DeskToy
        {
            id = "toy_csharp",
            title = "Голографический куб C# / Unity",
            icon = "💎",
            lore = "Светящийся кристалл дзен-программирования. Излучает ауру безупречной архитектуры без багов.",
            perkDesc = "+30% ко всем денежным доходам студии перманентно",
            costMoney = 300000,
            costCode = 70000,
            isOwned = false,
            isDisplayedOnShelf = true,
            themeColor = new Color(0.1f, 0.9f, 1f)
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < toys.Count; i++)
        {
            toys[i].isOwned = PlayerPrefs.GetInt(PrefToyOwnedPrefix + toys[i].id, 0) == 1;
            toys[i].isDisplayedOnShelf = PlayerPrefs.GetInt(PrefToyShelfPrefix + toys[i].id, 1) == 1;
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < toys.Count; i++)
        {
            PlayerPrefs.SetInt(PrefToyOwnedPrefix + toys[i].id, toys[i].isOwned ? 1 : 0);
            PlayerPrefs.SetInt(PrefToyShelfPrefix + toys[i].id, toys[i].isDisplayedOnShelf ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public double GetCollectiblesIncomeMultiplier()
    {
        double mult = 1.0;
        var csharp = toys.Find(t => t.id == "toy_csharp");
        if (csharp != null && csharp.isOwned) mult += 0.30;

        var floppy = toys.Find(t => t.id == "toy_floppy");
        if (floppy != null && floppy.isOwned) mult += 0.15;

        return mult;
    }

    public double GetCollectiblesClickMultiplier()
    {
        double mult = 1.0;
        var nes = toys.Find(t => t.id == "toy_nes");
        if (nes != null && nes.isOwned) mult += 0.20;

        var sw = toys.Find(t => t.id == "toy_switch");
        if (sw != null && sw.isOwned) mult += 0.25;

        return mult;
    }

    public int GetOwnedToysCount()
    {
        int count = 0;
        for (int i = 0; i < toys.Count; i++)
        {
            if (toys[i].isOwned) count++;
        }
        return count;
    }

    public bool TryBuyToy(string toyId)
    {
        var toy = toys.Find(t => t.id == toyId);
        if (toy == null || toy.isOwned) return false;

        if (GameManager.Instance == null || GameManager.Instance.Money < toy.costMoney || GameManager.Instance.CodeLines < toy.costCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(toy.costMoney)} ₽ и {NumberFormatter.Format(toy.costCode)} кода!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendMoney(toy.costMoney);
        GameManager.Instance.SpendLinesOfCode(toy.costCode);

        toy.isOwned = true;
        toy.isDisplayedOnShelf = true;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🧸 ФИГУРКА НА ПОЛКЕ!\n{toy.icon} {toy.title}\n<color=#00FF88>{toy.perkDesc}</color>", transform.position, toy.themeColor, true);
        }

        UpdateModalUI();
        return true;
    }

    public void PokeRandomToy()
    {
        var ownedToys = toys.FindAll(t => t.isOwned);
        if (ownedToys.Count == 0)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("🧸 Полка пока пуста! Купите первую фигурку.", transform.position, Color.yellow, false);
            }
            return;
        }

        int r = UnityEngine.Random.Range(0, ownedToys.Count);
        var t = ownedToys[r];

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.15f);
            double bonus = Math.Max(10.0, GameManager.Instance.GetCodePerClick() * 5.0);
            GameManager.Instance.AddDirectCurrencies(bonus, 0.0);
        }

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Щелчок по {t.icon} {t.title}!\n+Вдохновение и комбо!", transform.position + Vector3.up * 25f, t.themeColor, false);
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

    private void BindButtons()
    {
        if (openShelfBtn != null)
        {
            openShelfBtn.onClick.RemoveAllListeners();
            openShelfBtn.onClick.AddListener(OpenModal);
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
        if (pokeShelfBtn != null)
        {
            pokeShelfBtn.onClick.RemoveAllListeners();
            pokeShelfBtn.onClick.AddListener(PokeRandomToy);
        }
    }

    private void UpdateModalUI()
    {
        int owned = GetOwnedToysCount();

        if (shelfSummaryTxt != null)
        {
            double clickB = (GetCollectiblesClickMultiplier() - 1.0) * 100.0;
            double incB = (GetCollectiblesIncomeMultiplier() - 1.0) * 100.0;
            shelfSummaryTxt.text = $"Собрано фигурок: <b><color=#FFD700>{owned}/{toys.Count}</color></b> | Клик: <b><color=#00FF88>+{clickB:F0}%</color></b> | Доход: <b><color=#00E5FF>+{incB:F0}%</color></b>";
        }

        RefreshToyCards();
    }

    private void RefreshToyCards()
    {
        if (toysContainer == null) return;

        for (int i = 0; i < toys.Count; i++)
        {
            var toy = toys[i];
            Transform cardTr = toysContainer.Find($"ToyCard_{toy.id}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actionBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = actionBtn != null ? actionBtn.GetComponentInChildren<TMP_Text>() : null;

            if (bg != null)
            {
                bg.color = toy.isOwned 
                    ? new Color(0.14f, 0.18f, 0.24f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.92f);
            }

            if (actionBtn != null && btnTxt != null)
            {
                string tId = toy.id;
                actionBtn.onClick.RemoveAllListeners();

                if (toy.isOwned)
                {
                    actionBtn.onClick.AddListener(PokeRandomToy);
                    actionBtn.interactable = true;
                    btnTxt.text = "✨ ОСМОТРЕТЬ";
                }
                else
                {
                    actionBtn.onClick.AddListener(() => TryBuyToy(tId));
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= toy.costMoney &&
                                     GameManager.Instance.CodeLines >= toy.costCode;
                    actionBtn.interactable = canAfford;
                    btnTxt.text = $"КУПИТЬ\n{NumberFormatter.Format(toy.costMoney)} ₽ | {NumberFormatter.Format(toy.costCode)} Кода";
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
        GameObject root = new GameObject("DeskCollectiblesModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.75f, 0.2f, 0.6f);
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
        headerTxt.text = "🧸 ПОЛКА КОЛЛЕКЦИОННЫХ ФИГУРОК";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.85f, 0.25f);

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

        // Shelf Summary Box
        GameObject sumObj = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumObj.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 42);
        sumObj.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.20f, 0.9f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumObj.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        shelfSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        shelfSummaryTxt.fontSize = 11;
        shelfSummaryTxt.alignment = TextAlignmentOptions.Center;
        shelfSummaryTxt.color = new Color(1f, 0.95f, 0.8f);

        // Poke Shelf Interactive Button
        GameObject pokeObj = new GameObject("PokeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pokeObj.transform.SetParent(cardObj.transform, false);
        RectTransform pokeRt = pokeObj.GetComponent<RectTransform>();
        pokeRt.anchorMin = new Vector2(0, 1);
        pokeRt.anchorMax = new Vector2(1, 1);
        pokeRt.pivot = new Vector2(0.5f, 1);
        pokeRt.anchoredPosition = new Vector2(0, -100);
        pokeRt.sizeDelta = new Vector2(-36, 38);
        pokeObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f, 0.95f);
        pokeShelfBtn = pokeObj.GetComponent<Button>();

        GameObject pokeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        pokeTxtObj.transform.SetParent(pokeObj.transform, false);
        RectTransform pokeTxtRt = pokeTxtObj.GetComponent<RectTransform>();
        pokeTxtRt.anchorMin = Vector2.zero; pokeTxtRt.anchorMax = Vector2.one; pokeTxtRt.sizeDelta = Vector2.zero;
        pokeShelfBtnTxt = pokeTxtObj.GetComponent<TextMeshProUGUI>();
        pokeShelfBtnTxt.fontSize = 11;
        pokeShelfBtnTxt.fontStyle = FontStyles.Bold;
        pokeShelfBtnTxt.alignment = TextAlignmentOptions.Center;
        pokeShelfBtnTxt.text = "✨ ПОЩЕЛКАТЬ ПО ФИГУРКАМ НА ПОЛКЕ";
        pokeShelfBtnTxt.color = Color.white;

        // Scroll Container for Toys
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -146);
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
        toysContainer = contentObj.transform;

        for (int i = 0; i < toys.Count; i++)
        {
            CreateToyCardTemplate(toysContainer, toys[i]);
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

    private void CreateToyCardTemplate(Transform parent, DeskToy toy)
    {
        GameObject card = new GameObject($"ToyCard_{toy.id}", typeof(RectTransform), typeof(Image));
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
        tt.text = $"{toy.icon} <b>{toy.title}</b>";
        tt.fontSize = 12;
        tt.color = toy.themeColor;

        // Lore and perk text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.62f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{toy.perkDesc}</color>\n<size=9><color=#8095B0>{toy.lore}</color></size>";
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
        b.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.65f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "КУПИТЬ";
        btxt.fontSize = 9;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openShelfBtn != null) return;

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

        GameObject btnGo = new GameObject("ShelfHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.45f, 0.35f, 0.15f, 0.9f);
        openShelfBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🧸 Полка";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
