using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Зал Достижений и Стенд Рекордов («Hall of Fame & Trophy Museum»):
/// - Интерактивный музей легендарных артефактов студии:
///   1. Floppy 1998 (Первая 3.5" дискета с дебютной инди-игрой)
///   2. Golden Disc (Памятный диск за 1 000 000 проданных копий)
///   3. Anti-DDoS CPU (Процессор стойкого сервера, выдержавший терабитный наплыв)
///   4. C# Creator Autograph (Реликвия с автографом создателя C#)
///   5. Major World Trophy (Хрустальный кубок всемирного киберспортивного триумфа)
/// - Реставрация и расширение стендов каждого экспоната с ростом перманентных бонусов
/// - Механика «ПРОВЕСТИ ЭКСКУРСИЮ»: сбор билетов, донаты от фанатов, взрывной прирост хайпа и дохода
/// </summary>
public class HallOfFameMuseumUI : MonoBehaviour
{
    private static HallOfFameMuseumUI instance;
    public static HallOfFameMuseumUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<HallOfFameMuseumUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(HallOfFameMuseumUI));
                    instance = go.AddComponent<HallOfFameMuseumUI>();
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
    public class MuseumExhibit
    {
        public string id;
        public string title;
        public string subtitle;
        public string icon;
        public string lore;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public float incomeMultiplierBonusPerLevel;
        public float clickMultiplierBonusPerLevel;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.50, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.45, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openMuseumBtn;

    [Header("Интерактивная экскурсия и стенды")]
    [SerializeField] private TMP_Text museumHeaderStatsTxt;
    [SerializeField] private Button startTourBtn;
    [SerializeField] private TMP_Text startTourBtnTxt;
    [SerializeField] private Slider tourProgressBar;
    [SerializeField] private Transform exhibitsContainer;

    private readonly List<MuseumExhibit> exhibits = new List<MuseumExhibit>();
    private bool isTourActive = false;
    private float tourTimer = 0f;
    private const float TourDuration = 8f;

    private const string PrefExhibitPrefix = "Museum_ExhibitLvl_";
    private const string PrefToursCompleted = "Museum_ToursCompleted";
    private int totalToursCompleted = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeExhibits();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeExhibits()
    {
        if (exhibits.Count > 0) return;

        exhibits.Add(new MuseumExhibit
        {
            id = "floppy_1998",
            title = "Первая дискета 1.44MB (1998)",
            subtitle = "Реликвия эпохи MS-DOS",
            icon = "💾",
            lore = "Та самая 3.5-дюймовая дискета, на которую была записана первая текстовая RPG студии в гараже.",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 25000,
            baseCostCode = 10000,
            incomeMultiplierBonusPerLevel = 0.08f,
            clickMultiplierBonusPerLevel = 0.05f
        });

        exhibits.Add(new MuseumExhibit
        {
            id = "golden_disc_1m",
            title = "Золотой диск 1,000,000 копий",
            subtitle = "Символ мирового признания",
            icon = "📀",
            lore = "Сверкающий диск в рамке из красного дерева, увековечивший первый миллион проданных лицензий.",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 150000,
            baseCostCode = 75000,
            incomeMultiplierBonusPerLevel = 0.15f,
            clickMultiplierBonusPerLevel = 0.10f
        });

        exhibits.Add(new MuseumExhibit
        {
            id = "antiddos_cpu",
            title = "Процессор Xeon (Выживший в DDoS)",
            subtitle = "Стержень сетевой крепости",
            icon = "⚡",
            lore = "Легендарный кристалл кремния, выдержавший терабитную ботнет-атаку конкурентов в день мирового релиза.",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 500000,
            baseCostCode = 250000,
            incomeMultiplierBonusPerLevel = 0.25f,
            clickMultiplierBonusPerLevel = 0.15f
        });

        exhibits.Add(new MuseumExhibit
        {
            id = "csharp_autograph",
            title = "Автограф Архитектора C#",
            subtitle = "Священная скрижаль синтаксиса",
            icon = "📜",
            lore = "Оригинальная рукописная спецификация языка с автографом создателя: 'Code with passion and zero GC pressure'.",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 1500000,
            baseCostCode = 800000,
            incomeMultiplierBonusPerLevel = 0.35f,
            clickMultiplierBonusPerLevel = 0.30f
        });

        exhibits.Add(new MuseumExhibit
        {
            id = "major_championship_trophy",
            title = "Кубок Мирового Гранд-Финала",
            subtitle = "Абсолютный триумф киберспорта",
            icon = "🏆",
            lore = "Хрустальный трофей высшей мировой лиги, добытый нашей киберспортивной командой на глазах 50 млн зрителей.",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 5000000,
            baseCostCode = 2500000,
            incomeMultiplierBonusPerLevel = 0.50f,
            clickMultiplierBonusPerLevel = 0.40f
        });
    }

    private void LoadData()
    {
        totalToursCompleted = PlayerPrefs.GetInt(PrefToursCompleted, 0);
        foreach (var exh in exhibits)
        {
            if (PlayerPrefs.HasKey(PrefExhibitPrefix + exh.id))
            {
                exh.level = PlayerPrefs.GetInt(PrefExhibitPrefix + exh.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefToursCompleted, totalToursCompleted);
        foreach (var exh in exhibits)
        {
            PlayerPrefs.SetInt(PrefExhibitPrefix + exh.id, exh.level);
        }
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (isTourActive)
        {
            tourTimer += Time.deltaTime;
            if (tourProgressBar != null)
            {
                tourProgressBar.value = Mathf.Clamp01(tourTimer / TourDuration);
            }

            if (startTourBtnTxt != null)
            {
                startTourBtnTxt.text = $"🚶 ЭКСКУРСИЯ ИДЕТ... ({TourDuration - tourTimer:0.0}с)";
            }

            if (tourTimer >= TourDuration)
            {
                CompleteMuseumTour();
            }
        }
    }

    public double GetMuseumIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var exh in exhibits)
        {
            mult += exh.level * exh.incomeMultiplierBonusPerLevel;
        }
        mult += Math.Min(totalToursCompleted * 0.02, 2.0);
        return mult;
    }

    public double GetMuseumClickMultiplier()
    {
        double mult = 1.0;
        foreach (var exh in exhibits)
        {
            mult += exh.level * exh.clickMultiplierBonusPerLevel;
        }
        return mult;
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
        if (startTourBtn != null)
        {
            startTourBtn.onClick.RemoveAllListeners();
            startTourBtn.onClick.AddListener(OnStartTourClicked);
        }
        if (openMuseumBtn != null)
        {
            openMuseumBtn.onClick.RemoveAllListeners();
            openMuseumBtn.onClick.AddListener(OpenModal);
        }
    }

    private void OnStartTourClicked()
    {
        if (isTourActive) return;

        isTourActive = true;
        tourTimer = 0f;
        if (startTourBtn != null) startTourBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("🏛️ Музей открыл двери для VIP-экскурсии!", transform.position, new Color(1f, 0.85f, 0.2f), false);
        }
    }

    private void CompleteMuseumTour()
    {
        isTourActive = false;
        tourTimer = 0f;
        totalToursCompleted++;

        if (startTourBtn != null) startTourBtn.interactable = true;
        if (tourProgressBar != null) tourProgressBar.value = 0f;

        // Награда за экскурсию: билеты + донаты посетителей
        double baseTicket = 20000 * Math.Max(1, totalToursCompleted);
        double moneyReward = baseTicket * GetMuseumIncomeMultiplier();
        double codeReward = baseTicket * 0.4 * GetMuseumClickMultiplier();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
            GameManager.Instance.AddLinesOfCode(codeReward);
            GameManager.Instance.AddComboEnergy(0.20f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 Экскурсия завершена!\nВыручка: <b>+{NumberFormatter.Format(moneyReward)} ₽</b> (+{NumberFormatter.Format(codeReward)} C#)", transform.position, new Color(0.2f, 1f, 0.4f), true);
        }
    }

    public void UpgradeExhibit(string exhibitId)
    {
        var exh = exhibits.Find(e => e.id == exhibitId);
        if (exh == null) return;

        if (exh.level >= exh.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Экспонат уже отреставрирован до максимума!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = exh.GetCostMoney();
        double costCode = exh.GetCostCode();

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

        exh.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Экспонат '{exh.title}' отреставрирован до Ур.{exh.level}!", transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
    }

    public void RefreshUI()
    {
        if (museumHeaderStatsTxt != null)
        {
            double incMult = GetMuseumIncomeMultiplier();
            double clkMult = GetMuseumClickMultiplier();
            museumHeaderStatsTxt.text = $"Множитель дохода музея: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>\nПроведено экскурсий: <b>{totalToursCompleted}</b>";
        }

        if (startTourBtnTxt != null && !isTourActive)
        {
            startTourBtnTxt.text = "🏛️ ПРОВЕСТИ VIP-ЭКСКУРСИЮ (8 сек)";
        }

        if (exhibitsContainer == null) return;

        // Обновляем карточки экспонатов
        for (int i = 0; i < exhibits.Count; i++)
        {
            var exh = exhibits[i];
            Transform child = i < exhibitsContainer.childCount ? exhibitsContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            TMP_Text statsTxt = child.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
            Button upBtn = child.Find("Action/UpgradeBtn")?.GetComponent<Button>();
            TMP_Text upBtnTxt = upBtn != null ? upBtn.GetComponentInChildren<TMP_Text>() : null;

            if (titleTxt != null)
            {
                titleTxt.text = $"{exh.icon} {exh.title} (Ур. {exh.level}/{exh.maxLevel})";
            }
            if (descTxt != null)
            {
                descTxt.text = $"<i>{exh.subtitle}</i>\n{exh.lore}";
            }
            if (statsTxt != null)
            {
                float totalInc = exh.level * exh.incomeMultiplierBonusPerLevel * 100f;
                float totalClk = exh.level * exh.clickMultiplierBonusPerLevel * 100f;
                statsTxt.text = $"Бонус: <color=#00FFAA>+{totalInc:0}% доход</color> | <color=#FFD700>+{totalClk:0}% клик</color>";
            }

            if (upBtn != null && upBtnTxt != null)
            {
                if (exh.level >= exh.maxLevel)
                {
                    upBtnTxt.text = "MAX УРОВЕНЬ";
                    upBtn.interactable = false;
                }
                else
                {
                    double m = exh.GetCostMoney();
                    double c = exh.GetCostCode();
                    upBtnTxt.text = $"Реставрация\n{NumberFormatter.Format(m)} ₽ | {NumberFormatter.Format(c)} C#";
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

        // Создаем модальное окно музейного зала
        GameObject root = new GameObject("HallOfFameMuseum_ModalRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        modalRoot = root;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Затемнение фона
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(root.transform, false);
        RectTransform bRect = backdrop.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        Image bImg = backdrop.GetComponent<Image>();
        bImg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Главная карточка музея
        GameObject card = new GameObject("MuseumCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 700);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.12f, 0.13f, 0.18f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 70);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.18f, 0.20f, 0.28f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🏛️ ЗАЛ СЛАВЫ И МУЗЕЙ РЕКОРДОВ";
        tTxt.fontSize = 20;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(1f, 0.85f, 0.3f);
        RectTransform tRect = titleObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = new Vector2(20, 0);
        tRect.offsetMax = new Vector2(-70, 0);

        // Close X Button
        GameObject xBtnObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        xBtnObj.transform.SetParent(header.transform, false);
        RectTransform xRect = xBtnObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 0.5f);
        xRect.anchorMax = new Vector2(1f, 0.5f);
        xRect.pivot = new Vector2(1f, 0.5f);
        xRect.sizeDelta = new Vector2(40, 40);
        xRect.anchoredPosition = new Vector2(-15, 0);
        xBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.85f);
        closeXBtn = xBtnObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(xBtnObj.transform, false);
        TextMeshProUGUI xTxt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xTxt.text = "✕";
        xTxt.fontSize = 22;
        xTxt.fontStyle = FontStyles.Bold;
        xTxt.alignment = TextAlignmentOptions.Center;
        xTxt.color = Color.white;
        RectTransform xtRect = xTxtObj.GetComponent<RectTransform>();
        xtRect.anchorMin = Vector2.zero;
        xtRect.anchorMax = Vector2.one;
        xtRect.offsetMin = Vector2.zero;
        xtRect.offsetMax = Vector2.zero;

        // Subheader Stats
        GameObject statsObj = new GameObject("MuseumStatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 45);
        stRect.anchoredPosition = new Vector2(0, -75);
        museumHeaderStatsTxt = statsObj.GetComponent<TextMeshProUGUI>();
        museumHeaderStatsTxt.fontSize = 13;
        museumHeaderStatsTxt.alignment = TextAlignmentOptions.Center;
        museumHeaderStatsTxt.color = new Color(0.9f, 0.9f, 0.95f);

        // Action: Экскурсия кнопка и прогресс-бар
        GameObject tourPanel = new GameObject("TourPanel", typeof(RectTransform), typeof(Image));
        tourPanel.transform.SetParent(card.transform, false);
        RectTransform tpRect = tourPanel.GetComponent<RectTransform>();
        tpRect.anchorMin = new Vector2(0f, 1f);
        tpRect.anchorMax = new Vector2(1f, 1f);
        tpRect.pivot = new Vector2(0.5f, 1f);
        tpRect.sizeDelta = new Vector2(-30, 60);
        tpRect.anchoredPosition = new Vector2(0, -125);
        tourPanel.GetComponent<Image>().color = new Color(0.16f, 0.18f, 0.25f, 1f);

        GameObject tourBtnObj = new GameObject("StartTourBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        tourBtnObj.transform.SetParent(tourPanel.transform, false);
        RectTransform tbRect = tourBtnObj.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 0f);
        tbRect.anchorMax = new Vector2(1f, 1f);
        tbRect.offsetMin = new Vector2(10, 8);
        tbRect.offsetMax = new Vector2(-10, -8);
        tourBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.65f, 0.38f, 1f);
        startTourBtn = tourBtnObj.GetComponent<Button>();

        GameObject tourTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        tourTxtObj.transform.SetParent(tourBtnObj.transform, false);
        startTourBtnTxt = tourTxtObj.GetComponent<TextMeshProUGUI>();
        startTourBtnTxt.text = "🏛️ ПРОВЕСТИ VIP-ЭКСКУРСИЮ (8 сек)";
        startTourBtnTxt.fontSize = 15;
        startTourBtnTxt.fontStyle = FontStyles.Bold;
        startTourBtnTxt.alignment = TextAlignmentOptions.Center;
        startTourBtnTxt.color = Color.white;
        RectTransform tbtRect = tourTxtObj.GetComponent<RectTransform>();
        tbtRect.anchorMin = Vector2.zero;
        tbtRect.anchorMax = Vector2.one;
        tbtRect.offsetMin = Vector2.zero;
        tbtRect.offsetMax = Vector2.zero;

        // Scrollable Exhibits Container
        GameObject scrollObj = new GameObject("ExhibitsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -195);
        scrollObj.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.5f);

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
        exhibitsContainer = content.transform;
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

        // Создаем карточки стендов для каждого экспоната
        foreach (var exh in exhibits)
        {
            CreateExhibitCardUI(content.transform, exh);
        }

        // Нижняя кнопка закрытия
        GameObject botCloseObj = new GameObject("CloseBottomBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 42);
        bcRect.anchoredPosition = new Vector2(0, 10);
        botCloseObj.GetComponent<Image>().color = new Color(0.25f, 0.28f, 0.38f, 1f);
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

    private void CreateExhibitCardUI(Transform parent, MuseumExhibit exh)
    {
        GameObject card = new GameObject("ExhibitCard_" + exh.id, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 95);
        card.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.24f, 1f);

        // Header
        GameObject hdr = new GameObject("Header", typeof(RectTransform));
        hdr.transform.SetParent(card.transform, false);
        RectTransform hr = hdr.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(1f, 1f);
        hr.pivot = new Vector2(0.5f, 1f);
        hr.sizeDelta = new Vector2(-20, 26);
        hr.anchoredPosition = new Vector2(10, -6);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(hdr.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 14;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = new Color(1f, 0.88f, 0.4f);
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
        br.anchorMax = new Vector2(0.68f, 1f);
        br.offsetMin = new Vector2(10, 8);
        br.offsetMax = new Vector2(0, -32);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.color = new Color(0.85f, 0.88f, 0.95f);
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
        ar.anchorMin = new Vector2(0.69f, 0f);
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
        upBtn.GetComponent<Image>().color = new Color(0.22f, 0.55f, 0.88f, 1f);

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

        string currentExhId = exh.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeExhibit(currentExhId));
    }
}
