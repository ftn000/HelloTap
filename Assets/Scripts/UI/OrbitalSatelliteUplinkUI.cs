using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Запуск Спутникового Интернета Студии («Orbital Satellite Uplink Array»):
/// - Развертывание собственной спутниковой сети на низкой околоземной орбите
/// - Нулевой пинг и мгновенная доставка патчей игр игрокам по всей планете
/// - 4 уровня орбитальных спутников:
///   1. 🛰️ CubeSat Relay Alpha (+150 ₽/сек, +8% к доходу)
///   2. 📡 LEO Broadband Swarm (+600 ₽/сек, +12% к клику)
///   3. ⚡ Quantum Optical Transceiver (+2200 ₽/сек, +20% к доходу)
///   4. 🌐 Geostationary DeepSpace Megaconstellation (+7500 ₽/сек, x1.35 ко всему доходу)
/// - Интерактивное действие: «🛰️ КАЛИБРОВКА ОРБИТАЛЬНОГО ЛИНКА» (приток данных, комбо +35% и орбитальный грант)
/// - Пассивный доход в рублях от трансляции данных и стриминга игр
/// </summary>
public class OrbitalSatelliteUplinkUI : MonoBehaviour
{
    private static OrbitalSatelliteUplinkUI instance;
    public static OrbitalSatelliteUplinkUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<OrbitalSatelliteUplinkUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(OrbitalSatelliteUplinkUI));
                    instance = go.AddComponent<OrbitalSatelliteUplinkUI>();
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
    public class SatelliteTier
    {
        public string id;
        public string title;
        public string orbitInfo;
        public string icon;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;
        public double moneyPerSecBonus;
        public float incomeMultiplierBonus;
        public float clickMultiplierBonus;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.48, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.42, level));
        public double GetTotalMps() => moneyPerSecBonus * level;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openUplinkBtn;

    [Header("Спутниковая панель")]
    [SerializeField] private TMP_Text statsSummaryTxt;
    [SerializeField] private Button calibrateBtn;
    [SerializeField] private TMP_Text calibrateBtnTxt;
    [SerializeField] private Transform satellitesContainer;

    [Header("Данные спутников")]
    [SerializeField] private List<SatelliteTier> satellites = new List<SatelliteTier>();

    private const string PrefCalibrationsCount = "Orbital_Calibrations_Count";
    private const string PrefSatLvlPrefix = "Orbital_Sat_Lvl_";

    private int calibrationsCompleted = 0;
    private bool isCalibrating = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitSatellitesList();
        LoadData();
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
            RefreshUI();
            if (modalRoot != null) modalRoot.SetActive(false);
        }
    }

    private void InitSatellitesList()
    {
        if (satellites != null && satellites.Count > 0) return;

        satellites = new List<SatelliteTier>
        {
            new SatelliteTier
            {
                id = "sat_cubesat",
                title = "CubeSat Relay Alpha",
                orbitInfo = "LEO 450 км • Малый ретранслятор пакетов",
                icon = "🛰️",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 4000.0,
                baseCostCode = 2500.0,
                moneyPerSecBonus = 150.0,
                incomeMultiplierBonus = 0.08f,
                clickMultiplierBonus = 0.05f
            },
            new SatelliteTier
            {
                id = "sat_leo_swarm",
                title = "LEO Broadband Swarm",
                orbitInfo = "LEO 550 км • Фазированная решетка Ka-диапазона",
                icon = "📡",
                level = 0,
                maxLevel = 25,
                baseCostMoney = 22000.0,
                baseCostCode = 14000.0,
                moneyPerSecBonus = 600.0,
                incomeMultiplierBonus = 0.12f,
                clickMultiplierBonus = 0.12f
            },
            new SatelliteTier
            {
                id = "sat_quantum_optical",
                title = "Quantum Optical Transceiver",
                orbitInfo = "MEO 2000 км • Лазерная передача терабит/сек",
                icon = "⚡",
                level = 0,
                maxLevel = 20,
                baseCostMoney = 95000.0,
                baseCostCode = 60000.0,
                moneyPerSecBonus = 2200.0,
                incomeMultiplierBonus = 0.20f,
                clickMultiplierBonus = 0.15f
            },
            new SatelliteTier
            {
                id = "sat_geostationary",
                title = "DeepSpace Megaconstellation",
                orbitInfo = "GEO 35786 км • Глобальное планетарное покрытие",
                icon = "🌐",
                level = 0,
                maxLevel = 15,
                baseCostMoney = 450000.0,
                baseCostCode = 300000.0,
                moneyPerSecBonus = 7500.0,
                incomeMultiplierBonus = 0.35f,
                clickMultiplierBonus = 0.25f
            }
        };
    }

    private void LoadData()
    {
        calibrationsCompleted = PlayerPrefs.GetInt(PrefCalibrationsCount, 0);
        foreach (var sat in satellites)
        {
            if (PlayerPrefs.HasKey(PrefSatLvlPrefix + sat.id))
            {
                sat.level = PlayerPrefs.GetInt(PrefSatLvlPrefix + sat.id);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefCalibrationsCount, calibrationsCompleted);
        foreach (var sat in satellites)
        {
            PlayerPrefs.SetInt(PrefSatLvlPrefix + sat.id, sat.level);
        }
        PlayerPrefs.Save();
    }

    public double GetSatelliteIncomeMultiplier()
    {
        double mult = 1.0;
        foreach (var sat in satellites)
        {
            mult += sat.level * sat.incomeMultiplierBonus;
        }
        mult += Math.Min(calibrationsCompleted * 0.015, 0.60);
        return mult;
    }

    public double GetSatelliteClickMultiplier()
    {
        double mult = 1.0;
        foreach (var sat in satellites)
        {
            mult += sat.level * sat.clickMultiplierBonus;
        }
        return mult;
    }

    public double GetSatelliteMoneyPerSec()
    {
        double sum = 0;
        foreach (var sat in satellites)
        {
            sum += sat.GetTotalMps();
        }
        return sum;
    }

    public void CalibrateUplink()
    {
        if (isCalibrating) return;
        StartCoroutine(CalibrateRoutine());
    }

    private IEnumerator CalibrateRoutine()
    {
        isCalibrating = true;
        if (calibrateBtn != null) calibrateBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (calibrateBtnTxt != null)
        {
            calibrateBtnTxt.text = "🛰️ СИНХРОНИЗАЦИЯ СПУТНИКОВОЙ ОРБИТЫ...";
        }

        yield return new WaitForSecondsRealtime(1.2f);

        calibrationsCompleted++;
        double baseBonusMoney = Math.Max(35000.0, GetSatelliteMoneyPerSec() * 60.0);
        double rewardMoney = Math.Floor(baseBonusMoney * GetSatelliteIncomeMultiplier());
        double rewardCode = Math.Floor(rewardMoney * 0.40 * GetSatelliteClickMultiplier());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.35f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛰️ ОРБИТАЛЬНЫЙ ЛИНК СИНХРОНИЗИРОВАН!\nДоход сети: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} C#)", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        isCalibrating = false;
        if (calibrateBtn != null) calibrateBtn.interactable = true;
        if (calibrateBtnTxt != null) calibrateBtnTxt.text = "🛰️ КАЛИБРОВКА ОРБИТАЛЬНОГО ЛИНКА";
    }

    public void UpgradeSatellite(string satId)
    {
        var sat = satellites.Find(s => s.id == satId);
        if (sat == null) return;

        if (sat.level >= sat.maxLevel)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Спутниковый эшелон уже развернут полностью!", transform.position, Color.yellow, false);
            }
            return;
        }

        double costMoney = sat.GetCostMoney();
        double costCode = sat.GetCostCode();

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

        sat.level++;
        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛰️ {sat.title} УРОВЕНЬ {sat.level}!\n+{sat.moneyPerSecBonus:F0} ₽/сек • +{(sat.incomeMultiplierBonus * 100):F0}% Доход", transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
    }

    public void OpenModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            RefreshUI();
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

    public void CloseModal()
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

    public void RefreshUI()
    {
        if (statsSummaryTxt != null)
        {
            double totalMps = GetSatelliteMoneyPerSec();
            double incMult = GetSatelliteIncomeMultiplier();
            double clkMult = GetSatelliteClickMultiplier();
            statsSummaryTxt.text = $"🛰️ Спутниковый поток: <b>+{NumberFormatter.Format(totalMps)} ₽/сек</b>\nМножитель дохода: <b>x{incMult:F2}</b> • Сила клика: <b>x{clkMult:F2}</b> (Калибровок: {calibrationsCompleted})";
        }

        if (satellitesContainer != null)
        {
            for (int i = 0; i < satellitesContainer.childCount; i++)
            {
                Transform child = satellitesContainer.GetChild(i);
                if (i < satellites.Count)
                {
                    UpdateSatelliteCard(child, satellites[i]);
                }
            }
        }
    }

    private void UpdateSatelliteCard(Transform card, SatelliteTier sat)
    {
        TMP_Text titleTxt = card.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
        TMP_Text orbitTxt = card.Find("Header/OrbitTxt")?.GetComponent<TMP_Text>();
        TMP_Text iconTxt = card.Find("IconBox/IconTxt")?.GetComponent<TMP_Text>();
        TMP_Text descTxt = card.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
        TMP_Text statsTxt = card.Find("Body/StatsTxt")?.GetComponent<TMP_Text>();
        Button upBtn = card.Find("Action/UpgradeBtn")?.GetComponent<Button>();
        TMP_Text upBtnTxt = card.Find("Action/UpgradeBtn/Txt")?.GetComponent<TMP_Text>();

        if (titleTxt != null) titleTxt.text = $"{sat.title} (Ур. {sat.level}/{sat.maxLevel})";
        if (orbitTxt != null) orbitTxt.text = sat.orbitInfo;
        if (iconTxt != null) iconTxt.text = sat.icon;
        if (descTxt != null) descTxt.text = $"+{sat.moneyPerSecBonus:F0} ₽/сек • +{(sat.incomeMultiplierBonus * 100):F0}% Доход • +{(sat.clickMultiplierBonus * 100):F0}% Клик";
        if (statsTxt != null) statsTxt.text = $"Текущий выпуск: +{NumberFormatter.Format(sat.GetTotalMps())} ₽/сек";

        if (upBtn != null && upBtnTxt != null)
        {
            if (sat.level >= sat.maxLevel)
            {
                upBtn.interactable = false;
                upBtnTxt.text = "МАКС.";
            }
            else
            {
                double costMoney = sat.GetCostMoney();
                double costCode = sat.GetCostCode();
                upBtn.interactable = true;
                upBtnTxt.text = $"Запуск\n{NumberFormatter.Format(costMoney)} ₽\n{NumberFormatter.Format(costCode)} C#";
            }
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseModal);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseModal);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseModal);
        if (openUplinkBtn != null) openUplinkBtn.onClick.AddListener(OpenModal);
        if (calibrateBtn != null) calibrateBtn.onClick.AddListener(CalibrateUplink);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("OrbitalSatelliteModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.02f, 0.05f, 0.12f, 0.85f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(modalRoot.transform, false);
        modalCardTransform = card.transform;
        RectTransform rtCard = card.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(480, 680);
        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.08f, 0.12f, 0.22f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "🛰️ ORBITAL SATELLITE UPLINK";
        hTxt.fontSize = 20;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.3f, 0.85f, 1f);
        RectTransform hRect = headerObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(-40, 36);
        hRect.anchoredPosition = new Vector2(0, -14);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI subTxt = subObj.GetComponent<TextMeshProUGUI>();
        subTxt.text = "Орбитальная спутниковая сеть передачи игр и стриминга данных";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.7f, 0.85f, 1f);
        RectTransform sRect = subObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 1f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.sizeDelta = new Vector2(-40, 22);
        sRect.anchoredPosition = new Vector2(0, -48);

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

        // Stats Summary Box
        GameObject summaryBox = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        summaryBox.transform.SetParent(card.transform, false);
        RectTransform sbRect = summaryBox.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0f, 1f);
        sbRect.anchorMax = new Vector2(1f, 1f);
        sbRect.pivot = new Vector2(0.5f, 1f);
        sbRect.sizeDelta = new Vector2(-30, 48);
        sbRect.anchoredPosition = new Vector2(0, -74);
        summaryBox.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.32f, 0.9f);

        GameObject sumTxtObj = new GameObject("StatsTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(summaryBox.transform, false);
        statsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        statsSummaryTxt.fontSize = 12;
        statsSummaryTxt.alignment = TextAlignmentOptions.Center;
        statsSummaryTxt.color = new Color(0.3f, 0.9f, 1f);
        RectTransform sumr = sumTxtObj.GetComponent<RectTransform>();
        sumr.anchorMin = Vector2.zero;
        sumr.anchorMax = Vector2.one;
        sumr.offsetMin = new Vector2(6, 4);
        sumr.offsetMax = new Vector2(-6, -4);

        // Action Panel (Калибровка спутникового линка)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 52);
        apRect.anchoredPosition = new Vector2(0, -126);
        actPanel.GetComponent<Image>().color = new Color(0.10f, 0.20f, 0.36f, 1f);

        GameObject calBtnObj = new GameObject("CalibrateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        calBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform cbRect = calBtnObj.GetComponent<RectTransform>();
        cbRect.anchorMin = Vector2.zero;
        cbRect.anchorMax = Vector2.one;
        cbRect.offsetMin = new Vector2(8, 6);
        cbRect.offsetMax = new Vector2(-8, -6);
        calBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.95f, 1f);
        calibrateBtn = calBtnObj.GetComponent<Button>();

        GameObject calTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        calTxtObj.transform.SetParent(calBtnObj.transform, false);
        calibrateBtnTxt = calTxtObj.GetComponent<TextMeshProUGUI>();
        calibrateBtnTxt.text = "🛰️ КАЛИБРОВКА ОРБИТАЛЬНОГО ЛИНКА";
        calibrateBtnTxt.fontSize = 14;
        calibrateBtnTxt.fontStyle = FontStyles.Bold;
        calibrateBtnTxt.alignment = TextAlignmentOptions.Center;
        calibrateBtnTxt.color = Color.white;
        RectTransform ctr = calTxtObj.GetComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero;
        ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero;
        ctr.offsetMax = Vector2.zero;

        // Scroll View
        GameObject scrollObj = new GameObject("SatellitesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform scRect = scrollObj.GetComponent<RectTransform>();
        scRect.anchorMin = new Vector2(0f, 0f);
        scRect.anchorMax = new Vector2(1f, 1f);
        scRect.offsetMin = new Vector2(15, 60);
        scRect.offsetMax = new Vector2(-15, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.16f, 0.5f);

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
        satellitesContainer = content.transform;
        RectTransform cRect = content.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0f, 1f);
        cRect.anchorMax = new Vector2(1f, 1f);
        cRect.pivot = new Vector2(0.5f, 1f);
        cRect.offsetMin = Vector2.zero;
        cRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(6, 6, 6, 6);
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

        // Instantiate cards
        foreach (var sat in satellites)
        {
            CreateSatelliteCardUI(content.transform, sat);
        }

        // Bottom Close Button
        GameObject botCloseObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 42);
        bcRect.anchoredPosition = new Vector2(0, 12);
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.28f, 0.45f, 1f);
        closeBtn = botCloseObj.GetComponent<Button>();

        GameObject bcTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bcTxtObj.transform.SetParent(botCloseObj.transform, false);
        TextMeshProUGUI bct = bcTxtObj.GetComponent<TextMeshProUGUI>();
        bct.text = "ЗАКРЫТЬ";
        bct.fontSize = 14;
        bct.fontStyle = FontStyles.Bold;
        bct.alignment = TextAlignmentOptions.Center;
        bct.color = Color.white;
        RectTransform bctr = bcTxtObj.GetComponent<RectTransform>();
        bctr.anchorMin = Vector2.zero;
        bctr.anchorMax = Vector2.one;
        bctr.offsetMin = Vector2.zero;
        bctr.offsetMax = Vector2.zero;

        // Launcher Floating Button
        CreateLauncherButton(canvas.transform);

        WireButtons();
        RefreshUI();
        modalRoot.SetActive(false);
    }

    private void CreateLauncherButton(Transform canvasTransform)
    {
        GameObject launchBtnObj = new GameObject("OrbitalUplinkLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvasTransform, false);
        RectTransform lbRect = launchBtnObj.GetComponent<RectTransform>();
        lbRect.anchorMin = new Vector2(1f, 0.5f);
        lbRect.anchorMax = new Vector2(1f, 0.5f);
        lbRect.pivot = new Vector2(1f, 0.5f);
        lbRect.sizeDelta = new Vector2(44, 44);
        lbRect.anchoredPosition = new Vector2(-12, 196);
        launchBtnObj.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.85f, 0.95f);
        openUplinkBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "🛰️";
        lt.fontSize = 20;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;
    }

    private void CreateSatelliteCardUI(Transform parent, SatelliteTier sat)
    {
        GameObject card = new GameObject("SatCard_" + sat.id, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.9f);

        LayoutElement le = card.GetComponent<LayoutElement>();
        le.preferredHeight = 94;
        le.minHeight = 94;

        // Icon Box
        GameObject iconBox = new GameObject("IconBox", typeof(RectTransform), typeof(Image));
        iconBox.transform.SetParent(card.transform, false);
        RectTransform ibRect = iconBox.GetComponent<RectTransform>();
        ibRect.anchorMin = new Vector2(0f, 0.5f);
        ibRect.anchorMax = new Vector2(0f, 0.5f);
        ibRect.pivot = new Vector2(0f, 0.5f);
        ibRect.sizeDelta = new Vector2(50, 50);
        ibRect.anchoredPosition = new Vector2(8, 0);
        iconBox.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.45f, 1f);

        GameObject iconTxt = new GameObject("IconTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconTxt.transform.SetParent(iconBox.transform, false);
        TextMeshProUGUI it = iconTxt.GetComponent<TextMeshProUGUI>();
        it.text = sat.icon;
        it.fontSize = 26;
        it.alignment = TextAlignmentOptions.Center;
        RectTransform itr = iconTxt.GetComponent<RectTransform>();
        itr.anchorMin = Vector2.zero;
        itr.anchorMax = Vector2.one;
        itr.offsetMin = Vector2.zero;
        itr.offsetMax = Vector2.zero;

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform));
        header.transform.SetParent(card.transform, false);
        RectTransform hr = header.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(0.68f, 1f);
        hr.pivot = new Vector2(0f, 1f);
        hr.anchoredPosition = new Vector2(66, -6);
        hr.sizeDelta = new Vector2(0, 36);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 13;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.color = Color.white;
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0f, 0.5f);
        tr.anchorMax = new Vector2(1f, 1f);
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        GameObject orbit = new GameObject("OrbitTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        orbit.transform.SetParent(header.transform, false);
        TextMeshProUGUI oTxt = orbit.GetComponent<TextMeshProUGUI>();
        oTxt.fontSize = 10;
        oTxt.color = new Color(0.35f, 0.85f, 1f);
        RectTransform or = orbit.GetComponent<RectTransform>();
        or.anchorMin = new Vector2(0f, 0f);
        or.anchorMax = new Vector2(1f, 0.5f);
        or.offsetMin = Vector2.zero;
        or.offsetMax = Vector2.zero;

        // Body
        GameObject body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(card.transform, false);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0f, 0f);
        br.anchorMax = new Vector2(0.68f, 1f);
        br.offsetMin = new Vector2(66, 6);
        br.offsetMax = new Vector2(0, -42);

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
        ar.anchorMin = new Vector2(0.68f, 0f);
        ar.anchorMax = new Vector2(1f, 1f);
        ar.offsetMin = new Vector2(0, 8);
        ar.offsetMax = new Vector2(-8, -8);

        GameObject upBtn = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        upBtn.transform.SetParent(act.transform, false);
        RectTransform ubr = upBtn.GetComponent<RectTransform>();
        ubr.anchorMin = Vector2.zero;
        ubr.anchorMax = Vector2.one;
        ubr.offsetMin = Vector2.zero;
        ubr.offsetMax = Vector2.zero;
        upBtn.GetComponent<Image>().color = new Color(0.18f, 0.50f, 0.85f, 1f);

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

        string currentSatId = sat.id;
        upBtn.GetComponent<Button>().onClick.AddListener(() => UpgradeSatellite(currentSatId));
    }
}
