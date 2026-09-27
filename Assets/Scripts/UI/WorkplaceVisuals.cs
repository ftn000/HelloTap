using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет атмосферой и визуальной эволюцией рабочего места инди-разработчика:
/// - Появление предметов на столе по мере покупок (мышь, кот, кофе, энергетик, 2-й экран доки)
/// - Живой терминал с кодом на мониторе
/// - Прогресс-бар текущего проекта с кнопкой быстрого релиза
/// - Неоновая шкала комбо "В Потоке" (x1.0 -> x1.5 -> x2.0 -> x3.0)
/// - Мини-ивент "Охота на баги" прямо на экране монитора
/// </summary>
public class WorkplaceVisuals : MonoBehaviour
{
    [Header("Монитор и Код")]
    [SerializeField] private TMP_Text monitorCodeText;
    [SerializeField] private Graphic monitorScreenGlow;
    [SerializeField] private TMP_Text secondMonitorText;
    [SerializeField] private GameObject secondMonitorPanel;
    [SerializeField] private int maxVisibleLines = 12;

    [Header("Клавиатура")]
    [SerializeField] private Transform keyboardTransform;
    [SerializeField] private Graphic keyboardGlowImage;
    [SerializeField] private Image keyboardBaseImage;
    [SerializeField] private Sprite sprKeyboardDefault;
    [SerializeField] private Sprite sprKeyboardRetro;
    [SerializeField] private Sprite sprKeyboardNeon;
    [SerializeField] private Button keyboardStyleButton;
    [SerializeField] private TMP_Text keyboardStyleText;
    [SerializeField] private Button keyboardSwitchButton;
    [SerializeField] private TMP_Text keyboardSwitchText;
    [SerializeField] private Color keyGlowNormal = new Color(0, 0.89f, 1f, 0f);
    [SerializeField] private Color keyGlowActive = new Color(0, 1f, 0.55f, 0.35f);
    [SerializeField] private Graphic keyGlowW;
    [SerializeField] private Graphic keyGlowA;
    [SerializeField] private Graphic keyGlowS;
    [SerializeField] private Graphic keyGlowD;
    [SerializeField] private Graphic keyGlowSpace;
    [SerializeField] private Graphic keyGlowEsc;

    [Header("Предметы на столе (Визуальная прогрессия)")]
    [SerializeField] private Transform mouseTransform;
    [SerializeField] private Graphic mouseGlowGraphic;
    [SerializeField] private Button mouseSwitchButton;
    [SerializeField] private TMP_Text mouseSwitchText;
    [SerializeField] private Transform coffeeMugTransform;
    [SerializeField] private Transform energyCanTransform;
    [SerializeField] private Transform catTransform;
    [SerializeField] private Image catImage;
    [SerializeField] private Sprite catSleepingSprite;
    [SerializeField] private Sprite catAwakeSprite;
    [SerializeField] private Sprite catStretchSprite;
    [SerializeField] private Sprite sprCorgiSleep;
    [SerializeField] private Sprite sprCorgiAwake;
    [SerializeField] private Sprite sprRoboSleep;
    [SerializeField] private Sprite sprRoboAwake;
    [SerializeField] private Button petSelectorButton;
    [SerializeField] private TMP_Text petSelectorText;
    [SerializeField] private Image catAccessoryImage;
    [SerializeField] private Sprite catGlassesSprite;
    [SerializeField] private Sprite catBowtieSprite;
    [SerializeField] private Button catAccessoryButton;
    [SerializeField] private RectTransform catHeartEmote;

    [Header("Освещение, День/Ночь и Лампа")]
    [SerializeField] private GameObject deskLampObj;
    [SerializeField] private Button deskLampButton;
    [SerializeField] private GameObject lampConeObj;
    [SerializeField] private Graphic ambientOverlayGraphic;
    [SerializeField] private Button timeOfDayButton;
    [SerializeField] private TMP_Text timeOfDayText;

    [Header("Обои и Окружение Комнаты")]
    [SerializeField] private Image roomWallpaperImage;
    [SerializeField] private Sprite sprWallpaperCozy;
    [SerializeField] private Sprite sprWallpaperCyberpunk;
    [SerializeField] private Sprite sprWallpaperMinimal;
    [SerializeField] private Button roomThemeButton;
    [SerializeField] private TMP_Text roomThemeText;

    [Header("Скины коврика рабочей зоны")]
    [SerializeField] private Image deskMatImage;
    [SerializeField] private Sprite sprDeskMatDefault;
    [SerializeField] private Sprite sprDeskMatFelt;
    [SerializeField] private Sprite sprDeskMatCyber;
    [SerializeField] private Sprite sprDeskMatBlueprint;
    [SerializeField] private Sprite sprDeskMatRGB;
    [SerializeField] private Button deskMatSkinButton;
    [SerializeField] private TMP_Text deskMatSkinText;

    [Header("Интерактивные напитки и мышь")]
    [SerializeField] private Button mouseButton;
    [SerializeField] private Button coffeeButton;
    [SerializeField] private Button energyButton;

    [Header("Стикеры на мониторе (Достижения)")]
    [SerializeField] private GameObject stickerUnity;
    [SerializeField] private GameObject stickerCSharp;
    [SerializeField] private GameObject stickerGit;
    [SerializeField] private GameObject stickerWorks;
    [SerializeField] private GameObject stickerCrown;

    [Header("Хоткеи и Подсказки IDE")]
    [SerializeField] private GameObject hotkeyBadgeObj;
    [SerializeField] private TMP_Text hotkeyBadgeText;
    [SerializeField] private Button hotkeyBadgeButton;

    [Header("Партиклы пара и пузырьков")]
    [SerializeField] private RectTransform[] coffeeSteamWisps;
    [SerializeField] private RectTransform[] energyFizzBubbles;

    [Header("Прогресс-бар проекта и Шкала В Потоке")]
    [SerializeField] private Image projectProgressFill;
    [SerializeField] private TMP_Text projectProgressText;
    [SerializeField] private Button quickReleaseButton;
    [SerializeField] private Image comboBarFill;
    [SerializeField] private TMP_Text comboBarText;

    [Header("Мини-ивент: Охота на Баги")]
    [SerializeField] private Button bugAlertButton;
    [SerializeField] private TMP_Text bugAlertText;

    private readonly List<string> terminalHistory = new List<string>();
    private float cursorTimer = 0f;
    private bool cursorVisible = true;
    private Coroutine glowCoroutine;

    // Плавный скролл терминала
    private float currentScrollY = 0f;
    private float targetScrollY = 0f;
    private RectTransform terminalRt;
    private const float TerminalLineHeight = 17.5f;
    private const float ViewportVisibleHeight = 230f;

    // Состояние охоты на баги
    private float bugSpawnTimer = 18f;
    private float bugActiveTimer = 0f;
    private int bugHp = 0;
    private const int MaxBugHp = 5;
    private const float BugLifetime = 8.5f;

    // Партиклы пара и пузырьков
    private Vector2[] steamBasePos;
    private Graphic[] steamGraphics;
    private Vector2[] fizzBasePos;
    private Graphic[] fizzGraphics;
    private float coffeeJostleBoost = 0f;
    private float energyJostleBoost = 0f;

    // Анимация кота
    private Coroutine catWiggleCoroutine;

    // Отслеживание открытых предметов для анимации появления
    private bool wasMouseUnlocked = false;
    private bool wasCatUnlocked = false;
    private bool wasCoffeeUnlocked = false;
    private bool wasMonitor2Unlocked = false;

    private static readonly string[] CodeSnippets = new string[]
    {
        "<color=#569CD6>public class</color> <color=#4EC9B0>GameManager</color> : <color=#4EC9B0>MonoBehaviour</color>",
        "  [<color=#4EC9B0>SerializeField</color>] <color=#569CD6>private double</color> codeLines;",
        "  <color=#569CD6>void</color> <color=#DCDCAA>Update</color>() => <color=#DCDCAA>ProcessIncome</color>();",
        "  <color=#569CD6>if</color> (isCrit) <color=#DCDCAA>PlayPunchEffect</color>();",
        "  <color=#CE9178>\"Build succeeded in 0.42s\"</color>",
        "  <color=#6A9955>// TODO: optimize draw calls</color>",
        "  <color=#4EC9B0>Instantiate</color>(coinPrefab, mousePos);",
        "  <color=#569CD6>var</color> profit = income * <color=#B5CEA8>2.0</color>;",
        "  <color=#4EC9B0>PlayerPrefs</color>.<color=#DCDCAA>Save</color>();",
        "  <color=#CE9178>\"Deploying to Yandex Games...\"</color>",
        "  <color=#569CD6>yield return new</color> <color=#4EC9B0>WaitForSeconds</color>(<color=#B5CEA8>0.1f</color>);",
        "  <color=#DCDCAA>Debug</color>.<color=#DCDCAA>Log</color>(<color=#CE9178>\"Release published!\"</color>);",
        "  <color=#569CD6>float</color> fps = <color=#B5CEA8>1.0f</color> / <color=#4EC9B0>Time</color>.deltaTime;",
        "  <color=#569CD6>git</color> commit -m <color=#CE9178>\"feat: add juice\"</color>"
    };

    private void Awake()
    {
        AutoBindMissingReferences();

        terminalHistory.Add("<color=#6A9955>// HelloTap Dev Terminal v1.3.0</color>");
        terminalHistory.Add("<color=#569CD6>using</color> UnityEngine;");
        terminalHistory.Add("<color=#569CD6>using</color> System.Collections;");
        terminalHistory.Add("");
        UpdateTerminalDisplay();

        if (bugAlertButton != null)
        {
            bugAlertButton.gameObject.SetActive(false);
            bugAlertButton.onClick.RemoveAllListeners();
            bugAlertButton.onClick.AddListener(OnBugClicked);
        }

        if (quickReleaseButton != null)
        {
            quickReleaseButton.onClick.RemoveAllListeners();
            quickReleaseButton.onClick.AddListener(OnQuickReleaseClicked);
        }
    }

