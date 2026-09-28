using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Инди-краудфандинг на Кикстартере (Indie Kickstarter Campaign):
/// - Запуск кампании сбора средств для новой игры студии
/// - Базовая цель сбора (250 000 ₽) и 4 сверхцели (Stretch Goals)
/// - Маркетинговые действия: Геймплейный трейлер, Концепт-арты, Стрим с разработчиками
/// - Приток бекеров, финансовые гранты и постоянные бонусы к качеству релизов
/// </summary>
public class KickstarterCampaignUI : MonoBehaviour
{
    private static KickstarterCampaignUI instance;
    public static KickstarterCampaignUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<KickstarterCampaignUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class StretchGoal
    {
        public string title;
        public double targetAmount;
        public string perkDescription;
        public bool isUnlocked;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openKickstarterBtn;
    [SerializeField] private TMP_Text openKickstarterBtnText;
    [SerializeField] private Button closeKickstarterBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Показатели кампании")]
    [SerializeField] private TMP_Text campaignStatusText;
    [SerializeField] private TMP_Text fundedAmountText;
    [SerializeField] private TMP_Text backersCountText;
    [SerializeField] private TMP_Text daysRemainingText;
    [SerializeField] private Image progressBarFill;

    [Header("Маркетинговые действия")]
    [SerializeField] private Button actionTrailerBtn;
    [SerializeField] private Button actionConceptArtBtn;
    [SerializeField] private Button actionAmaStreamBtn;
    [SerializeField] private Button startCampaignBtn;
    [SerializeField] private Button claimFundsBtn;

    [Header("Контейнер сверхцелей")]
    [SerializeField] private Transform stretchGoalsContainer;

    private readonly List<StretchGoal> stretchGoals = new List<StretchGoal>();
    private bool isCampaignActive = false;
    private double currentPledged = 0;
    private int backersCount = 0;
    private float campaignRemainingSeconds = 0f;
    private bool isFundsClaimed = false;

    private const double BaseFundingGoal = 250000.0;
    private const float CampaignTotalDuration = 180f; // 3 минуты активной кампании

    private const string PrefPledged = "Studio_Kickstarter_Pledged";
    private const string PrefBackers = "Studio_Kickstarter_Backers";
    private const string PrefActive = "Studio_Kickstarter_Active";
    private const string PrefClaimed = "Studio_Kickstarter_Claimed";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool IsCampaignActive => isCampaignActive;
    public double CurrentPledged => currentPledged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeStretchGoals();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Активный набор кода привлекает бекеров во время кампании
        if (isCampaignActive && campaignRemainingSeconds > 0)
        {
            double backerPledge = (amount * 1.5) + UnityEngine.Random.Range(50f, 350f);
            currentPledged += backerPledge;
            backersCount += UnityEngine.Random.Range(1, 3);
            CheckStretchGoals();
        }
    }

    private void Update()
    {
        if (isCampaignActive)
        {
            campaignRemainingSeconds = Mathf.Max(0f, campaignRemainingSeconds - Time.deltaTime);

            // Пассивный приток бекеров каждые пару секунд
            if (UnityEngine.Random.value < 0.25f)
            {
                currentPledged += UnityEngine.Random.Range(80f, 400f);
                backersCount += 1;
                CheckStretchGoals();
            }

            if (campaignRemainingSeconds <= 0f)
            {
                FinishCampaign();
            }

            if (IsModalOpen) UpdateModalUI();
        }
    }

    private void InitializeStretchGoals()
    {
        stretchGoals.Clear();

        stretchGoals.Add(new StretchGoal
        {
            title = "🎵 Оркестровый Lo-Fi Саундтрек",
            targetAmount = 100000.0,
            perkDescription = "+15% к скорости набора кода и атмосферный звук.",
            isUnlocked = false
        });

        stretchGoals.Add(new StretchGoal
        {
            title = "🌍 Полная локализация на 12 языков",
            targetAmount = 250000.0,
            perkDescription = "Основная цель! +30% к продажам на международном рынке.",
            isUnlocked = false
        });

        stretchGoals.Add(new StretchGoal
        {
            title = "🎮 Портирование на Nintendo Switch & Steam Deck",
            targetAmount = 450000.0,
            perkDescription = "Мультипликатор дохода x1.3 от портативных консолей.",
            isUnlocked = false
        });

        stretchGoals.Add(new StretchGoal
        {
            title = "🎬 Анимированные синематики и озвучка звезд",
            targetAmount = 750000.0,
            perkDescription = "Статус культовой классики и вирусный охват!",
            isUnlocked = false
        });
    }

