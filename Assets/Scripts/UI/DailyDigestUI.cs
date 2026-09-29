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
/// - Сводная статистика эффективности студии (C#/сек, ₽/сек, синергия множителей)
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

    private const string PrefDigestClaimsCount = "Daily_Digest_Claims_Count";
    private int claimsCompleted = 0;
    private bool isClaiming = false;

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
    }

    public void OpenModal() => OpenDigest();

    public void OpenDigest()
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

    public void CloseDigest()
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

        yield return new WaitForSecondsRealtime(1.2f);

        // 1. Активируем доступные действия подсистем
        try
        {
            if (AssetStoreMarketplaceUI.Instance != null) AssetStoreMarketplaceUI.Instance.LaunchFlashSale();
            if (VentureCapitalFundUI.Instance != null) VentureCapitalFundUI.Instance.CollectDividends();
            if (OrbitalSatelliteUplinkUI.Instance != null) OrbitalSatelliteUplinkUI.Instance.CalibrateUplink();
            if (QuantumDataCenterUI.Instance != null) QuantumDataCenterUI.Instance.TriggerSuperposition();
            if (EsportsArenaLeagueUI.Instance != null) EsportsArenaLeagueUI.Instance.HostMajorGrandFinals();
            if (CorporateBoardroomUI.Instance != null) CorporateBoardroomUI.Instance.ExecuteMegaDeal();
            if (CloudGamingStreamUI.Instance != null) CloudGamingStreamUI.Instance.HostFreeWeekend();
            if (NeuroInterfaceLabUI.Instance != null) NeuroInterfaceLabUI.Instance.TriggerNeuralSync();
            if (MarsColonyStudioUI.Instance != null) MarsColonyStudioUI.Instance.ReceiveTelemetryPacket();
            if (AutonomousAiAgentsUI.Instance != null) AutonomousAiAgentsUI.Instance.StartAutoSprint();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[DailyDigest] Subsystem claim notice: " + ex.Message);
        }

        claimsCompleted++;
        PlayerPrefs.SetInt(PrefDigestClaimsCount, claimsCompleted);
        PlayerPrefs.Save();

        // 2. Мега-бонус сводного сбора
        double bonusMoney = 75000.0 * (GameManager.Instance != null ? GameManager.Instance.GetGlobalMultiplier() : 1.0);
        double bonusCode = 45000.0 * (GameManager.Instance != null ? GameManager.Instance.GetGlobalMultiplier() : 1.0);

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
            ClickJuice.Instance.SpawnCustomPopup($"🎉 СВОДНЫЙ ДАЙДЖЕСТ СОБРАН!\nКОМБО 100% (x3.0)! <b>+{NumberFormatter.Format(bonusMoney)} ₽</b> (+{NumberFormatter.Format(bonusCode)} C#)", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        isClaiming = false;
        if (claimAllBtn != null) claimAllBtn.interactable = true;
        if (claimAllBtnTxt != null) claimAllBtnTxt.text = "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ";
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

            digestStatsTxt.text = $"Ранг: <b>{rank}</b>\n" +
                                  $"Пассивный C#: <b>+{NumberFormatter.Format(cps)} C#/сек</b>\n" +
                                  $"Пассивный доход: <b>+{NumberFormatter.Format(mps)} ₽/сек</b>\n" +
                                  $"Сила клика: <b>+{NumberFormatter.Format(cpc)} C#</b>\n" +
                                  $"Глобальный множитель: <b>x{mult:F2}</b>\n" +
                                  $"Всего сборов дайджеста: <b>{claimsCompleted}</b>";
        }
    }

    private void WireButtons()
    {
        if (closeBtn != null) closeBtn.onClick.AddListener(CloseDigest);
        if (closeXBtn != null) closeXBtn.onClick.AddListener(CloseDigest);
        if (backdropBtn != null) backdropBtn.onClick.AddListener(CloseDigest);
        if (openDigestBtn != null) openDigestBtn.onClick.AddListener(OpenDigest);
        if (claimAllBtn != null) claimAllBtn.onClick.AddListener(ClaimAllRewards);
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
        rtCard.sizeDelta = new Vector2(440, 520);
        Image cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.08f, 0.14f, 0.22f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "📊 ЕЖЕДНЕВНЫЙ ДАЙДЖЕСТ СТУДИИ";
        hTxt.fontSize = 18;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(1f, 0.85f, 0.25f);
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
        subTxt.text = "Сводный операционный центр и автоматический сбор наград";
        subTxt.fontSize = 11;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.8f, 0.9f, 1f);
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

        // Stats Box
        GameObject statsBox = new GameObject("StatsBox", typeof(RectTransform), typeof(Image));
        statsBox.transform.SetParent(card.transform, false);
        RectTransform sbRect = statsBox.GetComponent<RectTransform>();
        sbRect.anchorMin = new Vector2(0f, 0f);
        sbRect.anchorMax = new Vector2(1f, 1f);
        sbRect.offsetMin = new Vector2(20, 140);
        sbRect.offsetMax = new Vector2(-20, -80);
        statsBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.14f, 0.85f);

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

        // Action Panel (Кнопка Claim All)
        GameObject actPanel = new GameObject("ActionPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 0f);
        apRect.anchorMax = new Vector2(1f, 0f);
        apRect.pivot = new Vector2(0.5f, 0f);
        apRect.sizeDelta = new Vector2(-40, 56);
        apRect.anchoredPosition = new Vector2(0, 68);
        actPanel.GetComponent<Image>().color = new Color(0.12f, 0.22f, 0.35f, 1f);

        GameObject claimBtnObj = new GameObject("ClaimAllBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        claimBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform cbR = claimBtnObj.GetComponent<RectTransform>();
        cbR.anchorMin = Vector2.zero;
        cbR.anchorMax = Vector2.one;
        cbR.offsetMin = new Vector2(6, 6);
        cbR.offsetMax = new Vector2(-6, -6);
        claimBtnObj.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.15f, 1f);
        claimAllBtn = claimBtnObj.GetComponent<Button>();

        GameObject cbTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cbTxtObj.transform.SetParent(claimBtnObj.transform, false);
        claimAllBtnTxt = cbTxtObj.GetComponent<TextMeshProUGUI>();
        claimAllBtnTxt.text = "💰 СОБРАТЬ ВСЕ НАГРАДЫ И ДИВИДЕНДЫ";
        claimAllBtnTxt.fontSize = 13;
        claimAllBtnTxt.fontStyle = FontStyles.Bold;
        claimAllBtnTxt.alignment = TextAlignmentOptions.Center;
        claimAllBtnTxt.color = new Color(0.10f, 0.08f, 0.02f);
        RectTransform ctr = cbTxtObj.GetComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero;
        ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero;
        ctr.offsetMax = Vector2.zero;

        // Bottom Close Button
        GameObject botCloseObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(160, 38);
        bcRect.anchoredPosition = new Vector2(0, 14);
        botCloseObj.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.42f, 1f);
        closeBtn = botCloseObj.GetComponent<Button>();

        GameObject bcTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bcTxtObj.transform.SetParent(botCloseObj.transform, false);
        TextMeshProUGUI bct = bcTxtObj.GetComponent<TextMeshProUGUI>();
        bct.text = "ЗАКРЫТЬ";
        bct.fontSize = 13;
        bct.fontStyle = FontStyles.Bold;
        bct.alignment = TextAlignmentOptions.Center;
        bct.color = Color.white;
        RectTransform bctr = bcTxtObj.GetComponent<RectTransform>();
        bctr.anchorMin = Vector2.zero;
        bctr.anchorMax = Vector2.one;
        bctr.offsetMin = Vector2.zero;
        bctr.offsetMax = Vector2.zero;

        // Создаем кнопку вызова дайджеста (в верхнем левом углу или рядом с хабом)
        CreateDigestLauncher(canvas.transform);

        WireButtons();
        RefreshUI();
        modalRoot.SetActive(false);
    }

    private void CreateDigestLauncher(Transform canvasTransform)
    {
        GameObject digestLaunchObj = new GameObject("DailyDigestLaunchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        digestLaunchObj.transform.SetParent(canvasTransform, false);
        RectTransform dlRect = digestLaunchObj.GetComponent<RectTransform>();
        dlRect.anchorMin = new Vector2(0f, 1f);
        dlRect.anchorMax = new Vector2(0f, 1f);
        dlRect.pivot = new Vector2(0f, 1f);
        dlRect.sizeDelta = new Vector2(44, 44);
        dlRect.anchoredPosition = new Vector2(12, -96);
        digestLaunchObj.GetComponent<Image>().color = new Color(0.95f, 0.70f, 0.15f, 0.95f);
        openDigestBtn = digestLaunchObj.GetComponent<Button>();

        GameObject dlTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        dlTxtObj.transform.SetParent(digestLaunchObj.transform, false);
        TextMeshProUGUI dlt = dlTxtObj.GetComponent<TextMeshProUGUI>();
        dlt.text = "📊";
        dlt.fontSize = 20;
        dlt.alignment = TextAlignmentOptions.Center;
        RectTransform dtr = dlTxtObj.GetComponent<RectTransform>();
        dtr.anchorMin = Vector2.zero;
        dtr.anchorMax = Vector2.one;
        dtr.offsetMin = Vector2.zero;
        dtr.offsetMax = Vector2.zero;
    }
}
