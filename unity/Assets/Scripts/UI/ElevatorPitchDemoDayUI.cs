using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система Венчурных Питчей и Презентаций («Elevator Pitch & Demo Day»):
/// - Интерактивное выступление студии на Демо-Дне перед жюри мировых венчурных фондов
/// - Карточки аргументов питча (Product-Market Fit, AI Automation, Viral Loops, Unit Economics)
/// - Оценка презентации инвесторами (баллы от 60 до 100) и подписание инвестиционных чеков
/// - Постоянный множитель инвестиционной оценки к пассивному доходу компании
/// </summary>
public class ElevatorPitchDemoDayUI : MonoBehaviour
{
    private static ElevatorPitchDemoDayUI instance;
    public static ElevatorPitchDemoDayUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ElevatorPitchDemoDayUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(ElevatorPitchDemoDayUI));
                    instance = go.AddComponent<ElevatorPitchDemoDayUI>();
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
    public class PitchArgument
    {
        public string id;
        public string title;
        public string icon;
        public string statement;
        public int scorePoints;
        public double bonusMultiplier;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openDemoDayBtn;

    [Header("Интерактивный питч")]
    [SerializeField] private TMP_Text demoDayStatsSummaryTxt;
    [SerializeField] private TMP_Text stageSpeechTxt;
    [SerializeField] private Slider investorInterestSlider;
    [SerializeField] private TMP_Text investorInterestScoreTxt;
    [SerializeField] private Button startPitchBtn;
    [SerializeField] private TMP_Text startPitchBtnTxt;
    [SerializeField] private Transform cardsContainer;

    private readonly List<PitchArgument> arguments = new List<PitchArgument>();
    private bool isPitching = false;
    private int currentPitchScore = 0;
    private int successfulPitchesCount = 0;
    private int argumentsPlayedThisSession = 0;

    private const string PrefPitchesCount = "DemoDay_PitchesCount";
    private const string PrefBestScore = "DemoDay_BestScore";
    private int bestPitchScore = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeArguments();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeArguments()
    {
        if (arguments.Count > 0) return;

        arguments.Add(new PitchArgument
        {
            id = "arg_pmf",
            title = "Product-Market Fit & Retention",
            icon = "🎯",
            statement = "«Наше D30 удержание — 42%, пользователи влюблены в механику кликера!»",
            scorePoints = 25,
            bonusMultiplier = 1.15
        });

        arguments.Add(new PitchArgument
        {
            id = "arg_ai_engine",
            title = "Zero-GC & AI Automation Core",
            icon = "🤖",
            statement = "«Архитектура на чистом C# без сборки мусора и с нейросетевым пайплайном контента!»",
            scorePoints = 30,
            bonusMultiplier = 1.25
        });

        arguments.Add(new PitchArgument
        {
            id = "arg_viral",
            title = "Viral UGC & Social Reach",
            icon = "🔥",
            statement = "«Виральные шортсы и мемы привлекают органический трафик по стоимости 0 рублей!»",
            scorePoints = 25,
            bonusMultiplier = 1.20
        });

        arguments.Add(new PitchArgument
        {
            id = "arg_unit_econ",
            title = "Unit Economics: LTV/CAC 4.8x",
            icon = "💎",
            statement = "«Каждый вложенный рубль возвращается пятикратно за первые 14 дней!»",
            scorePoints = 35,
            bonusMultiplier = 1.30
        });
    }

    private void LoadData()
    {
        successfulPitchesCount = PlayerPrefs.GetInt(PrefPitchesCount, 0);
        bestPitchScore = PlayerPrefs.GetInt(PrefBestScore, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefPitchesCount, successfulPitchesCount);
        PlayerPrefs.SetInt(PrefBestScore, bestPitchScore);
        PlayerPrefs.Save();
    }

    public double GetDemoDayIncomeMultiplier()
    {
        // Перманентный бонус к доходу от проведенных питчей и лучшего скора
        return 1.0 + Math.Min(successfulPitchesCount * 0.05, 1.20) + (bestPitchScore * 0.002);
    }

    public void StartDemoDayPitch()
    {
        if (isPitching) return;

        isPitching = true;
        currentPitchScore = 0;
        argumentsPlayedThisSession = 0;

        if (stageSpeechTxt != null)
        {
            stageSpeechTxt.text = "🎤 Вы вышли на сцену перед инвесторами. Выберите 3 ключевых тезиса презентации!";
        }

        if (startPitchBtn != null) startPitchBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        RefreshUI();
    }

    public void PlayArgument(string argId)
    {
        if (!isPitching || argumentsPlayedThisSession >= 3) return;

        var arg = arguments.Find(a => a.id == argId);
        if (arg == null) return;

        argumentsPlayedThisSession++;
        currentPitchScore += arg.scorePoints;

        if (stageSpeechTxt != null)
        {
            stageSpeechTxt.text = $"🗣️ Студия: {arg.statement}\n<color=#00FFAA>Инвесторы впечатлены! (+{arg.scorePoints} баллов)</color>";
        }

        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        RefreshUI();

        if (argumentsPlayedThisSession >= 3)
        {
            StartCoroutine(FinishPitchRoutine());
        }
    }

    private IEnumerator FinishPitchRoutine()
    {
        if (stageSpeechTxt != null)
        {
            stageSpeechTxt.text = "⏳ ЖЮРИ СОВЕЩАЕТСЯ И ВЫСТАВЛЯЕТ ИТОГОВУЮ ОЦЕНКУ...";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        successfulPitchesCount++;
        if (currentPitchScore > bestPitchScore) bestPitchScore = currentPitchScore;

        double baseGrant = 60000.0 * (currentPitchScore / 70.0);
        double checkMoney = Math.Floor(baseGrant * GetDemoDayIncomeMultiplier());
        double bonusCode = Math.Floor(checkMoney * 0.35);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(checkMoney);
            GameManager.Instance.AddLinesOfCode(bonusCode);
            GameManager.Instance.AddComboEnergy(0.50f);
        }

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 ТЕРМШИТ ПОДПИСАН!\nОценка питча: <b>{currentPitchScore}/100</b>\nЧек инвестиций: <b>+{NumberFormatter.Format(checkMoney)} ₽</b> (+{NumberFormatter.Format(bonusCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isPitching = false;
        if (startPitchBtn != null) startPitchBtn.interactable = true;
        if (stageSpeechTxt != null)
        {
            stageSpeechTxt.text = $"🏆 Питч завершен со счетом {currentPitchScore}/100! Инвесторы вложили {NumberFormatter.Format(checkMoney)} ₽.";
        }

        RefreshUI();
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
        if (startPitchBtn != null)
        {
            startPitchBtn.onClick.RemoveAllListeners();
            startPitchBtn.onClick.AddListener(StartDemoDayPitch);
        }
        if (openDemoDayBtn != null)
        {
            openDemoDayBtn.onClick.RemoveAllListeners();
            openDemoDayBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (demoDayStatsSummaryTxt != null)
        {
            double mult = GetDemoDayIncomeMultiplier();
            demoDayStatsSummaryTxt.text = $"Успешных Demo Day: <color=#00FFAA><b>{successfulPitchesCount}</b></color> | Рекорд: <color=#FFD700><b>{bestPitchScore} pts</b></color>\nМножитель оценки: <color=#00FFAA>x{mult:0.00}</color>";
        }

        if (investorInterestSlider != null)
        {
            investorInterestSlider.value = Mathf.Clamp01(currentPitchScore / 100f);
        }

        if (investorInterestScoreTxt != null)
        {
            investorInterestScoreTxt.text = isPitching ? $"{currentPitchScore}/100 pts (Сдано тезисов: {argumentsPlayedThisSession}/3)" : $"{bestPitchScore}/100 pts";
        }

        if (startPitchBtnTxt != null && !isPitching)
        {
            startPitchBtnTxt.text = "🎤 НАЧАТЬ ВЫСТУПЛЕНИЕ НА DEMO DAY";
        }

        if (cardsContainer == null) return;

        for (int i = 0; i < arguments.Count; i++)
        {
            var arg = arguments[i];
            Transform child = i < cardsContainer.childCount ? cardsContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            Button playBtn = child.Find("Action/PlayBtn")?.GetComponent<Button>();
            TMP_Text playBtnTxt = playBtn != null ? playBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{arg.icon} {arg.title}";
            }
            if (descTxt != null)
            {
                descTxt.text = arg.statement;
            }

            if (playBtn != null && playBtnTxt != null)
            {
                playBtn.interactable = isPitching && argumentsPlayedThisSession < 3;
                playBtnTxt.text = $"ПРИВЕСТИ ТЕЗИС (+{arg.scorePoints})";
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("ElevatorPitch_ModalRoot", typeof(RectTransform));
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
        GameObject card = new GameObject("PitchCard", typeof(RectTransform), typeof(Image));
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
        header.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.28f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🎤 DEMO DAY & ELEVATOR PITCH";
        tTxt.fontSize = 19;
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
        GameObject statsObj = new GameObject("DemoDayStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        demoDayStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        demoDayStatsSummaryTxt.fontSize = 13;
        demoDayStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        demoDayStatsSummaryTxt.color = new Color(0.92f, 0.92f, 1f);

        // Stage Panel (Speech text and score)
        GameObject stagePanel = new GameObject("StagePanel", typeof(RectTransform), typeof(Image));
        stagePanel.transform.SetParent(card.transform, false);
        RectTransform spRect = stagePanel.GetComponent<RectTransform>();
        spRect.anchorMin = new Vector2(0f, 1f);
        spRect.anchorMax = new Vector2(1f, 1f);
        spRect.pivot = new Vector2(0.5f, 1f);
        spRect.sizeDelta = new Vector2(-30, 80);
        spRect.anchoredPosition = new Vector2(0, -126);
        stagePanel.GetComponent<Image>().color = new Color(0.16f, 0.16f, 0.25f, 1f);

        GameObject speechObj = new GameObject("SpeechTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        speechObj.transform.SetParent(stagePanel.transform, false);
        RectTransform spcRect = speechObj.GetComponent<RectTransform>();
        spcRect.anchorMin = new Vector2(0f, 0.35f);
        spcRect.anchorMax = new Vector2(1f, 1f);
        spcRect.offsetMin = new Vector2(10, 0);
        spcRect.offsetMax = new Vector2(-10, -5);
        stageSpeechTxt = speechObj.GetComponent<TextMeshProUGUI>();
        stageSpeechTxt.fontSize = 12;
        stageSpeechTxt.alignment = TextAlignmentOptions.Center;
        stageSpeechTxt.color = new Color(1f, 0.95f, 0.8f);
        stageSpeechTxt.text = "Нажмите «НАЧАТЬ ВЫСТУПЛЕНИЕ», чтобы представить студию фондам!";

        // Score Slider
        GameObject sliderObj = new GameObject("InterestSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(stagePanel.transform, false);
        RectTransform slRect = sliderObj.GetComponent<RectTransform>();
        slRect.anchorMin = new Vector2(0.05f, 0.1f);
        slRect.anchorMax = new Vector2(0.75f, 0.3f);
        slRect.offsetMin = Vector2.zero;
        slRect.offsetMax = Vector2.zero;
        investorInterestSlider = sliderObj.GetComponent<Slider>();

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.GetComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fRect = fill.GetComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.offsetMin = Vector2.zero;
        fRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.2f, 0.85f, 1f);
        investorInterestSlider.fillRect = fRect;

        GameObject scTxtObj = new GameObject("ScoreTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        scTxtObj.transform.SetParent(stagePanel.transform, false);
        RectTransform scrRect = scTxtObj.GetComponent<RectTransform>();
        scrRect.anchorMin = new Vector2(0.76f, 0.05f);
        scrRect.anchorMax = new Vector2(0.98f, 0.35f);
        scrRect.offsetMin = Vector2.zero;
        scrRect.offsetMax = Vector2.zero;
        investorInterestScoreTxt = scTxtObj.GetComponent<TextMeshProUGUI>();
        investorInterestScoreTxt.fontSize = 12;
        investorInterestScoreTxt.fontStyle = FontStyles.Bold;
        investorInterestScoreTxt.alignment = TextAlignmentOptions.Center;
        investorInterestScoreTxt.color = new Color(0.2f, 1f, 0.5f);

        // Start Pitch Action Button
        GameObject pBtnObj = new GameObject("StartPitchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pBtnObj.transform.SetParent(card.transform, false);
        RectTransform pbRect = pBtnObj.GetComponent<RectTransform>();
        pbRect.anchorMin = new Vector2(0f, 1f);
        pbRect.anchorMax = new Vector2(1f, 1f);
        pbRect.pivot = new Vector2(0.5f, 1f);
        pbRect.sizeDelta = new Vector2(-30, 48);
        pbRect.anchoredPosition = new Vector2(0, -212);
        pBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.60f, 0.38f, 1f);
        startPitchBtn = pBtnObj.GetComponent<Button>();

        GameObject ptTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        ptTxtObj.transform.SetParent(pBtnObj.transform, false);
        startPitchBtnTxt = ptTxtObj.GetComponent<TextMeshProUGUI>();
        startPitchBtnTxt.text = "🎤 НАЧАТЬ ВЫСТУПЛЕНИЕ НА DEMO DAY";
        startPitchBtnTxt.fontSize = 14;
        startPitchBtnTxt.fontStyle = FontStyles.Bold;
        startPitchBtnTxt.alignment = TextAlignmentOptions.Center;
        startPitchBtnTxt.color = Color.white;
        RectTransform ptr = ptTxtObj.GetComponent<RectTransform>();
        ptr.anchorMin = Vector2.zero;
        ptr.anchorMax = Vector2.one;
        ptr.offsetMin = Vector2.zero;
        ptr.offsetMax = Vector2.zero;

        // Arguments Scroll View
        GameObject scrollObj = new GameObject("CardsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -268);
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
        cardsContainer = content.transform;
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

        foreach (var arg in arguments)
        {
            CreateArgumentCardUI(content.transform, arg);
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

    private void CreateArgumentCardUI(Transform parent, PitchArgument arg)
    {
        GameObject card = new GameObject("ArgCard_" + arg.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 78);
        card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.23f, 1f);

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
        tTxt.fontSize = 13;
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
        br.anchorMax = new Vector2(0.68f, 1f);
        br.offsetMin = new Vector2(10, 8);
        br.offsetMax = new Vector2(0, -30);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.color = new Color(0.85f, 0.90f, 1f);
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = Vector2.zero;
        dr.anchorMax = Vector2.one;
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;

        // Action
        GameObject act = new GameObject("Action", typeof(RectTransform));
        act.transform.SetParent(card.transform, false);
        RectTransform ar = act.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.70f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 8);
        ar.offsetMax = new Vector2(-10, -10);

        GameObject pBtn = new GameObject("PlayBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pBtn.transform.SetParent(act.transform, false);
        RectTransform pbr = pBtn.GetComponent<RectTransform>();
        pbr.anchorMin = Vector2.zero;
        pbr.anchorMax = Vector2.one;
        pbr.offsetMin = Vector2.zero;
        pbr.offsetMax = Vector2.zero;
        pBtn.GetComponent<Image>().color = new Color(0.20f, 0.55f, 0.85f, 1f);

        GameObject pTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        pTxt.transform.SetParent(pBtn.transform, false);
        TextMeshProUGUI pt = pTxt.GetComponent<TextMeshProUGUI>();
        pt.fontSize = 10;
        pt.fontStyle = FontStyles.Bold;
        pt.alignment = TextAlignmentOptions.Center;
        pt.color = Color.white;
        RectTransform ptr = pTxt.GetComponent<RectTransform>();
        ptr.anchorMin = Vector2.zero;
        ptr.anchorMax = Vector2.one;
        ptr.offsetMin = Vector2.zero;
        ptr.offsetMax = Vector2.zero;

        string currentArgId = arg.id;
        pBtn.GetComponent<Button>().onClick.AddListener(() => PlayArgument(currentArgId));
    }
}
