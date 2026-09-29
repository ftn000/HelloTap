using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система Сезонных Боевых Пропусков («Studio Season Battle Pass 2.0»):
/// - Сезон 2: «Cyberpunk Code Renaissance»
/// - 15 уровней прогрессии с двумя дорожками: Free Pass и Cyber Pass (Gold)
/// - Еженедельные испытания (Seasonal Bounties): выполнение заданий дает солидные порции XP и наград
/// - Перманентные множители к пассивному доходу студии и силе клика
/// - Награды: чеки в рублях, пакеты строк кода, временные бусты и титулы
/// </summary>
public class StudioBattlePassUI : MonoBehaviour
{
    private static StudioBattlePassUI instance;
    public static StudioBattlePassUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StudioBattlePassUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(StudioBattlePassUI));
                    instance = go.AddComponent<StudioBattlePassUI>();
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
    public class BattlePassTier
    {
        public int level;
        public int xpRequired;
        public string freeRewardTitle;
        public double freeMoney;
        public double freeCode;
        public string cyberRewardTitle;
        public double cyberMoney;
        public double cyberCode;
        public bool isFreeClaimed;
        public bool isCyberClaimed;
    }

    [System.Serializable]
    public class SeasonBounty
    {
        public string id;
        public string title;
        public string desc;
        public int targetAmount;
        public int currentAmount;
        public int xpReward;
        public double moneyReward;
        public bool isCompleted;
        public bool isClaimed;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openPassBtn;

    [Header("Прогресс сезона и Cyber Pass")]
    [SerializeField] private TMP_Text seasonHeaderTxt;
    [SerializeField] private TMP_Text seasonLevelTxt;
    [SerializeField] private TMP_Text seasonXpTxt;
    [SerializeField] private Slider seasonXpSlider;
    [SerializeField] private Button unlockCyberPassBtn;
    [SerializeField] private TMP_Text unlockCyberPassBtnTxt;
    [SerializeField] private Transform tiersContainer;

    [Header("Испытания (Bounties)")]
    [SerializeField] private Transform bountiesContainer;

    private readonly List<BattlePassTier> tiers = new List<BattlePassTier>();
    private readonly List<SeasonBounty> bounties = new List<SeasonBounty>();

    private int currentXp = 0;
    private int currentLevel = 1;
    private bool hasCyberPass = false;

    private const int XpPerTier = 250;
    private const double CyberPassCostRub = 250000.0;
    private const double CyberPassCostCode = 125000.0;

    private const string PrefPassXp = "BattlePass2_XP";
    private const string PrefHasCyber = "BattlePass2_HasCyber";
    private const string PrefClaimedFree = "BattlePass2_ClaimedFree_";
    private const string PrefClaimedCyber = "BattlePass2_ClaimedCyber_";
    private const string PrefBountyProgress = "BattlePass2_BountyProg_";
    private const string PrefBountyClaimed = "BattlePass2_BountyClaimed_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CurrentLevel => currentLevel;
    public bool HasCyberPass => hasCyberPass;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTiers();
        InitializeBounties();
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
        }
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Накопление XP за клики
        int xpGain = Math.Max(1, (int)(amount / 50.0));
        AddPassXp(xpGain, false);

        // Прогресс испытаний
        UpdateBountyProgress("bounty_click_code", (int)Math.Min(amount, 100));
        if (isCrit)
        {
            UpdateBountyProgress("bounty_crits", 1);
        }
    }

    private void HandleProjectCompleted(GameProjectData proj)
    {
        AddPassXp(120, true);
        UpdateBountyProgress("bounty_earn_money", 50000);
    }

    private void InitializeTiers()
    {
        if (tiers.Count > 0) return;

        for (int i = 1; i <= 15; i++)
        {
            double baseM = 15000.0 * i;
            double baseC = 7000.0 * i;

            tiers.Add(new BattlePassTier
            {
                level = i,
                xpRequired = i * XpPerTier,
                freeRewardTitle = $"Чек {NumberFormatter.Format(baseM)} ₽",
                freeMoney = baseM,
                freeCode = Math.Floor(baseC * 0.5),
                cyberRewardTitle = $"VIP Пакет {NumberFormatter.Format(baseM * 2.5)} ₽ + {NumberFormatter.Format(baseC * 2)} C#",
                cyberMoney = baseM * 2.5,
                cyberCode = baseC * 2.0,
                isFreeClaimed = false,
                isCyberClaimed = false
            });
        }
    }

    private void InitializeBounties()
    {
        if (bounties.Count > 0) return;

        bounties.Add(new SeasonBounty
        {
            id = "bounty_click_code",
            title = "💻 Неудержимый кодинг",
            desc = "Написать 2,000 строк кода в потоке",
            targetAmount = 2000,
            currentAmount = 0,
            xpReward = 300,
            moneyReward = 45000.0
        });

        bounties.Add(new SeasonBounty
        {
            id = "bounty_crits",
            title = "⚡ Архитектурный фокус",
            desc = "Выбить 25 критических кликов кода",
            targetAmount = 25,
            currentAmount = 0,
            xpReward = 350,
            moneyReward = 60000.0
        });

        bounties.Add(new SeasonBounty
        {
            id = "bounty_earn_money",
            title = "💎 Успешный релиз студии",
            desc = "Заработать капитал на проектах студии",
            targetAmount = 150000,
            currentAmount = 0,
            xpReward = 400,
            moneyReward = 90000.0
        });
    }

    private void LoadData()
    {
        currentXp = PlayerPrefs.GetInt(PrefPassXp, 0);
        hasCyberPass = PlayerPrefs.GetInt(PrefHasCyber, 0) == 1;

        for (int i = 0; i < tiers.Count; i++)
        {
            tiers[i].isFreeClaimed = PlayerPrefs.GetInt(PrefClaimedFree + i, 0) == 1;
            tiers[i].isCyberClaimed = PlayerPrefs.GetInt(PrefClaimedCyber + i, 0) == 1;
        }

        foreach (var b in bounties)
        {
            b.currentAmount = PlayerPrefs.GetInt(PrefBountyProgress + b.id, 0);
            b.isClaimed = PlayerPrefs.GetInt(PrefBountyClaimed + b.id, 0) == 1;
            b.isCompleted = b.currentAmount >= b.targetAmount;
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefPassXp, currentXp);
        PlayerPrefs.SetInt(PrefHasCyber, hasCyberPass ? 1 : 0);

        for (int i = 0; i < tiers.Count; i++)
        {
            PlayerPrefs.SetInt(PrefClaimedFree + i, tiers[i].isFreeClaimed ? 1 : 0);
            PlayerPrefs.SetInt(PrefClaimedCyber + i, tiers[i].isCyberClaimed ? 1 : 0);
        }

        foreach (var b in bounties)
        {
            PlayerPrefs.SetInt(PrefBountyProgress + b.id, b.currentAmount);
            PlayerPrefs.SetInt(PrefBountyClaimed + b.id, b.isClaimed ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    public void AddPassXp(int amount, bool showPopup = true)
    {
        if (amount <= 0) return;
        currentXp += amount;
        int oldLvl = currentLevel;
        CalculateLevel();

        SaveData();

        if (currentLevel > oldLvl)
        {
            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎖️ НОВЫЙ ТИР BATTLE PASS {currentLevel}!\nЗаберите награды в меню пропуска!", transform.position, new Color(1f, 0.85f, 0.2f), true);
            }
        }
        else if (showPopup && ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"+{amount} Battle Pass XP", transform.position, new Color(0.3f, 0.9f, 1f), false);
        }

        RefreshUI();
    }

    private void CalculateLevel()
    {
        currentLevel = 1 + (currentXp / XpPerTier);
        if (currentLevel > 15) currentLevel = 15;
    }

    public void UpdateBountyProgress(string bountyId, int amount)
    {
        var b = bounties.Find(x => x.id == bountyId);
        if (b == null || b.isCompleted) return;

        b.currentAmount = Math.Min(b.targetAmount, b.currentAmount + amount);
        if (b.currentAmount >= b.targetAmount)
        {
            b.isCompleted = true;
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎯 ИСПЫТАНИЕ ЗАВЕРШЕНО: {b.title}!\nНаграда ждет в Battle Pass!", transform.position, new Color(0.2f, 1f, 0.5f), true);
            }
        }
        SaveData();
        RefreshUI();
    }

    public double GetBattlePassIncomeMultiplier()
    {
        // +3% к доходу за каждый открытый тир, +15% если активен Cyber Pass
        double bonus = (currentLevel - 1) * 0.03;
        if (hasCyberPass) bonus += 0.15;
        return 1.0 + bonus;
    }

    public double GetBattlePassClickMultiplier()
    {
        // +2% к коду за клик за каждый тир
        double bonus = (currentLevel - 1) * 0.02;
        if (hasCyberPass) bonus += 0.10;
        return 1.0 + bonus;
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
        if (unlockCyberPassBtn != null)
        {
            unlockCyberPassBtn.onClick.RemoveAllListeners();
            unlockCyberPassBtn.onClick.AddListener(UnlockCyberPass);
        }
        if (openPassBtn != null)
        {
            openPassBtn.onClick.RemoveAllListeners();
            openPassBtn.onClick.AddListener(OpenModal);
        }
    }

    public void UnlockCyberPass()
    {
        if (hasCyberPass) return;

        double curMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;
        double curCode = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;

        if (curMoney < CyberPassCostRub || curCode < CyberPassCostCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(CyberPassCostRub)} ₽ и {NumberFormatter.Format(CyberPassCostCode)} C#!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendMoney(CyberPassCostRub);
            GameManager.Instance.SpendLinesOfCode(CyberPassCostCode);
        }

        hasCyberPass = true;
        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("👑 CYBER PASS РАЗБЛОКИРОВАН!\nДоступны все премиум награды сезона!", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }
    }

    public void ClaimReward(int tierIndex, bool isCyber)
    {
        if (tierIndex < 0 || tierIndex >= tiers.Count) return;
        var tier = tiers[tierIndex];

        if (currentLevel < tier.level)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"Уровень {tier.level} еще не достигнут!", transform.position, Color.yellow, false);
            }
            return;
        }

        if (isCyber)
        {
            if (!hasCyberPass)
            {
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup("Требуется активация Cyber Pass!", transform.position, Color.yellow, false);
                }
                return;
            }
            if (tier.isCyberClaimed) return;

            tier.isCyberClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(tier.cyberMoney);
                GameManager.Instance.AddLinesOfCode(tier.cyberCode);
                GameManager.Instance.AddComboEnergy(0.15f);
            }
        }
        else
        {
            if (tier.isFreeClaimed) return;

            tier.isFreeClaimed = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(tier.freeMoney);
                GameManager.Instance.AddLinesOfCode(tier.freeCode);
            }
        }

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            double m = isCyber ? tier.cyberMoney : tier.freeMoney;
            double c = isCyber ? tier.cyberCode : tier.freeCode;
            ClickJuice.Instance.SpawnCustomPopup($"🎁 НАГРАДА ПОЛУЧЕНА!\n+{NumberFormatter.Format(m)} ₽ и +{NumberFormatter.Format(c)} C#", transform.position, new Color(0.2f, 1f, 0.5f), true);
        }
    }

    public void ClaimBounty(string bountyId)
    {
        var b = bounties.Find(x => x.id == bountyId);
        if (b == null || !b.isCompleted || b.isClaimed) return;

        b.isClaimed = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(b.moneyReward);
        }
        AddPassXp(b.xpReward, false);

        SaveData();
        RefreshUI();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎯 НАГРАДА ЗА ИСПЫТАНИЕ!\n+{NumberFormatter.Format(b.moneyReward)} ₽ и +{b.xpReward} Battle Pass XP!", transform.position, new Color(0.3f, 0.9f, 1f), true);
        }
    }

    public void RefreshUI()
    {
        if (seasonHeaderTxt != null)
        {
            double incMult = GetBattlePassIncomeMultiplier();
            double clkMult = GetBattlePassClickMultiplier();
            seasonHeaderTxt.text = $"Сезон 2: <color=#00FFAA>«Cyberpunk Code Renaissance»</color>\nБонус дохода: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>";
        }

        if (seasonLevelTxt != null)
        {
            seasonLevelTxt.text = $"Уровень пропуска: <color=#00FFAA><b>{currentLevel} / 15</b></color>";
        }

        if (seasonXpTxt != null)
        {
            int tierXp = currentXp % XpPerTier;
            seasonXpTxt.text = currentLevel >= 15 ? "MAX УРОВЕНЬ" : $"{tierXp} / {XpPerTier} XP";
        }

        if (seasonXpSlider != null)
        {
            float prog = currentLevel >= 15 ? 1f : (float)(currentXp % XpPerTier) / XpPerTier;
            seasonXpSlider.value = prog;
        }

        if (unlockCyberPassBtnTxt != null)
        {
            if (hasCyberPass)
            {
                unlockCyberPassBtnTxt.text = "👑 CYBER PASS АКТИВЕН";
                if (unlockCyberPassBtn != null) unlockCyberPassBtn.interactable = false;
            }
            else
            {
                unlockCyberPassBtnTxt.text = $"👑 КУПИТЬ CYBER PASS ({NumberFormatter.Format(CyberPassCostRub)} ₽ | {NumberFormatter.Format(CyberPassCostCode)} C#)";
                if (unlockCyberPassBtn != null) unlockCyberPassBtn.interactable = true;
            }
        }

        // Обновляем карточки тиров
        if (tiersContainer != null)
        {
            for (int i = 0; i < tiers.Count; i++)
            {
                var tier = tiers[i];
                Transform child = i < tiersContainer.childCount ? tiersContainer.GetChild(i) : null;
                if (child == null) continue;

                TMP_Text tTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
                Button freeBtn = child.Find("Body/FreeClaimBtn")?.GetComponent<Button>();
                TMP_Text freeBtnTxt = freeBtn != null ? freeBtn.GetComponentInChildren<TMP_Text>() : null;
                Button cyberBtn = child.Find("Body/CyberClaimBtn")?.GetComponent<Button>();
                TMP_Text cyberBtnTxt = cyberBtn != null ? cyberBtn.GetComponentInChildren<TMP_Text>() : null;

                if (tTxt != null)
                {
                    bool isReached = currentLevel >= tier.level;
                    tTxt.text = $"Тир {tier.level} {(isReached ? "🔓" : "🔒")}";
                    tTxt.color = isReached ? new Color(0.2f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f);
                }

                if (freeBtn != null && freeBtnTxt != null)
                {
                    if (tier.isFreeClaimed)
                    {
                        freeBtnTxt.text = "ВЗЯТО ✓";
                        freeBtn.interactable = false;
                    }
                    else
                    {
                        freeBtnTxt.text = $"Free: {tier.freeRewardTitle}";
                        freeBtn.interactable = currentLevel >= tier.level;
                    }
                }

                if (cyberBtn != null && cyberBtnTxt != null)
                {
                    if (tier.isCyberClaimed)
                    {
                        cyberBtnTxt.text = "ВЗЯТО ✓";
                        cyberBtn.interactable = false;
                    }
                    else
                    {
                        cyberBtnTxt.text = $"Cyber: {tier.cyberRewardTitle}";
                        cyberBtn.interactable = currentLevel >= tier.level && hasCyberPass;
                    }
                }
            }
        }

        // Обновляем карточки испытаний (Bounties)
        if (bountiesContainer != null)
        {
            for (int i = 0; i < bounties.Count; i++)
            {
                var b = bounties[i];
                Transform child = i < bountiesContainer.childCount ? bountiesContainer.GetChild(i) : null;
                if (child == null) continue;

                TMP_Text title = child.Find("Title")?.GetComponent<TMP_Text>();
                TMP_Text prog = child.Find("Progress")?.GetComponent<TMP_Text>();
                Button claimBtn = child.Find("ClaimBtn")?.GetComponent<Button>();
                TMP_Text claimTxt = claimBtn != null ? claimBtn.GetComponentInChildren<TMP_Text>() : null;

                if (title != null) title.text = b.title;
                if (prog != null) prog.text = $"{b.desc} ({b.currentAmount}/{b.targetAmount})";

                if (claimBtn != null && claimTxt != null)
                {
                    if (b.isClaimed)
                    {
                        claimTxt.text = "ВЫПОЛНЕНО ✓";
                        claimBtn.interactable = false;
                    }
                    else if (b.isCompleted)
                    {
                        claimTxt.text = $"ЗАБРАТЬ +{b.xpReward} XP";
                        claimBtn.interactable = true;
                    }
                    else
                    {
                        claimTxt.text = $"В ПРОЦЕССЕ";
                        claimBtn.interactable = false;
                    }
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("StudioBattlePass_ModalRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        modalRoot = root;

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(root.transform, false);
        RectTransform bRect = backdrop.GetComponent<RectTransform>();
        bRect.anchorMin = Vector2.zero;
        bRect.anchorMax = Vector2.one;
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        Image bImg = backdrop.GetComponent<Image>();
        bImg.color = new Color(0.04f, 0.04f, 0.08f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("PassCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(640, 720);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.11f, 0.12f, 0.18f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 70);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.18f, 0.15f, 0.32f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "⚡ СЕЗОННЫЙ БОЕВОЙ ПРОПУСК 2.0";
        tTxt.fontSize = 19;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(1f, 0.80f, 0.25f);
        RectTransform tRect = titleObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0f, 0f);
        tRect.anchorMax = new Vector2(1f, 1f);
        tRect.offsetMin = new Vector2(20, 0);
        tRect.offsetMax = new Vector2(-70, 0);

        // Close X
        GameObject xBtnObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        xBtnObj.transform.SetParent(header.transform, false);
        RectTransform xRect = xBtnObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 0.5f);
        xRect.anchorMax = new Vector2(1f, 0.5f);
        xRect.pivot = new Vector2(1f, 0.5f);
        xRect.sizeDelta = new Vector2(38, 38);
        xRect.anchoredPosition = new Vector2(-15, 0);
        xBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.85f);
        closeXBtn = xBtnObj.GetComponent<Button>();

        GameObject xTxtObj = new GameObject("XTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxtObj.transform.SetParent(xBtnObj.transform, false);
        TextMeshProUGUI xTxt = xTxtObj.GetComponent<TextMeshProUGUI>();
        xTxt.text = "✕";
        xTxt.fontSize = 20;
        xTxt.fontStyle = FontStyles.Bold;
        xTxt.alignment = TextAlignmentOptions.Center;
        xTxt.color = Color.white;
        RectTransform xtRect = xTxtObj.GetComponent<RectTransform>();
        xtRect.anchorMin = Vector2.zero;
        xtRect.anchorMax = Vector2.one;
        xtRect.offsetMin = Vector2.zero;
        xtRect.offsetMax = Vector2.zero;

        // Subheader Info
        GameObject statsObj = new GameObject("SeasonHeaderTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 48);
        stRect.anchoredPosition = new Vector2(0, -74);
        seasonHeaderTxt = statsObj.GetComponent<TextMeshProUGUI>();
        seasonHeaderTxt.fontSize = 13;
        seasonHeaderTxt.alignment = TextAlignmentOptions.Center;
        seasonHeaderTxt.color = new Color(0.92f, 0.92f, 1f);

        // Progress Bar & Cyber Pass button
        GameObject progPanel = new GameObject("ProgPanel", typeof(RectTransform), typeof(Image));
        progPanel.transform.SetParent(card.transform, false);
        RectTransform ppRect = progPanel.GetComponent<RectTransform>();
        ppRect.anchorMin = new Vector2(0f, 1f);
        ppRect.anchorMax = new Vector2(1f, 1f);
        ppRect.pivot = new Vector2(0.5f, 1f);
        ppRect.sizeDelta = new Vector2(-30, 68);
        ppRect.anchoredPosition = new Vector2(0, -125);
        progPanel.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.24f, 1f);

        // Slider for XP
        GameObject sliderObj = new GameObject("XpSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(progPanel.transform, false);
        RectTransform slRect = sliderObj.GetComponent<RectTransform>();
        slRect.anchorMin = new Vector2(0f, 0.5f);
        slRect.anchorMax = new Vector2(0.5f, 0.5f);
        slRect.pivot = new Vector2(0f, 0.5f);
        slRect.offsetMin = new Vector2(12, -8);
        slRect.offsetMax = new Vector2(0, 8);
        seasonXpSlider = sliderObj.GetComponent<Slider>();

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.GetComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fRect = fill.GetComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.offsetMin = Vector2.zero;
        fRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.2f, 0.85f, 1f);
        seasonXpSlider.fillRect = fRect;

        // Level & XP Text
        GameObject lvlTxtObj = new GameObject("LvlTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlTxtObj.transform.SetParent(progPanel.transform, false);
        RectTransform ltRect = lvlTxtObj.GetComponent<RectTransform>();
        ltRect.anchorMin = new Vector2(0f, 1f);
        ltRect.anchorMax = new Vector2(0.5f, 1f);
        ltRect.offsetMin = new Vector2(12, -26);
        ltRect.offsetMax = new Vector2(0, -6);
        seasonLevelTxt = lvlTxtObj.GetComponent<TextMeshProUGUI>();
        seasonLevelTxt.fontSize = 12;
        seasonLevelTxt.color = Color.white;

        // Unlock Cyber Pass Button
        GameObject cpBtnObj = new GameObject("UnlockCyberPassBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cpBtnObj.transform.SetParent(progPanel.transform, false);
        RectTransform cpbRect = cpBtnObj.GetComponent<RectTransform>();
        cpbRect.anchorMin = new Vector2(0.52f, 0.1f);
        cpbRect.anchorMax = new Vector2(0.98f, 0.9f);
        cpbRect.offsetMin = Vector2.zero;
        cpbRect.offsetMax = Vector2.zero;
        cpBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.15f, 1f);
        unlockCyberPassBtn = cpBtnObj.GetComponent<Button>();

        GameObject cpTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cpTxtObj.transform.SetParent(cpBtnObj.transform, false);
        unlockCyberPassBtnTxt = cpTxtObj.GetComponent<TextMeshProUGUI>();
        unlockCyberPassBtnTxt.text = "👑 КУПИТЬ CYBER PASS";
        unlockCyberPassBtnTxt.fontSize = 11;
        unlockCyberPassBtnTxt.fontStyle = FontStyles.Bold;
        unlockCyberPassBtnTxt.alignment = TextAlignmentOptions.Center;
        unlockCyberPassBtnTxt.color = Color.white;
        RectTransform cptr = cpTxtObj.GetComponent<RectTransform>();
        cptr.anchorMin = Vector2.zero;
        cptr.anchorMax = Vector2.one;
        cptr.offsetMin = Vector2.zero;
        cptr.offsetMax = Vector2.zero;

        // Tiers Scroll View
        GameObject scrollObj = new GameObject("TiersScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -200);
        scrollObj.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 0.5f);

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
        tiersContainer = content.transform;
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
        vlg.spacing = 6f;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.viewport = vRect;
        sr.content = ctnRect;
        sr.horizontal = false;
        sr.vertical = true;

        for (int i = 0; i < tiers.Count; i++)
        {
            CreateTierCardUI(content.transform, tiers[i], i);
        }

        // Close Bottom Button
        GameObject botCloseObj = new GameObject("CloseBottomBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        botCloseObj.transform.SetParent(card.transform, false);
        RectTransform bcRect = botCloseObj.GetComponent<RectTransform>();
        bcRect.anchorMin = new Vector2(0.5f, 0f);
        bcRect.anchorMax = new Vector2(0.5f, 0f);
        bcRect.pivot = new Vector2(0.5f, 0f);
        bcRect.sizeDelta = new Vector2(180, 42);
        bcRect.anchoredPosition = new Vector2(0, 10);
        botCloseObj.GetComponent<Image>().color = new Color(0.24f, 0.26f, 0.38f, 1f);
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

    private void CreateTierCardUI(Transform parent, BattlePassTier tier, int index)
    {
        GameObject card = new GameObject("TierCard_" + tier.level, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 78);
        card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.23f, 1f);

        // Header
        GameObject hdr = new GameObject("Header", typeof(RectTransform));
        hdr.transform.SetParent(card.transform, false);
        RectTransform hr = hdr.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(0.25f, 1f);
        hr.pivot = new Vector2(0f, 1f);
        hr.sizeDelta = new Vector2(0, 26);
        hr.anchoredPosition = new Vector2(10, -6);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(hdr.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 13;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.text = $"Тир {tier.level}";
        tTxt.color = Color.white;
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        // Body
        GameObject body = new GameObject("Body", typeof(RectTransform));
        body.transform.SetParent(card.transform, false);
        RectTransform br = body.GetComponent<RectTransform>();
        br.anchorMin = Vector2.zero;
        br.anchorMax = Vector2.one;
        br.offsetMin = new Vector2(10, 8);
        br.offsetMax = new Vector2(-10, -32);

        // Free Claim Button
        GameObject freeBtnObj = new GameObject("FreeClaimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        freeBtnObj.transform.SetParent(body.transform, false);
        RectTransform fbr = freeBtnObj.GetComponent<RectTransform>();
        fbr.anchorMin = new Vector2(0f, 0f);
        fbr.anchorMax = new Vector2(0.48f, 1f);
        fbr.offsetMin = Vector2.zero;
        fbr.offsetMax = Vector2.zero;
        freeBtnObj.GetComponent<Image>().color = new Color(0.20f, 0.50f, 0.80f, 1f);

        GameObject fTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        fTxt.transform.SetParent(freeBtnObj.transform, false);
        TextMeshProUGUI ft = fTxt.GetComponent<TextMeshProUGUI>();
        ft.fontSize = 10;
        ft.fontStyle = FontStyles.Bold;
        ft.alignment = TextAlignmentOptions.Center;
        ft.color = Color.white;
        RectTransform ftr = fTxt.GetComponent<RectTransform>();
        ftr.anchorMin = Vector2.zero;
        ftr.anchorMax = Vector2.one;
        ftr.offsetMin = Vector2.zero;
        ftr.offsetMax = Vector2.zero;

        // Cyber Claim Button
        GameObject cyberBtnObj = new GameObject("CyberClaimBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cyberBtnObj.transform.SetParent(body.transform, false);
        RectTransform cbr = cyberBtnObj.GetComponent<RectTransform>();
        cbr.anchorMin = new Vector2(0.52f, 0f);
        cbr.anchorMax = new Vector2(1f, 1f);
        cbr.offsetMin = Vector2.zero;
        cbr.offsetMax = Vector2.zero;
        cyberBtnObj.GetComponent<Image>().color = new Color(0.75f, 0.55f, 0.12f, 1f);

        GameObject cTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cTxt.transform.SetParent(cyberBtnObj.transform, false);
        TextMeshProUGUI ct = cTxt.GetComponent<TextMeshProUGUI>();
        ct.fontSize = 10;
        ct.fontStyle = FontStyles.Bold;
        ct.alignment = TextAlignmentOptions.Center;
        ct.color = Color.white;
        RectTransform ctr = cTxt.GetComponent<RectTransform>();
        ctr.anchorMin = Vector2.zero;
        ctr.anchorMax = Vector2.one;
        ctr.offsetMin = Vector2.zero;
        ctr.offsetMax = Vector2.zero;

        int idx = index;
        freeBtnObj.GetComponent<Button>().onClick.AddListener(() => ClaimReward(idx, false));
        cyberBtnObj.GetComponent<Button>().onClick.AddListener(() => ClaimReward(idx, true));
    }
}
