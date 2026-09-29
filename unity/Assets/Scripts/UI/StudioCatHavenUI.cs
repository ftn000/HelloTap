using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Питомник и Кошачье Кафе в офисе (Studio Cat Haven):
/// - 4 уникальных котика (Рыжик, Барсик, Мейн-кун Цезарь, Кибер-Кот Нео)
/// - Обустройство кошачьего уголка (Мягкие лежанки, Когтеточка-башня, Автокормушка)
/// - Интерактивное поглаживание и кормление котиков
/// - Постоянные баффы к охлаждению серверов, критам и пассивному доходу студии
/// </summary>
public class StudioCatHavenUI : MonoBehaviour
{
    private static StudioCatHavenUI instance;
    public static StudioCatHavenUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<StudioCatHavenUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class StudioCat
    {
        public string id;
        public string name;
        public string breed;
        public string icon;
        public string perkDesc;
        public double adoptCost;
        public double bonusMultiplier;
        public int affectionLevel;
        public bool isAdopted;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openHavenBtn;
    [SerializeField] private TMP_Text openHavenBtnText;
    [SerializeField] private Button closeHavenBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка приюта")]
    [SerializeField] private TMP_Text totalCatsAdoptedText;
    [SerializeField] private TMP_Text havenMultiplierText;

    [Header("Контейнер котиков")]
    [SerializeField] private Transform catsContainer;

    private readonly List<StudioCat> cats = new List<StudioCat>();

    private const string PrefAdoptedPrefix = "Studio_CatAdopted_";
    private const string PrefAffectionPrefix = "Studio_CatAffection_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<StudioCat> Cats => cats;

    public double GetCatHavenMultiplier()
    {
        double mult = 1.0;
        for (int i = 0; i < cats.Count; i++)
        {
            if (cats[i].isAdopted)
            {
                mult += cats[i].bonusMultiplier * (1.0 + cats[i].affectionLevel * 0.1);
            }
        }
        return mult;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCats();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeCats()
    {
        cats.Clear();

        cats.Add(new StudioCat
        {
            id = "cat_ginger",
            name = "Рыжик",
            breed = "Уютный рыжий полосатик",
            icon = "🐱",
            perkDesc = "Анти-стресс: ускоряет охлаждение серверов на 25%",
            adoptCost = 0.0, // Бесплатный первый кот
            bonusMultiplier = 0.05,
            affectionLevel = 1,
            isAdopted = true
        });

        cats.Add(new StudioCat
        {
            id = "cat_tuxedo",
            name = "Барсик",
            breed = "Черно-белый смокинг",
            icon = "🐈",
            perkDesc = "Кот на клавиатуре: +15% к пассивному коду персонала",
            adoptCost = 35000.0,
            bonusMultiplier = 0.10,
            affectionLevel = 0,
            isAdopted = false
        });

        cats.Add(new StudioCat
        {
            id = "cat_caesar",
            name = "Цезарь",
            breed = "Мейн-кун Великан",
            icon = "🦁",
            perkDesc = "Взгляд босса: +20% к силе критических кликов",
            adoptCost = 150000.0,
            bonusMultiplier = 0.15,
            affectionLevel = 0,
            isAdopted = false
        });

        cats.Add(new StudioCat
        {
            id = "cat_cyber",
            name = "Нео",
            breed = "Кибер-Кот с LED-ошейником",
            icon = "⚡",
            perkDesc = "Квантовый дзен: +20% абсолютный множитель доходов",
            adoptCost = 650000.0,
            bonusMultiplier = 0.20,
            affectionLevel = 0,
            isAdopted = false
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < cats.Count; i++)
        {
            if (i == 0) cats[i].isAdopted = true;
            else cats[i].isAdopted = PlayerPrefs.GetInt(PrefAdoptedPrefix + cats[i].id, 0) == 1;

            cats[i].affectionLevel = PlayerPrefs.GetInt(PrefAffectionPrefix + cats[i].id, i == 0 ? 1 : 0);
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < cats.Count; i++)
        {
            PlayerPrefs.SetInt(PrefAdoptedPrefix + cats[i].id, cats[i].isAdopted ? 1 : 0);
            PlayerPrefs.SetInt(PrefAffectionPrefix + cats[i].id, cats[i].affectionLevel);
        }
        PlayerPrefs.Save();
    }

    public void AdoptCat(int index)
    {
        if (index < 0 || index >= cats.Count) return;
        var cat = cats[index];

        if (cat.isAdopted) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < cat.adoptCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств ({NumberFormatter.Format(cat.adoptCost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(cat.adoptCost);
        cat.isAdopted = true;
        cat.affectionLevel = 1;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCatPurr();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🐾 КОТИК ПРИЮЧЕН!\n{cat.icon} {cat.name} ({cat.breed})\n{cat.perkDesc}", transform.position, new Color(1f, 0.6f, 0.8f), true);
        }

        UpdateModalUI();
    }

    public void PetCat(int index)
    {
        if (index < 0 || index >= cats.Count) return;
        var cat = cats[index];

        if (!cat.isAdopted) return;

        cat.affectionLevel++;
        SaveData();

        // Поглаживание даёт мгновенную порцию кода от радости кота
        double codeBonus = 2500.0 * cat.affectionLevel;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLinesOfCode(codeBonus);
        }

        if (cat.id == "cat_ginger" && ServerRackUI.Instance != null)
        {
            ServerRackUI.Instance.OnEmergencyCoolingClicked();
        }

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCatPurr();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💖 МУРРР! Уровень любви {cat.name}: {cat.affectionLevel}\n+{NumberFormatter.Format(codeBonus)} строк кода!", transform.position, new Color(1f, 0.4f, 0.75f), false);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openHavenBtn != null)
        {
            openHavenBtn.onClick.RemoveAllListeners();
            openHavenBtn.onClick.AddListener(OpenModal);
        }
        if (closeHavenBtn != null)
        {
            closeHavenBtn.onClick.RemoveAllListeners();
            closeHavenBtn.onClick.AddListener(CloseModal);
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
        int countAdopted = 0;
        for (int i = 0; i < cats.Count; i++) if (cats[i].isAdopted) countAdopted++;

        if (totalCatsAdoptedText != null)
        {
            totalCatsAdoptedText.text = $"🐾 В приюте студии: <b>{countAdopted}/{cats.Count} котиков</b>";
        }

        if (havenMultiplierText != null)
        {
            double m = GetCatHavenMultiplier();
            havenMultiplierText.text = $"Множитель кошачьего уюта: <b><color=#FF77AA>x{m:F2}</color></b>";
        }

        RefreshCatsList();
    }

    private void RefreshCatsList()
    {
        if (catsContainer == null) return;

        double currentMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;

        for (int i = 0; i < cats.Count; i++)
        {
            int idx = i;
            var c = cats[i];
            Transform cardTr = catsContainer.Find($"CatCard_{idx}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text actBtnTxt = actBtn != null ? actBtn.GetComponentInChildren<TMP_Text>() : null;
            TMP_Text statusTxt = cardTr.Find("AffectionText")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = c.isAdopted 
                    ? new Color(0.18f, 0.12f, 0.20f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.90f);
            }

            if (statusTxt != null)
            {
                statusTxt.text = c.isAdopted ? $"❤️ Любовь: Ур. {c.affectionLevel}" : "Ждёт нового дома";
                statusTxt.color = c.isAdopted ? new Color(1f, 0.5f, 0.8f) : new Color(0.6f, 0.7f, 0.8f);
            }

            if (actBtn != null && actBtnTxt != null)
            {
                actBtn.onClick.RemoveAllListeners();

                if (c.isAdopted)
                {
                    actBtn.interactable = true;
                    actBtnTxt.text = "💖 ПОГЛАДИТЬ";
                    actBtn.GetComponent<Image>().color = new Color(0.85f, 0.35f, 0.65f);
                    actBtn.onClick.AddListener(() => PetCat(idx));
                }
                else
                {
                    bool canAfford = currentMoney >= c.adoptCost;
                    actBtn.interactable = canAfford;
                    actBtnTxt.text = $"ПРИЮТИТЬ ({NumberFormatter.Format(c.adoptCost)} ₽)";
                    actBtn.GetComponent<Image>().color = canAfford ? new Color(0.2f, 0.6f, 0.8f) : new Color(0.25f, 0.25f, 0.3f);
                    actBtn.onClick.AddListener(() => AdoptCat(idx));
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("StudioCatHavenModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.5f, 0.8f, 0.5f);
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
        headerTxt.text = "🐱 КОШАЧЬЕ КАФЕ СТУДИИ";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.6f, 0.85f);

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
        infoRt.anchoredPosition = new Vector2(0, -56);
        infoRt.sizeDelta = new Vector2(-36, 68);
        infoPanel.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

        GameObject totObj = new GameObject("TotalCats", typeof(RectTransform), typeof(TextMeshProUGUI));
        totObj.transform.SetParent(infoPanel.transform, false);
        RectTransform totRt = totObj.GetComponent<RectTransform>();
        totRt.anchorMin = new Vector2(0, 0.5f);
        totRt.anchorMax = new Vector2(1, 1);
        totRt.offsetMin = new Vector2(12, 0);
        totRt.offsetMax = new Vector2(-12, -4);
        totalCatsAdoptedText = totObj.GetComponent<TextMeshProUGUI>();
        totalCatsAdoptedText.fontSize = 13;
        totalCatsAdoptedText.fontStyle = FontStyles.Bold;

        GameObject multObj = new GameObject("Mult", typeof(RectTransform), typeof(TextMeshProUGUI));
        multObj.transform.SetParent(infoPanel.transform, false);
        RectTransform multRt = multObj.GetComponent<RectTransform>();
        multRt.anchorMin = new Vector2(0, 0);
        multRt.anchorMax = new Vector2(1, 0.5f);
        multRt.offsetMin = new Vector2(12, 4);
        multRt.offsetMax = new Vector2(-12, 0);
        havenMultiplierText = multObj.GetComponent<TextMeshProUGUI>();
        havenMultiplierText.fontSize = 12;

        // Scroll View with Cats
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 68);
        scrollRt.offsetMax = new Vector2(-18, -135);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        catsContainer = contentObj.transform;
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;

        for (int i = 0; i < cats.Count; i++)
        {
            CreateCatCardTemplate(catsContainer, cats[i], i);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 14);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeHavenBtn = closeBtnObj.GetComponent<Button>();

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

    private void CreateCatCardTemplate(Transform parent, StudioCat cat, int index)
    {
        GameObject card = new GameObject($"CatCard_{index}", typeof(RectTransform), typeof(Image));
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
        tt.text = $"{cat.icon} <b>{cat.name}</b> <color=#D0A0C0>({cat.breed})</color>";
        tt.fontSize = 13;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.6f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"{cat.perkDesc}";
        dt.fontSize = 11;
        dt.color = new Color(0.85f, 0.85f, 0.95f);

        GameObject aff = new GameObject("AffectionText", typeof(RectTransform), typeof(TextMeshProUGUI));
        aff.transform.SetParent(card.transform, false);
        RectTransform affrt = aff.GetComponent<RectTransform>();
        affrt.anchorMin = new Vector2(0.62f, 1);
        affrt.anchorMax = new Vector2(1, 1);
        affrt.pivot = new Vector2(1, 1);
        affrt.anchoredPosition = new Vector2(-10, -8);
        affrt.sizeDelta = new Vector2(140, 20);
        TMP_Text afft = aff.GetComponent<TextMeshProUGUI>();
        afft.fontSize = 11;
        afft.alignment = TextAlignmentOptions.Right;

        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0);
        brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(1, 0);
        brt.anchoredPosition = new Vector2(-10, 8);
        brt.sizeDelta = new Vector2(150, 32);
        b.GetComponent<Image>().color = new Color(0.85f, 0.35f, 0.65f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "💖 ПОГЛАДИТЬ";
        btxt.fontSize = 11;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }
}
