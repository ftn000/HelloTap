using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Синдикат Инди-Разработчиков (Indie Dev Syndicate & Co-op Milestones):
/// - 3 профильных синдиката (Pixel Pioneers, Cyber Crafters, Indie Rebels)
/// - Общий репозиторий синдиката и совместный вклад строк кода
/// - 3 еженедельных майлстоуна (Бронзовый, Серебряный и Золотой Фонд)
/// - Синдикатные ранги (Junior, Contributor, Architect, Guildmaster)
/// - Перманентные командные множители к клику, скорости разработки и доходам
/// </summary>
public class IndieDevSyndicateUI : MonoBehaviour
{
    private static IndieDevSyndicateUI instance;
    public static IndieDevSyndicateUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<IndieDevSyndicateUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(IndieDevSyndicateUI));
                    instance = go.AddComponent<IndieDevSyndicateUI>();
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
    public class SyndicateFaction
    {
        public string id;
        public string name;
        public string icon;
        public string motto;
        public string perkDesc;
        public double bonusMultiplier;
        public Color themeColor;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openSyndicateBtn;

    [Header("Прогресс синдиката и майлстоуны")]
    [SerializeField] private TMP_Text currentFactionTitleTxt;
    [SerializeField] private TMP_Text memberRankTxt;
    [SerializeField] private TMP_Text totalContributionTxt;
    [SerializeField] private Image contributionProgressFill;
    [SerializeField] private Button contributeCodeBtn;
    [SerializeField] private TMP_Text contributeCodeBtnTxt;
    [SerializeField] private Transform factionsContainer;

    private readonly List<SyndicateFaction> factions = new List<SyndicateFaction>();
    private int selectedFactionIndex = 0;
    private double totalContributedCode = 0;
    private int claimedMilestoneTier = 0; // 0, 1, 2, 3

    private const string PrefFaction = "Syndicate_FactionIdx";
    private const string PrefContribution = "Syndicate_TotalContributed";
    private const string PrefClaimedMilestone = "Syndicate_ClaimedTier";

    private static readonly double[] MilestoneGoals = new double[] { 5000, 25000, 100000 };
    private static readonly double[] MilestoneRewardMoney = new double[] { 25000, 80000, 300000 };
    private static readonly double[] MilestoneRewardCode = new double[] { 1000, 4000, 15000 };

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int SelectedFactionIndex => selectedFactionIndex;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeFactions();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeFactions()
    {
        factions.Clear();

        factions.Add(new SyndicateFaction
        {
            id = "pixel_pioneers",
            name = "Pixel Pioneers Guild",
            icon = "👾",
            motto = "Душевность, пиксель-арт и чистота ретро-геймплея",
            perkDesc = "+25% к силе клика и критическим нажатиям",
            bonusMultiplier = 0.25,
            themeColor = new Color(0.9f, 0.4f, 1f)
        });

        factions.Add(new SyndicateFaction
        {
            id = "cyber_crafters",
            name = "Cyber Crafters Syndicate",
            icon = "⚙️",
            motto = "Собственные движки, нулевой оверхед и сетевой код",
            perkDesc = "+25% к скорости релизов и пассивному коду",
            bonusMultiplier = 0.25,
            themeColor = new Color(0.1f, 0.85f, 1f)
        });

        factions.Add(new SyndicateFaction
        {
            id = "indie_rebels",
            name = "Indie Rebels Alliance",
            icon = "🔥",
            motto = "Анти-корпораты, честная монетизация и свобода кода",
            perkDesc = "+30% ко всем выплатам от релизов студии",
            bonusMultiplier = 0.30,
            themeColor = new Color(1f, 0.5f, 0.1f)
        });
    }

    private void LoadData()
    {
        selectedFactionIndex = PlayerPrefs.GetInt(PrefFaction, 0);
        selectedFactionIndex = Mathf.Clamp(selectedFactionIndex, 0, factions.Count - 1);

        double.TryParse(PlayerPrefs.GetString(PrefContribution, "0"), out totalContributedCode);
        claimedMilestoneTier = PlayerPrefs.GetInt(PrefClaimedMilestone, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefFaction, selectedFactionIndex);
        PlayerPrefs.SetString(PrefContribution, totalContributedCode.ToString("F0"));
        PlayerPrefs.SetInt(PrefClaimedMilestone, claimedMilestoneTier);
        PlayerPrefs.Save();
    }

    public double GetSyndicateMultiplier()
    {
        var f = factions[Mathf.Clamp(selectedFactionIndex, 0, factions.Count - 1)];
        double milestoneBonus = claimedMilestoneTier * 0.08;
        return 1.0 + f.bonusMultiplier + milestoneBonus;
    }

    public double GetSyndicateClickMultiplier()
    {
        if (selectedFactionIndex == 0) return 1.25; // Pixel Pioneers
        return 1.0;
    }

    public string GetMemberRankName()
    {
        if (totalContributedCode >= 100000) return "👑 Синдикат-Магистр (Guildmaster)";
        if (totalContributedCode >= 25000) return "🚀 Ведущий Архитектор (Lead Architect)";
        if (totalContributedCode >= 5000) return "💻 Основной Контрибьютор (Core Contributor)";
        return "🌱 Младший Разработчик (Junior Member)";
    }

    public void JoinFaction(int factionIdx)
    {
        if (factionIdx < 0 || factionIdx >= factions.Count) return;
        if (selectedFactionIndex == factionIdx) return;

        selectedFactionIndex = factionIdx;
        SaveData();

        var f = factions[selectedFactionIndex];
        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🌐 ВЫ ВСТУПИЛИ В СИНДИКАТ:\n{f.icon} {f.name}\n<color=#00FF88>{f.perkDesc}</color>", transform.position, f.themeColor, true);
        }

        UpdateModalUI();
    }

    public void ContributeCodeLines()
    {
        if (GameManager.Instance == null) return;

        double contributionAmount = Math.Max(100.0, Math.Min(GameManager.Instance.CodeLines * 0.25, 25000.0));
        if (GameManager.Instance.CodeLines < contributionAmount || contributionAmount <= 0)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("❌ Недостаточно строк кода для взноса в синдикат!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        GameManager.Instance.SpendLinesOfCode(contributionAmount);
        totalContributedCode += contributionAmount;

        CheckMilestones();
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            var f = factions[selectedFactionIndex];
            ClickJuice.Instance.SpawnCustomPopup($"💾 ВЗНОС В СИНДИКАТ ПРИНЯТ!\n+{NumberFormatter.Format(contributionAmount)} строк кода в репозиторий", transform.position, f.themeColor, false);
        }

        UpdateModalUI();
    }

    private void CheckMilestones()
    {
        for (int i = 0; i < MilestoneGoals.Length; i++)
        {
            if (i >= claimedMilestoneTier && totalContributedCode >= MilestoneGoals[i])
            {
                claimedMilestoneTier = i + 1;
                double rewardM = MilestoneRewardMoney[i];
                double rewardC = MilestoneRewardCode[i];

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddMoney(rewardM);
                    GameManager.Instance.AddLinesOfCode(rewardC);
                }

                HapticFeedback.SuccessPattern();
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"🎉 ЗАКРЫТ МАЙЛСТОУН СИНДИКАТА #{claimedMilestoneTier}!\nНаграда: <b>+{NumberFormatter.Format(rewardM)} ₽</b> и +{NumberFormatter.Format(rewardC)} кода!", transform.position, new Color(1f, 0.85f, 0.2f), true);
                }
            }
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

    private void BindButtons()
    {
        if (openSyndicateBtn != null)
        {
            openSyndicateBtn.onClick.RemoveAllListeners();
            openSyndicateBtn.onClick.AddListener(OpenModal);
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
        if (contributeCodeBtn != null)
        {
            contributeCodeBtn.onClick.RemoveAllListeners();
            contributeCodeBtn.onClick.AddListener(ContributeCodeLines);
        }
    }

    private void UpdateModalUI()
    {
        var f = factions[Mathf.Clamp(selectedFactionIndex, 0, factions.Count - 1)];

        if (currentFactionTitleTxt != null)
        {
            currentFactionTitleTxt.text = $"🌐 ВАШ СИНДИКАТ: <color=#{ColorUtility.ToHtmlStringRGB(f.themeColor)}>{f.name}</color>";
        }

        if (memberRankTxt != null)
        {
            memberRankTxt.text = $"Ранг в гильдии: <b><color=#00E5FF>{GetMemberRankName()}</color></b>\n<size=10><color=#90B0D0>{f.motto}</color></size>";
        }

        double nextGoal = MilestoneGoals[Mathf.Clamp(claimedMilestoneTier, 0, MilestoneGoals.Length - 1)];
        if (totalContributionTxt != null)
        {
            totalContributionTxt.text = $"Вклад в репозиторий: <b><color=#00FF88>{NumberFormatter.Format(totalContributedCode)}</color></b> / {NumberFormatter.Format(nextGoal)} строк (Ур. {claimedMilestoneTier}/3)";
        }

        if (contributionProgressFill != null && nextGoal > 0)
        {
            contributionProgressFill.fillAmount = Mathf.Clamp01((float)(totalContributedCode / nextGoal));
        }

        if (contributeCodeBtn != null && contributeCodeBtnTxt != null)
        {
            double available = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;
            double share = Math.Max(100.0, Math.Min(available * 0.25, 25000.0));
            contributeCodeBtn.interactable = available >= 100;
            contributeCodeBtnTxt.text = $"💾 ВНЕСТИ ВКЛАД ({NumberFormatter.Format(share)} СТРОК КОДА)";
        }

        RefreshFactionCards();
    }

    private void RefreshFactionCards()
    {
        if (factionsContainer == null) return;

        for (int i = 0; i < factions.Count; i++)
        {
            int idx = i;
            var f = factions[i];
            Transform cardTr = factionsContainer.Find($"FactionCard_{f.id}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actionBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = actionBtn != null ? actionBtn.GetComponentInChildren<TMP_Text>() : null;

            bool isCurrent = selectedFactionIndex == idx;

            if (bg != null)
            {
                bg.color = isCurrent 
                    ? new Color(0.12f, 0.20f, 0.28f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.14f, 0.92f);
            }

            if (actionBtn != null && btnTxt != null)
            {
                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() => JoinFaction(idx));

                if (isCurrent)
                {
                    actionBtn.interactable = false;
                    btnTxt.text = "⭐ АКТИВНЫЙ СИНДИКАТ";
                }
                else
                {
                    actionBtn.interactable = true;
                    btnTxt.text = "ВСТУПИТЬ";
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
        GameObject root = new GameObject("IndieSyndicateModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.14f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.8f, 1f, 0.6f);
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
        headerTxt.text = "🌐 СИНДИКАТ ИНДИ-РАЗРАБОТЧИКОВ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.2f, 0.9f, 1f);

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

        // Faction Info Box
        GameObject infoBox = new GameObject("InfoBox", typeof(RectTransform), typeof(Image));
        infoBox.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoBox.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -52);
        infoRt.sizeDelta = new Vector2(-36, 100);
        infoBox.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.22f, 0.9f);

        GameObject curTitleObj = new GameObject("CurrentTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        curTitleObj.transform.SetParent(infoBox.transform, false);
        RectTransform curTitleRt = curTitleObj.GetComponent<RectTransform>();
        curTitleRt.anchorMin = new Vector2(0, 1);
        curTitleRt.anchorMax = new Vector2(1, 1);
        curTitleRt.pivot = new Vector2(0.5f, 1);
        curTitleRt.anchoredPosition = new Vector2(0, -6);
        curTitleRt.sizeDelta = new Vector2(-16, 20);
        currentFactionTitleTxt = curTitleObj.GetComponent<TextMeshProUGUI>();
        currentFactionTitleTxt.fontSize = 12;
        currentFactionTitleTxt.fontStyle = FontStyles.Bold;
        currentFactionTitleTxt.alignment = TextAlignmentOptions.Center;
        currentFactionTitleTxt.color = Color.white;

        GameObject rankObj = new GameObject("Rank", typeof(RectTransform), typeof(TextMeshProUGUI));
        rankObj.transform.SetParent(infoBox.transform, false);
        RectTransform rankRt = rankObj.GetComponent<RectTransform>();
        rankRt.anchorMin = new Vector2(0, 1);
        rankRt.anchorMax = new Vector2(1, 1);
        rankRt.pivot = new Vector2(0.5f, 1);
        rankRt.anchoredPosition = new Vector2(0, -28);
        rankRt.sizeDelta = new Vector2(-16, 32);
        memberRankTxt = rankObj.GetComponent<TextMeshProUGUI>();
        memberRankTxt.fontSize = 10;
        memberRankTxt.alignment = TextAlignmentOptions.Center;
        memberRankTxt.color = new Color(0.85f, 0.95f, 1f);

        GameObject contribObj = new GameObject("ContributionTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        contribObj.transform.SetParent(infoBox.transform, false);
        RectTransform contribRt = contribObj.GetComponent<RectTransform>();
        contribRt.anchorMin = new Vector2(0, 0);
        contribRt.anchorMax = new Vector2(1, 0);
        contribRt.pivot = new Vector2(0.5f, 0);
        contribRt.anchoredPosition = new Vector2(0, 8);
        contribRt.sizeDelta = new Vector2(-16, 20);
        totalContributionTxt = contribObj.GetComponent<TextMeshProUGUI>();
        totalContributionTxt.fontSize = 10;
        totalContributionTxt.alignment = TextAlignmentOptions.Center;
        totalContributionTxt.color = new Color(0.9f, 0.95f, 1f);

        // Contribute Code Button
        GameObject contBtnObj = new GameObject("ContributeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        contBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform contRt = contBtnObj.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0, 1);
        contRt.anchorMax = new Vector2(1, 1);
        contRt.pivot = new Vector2(0.5f, 1);
        contRt.anchoredPosition = new Vector2(0, -160);
        contRt.sizeDelta = new Vector2(-36, 40);
        contBtnObj.GetComponent<Image>().color = new Color(0.1f, 0.55f, 0.75f, 0.95f);
        contributeCodeBtn = contBtnObj.GetComponent<Button>();

        GameObject contTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        contTxtObj.transform.SetParent(contBtnObj.transform, false);
        RectTransform contTxtRt = contTxtObj.GetComponent<RectTransform>();
        contTxtRt.anchorMin = Vector2.zero; contTxtRt.anchorMax = Vector2.one; contTxtRt.sizeDelta = Vector2.zero;
        contributeCodeBtnTxt = contTxtObj.GetComponent<TextMeshProUGUI>();
        contributeCodeBtnTxt.fontSize = 11;
        contributeCodeBtnTxt.fontStyle = FontStyles.Bold;
        contributeCodeBtnTxt.alignment = TextAlignmentOptions.Center;
        contributeCodeBtnTxt.text = "💾 ВНЕСТИ ВКЛАД В СИНДИКАТ";
        contributeCodeBtnTxt.color = Color.white;

        // Scroll Container for Factions
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -210);
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
        factionsContainer = contentObj.transform;

        for (int i = 0; i < factions.Count; i++)
        {
            CreateFactionCardTemplate(factionsContainer, factions[i]);
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

    private void CreateFactionCardTemplate(Transform parent, SyndicateFaction faction)
    {
        GameObject card = new GameObject($"FactionCard_{faction.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 80);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

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
        tt.text = $"{faction.icon} <b>{faction.name}</b>";
        tt.fontSize = 12;
        tt.color = faction.themeColor;

        // Perk text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.65f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Бонус: <color=#00FF88>{faction.perkDesc}</color>";
        dt.fontSize = 10;

        // Action button
        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, 0);
        brt.sizeDelta = new Vector2(150, 36);
        b.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.55f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ВСТУПИТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openSyndicateBtn != null) return;

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

        GameObject btnGo = new GameObject("SyndicateHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.15f, 0.35f, 0.5f, 0.9f);
        openSyndicateBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🌐 Синдикат";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
