using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Сезонный боевой пропуск разработчика (Indie Season Pass):
/// - Сезон 1: "Neon Indie Revolution"
/// - 15 уровней прогрессии с двумя дорожками наград (Free и Gold Pass)
/// - Накопление сезонного опыта (Season XP) за клики, релизы проектов, квесты и хакатоны
/// - Эксклюзивные награды: крупные денежные чеки, пакеты кода, бусты и перманентный титул
/// </summary>
public class IndieSeasonPassUI : MonoBehaviour
{
    private static IndieSeasonPassUI instance;
    public static IndieSeasonPassUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<IndieSeasonPassUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class SeasonTier
    {
        public int level;
        public int xpRequired;
        public string freeRewardTitle;
        public double freeMoney;
        public double freeCode;
        public string goldRewardTitle;
        public double goldMoney;
        public double goldCode;
        public float boostSeconds;
        public bool isFreeClaimed;
        public bool isGoldClaimed;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openPassBtn;
    [SerializeField] private TMP_Text openPassBtnText;
    [SerializeField] private Button closePassBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Прогресс сезона")]
    [SerializeField] private TMP_Text seasonTitleText;
    [SerializeField] private TMP_Text seasonLevelText;
    [SerializeField] private TMP_Text seasonXpText;
    [SerializeField] private Image seasonXpBarFill;
    [SerializeField] private Button unlockGoldPassBtn;
    [SerializeField] private TMP_Text unlockGoldPassBtnText;

    [Header("Контейнер уровней")]
    [SerializeField] private Transform tiersContainer;

    private readonly List<SeasonTier> tiers = new List<SeasonTier>();
    private int currentXp = 0;
    private int currentLevel = 1;
    private bool hasGoldPass = false;

    private const int XpPerLevel = 250;
    private const double GoldPassCostRub = 150000.0;

    private const string PrefSeasonXp = "Studio_SeasonPass_XP";
    private const string PrefHasGold = "Studio_SeasonPass_HasGold";
    private const string PrefClaimedFree = "Studio_SeasonPass_ClaimedFree_";
    private const string PrefClaimedGold = "Studio_SeasonPass_ClaimedGold_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CurrentLevel => currentLevel;
    public bool HasGoldPass => hasGoldPass;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTiers();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        SubscribeEvents();
        CalculateLevel();
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
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
            GameManager.Instance.OnPrestigeCompleted += HandlePrestigeCompleted;
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
            GameManager.Instance.OnPrestigeCompleted -= HandlePrestigeCompleted;
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // 1 XP за каждые 40 строк кода
        int xpGain = Math.Max(1, (int)(amount / 40.0));
        AddSeasonXp(xpGain, false);
    }

    private void HandleProjectCompleted(GameProjectData proj)
    {
        AddSeasonXp(180, true);
    }

    private void HandlePrestigeCompleted(int prestigeLvl)
    {
        AddSeasonXp(400, true);
    }

    private void InitializeTiers()
    {
        tiers.Clear();

        for (int i = 1; i <= 15; i++)
        {
            double freeM = i * 4000.0;
            double freeC = i * 2500.0;
            double goldM = i * 15000.0;
            double goldC = i * 8000.0;

            string freeTitle = $"+{NumberFormatter.Format(freeM)} ₽";
            string goldTitle = i == 15 ? "👑 ТИТУЛ 'NEON LEGEND' & +250K ₽" : $"+{NumberFormatter.Format(goldM)} ₽ & ⚡ Буст";

            tiers.Add(new SeasonTier
            {
                level = i,
                xpRequired = i * XpPerLevel,
                freeRewardTitle = freeTitle,
                freeMoney = freeM,
                freeCode = freeC,
                goldRewardTitle = goldTitle,
                goldMoney = goldM,
                goldCode = goldC,
                boostSeconds = i % 3 == 0 ? 60f : 0f,
                isFreeClaimed = false,
                isGoldClaimed = false
            });
        }
    }