    private void AutoBindMissingReferences()
    {
        if (mouseTransform == null)
        {
            Transform found = transform.Find("DeskMat/GamingMouse");
            if (found != null) mouseTransform = found;
        }
        if (mouseTransform == null)
        {
            Transform found = transform.Find("DeskMat/GamingMouse");
            if (found != null) mouseTransform = found;
        }
        if (mouseGlowGraphic == null)
        {
            mouseGlowGraphic = transform.Find("DeskMat/GamingMouse/MouseGlow")?.GetComponent<Graphic>();
        }
        if (coffeeMugTransform == null)
        {
            Transform found = transform.Find("DeskMat/CoffeeMug");
            if (found != null) coffeeMugTransform = found;
        }
        if (energyCanTransform == null)
        {
            Transform found = transform.Find("DeskMat/EnergyCan");
            if (found != null) energyCanTransform = found;
        }
        if (catTransform == null)
        {
            Transform found = transform.Find("DeskMat/CatMascot");
            if (found != null) catTransform = found;
        }

        if (keyGlowW == null) keyGlowW = transform.Find("DeskMat/KeyboardGroup/KeyGlow_W")?.GetComponent<Graphic>();
        if (keyGlowA == null) keyGlowA = transform.Find("DeskMat/KeyboardGroup/KeyGlow_A")?.GetComponent<Graphic>();
        if (keyGlowS == null) keyGlowS = transform.Find("DeskMat/KeyboardGroup/KeyGlow_S")?.GetComponent<Graphic>();
        if (keyGlowD == null) keyGlowD = transform.Find("DeskMat/KeyboardGroup/KeyGlow_D")?.GetComponent<Graphic>();
        if (keyGlowSpace == null) keyGlowSpace = transform.Find("DeskMat/KeyboardGroup/KeyGlow_Space")?.GetComponent<Graphic>();
        if (keyGlowEsc == null) keyGlowEsc = transform.Find("DeskMat/KeyboardGroup/KeyGlow_Esc")?.GetComponent<Graphic>();

        if (monitorScreenGlow == null)
        {
            monitorScreenGlow = transform.Find("DeskMat/MonitorFrame/MonitorScreen/MonitorScreenGlow")?.GetComponent<Graphic>();
        }

        if (coffeeSteamWisps == null || coffeeSteamWisps.Length == 0)
        {
            Transform steamGroup = transform.Find("DeskMat/CoffeeMug/CoffeeSteamGroup");
            if (steamGroup != null)
            {
                var list = new List<RectTransform>();
                for (int i = 0; i < steamGroup.childCount; i++)
                {
                    if (steamGroup.GetChild(i) is RectTransform rt) list.Add(rt);
                }
                coffeeSteamWisps = list.ToArray();
            }
        }

        if (energyFizzBubbles == null || energyFizzBubbles.Length == 0)
        {
            Transform fizzGroup = transform.Find("DeskMat/EnergyCan/EnergyFizzGroup");
            if (fizzGroup != null)
            {
                var list = new List<RectTransform>();
                for (int i = 0; i < fizzGroup.childCount; i++)
                {
                    if (fizzGroup.GetChild(i) is RectTransform rt) list.Add(rt);
                }
                energyFizzBubbles = list.ToArray();
            }
        }

        if (keyboardBaseImage == null && keyboardTransform != null)
        {
            keyboardBaseImage = keyboardTransform.Find("KeyboardImage")?.GetComponent<Image>();
        }
        if (keyboardStyleButton == null && keyboardTransform != null)
        {
            keyboardStyleButton = keyboardTransform.Find("KeyboardCustomBar/StyleBtn")?.GetComponent<Button>();
        }
        if (keyboardStyleText == null && keyboardStyleButton != null)
        {
            keyboardStyleText = keyboardStyleButton.GetComponentInChildren<TMP_Text>();
        }
        if (keyboardSwitchButton == null && keyboardTransform != null)
        {
            keyboardSwitchButton = keyboardTransform.Find("KeyboardCustomBar/SwitchBtn")?.GetComponent<Button>();
        }
        if (keyboardSwitchText == null && keyboardSwitchButton != null)
        {
            keyboardSwitchText = keyboardSwitchButton.GetComponentInChildren<TMP_Text>();
        }
        if (roomWallpaperImage == null)
        {
            roomWallpaperImage = transform.root.Find("Canvas/MobileFrame/RoomWallpaper")?.GetComponent<Image>();
        }
        if (roomThemeButton == null)
        {
            roomThemeButton = transform.Find("DeskMat/RoomThemeBtn")?.GetComponent<Button>();
        }
        if (roomThemeText == null && roomThemeButton != null)
        {
            roomThemeText = roomThemeButton.GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Start()
    {
        SubscribeEvents();
        if (monitorCodeText != null)
        {
            terminalRt = monitorCodeText.rectTransform;
        }
        InitSteamAndFizz();
        InitCatInteraction();
        InitCatCustomization();
        InitKeyboardCustomization();
        InitMouseSwitchCustomization();
        InitRoomThemeCustomization();
        InitPetCompanionCustomization();
        InitDeskMatCustomization();
        InitLighting();
        InitDrinksInteraction();
        InitMouseInteraction();
        InitStickersInteraction();
        InitHotkeyBadge();
        RefreshDeskUnlockables(false);
        RefreshProgressAndComboUI();
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            GameManager.Instance.OnCurrenciesChanged -= HandleCurrenciesChanged;
            GameManager.Instance.OnCurrenciesChanged += HandleCurrenciesChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnCurrenciesChanged -= HandleCurrenciesChanged;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Мигание курсора в терминале
        cursorTimer += dt;
        if (cursorTimer >= 0.5f)
        {
            cursorTimer = 0f;
            cursorVisible = !cursorVisible;
            UpdateTerminalDisplay();
        }

        // 2. Анимация кота (дыхание и покачивание хвостом)
        UpdateCatIdle(dt);

        // 3. Анимация поднимающегося пара над кружкой кофе
        UpdateCoffeeSteam(dt);

        // 4. Анимация шипучих пузырьков энергетика
        UpdateEnergyFizz(dt);

        // 5. Плавный скролл истории кода
        UpdateTerminalScroll(dt);

        // 6. RGB подсветка и спектральная волна мыши
        UpdateMouseRgb(dt);

        // 7. Обновление шкалы комбо "В Потоке"
        UpdateComboVisuals();

        // 8. Таймер спавна и жизни Бага на мониторе
        UpdateBugHunt(dt);

        // 9. RGB радужный перелив клавиатуры при комбо x3.0
        UpdateKeyboardRgb(dt);

        // 10. Обновление хоткей-подсказок в IDE
        UpdateHotkeyHints(dt);

        // 11. Освещение, день/ночь и настольная лампа
        UpdateLighting(dt);
    }

    #region Визуальная эволюция стола

    private void HandleUpgradePurchased(UpgradeItem item)
    {
        RefreshDeskUnlockables(true);
        RefreshProgressAndComboUI();
    }

    private void HandleCurrenciesChanged()
    {
        RefreshDeskUnlockables(false);
        RefreshProgressAndComboUI();
    }

    public void RefreshDeskUnlockables(bool animateNew)
    {
        if (GameManager.Instance == null) return;

        bool hasMouse = GameManager.Instance.GetUpgradeLevel("hw_mouse") > 0;
        bool hasCat = true; // Кот-маскот всегда мило спит на столе
        bool hasCoffee = true; // Кружка с паром всегда на столе
        bool hasMonitor2 = GameManager.Instance.GetUpgradeLevel("hw_monitor2") > 0;
        bool hasEnergy = true; // Энергетик с пузырьками на столе

        if (mouseTransform != null)
        {
            mouseTransform.gameObject.SetActive(hasMouse);
            if (animateNew && hasMouse && !wasMouseUnlocked) StartCoroutine(PopInRoutine(mouseTransform));
        }
        wasMouseUnlocked = hasMouse;

        if (catTransform != null)
        {
            catTransform.gameObject.SetActive(hasCat);
            if (animateNew && hasCat && !wasCatUnlocked) StartCoroutine(PopInRoutine(catTransform));
        }
        wasCatUnlocked = hasCat;

        if (coffeeMugTransform != null)
        {
            coffeeMugTransform.gameObject.SetActive(hasCoffee);
            if (animateNew && hasCoffee && !wasCoffeeUnlocked) StartCoroutine(PopInRoutine(coffeeMugTransform));
        }
        wasCoffeeUnlocked = hasCoffee;

        if (energyCanTransform != null)
        {
            energyCanTransform.gameObject.SetActive(hasEnergy);
        }

        if (secondMonitorPanel != null)
        {
            secondMonitorPanel.SetActive(hasMonitor2);
            if (animateNew && hasMonitor2 && !wasMonitor2Unlocked) StartCoroutine(PopInRoutine(secondMonitorPanel.transform));
            if (hasMonitor2 && secondMonitorText != null)
            {
                int rtxLvl = GameManager.Instance.GetUpgradeLevel("hw_pc_rig");
                int gptLvl = GameManager.Instance.GetUpgradeLevel("staff_gpt");
                secondMonitorText.text =
                    "<color=#4EC9B0>[DEV DOCS & METRICS]</color>\n" +
                    $"GPU: {(rtxLvl > 0 ? $"RTX ON (Lv.{rtxLvl})" : "Integrated")}\n" +
                    $"AI Copilot: {(gptLvl > 0 ? $"v{gptLvl}.0 Active" : "Offline")}\n" +
                    $"Багов пофикшено: {GameManager.Instance.BugsFixedCount}\n" +
                    $"Бонус ачивок: +{(GameManager.Instance.GetAchievementMultiplier() - 1.0) * 100:F0}%";
            }
        }
        wasMonitor2Unlocked = hasMonitor2;

        UpdateStickersVisibility(animateNew);
    }

    private IEnumerator PopInRoutine(Transform target)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float s = Mathf.Lerp(0.2f, 1.0f, t) + Mathf.Sin(t * Mathf.PI) * 0.2f;
            target.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    #endregion

    #region Прогресс-бар проекта и Шкала Комбо

    private void RefreshProgressAndComboUI()
    {
        if (GameManager.Instance == null) return;

        if (projectProgressFill != null && projectProgressText != null)
        {
            GameProjectData nextProj = GameManager.Instance.GetNextTargetProject();
            if (nextProj != null)
            {
                double code = GameManager.Instance.CodeLines;
                double req = nextProj.RequiredCodeLines;
                float progress = nextProj.GetProgress(code);
                projectProgressFill.fillAmount = progress;

                bool ready = code >= req;
                if (ready)
                {
                    projectProgressFill.color = new Color(0.15f, 0.90f, 0.45f, 0.95f);
                    projectProgressText.text = $"ГОТОВО К РЕЛИЗУ: {nextProj.Title}! [НАЖМИ — +{NumberFormatter.Format(nextProj.RewardMoney)} руб.]";
                    projectProgressText.color = new Color(1f, 0.95f, 0.5f, 1f);
                }
                else
                {
                    projectProgressFill.color = new Color(0.18f, 0.55f, 0.95f, 0.85f);
                    projectProgressText.text = $"Цель: {nextProj.Title} ({NumberFormatter.Format(code)} / {NumberFormatter.Format(req)} строк — {progress * 100:F0}%)";
                    projectProgressText.color = Color.white;
                }

                if (quickReleaseButton != null)
                {
                    quickReleaseButton.interactable = ready;
                }
            }
            else
            {
                projectProgressFill.fillAmount = 1f;
                projectProgressFill.color = new Color(0.85f, 0.65f, 0.15f, 0.9f);
                projectProgressText.text = "ВСЕ ПРОЕКТЫ РЕЛИЗНУТЫ! Выходите на IPO во вкладке ПРОЕКТЫ";
                if (quickReleaseButton != null) quickReleaseButton.interactable = false;
            }
        }
    }

    private void OnQuickReleaseClicked()
    {
        if (GameManager.Instance == null) return;
        GameProjectData nextProj = GameManager.Instance.GetNextTargetProject();
        if (nextProj != null && GameManager.Instance.CodeLines >= nextProj.RequiredCodeLines)
        {
            ProjectBuildMiniGame.Instance.StartBuildMiniGame(nextProj, () =>
            {
                if (GameManager.Instance.TryReleaseProject(nextProj.Id))
                {
                    if (ClickJuice.Instance != null && quickReleaseButton != null)
                    {
                        ClickJuice.Instance.SpawnCustomPopup(
                            $"РЕЛИЗ: {nextProj.Title}! +{NumberFormatter.Format(nextProj.RewardMoney)} руб.",
                            quickReleaseButton.transform.position,
                            new Color(1f, 0.85f, 0.25f),
                            true);
                    }
                    RefreshProgressAndComboUI();
                }
            });
        }
    }

    private void UpdateComboVisuals()
    {
        if (GameManager.Instance == null) return;
        if (comboBarFill == null || comboBarText == null) return;

        float energy = GameManager.Instance.ComboEnergy;
        double mult = GameManager.Instance.GetComboMultiplier();
        comboBarFill.fillAmount = energy;

        if (mult >= 2.9)
        {
            comboBarFill.color = new Color(1.0f, 0.30f, 0.55f, 0.95f);
            comboBarText.text = "РЕЖИМ «В ПОТОКЕ»: БОНУС КЛИКА x3.0!!";
            comboBarText.color = new Color(1.0f, 0.85f, 0.3f, 1f);
        }
        else if (mult >= 1.9)
        {
            comboBarFill.color = new Color(1.0f, 0.65f, 0.15f, 0.95f);
            comboBarText.text = "УСКОРЕНИЕ ПЕЧАТИ: БОНУС КЛИКА x2.0!";
            comboBarText.color = Color.white;
        }
        else if (mult >= 1.4)
        {
            comboBarFill.color = new Color(0.2f, 0.88f, 0.55f, 0.9f);
            comboBarText.text = "РАЗОГРЕВ: БОНУС КЛИКА x1.5";
            comboBarText.color = Color.white;
        }
        else
        {
            comboBarFill.color = new Color(0.20f, 0.65f, 0.95f, 0.75f);
            comboBarText.text = "ТЕМП ПЕЧАТИ: x1.0 (тапай быстрее для x3.0)";
            comboBarText.color = new Color(0.75f, 0.82f, 0.92f, 1f);
        }
    }

    #endregion

    #region Охота на баги (Bug Hunt Mini-Event)

    private void UpdateBugHunt(float dt)
    {
        if (monitorScreenGlow != null)
        {
            if (bugHp > 0)
            {
                float pulse = (Mathf.Sin(Time.time * 9f) + 1f) * 0.5f;
                monitorScreenGlow.color = new Color(1f, 0.12f, 0.15f, 0.10f + pulse * 0.32f);
            }
            else if (monitorScreenGlow.color.a > 0.005f)
            {
                monitorScreenGlow.color = Color.Lerp(monitorScreenGlow.color, new Color(1f, 0.12f, 0.15f, 0f), dt * 6f);
            }
        }

        if (bugAlertButton == null) return;

        if (bugHp > 0)
        {
            bugActiveTimer -= dt;
            if (bugAlertText != null)
            {
                bugAlertText.text = $"⚡ ТАПАЙ БЫСТРО! БАГ ({bugHp} шт) [{bugActiveTimer:F1}с]";
            }

            if (bugActiveTimer <= 0f)
            {
                bugHp = 0;
                bugAlertButton.gameObject.SetActive(false);
                bugSpawnTimer = Random.Range(22f, 36f);
            }
        }
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.TotalCodeWritten >= 15)
            {
                bugSpawnTimer -= dt;
                if (bugSpawnTimer <= 0f)
                {
                    SpawnBug();
                }
            }
        }
    }

