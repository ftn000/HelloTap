using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Зал Славы и Спидран-Лидерборд (Global Speedrun Hall of Fame):
/// - Автоматический секундомер забега (Speedrun Timer / Splits)
/// - 4 спидран-категории: First Game Any%, 100k Lines%, Unicorn Valuation%, IPO Prestige%
/// - Личные рекорды (PB) и мировые рекорды виртуальных легенд спидрана
/// - Медали (Gold, Silver, Bronze) и постоянный бафф к комбо и скорости клика
/// </summary>
public class GlobalSpeedrunRecordsUI : MonoBehaviour
{
    private static GlobalSpeedrunRecordsUI instance;
    public static GlobalSpeedrunRecordsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<GlobalSpeedrunRecordsUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class SpeedrunCategory
    {
        public string id;
        public string title;
        public string icon;
        public string goalDescription;
        public float worldRecordSeconds;
        public string wrHolderName;
        public float personalBestSeconds; // 0 if not completed yet
        public bool isCompletedThisRun;
        public string medalTier; // "Gold", "Silver", "Bronze", "None"
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openSpeedrunBtn;
    [SerializeField] private TMP_Text openSpeedrunBtnText;
    [SerializeField] private Button closeSpeedrunBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка спидрана")]
    [SerializeField] private TMP_Text liveSessionTimerText;
    [SerializeField] private TMP_Text speedrunBuffText;
    [SerializeField] private TMP_Text medalsCountText;

    [Header("Контейнер категорий")]
    [SerializeField] private Transform categoriesContainer;

    private readonly List<SpeedrunCategory> categories = new List<SpeedrunCategory>();
    private float currentRunTime = 0f;
    private bool isTimerRunning = true;

    private const string PrefPbPrefix = "Speedrun_PB_";
    private const string PrefRunTimeKey = "Speedrun_CurrentRunTime";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public float CurrentRunTime => currentRunTime;
    public IReadOnlyList<SpeedrunCategory> Categories => categories;

    public double GetSpeedrunMultiplier()
    {
        int medals = 0;
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].personalBestSeconds > 0)
            {
                medals++;
            }
        }
        return 1.0 + (medals * 0.08); // +8% к клику и скорости за каждую взятую категорию
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCategories();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentRunTime += Time.deltaTime;
            CheckMilestones();
        }

        if (IsModalOpen)
        {
            UpdateModalUI();
        }
    }

    private void CheckMilestones()
    {
        if (GameManager.Instance == null) return;

        // 1. First Game Any%
        var cat0 = categories[0];
        if (!cat0.isCompletedThisRun)
        {
            bool hasCompletedAnyProject = false;
            if (GameManager.Instance.Projects != null)
            {
                for (int i = 0; i < GameManager.Instance.Projects.Count; i++)
                {
                    if (GameManager.Instance.Projects[i].IsCompleted)
                    {
                        hasCompletedAnyProject = true;
                        break;
                    }
                }
            }
            if (hasCompletedAnyProject)
            {
                CompleteCategory(0);
            }
        }

        // 2. 100k Lines%
        var cat1 = categories[1];
        if (!cat1.isCompletedThisRun && GameManager.Instance.TotalCodeWritten >= 100000.0)
        {
            CompleteCategory(1);
        }

        // 3. Unicorn Valuation% (10,000,000 ₽)
        var cat2 = categories[2];
        if (!cat2.isCompletedThisRun && GameManager.Instance.TotalMoneyEarned >= 10000000.0)
        {
            CompleteCategory(2);
        }

        // 4. IPO Prestige%
        var cat3 = categories[3];
        if (!cat3.isCompletedThisRun && GameManager.Instance.PrestigeLevel >= 1)
        {
            CompleteCategory(3);
        }
    }

    private void CompleteCategory(int index)
    {
        if (index < 0 || index >= categories.Count) return;
        var cat = categories[index];
        cat.isCompletedThisRun = true;

        bool isNewPb = false;
        if (cat.personalBestSeconds <= 0f || currentRunTime < cat.personalBestSeconds)
        {
            cat.personalBestSeconds = currentRunTime;
            isNewPb = true;

            // Вычисляем медаль
            if (cat.personalBestSeconds <= cat.worldRecordSeconds * 1.15f)
                cat.medalTier = "🥇 Золото";
            else if (cat.personalBestSeconds <= cat.worldRecordSeconds * 1.5f)
                cat.medalTier = "🥈 Серебро";
            else
                cat.medalTier = "🥉 Бронза";

            SaveData();
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            string pbBadge = isNewPb ? "🔥 НОВЫЙ ЛИЧНЫЙ РЕКОРД (PB)!" : "✓ СПЛИТ ЗАВЕРШЕН!";
            ClickJuice.Instance.SpawnCustomPopup($"⚡ {cat.title}\n{pbBadge}\nВремя: {FormatTime(currentRunTime)} ({cat.medalTier})", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateModalUI();
    }

    public void ResetRunTimer()
    {
        currentRunTime = 0f;
        for (int i = 0; i < categories.Count; i++)
        {
            categories[i].isCompletedThisRun = false;
        }
        PlayerPrefs.SetFloat(PrefRunTimeKey, 0f);
        PlayerPrefs.Save();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("⏱️ ТАЙМЕР ЗАБЕГА СБРОШЕН!\nНовый сплит-таймер стартовал!", transform.position, new Color(0.4f, 0.85f, 1f), false);
        }

        UpdateModalUI();
    }

    private void InitializeCategories()
    {
        categories.Clear();

        categories.Add(new SpeedrunCategory
        {
            id = "cat_first_game",
            title = "First Game Any%",
            icon = "🎮",
            goalDescription = "Выпустить первую коммерческую игру студии",
            worldRecordSeconds = 145f,
            wrHolderName = "FlashDev_Turbo",
            personalBestSeconds = 0f,
            isCompletedThisRun = false,
            medalTier = "None"
        });

        categories.Add(new SpeedrunCategory
        {
            id = "cat_100k_code",
            title = "100k Lines Code%",
            icon = "⚡",
            goalDescription = "Написать 100 000 суммарных строк кода",
            worldRecordSeconds = 380f,
            wrHolderName = "StackNinja_Pro",
            personalBestSeconds = 0f,
            isCompletedThisRun = false,
            medalTier = "None"
        });

        categories.Add(new SpeedrunCategory
        {
            id = "cat_unicorn_val",
            title = "Unicorn Valuation%",
            icon = "🦄",
            goalDescription = "Заработать первые 10 000 000 ₽ выручки",
            worldRecordSeconds = 850f,
            wrHolderName = "VentureSpeedrunner",
            personalBestSeconds = 0f,
            isCompletedThisRun = false,
            medalTier = "None"
        });

        categories.Add(new SpeedrunCategory
        {
            id = "cat_ipo_prestige",
            title = "IPO Speedrun%",
            icon = "🚀",
            goalDescription = "Провести первый выход компании на IPO (Престиж)",
            worldRecordSeconds = 1420f,
            wrHolderName = "GlitchlessCEO",
            personalBestSeconds = 0f,
            isCompletedThisRun = false,
            medalTier = "None"
        });
    }

    private void LoadData()
    {
        currentRunTime = PlayerPrefs.GetFloat(PrefRunTimeKey, 0f);

        for (int i = 0; i < categories.Count; i++)
        {
            categories[i].personalBestSeconds = PlayerPrefs.GetFloat(PrefPbPrefix + categories[i].id, 0f);
            if (categories[i].personalBestSeconds > 0)
            {
                if (categories[i].personalBestSeconds <= categories[i].worldRecordSeconds * 1.15f)
                    categories[i].medalTier = "🥇 Золото";
                else if (categories[i].personalBestSeconds <= categories[i].worldRecordSeconds * 1.5f)
                    categories[i].medalTier = "🥈 Серебро";
                else
                    categories[i].medalTier = "🥉 Бронза";
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat(PrefRunTimeKey, currentRunTime);

        for (int i = 0; i < categories.Count; i++)
        {
            PlayerPrefs.SetFloat(PrefPbPrefix + categories[i].id, categories[i].personalBestSeconds);
        }
        PlayerPrefs.Save();
    }

    private void BindButtons()
    {
        if (openSpeedrunBtn != null)
        {
            openSpeedrunBtn.onClick.RemoveAllListeners();
            openSpeedrunBtn.onClick.AddListener(OpenModal);
        }
        if (closeSpeedrunBtn != null)
        {
            closeSpeedrunBtn.onClick.RemoveAllListeners();
            closeSpeedrunBtn.onClick.AddListener(CloseModal);
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
        if (liveSessionTimerText != null)
        {
            liveSessionTimerText.text = $"⏱️ Текущий забег: <b><color=#00FF88>{FormatTime(currentRunTime)}</color></b>";
        }

        int medalsCount = 0;
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].personalBestSeconds > 0) medalsCount++;
        }

        if (medalsCountText != null)
        {
            medalsCountText.text = $"Завершено категорий: <b>{medalsCount}/{categories.Count}</b>";
        }

        if (speedrunBuffText != null)
        {
            double buff = GetSpeedrunMultiplier();
            speedrunBuffText.text = $"Бонус Зала Славы: <b><color=#FFD700>x{buff:F2}</color></b> к клику и комбо";
        }

        RefreshCategoriesList();
    }

    private void RefreshCategoriesList()
    {
        if (categoriesContainer == null) return;

        for (int i = 0; i < categories.Count; i++)
        {
            var cat = categories[i];
            Transform cardTr = categoriesContainer.Find($"SpeedrunCard_{i}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            TMP_Text pbTxt = cardTr.Find("PbText")?.GetComponent<TMP_Text>();
            TMP_Text wrTxt = cardTr.Find("WrText")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = cat.isCompletedThisRun
                    ? new Color(0.12f, 0.20f, 0.18f, 0.95f)
                    : new Color(0.08f, 0.10f, 0.14f, 0.90f);
            }

            if (pbTxt != null)
            {
                if (cat.personalBestSeconds > 0)
                {
                    pbTxt.text = $"PB: <color=#00FF88><b>{FormatTime(cat.personalBestSeconds)}</b></color> ({cat.medalTier})";
                }
                else
                {
                    pbTxt.text = "PB: <color=#8899AA>Еще не установлен</color>";
                }
            }

            if (wrTxt != null)
            {
                wrTxt.text = $"WR: <color=#FFD700><b>{FormatTime(cat.worldRecordSeconds)}</b></color> ({cat.wrHolderName})";
            }
        }
    }

    private string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        float secs = seconds % 60;
        return $"{mins:00}:{secs:00.0}";
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("GlobalSpeedrunRecordsModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.85f, 0.2f, 0.5f);
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
        headerTxt.text = "⚡ ЗАЛ СЛАВЫ И СПИДРАН-ЛИДЕРБОРД";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.85f, 0.3f);

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
        infoRt.anchoredPosition = new Vector2(0, -60);
        infoRt.sizeDelta = new Vector2(-36, 75);
        infoPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.9f);

        GameObject timerObj = new GameObject("TimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        timerObj.transform.SetParent(infoPanel.transform, false);
        RectTransform tRt = timerObj.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 0.66f);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.offsetMin = new Vector2(12, 0);
        tRt.offsetMax = new Vector2(-12, -4);
        liveSessionTimerText = timerObj.GetComponent<TextMeshProUGUI>();
        liveSessionTimerText.fontSize = 12;

        GameObject medalsObj = new GameObject("MedalsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        medalsObj.transform.SetParent(infoPanel.transform, false);
        RectTransform mRt = medalsObj.GetComponent<RectTransform>();
        mRt.anchorMin = new Vector2(0, 0.33f);
        mRt.anchorMax = new Vector2(1, 0.66f);
        mRt.offsetMin = new Vector2(12, 0);
        mRt.offsetMax = new Vector2(-12, 0);
        medalsCountText = medalsObj.GetComponent<TextMeshProUGUI>();
        medalsCountText.fontSize = 12;

        GameObject buffObj = new GameObject("BuffText", typeof(RectTransform), typeof(TextMeshProUGUI));
        buffObj.transform.SetParent(infoPanel.transform, false);
        RectTransform bRt = buffObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 0);
        bRt.anchorMax = new Vector2(1, 0.33f);
        bRt.offsetMin = new Vector2(12, 4);
        bRt.offsetMax = new Vector2(-12, 0);
        speedrunBuffText = buffObj.GetComponent<TextMeshProUGUI>();
        speedrunBuffText.fontSize = 12;

        // Scroll View Container
        GameObject scrollObj = new GameObject("CategoriesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 70);
        scrollRt.offsetMax = new Vector2(-18, -145);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        categoriesContainer = contentObj.transform;
        RectTransform contRt = contentObj.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0, 1);
        contRt.anchorMax = new Vector2(1, 1);
        contRt.pivot = new Vector2(0.5f, 1);
        contRt.sizeDelta = new Vector2(0, 400);

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contRt;
        sr.viewport = vpRt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        for (int i = 0; i < categories.Count; i++)
        {
            CreateCategoryCard(i, categories[i], categoriesContainer);
        }

        // Bottom Bar: Reset Timer & Close
        GameObject bottomBar = new GameObject("BottomBar", typeof(RectTransform));
        bottomBar.transform.SetParent(cardObj.transform, false);
        RectTransform bbarRt = bottomBar.GetComponent<RectTransform>();
        bbarRt.anchorMin = new Vector2(0, 0);
        bbarRt.anchorMax = new Vector2(1, 0);
        bbarRt.pivot = new Vector2(0.5f, 0);
        bbarRt.anchoredPosition = new Vector2(0, 14);
        bbarRt.sizeDelta = new Vector2(-36, 46);

        GameObject resetBtnObj = new GameObject("ResetBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        resetBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform rRt = resetBtnObj.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(0, 0);
        rRt.anchorMax = new Vector2(0.68f, 1);
        rRt.offsetMin = Vector2.zero;
        rRt.offsetMax = new Vector2(-6, 0);
        resetBtnObj.GetComponent<Image>().color = new Color(0.7f, 0.25f, 0.25f);
        Button resetBtn = resetBtnObj.GetComponent<Button>();
        resetBtn.onClick.AddListener(ResetRunTimer);

        GameObject resetTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        resetTxtObj.transform.SetParent(resetBtnObj.transform, false);
        RectTransform rtRt = resetTxtObj.GetComponent<RectTransform>();
        rtRt.anchorMin = Vector2.zero;
        rtRt.anchorMax = Vector2.one;
        rtRt.sizeDelta = Vector2.zero;
        TMP_Text rt = resetTxtObj.GetComponent<TextMeshProUGUI>();
        rt.text = "🔄 СБРОСИТЬ ТАЙМЕР ЗАБЕГА";
        rt.fontSize = 11;
        rt.fontStyle = FontStyles.Bold;
        rt.alignment = TextAlignmentOptions.Center;
        rt.color = Color.white;

        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform clRt = closeBtnObj.GetComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0.70f, 0);
        clRt.anchorMax = new Vector2(1, 1);
        clRt.offsetMin = Vector2.zero;
        clRt.offsetMax = Vector2.zero;
        closeBtnObj.GetComponent<Image>().color = new Color(0.28f, 0.32f, 0.38f);
        closeSpeedrunBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform cltRt = closeTxtObj.GetComponent<RectTransform>();
        cltRt.anchorMin = Vector2.zero;
        cltRt.anchorMax = Vector2.one;
        cltRt.sizeDelta = Vector2.zero;
        TMP_Text clt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        clt.text = "ЗАКРЫТЬ";
        clt.fontSize = 11;
        clt.fontStyle = FontStyles.Bold;
        clt.alignment = TextAlignmentOptions.Center;
        clt.color = Color.white;

        modalRoot = root;
    }

    private void CreateCategoryCard(int index, SpeedrunCategory cat, Transform parent)
    {
        GameObject card = new GameObject($"SpeedrunCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 92);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.92f);

        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{cat.icon} <b>{cat.title}</b>";
        tt.fontSize = 13;
        tt.color = new Color(1f, 0.88f, 0.4f);

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.55f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"{cat.goalDescription}";
        dt.fontSize = 10;
        dt.color = new Color(0.8f, 0.85f, 0.95f);

        GameObject pb = new GameObject("PbText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pb.transform.SetParent(card.transform, false);
        RectTransform pbRt = pb.GetComponent<RectTransform>();
        pbRt.anchorMin = new Vector2(0.55f, 0.5f);
        pbRt.anchorMax = new Vector2(1, 1);
        pbRt.offsetMin = new Vector2(0, 0);
        pbRt.offsetMax = new Vector2(-10, -6);
        TMP_Text pbt = pb.GetComponent<TextMeshProUGUI>();
        pbt.fontSize = 11;
        pbt.alignment = TextAlignmentOptions.Right;

        GameObject wr = new GameObject("WrText", typeof(RectTransform), typeof(TextMeshProUGUI));
        wr.transform.SetParent(card.transform, false);
        RectTransform wrRt = wr.GetComponent<RectTransform>();
        wrRt.anchorMin = new Vector2(0.55f, 0);
        wrRt.anchorMax = new Vector2(1, 0.5f);
        wrRt.offsetMin = new Vector2(0, 6);
        wrRt.offsetMax = new Vector2(-10, 0);
        TMP_Text wrt = wr.GetComponent<TextMeshProUGUI>();
        wrt.fontSize = 10;
        wrt.alignment = TextAlignmentOptions.Right;
    }
}
