using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Инкубатор ИИ-Агентов для Авто-Разработки («Autonomous AI Coding Agents Rig»):
/// - Развертывание локальных LLM-моделей студии:
///   1. 🤖 CodeLlama 7B (Junior AI Assistant) — базовые правки и рефакторинг
///   2. ⚡ Mistral Dev 14B (Feature Coder) — генерация игровых фичей и скриптов
///   3. 🧠 DeepSeek Coder 33B (Senior AI Architect) — проектирование подсистем и оптимизация
///   4. 🌌 Quantum Synthetic Super-Agent 70B — полностью автономный стек разработки
/// - Пассивный приток строк кода в секунду от работающих нейросетевых агентов
/// - Интерактивное действие: «🚀 ЗАПУСТИТЬ АВТО-СПРИНТ ИИ-АГЕНТОВ» (взрыв кода + комбо)
/// - Множители производительности к клику и общему доходу студии
/// </summary>
public class AutonomousAiAgentsUI : MonoBehaviour
{
    private static AutonomousAiAgentsUI instance;
    public static AutonomousAiAgentsUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AutonomousAiAgentsUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(AutonomousAiAgentsUI));
                    instance = go.AddComponent<AutonomousAiAgentsUI>();
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
    public class AiAgentModel
    {
        public string id;
        public string title;
        public string modelFamily;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double codePerSecBonus;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public double GetTotalCps() => codePerSecBonus * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openAiAgentsBtn;

    [Header("Авто-спринт и модели")]
    [SerializeField] private TMP_Text aiStatsSummaryTxt;
    [SerializeField] private Button autoSprintBtn;
    [SerializeField] private TMP_Text autoSprintBtnTxt;
    [SerializeField] private Transform modelsContainer;

    private readonly List<AiAgentModel> agents = new List<AiAgentModel>();
    private bool isSprintActive = false;
    private int autoSprintsCompleted = 0;

    private const string PrefAgentLvlPrefix = "AiAgent_Lvl_";
    private const string PrefSprintsCount = "AiAgent_SprintsCount";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeAgents();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeAgents()
    {
        if (agents.Count > 0) return;

        agents.Add(new AiAgentModel
        {
            id = "agent_codellama",
            title = "CodeLlama 7B (Junior AI)",
            modelFamily = "Авто-исправление багов и автодополнение синтаксиса",
            icon = "🤖",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 40000,
            baseCostCode = 18000,
            codePerSecBonus = 40.0,
            incomeMultiplierBonus = 0.06f,
            clickMultiplierBonus = 0.05f
        });

        agents.Add(new AiAgentModel
        {
            id = "agent_mistral",
            title = "Mistral Dev 14B (Feature Coder)",
            modelFamily = "Генерация шейдеров, физики и логики уровней",
            icon = "⚡",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 140000,
            baseCostCode = 70000,
            codePerSecBonus = 120.0,
            incomeMultiplierBonus = 0.12f,
            clickMultiplierBonus = 0.08f
        });

        agents.Add(new AiAgentModel
        {
            id = "agent_deepseek",
            title = "DeepSeek Coder 33B (Architect)",
            modelFamily = "Проектирование ECS архитектуры и бенчмаркинг",
            icon = "🧠",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 450000,
            baseCostCode = 225000,
            codePerSecBonus = 350.0,
            incomeMultiplierBonus = 0.20f,
            clickMultiplierBonus = 0.15f
        });

        agents.Add(new AiAgentModel
        {
            id = "agent_quantum_super",
            title = "Quantum Super-Agent 70B",
            modelFamily = "Полная автономная разработка, тестирование и релиз",
            icon = "🌌",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1500000,
            baseCostCode = 750000,
            codePerSecBonus = 1000.0,
            incomeMultiplierBonus = 0.32f,
            clickMultiplierBonus = 0.25f
        });
    }

    private void LoadData()
    {
        autoSprintsCompleted = PlayerPrefs.GetInt(PrefSprintsCount, 0);
        foreach (var a in agents)
        {
            if (PlayerPrefs.HasKey(PrefAgentLvlPrefix + a.id))
            {
                a.level = PlayerPrefs.GetInt(PrefAgentLvlPrefix + a.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSprintsCount, autoSprintsCompleted);
        foreach (var a in agents)
        {
            PlayerPrefs.SetInt(PrefAgentLvlPrefix + a.id, a.level);
        }
        PlayerPrefs.Save();
    }

    public double GetAiIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var a in agents)
        {
            mult += a.level * a.incomeMultiplierBonus;
        }
        mult += Math.Min(autoSprintsCompleted * 0.015, 0.60);
        return mult;
    }

    public double GetAiClickMultiplier()
    {
        double mult = 1.0;
        foreach (var a in agents)
        {
            mult += a.level * a.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetAiCodePerSec()
    {
        double sum = 0;
        foreach (var a in agents)
        {
            sum += a.GetTotalCps();
        }
        return sum;
    }

    public void StartAutoSprint()
    {
        if (isSprintActive) return;
        StartCoroutine(AutoSprintRoutine());
    }

    private IEnumerator AutoSprintRoutine()
    {
        isSprintActive = true;
        if (autoSprintBtn != null) autoSprintBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (autoSprintBtnTxt != null)
        {
            autoSprintBtnTxt.text = "🤖 АГЕНТЫ РЕШАЮТ ЗАДАЧИ БЭКЛОГА И ДЕЛАЮТ КОММИТЫ...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        autoSprintsCompleted++;
        double baseCode = Math.Max(25000.0, GetAiCodePerSec() * 45.0);
        double rewardCode = Math.Floor(baseCode * GetAiClickMultiplier());
        double rewardMoney = Math.Floor(rewardCode * 1.5 * GetAiIncomeMultiplier());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddComboEnergy(0.35f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🤖 СПРИНТ ЗАВЕРШЕН!\nСгенерировано: <b>+{NumberFormatter.Format(rewardCode)} C#</b> (+{NumberFormatter.Format(rewardMoney)} ₽)", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        isSprintActive = false;
        if (autoSprintBtn != null) autoSprintBtn.interactable = true;
        if (autoSprintBtnTxt != null) autoSprintBtnTxt.text = "🤖 ЗАПУСТИТЬ АВТО-СПРИНТ ИИ-АГЕНТОВ";
    }

    public void UpgradeAgent(string agentId)
    {
        var agent = agents.Find(a => a.id == agentId);
        if (agent == null) return;

        if (agent.level >= agent.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Модель уже модернизирована до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = agent.GetCostMoney();
        double costCode = agent.GetCostCode();

        double curMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;
        double curCode = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;

        if (curMoney < costMoney || curCode < costCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(costMoney)} ₽ и {NumberFormatter.Format(costCode)} C#!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMoney(costMoney);
            GameManager.Instance.SpendLinesOfCode(costCode);
        }

        agent.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ {agent.title} обновлен до Ур.{agent.level}!\nГенерация: +{agent.codePerSecBonus} C#/сек", transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
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
        if (autoSprintBtn != null)
        {
            autoSprintBtn.onClick.RemoveAllListeners();
            autoSprintBtn.onClick.AddListener(StartAutoSprint);
        }
        if (openAiAgentsBtn != null)
        {
            openAiAgentsBtn.onClick.RemoveAllListeners();
            openAiAgentsBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (aiStatsSummaryTxt != null)
        {
            double cps = GetAiCodePerSec();
            double incMult = GetAiIncomeMultiplier();
            double clkMult = GetAiClickMultiplier();
            aiStatsSummaryTxt.text = $"Авто-кодинг: <color=#00FFAA><b>+{NumberFormatter.Format(cps)} C#/сек</b></color> | Проведено спринтов: <b>{autoSprintsCompleted}</b>\nДоход: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>";
        }

        if (autoSprintBtnTxt != null && !isSprintActive)
        {
            autoSprintBtnTxt.text = "🤖 ЗАПУСТИТЬ АВТО-СПРИНТ ИИ-АГЕНТОВ";
        }

        if (modelsContainer == null) return;

        for (int i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            Transform child = i < modelsContainer.childCount ? modelsContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{agent.icon} {agent.title} (Ур. {agent.level}/{agent.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = agent.modelFamily;
            }
            if (statsTxt != null)
            {
                float totalInc = agent.level * agent.incomeMultiplierBonus * 100f;
                statsTxt.text = $"Поток: <color=#00FFAA>+{agent.GetTotalCps()} C#/сек</color> | Бонус: <color=#FFD700>+{totalInc:0}% доход</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (agent.level >= agent.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = agent.GetCostMoney();
                    double c = agent.GetCostCode();
                    upBtnTxt.text = $"Тюнинг\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
                    upBtn.interactable = true;
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("AutonomousAiAgents_ModalRoot", typeof(RectTransform));
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
        bImg.color = new Color(0.03f, 0.04f, 0.07f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Card
        GameObject card = new GameObject("AiCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.14f, 0.18f, 0.30f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🤖 ИНКУБАТОР ИИ-АГЕНТОВ АВТО-КОДИНГА";
        tTxt.fontSize = 18;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(0.2f, 0.9f, 1f);
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
        GameObject statsObj = new GameObject("AiStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        aiStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        aiStatsSummaryTxt.fontSize = 13;
        aiStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        aiStatsSummaryTxt.color = new Color(0.90f, 0.94f, 1f);

        // Action Panel
        GameObject actPanel = new GameObject("ActPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.14f, 0.18f, 0.28f, 1f);

        GameObject sBtnObj = new GameObject("SprintBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        sBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform sbRect = sBtnObj.GetComponent<RectTransform>();
        sbRect.anchorMin = Vector2.zero;
        sbRect.anchorMax = Vector2.one;
        sbRect.offsetMin = new Vector2(8, 6);
        sbRect.offsetMax = new Vector2(-8, -6);
        sBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.60f, 0.90f, 1f);
        autoSprintBtn = sBtnObj.GetComponent<Button>();

        GameObject stTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stTxtObj.transform.SetParent(sBtnObj.transform, false);
        autoSprintBtnTxt = stTxtObj.GetComponent<TextMeshProUGUI>();
        autoSprintBtnTxt.text = "🤖 ЗАПУСТИТЬ АВТО-СПРИНТ ИИ-АГЕНТОВ";
        autoSprintBtnTxt.fontSize = 14;
        autoSprintBtnTxt.fontStyle = FontStyles.Bold;
        autoSprintBtnTxt.alignment = TextAlignmentOptions.Center;
        autoSprintBtnTxt.color = Color.white;
        RectTransform str = stTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = Vector2.zero;
        str.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("AgentsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -188);
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
        modelsContainer = content.transform;
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

        foreach (var agent in agents)
        {
            CreateAgentCardUI(content.transform, agent);
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

    private void CreateAgentCardUI(Transform parent, AiAgentModel agent)
    {
        GameObject card = new GameObject("AgentCard_" + agent.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 92);
        card.GetComponent<Image>().color = new Color(0.13f, 0.16f, 0.24f, 1f);

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
        tTxt.fontSize = 14;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = new Color(0.2f, 0.9f, 1f);
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
        br.anchorMax = new Vector2(0.66f, 1f);
        br.offsetMin = new Vector2(10, 8);
        br.offsetMax = new Vector2(0, -30);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.color = new Color(0.85f, 0.90f, 1f);
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = new Vector2(0f, 0.45f);
        dr.anchorMax = new Vector2(1f, 1f);
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;

        GameObject stats = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stats.transform.SetParent(body.transform, false);
        TextMeshProUGUI sTxt = stats.GetComponent<TextMeshProUGUI>();
        sTxt.fontSize = 11;
        sTxt.fontStyle = FontStyles.Bold;
        sTxt.color = new Color(0.2f, 1f, 0.6f);
        RectTransform sr = stats.GetComponent<RectTransform>();
        sr.anchorMin = new Vector2(0f, 0f);
        sr.anchorMax = new Vector2(1f, 0.45f);
        sr.offsetMin = Vector2.zero;
        sr.offsetMax = Vector2.zero;

        // Action
        GameObject act = new GameObject("Action", typeof(RectTransform));
        act.transform.SetParent(card.transform, false);
        RectTransform ar = act.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.67f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 8);
        ar.offsetMax = new Vector2(-10, -10);

        GameObject upBtn = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        upBtn.transform.SetParent(act.transform, false);
        RectTransform ubr = upBtn.GetComponent<RectTransform>();
        ubr.anchorMin = Vector2.zero;
        ubr.anchorMax = Vector2.one;
        ubr.offsetMin = Vector2.zero;
        ubr.offsetMax = Vector2.zero;
        upBtn.GetComponent<Image>().color = new Color(0.20f, 0.55f, 0.88f, 1f);

        GameObject upTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        upTxt.transform.SetParent(upBtn.transform, false);
        TextMeshProUGUI ut = upTxt.GetComponent<TextMeshProUGUI>();
        ut.fontSize = 11;
        ut.fontStyle = FontStyles.Bold;
        ut.alignment = TextAlignmentOptions.Center;
        ut.color = Color.white;
        RectTransform utr = upTxt.GetComponent<RectTransform>();
        utr.anchorMin = Vector2.zero;
        utr.anchorMax = Vector2.one;
        utr.offsetMin = Vector2.zero;
        utr.offsetMax = Vector2.zero;

        string currentAgentId = agent.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeAgent(currentAgentId));
    }
}
