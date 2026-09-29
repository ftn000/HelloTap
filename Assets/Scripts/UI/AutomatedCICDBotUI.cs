using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ИИ-Автотестирование и CI/CD Пайплайн («Automated CI/CD Bot»):
/// - 4 стадии пайплайна: Unit-тесты, ИИ-линтер, Матрица Docker, Zero-Downtime Auto-Deploy
/// - Интерактивный запуск конвейера «RUN PIPELINE» с проверкой стадий и наградами
/// - Уровни раннеров CI/CD: Локальный -> Кластерный -> Нейро-облако
/// - Перманентное ускорение релизов, предотвращение багов и прирост доходов
/// </summary>
public class AutomatedCICDBotUI : MonoBehaviour
{
    private static AutomatedCICDBotUI instance;
    public static AutomatedCICDBotUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AutomatedCICDBotUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(AutomatedCICDBotUI));
                    instance = go.AddComponent<AutomatedCICDBotUI>();
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
    public class PipelineStage
    {
        public string id;
        public string name;
        public string icon;
        public string description;
        public string perkDesc;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openCICDBtn;

    [Header("Конвейер и запуск пайплайна")]
    [SerializeField] private TMP_Text pipelineSummaryTxt;
    [SerializeField] private Button runPipelineBtn;
    [SerializeField] private TMP_Text runPipelineBtnTxt;
    [SerializeField] private Image pipelineProgressFill;
    [SerializeField] private TMP_Text pipelineStatusTxt;
    [SerializeField] private Transform stagesContainer;

    private readonly List<PipelineStage> stages = new List<PipelineStage>();
    private int runnerLevel = 1;
    private bool isPipelineRunning = false;
    private Coroutine pipelineCoroutine;

    private const string PrefRunnerLvl = "CICD_RunnerLvl";
    private const string PrefStagePrefix = "CICD_StageLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool IsPipelineRunning => isPipelineRunning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeStages();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeStages()
    {
        stages.Clear();

        stages.Add(new PipelineStage
        {
            id = "unit_tests",
            name = "Unit & Integration Tests",
            icon = "🧪",
            description = "Автоматические тесты игровой логики и физики",
            perkDesc = "+4% к качеству кода за уровень",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 5000,
            baseCostCode = 1200
        });

        stages.Add(new PipelineStage
        {
            id = "ai_linter",
            name = "AI Code Linter & Static Analysis",
            icon = "🔍",
            description = "Поиск утечек памяти и неоптимальных алгоритмов",
            perkDesc = "+3% строк кода за клик за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 12000,
            baseCostCode = 2500
        });

        stages.Add(new PipelineStage
        {
            id = "docker_build",
            name = "Docker Build Matrix (Multi-Arch)",
            icon = "🐳",
            description = "Параллельная сборка WebGL, Windows и Mobile билдов",
            perkDesc = "+5% к скорости релизов проектов за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 25000,
            baseCostCode = 6000
        });

        stages.Add(new PipelineStage
        {
            id = "auto_deploy",
            name = "Zero-Downtime Auto-Deploy",
            icon = "🚀",
            description = "Бесшовный релиз на прод без падения серверов",
            perkDesc = "+4% к пассивному доходу студии за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 50000,
            baseCostCode = 12000
        });
    }

    private void LoadData()
    {
        runnerLevel = PlayerPrefs.GetInt(PrefRunnerLvl, 1);
        for (int i = 0; i < stages.Count; i++)
        {
            int defLvl = stages[i].id == "unit_tests" ? 1 : 0;
            stages[i].level = PlayerPrefs.GetInt(PrefStagePrefix + stages[i].id, defLvl);
            stages[i].level = Mathf.Clamp(stages[i].level, 0, stages[i].maxLevel);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefRunnerLvl, runnerLevel);
        for (int i = 0; i < stages.Count; i++)
        {
            PlayerPrefs.SetInt(PrefStagePrefix + stages[i].id, stages[i].level);
        }
        PlayerPrefs.Save();
    }

    public double GetCICDIncomeMultiplier()
    {
        var deploy = stages.Find(s => s.id == "auto_deploy");
        int dLvl = deploy != null ? deploy.level : 0;
        return 1.0 + (dLvl * 0.04) + (runnerLevel - 1) * 0.05;
    }

    public double GetCICDClickMultiplier()
    {
        var linter = stages.Find(s => s.id == "ai_linter");
        int lLvl = linter != null ? linter.level : 0;
        return 1.0 + (lLvl * 0.03);
    }

    public double GetCICDReleaseSpeedMultiplier()
    {
        var dock = stages.Find(s => s.id == "docker_build");
        int dkLvl = dock != null ? dock.level : 0;
        return 1.0 + (dkLvl * 0.05);
    }

    public int GetStageLevel(string id)
    {
        var s = stages.Find(x => x.id == id);
        return s != null ? s.level : 0;
    }

    public void RunPipeline()
    {
        if (isPipelineRunning) return;
        if (pipelineCoroutine != null) StopCoroutine(pipelineCoroutine);
        pipelineCoroutine = StartCoroutine(PipelineExecutionRoutine());
    }

    private IEnumerator PipelineExecutionRoutine()
    {
        isPipelineRunning = true;
        if (runPipelineBtn != null) runPipelineBtn.interactable = false;

        string[] stageMessages = new string[]
        {
            "🧪 Стадия 1/4: Запуск Unit-тестов...",
            "🔍 Стадия 2/4: ИИ-анализ архитектуры и линтинг...",
            "🐳 Стадия 3/4: Сборка Docker контейнеров WebGL...",
            "🚀 Стадия 4/4: Zero-Downtime деплой на прод-серверы..."
        };

        float totalDuration = Mathf.Max(2.5f, 5.0f - (runnerLevel * 0.8f));
        float stageTime = totalDuration / 4f;

        for (int i = 0; i < 4; i++)
        {
            if (pipelineStatusTxt != null)
            {
                pipelineStatusTxt.text = stageMessages[i];
            }
            if (pipelineProgressFill != null)
            {
                pipelineProgressFill.fillAmount = (float)i / 4f;
            }

            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

            yield return new WaitForSecondsRealtime(stageTime);
        }

        if (pipelineProgressFill != null) pipelineProgressFill.fillAmount = 1f;
        if (pipelineStatusTxt != null)
        {
            pipelineStatusTxt.text = "✅ <color=#00FF88>PIPELINE PASSED! Все 4 стадии завершены успешно.</color>";
        }

        // Награда за успешный билд
        double baseGrant = Math.Max(5000.0, (GameManager.Instance != null ? GameManager.Instance.Money * 0.05 : 10000.0));
        double rewardMoney = Math.Floor(baseGrant * (1.0 + runnerLevel * 0.25));
        double rewardCode = Math.Floor(rewardMoney * 0.08);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🤖 CI/CD БИЛД ЗЕЛЕНЫЙ!\nНаграда: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} кода)", transform.position, new Color(0.1f, 1f, 0.5f), true);
        }

        isPipelineRunning = false;
        if (runPipelineBtn != null) runPipelineBtn.interactable = true;
        UpdateModalUI();
    }

    public bool TryUpgradeStage(string stageId)
    {
        var stage = stages.Find(s => s.id == stageId);
        if (stage == null || stage.level >= stage.maxLevel) return false;

        double costMoney = stage.GetCostMoney();
        double costCode = stage.GetCostCode();

        if (GameManager.Instance == null || GameManager.Instance.Money < costMoney || GameManager.Instance.CodeLines < costCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(costMoney)} ₽ и {NumberFormatter.Format(costCode)} кода!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendMoney(costMoney);
        GameManager.Instance.SpendLinesOfCode(costCode);

        stage.level++;
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛠️ СТАДИЯ CI/CD УЛУЧШЕНА!\n{stage.icon} {stage.name} -> ур. {stage.level}", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        UpdateModalUI();
        return true;
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
        if (openCICDBtn != null)
        {
            openCICDBtn.onClick.RemoveAllListeners();
            openCICDBtn.onClick.AddListener(OpenModal);
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
        if (runPipelineBtn != null)
        {
            runPipelineBtn.onClick.RemoveAllListeners();
            runPipelineBtn.onClick.AddListener(RunPipeline);
        }
    }

    private void UpdateModalUI()
    {
        if (pipelineSummaryTxt != null)
        {
            double speedB = (GetCICDReleaseSpeedMultiplier() - 1.0) * 100.0;
            double incB = (GetCICDIncomeMultiplier() - 1.0) * 100.0;
            double clkB = (GetCICDClickMultiplier() - 1.0) * 100.0;
            pipelineSummaryTxt.text = $"Раннер: <b><color=#00E5FF>Ранг {runnerLevel}</color></b> | Скорость релизов: <b><color=#00FF88>+{speedB:F0}%</color></b> | Доход: <b><color=#00E5FF>+{incB:F0}%</color></b> | Клик: <b><color=#FFD700>+{clkB:F0}%</color></b>";
        }

        RefreshStageCards();
    }

    private void RefreshStageCards()
    {
        if (stagesContainer == null) return;

        for (int i = 0; i < stages.Count; i++)
        {
            var st = stages[i];
            Transform cardTr = stagesContainer.Find($"StageCard_{st.id}");
            if (cardTr == null) continue;

            TMP_Text lvlTxt = cardTr.Find("LevelTxt")?.GetComponent<TMP_Text>();
            if (lvlTxt != null)
            {
                lvlTxt.text = st.level >= st.maxLevel ? "<color=#FFD700>МАКС</color>" : $"Ур. {st.level}/{st.maxLevel}";
            }

            Button upgBtn = cardTr.Find("UpgradeBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = upgBtn != null ? upgBtn.GetComponentInChildren<TMP_Text>() : null;

            if (upgBtn != null && btnTxt != null)
            {
                string sId = st.id;
                upgBtn.onClick.RemoveAllListeners();
                upgBtn.onClick.AddListener(() => TryUpgradeStage(sId));

                if (st.level >= st.maxLevel)
                {
                    upgBtn.interactable = false;
                    btnTxt.text = "МАКС. УРОВЕНЬ";
                }
                else
                {
                    double costMoney = st.GetCostMoney();
                    double costCode = st.GetCostCode();
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= costMoney &&
                                     GameManager.Instance.CodeLines >= costCode;
                    upgBtn.interactable = canAfford;
                    btnTxt.text = $"ПРОКАЧАТЬ\n{NumberFormatter.Format(costMoney)} ₽ | {NumberFormatter.Format(costCode)} Кода";
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
        GameObject root = new GameObject("AutomatedCICDModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.8f, 0.9f, 0.6f);
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
        headerTxt.text = "🤖 ИИ-АВТОТЕСТИРОВАНИЕ И CI/CD BOT";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 0.85f, 1f);

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

        // Pipeline Status Box
        GameObject statusBox = new GameObject("StatusBox", typeof(RectTransform), typeof(Image));
        statusBox.transform.SetParent(cardObj.transform, false);
        RectTransform statRt = statusBox.GetComponent<RectTransform>();
        statRt.anchorMin = new Vector2(0, 1);
        statRt.anchorMax = new Vector2(1, 1);
        statRt.pivot = new Vector2(0.5f, 1);
        statRt.anchoredPosition = new Vector2(0, -52);
        statRt.sizeDelta = new Vector2(-36, 76);
        statusBox.GetComponent<Image>().color = new Color(0.1f, 0.13f, 0.2f, 0.9f);

        GameObject sumTxtObj = new GameObject("Summary", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(statusBox.transform, false);
        RectTransform sumRt = sumTxtObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -6);
        sumRt.sizeDelta = new Vector2(-16, 20);
        pipelineSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        pipelineSummaryTxt.fontSize = 10;
        pipelineSummaryTxt.alignment = TextAlignmentOptions.Center;
        pipelineSummaryTxt.color = new Color(0.85f, 0.95f, 1f);

        GameObject pipeTxtObj = new GameObject("StatusTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        pipeTxtObj.transform.SetParent(statusBox.transform, false);
        RectTransform pipeRt = pipeTxtObj.GetComponent<RectTransform>();
        pipeRt.anchorMin = new Vector2(0, 0);
        pipeRt.anchorMax = new Vector2(1, 0);
        pipeRt.pivot = new Vector2(0.5f, 0);
        pipeRt.anchoredPosition = new Vector2(0, 8);
        pipeRt.sizeDelta = new Vector2(-16, 24);
        pipelineStatusTxt = pipeTxtObj.GetComponent<TextMeshProUGUI>();
        pipelineStatusTxt.fontSize = 11;
        pipelineStatusTxt.alignment = TextAlignmentOptions.Center;
        pipelineStatusTxt.text = "🟢 Конвейер готов к сборке и тестированию";
        pipelineStatusTxt.color = Color.white;

        // Run Pipeline Action Button
        GameObject runObj = new GameObject("RunPipelineBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        runObj.transform.SetParent(cardObj.transform, false);
        RectTransform runRt = runObj.GetComponent<RectTransform>();
        runRt.anchorMin = new Vector2(0, 1);
        runRt.anchorMax = new Vector2(1, 1);
        runRt.pivot = new Vector2(0.5f, 1);
        runRt.anchoredPosition = new Vector2(0, -136);
        runRt.sizeDelta = new Vector2(-36, 40);
        runObj.GetComponent<Image>().color = new Color(0.1f, 0.55f, 0.45f, 0.95f);
        runPipelineBtn = runObj.GetComponent<Button>();

        GameObject runTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        runTxtObj.transform.SetParent(runObj.transform, false);
        RectTransform runTxtRt = runTxtObj.GetComponent<RectTransform>();
        runTxtRt.anchorMin = Vector2.zero; runTxtRt.anchorMax = Vector2.one; runTxtRt.sizeDelta = Vector2.zero;
        runPipelineBtnTxt = runTxtObj.GetComponent<TextMeshProUGUI>();
        runPipelineBtnTxt.fontSize = 11;
        runPipelineBtnTxt.fontStyle = FontStyles.Bold;
        runPipelineBtnTxt.alignment = TextAlignmentOptions.Center;
        runPipelineBtnTxt.text = "▶ ЗАПУСТИТЬ CI/CD ПАЙПЛАЙН (TEST & DEPLOY)";
        runPipelineBtnTxt.color = Color.white;

        // Scroll Container for Stages
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -186);
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
        stagesContainer = contentObj.transform;

        for (int i = 0; i < stages.Count; i++)
        {
            CreateStageCardTemplate(stagesContainer, stages[i]);
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

    private void CreateStageCardTemplate(Transform parent, PipelineStage stage)
    {
        GameObject card = new GameObject($"StageCard_{stage.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 72);
        card.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

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
        tt.text = $"{stage.icon} <b>{stage.name}</b>";
        tt.fontSize = 12;
        tt.color = new Color(0.9f, 0.95f, 1f);

        // Level text
        GameObject lvlObj = new GameObject("LevelTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlObj.transform.SetParent(card.transform, false);
        RectTransform lvlRt = lvlObj.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0.65f, 1);
        lvlRt.anchorMax = new Vector2(1, 1);
        lvlRt.pivot = new Vector2(1, 1);
        lvlRt.anchoredPosition = new Vector2(-10, -6);
        lvlRt.sizeDelta = new Vector2(0, 20);
        TMP_Text lt = lvlObj.GetComponent<TextMeshProUGUI>();
        lt.text = $"Ур. {stage.level}/{stage.maxLevel}";
        lt.fontSize = 11;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = new Color(0.2f, 0.85f, 1f);

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.62f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{stage.perkDesc}</color>";
        dt.fontSize = 10;

        // Upgrade button
        GameObject b = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, -2);
        brt.sizeDelta = new Vector2(150, 36);
        b.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.65f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ПРОКАЧАТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openCICDBtn != null) return;

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

        GameObject btnGo = new GameObject("CICDHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.45f, 0.9f);
        openCICDBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🤖 CI/CD";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
