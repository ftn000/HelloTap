using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Академия Инженеров и Стажировки («Junior Dev Internship Academy»):
/// - 4 специализированных курса обучения стажеров:
///   1. C# Fundamentals & OOP (Основы языка и синтаксис)
///   2. Data Structures & Algorithms (Быстрые алгоритмы и структуры)
///   3. Clean Architecture & Unit Tests (Архитектура и надежные тесты)
///   4. Game Engine & Shader Magic (Шейдеры, физика и ECS)
/// - Прокачка курсов увеличивает приток пассивных строк кода от стажеров и множители клика
/// - Интерактивный выпускной: «🎓 ВЫПУСТИТЬ ПОТОК СТАЖЕРОВ» (награды и сертификаты)
/// - Множители пассивного дохода и генерации строк кода в секунду
/// </summary>
public class JuniorDevAcademyUI : MonoBehaviour
{
    private static JuniorDevAcademyUI instance;
    public static JuniorDevAcademyUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<JuniorDevAcademyUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(JuniorDevAcademyUI));
                    instance = go.AddComponent<JuniorDevAcademyUI>();
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
    public class AcademyCourse
    {
        public string id;
        public string title;
        public string desc;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double codePerSecBonus;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.46, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.40, level));
        public double GetTotalCodePerSec() => codePerSecBonus * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openAcademyBtn;

    [Header("Выпуск стажеров и курсы")]
    [SerializeField] private TMP_Text academyStatsSummaryTxt;
    [SerializeField] private Button graduateBtn;
    [SerializeField] private TMP_Text graduateBtnTxt;
    [SerializeField] private Transform coursesContainer;

    private readonly List<AcademyCourse> courses = new List<AcademyCourse>();
    private int totalGraduatesCount = 0;
    private bool isGraduationInProgress = false;

    private const string PrefCourseLvlPrefix = "Academy_CourseLvl_";
    private const string PrefGraduatesCount = "Academy_GraduatesCount";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCourses();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeCourses()
    {
        if (courses.Count > 0) return;

        courses.Add(new AcademyCourse
        {
            id = "course_csharp",
            title = "C# Fundamentals & Syntax",
            desc = "Классы, структуры, LINQ и эффективное управление памятью без аллокаций.",
            icon = "📘",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 30000,
            baseCostCode = 12000,
            codePerSecBonus = 20.0,
            incomeMultiplierBonus = 0.05f,
            clickMultiplierBonus = 0.04f
        });

        courses.Add(new AcademyCourse
        {
            id = "course_algo",
            title = "Data Structures & Big-O",
            desc = "Хеш-таблицы, графы, бинарные деревья и алгоритмы поиска путей A*.",
            icon = "📐",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 110000,
            baseCostCode = 55000,
            codePerSecBonus = 65.0,
            incomeMultiplierBonus = 0.10f,
            clickMultiplierBonus = 0.06f
        });

        courses.Add(new AcademyCourse
        {
            id = "course_architecture",
            title = "Clean Architecture & Tests",
            desc = "Паттерны проектирования, Dependency Injection и 100% покрытие юнит-тестами.",
            icon = "🏛️",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 380000,
            baseCostCode = 190000,
            codePerSecBonus = 180.0,
            incomeMultiplierBonus = 0.18f,
            clickMultiplierBonus = 0.12f
        });

        courses.Add(new AcademyCourse
        {
            id = "course_shaders_engine",
            title = "Game Engine & Shader Magic",
            desc = "HLSL-шейдеры, физический конвейер, оптимизация батчинга и ECS архитектура.",
            icon = "🔮",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1200000,
            baseCostCode = 600000,
            codePerSecBonus = 500.0,
            incomeMultiplierBonus = 0.28f,
            clickMultiplierBonus = 0.20f
        });
    }

    private void LoadData()
    {
        totalGraduatesCount = PlayerPrefs.GetInt(PrefGraduatesCount, 0);
        foreach (var c in courses)
        {
            if (PlayerPrefs.HasKey(PrefCourseLvlPrefix + c.id))
            {
                c.level = PlayerPrefs.GetInt(PrefCourseLvlPrefix + c.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefGraduatesCount, totalGraduatesCount);
        foreach (var c in courses)
        {
            PlayerPrefs.SetInt(PrefCourseLvlPrefix + c.id, c.level);
        }
        PlayerPrefs.Save();
    }

    public double GetAcademyIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var c in courses)
        {
            mult += c.level * c.incomeMultiplierBonus;
        }
        mult += Math.Min(totalGraduatesCount * 0.02, 1.0);
        return mult;
    }

    public double GetAcademyClickMultiplier()
    {
        double mult = 1.0;
        foreach (var c in courses)
        {
            mult += c.level * c.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetAcademyCodePerSec()
    {
        double sum = 0;
        foreach (var c in courses)
        {
            sum += c.GetTotalCodePerSec();
        }
        return sum * (1.0 + Math.Min(totalGraduatesCount * 0.05, 2.0));
    }

    public void GraduateInterns()
    {
        if (isGraduationInProgress) return;
        StartCoroutine(GraduationRoutine());
    }

    private IEnumerator GraduationRoutine()
    {
        isGraduationInProgress = true;
        if (graduateBtn != null) graduateBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (graduateBtnTxt != null)
        {
            graduateBtnTxt.text = "📜 ИДЕТ ВЫПУСКНОЙ ЭКЗАМЕН СТАЖЕРОВ...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        totalGraduatesCount += 5;
        double rewardMoney = 35000.0 * GetAcademyIncomeMultiplier();
        double rewardCode = 15000.0 * GetAcademyClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.20f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎓 ВЫПУСК 5 ДЖУНОВ ЗАВЕРШЕН!\nПремия академии: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.3f, 0.85f, 1f), true);
        }

        isGraduationInProgress = false;
        if (graduateBtn != null) graduateBtn.interactable = true;
        if (graduateBtnTxt != null) graduateBtnTxt.text = "🎓 ВЫПУСТИТЬ ПОТОК СТАЖЕРОВ В ШТАТ";
    }

    public void UpgradeCourse(string courseId)
    {
        var course = courses.Find(c => c.id == courseId);
        if (course == null) return;

        if (course.level >= course.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Курс уже модернизирован до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = course.GetCostMoney();
        double costCode = course.GetCostCode();

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

        course.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Курс '{course.title}' улучшен до Ур.{course.level}!\nГенерация: +{course.codePerSecBonus} C#/сек", transform.position, new Color(0.2f, 1f, 0.5f), true);
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
        if (graduateBtn != null)
        {
            graduateBtn.onClick.RemoveAllListeners();
            graduateBtn.onClick.AddListener(GraduateInterns);
        }
        if (openAcademyBtn != null)
        {
            openAcademyBtn.onClick.RemoveAllListeners();
            openAcademyBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (academyStatsSummaryTxt != null)
        {
            double cps = GetAcademyCodePerSec();
            double incMult = GetAcademyIncomeMultiplier();
            double clkMult = GetAcademyClickMultiplier();
            academyStatsSummaryTxt.text = $"Выпущено джунов: <color=#00FFAA><b>{totalGraduatesCount}</b></color> | Поток: <color=#00FFAA><b>+{NumberFormatter.Format(cps)} C#/сек</b></color>\nДоход: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>";
        }

        if (graduateBtnTxt != null && !isGraduationInProgress)
        {
            graduateBtnTxt.text = "🎓 ВЫПУСТИТЬ ПОТОК СТАЖЕРОВ В ШТАТ";
        }

        if (coursesContainer == null) return;

        for (int i = 0; i < courses.Count; i++)
        {
            var course = courses[i];
            Transform child = i < coursesContainer.childCount ? coursesContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{course.icon} {course.title} (Ур. {course.level}/{course.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = course.desc;
            }
            if (statsTxt != null)
            {
                float incBonus = course.level * course.incomeMultiplierBonus * 100f;
                statsTxt.text = $"Бонус: <color=#00FFAA>+{course.GetTotalCodePerSec()} C#/сек</color> | <color=#FFD700>+{incBonus:0}% доход</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (course.level >= course.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = course.GetCostMoney();
                    double c = course.GetCostCode();
                    upBtnTxt.text = $"Обучение\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        GameObject root = new GameObject("JuniorDevAcademy_ModalRoot", typeof(RectTransform));
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
        bImg.color = new Color(0.04f, 0.05f, 0.08f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Card
        GameObject card = new GameObject("AcademyCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.11f, 0.13f, 0.20f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.16f, 0.22f, 0.35f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🎓 АКАДЕМИЯ ИНЖЕНЕРОВ & СТАЖИРОВКИ";
        tTxt.fontSize = 19;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(0.35f, 0.85f, 1f);
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
        GameObject statsObj = new GameObject("AcademyStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        academyStatsSummaryTxt = statsObj.GetComponent<TextMeshProUGUI>();
        academyStatsSummaryTxt.fontSize = 13;
        academyStatsSummaryTxt.alignment = TextAlignmentOptions.Center;
        academyStatsSummaryTxt.color = new Color(0.90f, 0.94f, 1f);

        // Graduate Action Button Panel
        GameObject gPanel = new GameObject("GradPanel", typeof(RectTransform), typeof(Image));
        gPanel.transform.SetParent(card.transform, false);
        RectTransform gpRect = gPanel.GetComponent<RectTransform>();
        gpRect.anchorMin = new Vector2(0f, 1f);
        gpRect.anchorMax = new Vector2(1f, 1f);
        gpRect.pivot = new Vector2(0.5f, 1f);
        gpRect.sizeDelta = new Vector2(-30, 52);
        gpRect.anchoredPosition = new Vector2(0, -126);
        gPanel.GetComponent<Image>().color = new Color(0.14f, 0.19f, 0.30f, 1f);

        GameObject gBtnObj = new GameObject("GraduateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        gBtnObj.transform.SetParent(gPanel.transform, false);
        RectTransform gbRect = gBtnObj.GetComponent<RectTransform>();
        gbRect.anchorMin = Vector2.zero;
        gbRect.anchorMax = Vector2.one;
        gbRect.offsetMin = new Vector2(8, 6);
        gbRect.offsetMax = new Vector2(-8, -6);
        gBtnObj.GetComponent<Image>().color = new Color(0.20f, 0.65f, 0.40f, 1f);
        graduateBtn = gBtnObj.GetComponent<Button>();

        GameObject gtTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        gtTxtObj.transform.SetParent(gBtnObj.transform, false);
        graduateBtnTxt = gtTxtObj.GetComponent<TextMeshProUGUI>();
        graduateBtnTxt.text = "🎓 ВЫПУСТИТЬ ПОТОК СТАЖЕРОВ В ШТАТ";
        graduateBtnTxt.fontSize = 14;
        graduateBtnTxt.fontStyle = FontStyles.Bold;
        graduateBtnTxt.alignment = TextAlignmentOptions.Center;
        graduateBtnTxt.color = Color.white;
        RectTransform gtr = gtTxtObj.GetComponent<RectTransform>();
        gtr.anchorMin = Vector2.zero;
        gtr.anchorMax = Vector2.one;
        gtr.offsetMin = Vector2.zero;
        gtr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("CoursesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
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
        coursesContainer = content.transform;
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

        foreach (var course in courses)
        {
            CreateCourseCardUI(content.transform, course);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.26f, 0.38f, 1f);
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

    private void CreateCourseCardUI(Transform parent, AcademyCourse course)
    {
        GameObject card = new GameObject("CourseCard_" + course.id, typeof(RectTransform), typeof(Image));
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
        tTxt.color = new Color(0.35f, 0.88f, 1f);
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
        upBtn.GetComponent<Image>().color = new Color(0.20f, 0.50f, 0.85f, 1f);

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

        string currentCourseId = course.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeCourse(currentCourseId));
    }
}
