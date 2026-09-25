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
    [SerializeField] private Color keyGlowNormal = new Color(0, 0.89f, 1f, 0f);
    [SerializeField] private Color keyGlowActive = new Color(0, 1f, 0.55f, 0.35f);

    [Header("Предметы на столе (Визуальная прогрессия)")]
    [SerializeField] private Transform mouseTransform;
    [SerializeField] private Transform coffeeMugTransform;
    [SerializeField] private Transform energyCanTransform;
    [SerializeField] private Transform catTransform;

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

    // Состояние охоты на баги
    private float bugSpawnTimer = 18f;
    private float bugActiveTimer = 0f;
    private int bugHp = 0;
    private const int MaxBugHp = 5;
    private const float BugLifetime = 8.5f;

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
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void Start()
    {
        SubscribeEvents();
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

        // 2. Дыхание кота (если открыт)
        if (catTransform != null && catTransform.gameObject.activeSelf)
        {
            float breath = 1f + Mathf.Sin(Time.time * 2.5f) * 0.025f;
            catTransform.localScale = new Vector3(breath, 2f - breath, 1f);
        }

        // 3. Обновление шкалы комбо "В Потоке"
        UpdateComboVisuals();

        // 4. Таймер спавна и жизни Бага на мониторе
        UpdateBugHunt(dt);
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
        bool hasCat = GameManager.Instance.GetUpgradeLevel("staff_cat") > 0;
        bool hasCoffee = GameManager.Instance.GetUpgradeLevel("staff_script") > 0 || GameManager.Instance.TotalCodeWritten >= 30;
        bool hasMonitor2 = GameManager.Instance.GetUpgradeLevel("hw_monitor2") > 0;
        bool hasEnergy = GameManager.Instance.IsBoostActive || GameManager.Instance.TotalMoneyEarned > 0;

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
        if (nextProj != null && GameManager.Instance.TryReleaseProject(nextProj.Id))
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
        if (bugAlertButton == null) return;

        if (bugHp > 0)
        {
            bugActiveTimer -= dt;
            if (bugAlertText != null)
            {
                bugAlertText.text = $"[ ! ] БАГ НА ПРОДЕ! Тапни ({bugHp}) [{bugActiveTimer:F1}с]";
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
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBugHit(false);
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"ФИКС БАГА! Осталось: {bugHp}", pos, new Color(1f, 0.55f, 0.2f), false);
            }
            StartCoroutine(PopInRoutine(bugAlertButton.transform));
        }
        else
        {
            bugAlertButton.gameObject.SetActive(false);
            bugSpawnTimer = Random.Range(25f, 40f);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayBugHit(true);
            GameManager.Instance.ClaimBugFixReward(pos, out double bonusCode, out double bonusMoney);

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup(
                    $"БАГ УСТРАНЕН! +{NumberFormatter.Format(bonusCode)} кода | +{NumberFormatter.Format(bonusMoney)} руб.",
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

        while (terminalHistory.Count > maxVisibleLines)
        {
            terminalHistory.RemoveAt(0);
        }
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

        if (coffeeMugTransform != null && coffeeMugTransform.gameObject.activeSelf)
        {
            coffeeMugTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-3f, 3f));
        }

        UpdateComboVisuals();
        RefreshProgressAndComboUI();
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
}
