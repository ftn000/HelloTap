using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Интерактивная мини-игра сборки проекта перед релизом:
/// - Прогресс-бар компиляции (0% -> 100%)
/// - Ускорение компиляции тапами по экрану
/// - Падающие баги (swat bugs for bonus code/money)
/// - Падающие ящики с патчами (catch packages to boost final release reward)
/// - Звуки squashing, package chime и victory fanfare
/// </summary>
public class ProjectBuildMiniGame : MonoBehaviour
{
    private static ProjectBuildMiniGame instance;
    public static ProjectBuildMiniGame Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ProjectBuildMiniGame>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ProjectBuildMiniGame");
                    Canvas canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null) go.transform.SetParent(canvas.transform, false);
                    instance = go.AddComponent<ProjectBuildMiniGame>();
                }
            }
            return instance;
        }
    }

    [Header("UI элементы мини-игры")]
    [SerializeField] private GameObject miniGameRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusLogText;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TMP_Text progressPercentText;
    [SerializeField] private Button tapAreaButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private RectTransform fallingItemsContainer;

    [Header("Спрайты")]
    [SerializeField] private Sprite bugSprite;
    [SerializeField] private Sprite crateSprite;

    private bool isPlaying = false;
    private float currentProgress = 0f;
    private GameProjectData currentProject;
    private Action onCompleteCallback;
    private float bonusMultiplier = 1.0f;
    private int bugsSquashed = 0;
    private int cratesCollected = 0;

    private readonly List<GameObject> activeFallingItems = new List<GameObject>();
    private Coroutine gameLoopCoroutine;
    private Coroutine spawnItemsCoroutine;

    private readonly string[] compileSteps = new string[]
    {
        "[1/4] Компиляция C# скриптов и IL2CPP...",
        "[2/4] Запекание шейдеров и сжатие текстур...",
        "[3/4] Оптимизация сборки и устранение утечек...",
        "[4/4] Финальная упаковка релизного билда..."
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (miniGameRoot != null)
        {
            miniGameRoot.SetActive(false);
        }
        BindButtons();
    }

    private void BindButtons()
    {
        if (tapAreaButton != null)
        {
            tapAreaButton.onClick.RemoveAllListeners();
            tapAreaButton.onClick.AddListener(OnTapAccelerate);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }
    }

    public void StartBuildMiniGame(GameProjectData project, Action onComplete)
    {
        if (isPlaying) return;
        currentProject = project;
        onCompleteCallback = onComplete;
        currentProgress = 0f;
        bonusMultiplier = 1.0f;
        bugsSquashed = 0;
        cratesCollected = 0;
        isPlaying = true;

        EnsureUIExists();

        if (miniGameRoot != null)
        {
            miniGameRoot.SetActive(true);
            miniGameRoot.transform.SetAsLastSibling();
        }

        if (titleText != null)
        {
            titleText.text = $"⚡ СБОРКА ПРОЕКТА: {project.Title}";
        }

        ClearFallingItems();
        UpdateUI();

        if (gameLoopCoroutine != null) StopCoroutine(gameLoopCoroutine);
        if (spawnItemsCoroutine != null) StopCoroutine(spawnItemsCoroutine);

        gameLoopCoroutine = StartCoroutine(BuildGameLoop());
        spawnItemsCoroutine = StartCoroutine(SpawnFallingItemsRoutine());
    }

    private void EnsureUIExists()
    {
        if (miniGameRoot != null) return;

        // Динамическое создание UI модального окна сборки, если оно не было заранее создано в сцене
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();

        GameObject root = new GameObject("ProjectBuildMiniGameModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Темный фон бэкдропа
        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.06f, 0.10f, 0.92f);

        // Кнопка ускорения кликом по всему фону
        tapAreaButton = root.AddComponent<Button>();
        tapAreaButton.transition = Selectable.Transition.None;
        tapAreaButton.onClick.AddListener(OnTapAccelerate);

        // Контейнер окна
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(500, 360);
        panel.GetComponent<Image>().color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        // Заголовок
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchoredPosition = new Vector2(0, 135);
        titleRt.sizeDelta = new Vector2(460, 40);
        titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.fontSize = 20;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0f, 0.9f, 1f);

        // Лог сборки
        GameObject logObj = new GameObject("LogText", typeof(RectTransform), typeof(TextMeshProUGUI));
        logObj.transform.SetParent(panel.transform, false);
        RectTransform logRt = logObj.GetComponent<RectTransform>();
        logRt.anchoredPosition = new Vector2(0, 80);
        logRt.sizeDelta = new Vector2(460, 45);
        statusLogText = logObj.GetComponent<TextMeshProUGUI>();
        statusLogText.fontSize = 15;
        statusLogText.alignment = TextAlignmentOptions.Center;
        statusLogText.color = new Color(0.75f, 0.85f, 0.95f);

        // Подсказка
        GameObject hintObj = new GameObject("HintText", typeof(RectTransform), typeof(TextMeshProUGUI));
        hintObj.transform.SetParent(panel.transform, false);
        RectTransform hintRt = hintObj.GetComponent<RectTransform>();
        hintRt.anchoredPosition = new Vector2(0, 30);
        hintRt.sizeDelta = new Vector2(460, 30);
        var hint = hintObj.GetComponent<TextMeshProUGUI>();
        hint.fontSize = 13;
        hint.alignment = TextAlignmentOptions.Center;
        hint.text = "Тапай по экрану для ускорения! Лови 📦 патчи и дави 🐛 баги!";
        hint.color = new Color(1f, 0.85f, 0.35f);

        // Прогресс бар фон
        GameObject barBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(panel.transform, false);
        RectTransform barBgRt = barBg.GetComponent<RectTransform>();
        barBgRt.anchoredPosition = new Vector2(0, -35);
        barBgRt.sizeDelta = new Vector2(440, 28);
        barBg.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 1f);

        // Прогресс бар заливка
        GameObject barFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform barFillRt = barFill.GetComponent<RectTransform>();
        barFillRt.anchorMin = Vector2.zero;
        barFillRt.anchorMax = Vector2.one;
        barFillRt.offsetMin = new Vector2(3, 3);
        barFillRt.offsetMax = new Vector2(-3, -3);
        progressBarFill = barFill.GetComponent<Image>();
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.color = new Color(0f, 0.9f, 0.55f);

        // Процент текста
        GameObject pctObj = new GameObject("PctText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pctObj.transform.SetParent(barBg.transform, false);
        RectTransform pctRt = pctObj.GetComponent<RectTransform>();
        pctRt.sizeDelta = new Vector2(440, 28);
        progressPercentText = pctObj.GetComponent<TextMeshProUGUI>();
        progressPercentText.fontSize = 15;
        progressPercentText.fontStyle = FontStyles.Bold;
        progressPercentText.alignment = TextAlignmentOptions.Center;
        progressPercentText.color = Color.white;

        // Контейнер для падающих предметов
        GameObject fallingCont = new GameObject("FallingItemsContainer", typeof(RectTransform));
        fallingCont.transform.SetParent(root.transform, false);
        fallingItemsContainer = fallingCont.GetComponent<RectTransform>();
        fallingItemsContainer.anchorMin = Vector2.zero;
        fallingItemsContainer.anchorMax = Vector2.one;
        fallingItemsContainer.offsetMin = Vector2.zero;
        fallingItemsContainer.offsetMax = Vector2.zero;

        // Кнопка Пропустить
        GameObject skipObj = new GameObject("SkipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        skipObj.transform.SetParent(panel.transform, false);
        RectTransform skipRt = skipObj.GetComponent<RectTransform>();
        skipRt.anchoredPosition = new Vector2(0, -115);
        skipRt.sizeDelta = new Vector2(160, 36);
        skipObj.GetComponent<Image>().color = new Color(0.25f, 0.28f, 0.38f);
        skipButton = skipObj.GetComponent<Button>();
        skipButton.onClick.AddListener(OnSkipClicked);

        GameObject skipLbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        skipLbl.transform.SetParent(skipObj.transform, false);
        RectTransform skipLblRt = skipLbl.GetComponent<RectTransform>();
        skipLblRt.sizeDelta = new Vector2(160, 36);
        var lbl = skipLbl.GetComponent<TextMeshProUGUI>();
        lbl.text = "Пропустить >>";
        lbl.fontSize = 14;
        lbl.alignment = TextAlignmentOptions.Center;
        lbl.color = Color.white;

        miniGameRoot = root;
    }

    private void UpdateUI()
    {
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentProgress;
        }

        if (progressPercentText != null)
        {
            progressPercentText.text = $"{Mathf.Clamp01(currentProgress) * 100f:F0}%";
        }

        if (statusLogText != null)
        {
            int stepIndex = Mathf.Clamp((int)(currentProgress * compileSteps.Length), 0, compileSteps.Length - 1);
            statusLogText.text = compileSteps[stepIndex];
        }
    }

    private void OnTapAccelerate()
    {
        if (!isPlaying) return;

        // Тап дает +4% прогресса сборки
        currentProgress = Mathf.Min(1.0f, currentProgress + 0.045f);
        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTyping();

        if (ClickJuice.Instance != null)
        {
            Vector2 mousePos = Input.mousePosition;
            ClickJuice.Instance.SpawnCustomPopup("+4% BUILD SPEED!", mousePos, new Color(0f, 0.9f, 1f), false);
        }

        UpdateUI();

        if (currentProgress >= 1.0f)
        {
            FinishBuild();
        }
    }

    private void OnSkipClicked()
    {
        if (!isPlaying) return;
        currentProgress = 1.0f;
        FinishBuild();
    }

    private IEnumerator BuildGameLoop()
    {
        // Базовое время компиляции без тапов ~5.5 секунд
        float duration = 5.5f;
        float elapsed = 0f;

        while (isPlaying && currentProgress < 1.0f)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            currentProgress = Mathf.Min(1.0f, currentProgress + (dt / duration));
            UpdateUI();
            yield return null;
        }

        if (isPlaying && currentProgress >= 1.0f)
        {
            FinishBuild();
        }
    }

    private IEnumerator SpawnFallingItemsRoutine()
    {
        yield return new WaitForSeconds(0.6f);

        while (isPlaying && currentProgress < 0.90f)
        {
            // С шансом 50% спавним баг или ящик
            bool isBug = UnityEngine.Random.value > 0.45f;
            SpawnItem(isBug);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.85f, 1.45f));
        }
    }

    private void SpawnItem(bool isBug)
    {
        if (fallingItemsContainer == null && miniGameRoot != null)
        {
            fallingItemsContainer = miniGameRoot.GetComponent<RectTransform>();
        }
        if (fallingItemsContainer == null) return;

        GameObject item = new GameObject(isBug ? "FallingBug" : "FallingCrate", typeof(RectTransform), typeof(Image), typeof(Button));
        item.transform.SetParent(fallingItemsContainer, false);
        activeFallingItems.Add(item);

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(56, 56);
        float randX = UnityEngine.Random.Range(-180f, 180f);
        rt.anchoredPosition = new Vector2(randX, 220f);

        Image img = item.GetComponent<Image>();
        if (isBug)
        {
            img.sprite = bugSprite;
            if (bugSprite == null) img.color = new Color(1f, 0.2f, 0.35f);
        }
        else
        {
            img.sprite = crateSprite;
            if (crateSprite == null) img.color = new Color(0.2f, 0.8f, 1f);
        }

        Button btn = item.GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            if (isBug) OnSquashBug(item);
            else OnCollectCrate(item);
        });

        StartCoroutine(FallRoutine(item, rt, UnityEngine.Random.Range(180f, 260f)));
    }

    private IEnumerator FallRoutine(GameObject item, RectTransform rt, float speed)
    {
        while (item != null && rt != null && rt.anchoredPosition.y > -260f)
        {
            rt.anchoredPosition -= new Vector2(0, speed * Time.deltaTime);
            yield return null;
        }

        if (item != null)
        {
            activeFallingItems.Remove(item);
            Destroy(item);
        }
    }

    private void OnSquashBug(GameObject bugObj)
    {
        if (!isPlaying || bugObj == null) return;
        activeFallingItems.Remove(bugObj);
        Vector3 pos = bugObj.transform.position;
        Destroy(bugObj);

        bugsSquashed++;
        HapticFeedback.Vibrate(40);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBugSquash();

        double bonusMoney = currentProject != null ? Math.Max(25, currentProject.RewardMoney * 0.05) : 50;
        double bonusCode = currentProject != null ? Math.Max(10, currentProject.RequiredCodeLines * 0.04) : 20;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(bonusCode, bonusMoney);
        }

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🐛 БАГ УСТРАНЕН! +{NumberFormatter.Format(bonusMoney)} руб.", pos, new Color(1f, 0.3f, 0.4f), true);
        }
    }

    private void OnCollectCrate(GameObject crateObj)
    {
        if (!isPlaying || crateObj == null) return;
        activeFallingItems.Remove(crateObj);
        Vector3 pos = crateObj.transform.position;
        Destroy(crateObj);

        cratesCollected++;
        bonusMultiplier += 0.10f; // +10% к награде за каждый пойманный патч!
        HapticFeedback.Vibrate(30);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCrateCollect();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📦 ПАТЧ ПОЙМАН! +10% к награде! (x{bonusMultiplier:F1})", pos, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    private void FinishBuild()
    {
        if (!isPlaying) return;
        isPlaying = false;

        if (gameLoopCoroutine != null) StopCoroutine(gameLoopCoroutine);
        if (spawnItemsCoroutine != null) StopCoroutine(spawnItemsCoroutine);
        ClearFallingItems();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayBuildComplete();

        // Выполняем релиз
        onCompleteCallback?.Invoke();

        // Если игрок собрал патчи, начисляем бонусный мультипликатор!
        if (bonusMultiplier > 1.01f && currentProject != null && GameManager.Instance != null)
        {
            double extraReward = currentProject.RewardMoney * (bonusMultiplier - 1.0f);
            GameManager.Instance.AddDirectCurrencies(0, extraReward);
            if (ClickJuice.Instance != null && miniGameRoot != null)
            {
                ClickJuice.Instance.SpawnCustomPopup(
                    $"🎉 БОНУС МИНИ-ИГРЫ ({cratesCollected} патчей): +{NumberFormatter.Format(extraReward)} руб.!",
                    miniGameRoot.transform.position,
                    new Color(0.2f, 1f, 0.6f),
                    true);
            }
        }

        StartCoroutine(CloseModalAfterDelay(1.2f));
    }

    private IEnumerator CloseModalAfterDelay(float delay)
    {
        if (statusLogText != null)
        {
            statusLogText.text = $"🚀 БИЛД УСПЕШНО СОБРАН И ОПУБЛИКОВАН!";
            statusLogText.color = new Color(0.2f, 1f, 0.5f);
        }

        yield return new WaitForSeconds(delay);

        if (miniGameRoot != null)
        {
            miniGameRoot.SetActive(false);
        }
    }

    private void ClearFallingItems()
    {
        foreach (var it in activeFallingItems)
        {
            if (it != null) Destroy(it);
        }
        activeFallingItems.Clear();
    }
}
