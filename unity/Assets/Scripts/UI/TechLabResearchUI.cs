using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Исследовательская VR/AR Лаборатория технологий (Next-Gen Tech Lab):
/// - 4 ветки R&D исследований: Графика, ИИ и Физика, Процедурный мир, VR/Нейроинтерфейсы
/// - Исследования оплачиваются написанными строками кода (Code Lines)
/// - Каждое завершенное исследование даёт постоянный глобальный перк к доходам и проектам
/// </summary>
public class TechLabResearchUI : MonoBehaviour
{
    private static TechLabResearchUI instance;
    public static TechLabResearchUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<TechLabResearchUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class TechNode
    {
        public string id;
        public string title;
        public string branchName;
        public string icon;
        public double codeCost;
        public string perkDesc;
        public double bonusMultiplier;
        public bool isResearched;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openTechLabBtn;
    [SerializeField] private TMP_Text openTechLabBtnText;
    [SerializeField] private Button closeTechLabBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка лаборатории")]
    [SerializeField] private TMP_Text totalTechResearchedText;
    [SerializeField] private TMP_Text globalTechMultiplierText;

    [Header("Контейнер древа технологий")]
    [SerializeField] private Transform techNodesContainer;

    private readonly List<TechNode> techNodes = new List<TechNode>();

    private const string PrefTechPrefix = "Studio_TechResearched_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<TechNode> TechNodes => techNodes;

    public double GetGlobalTechMultiplier()
    {
        double mult = 1.0;
        for (int i = 0; i < techNodes.Count; i++)
        {
            if (techNodes[i].isResearched)
            {
                mult += techNodes[i].bonusMultiplier;
            }
        }
        return mult;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTechTree();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeTechTree()
    {
        techNodes.Clear();

        // 1. Графика и Шейдеры
        techNodes.Add(new TechNode
        {
            id = "tech_pbr",
            title = "PBR Фотореалистичные Материалы",
            branchName = "Рендеринг",
            icon = "🎨",
            codeCost = 15000.0,
            perkDesc = "+8% к глобальному доходу от проектов",
            bonusMultiplier = 0.08,
            isResearched = false
        });

        techNodes.Add(new TechNode
        {
            id = "tech_raytracing",
            title = "Аппаратный Ray Tracing",
            branchName = "Рендеринг",
            icon = "✨",
            codeCost = 85000.0,
            perkDesc = "+12% к пассивным продажам игр",
            bonusMultiplier = 0.12,
            isResearched = false
        });

        techNodes.Add(new TechNode
        {
            id = "tech_neural_dlss",
            title = "Нейросетевой Апскейлер DLSS 4.0",
            branchName = "Рендеринг",
            icon = "🔮",
            codeCost = 350000.0,
            perkDesc = "+18% к общему множителю компании",
            bonusMultiplier = 0.18,
            isResearched = false
        });

        // 2. ИИ и Физика
        techNodes.Add(new TechNode
        {
            id = "tech_behavior_trees",
            title = "Иерархические Деревья Поведений ИИ",
            branchName = "ИИ и Физика",
            icon = "🧠",
            codeCost = 25000.0,
            perkDesc = "+10% к скорости набора кода кликами",
            bonusMultiplier = 0.10,
            isResearched = false
        });

        techNodes.Add(new TechNode
        {
            id = "tech_ragdoll_quantum",
            title = "Квантовая Физика Рэгдолла",
            branchName = "ИИ и Физика",
            icon = "🤸",
            codeCost = 120000.0,
            perkDesc = "+15% к очкам критов",
            bonusMultiplier = 0.15,
            isResearched = false
        });

        // 3. Процедурная генерация
        techNodes.Add(new TechNode
        {
            id = "tech_perlin_biomes",
            title = "Процедурные Биомы (Шум Перлина)",
            branchName = "Мироустройство",
            icon = "🌲",
            codeCost = 45000.0,
            perkDesc = "+10% к пассивному доходу персонала",
            bonusMultiplier = 0.10,
            isResearched = false
        });

        techNodes.Add(new TechNode
        {
            id = "tech_infinite_multiverse",
            title = "Генератор Бесконечных Мультивселенных",
            branchName = "Мироустройство",
            icon = "🌌",
            codeCost = 500000.0,
            perkDesc = "+25% ко всем показателям студии",
            bonusMultiplier = 0.25,
            isResearched = false
        });

        // 4. VR/AR и Метаверс
        techNodes.Add(new TechNode
        {
            id = "tech_hand_tracking",
            title = "Оптический Трекинг Рук и Взгляда",
            branchName = "VR / AR",
            icon = "🥽",
            codeCost = 90000.0,
            perkDesc = "+12% к длительности энергетического буста",
            bonusMultiplier = 0.12,
            isResearched = false
        });

        techNodes.Add(new TechNode
        {
            id = "tech_neural_link",
            title = "Прямой Нейроинтерфейс Разработчика",
            branchName = "VR / AR",
            icon = "⚡",
            codeCost = 1500000.0,
            perkDesc = "+30% абсолютный мультипликатор студии",
            bonusMultiplier = 0.30,
            isResearched = false
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < techNodes.Count; i++)
        {
            techNodes[i].isResearched = PlayerPrefs.GetInt(PrefTechPrefix + techNodes[i].id, 0) == 1;
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < techNodes.Count; i++)
        {
            PlayerPrefs.SetInt(PrefTechPrefix + techNodes[i].id, techNodes[i].isResearched ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void ResearchTech(int index)
    {
        if (index < 0 || index >= techNodes.Count) return;
        var node = techNodes[index];

        if (node.isResearched) return;

        if (GameManager.Instance == null || GameManager.Instance.CodeLines < node.codeCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно строк кода ({NumberFormatter.Format(node.codeCost)} строк)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        // Списываем строки кода
        GameManager.Instance.SpendLinesOfCode(node.codeCost);
        node.isResearched = true;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🔬 ИССЛЕДОВАНИЕ ЗАВЕРШЕНО!\n{node.icon} {node.title}\n{node.perkDesc}", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openTechLabBtn != null)
        {
            openTechLabBtn.onClick.RemoveAllListeners();
            openTechLabBtn.onClick.AddListener(OpenModal);
        }
        if (closeTechLabBtn != null)
        {
            closeTechLabBtn.onClick.RemoveAllListeners();
            closeTechLabBtn.onClick.AddListener(CloseModal);
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
        int countResearched = 0;
        for (int i = 0; i < techNodes.Count; i++) if (techNodes[i].isResearched) countResearched++;

        if (totalTechResearchedText != null)
        {
            totalTechResearchedText.text = $"🔬 Изучено технологий: <b>{countResearched}/{techNodes.Count}</b>";
        }

        if (globalTechMultiplierText != null)
        {
            double mult = GetGlobalTechMultiplier();
            globalTechMultiplierText.text = $"Общий бонус лаборатории: <b><color=#00FF88>x{mult:F2}</color></b>";
        }

        RefreshTechList();
    }

    private void RefreshTechList()
    {
        if (techNodesContainer == null) return;

        double currentCode = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;

        for (int i = 0; i < techNodes.Count; i++)
        {
            int idx = i;
            var node = techNodes[i];
            Transform cardTr = techNodesContainer.Find($"TechCard_{idx}");
            if (cardTr == null) continue;

            Button resBtn = cardTr.Find("ResBtn")?.GetComponent<Button>();
            TMP_Text resBtnTxt = resBtn != null ? resBtn.GetComponentInChildren<TMP_Text>() : null;
            Image bg = cardTr.GetComponent<Image>();

            if (bg != null)
            {
                bg.color = node.isResearched 
                    ? new Color(0.11f, 0.22f, 0.16f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.9f);
            }

            if (resBtn != null && resBtnTxt != null)
            {
                resBtn.onClick.RemoveAllListeners();
                resBtn.onClick.AddListener(() => ResearchTech(idx));

                if (node.isResearched)
                {
                    resBtn.interactable = false;
                    resBtnTxt.text = "✓ ИЗУЧЕНО";
                    resBtn.GetComponent<Image>().color = new Color(0.15f, 0.45f, 0.25f);
                }
                else
                {
                    bool canAfford = currentCode >= node.codeCost;
                    resBtn.interactable = canAfford;
                    resBtnTxt.text = $"ИЗУЧИТЬ ({NumberFormatter.Format(node.codeCost)} строк)";
                    resBtn.GetComponent<Image>().color = canAfford ? new Color(0.15f, 0.55f, 0.75f) : new Color(0.2f, 0.25f, 0.32f);
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("TechLabResearchModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.8f, 1f, 0.5f);
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
        headerTxt.text = "🔬 ТЕХНО-ЛАБОРАТОРИЯ R&D";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 0.85f, 1f);

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

        // Info Summary Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -56);
        infoRt.sizeDelta = new Vector2(-36, 68);
        infoPanel.GetComponent<Image>().color = new Color(0.11f, 0.15f, 0.22f, 0.95f);

        GameObject totObj = new GameObject("TotalRes", typeof(RectTransform), typeof(TextMeshProUGUI));
        totObj.transform.SetParent(infoPanel.transform, false);
        RectTransform totRt = totObj.GetComponent<RectTransform>();
        totRt.anchorMin = new Vector2(0, 0.5f);
        totRt.anchorMax = new Vector2(1, 1);
        totRt.offsetMin = new Vector2(12, 0);
        totRt.offsetMax = new Vector2(-12, -4);
        totalTechResearchedText = totObj.GetComponent<TextMeshProUGUI>();
        totalTechResearchedText.fontSize = 13;
        totalTechResearchedText.fontStyle = FontStyles.Bold;

        GameObject multObj = new GameObject("Mult", typeof(RectTransform), typeof(TextMeshProUGUI));
        multObj.transform.SetParent(infoPanel.transform, false);
        RectTransform multRt = multObj.GetComponent<RectTransform>();
        multRt.anchorMin = new Vector2(0, 0);
        multRt.anchorMax = new Vector2(1, 0.5f);
        multRt.offsetMin = new Vector2(12, 4);
        multRt.offsetMax = new Vector2(-12, 0);
        globalTechMultiplierText = multObj.GetComponent<TextMeshProUGUI>();
        globalTechMultiplierText.fontSize = 12;

        // Scroll View with Tech Nodes
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 68);
        scrollRt.offsetMax = new Vector2(-18, -135);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        techNodesContainer = contentObj.transform;
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;

        for (int i = 0; i < techNodes.Count; i++)
        {
            CreateTechCardTemplate(techNodesContainer, techNodes[i], i);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 14);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeTechLabBtn = closeBtnObj.GetComponent<Button>();

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

    private void CreateTechCardTemplate(Transform parent, TechNode node, int index)
    {
        GameObject card = new GameObject($"TechCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 84);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{node.icon} <b>{node.title}</b> <color=#90B0D0>[{node.branchName}]</color>";
        tt.fontSize = 13;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.65f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{node.perkDesc}</color>";
        dt.fontSize = 11;

        GameObject b = new GameObject("ResBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-10, -4);
        brt.sizeDelta = new Vector2(150, 34);
        b.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.75f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = $"ИЗУЧИТЬ ({NumberFormatter.Format(node.codeCost)})";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }
}