    private void LoadData()
    {
        currentPledged = double.TryParse(PlayerPrefs.GetString(PrefPledged, "0"), out double p) ? p : 0;
        backersCount = PlayerPrefs.GetInt(PrefBackers, 0);
        isCampaignActive = PlayerPrefs.GetInt(PrefActive, 0) == 1;
        isFundsClaimed = PlayerPrefs.GetInt(PrefClaimed, 0) == 1;

        if (isCampaignActive) campaignRemainingSeconds = 120f;
        CheckStretchGoals();
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(PrefPledged, currentPledged.ToString("R"));
        PlayerPrefs.SetInt(PrefBackers, backersCount);
        PlayerPrefs.SetInt(PrefActive, isCampaignActive ? 1 : 0);
        PlayerPrefs.SetInt(PrefClaimed, isFundsClaimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void CheckStretchGoals()
    {
        for (int i = 0; i < stretchGoals.Count; i++)
        {
            if (!stretchGoals[i].isUnlocked && currentPledged >= stretchGoals[i].targetAmount)
            {
                stretchGoals[i].isUnlocked = true;
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"🎯 СВЕРХЦЕЛЬ ДОСТИГНУТА!\n{stretchGoals[i].title}", transform.position, new Color(0.2f, 1f, 0.6f), true);
                }
            }
        }
    }

    public void StartCampaign()
    {
        if (isCampaignActive) return;

        isCampaignActive = true;
        isFundsClaimed = false;
        currentPledged = 12500.0; // Стартовые взносы
        backersCount = 24;
        campaignRemainingSeconds = CampaignTotalDuration;

        for (int i = 0; i < stretchGoals.Count; i++) stretchGoals[i].isUnlocked = false;

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("🚀 КАМПАНИЯ НА КИКСТАРТЕРЕ ЗАПУЩЕНА!\nСобирайте бекеров кликами и обновлениями!", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        UpdateModalUI();
    }

    public void PublishTrailer()
    {
        if (!isCampaignActive) return;
        double cost = 6000.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.SpendMoney(cost);
            double boost = UnityEngine.Random.Range(25000f, 45000f);
            currentPledged += boost;
            backersCount += UnityEngine.Random.Range(45, 90);
            CheckStretchGoals();

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

            if (ClickJuice.Instance != null && actionTrailerBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎬 ТРЕЙЛЕР В ТРЕНДАХ!\n+{NumberFormatter.Format(boost)} ₽ от новых бекеров!", actionTrailerBtn.transform.position, new Color(1f, 0.84f, 0.2f), false);
            }
            UpdateModalUI();
        }
    }

