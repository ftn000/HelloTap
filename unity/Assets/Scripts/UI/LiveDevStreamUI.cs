using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Стриминг-комната разработчика (Live Dev Streaming Mini-Game):
/// - Прямой эфир разработки инди-игры (симуляция стрима на Twitch/YouTube)
/// - Динамический счетчик зрителей онлайн (Viewers count)
/// - Бегущий чат с комментариями зрителей, смайликами и мемами
/// - Входящие донаты от зрителей в реальном времени с голосовыми уведомлениями
/// - Мини-игра модерации чата: бан троллей и спамеров для запуска Hype Train!
/// </summary>
public class LiveDevStreamUI : MonoBehaviour
{
    private static LiveDevStreamUI instance;
    public static LiveDevStreamUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<LiveDevStreamUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openStreamBtn;
    [SerializeField] private TMP_Text openStreamBtnText;
    [SerializeField] private Button closeStreamBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Индикаторы стрима")]
    [SerializeField] private TMP_Text liveIndicatorText;
    [SerializeField] private TMP_Text viewerCountText;
    [SerializeField] private TMP_Text streamUptimeText;
    [SerializeField] private TMP_Text totalDonationsText;

    [Header("Чат стрима")]
    [SerializeField] private Transform chatMessagesContainer;
    [SerializeField] private ScrollRect chatScrollRect;

    [Header("Управление и модерация")]
    [SerializeField] private Button toggleStreamBtn;
    [SerializeField] private TMP_Text toggleStreamBtnText;
    [SerializeField] private Button banTrollBtn;
    [SerializeField] private TMP_Text banTrollBtnText;
    [SerializeField] private GameObject hypeTrainBadgeObj;

    private bool isStreaming = false;
    private float streamDuration = 0f;
    private int currentViewers = 0;
    private double sessionDonations = 0;
    private float nextChatMessageTimer = 0f;
    private float nextDonationTimer = 0f;
    private float trollSpamTimer = 0f;
    private bool isTrollActive = false;
    private float trollTimeoutDuration = 0f;
    private float hypeTrainRemaining = 0f;

    private static readonly string[] ViewerNames = new string[] {
        "xX_GamerPro_Xx", "CyberCat_42", "PixelKnight", "CoffeeCoder", "IndieFan_99",
        "JuniorDev123", "RustEnthusiast", "ShaderWizard", "MemerCat", "RetroGamer"
    };

    private static readonly string[] ChatPhrases = new string[] {
        "Кайфовый проект, когда в ранний доступ?",
        "PogChamp графон выглядит сочно!",
        "LUL кот опять спит на фоне?",
        "На каком движке пилишь?",
        "Музыка на стриме топ!",
        "Кликни еще быстрее!",
        "Жду в Стиме в вишлист закинул",
        "gg wp чистый дзен"
    };