    private void SpawnBug()
    {
        if (bugAlertButton == null) return;
        bugHp = MaxBugHp;
        bugActiveTimer = BugLifetime;
        bugAlertButton.gameObject.SetActive(true);

        RectTransform rt = bugAlertButton.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(Random.Range(-150f, 150f), Random.Range(-60f, 80f));
        }

        StartCoroutine(PopInRoutine(bugAlertButton.transform));
    }

    private void OnBugClicked()
    {
        if (bugHp <= 0 || GameManager.Instance == null) return;

        bugHp--;
        Vector2 pos = bugAlertButton != null ? (Vector2)bugAlertButton.transform.position : Vector2.zero;

        if (bugHp > 0)
        {
            HapticFeedback.Vibrate(24);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBugHit(false);
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"ТАП! Еще {bugHp}", pos, new Color(1f, 0.45f, 0.25f), false);
            }
            StartCoroutine(PopInRoutine(bugAlertButton.transform));
        }
        else
        {
            HapticFeedback.Vibrate(45);
            bugAlertButton.gameObject.SetActive(false);
            bugSpawnTimer = Random.Range(25f, 40f);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayBugHit(true);
            GameManager.Instance.ClaimBugFixReward(pos, out double bonusCode, out double bonusMoney);
            if (DailyQuestsUI.Instance != null) DailyQuestsUI.Instance.OnBugSquashed();

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup(
                    $"🎉 БАГ УСТРАНЕН! +{NumberFormatter.Format(bonusCode)} кода | +{NumberFormatter.Format(bonusMoney)} руб.",
                    pos,
                    new Color(0.2f, 1f, 0.55f),
                    true);
            }
        }
    }

    #endregion

    #region Обработка кликов и терминала

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 screenPos)
    {
        string nextLine = CodeSnippets[Random.Range(0, CodeSnippets.Length)];
        terminalHistory.Add(nextLine);

        // Храним до 28 строк для непрерывного плавного вертикального скролла
        if (terminalHistory.Count > Mathf.Max(28, maxVisibleLines * 2))
        {
            terminalHistory.RemoveAt(0);
            currentScrollY = Mathf.Max(0f, currentScrollY - TerminalLineHeight);
            targetScrollY = Mathf.Max(0f, targetScrollY - TerminalLineHeight);
        }

        float totalHeight = (terminalHistory.Count + 1) * TerminalLineHeight;
        targetScrollY = Mathf.Max(0f, totalHeight - ViewportVisibleHeight);
        UpdateTerminalDisplay();

        if (keyboardGlowImage != null)
        {
            if (glowCoroutine != null) StopCoroutine(glowCoroutine);
            glowCoroutine = StartCoroutine(KeyboardGlowRoutine(isCrit));
        }

        if (keyboardTransform != null)
        {
            StartCoroutine(KeyboardTapRoutine());
        }

        if (mouseTransform != null && mouseTransform.gameObject.activeSelf)
        {
            StartCoroutine(MouseClickPunchRoutine());
        }

        FlashKey(isCrit);

        if (coffeeMugTransform != null && coffeeMugTransform.gameObject.activeSelf)
        {
            coffeeMugTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
            coffeeJostleBoost = 1.0f;
        }

        if (energyCanTransform != null && energyCanTransform.gameObject.activeSelf)
        {
            energyJostleBoost = 1.0f;
        }

        bool isCombo = GameManager.Instance != null && GameManager.Instance.GetComboMultiplier() > 1.15;
        TriggerCatTapReaction(isCombo);

        UpdateComboVisuals();
        RefreshProgressAndComboUI();
    }

    private int keyFlashIndex = 0;
    private Graphic[] activeKeyGlows;

    private void FlashKey(bool isCrit)
    {
        if (activeKeyGlows == null || activeKeyGlows.Length == 0)
        {
            var list = new List<Graphic>();
            if (keyGlowW != null) list.Add(keyGlowW);
            if (keyGlowA != null) list.Add(keyGlowA);
            if (keyGlowS != null) list.Add(keyGlowS);
            if (keyGlowD != null) list.Add(keyGlowD);
            if (keyGlowSpace != null) list.Add(keyGlowSpace);
            if (keyGlowEsc != null) list.Add(keyGlowEsc);
            activeKeyGlows = list.ToArray();
        }

        if (activeKeyGlows.Length == 0) return;

        if (isCrit)
        {
            Color goldColor = new Color(1f, 0.88f, 0.25f, 0.95f);
            foreach (var g in activeKeyGlows)
            {
                if (g != null) StartCoroutine(IndividualKeyFlashRoutine(g, goldColor, 0.24f));
            }
        }
        else
        {
            Graphic keyToFlash = activeKeyGlows[keyFlashIndex % activeKeyGlows.Length];
            keyFlashIndex++;
            Color cyanColor = new Color(0f, 0.95f, 1f, 0.9f);
            StartCoroutine(IndividualKeyFlashRoutine(keyToFlash, cyanColor, 0.12f));

            if (keyGlowSpace != null && Random.value < 0.35f && keyToFlash != keyGlowSpace)
            {
                StartCoroutine(IndividualKeyFlashRoutine(keyGlowSpace, new Color(0f, 0.95f, 1f, 0.8f), 0.14f));
            }
        }
    }

    private IEnumerator IndividualKeyFlashRoutine(Graphic keyGraphic, Color flashColor, float duration)
    {
        if (keyGraphic == null) yield break;
        keyGraphic.color = flashColor;
        Transform kt = keyGraphic.transform;
        Vector3 origScale = Vector3.one;
        kt.localScale = new Vector3(0.92f, 0.92f, 1f);

        float elapsed = 0f;
        Color clearColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            keyGraphic.color = Color.Lerp(flashColor, clearColor, t);
            kt.localScale = Vector3.Lerp(new Vector3(0.92f, 0.92f, 1f), origScale, t);
            yield return null;
        }

        keyGraphic.color = clearColor;
        kt.localScale = origScale;
    }

    private void UpdateTerminalDisplay()
    {
        if (monitorCodeText == null) return;

        string cursor = cursorVisible ? "<color=#00FF88>_</color>" : " ";
        string fullCode = string.Join("\n", terminalHistory) + "\n> " + cursor;
        monitorCodeText.text = fullCode;
    }

    private IEnumerator KeyboardGlowRoutine(bool isCrit)
    {
        keyboardGlowImage.color = isCrit ? Color.yellow : keyGlowActive;
        float elapsed = 0f;
        float duration = isCrit ? 0.25f : 0.12f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            keyboardGlowImage.color = Color.Lerp(keyboardGlowImage.color, keyGlowNormal, elapsed / duration);
            yield return null;
        }

        keyboardGlowImage.color = keyGlowNormal;
    }

    private IEnumerator KeyboardTapRoutine()
    {
        Vector3 baseScale = Vector3.one;
        keyboardTransform.localScale = new Vector3(0.98f, 0.96f, 1f);
        yield return new WaitForSecondsRealtime(0.06f);
        keyboardTransform.localScale = baseScale;
    }

    #endregion

    #region Анимация Кота (Cat Reactions, Idle Sleep & Awake Emote)

    private float catDreamTimer = 0f;
    private Coroutine catAwakeCoroutine;
    private Coroutine catHeartCoroutine;

    private void InitCatInteraction()
    {
        if (catTransform != null)
        {
            if (catImage == null) catImage = catTransform.GetComponent<Image>();
            if (catSleepingSprite == null && catImage != null) catSleepingSprite = catImage.sprite;

            Button catBtn = catTransform.GetComponent<Button>();
            if (catBtn == null) catBtn = catTransform.gameObject.AddComponent<Button>();
            catBtn.onClick.RemoveAllListeners();
            catBtn.onClick.AddListener(OnCatClickedDirectly);
        }
    }

    private void UpdateCatIdle(float dt)
    {
        if (catTransform == null || !catTransform.gameObject.activeSelf) return;

        // Если кот не занят реакцией на тап, он плавно дышит и изредка покачивает хвостиком
        if (catWiggleCoroutine == null)
        {
            float breath = 1f + Mathf.Sin(Time.time * 2.2f) * 0.025f;
            float idleTail = Mathf.Sin(Time.time * 1.5f) * 2.2f;
            catTransform.localScale = new Vector3(breath, 2f - breath, 1f);
            catTransform.localRotation = Quaternion.Euler(0, 0, idleTail);

            // Редкое сонное подергивание ушком во сне раз в 4-5 секунд
            catDreamTimer += dt;
            if (catDreamTimer > 4.5f)
            {
                catDreamTimer = 0f;
                StartCoroutine(CatSleepTwitchRoutine());
            }
        }
    }

    private IEnumerator CatSleepTwitchRoutine()
    {
        if (catWiggleCoroutine != null) yield break;
        float elapsed = 0f;
        while (elapsed < 0.22f && catWiggleCoroutine == null)
        {
            elapsed += Time.unscaledDeltaTime;
            float twitch = Mathf.Sin(elapsed * 45f) * 2.0f;
            catTransform.localRotation = Quaternion.Euler(0, 0, twitch);
            yield return null;
        }
    }

    public void TriggerCatTapReaction(bool isCombo)
    {
        if (catTransform == null || !catTransform.gameObject.activeSelf) return;
        if (catWiggleCoroutine != null) StopCoroutine(catWiggleCoroutine);
        catWiggleCoroutine = StartCoroutine(CatWiggleRoutine(isCombo));
    }

    private IEnumerator CatWiggleRoutine(bool isCombo)
    {
        float duration = isCombo ? 0.32f : 0.20f;
        float freq = isCombo ? 26f : 16f;
        float maxAngle = isCombo ? 6.0f : 3.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float decay = 1f - t;

            // Подергивание ушком и хвостом (вращение туда-обратно)
            float angle = Mathf.Sin(elapsed * freq) * maxAngle * decay;
            catTransform.localRotation = Quaternion.Euler(0, 0, angle);

            // Мурчащий отскок (squash-stretch)
            float bounce = Mathf.Sin(elapsed * freq * 0.5f) * (isCombo ? 0.09f : 0.05f) * decay;
            catTransform.localScale = new Vector3(1f + bounce, 1f - bounce * 0.7f, 1f);

            yield return null;
        }

        catTransform.localRotation = Quaternion.identity;
        catTransform.localScale = Vector3.one;
        catWiggleCoroutine = null;
    }

    public void OnCatClickedDirectly()
    {
        TriggerCatTapReaction(true);
        if (AudioManager.Instance != null)
        {
            if (currentPetType == 0) AudioManager.Instance.PlayCatPurr();
            else if (currentPetType == 1) AudioManager.Instance.PlayDogBark();
            else AudioManager.Instance.PlayRoboBeep();
        }

        // Поглаживание питомца увеличивает уровень счастья ("Мурчалометр / Настроение")
        catHappiness = Mathf.Min(1.0f, catHappiness + 0.25f);
        if (catHappiness >= 0.99f)
        {
            catHappiness = 0.35f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddComboEnergy(0.35f);
            }
            if (ClickJuice.Instance != null && catTransform != null)
            {
                string happyMsg = currentPetType == 0 ? "😻 МУРЧАЛОМЕТР 100%! +35% ПОТОКА!" :
                                 (currentPetType == 1 ? "🐶 КОРГИ СЧАСТЛИВ! +35% ПОТОКА!" : "🤖 СИСТЕМЫ ПЕРЕГРУЖЕНЫ РАДОСТЬЮ! +35% ПОТОКА!");
                ClickJuice.Instance.SpawnCustomPopup(happyMsg, catTransform.position + Vector3.up * 55f, new Color(1f, 0.4f, 0.85f, 1f), true);
            }
        }

        if (catAwakeCoroutine != null) StopCoroutine(catAwakeCoroutine);
        catAwakeCoroutine = StartCoroutine(CatPetReactionRoutine());

        if (catHeartEmote != null)
        {
            if (catHeartCoroutine != null) StopCoroutine(catHeartCoroutine);
            catHeartCoroutine = StartCoroutine(CatHeartEmoteRoutine());
        }
        else if (ClickJuice.Instance != null && catTransform != null)
        {
            string heartMsg = currentPetType == 0 ? "💖 Муррр~" : (currentPetType == 1 ? "💖 Гав!" : "⚡ Бип-боп!");
            ClickJuice.Instance.SpawnCustomPopup(heartMsg, catTransform.position + Vector3.up * 45f, new Color(1f, 0.40f, 0.70f, 1f), false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.08f);
        }
    }

    private IEnumerator CatPetReactionRoutine()
    {
        Sprite awakeSpr = catAwakeSprite;
        Sprite sleepSpr = catSleepingSprite;
        Sprite stretchSpr = catStretchSprite;

        if (currentPetType == 1)
        {
            awakeSpr = sprCorgiAwake != null ? sprCorgiAwake : catAwakeSprite;
            sleepSpr = sprCorgiSleep != null ? sprCorgiSleep : catSleepingSprite;
            stretchSpr = sprCorgiAwake;
        }
        else if (currentPetType == 2)
        {
            awakeSpr = sprRoboAwake != null ? sprRoboAwake : catAwakeSprite;
            sleepSpr = sprRoboSleep != null ? sprRoboSleep : catSleepingSprite;
            stretchSpr = sprRoboAwake;
        }

        // 1. Потягивание / анимация радости
        if (catImage != null && stretchSpr != null)
        {
            catImage.sprite = stretchSpr;
            Vector3 baseScale = catTransform.localScale;
            catTransform.localScale = new Vector3(1.10f, 0.92f, 1f);
            yield return new WaitForSecondsRealtime(0.75f);
            catTransform.localScale = baseScale;
        }

        // 2. Довольная пробужденная мордочка
        if (catImage != null && awakeSpr != null)
        {
            catImage.sprite = awakeSpr;
        }

        yield return new WaitForSecondsRealtime(2.0f);

        // 3. Возвращение в спящий режим
        if (catImage != null && sleepSpr != null)
        {
            catImage.sprite = sleepSpr;
        }
        catAwakeCoroutine = null;
    }

    private IEnumerator CatHeartEmoteRoutine()
    {
        if (catHeartEmote == null) yield break;
        catHeartEmote.gameObject.SetActive(true);
        CanvasGroup cg = catHeartEmote.GetComponent<CanvasGroup>();
        if (cg == null) cg = catHeartEmote.gameObject.AddComponent<CanvasGroup>();

        Vector2 startPos = new Vector2(0, 65);
        Vector2 targetPos = new Vector2(0, 125);
        catHeartEmote.anchoredPosition = startPos;
        catHeartEmote.localScale = Vector3.zero;
        cg.alpha = 1f;

        float duration = 1.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float sway = Mathf.Sin(t * Mathf.PI * 4f) * 10f;
            catHeartEmote.anchoredPosition = Vector2.Lerp(startPos, targetPos, t) + new Vector2(sway, 0);

            if (t < 0.25f)
            {
                catHeartEmote.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.35f, t / 0.25f);
            }
            else if (t < 0.45f)
            {
                catHeartEmote.localScale = Vector3.Lerp(Vector3.one * 1.35f, Vector3.one, (t - 0.25f) / 0.20f);
            }

            if (t > 0.65f)
            {
                cg.alpha = 1f - (t - 0.65f) / 0.35f;
            }

            yield return null;
        }

        catHeartEmote.gameObject.SetActive(false);
        catHeartCoroutine = null;
    }

    #region Кастомизация Кота (Cat Accessories & Skin Tints)

    private int catAccessoryIndex = 0; // 0 = Нет, 1 = Очки, 2 = Бабочка
    private int catSkinIndex = 0; // 0 = Рыжик, 1 = Серый, 2 = Чёрный
    private float catHappiness = 0.2f;

    private readonly Color[] CatSkinTints = new Color[]
    {
        Color.white, // Classic ginger
        new Color(0.85f, 0.88f, 0.95f), // Smoky grey
        new Color(0.48f, 0.50f, 0.55f)  // Dark tuxedo
    };

    private void InitCatCustomization()
    {
        catAccessoryIndex = PlayerPrefs.GetInt("Dev_CatAccessory", 0);
        catSkinIndex = PlayerPrefs.GetInt("Dev_CatSkin", 0);

        if (catAccessoryImage == null && catTransform != null)
        {
            catAccessoryImage = catTransform.Find("CatAccessory")?.GetComponent<Image>();
        }

        if (catAccessoryButton == null && catTransform != null)
        {
            catAccessoryButton = catTransform.Find("CatAccessoryBtn")?.GetComponent<Button>();
        }

        if (catAccessoryButton != null)
        {
            catAccessoryButton.onClick.RemoveAllListeners();
            catAccessoryButton.onClick.AddListener(OnCatAccessoryClicked);
        }

        ApplyCatVisuals();
    }

    private void ApplyCatVisuals()
    {
        if (catImage != null && catSkinIndex >= 0 && catSkinIndex < CatSkinTints.Length)
        {
            catImage.color = CatSkinTints[catSkinIndex];
        }

        if (catAccessoryImage != null)
        {
            if (catAccessoryIndex == 1 && catGlassesSprite != null)
            {
                catAccessoryImage.gameObject.SetActive(true);
                catAccessoryImage.sprite = catGlassesSprite;
                catAccessoryImage.rectTransform.anchoredPosition = new Vector2(-18, 12);
                catAccessoryImage.rectTransform.sizeDelta = new Vector2(48, 24);
            }
            else if (catAccessoryIndex == 2 && catBowtieSprite != null)
            {
                catAccessoryImage.gameObject.SetActive(true);
                catAccessoryImage.sprite = catBowtieSprite;
                catAccessoryImage.rectTransform.anchoredPosition = new Vector2(-15, -18);
                catAccessoryImage.rectTransform.sizeDelta = new Vector2(40, 24);
            }
            else
            {
                catAccessoryImage.gameObject.SetActive(false);
            }
        }
    }

    public void OnCatAccessoryClicked()
    {
        catAccessoryIndex = (catAccessoryIndex + 1) % 3;
        PlayerPrefs.SetInt("Dev_CatAccessory", catAccessoryIndex);
        PlayerPrefs.Save();

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();
        ApplyCatVisuals();

        if (catAccessoryImage != null && catAccessoryImage.gameObject.activeSelf)
        {
            StartCoroutine(StickerPunchRoutine(catAccessoryImage.transform));
        }

        string accName = catAccessoryIndex == 1 ? "🕶️ Очки хакера" : (catAccessoryIndex == 2 ? "🎀 Бабочка" : "Без аксессуара");
        if (ClickJuice.Instance != null && catTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(accName, catTransform.position + Vector3.up * 40f, new Color(1f, 0.65f, 0.9f, 1f), false);
        }
    }

    #endregion

    #region Освещение и День/Ночь (Day/Night & Desk Lamp)

    private int timeOfDayIndex = 0; // 0 = День, 1 = Закат, 2 = Ночь
    private bool isLampOn = true;
    private readonly Color AmbientDay = new Color(1f, 0.96f, 0.88f, 0f);
    private readonly Color AmbientSunset = new Color(0.96f, 0.44f, 0.12f, 0.16f);
    private readonly Color AmbientNight = new Color(0.04f, 0.06f, 0.20f, 0.45f);

    private void InitLighting()
    {
        timeOfDayIndex = PlayerPrefs.GetInt("Dev_TimeOfDay", 0);
        isLampOn = PlayerPrefs.GetInt("Dev_LampOn", 1) == 1;

        if (deskLampObj == null) deskLampObj = transform.Find("DeskMat/DeskLamp")?.gameObject;
        if (deskLampButton == null && deskLampObj != null) deskLampButton = deskLampObj.GetComponent<Button>();
        if (deskLampButton != null)
        {
            deskLampButton.onClick.RemoveAllListeners();
            deskLampButton.onClick.AddListener(OnDeskLampClicked);
        }

        if (lampConeObj == null) lampConeObj = transform.Find("DeskMat/DeskLamp/LampLightCone")?.gameObject;
        if (ambientOverlayGraphic == null) ambientOverlayGraphic = transform.Find("AmbientOverlay")?.GetComponent<Graphic>();
        if (timeOfDayButton == null) timeOfDayButton = transform.Find("TimeOfDayToggleBtn")?.GetComponent<Button>();
        if (timeOfDayText == null && timeOfDayButton != null) timeOfDayText = timeOfDayButton.GetComponentInChildren<TMP_Text>();

        if (timeOfDayButton != null)
        {
            timeOfDayButton.onClick.RemoveAllListeners();
            timeOfDayButton.onClick.AddListener(OnTimeOfDayClicked);
        }

        ApplyLightingState(false);
    }

    private void UpdateLighting(float dt)
    {
        // Плавное мягкое дыхание света лампы
        if (isLampOn && lampConeObj != null && lampConeObj.activeSelf)
        {
            float pulse = 0.98f + Mathf.Sin(Time.time * 2.8f) * 0.025f;
            lampConeObj.transform.localScale = new Vector3(pulse, pulse, 1f);
        }

        // Если вечер или ночь, клавиатура играет мягкими RGB Chroma волнами
        if (keyboardGlowImage != null && (timeOfDayIndex == 1 || timeOfDayIndex == 2))
        {
            float hue = (Time.time * 0.15f) % 1f;
            Color chroma = Color.HSVToRGB(hue, 0.70f, 0.90f);
            chroma.a = timeOfDayIndex == 2 ? 0.40f : 0.25f;
            keyboardGlowImage.color = chroma;
        }
    }

    public void OnDeskLampClicked()
    {
        isLampOn = !isLampOn;
        PlayerPrefs.SetInt("Dev_LampOn", isLampOn ? 1 : 0);
        PlayerPrefs.Save();

        HapticFeedback.Vibrate(35);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLampSwitch();

        if (deskLampObj != null)
        {
            StartCoroutine(StickerPunchRoutine(deskLampObj.transform));
        }

        ApplyLightingState(true);

        if (ClickJuice.Instance != null && deskLampObj != null)
        {
            string msg = isLampOn ? "💡 Лампа включена" : "🌑 Лампа выключена";
            ClickJuice.Instance.SpawnCustomPopup(msg, deskLampObj.transform.position + Vector3.up * 40f, new Color(1f, 0.92f, 0.35f, 1f), false);
        }
    }

    public void OnTimeOfDayClicked()
    {
        timeOfDayIndex = (timeOfDayIndex + 1) % 3;
        PlayerPrefs.SetInt("Dev_TimeOfDay", timeOfDayIndex);
        PlayerPrefs.Save();

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();

        if (timeOfDayIndex == 2 && !isLampOn)
        {
            isLampOn = true;
            PlayerPrefs.SetInt("Dev_LampOn", 1);
        }

        ApplyLightingState(true);

        string label = timeOfDayIndex == 0 ? "☀️ День" : (timeOfDayIndex == 1 ? "🌇 Закат" : "🌙 Ночь");
        if (ClickJuice.Instance != null && timeOfDayButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(label, timeOfDayButton.transform.position + Vector3.down * 25f, new Color(0.9f, 0.9f, 1f, 1f), false);
        }
    }

    private void ApplyLightingState(bool animate)
    {
        if (lampConeObj != null)
        {
            lampConeObj.SetActive(isLampOn);
        }

        Color targetAmbient = timeOfDayIndex == 0 ? AmbientDay : (timeOfDayIndex == 1 ? AmbientSunset : AmbientNight);
        if (ambientOverlayGraphic != null)
        {
            if (animate) StartCoroutine(FadeAmbientRoutine(ambientOverlayGraphic, targetAmbient, 0.4f));
            else ambientOverlayGraphic.color = targetAmbient;
        }

        if (timeOfDayText != null)
        {
            timeOfDayText.text = timeOfDayIndex == 0 ? "☀️ ДЕНЬ" : (timeOfDayIndex == 1 ? "🌇 ЗАКАТ" : "🌙 НОЧЬ");
        }
    }

    private IEnumerator FadeAmbientRoutine(Graphic g, Color target, float duration)
    {
        Color start = g.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            g.color = Color.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        g.color = target;
    }

    #endregion

    #endregion

    #region Интерактивные Напитки (Coffee & Energy Can Click / Sip)

    private void InitDrinksInteraction()
    {
        if (coffeeMugTransform != null)
        {
            if (coffeeButton == null) coffeeButton = coffeeMugTransform.GetComponent<Button>();
            if (coffeeButton == null) coffeeButton = coffeeMugTransform.gameObject.AddComponent<Button>();
            coffeeButton.onClick.RemoveAllListeners();
            coffeeButton.onClick.AddListener(OnCoffeeClicked);
        }

        if (energyCanTransform != null)
        {
            if (energyButton == null) energyButton = energyCanTransform.GetComponent<Button>();
            if (energyButton == null) energyButton = energyCanTransform.gameObject.AddComponent<Button>();
            energyButton.onClick.RemoveAllListeners();
            energyButton.onClick.AddListener(OnEnergyClicked);
        }
    }

    public void OnCoffeeClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySipSound();
        if (coffeeMugTransform != null) StartCoroutine(DrinkPunchRoutine(coffeeMugTransform));
        coffeeJostleBoost = 3.2f;

        if (ClickJuice.Instance != null && coffeeMugTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("Глоток кофе! ☕ (+бодрость)", coffeeMugTransform.position, new Color(0.95f, 0.70f, 0.40f, 1f), false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.12f);
        }
    }

    public void OnEnergyClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySipSound();
        if (energyCanTransform != null) StartCoroutine(DrinkPunchRoutine(energyCanTransform));
        energyJostleBoost = 3.2f;

        if (ClickJuice.Instance != null && energyCanTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("Энергетик! ⚡ (В ПОТОКЕ)", energyCanTransform.position, new Color(0.1f, 1f, 0.85f, 1f), false);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.20f);
        }
    }

    private IEnumerator DrinkPunchRoutine(Transform tr)
    {
        if (tr == null) yield break;
        Vector3 baseScale = Vector3.one;
        Quaternion baseRot = tr.localRotation;

        tr.localScale = new Vector3(1.18f, 0.82f, 1f);
        tr.localRotation = Quaternion.Euler(0, 0, Random.Range(-6f, 6f));

        float duration = 0.22f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            tr.localScale = Vector3.Lerp(new Vector3(1.18f, 0.82f, 1f), baseScale, t);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, baseRot, t);
            yield return null;
        }

        tr.localScale = baseScale;
        tr.localRotation = baseRot;
    }

    #endregion

    #region Радужный перелив клавиатуры при комбо x3.0

    private bool wasMaxCombo = false;

    private void UpdateKeyboardRgb(float dt)
    {
        if (keyboardTransform == null || !keyboardTransform.gameObject.activeSelf) return;

        double combo = GameManager.Instance != null ? GameManager.Instance.GetComboMultiplier() : 1.0;
        bool isMaxCombo = combo >= 2.85;

        if (activeKeyGlows == null || activeKeyGlows.Length == 0)
        {
            var list = new List<Graphic>();
            if (keyGlowW != null) list.Add(keyGlowW);
            if (keyGlowA != null) list.Add(keyGlowA);
            if (keyGlowS != null) list.Add(keyGlowS);
            if (keyGlowD != null) list.Add(keyGlowD);
            if (keyGlowSpace != null) list.Add(keyGlowSpace);
            if (keyGlowEsc != null) list.Add(keyGlowEsc);
            activeKeyGlows = list.ToArray();
        }

        if (isMaxCombo)
        {
            wasMaxCombo = true;
            float waveSpeed = 2.2f;
            float baseHue = Mathf.Repeat(Time.time * waveSpeed, 1f);
            float pulse = (Mathf.Sin(Time.time * 8.5f) + 1f) * 0.5f;

            if (keyboardGlowImage != null)
            {
                Color kbColor = Color.HSVToRGB(baseHue, 0.95f, 1f);
                kbColor.a = 0.50f + pulse * 0.35f;
                keyboardGlowImage.color = kbColor;
            }

            for (int i = 0; i < activeKeyGlows.Length; i++)
            {
                if (activeKeyGlows[i] == null) continue;
                float keyHue = Mathf.Repeat(baseHue + i * 0.14f, 1f);
                Color kColor = Color.HSVToRGB(keyHue, 0.92f, 1f);
                kColor.a = 0.65f + pulse * 0.35f;
                activeKeyGlows[i].color = kColor;
            }
        }
        else if (wasMaxCombo)
        {
            wasMaxCombo = false;
            if (keyboardGlowImage != null) keyboardGlowImage.color = keyGlowNormal;
            if (activeKeyGlows != null)
            {
                for (int i = 0; i < activeKeyGlows.Length; i++)
                {
                    if (activeKeyGlows[i] != null) activeKeyGlows[i].color = keyGlowNormal;
                }
            }
        }
    }

    #endregion

    #region Партиклы пара кофе и пузырьков энергетика

    private void InitSteamAndFizz()
    {
        if (coffeeSteamWisps != null && coffeeSteamWisps.Length > 0)
        {
            steamBasePos = new Vector2[coffeeSteamWisps.Length];
            steamGraphics = new Graphic[coffeeSteamWisps.Length];
            for (int i = 0; i < coffeeSteamWisps.Length; i++)
            {
                if (coffeeSteamWisps[i] != null)
                {
                    steamBasePos[i] = coffeeSteamWisps[i].anchoredPosition;
                    steamGraphics[i] = coffeeSteamWisps[i].GetComponent<Graphic>();
                }
            }
        }

        if (energyFizzBubbles != null && energyFizzBubbles.Length > 0)
        {
            fizzBasePos = new Vector2[energyFizzBubbles.Length];
            fizzGraphics = new Graphic[energyFizzBubbles.Length];
            for (int i = 0; i < energyFizzBubbles.Length; i++)
            {
                if (energyFizzBubbles[i] != null)
                {
                    fizzBasePos[i] = energyFizzBubbles[i].anchoredPosition;
                    fizzGraphics[i] = energyFizzBubbles[i].GetComponent<Graphic>();
                }
            }
        }
    }

    private void UpdateCoffeeSteam(float dt)
    {
        if (coffeeMugTransform == null || !coffeeMugTransform.gameObject.activeSelf) return;
        if (coffeeSteamWisps == null || coffeeSteamWisps.Length == 0) return;

        if (coffeeJostleBoost > 0f)
        {
            coffeeJostleBoost = Mathf.Max(0f, coffeeJostleBoost - dt * 2.5f);
        }

        float time = Time.time * 0.85f;
        for (int i = 0; i < coffeeSteamWisps.Length; i++)
        {
            var rt = coffeeSteamWisps[i];
            if (rt == null) continue;

            float phase = (time + i * 0.33f) % 1.0f;
            Vector2 baseP = steamBasePos != null && i < steamBasePos.Length ? steamBasePos[i] : Vector2.zero;

            // Плавный подъем вверх с легким покачиванием пара
            float riseY = phase * (55f + coffeeJostleBoost * 20f);
            float swayX = Mathf.Sin((time * 3f) + i * 1.5f) * (6f + i * 2f);
            rt.anchoredPosition = new Vector2(baseP.x + swayX, baseP.y + riseY);

            // Клубы пара слегка расширяются при подъеме
            float scale = Mathf.Lerp(0.55f, 1.25f, phase);
            rt.localScale = new Vector3(scale, scale, 1f);

            // Прозрачность: рождается у ободка, максимум в середине, тает наверху
            if (steamGraphics != null && i < steamGraphics.Length && steamGraphics[i] != null)
            {
                float alpha = Mathf.Sin(phase * Mathf.PI) * (0.42f + coffeeJostleBoost * 0.35f);
                Color c = steamGraphics[i].color;
                steamGraphics[i].color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
            }
        }
    }

    private void UpdateEnergyFizz(float dt)
    {
        if (energyCanTransform == null || !energyCanTransform.gameObject.activeSelf) return;
        if (energyFizzBubbles == null || energyFizzBubbles.Length == 0) return;

        if (energyJostleBoost > 0f)
        {
            energyJostleBoost = Mathf.Max(0f, energyJostleBoost - dt * 3f);
        }

        bool isBoostActive = GameManager.Instance != null && GameManager.Instance.IsBoostActive;
        float speed = isBoostActive ? 1.8f : (0.95f + energyJostleBoost * 1.2f);
        float time = Time.time * speed;

        for (int i = 0; i < energyFizzBubbles.Length; i++)
        {
            var rt = energyFizzBubbles[i];
            if (rt == null) continue;

            float phase = (time + i * 0.28f) % 1.0f;
            Vector2 baseP = fizzBasePos != null && i < fizzBasePos.Length ? fizzBasePos[i] : Vector2.zero;

            // Вылет пузырьков из горлышка банки с мелкой вибрацией
            float riseY = phase * (38f + energyJostleBoost * 18f);
            float jitterX = Mathf.Cos((time * 8f) + i * 2.1f) * 4.5f;
            rt.anchoredPosition = new Vector2(baseP.x + jitterX, baseP.y + riseY);

            // Микро-хлопок в конце подъема
            float popScale = phase > 0.85f ? Mathf.Lerp(1f, 1.4f, (phase - 0.85f) / 0.15f) : 1f;
            rt.localScale = new Vector3(popScale, popScale, 1f);

            if (fizzGraphics != null && i < fizzGraphics.Length && fizzGraphics[i] != null)
            {
                // Неоновое свечение пузырька
                float alpha = phase > 0.88f 
                    ? Mathf.Lerp(0.85f, 0f, (phase - 0.88f) / 0.12f)
                    : Mathf.Sin(phase * Mathf.PI) * 0.85f;
                Color c = fizzGraphics[i].color;
                fizzGraphics[i].color = new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha));
            }
        }
    }

    #endregion

    #region Плавный скролл терминала и RGB Мышь

    private void UpdateTerminalScroll(float dt)
    {
        if (terminalRt == null) return;
        currentScrollY = Mathf.Lerp(currentScrollY, targetScrollY, dt * 14f);
        terminalRt.anchoredPosition = new Vector2(terminalRt.anchoredPosition.x, currentScrollY);
    }

    private void UpdateMouseRgb(float dt)
    {
        if (mouseTransform == null || !mouseTransform.gameObject.activeSelf) return;
        if (mouseGlowGraphic == null) return;

        double combo = GameManager.Instance != null ? GameManager.Instance.GetComboMultiplier() : 1.0;
        bool isCombo = combo > 1.15;

        // Скорость спектрального перелива RGB возрастает в комбо
        float waveSpeed = isCombo ? 1.8f : 0.4f;
        float hue = Mathf.Repeat(Time.time * waveSpeed, 1f);
        Color rgbColor = Color.HSVToRGB(hue, isCombo ? 0.95f : 0.75f, 1f);

        // Пульсация яркости подсветки
        float pulse = (Mathf.Sin(Time.time * (isCombo ? 7f : 2.5f)) + 1f) * 0.5f;
        rgbColor.a = isCombo ? (0.65f + pulse * 0.35f) : (0.35f + pulse * 0.25f);
        mouseGlowGraphic.color = rgbColor;
    }

    private IEnumerator MouseClickPunchRoutine()
    {
        Vector3 baseScale = Vector3.one;
        mouseTransform.localScale = new Vector3(0.97f, 0.94f, 1f);
        yield return new WaitForSecondsRealtime(0.05f);
        mouseTransform.localScale = baseScale;
    }

    #endregion

    #region Интерактивная Мышь (Omron Microswitch Click & Cord Sway)

    private void InitMouseInteraction()
    {
        if (mouseTransform != null)
        {
            if (mouseButton == null) mouseButton = mouseTransform.GetComponent<Button>();
            if (mouseButton == null) mouseButton = mouseTransform.gameObject.AddComponent<Button>();
            mouseButton.onClick.RemoveAllListeners();
            mouseButton.onClick.AddListener(OnMouseClickedDirectly);
        }
    }

    public void OnMouseClickedDirectly()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClickWithSwitch(currentMouseSwitchType);
        if (mouseTransform != null) StartCoroutine(MouseDirectClickRoutine());

        // Нажатие на игровую мышь кликает код
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClickCode(mouseTransform.position);
        }

        if (ClickJuice.Instance != null && mouseTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("Клик! 🖱️", mouseTransform.position, new Color(0.2f, 0.95f, 1f, 1f), false);
        }
    }

    private IEnumerator MouseDirectClickRoutine()
    {
        if (mouseTransform == null) yield break;
        Vector3 baseScale = Vector3.one;
        Quaternion baseRot = Quaternion.identity;

        mouseTransform.localScale = new Vector3(0.93f, 0.89f, 1f);
        mouseTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-4.5f, 4.5f));

        if (mouseGlowGraphic != null)
        {
            mouseGlowGraphic.color = Color.white;
        }

        float duration = 0.16f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            mouseTransform.localScale = Vector3.Lerp(new Vector3(0.93f, 0.89f, 1f), baseScale, t);
            mouseTransform.localRotation = Quaternion.Slerp(mouseTransform.localRotation, baseRot, t);
            yield return null;
        }

        mouseTransform.localScale = baseScale;
        mouseTransform.localRotation = baseRot;
    }

    #endregion

    #region Стикеры на Мониторе (Developer Badges & Tooltips)

    private bool wasStickerUnity = false;
    private bool wasStickerCSharp = false;
    private bool wasStickerGit = false;
    private bool wasStickerWorks = false;

    private void InitStickersInteraction()
    {
        BindSticker(stickerUnity, "Unity 2D", "Сделано на Unity 2D (URP)!");
        BindSticker(stickerCSharp, "C# .NET", "Чистый C# 12 без багов!");
        BindSticker(stickerGit, "Git", "Git: ветка main актуальна!");
        BindSticker(stickerWorks, "It Works!", "На моём компьютере всё работало!");
    }

    private void BindSticker(GameObject stickerGo, string title, string tooltip)
    {
        if (stickerGo == null) return;
        Button btn = stickerGo.GetComponent<Button>();
        if (btn == null) btn = stickerGo.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnStickerClicked(stickerGo.transform, tooltip));
    }

    private void OnStickerClicked(Transform tr, string tooltip)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();
        StartCoroutine(StickerPunchRoutine(tr));
        if (ClickJuice.Instance != null && tr != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(tooltip, tr.position, new Color(1f, 0.85f, 0.35f, 1f), false);
        }
    }

    private IEnumerator StickerPunchRoutine(Transform tr)
    {
        if (tr == null) yield break;
        Vector3 baseScale = Vector3.one;
        Quaternion baseRot = tr.localRotation;

        tr.localScale = new Vector3(1.24f, 1.24f, 1f);
        tr.localRotation = baseRot * Quaternion.Euler(0, 0, Random.Range(-9f, 9f));

        float duration = 0.22f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            tr.localScale = Vector3.Lerp(new Vector3(1.24f, 1.24f, 1f), baseScale, t);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, baseRot, t);
            yield return null;
        }

        tr.localScale = baseScale;
        tr.localRotation = baseRot;
    }

    private void UpdateStickersVisibility(bool animateNew)
    {
        if (GameManager.Instance == null) return;

        bool hasUnity = GameManager.Instance.TotalCodeWritten >= 5 || GameManager.Instance.CodeLines >= 5;
        bool hasCSharp = GameManager.Instance.GetUpgradeLevel("soft_coffee") > 0 || GameManager.Instance.GetUpgradeLevel("hw_mouse") > 0 || GameManager.Instance.TotalCodeWritten >= 30;
        bool hasGit = GameManager.Instance.TotalReleasesCount > 0 || GameManager.Instance.TotalCodeWritten >= 100;
        bool hasWorks = GameManager.Instance.BugsFixedCount > 0 || GameManager.Instance.GetComboMultiplier() >= 1.5;

        if (stickerUnity != null)
        {
            stickerUnity.SetActive(hasUnity);
            if (animateNew && hasUnity && !wasStickerUnity) StartCoroutine(PopInRoutine(stickerUnity.transform));
        }
        wasStickerUnity = hasUnity;

        if (stickerCSharp != null)
        {
            stickerCSharp.SetActive(hasCSharp);
            if (animateNew && hasCSharp && !wasStickerCSharp) StartCoroutine(PopInRoutine(stickerCSharp.transform));
        }
        wasStickerCSharp = hasCSharp;

        if (stickerGit != null)
        {
            stickerGit.SetActive(hasGit);
            if (animateNew && hasGit && !wasStickerGit) StartCoroutine(PopInRoutine(stickerGit.transform));
        }
        wasStickerGit = hasGit;

        if (stickerWorks != null)
        {
            stickerWorks.SetActive(hasWorks);
            if (animateNew && hasWorks && !wasStickerWorks) StartCoroutine(PopInRoutine(stickerWorks.transform));
        }
        wasStickerWorks = hasWorks;

        bool hasCrown = PlayerPrefs.GetInt("Streak_HasCrownUnlocked", 0) == 1;
        if (stickerCrown != null)
        {
            stickerCrown.SetActive(hasCrown);
        }
    }

    #endregion

    #region Хоткей-подсказки на мониторе (IDE Shortcut Hints)

    private static readonly string[] HotkeyHints = new string[]
    {
        "<color=#569CD6>[Ctrl+S]</color> Быстрое сохранение",
        "<color=#4EC9B0>[F5]</color> Запуск сборки проекта",
        "<color=#CE9178>[git commit]</color> Фиксация изменений",
        "<color=#FFCC00>[Ctrl+Space]</color> IntelliSense код",
        "<color=#00FF88>[F12]</color> Перейти к определению"
    };

    private float hotkeyTimer = 0f;
    private int hotkeyIndex = 0;

    private void InitHotkeyBadge()
    {
        if (hotkeyBadgeButton != null)
        {
            hotkeyBadgeButton.onClick.RemoveAllListeners();
            hotkeyBadgeButton.onClick.AddListener(OnHotkeyBadgeClicked);
        }
        if (hotkeyBadgeText != null && HotkeyHints.Length > 0)
        {
            hotkeyBadgeText.text = HotkeyHints[0];
        }
    }

    private void UpdateHotkeyHints(float dt)
    {
        if (hotkeyBadgeObj == null || !hotkeyBadgeObj.activeSelf) return;

        hotkeyTimer += dt;
        if (hotkeyTimer >= 7.5f)
        {
            hotkeyTimer = 0f;
            hotkeyIndex = (hotkeyIndex + 1) % HotkeyHints.Length;
            if (hotkeyBadgeText != null)
            {
                hotkeyBadgeText.text = HotkeyHints[hotkeyIndex];
            }
        }
    }

    public void OnHotkeyBadgeClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
            GameManager.Instance.AddComboEnergy(0.06f);
        }

        if (hotkeyBadgeObj != null)
        {
            StartCoroutine(StickerPunchRoutine(hotkeyBadgeObj.transform));
        }

        if (ClickJuice.Instance != null && hotkeyBadgeObj != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("💾 Проект сохранён!", hotkeyBadgeObj.transform.position + Vector3.up * 30f, new Color(0.3f, 0.95f, 0.7f, 1f), false);
        }
    }

    #endregion

    #region Кастомизация клавиатуры и звуковых свитчей

    private int currentKeyboardStyle = 0;
    private int currentSwitchType = 0;

    private static readonly string[] KeyboardStyleNames = new string[]
    {
        "CYBERPUNK",
        "RETRO IBM",
        "TOKYO NEON"
    };

    private static readonly string[] SwitchTypeNames = new string[]
    {
        "CHERRY BLUE",
        "CHERRY BROWN",
        "SPEED RED",
        "THOCK SPACE"
    };

    private void InitKeyboardCustomization()
    {
        currentKeyboardStyle = PlayerPrefs.GetInt("SelectedKeyboardStyle", 0);
        currentSwitchType = PlayerPrefs.GetInt("SelectedSwitchType", 0);

        if (keyboardStyleButton != null)
        {
            keyboardStyleButton.onClick.RemoveAllListeners();
            keyboardStyleButton.onClick.AddListener(CycleKeyboardStyle);
        }

        if (keyboardSwitchButton != null)
        {
            keyboardSwitchButton.onClick.RemoveAllListeners();
            keyboardSwitchButton.onClick.AddListener(CycleSwitchType);
        }

        ApplyKeyboardStyle(currentKeyboardStyle, false);
        UpdateKeyboardCustomizationLabels();
    }

    public void CycleKeyboardStyle()
    {
        currentKeyboardStyle = (currentKeyboardStyle + 1) % 3;
        PlayerPrefs.SetInt("SelectedKeyboardStyle", currentKeyboardStyle);
        PlayerPrefs.Save();
        ApplyKeyboardStyle(currentKeyboardStyle, true);
        UpdateKeyboardCustomizationLabels();

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        if (ClickJuice.Instance != null && keyboardStyleButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"КЛАВИАТУРА: {KeyboardStyleNames[currentKeyboardStyle]}", keyboardStyleButton.transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
    }

    public void CycleSwitchType()
    {
        currentSwitchType = (currentSwitchType + 1) % 4;
        PlayerPrefs.SetInt("SelectedSwitchType", currentSwitchType);
        PlayerPrefs.Save();
        UpdateKeyboardCustomizationLabels();

        HapticFeedback.Vibrate(30);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTypingWithSwitch(currentSwitchType, false);
        }
        if (ClickJuice.Instance != null && keyboardSwitchButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"СВИТЧИ: {SwitchTypeNames[currentSwitchType]}", keyboardSwitchButton.transform.position, new Color(1f, 0.85f, 0.3f), true);
        }
    }

    private void ApplyKeyboardStyle(int style, bool animate)
    {
        if (keyboardBaseImage == null) return;
        Sprite targetSprite = sprKeyboardDefault;
        Color glowColor = new Color(0f, 0.89f, 1f, 0f);

        switch (style)
        {
            case 0: // Cyberpunk
                targetSprite = sprKeyboardDefault != null ? sprKeyboardDefault : keyboardBaseImage.sprite;
                glowColor = new Color(0f, 0.89f, 1f, 0f);
                break;
            case 1: // Retro IBM
                if (sprKeyboardRetro != null) targetSprite = sprKeyboardRetro;
                glowColor = new Color(1f, 0.7f, 0.2f, 0f);
                break;
            case 2: // Tokyo Neon
                if (sprKeyboardNeon != null) targetSprite = sprKeyboardNeon;
                glowColor = new Color(1f, 0.2f, 0.8f, 0f);
                break;
        }

        if (targetSprite != null) keyboardBaseImage.sprite = targetSprite;
        keyGlowNormal = glowColor;

        if (animate && keyboardTransform != null)
        {
            StartCoroutine(PopInRoutine(keyboardTransform));
        }
    }

    private void UpdateKeyboardCustomizationLabels()
    {
        if (keyboardStyleText != null)
        {
            keyboardStyleText.text = $"🎨 {KeyboardStyleNames[currentKeyboardStyle]}";
        }
        if (keyboardSwitchText != null)
        {
            keyboardSwitchText.text = $"🔊 {SwitchTypeNames[currentSwitchType]}";
        }
    }

    #endregion

    #region Кастомизация обоев и атмосферы комнаты

    private int currentRoomTheme = 0; // 0=Cozy Indie, 1=Cyberpunk, 2=Minimal
    private static readonly string[] RoomThemeNames = new string[]
    {
        "COZY INDIE",
        "CYBER LOFT",
        "MINIMAL TECH"
    };

    private void InitRoomThemeCustomization()
    {
        currentRoomTheme = PlayerPrefs.GetInt("SelectedRoomTheme", 0);
        ApplyRoomTheme(currentRoomTheme, false);

        if (roomThemeButton != null)
        {
            roomThemeButton.onClick.RemoveAllListeners();
            roomThemeButton.onClick.AddListener(CycleRoomTheme);
        }
    }

    public void CycleRoomTheme()
    {
        currentRoomTheme = (currentRoomTheme + 1) % 3;
        PlayerPrefs.SetInt("SelectedRoomTheme", currentRoomTheme);
        PlayerPrefs.Save();
        ApplyRoomTheme(currentRoomTheme, true);

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        if (ClickJuice.Instance != null && roomThemeButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏠 КОМНАТА: {RoomThemeNames[currentRoomTheme]}", roomThemeButton.transform.position, new Color(0.4f, 0.9f, 1f), true);
        }
    }

    private void ApplyRoomTheme(int theme, bool animate)
    {
        if (roomWallpaperImage == null) return;

        Sprite targetSprite = sprWallpaperCozy;
        if (theme == 1 && sprWallpaperCyberpunk != null) targetSprite = sprWallpaperCyberpunk;
        else if (theme == 2 && sprWallpaperMinimal != null) targetSprite = sprWallpaperMinimal;

        if (targetSprite != null)
        {
            roomWallpaperImage.sprite = targetSprite;
            roomWallpaperImage.color = Color.white;
        }

        if (roomThemeText != null)
        {
            roomThemeText.text = $"🏠 {RoomThemeNames[currentRoomTheme]}";
        }

        if (animate)
        {
            StartCoroutine(PopInRoutine(roomWallpaperImage.transform));
        }
    }

    #endregion

    #region Звуковые профили свитчей мыши (Mouse Switch Sound Profiles)

    private int currentMouseSwitchType = 0; // 0=Omron Classic, 1=Optical Gaming, 2=Silent Office
    private static readonly string[] MouseSwitchNames = new string[]
    {
        "ОМРОН КЛИК",
        "ОПТИКА ГЕЙМ",
        "ТИХИЙ ОФИС"
    };

    private void InitMouseSwitchCustomization()
    {
        currentMouseSwitchType = PlayerPrefs.GetInt("SelectedMouseSwitch", 0);
        if (mouseSwitchButton != null)
        {
            mouseSwitchButton.onClick.RemoveAllListeners();
            mouseSwitchButton.onClick.AddListener(CycleMouseSwitchType);
        }
        UpdateMouseSwitchLabel();
    }

    public void CycleMouseSwitchType()
    {
        currentMouseSwitchType = (currentMouseSwitchType + 1) % 3;
        PlayerPrefs.SetInt("SelectedMouseSwitch", currentMouseSwitchType);
        PlayerPrefs.Save();
        UpdateMouseSwitchLabel();

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMouseClickWithSwitch(currentMouseSwitchType);
        }
        if (ClickJuice.Instance != null && mouseSwitchButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🖱 МЫШЬ: {MouseSwitchNames[currentMouseSwitchType]}", mouseSwitchButton.transform.position, new Color(0.2f, 0.9f, 1f), true);
        }
    }

    private void UpdateMouseSwitchLabel()
    {
        if (mouseSwitchText != null)
        {
            mouseSwitchText.text = $"🖱 {MouseSwitchNames[currentMouseSwitchType]}";
        }
    }

    #endregion

    #region Интерактивные Питомцы и Компаньоны (Pets: Cat, Corgi, Robo)

    private int currentPetType = 0; // 0=Coder Cat, 1=Dev Corgi, 2=Robo-Dog
    private static readonly string[] PetTypeNames = new string[]
    {
        "КОТ-КОДЕР",
        "КОРГИ ДЕВ",
        "РОБО-ПЕС"
    };

    private void InitPetCompanionCustomization()
    {
        currentPetType = PlayerPrefs.GetInt("SelectedPetType", 0);
        ApplyPetType(currentPetType, false);

        if (petSelectorButton != null)
        {
            petSelectorButton.onClick.RemoveAllListeners();
            petSelectorButton.onClick.AddListener(CyclePetType);
        }
    }

    public void CyclePetType()
    {
        currentPetType = (currentPetType + 1) % 3;
        PlayerPrefs.SetInt("SelectedPetType", currentPetType);
        PlayerPrefs.Save();
        ApplyPetType(currentPetType, true);

        HapticFeedback.Vibrate(35);
        if (AudioManager.Instance != null)
        {
            if (currentPetType == 0) AudioManager.Instance.PlayCatPurr();
            else if (currentPetType == 1) AudioManager.Instance.PlayDogBark();
            else AudioManager.Instance.PlayRoboBeep();
        }

        if (ClickJuice.Instance != null && petSelectorButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🐾 ПИТОМЕЦ: {PetTypeNames[currentPetType]}", petSelectorButton.transform.position, new Color(1f, 0.85f, 0.3f), true);
        }
    }

    private void ApplyPetType(int petType, bool animate)
    {
        if (catImage == null) return;

        Sprite sleepSpr = catSleepingSprite;
        if (petType == 1 && sprCorgiSleep != null) sleepSpr = sprCorgiSleep;
        else if (petType == 2 && sprRoboSleep != null) sleepSpr = sprRoboSleep;

        if (sleepSpr != null) catImage.sprite = sleepSpr;

        if (petSelectorText != null)
        {
            petSelectorText.text = $"🐾 {PetTypeNames[currentPetType]}";
        }

        if (animate && catTransform != null)
        {
            StartCoroutine(PopInRoutine(catTransform));
        }
    }

    #endregion

    #region Кастомизация коврика (Desk Mat Skins)

    private int currentDeskMatSkin = 0; // 0=Default, 1=Felt, 2=Cyber, 3=Blueprint, 4=RGB
    private static readonly string[] DeskMatSkinNames = new string[]
    {
        "БАЗОВЫЙ",
        "ВОЙЛОК",
        "КИБЕР",
        "ЧЕРТЁЖ",
        "RGB GLOW"
    };

    private void InitDeskMatCustomization()
    {
        currentDeskMatSkin = PlayerPrefs.GetInt("SelectedDeskMatSkin", 0);
        ApplyDeskMatSkin(currentDeskMatSkin, false);

        if (deskMatSkinButton != null)
        {
            deskMatSkinButton.onClick.RemoveAllListeners();
            deskMatSkinButton.onClick.AddListener(CycleDeskMatSkin);
        }
    }

    public void CycleDeskMatSkin()
    {
        currentDeskMatSkin = (currentDeskMatSkin + 1) % 5;
        PlayerPrefs.SetInt("SelectedDeskMatSkin", currentDeskMatSkin);
        PlayerPrefs.Save();
        ApplyDeskMatSkin(currentDeskMatSkin, true);

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUpgrade();
        }

        if (ClickJuice.Instance != null && deskMatSkinButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🟪 КОВРИК: {DeskMatSkinNames[currentDeskMatSkin]}", deskMatSkinButton.transform.position, new Color(0.4f, 0.9f, 1f), true);
        }
    }

    private void ApplyDeskMatSkin(int skinIdx, bool animate)
    {
        if (deskMatImage != null)
        {
            Sprite targetSprite = sprDeskMatDefault;
            if (skinIdx == 1 && sprDeskMatFelt != null) targetSprite = sprDeskMatFelt;
            else if (skinIdx == 2 && sprDeskMatCyber != null) targetSprite = sprDeskMatCyber;
            else if (skinIdx == 3 && sprDeskMatBlueprint != null) targetSprite = sprDeskMatBlueprint;
            else if (skinIdx == 4 && sprDeskMatRGB != null) targetSprite = sprDeskMatRGB;

            if (targetSprite != null) deskMatImage.sprite = targetSprite;
        }

        if (deskMatSkinText != null)
        {
            deskMatSkinText.text = $"🟪 {DeskMatSkinNames[currentDeskMatSkin]}";
        }
    }

    #endregion
}