    public void ShowConceptArts()
    {
        if (!isCampaignActive) return;
        double cost = 3000.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.SpendMoney(cost);
            double boost = UnityEngine.Random.Range(12000f, 24000f);
            currentPledged += boost;
            backersCount += UnityEngine.Random.Range(20, 50);
            CheckStretchGoals();

            HapticFeedback.MediumImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

            if (ClickJuice.Instance != null && actionConceptArtBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎨 АРТЫ ПРИВЛЕКЛИ ВНИМАНИЕ!\n+{NumberFormatter.Format(boost)} ₽!", actionConceptArtBtn.transform.position, new Color(0.3f, 0.8f, 1f), false);
            }
            UpdateModalUI();
        }
    }

    public void HostAmaStream()
    {
        if (!isCampaignActive) return;
        double cost = 10000.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= cost)
        {
            GameManager.Instance.SpendMoney(cost);
            double boost = UnityEngine.Random.Range(40000f, 85000f);
            currentPledged += boost;
            backersCount += UnityEngine.Random.Range(80, 160);
            CheckStretchGoals();

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            if (ClickJuice.Instance != null && actionAmaStreamBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎙️ СТРИМ ВЗОРВАЛ СООБЩЕСТВО!\n+{NumberFormatter.Format(boost)} ₽ от фанатов!", actionAmaStreamBtn.transform.position, new Color(0.85f, 0.45f, 1f), true);
            }
            UpdateModalUI();
        }
    }

    private void FinishCampaign()
    {
        isCampaignActive = false;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        bool isFunded = currentPledged >= BaseFundingGoal;
        string result = isFunded 
            ? $"🎉 КАМПАНИЯ УСПЕШНО ЗАВЕРШЕНА!\nСобрано: {NumberFormatter.Format(currentPledged)} ₽ ({backersCount} бекеров)" 
            : "⚠️ Время кампании истекло. Заберите собранные средства.";

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(result, transform.position, isFunded ? new Color(0.2f, 1f, 0.6f) : new Color(1f, 0.5f, 0.2f), true);
        }

        UpdateModalUI();
    }

    public void ClaimFunds()
    {
        if (isFundsClaimed || isCampaignActive || currentPledged <= 0) return;

        isFundsClaimed = true;
        double payout = currentPledged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(payout);
            GameManager.Instance.AddLinesOfCode(backersCount * 50.0);
            GameManager.Instance.ActivateEnergyBoost(3600f, 1.3);
        }

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null && claimFundsBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 БЮДЖЕТ КИКСТАРТЕРА В КАССЕ!\n+{NumberFormatter.Format(payout)} ₽ на разработку!", claimFundsBtn.transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openKickstarterBtn != null)
        {
            openKickstarterBtn.onClick.RemoveAllListeners();
            openKickstarterBtn.onClick.AddListener(OpenModal);
        }
        if (closeKickstarterBtn != null)
        {
            closeKickstarterBtn.onClick.RemoveAllListeners();
            closeKickstarterBtn.onClick.AddListener(CloseModal);
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

        if (startCampaignBtn != null)
        {
            startCampaignBtn.onClick.RemoveAllListeners();
            startCampaignBtn.onClick.AddListener(StartCampaign);
        }
        if (claimFundsBtn != null)
        {
            claimFundsBtn.onClick.RemoveAllListeners();
            claimFundsBtn.onClick.AddListener(ClaimFunds);
        }
        if (actionTrailerBtn != null)
        {
            actionTrailerBtn.onClick.RemoveAllListeners();
            actionTrailerBtn.onClick.AddListener(PublishTrailer);
        }
        if (actionConceptArtBtn != null)
        {
            actionConceptArtBtn.onClick.RemoveAllListeners();
            actionConceptArtBtn.onClick.AddListener(ShowConceptArts);
        }
        if (actionAmaStreamBtn != null)
        {
            actionAmaStreamBtn.onClick.RemoveAllListeners();
            actionAmaStreamBtn.onClick.AddListener(HostAmaStream);
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
        if (fundedAmountText != null)
        {
            double percent = (currentPledged / BaseFundingGoal) * 100.0;
            fundedAmountText.text = $"💰 Собрано: <b>{NumberFormatter.Format(currentPledged)} ₽</b> ({percent:F0}%)";
        }

        if (backersCountText != null)
        {
            backersCountText.text = $"👥 Бекеров: <b>{backersCount:N0}</b>";
        }

        if (daysRemainingText != null)
        {
            if (isCampaignActive)
            {
                daysRemainingText.text = $"⏳ До конца кампании: <b>{campaignRemainingSeconds:F0} сек.</b>";
            }
            else
            {
                daysRemainingText.text = isFundsClaimed ? "✓ Средства получены" : "Готово к запуску";
            }
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = Mathf.Clamp01((float)(currentPledged / stretchGoals[stretchGoals.Count - 1].targetAmount));
        }

        if (startCampaignBtn != null)
        {
            startCampaignBtn.gameObject.SetActive(!isCampaignActive && (isFundsClaimed || currentPledged == 0));
        }

        if (claimFundsBtn != null)
        {
            claimFundsBtn.gameObject.SetActive(!isCampaignActive && !isFundsClaimed && currentPledged > 0);
        }

        if (actionTrailerBtn != null) actionTrailerBtn.interactable = isCampaignActive;
        if (actionConceptArtBtn != null) actionConceptArtBtn.interactable = isCampaignActive;
        if (actionAmaStreamBtn != null) actionAmaStreamBtn.interactable = isCampaignActive;

        RefreshStretchGoalsList();
    }

    private void RefreshStretchGoalsList()
    {
        if (stretchGoalsContainer == null) return;

        for (int i = 0; i < stretchGoals.Count; i++)
        {
            var sg = stretchGoals[i];
            Transform cardTr = stretchGoalsContainer.Find($"GoalCard_{i}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            TMP_Text statusTxt = cardTr.Find("Status/Text")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = sg.isUnlocked 
                    ? new Color(0.12f, 0.24f, 0.18f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.85f);
            }

            if (statusTxt != null)
            {
                statusTxt.text = sg.isUnlocked ? "✓ ДОСТИГНУТО" : $"{NumberFormatter.Format(sg.targetAmount)} ₽";
                statusTxt.color = sg.isUnlocked ? new Color(0.2f, 1f, 0.6f) : new Color(0.7f, 0.8f, 0.9f);
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("KickstarterCampaignModal", typeof(RectTransform));
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
        outline.effectColor = new Color(0.1f, 0.85f, 0.55f, 0.5f);
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
        headerTxt.text = "💡 КРАУДФАНДИНГ НА КИКСТАРТЕРЕ";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 1f, 0.65f);

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

        // Info Metrics Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -56);
        infoRt.sizeDelta = new Vector2(-36, 95);
        infoPanel.GetComponent<Image>().color = new Color(0.11f, 0.15f, 0.22f, 0.95f);

        GameObject fndObj = new GameObject("Funded", typeof(RectTransform), typeof(TextMeshProUGUI));
        fndObj.transform.SetParent(infoPanel.transform, false);
        RectTransform fndRt = fndObj.GetComponent<RectTransform>();
        fndRt.anchorMin = new Vector2(0, 1);
        fndRt.anchorMax = new Vector2(1, 1);
        fndRt.pivot = new Vector2(0.5f, 1);
        fndRt.anchoredPosition = new Vector2(12, -6);
        fndRt.sizeDelta = new Vector2(-24, 24);
        fundedAmountText = fndObj.GetComponent<TextMeshProUGUI>();
        fundedAmountText.fontSize = 14;
        fundedAmountText.fontStyle = FontStyles.Bold;

        GameObject bckObj = new GameObject("Backers", typeof(RectTransform), typeof(TextMeshProUGUI));
        bckObj.transform.SetParent(infoPanel.transform, false);
        RectTransform bckRt = bckObj.GetComponent<RectTransform>();
        bckRt.anchorMin = new Vector2(0, 1);
        bckRt.anchorMax = new Vector2(0.5f, 1);
        bckRt.anchoredPosition = new Vector2(12, -32);
        bckRt.sizeDelta = new Vector2(-20, 20);
        backersCountText = bckObj.GetComponent<TextMeshProUGUI>();
        backersCountText.fontSize = 12;

        GameObject dayObj = new GameObject("Days", typeof(RectTransform), typeof(TextMeshProUGUI));
        dayObj.transform.SetParent(infoPanel.transform, false);
        RectTransform dayRt = dayObj.GetComponent<RectTransform>();
        dayRt.anchorMin = new Vector2(0.5f, 1);
        dayRt.anchorMax = new Vector2(1, 1);
        dayRt.anchoredPosition = new Vector2(0, -32);
        dayRt.sizeDelta = new Vector2(-12, 20);
        daysRemainingText = dayObj.GetComponent<TextMeshProUGUI>();
        daysRemainingText.fontSize = 12;
        daysRemainingText.alignment = TextAlignmentOptions.Right;

        // Progress Bar
        GameObject barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(infoPanel.transform, false);
        RectTransform bbrt = barBg.GetComponent<RectTransform>();
        bbrt.anchorMin = new Vector2(0, 0);
        bbrt.anchorMax = new Vector2(1, 0);
        bbrt.pivot = new Vector2(0.5f, 0);
        bbrt.anchoredPosition = new Vector2(0, 8);
        bbrt.sizeDelta = new Vector2(-24, 14);
        barBg.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f);

        GameObject barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform bfrt = barFill.GetComponent<RectTransform>();
        bfrt.anchorMin = Vector2.zero;
        bfrt.anchorMax = Vector2.one;
        bfrt.sizeDelta = Vector2.zero;
        progressBarFill = barFill.GetComponent<Image>();
        progressBarFill.color = new Color(0.1f, 0.85f, 0.55f);
        progressBarFill.type = Image.Type.Filled;
        progressBarFill.fillMethod = Image.FillMethod.Horizontal;
        progressBarFill.fillAmount = 0.4f;

        // Action Buttons Row (Trailer, Concept, AMA)
        GameObject actionsRow = new GameObject("ActionsRow", typeof(RectTransform));
        actionsRow.transform.SetParent(cardObj.transform, false);
        RectTransform actRt = actionsRow.GetComponent<RectTransform>();
        actRt.anchorMin = new Vector2(0, 1);
        actRt.anchorMax = new Vector2(1, 1);
        actRt.pivot = new Vector2(0.5f, 1);
        actRt.anchoredPosition = new Vector2(0, -158);
        actRt.sizeDelta = new Vector2(-36, 44);

        actionTrailerBtn = CreateActionBtn(actionsRow.transform, "🎬 Трейлер (6K ₽)", 0, new Color(0.15f, 0.55f, 0.75f));
        actionConceptArtBtn = CreateActionBtn(actionsRow.transform, "🎨 Арты (3K ₽)", 1, new Color(0.2f, 0.65f, 0.5f));
        actionAmaStreamBtn = CreateActionBtn(actionsRow.transform, "🎙️ AMA (10K ₽)", 2, new Color(0.65f, 0.45f, 0.85f));

        // Scroll View with Stretch Goals
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 70);
        scrollRt.offsetMax = new Vector2(-18, -210);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        stretchGoalsContainer = contentObj.transform;
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

        for (int i = 0; i < stretchGoals.Count; i++)
        {
            CreateGoalRowTemplate(stretchGoalsContainer, stretchGoals[i], i);
        }

        // Bottom Start / Claim Buttons
        startCampaignBtn = CreateBigBottomButton(cardObj.transform, "🚀 ЗАПУСТИТЬ КАМПАНИЮ КИКСТАРТЕРА", new Color(0.12f, 0.65f, 0.4f));
        claimFundsBtn = CreateBigBottomButton(cardObj.transform, "💰 ЗАБРАТЬ СОБРАННЫЙ БЮДЖЕТ", new Color(0.85f, 0.65f, 0.15f));
        claimFundsBtn.gameObject.SetActive(false);

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 14);
        closeBtnRt.sizeDelta = new Vector2(-40, 40);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeKickstarterBtn = closeBtnObj.GetComponent<Button>();

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

    private Button CreateActionBtn(Transform parent, string title, int index, Color col)
    {
        GameObject b = new GameObject($"ActionBtn_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        float step = 1f / 3f;
        brt.anchorMin = new Vector2(index * step, 0);
        brt.anchorMax = new Vector2((index + 1) * step, 1);
        brt.offsetMin = new Vector2(2, 0);
        brt.offsetMax = new Vector2(-2, 0);
        b.GetComponent<Image>().color = col;

        GameObject t = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(b.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TMP_Text txt = t.GetComponent<TextMeshProUGUI>();
        txt.text = title;
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;

        return b.GetComponent<Button>();
    }

    private void CreateGoalRowTemplate(Transform parent, StretchGoal goal, int index)
    {
        GameObject row = new GameObject($"GoalCard_{index}", typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(440, 68);
        row.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(row.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(0.72f, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(0, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = goal.title;
        tt.fontSize = 12;
        tt.fontStyle = FontStyles.Bold;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(row.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.72f, 1);
        drt.pivot = new Vector2(0, 0);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -28);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = goal.perkDescription;
        dt.fontSize = 10;
        dt.color = new Color(0.75f, 0.85f, 0.95f);

        GameObject st = new GameObject("Status", typeof(RectTransform));
        st.transform.SetParent(row.transform, false);
        RectTransform strt = st.GetComponent<RectTransform>();
        strt.anchorMin = new Vector2(0.72f, 0);
        strt.anchorMax = new Vector2(1, 1);
        strt.offsetMin = Vector2.zero;
        strt.offsetMax = new Vector2(-10, 0);

        GameObject stt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        stt.transform.SetParent(st.transform, false);
        RectTransform sttrt = stt.GetComponent<RectTransform>();
        sttrt.anchorMin = Vector2.zero;
        sttrt.anchorMax = Vector2.one;
        sttrt.sizeDelta = Vector2.zero;
        TMP_Text stxt = stt.GetComponent<TextMeshProUGUI>();
        stxt.text = $"{NumberFormatter.Format(goal.targetAmount)} ₽";
        stxt.fontSize = 11;
        stxt.fontStyle = FontStyles.Bold;
        stxt.alignment = TextAlignmentOptions.Right;
    }

    private Button CreateBigBottomButton(Transform parent, string label, Color col)
    {
        GameObject b = new GameObject("BigActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 58);
        rt.sizeDelta = new Vector2(-40, 44);
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TMP_Text t = to.GetComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 13;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;

        return b.GetComponent<Button>();
    }
}
