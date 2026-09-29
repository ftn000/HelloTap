using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Режим «Кибер-Турнира» по скоростному кликингу (Speed Coding Cyber Arena):
/// - Соревнование с ботами-соперниками на скорость кодинга (20 секунд)
/// - Турнирная сетка плей-офф: 1/4 Финала -> Полуфинал -> Гранд-Финал
/// - Параллельные шкалы прогресса в реальном времени
/// - Крупные денежные призовые и Чемпионский Кубок Арены
/// </summary>
public class SpeedCodingArenaUI : MonoBehaviour
{
    private static SpeedCodingArenaUI instance;
    public static SpeedCodingArenaUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SpeedCodingArenaUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class TournamentMatch
    {
        public string stageName;
        public string opponentName;
        public string opponentAvatar;
        public double targetCode;
        public float opponentSpeedPerSec;
        public double prizeMoney;
        public bool isWon;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openArenaBtn;
    [SerializeField] private TMP_Text openArenaBtnText;
    [SerializeField] private Button closeArenaBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Турнирный матч")]
    [SerializeField] private GameObject matchViewRoot;
    [SerializeField] private TMP_Text matchStageText;
    [SerializeField] private TMP_Text matchTimerText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Image playerProgressBar;
    [SerializeField] private TMP_Text opponentNameText;
    [SerializeField] private Image opponentProgressBar;
    [SerializeField] private Button clickCodeBtn;
    [SerializeField] private TMP_Text matchStatusText;

    [Header("Турнирная сетка")]
    [SerializeField] private GameObject bracketViewRoot;
    [SerializeField] private Button startMatchBtn;
    [SerializeField] private TMP_Text currentRoundTitleText;
    [SerializeField] private TMP_Text prizePoolText;

    private readonly List<TournamentMatch> matches = new List<TournamentMatch>();
    private int currentMatchIndex = 0;
    private bool isMatchActive = false;
    private float matchRemainingTime = 20f;
    private double currentMatchTarget = 150.0;
    private double playerCodeInMatch = 0;
    private double opponentCodeInMatch = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool IsMatchActive => isMatchActive;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeMatches();

        if (modalRoot != null) modalRoot.SetActive(false);
        if (matchViewRoot != null) matchViewRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeMatches()
    {
        matches.Clear();

        matches.Add(new TournamentMatch
        {
            stageName = "1/4 ФИНАЛА",
            opponentName = "Junior Bot 'Vanya_404'",
            opponentAvatar = "🤖",
            targetCode = 120.0,
            opponentSpeedPerSec = 5.0f,
            prizeMoney = 25000.0,
            isWon = false
        });

        matches.Add(new TournamentMatch
        {
            stageName = "ПОЛУФИНАЛ",
            opponentName = "Middle Speedrunner 'Alex_FastKey'",
            opponentAvatar = "⚡",
            targetCode = 320.0,
            opponentSpeedPerSec = 14.5f,
            prizeMoney = 100000.0,
            isWon = false
        });

        matches.Add(new TournamentMatch
        {
            stageName = "ГРАНД-ФИНАЛ",
            opponentName = "Senior Champion 'DeepMatrix_9000'",
            opponentAvatar = "👑",
            targetCode = 700.0,
            opponentSpeedPerSec = 32.0f,
            prizeMoney = 400000.0,
            isWon = false
        });
    }

    private void Update()
    {
        if (!isMatchActive) return;

        float dt = Time.deltaTime;
        matchRemainingTime = Mathf.Max(0f, matchRemainingTime - dt);

        var match = matches[currentMatchIndex];

        // Противник печатает с постоянной скоростью
        opponentCodeInMatch += match.opponentSpeedPerSec * dt;

        // Обновление прогресс-баров
        if (playerProgressBar != null)
        {
            playerProgressBar.fillAmount = Mathf.Clamp01((float)(playerCodeInMatch / currentMatchTarget));
        }

        if (opponentProgressBar != null)
        {
            opponentProgressBar.fillAmount = Mathf.Clamp01((float)(opponentCodeInMatch / currentMatchTarget));
        }

        if (matchTimerText != null)
        {
            matchTimerText.text = $"⏱️ {matchRemainingTime:F1} сек";
        }

        // Проверка победы игрока
        if (playerCodeInMatch >= currentMatchTarget)
        {
            FinishMatch(true);
            return;
        }

        // Проверка победы соперника
        if (opponentCodeInMatch >= currentMatchTarget || matchRemainingTime <= 0f)
        {
            FinishMatch(false);
            return;
        }
    }

    public void StartCurrentMatch()
    {
        if (currentMatchIndex >= matches.Count) currentMatchIndex = 0;

        var m = matches[currentMatchIndex];
        currentMatchTarget = m.targetCode;
        playerCodeInMatch = 0;
        opponentCodeInMatch = 0;
        matchRemainingTime = 20.0f;
        isMatchActive = true;

        if (bracketViewRoot != null) bracketViewRoot.SetActive(false);
        if (matchViewRoot != null) matchViewRoot.SetActive(true);

        if (matchStageText != null) matchStageText.text = $"⚔️ {m.stageName}: {m.opponentName} ({m.opponentAvatar})";
        if (matchStatusText != null) matchStatusText.text = "КЛИКАЙТЕ ПО КНОПКЕ СО ВСЕЙ СКОРОСТЬЮ!";

        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    public void OnClickCodeInMatch()
    {
        if (!isMatchActive) return;

        // Каждый клик продвигает игрока
        double power = GameManager.Instance != null ? Math.Max(1.0, GameManager.Instance.GetCodePerClick() * 0.25) : 3.0;
        playerCodeInMatch += power;

        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTyping();

        if (ClickJuice.Instance != null && clickCodeBtn != null)
        {
            ClickJuice.Instance.SpawnTapParticle(clickCodeBtn.transform.position, false);
        }
    }

    private void FinishMatch(bool isPlayerWinner)
    {
        isMatchActive = false;
        var m = matches[currentMatchIndex];

        if (isPlayerWinner)
        {
            m.isWon = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(m.prizeMoney);
            }

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            bool isGrandFinal = currentMatchIndex == matches.Count - 1;
            string winMsg = isGrandFinal 
                ? $"🏆 ЧЕМПИОН КИБЕР-АРЕНЫ!\nГран-при: +{NumberFormatter.Format(m.prizeMoney)} ₽!" 
                : $"🎉 ПОБЕДА В {m.stageName}!\nПризовые: +{NumberFormatter.Format(m.prizeMoney)} ₽!";

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup(winMsg, transform.position, new Color(1f, 0.84f, 0.2f), true);
            }

            currentMatchIndex++;
            if (currentMatchIndex >= matches.Count)
            {
                currentMatchIndex = 0; // Сброс турнира на новый круг
                for (int i = 0; i < matches.Count; i++) matches[i].isWon = false;
            }
        }
        else
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"💀 ПОРАЖЕНИЕ В {m.stageName}!\nСоперник оказался быстрее. Попробуйте снова!", transform.position, new Color(1f, 0.35f, 0.35f), false);
            }
        }

        if (matchViewRoot != null) matchViewRoot.SetActive(false);
        if (bracketViewRoot != null) bracketViewRoot.SetActive(true);

        UpdateBracketUI();
    }

    private void UpdateBracketUI()
    {
        if (currentRoundTitleText != null && currentMatchIndex < matches.Count)
        {
            var m = matches[currentMatchIndex];
            currentRoundTitleText.text = $"Следующий матч: <b>{m.stageName}</b> против <b>{m.opponentName}</b>";
        }

        if (prizePoolText != null && currentMatchIndex < matches.Count)
        {
            var m = matches[currentMatchIndex];
            prizePoolText.text = $"🏆 Приз за победу: <b><color=#00FF88>+{NumberFormatter.Format(m.prizeMoney)} ₽</color></b> | Цель: {m.targetCode:F0} строк";
        }
    }

    private void BindButtons()
    {
        if (openArenaBtn != null)
        {
            openArenaBtn.onClick.RemoveAllListeners();
            openArenaBtn.onClick.AddListener(OpenModal);
        }
        if (closeArenaBtn != null)
        {
            closeArenaBtn.onClick.RemoveAllListeners();
            closeArenaBtn.onClick.AddListener(CloseModal);
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

        if (startMatchBtn != null)
        {
            startMatchBtn.onClick.RemoveAllListeners();
            startMatchBtn.onClick.AddListener(StartCurrentMatch);
        }

        if (clickCodeBtn != null)
        {
            clickCodeBtn.onClick.RemoveAllListeners();
            clickCodeBtn.onClick.AddListener(OnClickCodeInMatch);
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
        if (matchViewRoot != null) matchViewRoot.SetActive(false);
        if (bracketViewRoot != null) bracketViewRoot.SetActive(true);
        UpdateBracketUI();
        HapticFeedback.Vibrate(20);
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        isMatchActive = false;
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

        GameObject root = new GameObject("SpeedCodingArenaModal", typeof(RectTransform));
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
        cardRt.sizeDelta = new Vector2(490, 620);
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.3f, 0.3f, 0.6f);
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
        headerTxt.text = "⚡ КИБЕР-АРЕНА СПИДРАНА КОДА";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.4f, 0.4f);

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

        // View 1: Bracket
        GameObject brkRoot = new GameObject("BracketView", typeof(RectTransform));
        brkRoot.transform.SetParent(cardObj.transform, false);
        bracketViewRoot = brkRoot;
        RectTransform brkRt = brkRoot.GetComponent<RectTransform>();
        brkRt.anchorMin = Vector2.zero;
        brkRt.anchorMax = Vector2.one;
        brkRt.sizeDelta = Vector2.zero;

        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(brkRoot.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -70);
        infoRt.sizeDelta = new Vector2(-40, 120);
        infoPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

        GameObject rndObj = new GameObject("Round", typeof(RectTransform), typeof(TextMeshProUGUI));
        rndObj.transform.SetParent(infoPanel.transform, false);
        RectTransform rndRt = rndObj.GetComponent<RectTransform>();
        rndRt.anchorMin = new Vector2(0, 0.5f);
        rndRt.anchorMax = new Vector2(1, 1);
        rndRt.offsetMin = new Vector2(12, 0);
        rndRt.offsetMax = new Vector2(-12, -8);
        currentRoundTitleText = rndObj.GetComponent<TextMeshProUGUI>();
        currentRoundTitleText.fontSize = 14;

        GameObject przObj = new GameObject("Prize", typeof(RectTransform), typeof(TextMeshProUGUI));
        przObj.transform.SetParent(infoPanel.transform, false);
        RectTransform przRt = przObj.GetComponent<RectTransform>();
        przRt.anchorMin = new Vector2(0, 0);
        przRt.anchorMax = new Vector2(1, 0.5f);
        przRt.offsetMin = new Vector2(12, 8);
        przRt.offsetMax = new Vector2(-12, 0);
        prizePoolText = przObj.GetComponent<TextMeshProUGUI>();
        prizePoolText.fontSize = 13;

        GameObject startBtnObj = new GameObject("StartMatchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        startBtnObj.transform.SetParent(brkRoot.transform, false);
        startMatchBtn = startBtnObj.GetComponent<Button>();
        RectTransform stRt = startBtnObj.GetComponent<RectTransform>();
        stRt.anchorMin = new Vector2(0, 0);
        stRt.anchorMax = new Vector2(1, 0);
        stRt.pivot = new Vector2(0.5f, 0);
        stRt.anchoredPosition = new Vector2(0, 70);
        stRt.sizeDelta = new Vector2(-40, 50);
        startBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.25f);

        GameObject stTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        stTxtObj.transform.SetParent(startBtnObj.transform, false);
        RectTransform sttrt = stTxtObj.GetComponent<RectTransform>();
        sttrt.anchorMin = Vector2.zero;
        sttrt.anchorMax = Vector2.one;
        sttrt.sizeDelta = Vector2.zero;
        TMP_Text stxt = stTxtObj.GetComponent<TextMeshProUGUI>();
        stxt.text = "⚔️ НАЧАТЬ ПОЕДИНОК";
        stxt.fontSize = 15;
        stxt.fontStyle = FontStyles.Bold;
        stxt.alignment = TextAlignmentOptions.Center;
        stxt.color = Color.white;

        // View 2: Match Active View
        GameObject mtRoot = new GameObject("MatchView", typeof(RectTransform));
        mtRoot.transform.SetParent(cardObj.transform, false);
        matchViewRoot = mtRoot;
        RectTransform mtRt = mtRoot.GetComponent<RectTransform>();
        mtRt.anchorMin = Vector2.zero;
        mtRt.anchorMax = Vector2.one;
        mtRt.sizeDelta = Vector2.zero;

        GameObject mStageObj = new GameObject("StageTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        mStageObj.transform.SetParent(mtRoot.transform, false);
        RectTransform msRt = mStageObj.GetComponent<RectTransform>();
        msRt.anchorMin = new Vector2(0, 1);
        msRt.anchorMax = new Vector2(1, 1);
        msRt.pivot = new Vector2(0.5f, 1);
        msRt.anchoredPosition = new Vector2(0, -60);
        msRt.sizeDelta = new Vector2(-40, 30);
        matchStageText = mStageObj.GetComponent<TextMeshProUGUI>();
        matchStageText.fontSize = 14;
        matchStageText.fontStyle = FontStyles.Bold;
        matchStageText.alignment = TextAlignmentOptions.Center;

        GameObject tmrObj = new GameObject("Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
        tmrObj.transform.SetParent(mtRoot.transform, false);
        RectTransform tmrRt = tmrObj.GetComponent<RectTransform>();
        tmrRt.anchorMin = new Vector2(0, 1);
        tmrRt.anchorMax = new Vector2(1, 1);
        tmrRt.pivot = new Vector2(0.5f, 1);
        tmrRt.anchoredPosition = new Vector2(0, -90);
        tmrRt.sizeDelta = new Vector2(-40, 30);
        matchTimerText = tmrObj.GetComponent<TextMeshProUGUI>();
        matchTimerText.fontSize = 20;
        matchTimerText.fontStyle = FontStyles.Bold;
        matchTimerText.alignment = TextAlignmentOptions.Center;
        matchTimerText.color = new Color(1f, 0.85f, 0.2f);

        // Player Bar
        CreateRacerBar(mtRoot.transform, "PlayerBar", -135, new Color(0.2f, 0.7f, 1f), "ВЫ (РАЗРАБОТЧИК)", out playerNameText, out playerProgressBar);

        // Opponent Bar
        CreateRacerBar(mtRoot.transform, "OpponentBar", -190, new Color(1f, 0.35f, 0.35f), "СОПЕРНИК", out opponentNameText, out opponentProgressBar);

        // Big Click Button
        GameObject clkBtnObj = new GameObject("ClickCodeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        clkBtnObj.transform.SetParent(mtRoot.transform, false);
        clickCodeBtn = clkBtnObj.GetComponent<Button>();
        RectTransform clkRt = clkBtnObj.GetComponent<RectTransform>();
        clkRt.anchorMin = new Vector2(0, 0);
        clkRt.anchorMax = new Vector2(1, 0);
        clkRt.pivot = new Vector2(0.5f, 0);
        clkRt.anchoredPosition = new Vector2(0, 70);
        clkRt.sizeDelta = new Vector2(-40, 85);
        clkBtnObj.GetComponent<Image>().color = new Color(0.12f, 0.65f, 0.4f);

        GameObject clkTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        clkTxtObj.transform.SetParent(clkBtnObj.transform, false);
        RectTransform clktrt = clkTxtObj.GetComponent<RectTransform>();
        clktrt.anchorMin = Vector2.zero;
        clktrt.anchorMax = Vector2.one;
        clktrt.sizeDelta = Vector2.zero;
        TMP_Text clktxt = clkTxtObj.GetComponent<TextMeshProUGUI>();
        clktxt.text = "⚡ КЛИКАЙТЕ БЫСТРО!\nНАПИСАТЬ КОД";
        clktxt.fontSize = 18;
        clktxt.fontStyle = FontStyles.Bold;
        clktxt.alignment = TextAlignmentOptions.Center;
        clktxt.color = Color.white;

        mtRoot.SetActive(false);

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
        closeArenaBtn = closeBtnObj.GetComponent<Button>();

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

    private void CreateRacerBar(Transform parent, string name, float posY, Color col, string label, out TMP_Text nameTxt, out Image fillImg)
    {
        GameObject p = new GameObject(name, typeof(RectTransform));
        p.transform.SetParent(parent, false);
        RectTransform prt = p.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 1);
        prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(0.5f, 1);
        prt.anchoredPosition = new Vector2(0, posY);
        prt.sizeDelta = new Vector2(-40, 45);

        GameObject tObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        tObj.transform.SetParent(p.transform, false);
        RectTransform trt = tObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, 0);
        trt.sizeDelta = new Vector2(0, 18);
        nameTxt = tObj.GetComponent<TextMeshProUGUI>();
        nameTxt.text = label;
        nameTxt.fontSize = 11;
        nameTxt.fontStyle = FontStyles.Bold;
        nameTxt.color = col;

        GameObject bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(p.transform, false);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0, 0);
        bgrt.anchorMax = new Vector2(1, 0);
        bgrt.pivot = new Vector2(0.5f, 0);
        bgrt.anchoredPosition = new Vector2(0, 4);
        bgrt.sizeDelta = new Vector2(0, 16);
        bg.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.18f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bg.transform, false);
        RectTransform fillrt = fill.GetComponent<RectTransform>();
        fillrt.anchorMin = Vector2.zero;
        fillrt.anchorMax = Vector2.one;
        fillrt.sizeDelta = Vector2.zero;
        fillImg = fill.GetComponent<Image>();
        fillImg.color = col;
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
    }
}
