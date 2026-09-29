using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Венчурные инвестиции и раунды финансирования (Venture Capital & Seed Rounds):
/// - 5 инвестиционных раундов (Pre-Seed, Seed, Series A, Series B, IPO)
/// - Питч-сессия перед инвесторами с выбором фокуса (Инновации, Монетизация, Виральность)
/// - Привлечение крупных грантов в обмен на миноритарную долю в акциях $TAP
/// - Оценка компании (Valuation) и рост рыночной капитализации
/// </summary>
public class VentureCapitalUI : MonoBehaviour
{
    private static VentureCapitalUI instance;
    public static VentureCapitalUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<VentureCapitalUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class FundingRound
    {
        public string id;
        public string title;
        public string investorName;
        public string investorIcon;
        public double companyValuation;
        public double investmentGrant;
        public double equityPercent; // Процент доли
        public int minReleases;
        public int minOfficeTier;
        public bool isCompleted;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openVentureBtn;
    [SerializeField] private TMP_Text openVentureBtnText;
    [SerializeField] private Button closeVentureBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Оценка компании")]
    [SerializeField] private TMP_Text companyValuationText;
    [SerializeField] private TMP_Text equityHeldText;

    [Header("Контейнер раундов")]
    [SerializeField] private Transform roundsContainer;

    [Header("Интерактивный питч-модал")]
    [SerializeField] private GameObject pitchModalRoot;
    [SerializeField] private TMP_Text pitchTitleText;
    [SerializeField] private TMP_Text pitchInvestorText;
    [SerializeField] private Button pitchOption1Btn;
    [SerializeField] private Button pitchOption2Btn;
    [SerializeField] private Button pitchOption3Btn;

    private readonly List<FundingRound> rounds = new List<FundingRound>();
    private double founderEquityPercent = 100.0;
    private int pendingPitchRoundIndex = -1;

    private const string PrefRoundPrefix = "Studio_VentureRound_";
    private const string PrefEquity = "Studio_FounderEquity";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public double FounderEquityPercent => founderEquityPercent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeRounds();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        if (pitchModalRoot != null) pitchModalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeRounds()
    {
        rounds.Clear();

        rounds.Add(new FundingRound
        {
            id = "round_preseed",
            title = "Pre-Seed Раунд",
            investorName = "Бизнес-ангел 'Дядя Валера'",
            investorIcon = "👼",
            companyValuation = 300000.0,
            investmentGrant = 75000.0,
            equityPercent = 10.0,
            minReleases = 1,
            minOfficeTier = 0,
            isCompleted = false
        });

        rounds.Add(new FundingRound
        {
            id = "round_seed",
            title = "Seed Раунд",
            investorName = "Фонд 'Y-Combinator CIS'",
            investorIcon = "🌱",
            companyValuation = 2000000.0,
            investmentGrant = 450000.0,
            equityPercent = 12.0,
            minReleases = 2,
            minOfficeTier = 1,
            isCompleted = false
        });

        rounds.Add(new FundingRound
        {
            id = "round_series_a",
            title = "Раунд Series A",
            investorName = "Sequoia Capital Games",
            investorIcon = "🌲",
            companyValuation = 15000000.0,
            investmentGrant = 3000000.0,
            equityPercent = 10.0,
            minReleases = 3,
            minOfficeTier = 2,
            isCompleted = false
        });

        rounds.Add(new FundingRound
        {
            id = "round_series_b",
            title = "Раунд Series B",
            investorName = "SoftBank Vision Fund",
            investorIcon = "🦅",
            companyValuation = 90000000.0,
            investmentGrant = 18000000.0,
            equityPercent = 8.0,
            minReleases = 4,
            minOfficeTier = 3,
            isCompleted = false
        });

        rounds.Add(new FundingRound
        {
            id = "round_ipo",
            title = "Выход на IPO (NASDAQ)",
            investorName = "Публичные инвесторы биржи",
            investorIcon = "🔔",
            companyValuation = 500000000.0,
            investmentGrant = 100000000.0,
            equityPercent = 5.0,
            minReleases = 5,
            minOfficeTier = 3,
            isCompleted = false
        });
    }

    private void LoadData()
    {
        founderEquityPercent = double.TryParse(PlayerPrefs.GetString(PrefEquity, "100"), out double eq) ? eq : 100.0;

        for (int i = 0; i < rounds.Count; i++)
        {
            rounds[i].isCompleted = PlayerPrefs.GetInt(PrefRoundPrefix + rounds[i].id, 0) == 1;
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(PrefEquity, founderEquityPercent.ToString("R"));
        for (int i = 0; i < rounds.Count; i++)
        {
            PlayerPrefs.SetInt(PrefRoundPrefix + rounds[i].id, rounds[i].isCompleted ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public double GetCurrentCompanyValuation()
    {
        double baseVal = 200000.0;
        if (GameManager.Instance != null)
        {
            baseVal += GameManager.Instance.TotalMoneyEarned * 1.5;
            baseVal += GameManager.Instance.TotalReleasesCount * 500000.0;
            baseVal += GameManager.Instance.PrestigeLevel * 2000000.0;
        }
        return baseVal;
    }

    public void StartPitchSession(int roundIndex)
    {
        if (roundIndex < 0 || roundIndex >= rounds.Count) return;
        var r = rounds[roundIndex];

        if (r.isCompleted) return;

        // Проверка требований
        int releases = GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount : 0;
        int office = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.CurrentTierIndex : 0;

        if (releases < r.minReleases || office < r.minOfficeTier)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🔒 Требования: {r.minReleases} релиза, офис #{r.minOfficeTier + 1}!", transform.position, new Color(1f, 0.4f, 0.2f), false);
            }
            return;
        }

        pendingPitchRoundIndex = roundIndex;
        OpenPitchModal(r);
    }

    private void OpenPitchModal(FundingRound r)
    {
        if (pitchModalRoot != null)
        {
            pitchModalRoot.SetActive(true);
        }

        if (pitchTitleText != null) pitchTitleText.text = $"ПИТЧ: {r.title}";
        if (pitchInvestorText != null) pitchInvestorText.text = $"Перед вами представитель: <b>{r.investorName}</b> {r.investorIcon}\nОценка студии: <b>{NumberFormatter.Format(r.companyValuation)} ₽</b>\nИнвестиции: <b>+{NumberFormatter.Format(r.investmentGrant)} ₽</b> (Доля: {r.equityPercent:F0}%)";

        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    public void ResolvePitch(int choice)
    {
        if (pendingPitchRoundIndex < 0 || pendingPitchRoundIndex >= rounds.Count) return;
        var r = rounds[pendingPitchRoundIndex];

        double bonusMultiplier = 1.0;
        string resultMsg = "";

        switch (choice)
        {
            case 1: // Инновации и AI
                bonusMultiplier = 1.25;
                resultMsg = "💡 Инвесторы восхищены видением AI-технологий! Оценка увеличена на +25%!";
                break;
            case 2: // Агрессивная монетизация
                bonusMultiplier = 1.15;
                resultMsg = "💰 Финансовые метрики убедили партнеров! +15% к сумме чека!";
                break;
            case 3: // Виральный мем-хайп
                bonusMultiplier = 1.30;
                resultMsg = "🔥 Стримы и виральность взорвали презентацию! Грант увеличен на +30%!";
                break;
        }

        double finalGrant = r.investmentGrant * bonusMultiplier;
        r.isCompleted = true;
        founderEquityPercent = Math.Max(10.0, founderEquityPercent - r.equityPercent);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(finalGrant);
        }

        SaveData();

        if (pitchModalRoot != null) pitchModalRoot.SetActive(false);
        pendingPitchRoundIndex = -1;

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 СДЕЛКА ЗАКРЫТА!\n+{NumberFormatter.Format(finalGrant)} ₽\n{resultMsg}", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openVentureBtn != null)
        {
            openVentureBtn.onClick.RemoveAllListeners();
            openVentureBtn.onClick.AddListener(OpenModal);
        }
        if (closeVentureBtn != null)
        {
            closeVentureBtn.onClick.RemoveAllListeners();
            closeVentureBtn.onClick.AddListener(CloseModal);
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

        if (pitchOption1Btn != null)
        {
            pitchOption1Btn.onClick.RemoveAllListeners();
            pitchOption1Btn.onClick.AddListener(() => ResolvePitch(1));
        }
        if (pitchOption2Btn != null)
        {
            pitchOption2Btn.onClick.RemoveAllListeners();
            pitchOption2Btn.onClick.AddListener(() => ResolvePitch(2));
        }
        if (pitchOption3Btn != null)
        {
            pitchOption3Btn.onClick.RemoveAllListeners();
            pitchOption3Btn.onClick.AddListener(() => ResolvePitch(3));
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
        if (pitchModalRoot != null) pitchModalRoot.SetActive(false);
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
        if (companyValuationText != null)
        {
            companyValuationText.text = $"🏛️ Оценка компании: <b>{NumberFormatter.Format(GetCurrentCompanyValuation())} ₽</b>";
        }

        if (equityHeldText != null)
        {
            equityHeldText.text = $"👑 Доля основателя: <b>{founderEquityPercent:F1}%</b> (Инвесторы: {100.0 - founderEquityPercent:F1}%)";
        }

        RefreshRoundCards();
    }

    private void RefreshRoundCards()
    {
        if (roundsContainer == null) return;

        int releases = GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount : 0;
        int office = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.CurrentTierIndex : 0;

        for (int i = 0; i < rounds.Count; i++)
        {
            int idx = i;
            var r = rounds[i];
            Transform cardTr = roundsContainer.Find($"RoundCard_{idx}");
            if (cardTr == null) continue;

            Button pitchBtn = cardTr.Find("PitchBtn")?.GetComponent<Button>();
            TMP_Text pitchBtnTxt = pitchBtn != null ? pitchBtn.GetComponentInChildren<TMP_Text>() : null;
            Image bg = cardTr.GetComponent<Image>();

            bool unlocked = releases >= r.minReleases && office >= r.minOfficeTier;

            if (bg != null)
            {
                if (r.isCompleted) bg.color = new Color(0.1f, 0.22f, 0.16f, 0.95f);
                else if (unlocked) bg.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
                else bg.color = new Color(0.06f, 0.08f, 0.11f, 0.85f);
            }

            if (pitchBtn != null && pitchBtnTxt != null)
            {
                pitchBtn.onClick.RemoveAllListeners();
                pitchBtn.onClick.AddListener(() => StartPitchSession(idx));

                if (r.isCompleted)
                {
                    pitchBtn.interactable = false;
                    pitchBtnTxt.text = "✓ РАУНД ЗАКРЫТ";
                }
                else if (unlocked)
                {
                    pitchBtn.interactable = true;
                    pitchBtnTxt.text = "🎤 НАЧАТЬ ПИТЧ";
                }
                else
                {
                    pitchBtn.interactable = false;
                    pitchBtnTxt.text = $"🔒 {r.minReleases} рел. / Офис #{r.minOfficeTier + 1}";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("VentureCapitalModal", typeof(RectTransform));
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
        outline.effectColor = new Color(1f, 0.82f, 0.2f, 0.5f);
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
        headerTxt.text = "💼 ВЕНЧУРНЫЕ ИНВЕСТИЦИИ";
        headerTxt.fontSize = 20;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.85f, 0.25f);

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

        // Status Panel
        GameObject statusPanel = new GameObject("StatusPanel", typeof(RectTransform), typeof(Image));
        statusPanel.transform.SetParent(cardObj.transform, false);
        RectTransform statusRt = statusPanel.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0, 1);
        statusRt.anchorMax = new Vector2(1, 1);
        statusRt.pivot = new Vector2(0.5f, 1);
        statusRt.anchoredPosition = new Vector2(0, -60);
        statusRt.sizeDelta = new Vector2(-40, 56);
        statusPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.9f);

        GameObject valObj = new GameObject("Valuation", typeof(RectTransform), typeof(TextMeshProUGUI));
        valObj.transform.SetParent(statusPanel.transform, false);
        RectTransform valRt = valObj.GetComponent<RectTransform>();
        valRt.anchorMin = new Vector2(0, 0.5f);
        valRt.anchorMax = new Vector2(1, 1);
        valRt.offsetMin = new Vector2(10, 0);
        valRt.offsetMax = new Vector2(-10, -2);
        companyValuationText = valObj.GetComponent<TextMeshProUGUI>();
        companyValuationText.fontSize = 13;

        GameObject eqObj = new GameObject("Equity", typeof(RectTransform), typeof(TextMeshProUGUI));
        eqObj.transform.SetParent(statusPanel.transform, false);
        RectTransform eqRt = eqObj.GetComponent<RectTransform>();
        eqRt.anchorMin = new Vector2(0, 0);
        eqRt.anchorMax = new Vector2(1, 0.5f);
        eqRt.offsetMin = new Vector2(10, 2);
        eqRt.offsetMax = new Vector2(-10, 0);
        equityHeldText = eqObj.GetComponent<TextMeshProUGUI>();
        equityHeldText.fontSize = 12;
        equityHeldText.color = new Color(0.8f, 0.85f, 0.95f);

        // Scroll View with Rounds
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(20, 70);
        scrollRt.offsetMax = new Vector2(-20, -125);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        roundsContainer = contentObj.transform;
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

        for (int i = 0; i < rounds.Count; i++)
        {
            CreateRoundCardTemplate(roundsContainer, rounds[i], i);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 16);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeVentureBtn = closeBtnObj.GetComponent<Button>();

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

        // Setup Pitch Sub-Modal
        CreatePitchSubModal(root.transform);

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private void CreateRoundCardTemplate(Transform parent, FundingRound r, int index)
    {
        GameObject card = new GameObject($"RoundCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 92);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        // Title & Investor
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{r.investorIcon} <b>{r.title}</b> — <color=#A0C0E0>{r.investorName}</color>";
        tt.fontSize = 13;

        // Details
        GameObject d = new GameObject("Details", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.65f, 1);
        drt.offsetMin = new Vector2(10, 8);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"💰 Грант: <color=#00FF88>+{NumberFormatter.Format(r.investmentGrant)} ₽</color>\n⚖️ Доля: {r.equityPercent:F0}% | Оценка: {NumberFormatter.Format(r.companyValuation)} ₽";
        dt.fontSize = 11;
        dt.color = new Color(0.8f, 0.85f, 0.9f);

        // Pitch Button
        GameObject b = new GameObject("PitchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-10, -4);
        brt.sizeDelta = new Vector2(145, 34);
        b.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.85f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "🎤 НАЧАТЬ ПИТЧ";
        btxt.fontSize = 11;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void CreatePitchSubModal(Transform parent)
    {
        GameObject pitchRoot = new GameObject("PitchModal", typeof(RectTransform));
        pitchRoot.transform.SetParent(parent, false);
        RectTransform prt = pitchRoot.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.sizeDelta = Vector2.zero;

        // Card
        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(pitchRoot.transform, false);
        RectTransform crt = card.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(440, 420);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.99f);
        var outline = card.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 1f, 0.6f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Title
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, -15);
        trt.sizeDelta = new Vector2(-30, 30);
        pitchTitleText = t.GetComponent<TextMeshProUGUI>();
        pitchTitleText.fontSize = 18;
        pitchTitleText.fontStyle = FontStyles.Bold;
        pitchTitleText.alignment = TextAlignmentOptions.Center;
        pitchTitleText.color = new Color(0.2f, 1f, 0.6f);

        // Investor desc
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 1);
        drt.anchorMax = new Vector2(1, 1);
        drt.pivot = new Vector2(0.5f, 1);
        drt.anchoredPosition = new Vector2(0, -50);
        drt.sizeDelta = new Vector2(-30, 80);
        pitchInvestorText = d.GetComponent<TextMeshProUGUI>();
        pitchInvestorText.fontSize = 12;
        pitchInvestorText.alignment = TextAlignmentOptions.Center;
        pitchInvestorText.color = new Color(0.85f, 0.9f, 0.95f);

        // Options
        pitchOption1Btn = CreatePitchChoiceButton(card.transform, "🤖 Фокус: AI-Технологии & Инновации (+25% к чеку)", -150, new Color(0.12f, 0.5f, 0.7f));
        pitchOption2Btn = CreatePitchChoiceButton(card.transform, "💰 Фокус: Агрессивная монетизация & Донат (+15% к чеку)", -215, new Color(0.15f, 0.6f, 0.4f));
        pitchOption3Btn = CreatePitchChoiceButton(card.transform, "🔥 Фокус: Виральный мем-хайп & Стримеры (+30% к чеку)", -280, new Color(0.7f, 0.45f, 0.1f));

        // Cancel
        GameObject cBtn = new GameObject("CancelBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cBtn.transform.SetParent(card.transform, false);
        RectTransform cbrt = cBtn.GetComponent<RectTransform>();
        cbrt.anchorMin = new Vector2(0.5f, 0);
        cbrt.anchorMax = new Vector2(0.5f, 0);
        cbrt.pivot = new Vector2(0.5f, 0);
        cbrt.anchoredPosition = new Vector2(0, 16);
        cbrt.sizeDelta = new Vector2(200, 36);
        cBtn.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f);
        var cb = cBtn.GetComponent<Button>();
        cb.onClick.AddListener(() => pitchRoot.SetActive(false));

        GameObject ctxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        ctxtObj.transform.SetParent(cBtn.transform, false);
        RectTransform ctrt = ctxtObj.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.sizeDelta = Vector2.zero;
        TMP_Text ctxt = ctxtObj.GetComponent<TextMeshProUGUI>();
        ctxt.text = "ОТМЕНА";
        ctxt.fontSize = 12;
        ctxt.alignment = TextAlignmentOptions.Center;
        ctxt.color = Color.white;

        pitchModalRoot = pitchRoot;
        pitchModalRoot.SetActive(false);
    }

    private Button CreatePitchChoiceButton(Transform parent, string label, float posY, Color col)
    {
        GameObject b = new GameObject("ChoiceBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(400, 52);
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = new Vector2(-16, 0);
        TMP_Text t = to.GetComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 12;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        return b.GetComponent<Button>();
    }
}
