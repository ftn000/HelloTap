using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Геймдев-конференции и Шоукейсы (DevGAMM, White Nights, The Game Awards):
/// - 4 престижных шоукейса с арендой стенда и защитой перед жюри
/// - Выбор оформления стенда (Мерч, Косплей, Энергетики) для максимизации хайпа
/// - Вручение престижных наград ("Best Indie", "Grand Prize", "TGA Indie of the Year")
/// - Крупные денежные гранты, приток фанатов и постоянные пассивные бонусы к продажам
/// </summary>
public class DevConferenceShowcaseUI : MonoBehaviour
{
    private static DevConferenceShowcaseUI instance;
    public static DevConferenceShowcaseUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<DevConferenceShowcaseUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class ConferenceEvent
    {
        public string id;
        public string title;
        public string location;
        public string icon;
        public double entryFee;
        public double cashPrize;
        public int minReleases;
        public int minOfficeTier;
        public string nominationTitle;
        public bool isCompleted;
        public int bestScore;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openConferenceBtn;
    [SerializeField] private TMP_Text openConferenceBtnText;
    [SerializeField] private Button closeConferenceBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Контейнер списка конференций")]
    [SerializeField] private Transform conferenceListContainer;

    [Header("Интерактивный экран участия (Showcase Demo Day)")]
    [SerializeField] private GameObject showcaseModalRoot;
    [SerializeField] private TMP_Text showcaseHeaderTitle;
    [SerializeField] private TMP_Text showcaseStatusText;
    [SerializeField] private Button boothUpgradeMerchBtn;
    [SerializeField] private Button boothUpgradeCosplayBtn;
    [SerializeField] private Button pitchToPressBtn;
    [SerializeField] private TMP_Text boothHypeText;
    [SerializeField] private TMP_Text pitchScoreText;

    private readonly List<ConferenceEvent> conferences = new List<ConferenceEvent>();
    private int activeShowcaseIndex = -1;
    private int currentBoothHype = 50; // 0..100
    private bool hasMerchUpgrade = false;
    private bool hasCosplayUpgrade = false;

    private const string PrefConfPrefix = "Studio_ConfCompleted_";
    private const string PrefConfScore = "Studio_ConfScore_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<ConferenceEvent> Conferences => conferences;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeConferences();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        if (showcaseModalRoot != null) showcaseModalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeConferences()
    {
        conferences.Clear();

        conferences.Add(new ConferenceEvent
        {
            id = "conf_igroprom",
            title = "Игропром Инди-Стенд",
            location = "Москва / Онлайн",
            icon = "🎪",
            entryFee = 15000.0,
            cashPrize = 75000.0,
            minReleases = 1,
            minOfficeTier = 0,
            nominationTitle = "⭐ Лучший дебют года",
            isCompleted = false,
            bestScore = 0
        });

        conferences.Add(new ConferenceEvent
        {
            id = "conf_devgamm",
            title = "DevGAMM Conference",
            location = "Гданьск / Вильнюс",
            icon = "🎮",
            entryFee = 85000.0,
            cashPrize = 400000.0,
            minReleases = 2,
            minOfficeTier = 1,
            nominationTitle = "🏆 Excellence in Game Design",
            isCompleted = false,
            bestScore = 0
        });

        conferences.Add(new ConferenceEvent
        {
            id = "conf_whitenights",
            title = "White Nights Showcase",
            location = "Санкт-Петербург",
            icon = "🏛️",
            entryFee = 350000.0,
            cashPrize = 1500000.0,
            minReleases = 3,
            minOfficeTier = 2,
            nominationTitle = "👑 Grand Prize & Publisher Choice",
            isCompleted = false,
            bestScore = 0
        });

        conferences.Add(new ConferenceEvent
        {
            id = "conf_tga",
            title = "The Game Awards (Indie Showcase)",
            location = "Лос-Анджелес (Live Stream)",
            icon = "🌟",
            entryFee = 2500000.0,
            cashPrize = 12000000.0,
            minReleases = 4,
            minOfficeTier = 3,
            nominationTitle = "💎 Indie Game of the Year (TGA)",
            isCompleted = false,
            bestScore = 0
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < conferences.Count; i++)
        {
            conferences[i].isCompleted = PlayerPrefs.GetInt(PrefConfPrefix + conferences[i].id, 0) == 1;
            conferences[i].bestScore = PlayerPrefs.GetInt(PrefConfScore + conferences[i].id, 0);
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < conferences.Count; i++)
        {
            PlayerPrefs.SetInt(PrefConfPrefix + conferences[i].id, conferences[i].isCompleted ? 1 : 0);
            PlayerPrefs.SetInt(PrefConfScore + conferences[i].id, conferences[i].bestScore);
        }
        PlayerPrefs.Save();
    }

    public void RegisterForConference(int index)
    {
        if (index < 0 || index >= conferences.Count) return;
        var conf = conferences[index];

        int releases = GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount : 0;
        int office = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.CurrentTierIndex : 0;

        if (releases < conf.minReleases || office < conf.minOfficeTier)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🔒 Требуется: {conf.minReleases} релиза и Офис #{conf.minOfficeTier + 1}!", transform.position, new Color(1f, 0.4f, 0.2f), false);
            }
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.Money < conf.entryFee)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств для аренды стенда ({NumberFormatter.Format(conf.entryFee)} ₽)!", transform.position, new Color(1f, 0.35f, 0.35f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(conf.entryFee);
        activeShowcaseIndex = index;
        currentBoothHype = UnityEngine.Random.Range(45, 65);
        hasMerchUpgrade = false;
        hasCosplayUpgrade = false;

        OpenShowcaseModal(conf);
    }