    private void LoadData()
    {
        currentXp = PlayerPrefs.GetInt(PrefSeasonXp, 45);
        hasGoldPass = PlayerPrefs.GetInt(PrefHasGold, 0) == 1;

        for (int i = 0; i < tiers.Count; i++)
        {
            tiers[i].isFreeClaimed = PlayerPrefs.GetInt(PrefClaimedFree + tiers[i].level, 0) == 1;
            tiers[i].isGoldClaimed = PlayerPrefs.GetInt(PrefClaimedGold + tiers[i].level, 0) == 1;
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefSeasonXp, currentXp);
        PlayerPrefs.SetInt(PrefHasGold, hasGoldPass ? 1 : 0);

        for (int i = 0; i < tiers.Count; i++)
        {
            PlayerPrefs.SetInt(PrefClaimedFree + tiers[i].level, tiers[i].isFreeClaimed ? 1 : 0);
            PlayerPrefs.SetInt(PrefClaimedGold + tiers[i].level, tiers[i].isGoldClaimed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public void AddSeasonXp(int amount, bool notifyPopup)
    {
        int prevLvl = currentLevel;
        currentXp += amount;
        CalculateLevel();
        SaveData();

        if (currentLevel > prevLvl)
        {
            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎖️ НОВЫЙ УРОВЕНЬ БОЕВОГО ПРОПУСКА!\nУровень {currentLevel}! Заберите награды!", transform.position, new Color(1f, 0.85f, 0.2f), true);
            }
        }
        else if (notifyPopup && ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"+{amount} Сезонного Опыта (XP)!", transform.position, new Color(0.2f, 0.8f, 1f), false);
        }

        if (IsModalOpen) UpdateModalUI();
    }

    private void CalculateLevel()
    {
        currentLevel = 1 + (currentXp / XpPerLevel);
        currentLevel = Mathf.Clamp(currentLevel, 1, tiers.Count);
    }

    public void UnlockGoldPass()
    {
        if (hasGoldPass) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < GoldPassCostRub)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств ({NumberFormatter.Format(GoldPassCostRub)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(GoldPassCostRub);
        hasGoldPass = true;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("👑 GOLD PASS АКТИВИРОВАН!\nВсе премиальные награды разблокированы!", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        UpdateModalUI();
    }

    public void ClaimReward(int level, bool isGold)
    {
        if (level < 1 || level > tiers.Count) return;
        var t = tiers[level - 1];

        if (currentLevel < level) return;

        if (!isGold)
        {
            if (t.isFreeClaimed) return;
            t.isFreeClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(t.freeMoney);
                GameManager.Instance.AddLinesOfCode(t.freeCode);
            }
        }
        else
        {
            if (!hasGoldPass || t.isGoldClaimed) return;
            t.isGoldClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(t.goldMoney);
                GameManager.Instance.AddLinesOfCode(t.goldCode);
                if (t.boostSeconds > 0) GameManager.Instance.ActivateEnergyBoost(t.boostSeconds, 2.0);
            }
        }

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            string txt = isGold ? $"👑 ЗАБРАНА GOLD НАГРАДА: {t.goldRewardTitle}" : $"🎁 ЗАБРАНА FREE НАГРАДА: {t.freeRewardTitle}";
            ClickJuice.Instance.SpawnCustomPopup(txt, transform.position, isGold ? new Color(1f, 0.84f, 0.2f) : new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openPassBtn != null)
        {
            openPassBtn.onClick.RemoveAllListeners();
            openPassBtn.onClick.AddListener(OpenModal);
        }
        if (closePassBtn != null)
        {
            closePassBtn.onClick.RemoveAllListeners();
            closePassBtn.onClick.AddListener(CloseModal);
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

        if (unlockGoldPassBtn != null)
        {
            unlockGoldPassBtn.onClick.RemoveAllListeners();
            unlockGoldPassBtn.onClick.AddListener(UnlockGoldPass);
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
        if (seasonLevelText != null)
        {
            seasonLevelText.text = $"УРОВЕНЬ: <b>{currentLevel}/{tiers.Count}</b>";
        }

        int xpInCurrentLevel = currentXp % XpPerLevel;
        if (seasonXpText != null)
        {
            seasonXpText.text = $"{xpInCurrentLevel} / {XpPerLevel} XP";
        }

        if (seasonXpBarFill != null)
        {
            seasonXpBarFill.fillAmount = (float)xpInCurrentLevel / XpPerLevel;
        }

        if (unlockGoldPassBtn != null)
        {
            unlockGoldPassBtn.interactable = !hasGoldPass;
            var img = unlockGoldPassBtn.GetComponent<Image>();
            if (img != null) img.color = hasGoldPass ? new Color(0.2f, 0.25f, 0.3f) : new Color(0.85f, 0.65f, 0.15f);
        }

        if (unlockGoldPassBtnText != null)
        {
            unlockGoldPassBtnText.text = hasGoldPass ? "👑 GOLD PASS АКТИВЕН" : $"👑 КУПИТЬ GOLD PASS ({NumberFormatter.Format(GoldPassCostRub)} ₽)";
        }

        RefreshTiersList();
    }

    private void RefreshTiersList()
    {
        if (tiersContainer == null) return;

        for (int i = 0; i < tiers.Count; i++)
        {
            var t = tiers[i];
            int lvl = t.level;
            Transform cardTr = tiersContainer.Find($"PassTier_{lvl}");
            if (cardTr == null) continue;

            // Free Claim Btn
            Button freeBtn = cardTr.Find("FreeClaimBtn")?.GetComponent<Button>();
            TMP_Text freeTxt = freeBtn != null ? freeBtn.GetComponentInChildren<TMP_Text>() : null;
            if (freeBtn != null && freeTxt != null)
            {
                freeBtn.onClick.RemoveAllListeners();
                freeBtn.onClick.AddListener(() => ClaimReward(lvl, false));

                bool unlocked = currentLevel >= lvl;
                if (t.isFreeClaimed)
                {
                    freeBtn.interactable = false;
                    freeTxt.text = "✓ ПОЛУЧЕНО";
                }
                else if (unlocked)
                {
                    freeBtn.interactable = true;
                    freeTxt.text = "ЗАБРАТЬ";
                }
                else
                {
                    freeBtn.interactable = false;
                    freeTxt.text = $"Ур. {lvl}";
                }
            }

            // Gold Claim Btn
            Button goldBtn = cardTr.Find("GoldClaimBtn")?.GetComponent<Button>();
            TMP_Text goldTxt = goldBtn != null ? goldBtn.GetComponentInChildren<TMP_Text>() : null;
            if (goldBtn != null && goldTxt != null)
            {
                goldBtn.onClick.RemoveAllListeners();
                goldBtn.onClick.AddListener(() => ClaimReward(lvl, true));

                bool unlocked = currentLevel >= lvl && hasGoldPass;
                if (t.isGoldClaimed)
                {
                    goldBtn.interactable = false;
                    goldTxt.text = "✓ ПОЛУЧЕНО";
                }
                else if (unlocked)
                {
                    goldBtn.interactable = true;
                    goldTxt.text = "ЗАБРАТЬ";
                }
                else
                {
                    goldBtn.interactable = false;
                    goldTxt.text = hasGoldPass ? $"Ур. {lvl}" : "👑 GOLD";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("IndieSeasonPassModal", typeof(RectTransform));
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
        cardRt.sizeDelta = new Vector2(500, 700);
        cardObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.85f, 0.55f, 1f, 0.5f);
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
        headerTxt.text = "⚡ СЕЗОННЫЙ БОЕВОЙ ПРОПУСК #1";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.9f, 0.6f, 1f);

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

        // Progress Panel
        GameObject progPanel = new GameObject("ProgressPanel", typeof(RectTransform), typeof(Image));
        progPanel.transform.SetParent(cardObj.transform, false);
        RectTransform prgRt = progPanel.GetComponent<RectTransform>();
        prgRt.anchorMin = new Vector2(0, 1);
        prgRt.anchorMax = new Vector2(1, 1);
        prgRt.pivot = new Vector2(0.5f, 1);
        prgRt.anchoredPosition = new Vector2(0, -56);
        prgRt.sizeDelta = new Vector2(-36, 75);
        progPanel.GetComponent<Image>().color = new Color(0.11f, 0.14f, 0.22f, 0.95f);

        GameObject lvlObj = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlObj.transform.SetParent(progPanel.transform, false);
        RectTransform lvlRt = lvlObj.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0, 1);
        lvlRt.anchorMax = new Vector2(0.5f, 1);
        lvlRt.offsetMin = new Vector2(12, -26);
        lvlRt.offsetMax = new Vector2(0, -6);
        seasonLevelText = lvlObj.GetComponent<TextMeshProUGUI>();
        seasonLevelText.fontSize = 13;
        seasonLevelText.fontStyle = FontStyles.Bold;

        GameObject xpObj = new GameObject("XP", typeof(RectTransform), typeof(TextMeshProUGUI));
        xpObj.transform.SetParent(progPanel.transform, false);
        RectTransform xpRt = xpObj.GetComponent<RectTransform>();
        xpRt.anchorMin = new Vector2(0.5f, 1);
        xpRt.anchorMax = new Vector2(1, 1);
        xpRt.offsetMin = new Vector2(0, -26);
        xpRt.offsetMax = new Vector2(-12, -6);
        seasonXpText = xpObj.GetComponent<TextMeshProUGUI>();
        seasonXpText.fontSize = 12;
        seasonXpText.alignment = TextAlignmentOptions.Right;
        seasonXpText.color = new Color(0.7f, 0.85f, 1f);

        // Bar BG
        GameObject barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
        barBg.transform.SetParent(progPanel.transform, false);
        RectTransform bbrt = barBg.GetComponent<RectTransform>();
        bbrt.anchorMin = new Vector2(0, 0);
        bbrt.anchorMax = new Vector2(1, 0);
        bbrt.pivot = new Vector2(0.5f, 0);
        bbrt.anchoredPosition = new Vector2(0, 10);
        bbrt.sizeDelta = new Vector2(-24, 14);
        barBg.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f);

        // Bar Fill
        GameObject barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        RectTransform bfrt = barFill.GetComponent<RectTransform>();
        bfrt.anchorMin = Vector2.zero;
        bfrt.anchorMax = Vector2.one;
        bfrt.sizeDelta = Vector2.zero;
        seasonXpBarFill = barFill.GetComponent<Image>();
        seasonXpBarFill.color = new Color(0.8f, 0.4f, 1f);
        seasonXpBarFill.type = Image.Type.Filled;
        seasonXpBarFill.fillMethod = Image.FillMethod.Horizontal;
        seasonXpBarFill.fillAmount = 0.35f;

        // Unlock Gold Pass Button
        GameObject goldBtnObj = new GameObject("UnlockGoldBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        goldBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform goldBtnRt = goldBtnObj.GetComponent<RectTransform>();
        goldBtnRt.anchorMin = new Vector2(0, 1);
        goldBtnRt.anchorMax = new Vector2(1, 1);
        goldBtnRt.pivot = new Vector2(0.5f, 1);
        goldBtnRt.anchoredPosition = new Vector2(0, -138);
        goldBtnRt.sizeDelta = new Vector2(-36, 40);
        goldBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f);
        unlockGoldPassBtn = goldBtnObj.GetComponent<Button>();

        GameObject goldTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        goldTxtObj.transform.SetParent(goldBtnObj.transform, false);
        RectTransform goldTxtRt = goldTxtObj.GetComponent<RectTransform>();
        goldTxtRt.anchorMin = Vector2.zero;
        goldTxtRt.anchorMax = Vector2.one;
        goldTxtRt.sizeDelta = Vector2.zero;
        unlockGoldPassBtnText = goldTxtObj.GetComponent<TextMeshProUGUI>();
        unlockGoldPassBtnText.text = "👑 КУПИТЬ GOLD PASS (150 000 ₽)";
        unlockGoldPassBtnText.fontSize = 12;
        unlockGoldPassBtnText.fontStyle = FontStyles.Bold;
        unlockGoldPassBtnText.alignment = TextAlignmentOptions.Center;
        unlockGoldPassBtnText.color = Color.white;

        // Scroll View with Tiers
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 68);
        scrollRt.offsetMax = new Vector2(-18, -188);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        tiersContainer = contentObj.transform;
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

        for (int i = 0; i < tiers.Count; i++)
        {
            CreateTierRowTemplate(tiersContainer, tiers[i]);
        }

        // Close Button
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 14);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closePassBtn = closeBtnObj.GetComponent<Button>();

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

    private void CreateTierRowTemplate(Transform parent, SeasonTier tier)
    {
        GameObject row = new GameObject($"PassTier_{tier.level}", typeof(RectTransform), typeof(Image));
        row.transform.SetParent(parent, false);
        RectTransform rowRt = row.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(440, 68);
        row.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.92f);

        // Level Badge
        GameObject lvlBadge = new GameObject("LvlBadge", typeof(RectTransform), typeof(Image));
        lvlBadge.transform.SetParent(row.transform, false);
        RectTransform lbrt = lvlBadge.GetComponent<RectTransform>();
        lbrt.anchorMin = new Vector2(0, 0.5f);
        lbrt.anchorMax = new Vector2(0, 0.5f);
        lbrt.anchoredPosition = new Vector2(25, 0);
        lbrt.sizeDelta = new Vector2(36, 36);
        lvlBadge.GetComponent<Image>().color = new Color(0.18f, 0.12f, 0.28f);

        GameObject ltObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        ltObj.transform.SetParent(lvlBadge.transform, false);
        RectTransform ltrt = ltObj.GetComponent<RectTransform>();
        ltrt.anchorMin = Vector2.zero;
        ltrt.anchorMax = Vector2.one;
        ltrt.sizeDelta = Vector2.zero;
        TMP_Text lt = ltObj.GetComponent<TextMeshProUGUI>();
        lt.text = tier.level.ToString();
        lt.fontSize = 14;
        lt.fontStyle = FontStyles.Bold;
        lt.alignment = TextAlignmentOptions.Center;
        lt.color = new Color(0.85f, 0.55f, 1f);

        // Free Reward Box
        GameObject freeBox = new GameObject("FreeBox", typeof(RectTransform));
        freeBox.transform.SetParent(row.transform, false);
        RectTransform fbrt = freeBox.GetComponent<RectTransform>();
        fbrt.anchorMin = new Vector2(0, 0);
        fbrt.anchorMax = new Vector2(0.55f, 1);
        fbrt.offsetMin = new Vector2(50, 6);
        fbrt.offsetMax = new Vector2(-6, -6);

        GameObject ftObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        ftObj.transform.SetParent(freeBox.transform, false);
        RectTransform ftrt = ftObj.GetComponent<RectTransform>();
        ftrt.anchorMin = new Vector2(0, 0.5f);
        ftrt.anchorMax = new Vector2(0.65f, 1);
        ftrt.offsetMin = Vector2.zero;
        ftrt.offsetMax = Vector2.zero;
        TMP_Text ft = ftObj.GetComponent<TextMeshProUGUI>();
        ft.text = $"FREE: {tier.freeRewardTitle}";
        ft.fontSize = 10;
        ft.color = new Color(0.7f, 0.85f, 1f);

        GameObject fBtnObj = new GameObject("FreeClaimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        fBtnObj.transform.SetParent(freeBox.transform, false);
        RectTransform fbtnrt = fBtnObj.GetComponent<RectTransform>();
        fbtnrt.anchorMin = new Vector2(0.68f, 0.15f);
        fbtnrt.anchorMax = new Vector2(1, 0.85f);
        fbtnrt.offsetMin = Vector2.zero;
        fbtnrt.offsetMax = Vector2.zero;
        fBtnObj.GetComponent<Image>().color = new Color(0.12f, 0.55f, 0.75f);

        GameObject fbtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        fbtnTxtObj.transform.SetParent(fBtnObj.transform, false);
        RectTransform ftxrt = fbtnTxtObj.GetComponent<RectTransform>();
        ftxrt.anchorMin = Vector2.zero;
        ftxrt.anchorMax = Vector2.one;
        ftxrt.sizeDelta = Vector2.zero;
        TMP_Text fbtnt = fbtnTxtObj.GetComponent<TextMeshProUGUI>();
        fbtnt.text = "ЗАБРАТЬ";
        fbtnt.fontSize = 9;
        fbtnt.fontStyle = FontStyles.Bold;
        fbtnt.alignment = TextAlignmentOptions.Center;
        fbtnt.color = Color.white;

        // Gold Reward Box
        GameObject goldBox = new GameObject("GoldBox", typeof(RectTransform));
        goldBox.transform.SetParent(row.transform, false);
        RectTransform gbrt = goldBox.GetComponent<RectTransform>();
        gbrt.anchorMin = new Vector2(0.55f, 0);
        gbrt.anchorMax = new Vector2(1, 1);
        gbrt.offsetMin = new Vector2(6, 6);
        gbrt.offsetMax = new Vector2(-8, -6);

        GameObject gtObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        gtObj.transform.SetParent(goldBox.transform, false);
        RectTransform gtrt = gtObj.GetComponent<RectTransform>();
        gtrt.anchorMin = new Vector2(0, 0.5f);
        gtrt.anchorMax = new Vector2(0.65f, 1);
        gtrt.offsetMin = Vector2.zero;
        gtrt.offsetMax = Vector2.zero;
        TMP_Text gt = gtObj.GetComponent<TextMeshProUGUI>();
        gt.text = $"GOLD: {tier.goldRewardTitle}";
        gt.fontSize = 10;
        gt.color = new Color(1f, 0.84f, 0.2f);

        GameObject gBtnObj = new GameObject("GoldClaimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        gBtnObj.transform.SetParent(goldBox.transform, false);
        RectTransform gbtnrt = gBtnObj.GetComponent<RectTransform>();
        gbtnrt.anchorMin = new Vector2(0.68f, 0.15f);
        gbtnrt.anchorMax = new Vector2(1, 0.85f);
        gbtnrt.offsetMin = Vector2.zero;
        gbtnrt.offsetMax = Vector2.zero;
        gBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f);

        GameObject gbtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        gbtnTxtObj.transform.SetParent(gBtnObj.transform, false);
        RectTransform gtxrt = gbtnTxtObj.GetComponent<RectTransform>();
        gtxrt.anchorMin = Vector2.zero;
        gtxrt.anchorMax = Vector2.one;
        gtxrt.sizeDelta = Vector2.zero;
        TMP_Text gbtnt = gbtnTxtObj.GetComponent<TextMeshProUGUI>();
        gbtnt.text = "ЗАБРАТЬ";
        gbtnt.fontSize = 9;
        gbtnt.fontStyle = FontStyles.Bold;
        gbtnt.alignment = TextAlignmentOptions.Center;
        gbtnt.color = Color.white;
    }
}
