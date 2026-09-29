using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// VR/AR Лаборатория Нового Поколения («Next-Gen Spatial Computing Lab»):
/// - 4 вектора исследований: Микро-OLED оптика, 6DoF трекинг, пространственный звук, Haptic-перчатки
/// - 3 поколения гарнитур: TapGlass v1 -> TapVision Studio -> Quantum HoloLens
/// - Интерактивное тестирование пространственных демо-приложений «TEST SPATIAL DEMO»
/// - Перманентные бонусы к силе клика, комбо и доходам компании
/// </summary>
public class SpatialComputingLabUI : MonoBehaviour
{
    private static SpatialComputingLabUI instance;
    public static SpatialComputingLabUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SpatialComputingLabUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(SpatialComputingLabUI));
                    instance = go.AddComponent<SpatialComputingLabUI>();
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
    public class SpatialTech
    {
        public string id;
        public string title;
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
    [SerializeField] private Button openSpatialLabBtn;

    [Header("Информационный заголовок и тестирование")]
    [SerializeField] private TMP_Text labSummaryTxt;
    [SerializeField] private Button testDemoBtn;
    [SerializeField] private TMP_Text testDemoBtnTxt;
    [SerializeField] private Transform techContainer;

    private readonly List<SpatialTech> techs = new List<SpatialTech>();
    private int headsetTier = 1;
    private bool isTestingDemo = false;

    private const string PrefHeadsetTier = "Spatial_HeadsetTier";
    private const string PrefTechPrefix = "Spatial_TechLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTechs();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeTechs()
    {
        techs.Clear();

        techs.Add(new SpatialTech
        {
            id = "spatial_optics",
            title = "Микро-OLED 8K & Pancake-оптика",
            icon = "🥽",
            description = "120 FPS без укачивания и задержка фотон-к-движению < 5 мс",
            perkDesc = "+3% строк кода за клик за уровень",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 8000,
            baseCostCode = 2000
        });

        techs.Add(new SpatialTech
        {
            id = "spatial_tracking",
            title = "Inside-Out 6DoF & Hand Tracking",
            icon = "👋",
            description = "Нейросетевое отслеживание пальцев без контроллеров",
            perkDesc = "+4% к длительности комбо и энергии за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 18000,
            baseCostCode = 4500
        });

        techs.Add(new SpatialTech
        {
            id = "spatial_audio",
            title = "Бинауральное Пространственное Аудио",
            icon = "🎧",
            description = "Трассировка звуковых лучей с учетом геометрии комнат",
            perkDesc = "+4% к пассивному доходу студии за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 35000,
            baseCostCode = 8500
        });

        techs.Add(new SpatialTech
        {
            id = "spatial_haptics",
            title = "Тактильные Haptic-перчатки",
            icon = "🧤",
            description = "Пневматическая обратная связь и текстуры виртуальных миров",
            perkDesc = "+5% к выплатам за релизы за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 70000,
            baseCostCode = 16000
        });
    }

    private void LoadData()
    {
        headsetTier = PlayerPrefs.GetInt(PrefHeadsetTier, 1);
        for (int i = 0; i < techs.Count; i++)
        {
            int defLvl = techs[i].id == "spatial_optics" ? 1 : 0;
            techs[i].level = PlayerPrefs.GetInt(PrefTechPrefix + techs[i].id, defLvl);
            techs[i].level = Mathf.Clamp(techs[i].level, 0, techs[i].maxLevel);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefHeadsetTier, headsetTier);
        for (int i = 0; i < techs.Count; i++)
        {
            PlayerPrefs.SetInt(PrefTechPrefix + techs[i].id, techs[i].level);
        }
        PlayerPrefs.Save();
    }

    public double GetSpatialIncomeMultiplier()
    {
        var hapt = techs.Find(t => t.id == "spatial_haptics");
        int hLvl = hapt != null ? hapt.level : 0;
        var aud = techs.Find(t => t.id == "spatial_audio");
        int aLvl = aud != null ? aud.level : 0;
        return 1.0 + (hLvl * 0.05) + (aLvl * 0.04) + (headsetTier - 1) * 0.10;
    }

    public double GetSpatialClickMultiplier()
    {
        var opt = techs.Find(t => t.id == "spatial_optics");
        int oLvl = opt != null ? opt.level : 0;
        return 1.0 + (oLvl * 0.03);
    }

    public string GetHeadsetName()
    {
        if (headsetTier >= 3) return "🔮 Quantum HoloLens Ultimate";
        if (headsetTier >= 2) return "🥽 TapVision Studio Pro";
        return "👓 TapGlass v1 Prototype";
    }

    public void TestSpatialDemo()
    {
        if (isTestingDemo) return;
        StartCoroutine(TestDemoRoutine());
    }

    private IEnumerator TestDemoRoutine()
    {
        isTestingDemo = true;
        if (testDemoBtn != null) testDemoBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (testDemoBtnTxt != null)
        {
            testDemoBtnTxt.text = "⏳ КАЛИБРОВКА 6DoF И РЕНДЕР СЦЕНЫ...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        double baseReward = Math.Max(6000.0, (GameManager.Instance != null ? GameManager.Instance.Money * 0.04 : 12000.0));
        double rewardMoney = Math.Floor(baseReward * (1.0 + headsetTier * 0.3));
        double rewardCode = Math.Floor(rewardMoney * 0.08);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.20f);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🥽 SPATIAL ДЕМО ЗАВЕРШЕНО!\nГрант за инновацию: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} кода)", transform.position, new Color(0.2f, 0.9f, 1f), true);
        }

        isTestingDemo = false;
        if (testDemoBtn != null) testDemoBtn.interactable = true;
        if (testDemoBtnTxt != null) testDemoBtnTxt.text = "🥽 ЗАПУСТИТЬ ТЕСТ SPATIAL DEMO";
        UpdateModalUI();
    }

    public bool TryUpgradeTech(string techId)
    {
        var t = techs.Find(x => x.id == techId);
        if (t == null || t.level >= t.maxLevel) return false;

        double costMoney = t.GetCostMoney();
        double costCode = t.GetCostCode();

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

        t.level++;

        // Проверяем возможность апгрейда поколения гарнитуры
        int minLvl = 99;
        for (int i = 0; i < techs.Count; i++) if (techs[i].level < minLvl) minLvl = techs[i].level;
        if (minLvl >= 5 && headsetTier < 3) headsetTier = 3;
        else if (minLvl >= 2 && headsetTier < 2) headsetTier = 2;

        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ SPATIAL ТЕХНОЛОГИЯ УЛУЧШЕНА!\n{t.icon} {t.title} -> ур. {t.level}", transform.position, new Color(0.2f, 0.85f, 1f), true);
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
        if (openSpatialLabBtn != null)
        {
            openSpatialLabBtn.onClick.RemoveAllListeners();
            openSpatialLabBtn.onClick.AddListener(OpenModal);
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
        if (testDemoBtn != null)
        {
            testDemoBtn.onClick.RemoveAllListeners();
            testDemoBtn.onClick.AddListener(TestSpatialDemo);
        }
    }

    private void UpdateModalUI()
    {
        if (labSummaryTxt != null)
        {
            double incB = (GetSpatialIncomeMultiplier() - 1.0) * 100.0;
            double clkB = (GetSpatialClickMultiplier() - 1.0) * 100.0;
            labSummaryTxt.text = $"Гарнитура: <b><color=#00E5FF>{GetHeadsetName()}</color></b>\nДоход: <b><color=#00FF88>+{incB:F0}%</color></b> | Клик: <b><color=#FFD700>+{clkB:F0}%</color></b>";
        }

        RefreshTechCards();
    }

    private void RefreshTechCards()
    {
        if (techContainer == null) return;

        for (int i = 0; i < techs.Count; i++)
        {
            var t = techs[i];
            Transform cardTr = techContainer.Find($"TechCard_{t.id}");
            if (cardTr == null) continue;

            TMP_Text lvlTxt = cardTr.Find("LevelTxt")?.GetComponent<TMP_Text>();
            if (lvlTxt != null)
            {
                lvlTxt.text = t.level >= t.maxLevel ? "<color=#FFD700>МАКС</color>" : $"Ур. {t.level}/{t.maxLevel}";
            }

            Button upgBtn = cardTr.Find("UpgradeBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = upgBtn != null ? upgBtn.GetComponentInChildren<TMP_Text>() : null;

            if (upgBtn != null && btnTxt != null)
            {
                string tId = t.id;
                upgBtn.onClick.RemoveAllListeners();
                upgBtn.onClick.AddListener(() => TryUpgradeTech(tId));

                if (t.level >= t.maxLevel)
                {
                    upgBtn.interactable = false;
                    btnTxt.text = "МАКС. УРОВЕНЬ";
                }
                else
                {
                    double costMoney = t.GetCostMoney();
                    double costCode = t.GetCostCode();
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= costMoney &&
                                     GameManager.Instance.CodeLines >= costCode;
                    upgBtn.interactable = canAfford;
                    btnTxt.text = $"ИЗУЧИТЬ\n{NumberFormatter.Format(costMoney)} ₽ | {NumberFormatter.Format(costCode)} Кода";
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
        GameObject root = new GameObject("SpatialComputingModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.85f, 1f, 0.6f);
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
        headerTxt.text = "🥽 SPATIAL COMPUTING & VR/AR LAB";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.1f, 0.9f, 1f);

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

        // Lab Summary Box
        GameObject sumBox = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumBox.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumBox.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 60);
        sumBox.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.9f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumBox.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        labSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        labSummaryTxt.fontSize = 11;
        labSummaryTxt.alignment = TextAlignmentOptions.Center;
        labSummaryTxt.color = new Color(0.9f, 0.95f, 1f);

        // Test Demo Action Button
        GameObject demoObj = new GameObject("TestDemoBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        demoObj.transform.SetParent(cardObj.transform, false);
        RectTransform demoRt = demoObj.GetComponent<RectTransform>();
        demoRt.anchorMin = new Vector2(0, 1);
        demoRt.anchorMax = new Vector2(1, 1);
        demoRt.pivot = new Vector2(0.5f, 1);
        demoRt.anchoredPosition = new Vector2(0, -120);
        demoRt.sizeDelta = new Vector2(-36, 40);
        demoObj.GetComponent<Image>().color = new Color(0.1f, 0.55f, 0.85f, 0.95f);
        testDemoBtn = demoObj.GetComponent<Button>();

        GameObject demoTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        demoTxtObj.transform.SetParent(demoObj.transform, false);
        RectTransform demoTxtRt = demoTxtObj.GetComponent<RectTransform>();
        demoTxtRt.anchorMin = Vector2.zero; demoTxtRt.anchorMax = Vector2.one; demoTxtRt.sizeDelta = Vector2.zero;
        testDemoBtnTxt = demoTxtObj.GetComponent<TextMeshProUGUI>();
        testDemoBtnTxt.fontSize = 11;
        testDemoBtnTxt.fontStyle = FontStyles.Bold;
        testDemoBtnTxt.alignment = TextAlignmentOptions.Center;
        testDemoBtnTxt.text = "🥽 ЗАПУСТИТЬ ТЕСТ SPATIAL DEMO";
        testDemoBtnTxt.color = Color.white;

        // Scroll Container for Techs
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -170);
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
        techContainer = contentObj.transform;

        for (int i = 0; i < techs.Count; i++)
        {
            CreateTechCardTemplate(techContainer, techs[i]);
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

    private void CreateTechCardTemplate(Transform parent, SpatialTech tech)
    {
        GameObject card = new GameObject($"TechCard_{tech.id}", typeof(RectTransform), typeof(Image));
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
        tt.text = $"{tech.icon} <b>{tech.title}</b>";
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
        lt.text = $"Ур. {tech.level}/{tech.maxLevel}";
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
        dt.text = $"Эффект: <color=#00FF88>{tech.perkDesc}</color>";
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
        btxt.text = "ИЗУЧИТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openSpatialLabBtn != null) return;

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

        GameObject btnGo = new GameObject("SpatialHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.1f, 0.35f, 0.55f, 0.9f);
        openSpatialLabBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🥽 VR/AR";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