    private static readonly string[] TrollPhrases = new string[] {
        "ИГРА ОТСТОЙ ВЕРНИТЕ ДЕНЬГИ!",
        "Спамлю рекламу дешевых ассетов тут: badsite.com",
        "Да это же клон флэшки из 2010!",
        "Движок кривой, я лучше на JS напишу!"
    };

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool IsStreaming => isStreaming;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (modalRoot != null) modalRoot.SetActive(false);
        if (hypeTrainBadgeObj != null) hypeTrainBadgeObj.SetActive(false);
        if (banTrollBtn != null) banTrollBtn.gameObject.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void Update()
    {
        if (!isStreaming) return;

        float dt = Time.deltaTime;
        streamDuration += dt;

        // Колебания числа зрителей
        if (UnityEngine.Random.value < 0.1f)
        {
            int baseV = 150 + (GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount * 350 : 0);
            if (hypeTrainRemaining > 0) baseV *= 3;
            currentViewers = Mathf.Clamp(currentViewers + UnityEngine.Random.Range(-15, 25), baseV / 2, baseV * 2);
        }

        // Таймер хайп-трейна
        if (hypeTrainRemaining > 0f)
        {
            hypeTrainRemaining = Mathf.Max(0f, hypeTrainRemaining - dt);
            if (hypeTrainRemaining <= 0f && hypeTrainBadgeObj != null)
            {
                hypeTrainBadgeObj.SetActive(false);
            }
        }

        // Генерация сообщений чата
        nextChatMessageTimer -= dt;
        if (nextChatMessageTimer <= 0f)
        {
            nextChatMessageTimer = hypeTrainRemaining > 0 ? UnityEngine.Random.Range(0.4f, 1.2f) : UnityEngine.Random.Range(1.2f, 3.5f);
            SpawnRandomChatMessage();
        }

        // Входящие донаты
        nextDonationTimer -= dt;
        if (nextDonationTimer <= 0f)
        {
            nextDonationTimer = hypeTrainRemaining > 0 ? UnityEngine.Random.Range(4f, 10f) : UnityEngine.Random.Range(12f, 28f);
            SpawnViewerDonation();
        }

        // Появление тролля для модерации
        trollSpamTimer -= dt;
        if (trollSpamTimer <= 0f && !isTrollActive)
        {
            trollSpamTimer = UnityEngine.Random.Range(35f, 75f);
            SpawnTrollIncident();
        }

        // Таймаут бана тролля
        if (isTrollActive)
        {
            trollTimeoutDuration -= dt;
            if (trollTimeoutDuration <= 0f)
            {
                isTrollActive = false;
                if (banTrollBtn != null) banTrollBtn.gameObject.SetActive(false);
            }
        }

        if (IsModalOpen) UpdateHUD();
    }

    public void ToggleStream()
    {
        isStreaming = !isStreaming;

        if (isStreaming)
        {
            streamDuration = 0f;
            sessionDonations = 0;
            currentViewers = 80 + (GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount * 120 : 0);
            hypeTrainRemaining = 0f;
            isTrollActive = false;

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            if (ClickJuice.Instance != null && toggleStreamBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("🔴 СТРИМ ЗАПУЩЕН! ЧАТ ОЖИЛ!", toggleStreamBtn.transform.position, new Color(1f, 0.25f, 0.35f), true);
            }
        }
        else
        {
            HapticFeedback.MediumImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();

            if (ClickJuice.Instance != null && toggleStreamBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🏁 СТРИМ ЗАВЕРШЁН!\nДонатов собрано: {NumberFormatter.Format(sessionDonations)} ₽", toggleStreamBtn.transform.position, new Color(0.2f, 0.85f, 1f), true);
            }
        }

        UpdateHUD();
    }

    private void SpawnRandomChatMessage()
    {
        string user = ViewerNames[UnityEngine.Random.Range(0, ViewerNames.Length)];
        string text = ChatPhrases[UnityEngine.Random.Range(0, ChatPhrases.Length)];
        AddChatMessageUI($"<color=#60A5FA>{user}:</color> {text}");
    }

    private void SpawnViewerDonation()
    {
        string user = ViewerNames[UnityEngine.Random.Range(0, ViewerNames.Length)];
        double donation = UnityEngine.Random.Range(250f, 2500f);
        if (hypeTrainRemaining > 0) donation *= 2.5;

        sessionDonations += donation;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(donation);
        }

        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();

