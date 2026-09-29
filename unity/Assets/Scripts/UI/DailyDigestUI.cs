using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Режим Быстрого Сбора Наград («Claim All Automation & Daily Digest»):
/// - Единое окно оперативной сводки студии
/// - Кнопка «💰 СОБРАТЬ ВСЕ НАГРАДЫ В ОДИН КЛИК»:
///   - Запускает сбор дивидендов, распродажи ассетов, калибровку спутников, телеметрию Марса и ИИ-спринты
///   - Начисляет единый мега-бонус дайджеста (+50k ₽, +25k C#, 100% комбо «В Потоке»)
/// - Симулятор Оффлайн-Смены (Offline Time Acceleration Simulator & Time Warp 2h):
///   - Расчет выработки AI-отделом и командой студии за время отсутствия
///   - Ускоритель смены Time Warp (мгновенная симуляция 2 часов работы с кулдауном)
/// </summary>
public class DailyDigestUI : MonoBehaviour
{
    private static DailyDigestUI instance;
    public static DailyDigestUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DailyDigestUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(DailyDigestUI));
                    instance = go.AddComponent<DailyDigestUI>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openDigestBtn;

    [Header("Панель сбора")]
    [SerializeField] private TMP_Text digestStatsTxt;
    [SerializeField] private Button claimAllBtn;
    [SerializeField] private TMP_Text claimAllBtnTxt;

    [Header("Time Warp Ускоритель")]
    [SerializeField] private Button timeWarpBtn;
    [SerializeField] private TMP_Text timeWarpBtnTxt;

    private const string PrefDigestClaimsCount = "Daily_Digest_Claims_Count";
    private const string PrefLastSeenTimestamp = "Daily_Digest_LastSeenTime";
    private const string PrefTimeWarpCooldown = "Daily_Digest_TimeWarpCooldown";

    private int claimsCompleted = 0;
    private bool isClaiming = false;
    private float autoSaveTimer = 0f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        claimsCompleted = PlayerPrefs.GetInt(PrefDigestClaimsCount, 0);
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
        RecordLastSeenTimestamp();
    }

    private void Update()
    {
        autoSaveTimer += Time.unscaledDeltaTime;
        if (autoSaveTimer >= 15f)
        {
            autoSaveTimer = 0f;
            RecordLastSeenTimestamp();
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            UpdateTimeWarpButtonState();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            RecordLastSeenTimestamp();
        }
    }

    private void OnApplicationQuit()
    {
        RecordLastSeenTimestamp();
    }

    private void RecordLastSeenTimestamp()
    {
        PlayerPrefs.SetString(PrefLastSeenTimestamp, DateTime.UtcNow.ToString("o"));
        PlayerPrefs.Save();
    }

    public void OpenModal() => OpenDigest();

    public void OpenDigest()
    {
        if (modalRoot == null)
        {
            BuildUI();
        }

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

    public void CloseDigest()
    {
        if (modalRoot != null && modalRoot.activeSelf)
        {
            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            StopAllCoroutines();
            StartCoroutine(AnimateModalClose());
        }
    }

    private IEnumerator AnimateModalOpen()
    {
        float timer = 0f;
        float duration = 0.16f;
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

    public void ClaimAllRewards()
    {
        if (isClaiming) return;
        StartCoroutine(ClaimAllRoutine());
    }

    private IEnumerator ClaimAllRoutine()
    {
        isClaiming = true;
        if (claimAllBtn != null) claimAllBtn.interactable = false;

        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (claimAllBtnTxt != null)
        {
            claimAllBtnTxt.text = "⚡ СБОР ВСЕХ НАГРАД И ДИВИДЕНДОВ СТУДИИ...";
        }

        yield return new WaitForSecondsRealtime(1.0f);

        // 1. Активируем доступные действия подсистем
        try
        {
            if (AssetStoreMarketplaceUI.Instance != null) AssetStoreMarketplaceUI.Instance.LaunchFlashSale();
            if (VentureCapitalFundUI.Instance != null) VentureCapitalFundUI.Instance.CollectDividends();
            if (OrbitalSatelliteUplinkUI.Instance != null) OrbitalSatelliteUplinkUI.Instance.CalibrateUplink();
            if (QuantumDataCenterUI.Instance != null) QuantumDataCenterUI.Instance.TriggerSuperposition();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[DailyDigest] Subsystem claim notice: " + ex.Message);
        }

        claimsCompleted++;
        PlayerPrefs.SetInt(PrefDigestClaimsCount, claimsCompleted);
        PlayerPrefs.Save();

        // 2. Мега-бонус сводного сбора
        double bonusMoney = 85000.0 * (GameManager.Instance != null ? GameManager.Instance.GetGlobalMultiplier() : 1.0);
        double bonusCode = 55000.0 * (GameManager.Instance != null ? GameManager.Instance.GetGlobalMultiplier() : 1.0);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(bonusMoney);
            GameManager.Instance.AddLinesOfCode(bonusCode);
            GameManager.Instance.AddComboEnergy(1.0f); // 100% комбо В Потоке (x3.0)
        }

        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 ДАЙДЖЕСТ СОБРАН!\nКОМБО 100% (x3.0)! <b>+{NumberFormatter.Format(bonusMoney)} ₽</b> (+{NumberFormatter.Format(bonusCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isClaiming = false;
        if (claimAllBtn != null) claimAllBtn.interactable = true;
        if (claimAllBtnTxt != null) claimAllBtnTxt.text = "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ";
    }

    public void TriggerTimeWarp()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long nextAllowed = (long)PlayerPrefs.GetFloat(PrefTimeWarpCooldown, 0f);
        if (now < nextAllowed)
        {
            long remaining = nextAllowed - now;
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"⏳ Ожидание Time Warp: {remaining / 60:D2}:{remaining % 60:D2}", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        // Симуляция 2 часов (7200 секунд)
        double cps = GameManager.Instance != null ? GameManager.Instance.GetCodePerSecond() : 10.0;
        double mps = GameManager.Instance != null ? GameManager.Instance.GetMoneyPerSecond() : 10.0;
        double mult = GameManager.Instance != null ? GameManager.Instance.GetGlobalMultiplier() : 1.0;

        float efficiency = GetAutonomousEfficiency();
        double simulatedCode = cps * 7200.0 * efficiency;
        double simulatedMoney = mps * 7200.0 * efficiency;

        // Минимальный гарантированный буст для ранней игры
        if (simulatedCode < 20000.0 * mult) simulatedCode = 20000.0 * mult;
        if (simulatedMoney < 35000.0 * mult) simulatedMoney = 35000.0 * mult;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddLinesOfCode(simulatedCode);
            GameManager.Instance.AddMoney(simulatedMoney);
            GameManager.Instance.AddComboEnergy(1.0f);
        }

        // Устанавливаем кулдаун на 30 минут (1800 сек)
        PlayerPrefs.SetFloat(PrefTimeWarpCooldown, (float)(now + 1800));
        PlayerPrefs.Save();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"⚡ TIME WARP (2 ЧАСА СМЕНЫ)!\n+{NumberFormatter.Format(simulatedCode)} C#  |  +{NumberFormatter.Format(simulatedMoney)} ₽", transform.position, new Color(0.2f, 1f, 0.8f), true);
        }

        RefreshUI();
    }

    private float GetAutonomousEfficiency()
    {
        int aiLvl = PlayerPrefs.GetInt("AutonomousAI_Level", 0);
        int cats = PlayerPrefs.GetInt("StudioCatHaven_CatCount", 0);
        int cicd = PlayerPrefs.GetInt("ShopUpgrade_2", 0); // Авто-CI/CD боты
        float eff = 0.45f + (aiLvl * 0.05f) + (cats * 0.02f) + (cicd * 0.03f);
        return Mathf.Clamp(eff, 0.45f, 1.50f);
    }

    private void UpdateTimeWarpButtonState()
    {
        if (timeWarpBtn == null || timeWarpBtnTxt == null) return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long nextAllowed = (long)PlayerPrefs.GetFloat(PrefTimeWarpCooldown, 0f);

        if (now >= nextAllowed)
        {
            timeWarpBtn.interactable = true;
            timeWarpBtnTxt.text = "⚡ ЗАПУСТИТЬ TIME WARP (СИМУЛЯЦИЯ 2 ЧАСОВ)";
        }
        else
        {
            timeWarpBtn.interactable = false;
            long rem = nextAllowed - now;
            timeWarpBtnTxt.text = $"⏳ TIME WARP НА ЗАРЯДКЕ ({rem / 60:D2}:{rem % 60:D2})";
        }
    }

    public void RefreshUI()
    {
        if (digestStatsTxt != null && GameManager.Instance != null)
        {
            double mps = GameManager.Instance.GetMoneyPerSecond();
            double cps = GameManager.Instance.GetCodePerSecond();
            double cpc = GameManager.Instance.GetCodePerClick();
            double mult = GameManager.Instance.GetGlobalMultiplier();
            string rank = GameManager.Instance.GetDeveloperRankTitle();
            float effPercent = GetAutonomousEfficiency() * 100f;

            // Расчет оффлайн времени с последнего сохранения
            string lastSeen = PlayerPrefs.GetString(PrefLastSeenTimestamp, "");
            string offlineInfo = "Студия в активном режиме";
            if (!string.IsNullOrEmpty(lastSeen) && DateTime.TryParse(lastSeen, out DateTime lastDt))
            {
                TimeSpan diff = DateTime.UtcNow - lastDt;
                if (diff.TotalMinutes >= 1.0)
                {
                    offlineInfo = $"Оффлайн-смена: <b>{(int)diff.TotalHours}ч {diff.Minutes}м</b>";
                }
            }

            digestStatsTxt.text = $"Ранг: <b>{rank}</b> | Множитель: <b>x{mult:F2}</b>\n" +
                                  $"Пассивный C#: <b>+{NumberFormatter.Format(cps)} C#/сек</b>\n" +
                                  $"Пассивный доход: <b>+{NumberFormatter.Format(mps)} ₽/сек</b>\n" +
                                  $"Сила клика: <b>+{NumberFormatter.Format(cpc)} C#</b>\n" +
                                  $"Автономность смены: <b>{effPercent:F0}%</b> | {offlineInfo}\n" +
                                  $"Всего сборов дайджеста: <b>{claimsCompleted}</b>";
        }
        UpdateTimeWarpButtonState();
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseDigest);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseDigest);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseDigest);
        if (openDigestBtn != null) openDigestBtn.onClick.AddListener(OpenDigest);
        if (claimAllBtn != null) claimAllBtn.onClick.AddListener(ClaimAllRewards);
        if (timeWarpBtn != null) timeWarpBtn.onClick.AddListener(TriggerTimeWarp);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Modal Root
        modalRoot = new GameObject("DailyDigestModalRoot", typeof(RectTransform));
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
        bgImg.color = new Color(0.02f, 0.05f, 0.08f, 0.88f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(modalRoot.transform, false);
        modalCardTransform = card.transform;
        RectTransform rtCard = card.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(440, 560);
        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.08f, 0.14f, 0.22f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "📊 ЕЖЕДНЕВНЫЙ ДАЙДЖЕСТ СТУДИИ";
        hTxt.fontSize = 17;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(1f, 0.85f, 0.25f);
        RectTransform hRect = headerObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(-40, 34);
        hRect.anchoredPosition = new Vector2(0, -12);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI subTxt = subObj.GetComponent<TextMeshProUGUI>();
        subTxt.text = "Симулятор оффлайн-смены и единый сбор всех наград студии";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.8f, 0.9f, 1f);
        RectTransform sRect = subObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 1f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.pivot = new Vector2(0.5f, 1f);
        sRect.sizeDelta = new Vector2(-40, 20);
        sRect.anchoredPosition = new Vector2(0, -44);

        // Close 'X' Button
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(card.transform, false);
        RectTransform xRect = closeXObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 1f);
        xRect.anchorMax = new Vector2(1f, 1f);
        xRect.pivot = new Vector2(1f, 1f);
        xRect.sizeDelta = new Vector2(32, 32);
        xRect.anchoredPosition = new Vector2(-12, -12);
        closeXObj.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(closeXObj.transform, false);
        TextMeshProUGUI xt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xt.text = "✕";
        xt.fontSize = 17;
        xt.alignment = TextAlignmentOptions.Center;
        xt.color = Color.white;
        RectTransform xtr = xTxtObj.GetComponent<RectTransform>();
        xtr.anchorMin = Vector2.zero;
        xtr.anchorMax = Vector2.one;
        xtr.offsetMin = Vector2.zero;
        xtr.offsetMax = Vector2.zero;

        // Stats Box
        GameObject statsBox = new GameObject("StatsBox", typeof(RectTransform), typeof(Image));
        statsBox.transform.SetParent(card.transform, false);
        RectTransform sbRect = statsBox.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0f, 0f);
        sbRect.anchorMax = new Vector2(1f, 1f);
        sbRect.offsetMin = new Vector2(20, 185);
        sbRect.offsetMax = new Vector2(-20, -70);
        statsBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.14f, 0.90f);

        GameObject stTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        stTxtObj.transform.SetParent(statsBox.transform, false);
        digestStatsTxt = stTxtObj.GetComponent<TextMeshProUGUI>();
        digestStatsTxt.fontSize = 13;
        digestStatsTxt.alignment = TextAlignmentOptions.Center;
        digestStatsTxt.color = new Color(0.3f, 0.95f, 1f);
        RectTransform str = stTxtObj.GetComponent<RectTransform>();
        str.anchorMin = Vector2.zero;
        str.anchorMax = Vector2.one;
        str.offsetMin = new Vector2(10, 10);
        str.offsetMax = new Vector2(-10, -10);

        // Time Warp Button Box
        GameObject twBox = new GameObject("TimeWarpBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        twBox.transform.SetParent(card.transform, false);
        RectTransform twRect = twBox.GetComponent<RectTransform>();
        twRect.anchorMin = new Vector2(0.06f, 0f);
        twRect.anchorMax = new Vector2(0.94f, 0f);
        twRect.pivot = new Vector2(0.5f, 0f);
        twRect.sizeDelta = new Vector2(0, 48);
        twRect.anchoredPosition = new Vector2(0, 126);
        twBox.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.65f, 1f);
        timeWarpBtn = twBox.GetComponent<Button>();

        GameObject twTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        twTxtObj.transform.SetParent(twBox.transform, false);
        timeWarpBtnTxt = twTxtObj.GetComponent<TextMeshProUGUI>();
        timeWarpBtnTxt.text = "⚡ ЗАПУСТИТЬ TIME WARP (СИМУЛЯЦИЯ 2 ЧАСОВ)";
        timeWarpBtnTxt.fontSize = 12;
        timeWarpBtnTxt.fontStyle = FontStyles.Bold;
        timeWarpBtnTxt.alignment = TextAlignmentOptions.Center;
        timeWarpBtnTxt.color = Color.white;
        RectTransform twtR = twTxtObj.GetComponent<RectTransform>();
        twtR.anchorMin = Vector2.zero;
        twtR.anchorMax = Vector2.one;
        twtR.offsetMin = Vector2.zero;
        twtR.offsetMax = Vector2.zero;

        // Action Panel (Кнопка Claim All)
        GameObject claimBtnObj = new GameObject("ClaimAllBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        claimBtnObj.transform.SetParent(card.transform, false);
        RectTransform cbR = claimBtnObj.GetComponent<RectTransform>();
        cbR.anchorMin = new Vector2(0.06f, 0f);
        cbR.anchorMax = new Vector2(0.94f, 0f);
        cbR.pivot = new Vector2(0.5f, 0f);
        cbR.sizeDelta = new Vector2(0, 52);
        cbR.anchoredPosition = new Vector2(0, 66);
        claimBtnObj.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.15f, 1f);
        claimAllBtn = claimBtnObj.GetComponent<Button>();

        GameObject cbTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cbTxtObj.transform.SetParent(claimBtnObj.transform, false);
        claimAllBtnTxt = cbTxtObj.GetComponent<TextMeshProUGUI>();
        claimAllBtnTxt.text = "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ";
        claimAllBtnTxt.fontSize = 13;
        claimAllBtnTxt.fontStyle = FontStyles.Bold;
        claimAllBtnTxt.alignment = TextAlignmentOptions.Center;
        claimAllBtnTxt.color = new Color(0.12f, 0.08f, 0.02f);
        RectTransform cbtr = cbTxtObj.GetComponent<RectTransform>();
        cbtr.anchorMin = Vector2.zero;
        cbtr.anchorMax = Vector2.one;
        cbtr.offsetMin = Vector2.zero;
        cbtr.offsetMax = Vector2.zero;

        // Close Bottom Button
        GameObject clBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        clBtnObj.transform.SetParent(card.transform, false);
        RectTransform clr = clBtnObj.GetComponent<RectTransform>();
        clr.anchorMin = new Vector2(0.2f, 0f);
        clr.anchorMax = new Vector2(0.8f, 0f);
        clr.pivot = new Vector2(0.5f, 0f);
        clr.sizeDelta = new Vector2(0, 36);
        clr.anchoredPosition = new Vector2(0, 16);
        clBtnObj.GetComponent<Image>().color = new Color(0.16f, 0.22f, 0.32f, 1f);
        closeBtn = clBtnObj.GetComponent<Button>();

        GameObject clTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        clTxtObj.transform.SetParent(clBtnObj.transform, false);
        TextMeshProUGUI clt = clTxtObj.GetComponent<TextMeshProUGUI>();
        clt.text = "ЗАКРЫТЬ ДАЙДЖЕСТ";
        clt.fontSize = 12;
        clt.fontStyle = FontStyles.Bold;
        clt.alignment = TextAlignmentOptions.Center;
        clt.color = Color.white;
        RectTransform cltr = clTxtObj.GetComponent<RectTransform>();
        cltr.anchorMin = Vector2.zero;
        cltr.anchorMax = Vector2.one;
        cltr.offsetMin = Vector2.zero;
        cltr.offsetMax = Vector2.zero;

        // Floating Quick Launch Button
        GameObject launchBtnObj = new GameObject("DailyDigestLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        launchBtnObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = launchBtnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(170f, 15f);
        rt.sizeDelta = new Vector2(52f, 52f);
        launchBtnObj.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.15f, 0.95f);
        openDigestBtn = launchBtnObj.GetComponent<Button>();

        GameObject lTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lTxtObj.transform.SetParent(launchBtnObj.transform, false);
        TextMeshProUGUI lt = lTxtObj.GetComponent<TextMeshProUGUI>();
        lt.text = "📋";
        lt.fontSize = 24;
        lt.alignment = TextAlignmentOptions.Center;
        RectTransform ltr = lTxtObj.GetComponent<RectTransform>();
        ltr.anchorMin = Vector2.zero;
        ltr.anchorMax = Vector2.one;
        ltr.offsetMin = Vector2.zero;
        ltr.offsetMax = Vector2.zero;

        WireButtons();
        RefreshUI();
        modalRoot.SetActive(false);
    }
}
