using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Кастомизация уточки-маскота («Rubber Duck Debugging Wardrobe»):
/// - Дефолтная классическая желтая уточка
/// - Эксклюзивная «Уточка-Сеньор» (в очках, с ноутбуком и языками программирования)
/// - «Киберпанк-уточка 2077» с неоновой подсветкой
/// - «Золотая VIP Уточка» за престиж/IPO
/// - Интерактивная примерка, звуки кряка, визуальные эффекты и уникальные геймплейные перки
/// </summary>
public class DuckCustomizationUI : MonoBehaviour
{
    private static DuckCustomizationUI instance;
    public static DuckCustomizationUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DuckCustomizationUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(DuckCustomizationUI));
                    instance = go.AddComponent<DuckCustomizationUI>();
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
    public class DuckSkinData
    {
        public int skinIndex;
        public string id;
        public string name;
        public string title;
        public string icon;
        public string description;
        public string perkDescription;
        public double costMoney;
        public double costCode;
        public bool isUnlocked;
        public Color themeColor;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openWardrobeBtn;

    [Header("Информационный блок выбранной уточки")]
    [SerializeField] private TMP_Text selectedDuckTitleTxt;
    [SerializeField] private TMP_Text selectedDuckPerkTxt;
    [SerializeField] private Button quackPreviewBtn;
    [SerializeField] private TMP_Text quackPreviewBtnTxt;
    [SerializeField] private Transform skinsContainer;

    private readonly List<DuckSkinData> skins = new List<DuckSkinData>();
    private int currentSelectedSkin = 0;

    private const string PrefSelectedSkin = "SelectedDuckSkin";
    private const string PrefSkinUnlockedPrefix = "DuckSkin_Unlocked_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CurrentSkinIndex => currentSelectedSkin;
    public IReadOnlyList<DuckSkinData> Skins => skins;

    public event Action<int> OnDuckSkinChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeSkins();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeSkins()
    {
        skins.Clear();

        skins.Add(new DuckSkinData
        {
            skinIndex = 0,
            id = "duck_yellow",
            name = "Классическая Желтая Уточка",
            title = "🐥 Дефолтная Инди-Уточка",
            icon = "🐥",
            description = "Чистая классика резинового дебага. Верный друг в поиске пропущенных точек с запятой.",
            perkDescription = "+10% комбо-энергии при каждом нажатии",
            costMoney = 0,
            costCode = 0,
            isUnlocked = true,
            themeColor = new Color(1f, 0.85f, 0.2f)
        });

        skins.Add(new DuckSkinData
        {
            skinIndex = 1,
            id = "duck_coder",
            name = "Уточка-Сеньор (Программист)",
            title = "💻 Уточка-Программист с Ноутбуком",
            icon = "💻",
            description = "В очках, с верным ThinkPad и принтами Python, C#, Rust и JS. Видела падение прода и легаси.",
            perkDescription = "x2 строк кода при кряке и +20% к длительности комбо",
            costMoney = 15000,
            costCode = 3000,
            isUnlocked = false,
            themeColor = new Color(0.2f, 0.85f, 1f)
        });

        skins.Add(new DuckSkinData
        {
            skinIndex = 2,
            id = "duck_cyber",
            name = "Киберпанк-Уточка 2077",
            title = "⚡ Кибер-Уточка с Имплантами",
            icon = "⚡",
            description = "Хромированный клюв, оптический визор и нейроинтерфейс. Дебажит память на наноуровне.",
            perkDescription = "+25% к шансу и силе критического клика",
            costMoney = 50000,
            costCode = 12000,
            isUnlocked = false,
            themeColor = new Color(0.95f, 0.2f, 0.95f)
        });

        skins.Add(new DuckSkinData
        {
            skinIndex = 3,
            id = "duck_gold",
            name = "Золотая VIP Уточка",
            title = "👑 Золотая Уточка-Миллиардер",
            icon = "👑",
            description = "Отлита из чистого золота в честь успешного IPO. Крякает со звоном золотых монет.",
            perkDescription = "+20% ко всем денежным доходам студии",
            costMoney = 150000,
            costCode = 35000,
            isUnlocked = false,
            themeColor = new Color(1f, 0.84f, 0f)
        });
    }

    private void LoadData()
    {
        currentSelectedSkin = PlayerPrefs.GetInt(PrefSelectedSkin, 0);
        currentSelectedSkin = Mathf.Clamp(currentSelectedSkin, 0, skins.Count - 1);

        for (int i = 0; i < skins.Count; i++)
        {
            if (i == 0)
            {
                skins[i].isUnlocked = true;
            }
            else
            {
                skins[i].isUnlocked = PlayerPrefs.GetInt(PrefSkinUnlockedPrefix + skins[i].id, 0) == 1;
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSelectedSkin, currentSelectedSkin);
        for (int i = 0; i < skins.Count; i++)
        {
            PlayerPrefs.SetInt(PrefSkinUnlockedPrefix + skins[i].id, skins[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public double GetDuckMultiplier()
    {
        // Перк активной уточки
        if (currentSelectedSkin == 3) return 1.20; // +20% дохода
        return 1.0;
    }

    public double GetDuckClickMultiplier()
    {
        if (currentSelectedSkin == 2) return 1.25; // +25% клика
        if (currentSelectedSkin == 1) return 1.15; // +15% клика
        return 1.0;
    }

    public void SelectSkin(int skinIdx)
    {
        if (skinIdx < 0 || skinIdx >= skins.Count) return;
        var s = skins[skinIdx];

        if (!s.isUnlocked)
        {
            // Покупка скина
            if (GameManager.Instance == null || GameManager.Instance.Money < s.costMoney || GameManager.Instance.CodeLines < s.costCode)
            {
                HapticFeedback.LightImpact();
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(s.costMoney)} ₽ и {NumberFormatter.Format(s.costCode)} кода!", transform.position, new Color(1f, 0.3f, 0.3f), false);
                }
                return;
            }

            GameManager.Instance.SpendMoney(s.costMoney);
            GameManager.Instance.SpendLinesOfCode(s.costCode);
            s.isUnlocked = true;
        }

        currentSelectedSkin = skinIdx;
        SaveData();

        HapticFeedback.DuckQuackHaptic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDuckQuack();

        // Синхронизируем с рабочим местом
        var visuals = FindFirstObjectByType<WorkplaceVisuals>();
        if (visuals != null)
        {
            visuals.ApplyDuckSkinExternal(skinIdx);
        }

        OnDuckSkinChanged?.Invoke(skinIdx);

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🦆 НАДЕТ СКИН:\n{s.icon} {s.name}\n<color=#00FF88>{s.perkDescription}</color>", transform.position, s.themeColor, true);
        }

        UpdateModalUI();
    }

    public void QuackPreview()
    {
        HapticFeedback.DuckQuackHaptic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDuckQuack();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.15f);
            double bonus = GameManager.Instance.GetCodePerClick() * (currentSelectedSkin == 1 ? 8.0 : 4.0);
            GameManager.Instance.AddDirectCurrencies(bonus, 0.0);
        }

        if (ClickJuice.Instance != null)
        {
            var s = skins[currentSelectedSkin];
            ClickJuice.Instance.SpawnCustomPopup($"🦆 «КРЯ-А-А!»\n{s.perkDescription}", transform.position + Vector3.up * 30f, s.themeColor, false);
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
        if (openWardrobeBtn != null)
        {
            openWardrobeBtn.onClick.RemoveAllListeners();
            openWardrobeBtn.onClick.AddListener(OpenModal);
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
        if (quackPreviewBtn != null)
        {
            quackPreviewBtn.onClick.RemoveAllListeners();
            quackPreviewBtn.onClick.AddListener(QuackPreview);
        }
    }

    private void UpdateModalUI()
    {
        var cur = skins[Mathf.Clamp(currentSelectedSkin, 0, skins.Count - 1)];

        if (selectedDuckTitleTxt != null)
        {
            selectedDuckTitleTxt.text = $"🦆 ТЕКУЩИЙ СКИН: <color=#{ColorUtility.ToHtmlStringRGB(cur.themeColor)}>{cur.name}</color>";
        }

        if (selectedDuckPerkTxt != null)
        {
            selectedDuckPerkTxt.text = $"Эффект: <b><color=#00FF88>{cur.perkDescription}</color></b>\n<size=10><color=#90B0D0>{cur.description}</color></size>";
        }

        RefreshSkinCards();
    }

    private void RefreshSkinCards()
    {
        if (skinsContainer == null) return;

        for (int i = 0; i < skins.Count; i++)
        {
            int idx = i;
            var s = skins[i];
            Transform cardTr = skinsContainer.Find($"SkinCard_{idx}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actionBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = actionBtn != null ? actionBtn.GetComponentInChildren<TMP_Text>() : null;

            bool isSelected = currentSelectedSkin == idx;

            if (bg != null)
            {
                bg.color = isSelected 
                    ? new Color(0.18f, 0.22f, 0.12f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.92f);
            }

            if (actionBtn != null && btnTxt != null)
            {
                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() => SelectSkin(idx));

                if (isSelected)
                {
                    actionBtn.interactable = false;
                    btnTxt.text = "⭐ НАДЕТО";
                }
                else if (s.isUnlocked)
                {
                    actionBtn.interactable = true;
                    btnTxt.text = "НАДЕТЬ";
                }
                else
                {
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= s.costMoney &&
                                     GameManager.Instance.CodeLines >= s.costCode;
                    actionBtn.interactable = canAfford;
                    btnTxt.text = $"ОТКРЫТЬ\n{NumberFormatter.Format(s.costMoney)} ₽ | {NumberFormatter.Format(s.costCode)} Кода";
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
        GameObject root = new GameObject("DuckCustomizationModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.6f);
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
        headerTxt.text = "🦆 ГАРДЕРОБ РЕЗИНОВОЙ УТОЧКИ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.88f, 0.25f);

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

        // Current Duck Info Box
        GameObject infoBox = new GameObject("InfoBox", typeof(RectTransform), typeof(Image));
        infoBox.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoBox.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -52);
        infoRt.sizeDelta = new Vector2(-36, 68);
        infoBox.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.9f);

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(infoBox.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -6);
        titleRt.sizeDelta = new Vector2(-16, 22);
        selectedDuckTitleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        selectedDuckTitleTxt.fontSize = 12;
        selectedDuckTitleTxt.fontStyle = FontStyles.Bold;
        selectedDuckTitleTxt.alignment = TextAlignmentOptions.Center;
        selectedDuckTitleTxt.color = Color.white;

        GameObject perkObj = new GameObject("Perk", typeof(RectTransform), typeof(TextMeshProUGUI));
        perkObj.transform.SetParent(infoBox.transform, false);
        RectTransform perkRt = perkObj.GetComponent<RectTransform>();
        perkRt.anchorMin = new Vector2(0, 0);
        perkRt.anchorMax = new Vector2(1, 0);
        perkRt.pivot = new Vector2(0.5f, 0);
        perkRt.anchoredPosition = new Vector2(0, 6);
        perkRt.sizeDelta = new Vector2(-16, 32);
        selectedDuckPerkTxt = perkObj.GetComponent<TextMeshProUGUI>();
        selectedDuckPerkTxt.fontSize = 10;
        selectedDuckPerkTxt.alignment = TextAlignmentOptions.Center;
        selectedDuckPerkTxt.color = new Color(0.9f, 0.95f, 1f);

        // Quack Interactive Button
        GameObject qkObj = new GameObject("QuackBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        qkObj.transform.SetParent(cardObj.transform, false);
        RectTransform qkRt = qkObj.GetComponent<RectTransform>();
        qkRt.anchorMin = new Vector2(0, 1);
        qkRt.anchorMax = new Vector2(1, 1);
        qkRt.pivot = new Vector2(0.5f, 1);
        qkRt.anchoredPosition = new Vector2(0, -128);
        qkRt.sizeDelta = new Vector2(-36, 38);
        qkObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.1f, 0.95f);
        quackPreviewBtn = qkObj.GetComponent<Button>();

        GameObject qkTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        qkTxtObj.transform.SetParent(qkObj.transform, false);
        RectTransform qkTxtRt = qkTxtObj.GetComponent<RectTransform>();
        qkTxtRt.anchorMin = Vector2.zero; qkTxtRt.anchorMax = Vector2.one; qkTxtRt.sizeDelta = Vector2.zero;
        quackPreviewBtnTxt = qkTxtObj.GetComponent<TextMeshProUGUI>();
        quackPreviewBtnTxt.fontSize = 12;
        quackPreviewBtnTxt.fontStyle = FontStyles.Bold;
        quackPreviewBtnTxt.alignment = TextAlignmentOptions.Center;
        quackPreviewBtnTxt.text = "🔊 ПОЖАТЬ УТОЧКУ (КРЯКНУТЬ ДЛЯ БУСТА)";
        quackPreviewBtnTxt.color = Color.white;

        // Scroll Container for Skins
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -174);
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
        skinsContainer = contentObj.transform;

        for (int i = 0; i < skins.Count; i++)
        {
            CreateSkinCardTemplate(skinsContainer, skins[i]);
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

    private void CreateSkinCardTemplate(Transform parent, DuckSkinData s)
    {
        GameObject card = new GameObject($"SkinCard_{s.skinIndex}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 72);
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
        tt.text = $"{s.icon} <b>{s.name}</b>";
        tt.fontSize = 12;
        tt.color = s.themeColor;

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.62f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{s.perkDescription}</color>";
        dt.fontSize = 10;

        // Action button
        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, 0);
        brt.sizeDelta = new Vector2(150, 36);
        b.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.35f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ВЫБРАТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openWardrobeBtn != null) return;

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

        GameObject btnGo = new GameObject("DuckHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.45f, 0.35f, 0.1f, 0.9f);
        openWardrobeBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🦆 Уточка";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
