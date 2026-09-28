using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ассистент разработчика на базе AI (Dev Copilot Companion):
/// - Интерактивный виртуальный помощник программиста в углу экрана
/// - 3 скина помощника: AI Copilot 4.0, Cyber Clippy, Quantum Neural Brain
/// - Механика Tab-автодополнения (появляется при активном наборе кода)
/// - Автоматическая генерация оптимизационных патчей (Smart Refactoring)
/// - Прокачка уровня Copilot (Level 1–10)
/// </summary>
public class DevCopilotCompanionUI : MonoBehaviour
{
    private static DevCopilotCompanionUI instance;
    public static DevCopilotCompanionUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<DevCopilotCompanionUI>();
            return instance;
        }
    }

    [Header("Плавающий виджет помощника на экране")]
    [SerializeField] private GameObject companionWidgetRoot;
    [SerializeField] private TMP_Text companionAvatarText;
    [SerializeField] private TMP_Text companionSpeechBubbleText;
    [SerializeField] private GameObject speechBubbleObj;
    [SerializeField] private Button companionClickBtn;

    [Header("Кнопка всплывающего Tab-автодополнения")]
    [SerializeField] private GameObject tabPromptObj;
    [SerializeField] private Button tabPromptBtn;
    [SerializeField] private TMP_Text tabPromptText;

    [Header("Модальное окно настроек и прокачки Copilot")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text powerText;
    [SerializeField] private Button upgradeLevelBtn;
    [SerializeField] private TMP_Text upgradeLevelBtnText;
    [SerializeField] private Button switchPersonalityBtn;
    [SerializeField] private TMP_Text switchPersonalityBtnText;
    [SerializeField] private Button triggerRefactorBtn;
    [SerializeField] private TMP_Text refactorBtnText;

    private int copilotLevel = 1;
    private int currentPersonality = 0; // 0=AI Bot, 1=Clippy, 2=Quantum Brain
    private float refactorCooldownTimer = 0f;
    private int clickStreakCount = 0;
    private bool isTabPromptReady = false;

    private static readonly string[] PersonalityAvatars = new string[] { "🤖", "📎", "🧠" };
    private static readonly string[] PersonalityNames = new string[] { "AI Copilot 4.0", "Cyber Clippy", "Quantum Neural Brain" };
    private static readonly string[] CompanionQuotes = new string[] {
        "Код компилируется без ворнингов!",
        "Нажмите TAB для автодополнения функции!",
        "Вижу потенциал для микро-оптимизации.",
        "Архитектура чиста, как слеза джуна.",
        "Кофе + AI = 1000 строк кода в минуту!"
    };

    private const string PrefCopilotLevel = "Studio_CopilotLevel";
    private const string PrefPersonality = "Studio_CopilotPersonality";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CopilotLevel => copilotLevel;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadData();
        if (modalRoot != null) modalRoot.SetActive(false);
        if (tabPromptObj != null) tabPromptObj.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        SubscribeEvents();
        UpdateWidgetVisuals();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void LoadData()
    {
        copilotLevel = PlayerPrefs.GetInt(PrefCopilotLevel, 1);
        currentPersonality = PlayerPrefs.GetInt(PrefPersonality, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefCopilotLevel, copilotLevel);
        PlayerPrefs.SetInt(PrefPersonality, currentPersonality);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (refactorCooldownTimer > 0f)
        {
            refactorCooldownTimer = Mathf.Max(0f, refactorCooldownTimer - Time.deltaTime);
            if (refactorBtnText != null && IsModalOpen)
            {
                refactorBtnText.text = refactorCooldownTimer > 0f 
                    ? $"КД: {refactorCooldownTimer:F0}с" 
                    : "🔧 ЗАПУСТИТЬ РЕФАКТОРИНГ";
            }
        }

        // Поддержка нажатия клавиши Tab на клавиатуре
        if (isTabPromptReady && Input.GetKeyDown(KeyCode.Tab))
        {
            OnTabPromptClicked();
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        clickStreakCount++;

        // Каждые 18-25 кликов Copilot предлагает автокомплит
        if (!isTabPromptReady && clickStreakCount >= (26 - Math.Min(12, copilotLevel)))
        {
            clickStreakCount = 0;
            ShowTabPrompt();
        }
    }

    private void ShowTabPrompt()
    {
        isTabPromptReady = true;
        if (tabPromptObj != null)
        {
            tabPromptObj.SetActive(true);
        }

        double codeBonus = CalculateTabBonus();
        if (tabPromptText != null)
        {
            tabPromptText.text = $"⚡ [TAB] Автокомплит: <b>+{NumberFormatter.Format(codeBonus)}</b> строк!";
        }

        // Показываем реплику в облачке
        if (companionSpeechBubbleText != null && speechBubbleObj != null)
        {
            speechBubbleObj.SetActive(true);
            companionSpeechBubbleText.text = "Жми TAB!";
        }

        HapticFeedback.LightImpact();
    }

    private double CalculateTabBonus()
    {
        double cpc = GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() : 1.0;
        return cpc * (15.0 + copilotLevel * 8.0);
    }

    public void OnTabPromptClicked()
    {
        if (!isTabPromptReady) return;

        double bonus = CalculateTabBonus();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLinesOfCode(bonus);
        }

        isTabPromptReady = false;
        if (tabPromptObj != null) tabPromptObj.SetActive(false);
        if (speechBubbleObj != null) speechBubbleObj.SetActive(false);

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCrit();

        if (ClickJuice.Instance != null && tabPromptBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🤖 AI АВТОКОМПЛИТ!\n+{NumberFormatter.Format(bonus)} строк кода!", tabPromptBtn.transform.position, new Color(0f, 1f, 0.75f), true);
        }
    }

    public void OnCompanionClicked()
    {
        // При клике по маскоту: показывает случайную цитату или открывает меню прокачки
        if (speechBubbleObj != null && companionSpeechBubbleText != null)
        {
            speechBubbleObj.SetActive(true);
            companionSpeechBubbleText.text = CompanionQuotes[UnityEngine.Random.Range(0, CompanionQuotes.Length)];
            StopAllCoroutines();
            StartCoroutine(HideBubbleAfterDelay(3.5f));
        }

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRoboBeep();

        OpenModal();
    }

    private IEnumerator HideBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (speechBubbleObj != null && !isTabPromptReady) speechBubbleObj.SetActive(false);
    }

    public void UpgradeCopilotLevel()
    {
        double cost = 25000.0 * Math.Pow(1.6, copilotLevel - 1);
        if (GameManager.Instance == null || GameManager.Instance.Money < cost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств ({NumberFormatter.Format(cost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(cost);
        copilotLevel++;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null && upgradeLevelBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🚀 AI COPILOT УР. {copilotLevel}!\nБонус автодополнения увеличен!", upgradeLevelBtn.transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    public void CyclePersonality()
    {
        currentPersonality = (currentPersonality + 1) % PersonalityNames.Length;
        SaveData();
        UpdateWidgetVisuals();
        UpdateModalUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRoboBeep();

        if (ClickJuice.Instance != null && switchPersonalityBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Ассистент: {PersonalityNames[currentPersonality]} {PersonalityAvatars[currentPersonality]}", switchPersonalityBtn.transform.position, new Color(0.3f, 0.8f, 1f), true);
        }
    }

    public void TriggerSmartRefactoring()
    {
        if (refactorCooldownTimer > 0f) return;

        refactorCooldownTimer = 120f;
        double payout = 15000.0 * copilotLevel;
        double codeCleaned = 5000.0 * copilotLevel;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(payout);
            GameManager.Instance.AddLinesOfCode(codeCleaned);
        }

        if (ServerRackUI.Instance != null)
        {
            ServerRackUI.Instance.OnEmergencyCoolingClicked();
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null && triggerRefactorBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🔧 ПАТЧ ОПТИМИЗАЦИИ ВЫПУЩЕН!\n+{NumberFormatter.Format(payout)} ₽ за рефакторинг\n+Охлаждение серверов!", triggerRefactorBtn.transform.position, new Color(0.2f, 1f, 0.5f), true);
        }

        UpdateModalUI();
    }

    private void UpdateWidgetVisuals()
    {
        if (companionAvatarText != null)
        {
            companionAvatarText.text = PersonalityAvatars[currentPersonality];
        }
    }

    private void BindButtons()
    {
        if (companionClickBtn != null)
        {
            companionClickBtn.onClick.RemoveAllListeners();
            companionClickBtn.onClick.AddListener(OnCompanionClicked);
        }

        if (tabPromptBtn != null)
        {
            tabPromptBtn.onClick.RemoveAllListeners();
            tabPromptBtn.onClick.AddListener(OnTabPromptClicked);
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
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(CloseModal);
        }

        if (upgradeLevelBtn != null)
        {
            upgradeLevelBtn.onClick.RemoveAllListeners();
            upgradeLevelBtn.onClick.AddListener(UpgradeCopilotLevel);
        }

        if (switchPersonalityBtn != null)
        {
            switchPersonalityBtn.onClick.RemoveAllListeners();
            switchPersonalityBtn.onClick.AddListener(CyclePersonality);
        }

        if (triggerRefactorBtn != null)
        {
            triggerRefactorBtn.onClick.RemoveAllListeners();
            triggerRefactorBtn.onClick.AddListener(TriggerSmartRefactoring);
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
        if (levelText != null)
        {
            levelText.text = $"{PersonalityAvatars[currentPersonality]} <b>{PersonalityNames[currentPersonality]}</b> (Ур. {copilotLevel})";
        }

        if (powerText != null)
        {
            double tabBonus = CalculateTabBonus();
            powerText.text = $"Множитель Tab-автокомплита: <b>+{NumberFormatter.Format(tabBonus)} строк</b>\nЧастота подсказок: каждые {Math.Max(12, 26 - copilotLevel)} кликов";
        }

        double nextCost = 25000.0 * Math.Pow(1.6, copilotLevel - 1);
        if (upgradeLevelBtn != null)
        {
            bool canAfford = GameManager.Instance != null && GameManager.Instance.Money >= nextCost;
            upgradeLevelBtn.interactable = canAfford;
            var img = upgradeLevelBtn.GetComponent<Image>();
            if (img != null) img.color = canAfford ? new Color(0.12f, 0.65f, 0.4f) : new Color(0.2f, 0.25f, 0.3f);
        }

        if (upgradeLevelBtnText != null)
        {
            upgradeLevelBtnText.text = $"ПРОКАЧАТЬ УРОВЕНЬ ({NumberFormatter.Format(nextCost)} ₽)";
        }

        if (switchPersonalityBtnText != null)
        {
            switchPersonalityBtnText.text = $"СМЕНИТЬ АССИСТЕНТА: {PersonalityNames[(currentPersonality + 1) % PersonalityNames.Length]}";
        }

        if (triggerRefactorBtn != null)
        {
            triggerRefactorBtn.interactable = refactorCooldownTimer <= 0f;
        }
        if (refactorBtnText != null)
        {
            refactorBtnText.text = refactorCooldownTimer > 0f 
                ? $"КД: {refactorCooldownTimer:F0}с" 
                : "🔧 ЗАПУСТИТЬ РЕФАКТОРИНГ";
        }
    }

    private void EnsureUIExists()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. Плавающий маскот на экране (если ещё не создан)
        if (companionWidgetRoot == null)
        {
            GameObject widget = new GameObject("DevCopilotWidget", typeof(RectTransform), typeof(Image), typeof(Button));
            widget.transform.SetParent(canvas.transform, false);
            companionWidgetRoot = widget;
            companionClickBtn = widget.GetComponent<Button>();

            RectTransform wrt = widget.GetComponent<RectTransform>();
            wrt.anchorMin = new Vector2(1, 0.5f);
            wrt.anchorMax = new Vector2(1, 0.5f);
            wrt.pivot = new Vector2(1, 0.5f);
            wrt.anchoredPosition = new Vector2(-15, 60);
            wrt.sizeDelta = new Vector2(55, 55);
            widget.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.95f);

            GameObject avObj = new GameObject("Avatar", typeof(RectTransform), typeof(TextMeshProUGUI));
            avObj.transform.SetParent(widget.transform, false);
            RectTransform avrt = avObj.GetComponent<RectTransform>();
            avrt.anchorMin = Vector2.zero;
            avrt.anchorMax = Vector2.one;
            avrt.sizeDelta = Vector2.zero;
            companionAvatarText = avObj.GetComponent<TextMeshProUGUI>();
            companionAvatarText.text = "🤖";
            companionAvatarText.fontSize = 28;
            companionAvatarText.alignment = TextAlignmentOptions.Center;

            // Speech Bubble
            GameObject bubble = new GameObject("SpeechBubble", typeof(RectTransform), typeof(Image));
            bubble.transform.SetParent(widget.transform, false);
            speechBubbleObj = bubble;
            RectTransform brt = bubble.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 1);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(1, 0);
            brt.anchoredPosition = new Vector2(-6, 6);
            brt.sizeDelta = new Vector2(160, 44);
            bubble.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 0.95f);

            GameObject btObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btObj.transform.SetParent(bubble.transform, false);
            RectTransform btrt = btObj.GetComponent<RectTransform>();
            btrt.anchorMin = Vector2.zero;
            btrt.anchorMax = Vector2.one;
            btrt.offsetMin = new Vector2(6, 4);
            btrt.offsetMax = new Vector2(-6, -4);
            companionSpeechBubbleText = btObj.GetComponent<TextMeshProUGUI>();
            companionSpeechBubbleText.fontSize = 11;
            companionSpeechBubbleText.alignment = TextAlignmentOptions.Center;
            companionSpeechBubbleText.color = Color.white;
            companionSpeechBubbleText.text = "AI готов!";

            bubble.SetActive(false);
        }

        // 2. Всплывающая плашка TAB
        if (tabPromptObj == null)
        {
            GameObject tabObj = new GameObject("TabPromptBadge", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            tabObj.transform.SetParent(canvas.transform, false);
            tabPromptObj = tabObj;
            tabPromptBtn = tabObj.GetComponent<Button>();

            RectTransform trt = tabObj.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(0, -60);
            trt.sizeDelta = new Vector2(320, 44);
            tabObj.GetComponent<Image>().color = new Color(0.08f, 0.45f, 0.35f, 0.98f);
            var o = tabObj.GetComponent<Outline>();
            o.effectColor = new Color(0.2f, 1f, 0.6f);
            o.effectDistance = new Vector2(2, -2);

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(tabObj.transform, false);
            RectTransform txrt = txtObj.GetComponent<RectTransform>();
            txrt.anchorMin = Vector2.zero;
            txrt.anchorMax = Vector2.one;
            txrt.sizeDelta = Vector2.zero;
            tabPromptText = txtObj.GetComponent<TextMeshProUGUI>();
            tabPromptText.fontSize = 13;
            tabPromptText.fontStyle = FontStyles.Bold;
            tabPromptText.alignment = TextAlignmentOptions.Center;
            tabPromptText.color = Color.white;

            tabObj.SetActive(false);
        }

        // 3. Модальное окно настроек
        if (modalRoot != null) return;

        GameObject root = new GameObject("DevCopilotModal", typeof(RectTransform));
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
        cardRt.sizeDelta = new Vector2(460, 520);
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.14f, 0.98f);
        var cardOutline = cardObj.GetComponent<Outline>();
        cardOutline.effectColor = new Color(0f, 0.85f, 0.65f, 0.5f);
        cardOutline.effectDistance = new Vector2(2, -2);

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
        headerTxt.text = "🤖 AI COPILOT COMPANION";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 1f, 0.75f);

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

        // Info Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -60);
        infoRt.sizeDelta = new Vector2(-40, 95);
        infoPanel.GetComponent<Image>().color = new Color(0.11f, 0.14f, 0.22f, 0.95f);

        GameObject lvlObj = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlObj.transform.SetParent(infoPanel.transform, false);
        RectTransform lvlRt = lvlObj.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0, 0.5f);
        lvlRt.anchorMax = new Vector2(1, 1);
        lvlRt.offsetMin = new Vector2(12, 0);
        lvlRt.offsetMax = new Vector2(-12, -6);
        levelText = lvlObj.GetComponent<TextMeshProUGUI>();
        levelText.fontSize = 15;
        levelText.fontStyle = FontStyles.Bold;

        GameObject powObj = new GameObject("Power", typeof(RectTransform), typeof(TextMeshProUGUI));
        powObj.transform.SetParent(infoPanel.transform, false);
        RectTransform powRt = powObj.GetComponent<RectTransform>();
        powRt.anchorMin = new Vector2(0, 0);
        powRt.anchorMax = new Vector2(1, 0.5f);
        powRt.offsetMin = new Vector2(12, 6);
        powRt.offsetMax = new Vector2(-12, 0);
        powerText = powObj.GetComponent<TextMeshProUGUI>();
        powerText.fontSize = 12;
        powerText.color = new Color(0.75f, 0.85f, 0.95f);

        // Buttons
        upgradeLevelBtn = CreateModalActionButton(cardObj.transform, "ПРОКАЧАТЬ УРОВЕНЬ", -175, new Color(0.12f, 0.6f, 0.45f), out upgradeLevelBtnText);
        switchPersonalityBtn = CreateModalActionButton(cardObj.transform, "СМЕНИТЬ АССИСТЕНТА", -240, new Color(0.15f, 0.45f, 0.7f), out switchPersonalityBtnText);
        triggerRefactorBtn = CreateModalActionButton(cardObj.transform, "🔧 ЗАПУСТИТЬ РЕФАКТОРИНГ", -305, new Color(0.75f, 0.45f, 0.1f), out refactorBtnText);

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
        closeBtn = closeBtnObj.GetComponent<Button>();

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

    private Button CreateModalActionButton(Transform parent, string initialText, float posY, Color col, out TMP_Text textComp)
    {
        GameObject b = new GameObject("ActBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(380, 50);
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = new Vector2(-16, 0);
        textComp = to.GetComponent<TextMeshProUGUI>();
        textComp.text = initialText;
        textComp.fontSize = 12;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        return b.GetComponent<Button>();
    }
}
