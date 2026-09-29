using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Генератор Случайных Офисных ЧП («Chaos Engineering & Incident Response»):
/// - Случайные критические ситуации в офисе студии во время активной игры:
///   1. 🚨 Пожарная тревога в серверной стойке (нужно тушить огнетушителем)
///   2. 🐈 Офисный кот выдернул главный силовой кабель питания
///   3. ☕ Разлитый горячий эспрессо на кастомную механическую клавиатуру
///   4. ⚡ Скачок напряжения в сети и перегрев серверных кластеров
/// - Аркадная механика Quick-Time Event (QTE): быстрые тапы по экрану до истечения таймера
/// - Награда за быструю реакцию: мгновенное заполнение комбо "В Потоке" на 100% (x3.0),
///   страховая премия в рублях и строках кода C#, а также временный турбо-буст
/// </summary>
public class ChaosIncidentResponseUI : MonoBehaviour
{
    private static ChaosIncidentResponseUI instance;
    public static ChaosIncidentResponseUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ChaosIncidentResponseUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(ChaosIncidentResponseUI));
                    instance = go.AddComponent<ChaosIncidentResponseUI>();
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
    public class IncidentDefinition
    {
        public string id;
        public string title;
        public string description;
        public string actionName;
        public string icon;
        public int requiredTaps;
        public float timeLimitSeconds;
        public Color bannerColor;
    }

    [Header("UI элементы QTE тревоги")]
    [SerializeField] private GameObject qteBannerRoot;
    [SerializeField] private TMP_Text incidentTitleTxt;
    [SerializeField] private TMP_Text incidentDescTxt;
    [SerializeField] private TMP_Text tapsRemainingTxt;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Button actionTapBtn;
    [SerializeField] private TMP_Text actionTapBtnTxt;

    [Header("Модальное окно штаба ЧП и тренировок")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button triggerDrillBtn;
    [SerializeField] private TMP_Text triggerDrillBtnTxt;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openChaosBtn;

    private readonly List<IncidentDefinition> incidentPool = new List<IncidentDefinition>();
    private IncidentDefinition currentIncident = null;
    private int currentTapsDone = 0;
    private float incidentTimer = 0f;
    private bool isIncidentActive = false;

    private float nextRandomIncidentCooldown = 90f;
    private float drillCooldown = 0f;

    private int totalIncidentsResolved = 0;
    private const string PrefResolvedCount = "Chaos_IncidentsResolvedCount";

    public bool IsIncidentActive => isIncidentActive;
    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializePool();
        LoadData();

        if (qteBannerRoot != null) qteBannerRoot.SetActive(false);
        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializePool()
    {
        if (incidentPool.Count > 0) return;

        incidentPool.Add(new IncidentDefinition
        {
            id = "fire_alarm",
            title = "🚨 ПОЖАРНАЯ ТРЕВОГА В СЕРВЕРНОЙ!",
            description = "В стойке #3 перегрелся блок питания! Быстро сбейте пламя огнетушителем!",
            actionName = "🧯 ТУШИТЬ ПЛАМЯ ОГНЕТУШИТЕЛЕМ!",
            icon = "🔥",
            requiredTaps = 8,
            timeLimitSeconds = 6.0f,
            bannerColor = new Color(0.85f, 0.20f, 0.15f, 0.95f)
        });

        incidentPool.Add(new IncidentDefinition
        {
            id = "cat_power_cut",
            title = "🐈 ОФИСНЫЙ КОТ ВЫДЕРНУЛ ПИТАНИЕ!",
            description = "Кот запутался в шнурах и выдернул силовой кабель главного свитча студии!",
            actionName = "🔌 ПОЙМАТЬ КОТА И ВКЛЮЧИТЬ КАБЕЛЬ!",
            icon = "🐱",
            requiredTaps = 6,
            timeLimitSeconds = 5.0f,
            bannerColor = new Color(0.90f, 0.55f, 0.10f, 0.95f)
        });

        incidentPool.Add(new IncidentDefinition
        {
            id = "coffee_spill",
            title = "☕ ДВОЙНОЙ ЭСПРЕССО НА КЛАВИАТУРЕ!",
            description = "Во время релиза разлита кружка горячего кофе! Быстро продуйте свитчи!",
            actionName = "💨 СУШИТЬ СЖАТЫМ ВОЗДУХОМ!",
            icon = "☕",
            requiredTaps = 7,
            timeLimitSeconds = 5.5f,
            bannerColor = new Color(0.65f, 0.35f, 0.15f, 0.95f)
        });

        incidentPool.Add(new IncidentDefinition
        {
            id = "power_surge",
            title = "⚡ СКАЧОК НАПРЯЖЕНИЯ И ПЕРЕГРЕВ!",
            description = "Гроза вызвала перепад в подстанции студии! Переключите аварийный рубильник ИБП!",
            actionName = "⚡ ПЕРЕКЛЮЧИТЬ АВАРИЙНЫЙ РУБИЛЬНИК!",
            icon = "⚡",
            requiredTaps = 9,
            timeLimitSeconds = 6.5f,
            bannerColor = new Color(0.75f, 0.15f, 0.65f, 0.95f)
        });
    }

    private void LoadData()
    {
        totalIncidentsResolved = PlayerPrefs.GetInt(PrefResolvedCount, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefResolvedCount, totalIncidentsResolved);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Таймер кулдауна тренировок
        if (drillCooldown > 0f)
        {
            drillCooldown -= dt;
            if (triggerDrillBtnTxt != null)
            {
                triggerDrillBtnTxt.text = $"⏳ ПЕРЕЗАРЯДКА УЧЕНИЙ ({drillCooldown:0}с)";
            }
        }
        else if (triggerDrillBtnTxt != null && !isIncidentActive)
        {
            triggerDrillBtnTxt.text = "🚨 ЗАПУСТИТЬ УЧЕБНУЮ ТРЕВОГУ";
        }

        // Логика активного ЧП
        if (isIncidentActive && currentIncident != null)
        {
            incidentTimer -= dt;

            if (timerSlider != null)
            {
                timerSlider.value = Mathf.Clamp01(incidentTimer / currentIncident.timeLimitSeconds);
            }

            if (incidentTimer <= 0f)
            {
                FailIncident();
            }
        }
        else
        {
            // Случайный спавн во время активного кликанья
            nextRandomIncidentCooldown -= dt;
            if (nextRandomIncidentCooldown <= 0f)
            {
                nextRandomIncidentCooldown = UnityEngine.Random.Range(75f, 135f);
                TriggerRandomIncident();
            }
        }
    }

    public void TriggerRandomIncident()
    {
        if (isIncidentActive || incidentPool.Count == 0) return;

        int r = UnityEngine.Random.Range(0, incidentPool.Count);
        StartIncident(incidentPool[r]);
    }

    public void TriggerDrill()
    {
        if (isIncidentActive || drillCooldown > 0f) return;

        drillCooldown = 45f;
        CloseModal();
        TriggerRandomIncident();
    }

    private void StartIncident(IncidentDefinition def)
    {
        EnsureUIExists();

        currentIncident = def;
        currentTapsDone = 0;
        incidentTimer = def.timeLimitSeconds;
        isIncidentActive = true;

        if (qteBannerRoot != null)
        {
            qteBannerRoot.SetActive(true);

            Image bImg = qteBannerRoot.GetComponent<Image>();
            if (bImg != null) bImg.color = def.bannerColor;
        }

        if (incidentTitleTxt != null) incidentTitleTxt.text = $"{def.icon} {def.title}";
        if (incidentDescTxt != null) incidentDescTxt.text = def.description;
        if (actionTapBtnTxt != null) actionTapBtnTxt.text = def.actionName;

        UpdateTapCounterText();

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🚨 ВНИМАНИЕ: {def.title}!\nБЫСТРО КЛИКАЙТЕ!", transform.position, Color.red, true);
        }
    }

    public void OnActionTapClicked()
    {
        if (!isIncidentActive || currentIncident == null) return;

        currentTapsDone++;
        UpdateTapCounterText();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (currentTapsDone >= currentIncident.requiredTaps)
        {
            ResolveIncidentSuccessfully();
        }
    }

    private void UpdateTapCounterText()
    {
        if (tapsRemainingTxt != null && currentIncident != null)
        {
            int remaining = Math.Max(0, currentIncident.requiredTaps - currentTapsDone);
            tapsRemainingTxt.text = $"ОСТАЛОСЬ ТАПОВ: <b>{remaining}</b> / {currentIncident.requiredTaps} (Время: {incidentTimer:0.0}с)";
        }
    }

    private void ResolveIncidentSuccessfully()
    {
        isIncidentActive = false;
        totalIncidentsResolved++;
        SaveData();

        if (qteBannerRoot != null) qteBannerRoot.SetActive(false);

        // Награда за успешную реакцию
        double curMoney = GameManager.Instance != null ? GameManager.Instance.Money : 50000.0;
        double rewardMoney = Math.Max(25000.0, Math.Floor(curMoney * 0.05));
        double rewardCode = Math.Max(12000.0, Math.Floor(rewardMoney * 0.4));

        if (GameManager.Instance != null)
        {
            // Максимум комбо "В Потоке"
            GameManager.Instance.AddComboEnergy(1.0f);
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 ЧП УСПЕШНО ЛИКВИДИРОВАНО!\nКомбо 100% (x3.0) + Страховка: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.2f, 1f, 0.4f), true);
        }

        RefreshUI();
    }

    private void FailIncident()
    {
        isIncidentActive = false;
        if (qteBannerRoot != null) qteBannerRoot.SetActive(false);

        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("⚠️ Время вышло! ЧП ликвидировано аварийной службой (сброс комбо).", transform.position, new Color(1f, 0.4f, 0.3f), false);
        }

        RefreshUI();
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
        if (actionTapBtn != null)
        {
            actionTapBtn.onClick.RemoveAllListeners();
            actionTapBtn.onClick.AddListener(OnActionTapClicked);
        }
        if (triggerDrillBtn != null)
        {
            triggerDrillBtn.onClick.RemoveAllListeners();
            triggerDrillBtn.onClick.AddListener(TriggerDrill);
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
        if (openChaosBtn != null)
        {
            openChaosBtn.onClick.RemoveAllListeners();
            openChaosBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        if (statsSummaryTxt != null)
        {
            statsSummaryTxt.text = $"Штаб Chaos Engineering студии\nУспешно ликвидировано ЧП: <color=#00FFAA><b>{totalIncidentsResolved}</b></color>\nБонус за реакцию: <color=#FFD700>Мгновенный x3.0 Комбо Boost + Страховка</color>";
        }
    }

    private void EnsureUIExists()
    {
        if (qteBannerRoot != null && modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 1. QTE Alarm Banner (располагается по центру экрана)
        if (qteBannerRoot == null)
        {
            GameObject qteObj = new GameObject("Chaos_QTE_Banner", typeof(RectTransform), typeof(Image));
            qteObj.transform.SetParent(canvas.transform, false);
            qteBannerRoot = qteObj;

            RectTransform qr = qteObj.GetComponent<RectTransform>();
            qr.anchorMin = new Vector2(0.5f, 0.5f);
            qr.anchorMax = new Vector2(0.5f, 0.5f);
            qr.pivot = new Vector2(0.5f, 0.5f);
            qr.sizeDelta = new Vector2(580, 220);
            qteObj.GetComponent<Image>().color = new Color(0.85f, 0.18f, 0.18f, 0.96f);

            // Title
            GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(qteObj.transform, false);
            incidentTitleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            incidentTitleTxt.fontSize = 17;
            incidentTitleTxt.fontStyle = FontStyles.Bold;
            incidentTitleTxt.alignment = TextAlignmentOptions.Center;
            incidentTitleTxt.color = Color.white;
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-20, 32);
            tr.anchoredPosition = new Vector2(0, -10);

            // Desc
            GameObject descObj = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(qteObj.transform, false);
            incidentDescTxt = descObj.GetComponent<TextMeshProUGUI>();
            incidentDescTxt.fontSize = 12;
            incidentDescTxt.alignment = TextAlignmentOptions.Center;
            incidentDescTxt.color = new Color(1f, 0.95f, 0.95f);
            RectTransform dr = descObj.GetComponent<RectTransform>();
            dr.anchorMin = new Vector2(0f, 1f);
            dr.anchorMax = new Vector2(1f, 1f);
            dr.pivot = new Vector2(0.5f, 1f);
            dr.sizeDelta = new Vector2(-20, 36);
            dr.anchoredPosition = new Vector2(0, -42);

            // Taps Remaining Text
            GameObject tapsObj = new GameObject("TapsRemainingTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            tapsObj.transform.SetParent(qteObj.transform, false);
            tapsRemainingTxt = tapsObj.GetComponent<TextMeshProUGUI>();
            tapsRemainingTxt.fontSize = 14;
            tapsRemainingTxt.fontStyle = FontStyles.Bold;
            tapsRemainingTxt.alignment = TextAlignmentOptions.Center;
            tapsRemainingTxt.color = new Color(1f, 0.92f, 0.2f);
            RectTransform tpr = tapsObj.GetComponent<RectTransform>();
            tpr.anchorMin = new Vector2(0f, 1f);
            tpr.anchorMax = new Vector2(1f, 1f);
            tpr.pivot = new Vector2(0.5f, 1f);
            tpr.sizeDelta = new Vector2(-20, 26);
            tpr.anchoredPosition = new Vector2(0, -80);

            // Action Tap Button
            GameObject actBtnObj = new GameObject("ActionTapBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            actBtnObj.transform.SetParent(qteObj.transform, false);
            RectTransform abr = actBtnObj.GetComponent<RectTransform>();
            abr.anchorMin = new Vector2(0.08f, 0.12f);
            abr.anchorMax = new Vector2(0.92f, 0.48f);
            abr.offsetMin = Vector2.zero;
            abr.offsetMax = Vector2.zero;
            actBtnObj.GetComponent<Image>().color = new Color(1f, 0.95f, 0.2f, 1f);
            actionTapBtn = actBtnObj.GetComponent<Button>();

            GameObject atTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
            atTxtObj.transform.SetParent(actBtnObj.transform, false);
            actionTapBtnTxt = atTxtObj.GetComponent<TextMeshProUGUI>();
            actionTapBtnTxt.text = "🚨 ТАПАЙТЕ СЮДА БЫСТРЕЕ!";
            actionTapBtnTxt.fontSize = 16;
            actionTapBtnTxt.fontStyle = FontStyles.Bold;
            actionTapBtnTxt.alignment = TextAlignmentOptions.Center;
            actionTapBtnTxt.color = new Color(0.12f, 0.12f, 0.15f);
            RectTransform atr = atTxtObj.GetComponent<RectTransform>();
            atr.anchorMin = Vector2.zero;
            atr.anchorMax = Vector2.one;
            atr.offsetMin = Vector2.zero;
            atr.offsetMax = Vector2.zero;

            qteObj.SetActive(false);
        }

        // 2. Modal Window Штаб ЧП
        if (modalRoot == null)
        {
            GameObject mRoot = new GameObject("ChaosIncident_ModalRoot", typeof(RectTransform));
            mRoot.transform.SetParent(canvas.transform, false);
            modalRoot = mRoot;

            RectTransform mr = mRoot.GetComponent<RectTransform>();
            mr.anchorMin = Vector2.zero;
            mr.anchorMax = Vector2.one;
            mr.offsetMin = Vector2.zero;
            mr.offsetMax = Vector2.zero;

            // Backdrop
            GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(mRoot.transform, false);
            RectTransform bRect = backdrop.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero;
            bRect.anchorMax = Vector2.one;
            bRect.offsetMin = Vector2.zero;
            bRect.offsetMax = Vector2.zero;
            backdrop.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.05f, 0.90f);
            backdropBtn = backdrop.GetComponent<Button>();

            // Card
            GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(mRoot.transform, false);
            RectTransform cr = card.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.5f, 0.5f);
            cr.anchorMax = new Vector2(0.5f, 0.5f);
            cr.pivot = new Vector2(0.5f, 0.5f);
            cr.sizeDelta = new Vector2(540, 380);
            card.GetComponent<Image>().color = new Color(0.16f, 0.13f, 0.20f, 0.98f);

            // Header
            GameObject hdr = new GameObject("Header", typeof(RectTransform), typeof(Image));
            hdr.transform.SetParent(card.transform, false);
            RectTransform hr = hdr.GetComponent<RectTransform>();
            hr.anchorMin = new Vector2(0f, 1f);
            hr.anchorMax = new Vector2(1f, 1f);
            hr.pivot = new Vector2(0.5f, 1f);
            hr.sizeDelta = new Vector2(0, 60);
            hr.anchoredPosition = Vector2.zero;
            hdr.GetComponent<Image>().color = new Color(0.28f, 0.16f, 0.22f, 1f);

            GameObject tObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            tObj.transform.SetParent(hdr.transform, false);
            TextMeshProUGUI mt = tObj.GetComponent<TextMeshProUGUI>();
            mt.text = "🚨 CHAOS ENGINEERING & ИНЦИДЕНТЫ";
            mt.fontSize = 17;
            mt.fontStyle = FontStyles.Bold;
            mt.alignment = TextAlignmentOptions.MidlineLeft;
            mt.color = new Color(1f, 0.5f, 0.4f);
            RectTransform mtr = tObj.GetComponent<RectTransform>();
            mtr.anchorMin = new Vector2(0f, 0f);
            mtr.anchorMax = new Vector2(1f, 1f);
            mtr.offsetMin = new Vector2(20, 0);
            mtr.offsetMax = new Vector2(-60, 0);

            // Close X
            GameObject xBtnObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            xBtnObj.transform.SetParent(hdr.transform, false);
            RectTransform xRect = xBtnObj.GetComponent<RectTransform>();
            xRect.anchorMin = new Vector2(1f, 0.5f);
            xRect.anchorMax = new Vector2(1f, 0.5f);
            xRect.pivot = new Vector2(1f, 0.5f);
            xRect.sizeDelta = new Vector2(36, 36);
            xRect.anchoredPosition = new Vector2(-15, 0);
            xBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.85f);
            closeXBtn = xBtnObj.GetComponent<Button>();

            GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            xTxtObj.transform.SetParent(xBtnObj.transform, false);
            TextMeshProUGUI xTxt = xTxtObj.GetComponent<TextMeshProUGUI>();
            xTxt.text = "✕";
            xTxt.fontSize = 18;
            xTxt.fontStyle = FontStyles.Bold;
            xTxt.alignment = TextAlignmentOptions.Center;
            xTxt.color = Color.white;
            RectTransform xtr = xTxtObj.GetComponent<RectTransform>();
            xtr.anchorMin = Vector2.zero;
            xtr.anchorMax = Vector2.one;
            xtr.offsetMin = Vector2.zero;
            xtr.offsetMax = Vector2.zero;

            // Summary Stats Text
            GameObject sumObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
            sumObj.transform.SetParent(card.transform, false);
            statsSummaryTxt = sumObj.GetComponent<TextMeshProUGUI>();
            statsSummaryTxt.fontSize = 13;
            statsSummaryTxt.alignment = TextAlignmentOptions.Center;
            statsSummaryTxt.color = new Color(0.92f, 0.90f, 0.95f);
            RectTransform sur = sumObj.GetComponent<RectTransform>();
            sur.anchorMin = new Vector2(0f, 0.45f);
            sur.anchorMax = new Vector2(1f, 0.85f);
            sur.offsetMin = new Vector2(20, 0);
            sur.offsetMax = new Vector2(-20, 0);

            // Trigger Drill Button
            GameObject drillBtnObj = new GameObject("TriggerDrillBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            drillBtnObj.transform.SetParent(card.transform, false);
            RectTransform dbr = drillBtnObj.GetComponent<RectTransform>();
            dbr.anchorMin = new Vector2(0.15f, 0.22f);
            dbr.anchorMax = new Vector2(0.85f, 0.38f);
            dbr.offsetMin = Vector2.zero;
            dbr.offsetMax = Vector2.zero;
            drillBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.35f, 0.18f, 1f);
            triggerDrillBtn = drillBtnObj.GetComponent<Button>();

            GameObject dtTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
            dtTxtObj.transform.SetParent(drillBtnObj.transform, false);
            triggerDrillBtnTxt = dtTxtObj.GetComponent<TextMeshProUGUI>();
            triggerDrillBtnTxt.text = "🚨 ЗАПУСТИТЬ УЧЕБНУЮ ТРЕВОГУ";
            triggerDrillBtnTxt.fontSize = 14;
            triggerDrillBtnTxt.fontStyle = FontStyles.Bold;
            triggerDrillBtnTxt.alignment = TextAlignmentOptions.Center;
            triggerDrillBtnTxt.color = Color.white;
            RectTransform dtr = dtTxtObj.GetComponent<RectTransform>();
            dtr.anchorMin = Vector2.zero;
            dtr.anchorMax = Vector2.one;
            dtr.offsetMin = Vector2.zero;
            dtr.offsetMax = Vector2.zero;

            // Close Bottom Button
            GameObject cBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            cBtnObj.transform.SetParent(card.transform, false);
            RectTransform cbr = cBtnObj.GetComponent<RectTransform>();
            cbr.anchorMin = new Vector2(0.35f, 0.05f);
            cbr.anchorMax = new Vector2(0.65f, 0.17f);
            cbr.offsetMin = Vector2.zero;
            cbr.offsetMax = Vector2.zero;
            cBtnObj.GetComponent<Image>().color = new Color(0.24f, 0.24f, 0.32f, 1f);
            closeBtn = cBtnObj.GetComponent<Button>();

            GameObject cTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
            cTxtObj.transform.SetParent(cBtnObj.transform, false);
            TextMeshProUGUI ct = cTxtObj.GetComponent<TextMeshProUGUI>();
            ct.text = "ЗАКРЫТЬ";
            ct.fontSize = 13;
            ct.fontStyle = FontStyles.Bold;
            ct.alignment = TextAlignmentOptions.Center;
            ct.color = Color.white;
            RectTransform ctr = cTxtObj.GetComponent<RectTransform>();
            ctr.anchorMin = Vector2.zero;
            ctr.anchorMax = Vector2.one;
            ctr.offsetMin = Vector2.zero;
            ctr.offsetMax = Vector2.zero;

            mRoot.SetActive(false);
        }
    }
}
