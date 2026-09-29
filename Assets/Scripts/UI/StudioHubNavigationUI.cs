using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Единый Навигационный Хаб Студии («Studio Operating System Hub»):
/// - Центр управления всеми подсистемами, отделами и космическими экспансиями студии
/// - 4 категории систем:
///   1. 🏢 Офис и Команда (Эргономика, CI/CD, Уточка Duck, Трофеи, Котики, Недвижимость)
///   2. 💼 Бизнес и Рынок (Аукцион ассетов, Demo Day, Патенты, Синдикат, Венчур, Совет директоров, GOTY)
///   3. ⚡ Технологии и ИИ (ИИ-агенты, Квантовый ЦОД, Нейролаборатория, Кибербезопасность, TechLab)
///   4. 🚀 Экспансия и Космос (Спутники связи, Облачный стриминг, Киберспорт Арена, Колония на Марсе, Голограммы)
/// - Дерево разблокировки по рангам («Milestone Unlock Gate»):
///   - Первокурсник / Junior / Middle / Senior / Техлид / IPO
/// - Автоматическая оптимизация экрана: сворачивание разрозненных кнопок в единый элегантный лаунчер
/// </summary>
public class StudioHubNavigationUI : MonoBehaviour
{
    private static StudioHubNavigationUI instance;
    public static StudioHubNavigationUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StudioHubNavigationUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(StudioHubNavigationUI));
                    instance = go.AddComponent<StudioHubNavigationUI>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

    public enum HubCategory
    {
        Office,
        Business,
        TechAI,
        Space
    }

    public class HubSystemEntry
    {
        public string id;
        public string title;
        public string categoryTitle;
        public string icon;
        public string description;
        public double reqTotalCode;
        public int reqPrestige;
        public string reqRankName;
        public HubCategory category;
        public Action openAction;

        public bool IsUnlocked()
        {
            double code = GameManager.Instance != null ? GameManager.Instance.TotalCodeWritten : 0;
            int prestige = GameManager.Instance != null ? GameManager.Instance.PrestigeLevel : 0;
            return code >= reqTotalCode && prestige >= reqPrestige;
        }

        public float GetUnlockProgress()
        {
            if (IsUnlocked()) return 1.0f;
            double code = GameManager.Instance != null ? GameManager.Instance.TotalCodeWritten : 0;
            if (reqTotalCode <= 0) return 1.0f;
            return Mathf.Clamp01((float)(code / reqTotalCode));
        }
    }

    [Header("UI элементы")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button hubMainLauncherBtn;
    [SerializeField] private TMP_Text hubMainLauncherTxt;

    [Header("Навигационные вкладки")]
    [SerializeField] private Button tabOfficeBtn;
    [SerializeField] private Button tabBusinessBtn;
    [SerializeField] private Button tabTechBtn;
    [SerializeField] private Button tabSpaceBtn;
    [SerializeField] private TMP_Text headerSummaryTxt;
    [SerializeField] private Transform systemsContainer;

    private List<HubSystemEntry> systemEntries = new List<HubSystemEntry>();
    private HubCategory currentTab = HubCategory.Office;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitSystemCatalog();
    }

    private void Start()
    {
        if (modalRoot == null)
        {
            BuildUI();
        }
        else
        {
            WireButtons();
            RefreshTabUI();
            if (modalRoot != null) modalRoot.SetActive(false);
        }

        StartCoroutine(CleanFloatingButtonsRoutine());
    }

    private IEnumerator CleanFloatingButtonsRoutine()
    {
        // Небольшая задержка, чтобы все компоненты успели зарегистрироваться
        yield return new WaitForSeconds(0.5f);
        TidyLegacyFloatingButtons();
    }

    public void TidyLegacyFloatingButtons()
    {
        string[] buttonNames = new string[]
        {
            "OrbitalUplinkLaunchBtn",
            "VentureFundLaunchBtn",
            "HoloKeynoteLaunchBtn",
            "QuantumClusterLaunchBtn",
            "EsportsArenaLaunchBtn",
            "CorporateBoardroomLaunchBtn",
            "CloudFabricLaunchBtn",
            "NeuroLabLaunchBtn",
            "MarsColonyLaunchBtn",
            "AutonomousAiLaunchBtn",
            "AssetStoreLaunchBtn",
            "GotyGalaLaunchBtn"
        };

        foreach (var bName in buttonNames)
        {
            GameObject obj = GameObject.Find(bName);
            if (obj != null)
            {
                // Скрываем отдельные перекрывающие кнопки, перенося управление в Hub
                obj.SetActive(false);
            }
        }
    }

    private void InitSystemCatalog()
    {
        systemEntries = new List<HubSystemEntry>
        {
            // --- Вкладка 1: Офис и Команда ---
            new HubSystemEntry
            {
                id = "sys_dailydigest",
                title = "Утренний Дайджест и Сбор Доходов",
                icon = "📋",
                category = HubCategory.Office,
                description = "Сводный отчет за сессию и быстрый сбор дивидендов и наград в 1 клик",
                reqTotalCode = 0,
                reqPrestige = 0,
                reqRankName = "Первокурсник",
                openAction = () => DailyDigestUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_ergo",
                title = "Эргономика Рабочего Места",
                icon = "💺",
                category = HubCategory.Office,
                description = "Ортопедические кресла, мониторы и стоячие столы студии",
                reqTotalCode = 0,
                reqPrestige = 0,
                reqRankName = "Первокурсник",
                openAction = () => WorkspaceErgonomicsUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_cicd",
                title = "Авто-CI/CD Робот Студии",
                icon = "🤖",
                category = HubCategory.Office,
                description = "Автоматизация сборки релизов и непрерывный деплой",
                reqTotalCode = 300,
                reqPrestige = 0,
                reqRankName = "Стажёр",
                openAction = () => AutomatedCICDBotUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_duck",
                title = "Резиновая Уточка Дебага",
                icon = "🦆",
                category = HubCategory.Office,
                description = "Кастомизация утки-ассистента и метод утиного программирования",
                reqTotalCode = 800,
                reqPrestige = 0,
                reqRankName = "Junior",
                openAction = () => DuckCustomizationUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_trophies",
                title = "Кабинет Студийных Трофеев",
                icon = "🏆",
                category = HubCategory.Office,
                description = "Витрина достижений, золотых дисков и наград за релизы",
                reqTotalCode = 2500,
                reqPrestige = 0,
                reqRankName = "Junior+",
                openAction = () => StudioTrophyCabinetUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_realestate",
                title = "Студийная Недвижимость",
                icon = "🏢",
                category = HubCategory.Office,
                description = "Переезд из гаража в open-space лофт и небоскреб Silicon Tower",
                reqTotalCode = 6000,
                reqPrestige = 0,
                reqRankName = "Middle",
                openAction = () => StudioRealEstateUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_cathaven",
                title = "Офисный Котоприют",
                icon = "🐱",
                category = HubCategory.Office,
                description = "Котики-талисманы, антистресс и постоянный пассивный бонус",
                reqTotalCode = 12000,
                reqPrestige = 0,
                reqRankName = "Middle+",
                openAction = () => StudioCatHavenUI.Instance.OpenModal()
            },

            // --- Вкладка 2: Бизнес и Рынок ---
            new HubSystemEntry
            {
                id = "sys_assetstore",
                title = "Маркетплейс Ассетов",
                icon = "🏪",
                category = HubCategory.Business,
                description = "Продажа 3D-паков, шейдеров и сетевого кода с пассивным доходом",
                reqTotalCode = 10000,
                reqPrestige = 0,
                reqRankName = "Middle",
                openAction = () => AssetStoreMarketplaceUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_demoday",
                title = "Инвесторский Demo Day",
                icon = "🎤",
                category = HubCategory.Business,
                description = "Elevator-питчи перед бизнес-ангелами и привлечение раундов",
                reqTotalCode = 18000,
                reqPrestige = 0,
                reqRankName = "Middle+",
                openAction = () => ElevatorPitchDemoDayUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_patents",
                title = "Патентные Войны",
                icon = "⚖️",
                category = HubCategory.Business,
                description = "Патентование уникальных игровых механик и защита авторских прав",
                reqTotalCode = 30000,
                reqPrestige = 0,
                reqRankName = "Senior",
                openAction = () => PatentPortfolioWarsUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_venture",
                title = "Венчурный Фонд Стартапов",
                icon = "💼",
                category = HubCategory.Business,
                description = "Инвестиции в молодые инди-команды и сбор дивидендов от IPO",
                reqTotalCode = 55000,
                reqPrestige = 0,
                reqRankName = "Senior+",
                openAction = () => VentureCapitalFundUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_goty",
                title = "Премия Игра Года (GOTY)",
                icon = "👑",
                category = HubCategory.Business,
                description = "Золотые статуэтки мировой индустрии и триумфальная речь победителя",
                reqTotalCode = 90000,
                reqPrestige = 0,
                reqRankName = "Техлид",
                openAction = () => GotyAwardsGalaUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_boardroom",
                title = "Совет Директоров и Поглощения",
                icon = "🤝",
                category = HubCategory.Business,
                description = "Враждебные поглощения AAA-корпораций и мировая монополия",
                reqTotalCode = 150000,
                reqPrestige = 1,
                reqRankName = "IPO x1",
                openAction = () => CorporateBoardroomUI.Instance.OpenModal()
            },

            // --- Вкладка 3: Технологии и ИИ ---
            new HubSystemEntry
            {
                id = "sys_security",
                title = "Кибербезопасность Студии",
                icon = "🛡️",
                category = HubCategory.TechAI,
                description = "Отражение DDoS-атак и защита серверов от хакеров",
                reqTotalCode = 5000,
                reqPrestige = 0,
                reqRankName = "Junior+",
                openAction = () => CyberSecurityDefenseUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_techlab",
                title = "Научно-Исследовательский Лаб",
                icon = "🔬",
                category = HubCategory.TechAI,
                description = "Глубокие исследования алгоритмов, сжатия и рендеринга",
                reqTotalCode = 15000,
                reqPrestige = 0,
                reqRankName = "Middle",
                openAction = () => TechLabResearchUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_ai_agents",
                title = "Инкубатор ИИ-Агентов",
                icon = "🤖",
                category = HubCategory.TechAI,
                description = "Автономные LLM-агенты, пишущие код в реальном времени",
                reqTotalCode = 40000,
                reqPrestige = 0,
                reqRankName = "Senior",
                openAction = () => AutonomousAiAgentsUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_quantum",
                title = "Квантовый Дата-Центр",
                icon = "⚛️",
                category = HubCategory.TechAI,
                description = "Криогенные квантовые процессоры и логические кубиты",
                reqTotalCode = 80000,
                reqPrestige = 0,
                reqRankName = "Техлид",
                openAction = () => QuantumDataCenterUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_neuro",
                title = "Нейроинтерфейсы (BCI)",
                icon = "🧠",
                category = HubCategory.TechAI,
                description = "Прямой ввод кода мыслями и нейро-синхронизация с движком",
                reqTotalCode = 120000,
                reqPrestige = 0,
                reqRankName = "Техлид+",
                openAction = () => NeuroInterfaceLabUI.Instance.OpenModal()
            },

            // --- Вкладка 4: Экспансия и Космос ---
            new HubSystemEntry
            {
                id = "sys_holokeynote",
                title = "Голографический Театр",
                icon = "✨",
                category = HubCategory.Space,
                description = "Планетарные презентации консолей и движков с 100% комбо",
                reqTotalCode = 50000,
                reqPrestige = 0,
                reqRankName = "Senior",
                openAction = () => HolographicKeynoteUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_esports_arena",
                title = "Киберспортивная Арена",
                icon = "🏆",
                category = HubCategory.Space,
                description = "Стадион на 50 000 фанатов, мировые мэйджоры и кассовые билеты",
                reqTotalCode = 70000,
                reqPrestige = 0,
                reqRankName = "Senior+",
                openAction = () => EsportsArenaLeagueUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_satellites",
                title = "Спутники Орбитального Линка",
                icon = "🛰️",
                category = HubCategory.Space,
                description = "Орбитальная группировкаKa-диапазона для мгновенных патчей игр",
                reqTotalCode = 100000,
                reqPrestige = 0,
                reqRankName = "Техлид",
                openAction = () => OrbitalSatelliteUplinkUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_cloud_stream",
                title = "Облачный Стриминг Игр",
                icon = "☁️",
                category = HubCategory.Space,
                description = "Собственный сервис облачного гейминга с GPU-нодами по всей планете",
                reqTotalCode = 135000,
                reqPrestige = 0,
                reqRankName = "Техлид+",
                openAction = () => CloudGamingStreamUI.Instance.OpenModal()
            },
            new HubSystemEntry
            {
                id = "sys_mars_colony",
                title = "Марсианская Колония Олимп",
                icon = "🔴",
                category = HubCategory.Space,
                description = "Межпланетный филиал студии в биокуполе Olympus Mons (0.38g)",
                reqTotalCode = 200000,
                reqPrestige = 1,
                reqRankName = "IPO x1",
                openAction = () => MarsColonyStudioUI.Instance.OpenModal()
            }
        };
    }

    public void SwitchTab(HubCategory newTab)
    {
        currentTab = newTab;
        RefreshTabUI();
        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
    }

    public void OpenHub()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            RefreshTabUI();
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = Vector3.one * 0.85f;
                StopAllCoroutines();
                StartCoroutine(AnimateModalOpen());
            }
            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    public void CloseHub()
    {
        if (modalRoot != null && modalRoot.activeSelf)
        {
            StartCoroutine(AnimateModalClose());
            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    private IEnumerator AnimateModalOpen()
    {
        float timer = 0f;
        float duration = 0.18f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float scale = Mathf.Lerp(0.85f, 1.0f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one;
    }

    private IEnumerator AnimateModalClose()
    {
        float timer = 0f;
        float duration = 0.14f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            float scale = Mathf.Lerp(1.0f, 0.85f, t);
            if (modalCardTransform != null) modalCardTransform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (modalRoot != null) modalRoot.SetActive(false);
    }

    public void RefreshTabUI()
    {
        int unlockedCount = 0;
        foreach (var s in systemEntries)
        {
            if (s.IsUnlocked()) unlockedCount++;
        }

        string rank = GameManager.Instance != null ? GameManager.Instance.GetDeveloperRankTitle() : "Первокурсник";
        if (headerSummaryTxt != null)
        {
            headerSummaryTxt.text = $"Ранг: <b>{rank}</b> • Развернуто систем: <b>{unlockedCount}/{systemEntries.Count}</b>";
        }

        UpdateTabButtonVisuals();

        if (systemsContainer != null)
        {
            // Filter by current tab
            List<HubSystemEntry> tabEntries = systemEntries.FindAll(s => s.category == currentTab);

            // Rebuild cards
            for (int i = 0; i < systemsContainer.childCount; i++)
            {
                Destroy(systemsContainer.GetChild(i).gameObject);
            }

            foreach (var entry in tabEntries)
            {
                CreateSystemCardUI(systemsContainer, entry);
            }
        }
    }

    private void UpdateTabButtonVisuals()
    {
        Color activeColor = new Color(0.18f, 0.55f, 0.88f, 1f);
        Color normalColor = new Color(0.12f, 0.16f, 0.24f, 1f);

        if (tabOfficeBtn != null) tabOfficeBtn.GetComponent<Image>().color = currentTab == HubCategory.Office ? activeColor : normalColor;
        if (tabBusinessBtn != null) tabBusinessBtn.GetComponent<Image>().color = currentTab == HubCategory.Business ? activeColor : normalColor;
        if (tabTechBtn != null) tabTechBtn.GetComponent<Image>().color = currentTab == HubCategory.TechAI ? activeColor : normalColor;
        if (tabSpaceBtn != null) tabSpaceBtn.GetComponent<Image>().color = currentTab == HubCategory.Space ? activeColor : normalColor;
    }

    private void CreateSystemCardUI(Transform parent, HubSystemEntry entry)
    {
        bool unlocked = entry.IsUnlocked();

        GameObject card = new GameObject("HubCard_" + entry.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = unlocked ? new Color(0.10f, 0.16f, 0.25f, 0.95f) : new Color(0.08f, 0.09f, 0.12f, 0.75f);

        LayoutElement le = card.GetComponent<LayoutElement>();
        le.preferredHeight = 78;
        le.minHeight = 78;

        // Icon Box
        GameObject iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image));
        iconBox.transform.SetParent(card.transform, false);
        RectTransform ibRect = iconBox.GetComponent<RectTransform>();
        ibRect.anchorMin = new Vector2(0f, 0.5f);
        ibRect.anchorMax = new Vector2(0f, 0.5f);
        ibRect.pivot = new Vector2(0f, 0.5f);
        ibRect.sizeDelta = new Vector2(46, 46);
        ibRect.anchoredPosition = new Vector2(8, 0);
        iconBox.GetComponent<Image>().color = unlocked ? new Color(0.16f, 0.28f, 0.44f, 1f) : new Color(0.14f, 0.14f, 0.16f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = unlocked ? entry.icon : "🔒";
        it.fontSize = 24;
        it.alignment = TextAlignmentOptions.Center;
        RectTransform itr = iconTxt.GetComponent<RectTransform>();
        itr.anchorMin = Vector2.zero;
        itr.anchorMax = Vector2.one;
        itr.offsetMin = Vector2.zero;
        itr.offsetMax = Vector2.zero;

        // Header & Desc
        GameObject body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(card.transform, false);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(0.72f, 1f);
        br.offsetMin = new Vector2(62, 6);
        br.offsetMax = new Vector2(0, -6);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(body.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 13;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = unlocked ? Color.white : new Color(0.6f, 0.6f, 0.65f);
        tTxt.text = unlocked ? entry.title : $"{entry.title} (Заблокировано)";
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0.5f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 10;
        if (unlocked)
        {
            dTxt.color = new Color(0.75f, 0.85f, 0.95f);
            dTxt.text = entry.description;
        }
        else
        {
            dTxt.color = new Color(0.95f, 0.55f, 0.25f);
            dTxt.text = $"Требуется ранг: {entry.reqRankName} ({NumberFormatter.Format(entry.reqTotalCode)} C#)";
        }
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = new Vector2(0f, 0f);
        dr.anchorMax = new Vector2(1f, 0.5f);
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;

        // Action Button
        GameObject act = new GameObject("Action", typeof(RectTransform));
        act.transform.SetParent(card.transform, false);
        RectTransform ar = act.GetComponent<RectTransform>();
        ar.anchorMin = new Vector2(0.72f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 16);
        ar.offsetMax = new Vector2(-8, -16);

        GameObject btnObj = new GameObject("LaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(act.transform, false);
        RectTransform bRect = btnObj.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        btnObj.GetComponent<Image>().color = unlocked ? new Color(0.18f, 0.55f, 0.88f, 1f) : new Color(0.20f, 0.22f, 0.26f, 1f);
        Button btn = btnObj.GetComponent<Button>();
        btn.interactable = unlocked;

        GameObject btnTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI bt = btnTxtObj.GetComponent<TextMeshProUGUI>();
        bt.fontSize = 11;
        bt.fontStyle = FontStyles.Bold;
        bt.alignment = TextAlignmentOptions.Center;
        bt.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.55f);
        bt.text = unlocked ? "ОТКРЫТЬ" : "🔒 ЗАКРЫТО";
        RectTransform btr = btnTxtObj.GetComponent<RectTransform>();
        btr.anchorMin = Vector2.zero;
        btr.anchorMax = Vector2.one;
        btr.offsetMin = Vector2.zero;
        btr.offsetMax = Vector2.zero;

        if (unlocked && entry.openAction != null)
        {
            btn.onClick.AddListener(() =>
            {
                CloseHub();
                entry.openAction.Invoke();
            });
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseHub);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseHub);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseHub);
        if (hubMainLauncherBtn != null) hubMainLauncherBtn.onClick.AddListener(OpenHub);

        if (tabOfficeBtn != null) tabOfficeBtn.onClick.AddListener(() => SwitchTab(HubCategory.Office));
        if (tabBusinessBtn != null) tabBusinessBtn.onClick.AddListener(() => SwitchTab(HubCategory.Business));
        if (tabTechBtn != null) tabTechBtn.onClick.AddListener(() => SwitchTab(HubCategory.TechAI));
        if (tabSpaceBtn != null) tabSpaceBtn.onClick.AddListener(() => SwitchTab(HubCategory.Space));
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("StudioHubModalRoot", typeof(RectTransform));
        modalRoot.transform.SetParent(canvas.transform, false);
        RectTransform rtRoot = modalRoot.GetComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.offsetMin = Vector2.zero;
        rtRoot.offsetMax = Vector2.zero;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(modalRoot.transform, false);
        RectTransform rtBackdrop = backdrop.GetComponent<RectTransform>();
        rtBackdrop.anchorMin = Vector2.zero;
        rtBackdrop.anchorMax = Vector2.one;
        rtBackdrop.offsetMin = Vector2.zero;
        rtBackdrop.offsetMax = Vector2.zero;
        Image bgImg = backdrop.GetComponent<Image>();
        bgImg.color = new Color(0.01f, 0.03f, 0.06f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(modalRoot.transform, false);
        modalCardTransform = card.transform;
        RectTransform rtCard = card.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(490, 690);
        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.06f, 0.10f, 0.16f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "💻 STUDIO OPERATING SYSTEM";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.2f, 0.85f, 1f);
        RectTransform hRect = headerObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(-40, 36);
        hRect.anchoredPosition = new Vector2(0, -12);

        // Header Summary Text
        GameObject summaryObj = new GameObject("SummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        summaryObj.transform.SetParent(card.transform, false);
        headerSummaryTxt = summaryObj.GetComponent<TextMeshProUGUI>();
        headerSummaryTxt.fontSize = 11;
        headerSummaryTxt.alignment = TextAlignmentOptions.Center;
        headerSummaryTxt.color = new Color(0.8f, 0.92f, 1f);
        RectTransform smRect = summaryObj.GetComponent<RectTransform>();
        smRect.anchorMin = new Vector2(0f, 1f);
        smRect.anchorMax = new Vector2(1f, 1f);
        smRect.pivot = new Vector2(0.5f, 1f);
        smRect.sizeDelta = new Vector2(-40, 22);
        smRect.anchoredPosition = new Vector2(0, -44);

        // Close 'X' Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(card.transform, false);
        RectTransform xRect = closeXObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 1f);
        xRect.anchorMax = new Vector2(1f, 1f);
        xRect.pivot = new Vector2(1f, 1f);
        xRect.sizeDelta = new Vector2(34, 34);
        xRect.anchoredPosition = new Vector2(-12, -12);
        closeXObj.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(closeXObj.transform, false);
        TextMeshProUGUI xt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xt.text = "✕";
        xt.fontSize = 18;
        xt.fontStyle = FontStyles.Bold;
        xt.alignment = TextAlignmentOptions.Center;
        xt.color = Color.white;
        RectTransform xtr = xTxtObj.GetComponent<RectTransform>();
        xtr.anchorMin = Vector2.zero;
        xtr.anchorMax = Vector2.one;
        xtr.offsetMin = Vector2.zero;
        xtr.offsetMax = Vector2.zero;

        // Tabs Bar (4 Вкладки)
        GameObject tabsBar = new GameObject("TabsBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabsBar.transform.SetParent(card.transform, false);
        RectTransform tbRect = tabsBar.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(1f, 1f);
        tbRect.pivot = new Vector2(0.5f, 1f);
        tbRect.sizeDelta = new Vector2(-24, 40);
        tbRect.anchoredPosition = new Vector2(0, -72);

        HorizontalLayoutGroup hlg = tabsBar.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 6;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        tabOfficeBtn = CreateTabButton(tabsBar.transform, "🏢 Офис", HubCategory.Office);
        tabBusinessBtn = CreateTabButton(tabsBar.transform, "💼 Бизнес", HubCategory.Business);
        tabTechBtn = CreateTabButton(tabsBar.transform, "⚡ ИИ и Наука", HubCategory.TechAI);
        tabSpaceBtn = CreateTabButton(tabsBar.transform, "🚀 Экспансия", HubCategory.Space);

        // Scroll View для списка систем
        GameObject scrollObj = new GameObject("SystemsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(14, 58);
        scRect.offsetMax = new Vector2(-14, -120);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.10f, 0.5f);

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
        systemsContainer = content.transform;
        RectTransform cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0f, 1f);
        cRect.anchorMax = new Vector2(1f, 1f);
        cRect.pivot = new Vector2(0.5f, 1f);
        cRect.offsetMin = Vector2.zero;
        cRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.padding = new RectOffset(4, 4, 6, 6);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = cRect;
        sr.viewport = vRect;
        sr.horizontal = false;
        sr.vertical = true;

        // Bottom Close Button
        GameObject botCloseObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 40);
        bcRect.anchoredPosition = new Vector2(0, 10);
        botCloseObj.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.42f, 1f);
        closeBtn = botCloseObj.GetComponent<Button>();

        GameObject bcTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bcTxtObj.transform.SetParent(botCloseObj.transform, false);
        TextMeshProUGUI bct = bcTxtObj.GetComponent<TextMeshProUGUI>();
        bct.text = "ЗАКРЫТЬ ХАБ";
        bct.fontSize = 13;
        bct.fontStyle = FontStyles.Bold;
        bct.alignment = TextAlignmentOptions.Center;
        bct.color = Color.white;
        RectTransform bctr = bcTxtObj.GetComponent<RectTransform>();
        bctr.anchorMin = Vector2.zero;
        bctr.anchorMax = Vector2.one;
        bctr.offsetMin = Vector2.zero;
        bctr.offsetMax = Vector2.zero;

        // Создаем главную компактную кнопку вызова Хаба
        CreateHubMainLauncher(canvas.transform);

        WireButtons();
        RefreshTabUI();
        modalRoot.SetActive(false);
    }

    private Button CreateTabButton(Transform parent, string title, HubCategory cat)
    {
        GameObject tBtnObj = new GameObject("Tab_" + cat, typeof(RectTransform), typeof(Image), typeof(Button));
        tBtnObj.transform.SetParent(parent, false);
        tBtnObj.GetComponent<Image>().color = new Color(0.12f, 0.16f, 0.24f, 1f);

        GameObject tTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        tTxtObj.transform.SetParent(tBtnObj.transform, false);
        TextMeshProUGUI tt = tTxtObj.GetComponent<TextMeshProUGUI>();
        tt.text = title;
        tt.fontSize = 11;
        tt.fontStyle = FontStyles.Bold;
        tt.alignment = TextAlignmentOptions.Center;
        tt.color = Color.white;
        RectTransform ttr = tTxtObj.GetComponent<RectTransform>();
        ttr.anchorMin = Vector2.zero;
        ttr.anchorMax = Vector2.one;
        ttr.offsetMin = Vector2.zero;
        ttr.offsetMax = Vector2.zero;

        return tBtnObj.GetComponent<Button>();
    }

    private void CreateHubMainLauncher(Transform canvasTransform)
    {
        GameObject hubLaunchObj = new GameObject("StudioHubMainDockBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        hubLaunchObj.transform.SetParent(canvasTransform, false);
        RectTransform hlRect = hubLaunchObj.GetComponent<RectTransform>();
        hlRect.anchorMin = new Vector2(0.5f, 0f);
        hlRect.anchorMax = new Vector2(0.5f, 0f);
        hlRect.pivot = new Vector2(0.5f, 0f);
        hlRect.sizeDelta = new Vector2(230, 46);
        hlRect.anchoredPosition = new Vector2(0, 16);
        hubLaunchObj.GetComponent<Image>().color = new Color(0.12f, 0.48f, 0.85f, 0.95f);
        hubMainLauncherBtn = hubLaunchObj.GetComponent<Button>();

        GameObject hlTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        hlTxtObj.transform.SetParent(hubLaunchObj.transform, false);
        hubMainLauncherTxt = hlTxtObj.GetComponent<TextMeshProUGUI>();
        hubMainLauncherTxt.text = "💻 СТУДИЯ OS • СИСТЕМЫ";
        hubMainLauncherTxt.fontSize = 14;
        hubMainLauncherTxt.fontStyle = FontStyles.Bold;
        hubMainLauncherTxt.alignment = TextAlignmentOptions.Center;
        hubMainLauncherTxt.color = Color.white;
        RectTransform htr = hlTxtObj.GetComponent<RectTransform>();
        htr.anchorMin = Vector2.zero;
        htr.anchorMax = Vector2.one;
        htr.offsetMin = Vector2.zero;
        htr.offsetMax = Vector2.zero;
    }
}