        AddChatMessageUI($"<color=#FBBF24>💰 [ДОНАТ] {user} задонатил {NumberFormatter.Format(donation)} ₽: 'Удачи с кодом!'</color>");

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 +{NumberFormatter.Format(donation)} ₽ ДОНАТ СТРИМА!", transform.position, new Color(1f, 0.85f, 0.2f), false);
        }
    }

    private void SpawnTrollIncident()
    {
        isTrollActive = true;
        trollTimeoutDuration = 5.0f; // 5 секунд на реакцию

        string trollMsg = TrollPhrases[UnityEngine.Random.Range(0, TrollPhrases.Length)];
        AddChatMessageUI($"<color=#EF4444>⚠️ [ТРОЛЛЬ]: {trollMsg}</color>");

        if (banTrollBtn != null)
        {
            banTrollBtn.gameObject.SetActive(true);
            if (banTrollBtnText != null) banTrollBtnText.text = "🚨 ЗАБАНИТЬ ТРОЛЛЯ! [TIMEOUT 5s]";
        }

        HapticFeedback.WarningHaptic();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    public void OnBanTrollClicked()
    {
        if (!isTrollActive) return;

        isTrollActive = false;
        if (banTrollBtn != null) banTrollBtn.gameObject.SetActive(false);

        // Успешный бан запускает Hype Train на 30 секунд
        hypeTrainRemaining = 30f;
        if (hypeTrainBadgeObj != null) hypeTrainBadgeObj.SetActive(true);

        AddChatMessageUI("<color=#10B981>🛡️ Модератор забанил тролля! Чат в восторге: PogChamp HYPE TRAIN АКТИВИРОВАН!</color>");

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null && banTrollBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("🚂 HYPE TRAIN ЗАПУЩЕН!\nx2 к зрителям и донатам на 30 секунд!", banTrollBtn.transform.position, new Color(0.9f, 0.4f, 1f), true);
        }
    }

    private void AddChatMessageUI(string richText)
    {
        if (chatMessagesContainer == null) return;

        GameObject msgObj = new GameObject("ChatMsg", typeof(RectTransform), typeof(TextMeshProUGUI));
        msgObj.transform.SetParent(chatMessagesContainer, false);
        RectTransform rt = msgObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 24);

        TMP_Text t = msgObj.GetComponent<TextMeshProUGUI>();
        t.text = richText;
        t.fontSize = 11;
        t.color = Color.white;

        // Ограничиваем историю чата 25 сообщениями
        if (chatMessagesContainer.childCount > 25)
        {
            Destroy(chatMessagesContainer.GetChild(0).gameObject);
        }

        // Авто-скролл вниз
        if (chatScrollRect != null)
        {
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void UpdateHUD()
    {
        if (liveIndicatorText != null)
        {
            liveIndicatorText.text = isStreaming ? "🔴 LIVE ЭФИР" : "⚪ ОФФЛАЙН";
            liveIndicatorText.color = isStreaming ? new Color(1f, 0.25f, 0.3f) : new Color(0.6f, 0.6f, 0.7f);
        }

        if (viewerCountText != null)
        {
            viewerCountText.text = isStreaming ? $"👥 Зрители: <b>{currentViewers:N0}</b>" : "👥 Зрители: 0";
        }

        if (streamUptimeText != null)
        {
            int mins = (int)(streamDuration / 60);
            int secs = (int)(streamDuration % 60);
            streamUptimeText.text = $"⏱️ Время: <b>{mins:D2}:{secs:D2}</b>";
        }

        if (totalDonationsText != null)
        {
            totalDonationsText.text = $"💰 Донаты: <b>+{NumberFormatter.Format(sessionDonations)} ₽</b>";
        }

        if (toggleStreamBtnText != null)
        {
            toggleStreamBtnText.text = isStreaming ? "⏹️ ЗАВЕРШИТЬ СТРИМ" : "🔴 НАЧАТЬ ПРЯМОЙ ЭФИР";
        }
    }

    private void BindButtons()
    {
        if (openStreamBtn != null)
        {
            openStreamBtn.onClick.RemoveAllListeners();
            openStreamBtn.onClick.AddListener(OpenModal);
        }
        if (closeStreamBtn != null)
        {
            closeStreamBtn.onClick.RemoveAllListeners();
            closeStreamBtn.onClick.AddListener(CloseModal);
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

        if (toggleStreamBtn != null)
        {
            toggleStreamBtn.onClick.RemoveAllListeners();
            toggleStreamBtn.onClick.AddListener(ToggleStream);
        }

        if (banTrollBtn != null)
        {
            banTrollBtn.onClick.RemoveAllListeners();
            banTrollBtn.onClick.AddListener(OnBanTrollClicked);
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
        UpdateHUD();
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

        GameObject root = new GameObject("LiveDevStreamModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.6f, 0.3f, 1f, 0.5f);
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
        headerTxt.text = "🎥 СТРИМИНГ-КОМНАТА РАЗРАБОТЧИКА";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.7f, 0.45f, 1f);

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

        // Stream Metrics Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -56);
        infoRt.sizeDelta = new Vector2(-36, 75);
        infoPanel.GetComponent<Image>().color = new Color(0.11f, 0.14f, 0.22f, 0.95f);

        GameObject liveObj = new GameObject("LiveInd", typeof(RectTransform), typeof(TextMeshProUGUI));
        liveObj.transform.SetParent(infoPanel.transform, false);
        RectTransform lrt = liveObj.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0, 1);
        lrt.anchorMax = new Vector2(0.5f, 1);
        lrt.offsetMin = new Vector2(12, -26);
        lrt.offsetMax = new Vector2(0, -6);
        liveIndicatorText = liveObj.GetComponent<TextMeshProUGUI>();
        liveIndicatorText.fontSize = 14;
        liveIndicatorText.fontStyle = FontStyles.Bold;

        GameObject viewObj = new GameObject("Viewers", typeof(RectTransform), typeof(TextMeshProUGUI));
        viewObj.transform.SetParent(infoPanel.transform, false);
        RectTransform vrt = viewObj.GetComponent<RectTransform>();
        vrt.anchorMin = new Vector2(0.5f, 1);
        vrt.anchorMax = new Vector2(1, 1);
        vrt.offsetMin = new Vector2(0, -26);
        vrt.offsetMax = new Vector2(-12, -6);
        viewerCountText = viewObj.GetComponent<TextMeshProUGUI>();
        viewerCountText.fontSize = 13;
        viewerCountText.alignment = TextAlignmentOptions.Right;

        GameObject timeObj = new GameObject("Time", typeof(RectTransform), typeof(TextMeshProUGUI));
        timeObj.transform.SetParent(infoPanel.transform, false);
        RectTransform trt = timeObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 0);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.offsetMin = new Vector2(12, 6);
        trt.offsetMax = new Vector2(0, -6);
        streamUptimeText = timeObj.GetComponent<TextMeshProUGUI>();
        streamUptimeText.fontSize = 12;

        GameObject donObj = new GameObject("Donations", typeof(RectTransform), typeof(TextMeshProUGUI));
        donObj.transform.SetParent(infoPanel.transform, false);
        RectTransform drt = donObj.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.5f, 0);
        drt.anchorMax = new Vector2(1, 0.5f);
        drt.offsetMin = new Vector2(0, 6);
        drt.offsetMax = new Vector2(-12, -6);
        totalDonationsText = donObj.GetComponent<TextMeshProUGUI>();
        totalDonationsText.fontSize = 12;
        totalDonationsText.alignment = TextAlignmentOptions.Right;
        totalDonationsText.color = new Color(0.2f, 1f, 0.6f);

        // Hype Train Badge
        GameObject hypeObj = new GameObject("HypeTrainBadge", typeof(RectTransform), typeof(Image));
        hypeObj.transform.SetParent(cardObj.transform, false);
        hypeTrainBadgeObj = hypeObj;
        RectTransform hyrt = hypeObj.GetComponent<RectTransform>();
        hyrt.anchorMin = new Vector2(0, 1);
        hyrt.anchorMax = new Vector2(1, 1);
        hyrt.pivot = new Vector2(0.5f, 1);
        hyrt.anchoredPosition = new Vector2(0, -135);
        hyrt.sizeDelta = new Vector2(-36, 26);
        hypeObj.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.8f, 0.95f);

        GameObject hyTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        hyTxtObj.transform.SetParent(hypeObj.transform, false);
        RectTransform hytxtrt = hyTxtObj.GetComponent<RectTransform>();
        hytxtrt.anchorMin = Vector2.zero;
        hytxtrt.anchorMax = Vector2.one;
        hytxtrt.sizeDelta = Vector2.zero;
        TMP_Text hytxt = hyTxtObj.GetComponent<TextMeshProUGUI>();
        hytxt.text = "🚂 HYPE TRAIN LEVEL 1! Донаты и зрители удвоены!";
        hytxt.fontSize = 11;
        hytxt.fontStyle = FontStyles.Bold;
        hytxt.alignment = TextAlignmentOptions.Center;
        hytxt.color = Color.white;
        hypeObj.SetActive(false);

        // Chat Container (ScrollView)
        GameObject scrollObj = new GameObject("ChatScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 115);
        scrollRt.offsetMax = new Vector2(-18, -168);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.65f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        chatMessagesContainer = contentObj.transform;
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 0);
        contentRt.anchorMax = new Vector2(1, 0);
        contentRt.pivot = new Vector2(0.5f, 0);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 4;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        chatScrollRect = scrollObj.GetComponent<ScrollRect>();
        chatScrollRect.content = contentRt;
        chatScrollRect.horizontal = false;
        chatScrollRect.vertical = true;

        // Troll Ban Button (Emergency action)
        GameObject trollBtnObj = new GameObject("BanTrollBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        trollBtnObj.transform.SetParent(cardObj.transform, false);
        banTrollBtn = trollBtnObj.GetComponent<Button>();
        RectTransform trbRt = trollBtnObj.GetComponent<RectTransform>();
        trbRt.anchorMin = new Vector2(0, 0);
        trbRt.anchorMax = new Vector2(1, 0);
        trbRt.pivot = new Vector2(0.5f, 0);
        trbRt.anchoredPosition = new Vector2(0, 112);
        trbRt.sizeDelta = new Vector2(-40, 36);
        trollBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.2f, 0.2f);

        GameObject trbTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        trbTxtObj.transform.SetParent(trollBtnObj.transform, false);
        RectTransform trbtxtrt = trbTxtObj.GetComponent<RectTransform>();
        trbtxtrt.anchorMin = Vector2.zero;
        trbtxtrt.anchorMax = Vector2.one;
        trbtxtrt.sizeDelta = Vector2.zero;
        banTrollBtnText = trbTxtObj.GetComponent<TextMeshProUGUI>();
        banTrollBtnText.text = "🚨 ЗАБАНИТЬ ТРОЛЛЯ! [TIMEOUT]";
        banTrollBtnText.fontSize = 12;
        banTrollBtnText.fontStyle = FontStyles.Bold;
        banTrollBtnText.alignment = TextAlignmentOptions.Center;
        banTrollBtnText.color = Color.white;
        trollBtnObj.SetActive(false);

        // Toggle Stream Button
        GameObject toggleBtnObj = new GameObject("ToggleStreamBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        toggleBtnObj.transform.SetParent(cardObj.transform, false);
        toggleStreamBtn = toggleBtnObj.GetComponent<Button>();
        RectTransform togRt = toggleBtnObj.GetComponent<RectTransform>();
        togRt.anchorMin = new Vector2(0, 0);
        togRt.anchorMax = new Vector2(1, 0);
        togRt.pivot = new Vector2(0.5f, 0);
        togRt.anchoredPosition = new Vector2(0, 62);
        togRt.sizeDelta = new Vector2(-40, 44);
        toggleBtnObj.GetComponent<Image>().color = new Color(0.55f, 0.25f, 0.85f);

        GameObject togTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        togTxtObj.transform.SetParent(toggleBtnObj.transform, false);
        RectTransform togtxtrt = togTxtObj.GetComponent<RectTransform>();
        togtxtrt.anchorMin = Vector2.zero;
        togtxtrt.anchorMax = Vector2.one;
        togtxtrt.sizeDelta = Vector2.zero;
        toggleStreamBtnText = togTxtObj.GetComponent<TextMeshProUGUI>();
        toggleStreamBtnText.text = "🔴 НАЧАТЬ ПРЯМОЙ ЭФИР";
        toggleStreamBtnText.fontSize = 14;
        toggleStreamBtnText.fontStyle = FontStyles.Bold;
        toggleStreamBtnText.alignment = TextAlignmentOptions.Center;
        toggleStreamBtnText.color = Color.white;

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 14);
        closeBtnRt.sizeDelta = new Vector2(-40, 40);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeStreamBtn = closeBtnObj.GetComponent<Button>();

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
}