    private void OpenShowcaseModal(ConferenceEvent conf)
    {
        if (showcaseModalRoot != null)
        {
            showcaseModalRoot.SetActive(true);
        }

        if (showcaseHeaderTitle != null) showcaseHeaderTitle.text = $"{conf.icon} {conf.title} ({conf.location})";
        if (showcaseStatusText != null) showcaseStatusText.text = $"Стенд открыт! Посетители собираются вокруг демо-компьютеров. Повысьте хайп стенда и проведите финальный питч перед жюри!";

        UpdateShowcaseUI();

        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    public void UpgradeBoothMerch()
    {
        if (hasMerchUpgrade) return;
        double cost = 5000.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.SpendMoney(cost);
            hasMerchUpgrade = true;
            currentBoothHype = Mathf.Min(100, currentBoothHype + 22);

            HapticFeedback.MediumImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

            if (ClickJuice.Instance != null && boothUpgradeMerchBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("🎁 Раздача мерча и стикеров! +22% Хайпа!", boothUpgradeMerchBtn.transform.position, new Color(0.2f, 1f, 0.6f), false);
            }
            UpdateShowcaseUI();
        }
    }

    public void UpgradeBoothCosplay()
    {
        if (hasCosplayUpgrade) return;
        double cost = 12000.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.SpendMoney(cost);
            hasCosplayUpgrade = true;
            currentBoothHype = Mathf.Min(100, currentBoothHype + 28);

            HapticFeedback.MediumImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

            if (ClickJuice.Instance != null && boothUpgradeCosplayBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("🎭 Косплееры главных героев игры! +28% Хайпа!", boothUpgradeCosplayBtn.transform.position, new Color(0.3f, 0.8f, 1f), false);
            }
            UpdateShowcaseUI();
        }
    }

    public void FinishShowcasePitch()
    {
        if (activeShowcaseIndex < 0 || activeShowcaseIndex >= conferences.Count) return;
        var conf = conferences[activeShowcaseIndex];

        int juryRoll = UnityEngine.Random.Range(10, 25);
        int finalScore = Mathf.Clamp(currentBoothHype + juryRoll, 50, 100);

        conf.isCompleted = true;
        if (finalScore > conf.bestScore) conf.bestScore = finalScore;

        double prize = conf.cashPrize * (finalScore / 100.0);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(prize);
            GameManager.Instance.ActivateEnergyBoost(3600f, 1.4); // 1 час x1.4 буст
        }

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        string awardStatus = finalScore >= 80 ? $"🥇 ПОБЕДИТЕЛЬ: {conf.nominationTitle}!" : "🥈 ПОЧЁТНЫЙ ДИПЛОМ ШОУКЕЙСА!";
        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 ШОУКЕЙС ЗАВЕРШЁН!\n{awardStatus}\nОценка жюри: {finalScore}/100\n+{NumberFormatter.Format(prize)} ₽", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        if (showcaseModalRoot != null) showcaseModalRoot.SetActive(false);
        activeShowcaseIndex = -1;

        UpdateModalUI();
    }

    private void UpdateShowcaseUI()
    {
        if (boothHypeText != null)
        {
            boothHypeText.text = $"🔥 Хайп вокруг стенда: <b>{currentBoothHype}%</b>";
        }

        if (pitchScoreText != null)
        {
            pitchScoreText.text = $"Мерч: {(hasMerchUpgrade ? "✓ Активен" : "5 000 ₽")} | Косплей: {(hasCosplayUpgrade ? "✓ Нанят" : "12 000 ₽")}";
        }

        if (boothUpgradeMerchBtn != null) boothUpgradeMerchBtn.interactable = !hasMerchUpgrade;
        if (boothUpgradeCosplayBtn != null) boothUpgradeCosplayBtn.interactable = !hasCosplayUpgrade;
    }

    private void BindButtons()
    {
        if (openConferenceBtn != null)
        {
            openConferenceBtn.onClick.RemoveAllListeners();
            openConferenceBtn.onClick.AddListener(OpenModal);
        }
        if (closeConferenceBtn != null)
        {
            closeConferenceBtn.onClick.RemoveAllListeners();
            closeConferenceBtn.onClick.AddListener(CloseModal);
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

        if (boothUpgradeMerchBtn != null)
        {
            boothUpgradeMerchBtn.onClick.RemoveAllListeners();
            boothUpgradeMerchBtn.onClick.AddListener(UpgradeBoothMerch);
        }
        if (boothUpgradeCosplayBtn != null)
        {
            boothUpgradeCosplayBtn.onClick.RemoveAllListeners();
            boothUpgradeCosplayBtn.onClick.AddListener(UpgradeBoothCosplay);
        }
        if (pitchToPressBtn != null)
        {
            pitchToPressBtn.onClick.RemoveAllListeners();
            pitchToPressBtn.onClick.AddListener(FinishShowcasePitch);
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
        if (showcaseModalRoot != null) showcaseModalRoot.SetActive(false);
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
        RefreshConferenceCards();
    }

    private void RefreshConferenceCards()
    {
        if (conferenceListContainer == null) return;

        int releases = GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount : 0;
        int office = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.CurrentTierIndex : 0;

        for (int i = 0; i < conferences.Count; i++)
        {
            int idx = i;
            var conf = conferences[i];
            Transform cardTr = conferenceListContainer.Find($"ConfCard_{idx}");
            if (cardTr == null) continue;

            Button regBtn = cardTr.Find("RegBtn")?.GetComponent<Button>();
            TMP_Text regBtnTxt = regBtn != null ? regBtn.GetComponentInChildren<TMP_Text>() : null;
            Image bg = cardTr.GetComponent<Image>();

            bool unlocked = releases >= conf.minReleases && office >= conf.minOfficeTier;

            if (bg != null)
            {
                if (conf.isCompleted) bg.color = new Color(0.12f, 0.22f, 0.18f, 0.95f);
                else if (unlocked) bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
                else bg.color = new Color(0.06f, 0.08f, 0.11f, 0.85f);
            }

            if (regBtn != null && regBtnTxt != null)
            {
                regBtn.onClick.RemoveAllListeners();
                regBtn.onClick.AddListener(() => RegisterForConference(idx));

                if (!unlocked)
                {
                    regBtn.interactable = false;
                    regBtnTxt.text = $"🔒 {conf.minReleases} рел. / Офис #{conf.minOfficeTier + 1}";
                }
                else
                {
                    bool canAfford = GameManager.Instance != null && GameManager.Instance.Money >= conf.entryFee;
                    regBtn.interactable = canAfford;
                    regBtnTxt.text = conf.isCompleted 
                        ? $"ПОВТОРИТЬ ({NumberFormatter.Format(conf.entryFee)} ₽)" 
                        : $"АРЕНДА ({NumberFormatter.Format(conf.entryFee)} ₽)";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("DevConferenceShowcaseModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.75f, 0.2f, 0.5f);
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
        headerTxt.text = "🏆 ГЕЙМДЕВ-КОНФЕРЕНЦИИ И ШОУКЕЙСЫ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.85f, 0.25f);

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

        // Subtitle Info
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(cardObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0, 1);
        subRt.anchorMax = new Vector2(1, 1);
        subRt.pivot = new Vector2(0.5f, 1);
        subRt.anchoredPosition = new Vector2(0, -56);
        subRt.sizeDelta = new Vector2(-40, 40);
        TMP_Text subTxt = subObj.GetComponent<TextMeshProUGUI>();
        subTxt.text = "Арендуйте стенды, показывайте демо игрокам и защищайте свои проекты перед жюри за награды и гранты!";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.8f, 0.85f, 0.95f);

        // Scroll View with Conferences
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(20, 70);
        scrollRt.offsetMax = new Vector2(-20, -100);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        conferenceListContainer = contentObj.transform;
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

        for (int i = 0; i < conferences.Count; i++)
        {
            CreateConferenceCardTemplate(conferenceListContainer, conferences[i], i);
        }

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
        closeConferenceBtn = closeBtnObj.GetComponent<Button>();

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

        // Setup Showcase Sub-Modal
        CreateShowcaseSubModal(root.transform);

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private void CreateConferenceCardTemplate(Transform parent, ConferenceEvent conf, int index)
    {
        GameObject card = new GameObject($"ConfCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 100);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        // Title & Location
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{conf.icon} <b>{conf.title}</b> <color=#90B0D0>({conf.location})</color>";
        tt.fontSize = 13;

        // Details
        GameObject d = new GameObject("Details", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.65f, 1);
        drt.offsetMin = new Vector2(10, 8);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        string bestScoreStr = conf.bestScore > 0 ? $" | Рекорд: {conf.bestScore}/100" : "";
        dt.text = $"Номинация: <b>{conf.nominationTitle}</b>\n💰 Грант победителя: <color=#00FF88>до {NumberFormatter.Format(conf.cashPrize)} ₽</color>{bestScoreStr}";
        dt.fontSize = 11;
        dt.color = new Color(0.8f, 0.85f, 0.9f);

        // Register Button
        GameObject b = new GameObject("RegBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-10, -4);
        brt.sizeDelta = new Vector2(145, 34);
        b.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.85f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = $"АРЕНДА ({NumberFormatter.Format(conf.entryFee)} ₽)";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void CreateShowcaseSubModal(Transform parent)
    {
        GameObject scRoot = new GameObject("ShowcaseModal", typeof(RectTransform));
        scRoot.transform.SetParent(parent, false);
        RectTransform prt = scRoot.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.sizeDelta = Vector2.zero;

        // Card
        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(scRoot.transform, false);
        RectTransform crt = card.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(450, 440);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.99f);
        var outline = card.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.8f, 0.2f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Header Title
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, -15);
        trt.sizeDelta = new Vector2(-30, 30);
        showcaseHeaderTitle = t.GetComponent<TextMeshProUGUI>();
        showcaseHeaderTitle.fontSize = 17;
        showcaseHeaderTitle.fontStyle = FontStyles.Bold;
        showcaseHeaderTitle.alignment = TextAlignmentOptions.Center;
        showcaseHeaderTitle.color = new Color(1f, 0.85f, 0.2f);

        // Status text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 1);
        drt.anchorMax = new Vector2(1, 1);
        drt.pivot = new Vector2(0.5f, 1);
        drt.anchoredPosition = new Vector2(0, -50);
        drt.sizeDelta = new Vector2(-30, 60);
        showcaseStatusText = d.GetComponent<TextMeshProUGUI>();
        showcaseStatusText.fontSize = 11;
        showcaseStatusText.alignment = TextAlignmentOptions.Center;
        showcaseStatusText.color = new Color(0.85f, 0.9f, 0.95f);

        // Hype label
        GameObject h = new GameObject("Hype", typeof(RectTransform), typeof(TextMeshProUGUI));
        h.transform.SetParent(card.transform, false);
        RectTransform hrt = h.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0, 1);
        hrt.anchorMax = new Vector2(1, 1);
        hrt.pivot = new Vector2(0.5f, 1);
        hrt.anchoredPosition = new Vector2(0, -115);
        hrt.sizeDelta = new Vector2(-30, 24);
        boothHypeText = h.GetComponent<TextMeshProUGUI>();
        boothHypeText.fontSize = 15;
        boothHypeText.fontStyle = FontStyles.Bold;
        boothHypeText.alignment = TextAlignmentOptions.Center;
        boothHypeText.color = new Color(1f, 0.45f, 0.1f);

        // Pitch Score Info
        GameObject p = new GameObject("ScoreInfo", typeof(RectTransform), typeof(TextMeshProUGUI));
        p.transform.SetParent(card.transform, false);
        RectTransform pr = p.GetComponent<RectTransform>();
        pr.anchorMin = new Vector2(0, 1);
        pr.anchorMax = new Vector2(1, 1);
        pr.pivot = new Vector2(0.5f, 1);
        pr.anchoredPosition = new Vector2(0, -140);
        pr.sizeDelta = new Vector2(-30, 22);
        pitchScoreText = p.GetComponent<TextMeshProUGUI>();
        pitchScoreText.fontSize = 11;
        pitchScoreText.alignment = TextAlignmentOptions.Center;
        pitchScoreText.color = new Color(0.7f, 0.8f, 0.9f);

        // Upgrade Buttons
        boothUpgradeMerchBtn = CreateShowcaseActionButton(card.transform, "🎁 Раздача мерча и футболок (-5 000 ₽)", -175, new Color(0.12f, 0.5f, 0.7f));
        boothUpgradeCosplayBtn = CreateShowcaseActionButton(card.transform, "🎭 Нанять косплееров на стенд (-12 000 ₽)", -235, new Color(0.15f, 0.6f, 0.4f));
        pitchToPressBtn = CreateShowcaseActionButton(card.transform, "🏆 ЗАЩИТИТЬ ПРОЕКТ ПЕРЕД ЖЮРИ", -300, new Color(0.85f, 0.55f, 0.1f));

        // Cancel
        GameObject cBtn = new GameObject("CancelBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cBtn.transform.SetParent(card.transform, false);
        RectTransform cbrt = cBtn.GetComponent<RectTransform>();
        cbrt.anchorMin = new Vector2(0.5f, 0);
        cbrt.anchorMax = new Vector2(0.5f, 0);
        cbrt.pivot = new Vector2(0.5f, 0);
        cbrt.anchoredPosition = new Vector2(0, 16);
        cbrt.sizeDelta = new Vector2(200, 36);
        cBtn.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f);
        var cb = cBtn.GetComponent<Button>();
        cb.onClick.AddListener(() => scRoot.SetActive(false));

        GameObject ctxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        ctxtObj.transform.SetParent(cBtn.transform, false);
        RectTransform ctrt = ctxtObj.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.sizeDelta = Vector2.zero;
        TMP_Text ctxt = ctxtObj.GetComponent<TextMeshProUGUI>();
        ctxt.text = "ОТМЕНА";
        ctxt.fontSize = 12;
        ctxt.alignment = TextAlignmentOptions.Center;
        ctxt.color = Color.white;

        showcaseModalRoot = scRoot;
        showcaseModalRoot.SetActive(false);
    }

    private Button CreateShowcaseActionButton(Transform parent, string label, float posY, Color col)
    {
        GameObject b = new GameObject("ActBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(400, 50);
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = new Vector2(-16, 0);
        TMP_Text t = to.GetComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 12;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        return b.GetComponent<Button>();
    }
}
