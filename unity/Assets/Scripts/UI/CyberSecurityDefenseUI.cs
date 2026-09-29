using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Кибербезопасность и Отражение DDoS-атак (Cyber Security & Incident Response):
/// - 4 рубежа обороны: Cloud WAF, Zero-Day Patcher, Honeypot-ловушки, SIEM Мониторинг
/// - Динамические кибератаки (DDoS ботнеты, APT-шпионаж, Ransomware, эксплойты)
/// - Интерактивная мини-игра отражения атак кликами по пакетам/контрмерам
/// - Bounty-награды за успешную нейтрализацию угроз
/// - Защита серверов от падений и перманентный множитель дохода студии
/// </summary>
public class CyberSecurityDefenseUI : MonoBehaviour
{
    private static CyberSecurityDefenseUI instance;
    public static CyberSecurityDefenseUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CyberSecurityDefenseUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CyberSecurityDefenseUI));
                    instance = go.AddComponent<CyberSecurityDefenseUI>();
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
    public class DefenseSystem
    {
        public string id;
        public string title;
        public string icon;
        public string description;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.5, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.4, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openSecurityBtn;

    [Header("Индикатор текущей угрозы")]
    [SerializeField] private GameObject threatBannerRoot;
    [SerializeField] private TMP_Text threatBannerTxt;
    [SerializeField] private Button threatBannerBtn;
    [SerializeField] private TMP_Text activeThreatTitleTxt;
    [SerializeField] private TMP_Text activeThreatTimerTxt;
    [SerializeField] private Image activeThreatTimerFill;
    [SerializeField] private Button mitigateAttackBtn;
    [SerializeField] private TMP_Text mitigateBtnTxt;
    [SerializeField] private TMP_Text securityStatusSummaryTxt;
    [SerializeField] private Transform defenseListContainer;

    private readonly List<DefenseSystem> systems = new List<DefenseSystem>();

    // Состояние инцидента
    private bool isUnderAttack = false;
    private string currentThreatName = "DDoS 500 Gbps Ботнет";
    private float threatDurationTotal = 35f;
    private float threatTimeRemaining = 0f;
    private int countermeasureClicksNeeded = 3;
    private int currentCountermeasureClicks = 0;
    private double currentBountyMoney = 15000;
    private double currentBountyCode = 1200;
    private float timeSinceLastAttack = 0f;
    private float attackInterval = 110f; // Каждые 1.5 - 2 минуты

    private const string PrefSystemPrefix = "CyberSec_DefLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool IsUnderAttack => isUnderAttack;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeDefenseSystems();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        if (threatBannerRoot != null) threatBannerRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void Update()
    {
        // Таймер инцидента
        if (isUnderAttack)
        {
            threatTimeRemaining -= Time.deltaTime;
            if (activeThreatTimerTxt != null)
            {
                activeThreatTimerTxt.text = $"⏱️ До падения серверов: <b><color=#FF4444>{threatTimeRemaining:F1} сек</color></b>";
            }
            if (activeThreatTimerFill != null && threatDurationTotal > 0f)
            {
                activeThreatTimerFill.fillAmount = Mathf.Clamp01(threatTimeRemaining / threatDurationTotal);
            }
            if (threatBannerTxt != null)
            {
                threatBannerTxt.text = $"🚨 <b>{currentThreatName}</b>! Отразить: {threatTimeRemaining:F0}с";
            }

            if (threatTimeRemaining <= 0f)
            {
                OnAttackFailedToMitigate();
            }
        }
        else
        {
            // Отсчет до следующей случайной атаки
            timeSinceLastAttack += Time.deltaTime;
            float actualInterval = attackInterval * (1.0f + GetSystemLevel("waf") * 0.1f);
            if (timeSinceLastAttack >= actualInterval)
            {
                TriggerRandomCyberAttack();
            }
        }
    }

    private void InitializeDefenseSystems()
    {
        systems.Clear();

        systems.Add(new DefenseSystem
        {
            id = "waf",
            title = "Облачный WAF Фаервол",
            icon = "🛡️",
            description = "+10% к интервалу между атаками и +2 сек к окну реакции",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 1500,
            baseCostCode = 300
        });

        systems.Add(new DefenseSystem
        {
            id = "zeroday",
            title = "Патчер Zero-Day Эксплойтов",
            icon = "🩹",
            description = "+4% к пассивной защите и надежности кода",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 2500,
            baseCostCode = 600
        });

        systems.Add(new DefenseSystem
        {
            id = "honeypot",
            title = "Honeypot ИИ-Ловушки",
            icon = "🍯",
            description = "+25% к выплатам Bounty-грантов за нейтрализацию атак",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 4000,
            baseCostCode = 1000
        });

        systems.Add(new DefenseSystem
        {
            id = "siem",
            title = "SIEM Нейро-Мониторинг",
            icon = "👁️",
            description = "+5% к общему доходу студии благодаря аптайму 99.99%",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 7000,
            baseCostCode = 1800
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < systems.Count; i++)
        {
            int def = systems[i].id == "waf" ? 1 : 0;
            systems[i].level = PlayerPrefs.GetInt(PrefSystemPrefix + systems[i].id, def);
            systems[i].level = Mathf.Clamp(systems[i].level, 0, systems[i].maxLevel);
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < systems.Count; i++)
        {
            PlayerPrefs.SetInt(PrefSystemPrefix + systems[i].id, systems[i].level);
        }
        PlayerPrefs.Save();
    }

    public double GetSecurityIncomeMultiplier()
    {
        double siemBonus = 1.0 + (GetSystemLevel("siem") * 0.05);
        double zeroDayBonus = 1.0 + (GetSystemLevel("zeroday") * 0.04);
        return siemBonus * zeroDayBonus;
    }

    public int GetSystemLevel(string id)
    {
        var s = systems.Find(x => x.id == id);
        return s != null ? s.level : 0;
    }

    public void TriggerRandomCyberAttack()
    {
        if (isUnderAttack) return;

        timeSinceLastAttack = 0f;
        isUnderAttack = true;

        string[] attackNames = new string[]
        {
            "DDoS 650 Gbps Ботнет (Mirai-X)",
            "SQL-инъекция в прод-кластер",
            "Ransomware-шифровальщик баз",
            "0-Day Эксплойт в сетевом сокете",
            "Brute-Force атака на SSH-шлюз"
        };

        int r = UnityEngine.Random.Range(0, attackNames.Length);
        currentThreatName = attackNames[r];

        float reactionBonus = GetSystemLevel("waf") * 2.0f;
        threatDurationTotal = 30f + reactionBonus;
        threatTimeRemaining = threatDurationTotal;

        countermeasureClicksNeeded = Mathf.Max(2, 4 - (GetSystemLevel("zeroday") / 3));
        currentCountermeasureClicks = 0;

        // Расчет Bounty награды на основе прогресса
        double studioNetWorth = GameManager.Instance != null ? GameManager.Instance.TotalMoneyEarned : 50000;
        double baseBounty = Math.Max(2500, studioNetWorth * 0.03);
        double honeypotMult = 1.0 + (GetSystemLevel("honeypot") * 0.25);
        currentBountyMoney = Math.Floor(baseBounty * honeypotMult);
        currentBountyCode = Math.Floor(currentBountyMoney * 0.08);

        if (threatBannerRoot != null) threatBannerRoot.SetActive(true);
        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCrit();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🚨 ВНИМАНИЕ! КИБЕРАТАКА!\n{currentThreatName}", transform.position, new Color(1f, 0.25f, 0.25f), true);
        }

        UpdateModalUI();
    }

    public void OnMitigateButtonClicked()
    {
        if (!isUnderAttack)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("🛡️ Периметр в безопасности, атак нет!", transform.position, Color.cyan, false);
            }
            return;
        }

        currentCountermeasureClicks++;
        HapticFeedback.MediumImpact();

        if (currentCountermeasureClicks >= countermeasureClicksNeeded)
        {
            OnAttackSuccessfullyRepelled();
        }
        else
        {
            if (mitigateBtnTxt != null)
            {
                mitigateBtnTxt.text = $"⚡ ВВОД ПАТЧА ({currentCountermeasureClicks}/{countermeasureClicksNeeded})... ЖМИ ЕЩЕ!";
            }
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🛡️ ПАКЕТ ЗАБЛОКИРОВАН! ({currentCountermeasureClicks}/{countermeasureClicksNeeded})", transform.position, new Color(0.2f, 0.8f, 1f), false);
            }
        }
    }

    private void OnAttackSuccessfullyRepelled()
    {
        isUnderAttack = false;
        timeSinceLastAttack = 0f;

        if (threatBannerRoot != null) threatBannerRoot.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(currentBountyMoney);
            GameManager.Instance.AddLinesOfCode(currentBountyCode);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 АТАКА ОТБИТА!\nПолучен Bounty-грант: <b>+{NumberFormatter.Format(currentBountyMoney)} ₽</b>\n+{NumberFormatter.Format(currentBountyCode)} строк кода!", transform.position, new Color(0.1f, 1f, 0.5f), true);
        }

        UpdateModalUI();
    }

    private void OnAttackFailedToMitigate()
    {
        isUnderAttack = false;
        timeSinceLastAttack = 0f;

        if (threatBannerRoot != null) threatBannerRoot.SetActive(false);

        HapticFeedback.HeavyImpact();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💥 СЕРВЕРЫ ПЕРЕГРУЖЕНЫ!\nАтака {currentThreatName} нанесла урон репутации!", transform.position, new Color(1f, 0.2f, 0.2f), true);
        }

        UpdateModalUI();
    }

    public bool TryUpgradeSystem(string id)
    {
        var sys = systems.Find(x => x.id == id);
        if (sys == null || sys.level >= sys.maxLevel) return false;

        double costMoney = sys.GetCostMoney();
        double costCode = sys.GetCostCode();

        if (GameManager.Instance == null || GameManager.Instance.Money < costMoney || GameManager.Instance.CodeLines < costCode)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно {NumberFormatter.Format(costMoney)} ₽ и {NumberFormatter.Format(costCode)} кода!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendMoney(costMoney);
        GameManager.Instance.SpendLinesOfCode(costCode);

        sys.level++;
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛡️ СИСТЕМА УСИЛЕНА!\n{sys.icon} {sys.title} -> ур. {sys.level}", transform.position, new Color(0.2f, 1f, 0.8f), true);
        }

        UpdateModalUI();
        return true;
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
        if (openSecurityBtn != null)
        {
            openSecurityBtn.onClick.RemoveAllListeners();
            openSecurityBtn.onClick.AddListener(OpenModal);
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
        if (threatBannerBtn != null)
        {
            threatBannerBtn.onClick.RemoveAllListeners();
            threatBannerBtn.onClick.AddListener(OpenModal);
        }
        if (mitigateAttackBtn != null)
        {
            mitigateAttackBtn.onClick.RemoveAllListeners();
            mitigateAttackBtn.onClick.AddListener(OnMitigateButtonClicked);
        }
    }

    private void UpdateModalUI()
    {
        if (activeThreatTitleTxt != null)
        {
            if (isUnderAttack)
            {
                activeThreatTitleTxt.text = $"⚠️ <b><color=#FF4444>АКТИВНАЯ УГРОЗА:</color></b> {currentThreatName}\nBounty-награда: <b>+{NumberFormatter.Format(currentBountyMoney)} ₽</b> (+{NumberFormatter.Format(currentBountyCode)} кода)";
            }
            else
            {
                activeThreatTitleTxt.text = "🟢 <b><color=#00FF88>ПЕРИМЕТР ЧИСТ</color></b> | Сетевой трафик защищен WAF";
            }
        }

        if (mitigateAttackBtn != null && mitigateBtnTxt != null)
        {
            if (isUnderAttack)
            {
                mitigateAttackBtn.interactable = true;
                mitigateBtnTxt.text = $"🛡️ ОТРАЗИТЬ АТАКУ ({currentCountermeasureClicks}/{countermeasureClicksNeeded})";
            }
            else
            {
                mitigateAttackBtn.interactable = false;
                mitigateBtnTxt.text = "СИСТЕМА В РЕЖИМЕ МОНИТОРИНГА";
            }
        }

        if (securityStatusSummaryTxt != null)
        {
            double secBonus = (GetSecurityIncomeMultiplier() - 1.0) * 100.0;
            double bountyBonus = GetSystemLevel("honeypot") * 25.0;
            securityStatusSummaryTxt.text = $"Бонус к доходу: <b><color=#00FF88>+{secBonus:F0}%</color></b> | Бонус к Bounty: <b><color=#00E5FF>+{bountyBonus:F0}%</color></b> | Защита WAF: <b>ур. {GetSystemLevel("waf")}</b>";
        }

        RefreshDefenseCards();
    }

    private void RefreshDefenseCards()
    {
        if (defenseListContainer == null) return;

        for (int i = 0; i < systems.Count; i++)
        {
            var sys = systems[i];
            Transform cardTr = defenseListContainer.Find($"DefenseCard_{sys.id}");
            if (cardTr == null) continue;

            TMP_Text levelTxt = cardTr.Find("LevelTxt")?.GetComponent<TMP_Text>();
            if (levelTxt != null)
            {
                levelTxt.text = sys.level >= sys.maxLevel ? "<color=#FFD700>МАКС</color>" : $"Ур. {sys.level}/{sys.maxLevel}";
            }

            Button upgBtn = cardTr.Find("UpgradeBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = upgBtn != null ? upgBtn.GetComponentInChildren<TMP_Text>() : null;
            if (upgBtn != null && btnTxt != null)
            {
                string sysId = sys.id;
                upgBtn.onClick.RemoveAllListeners();
                upgBtn.onClick.AddListener(() => TryUpgradeSystem(sysId));

                if (sys.level >= sys.maxLevel)
                {
                    upgBtn.interactable = false;
                    btnTxt.text = "МАКСИМУМ";
                }
                else
                {
                    double costMoney = sys.GetCostMoney();
                    double costCode = sys.GetCostCode();
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= costMoney &&
                                     GameManager.Instance.CodeLines >= costCode;
                    upgBtn.interactable = canAfford;
                    btnTxt.text = $"УСИЛИТЬ\n{NumberFormatter.Format(costMoney)} ₽ | {NumberFormatter.Format(costCode)} <color=#00E5FF>Кода</color>";
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
        EnsureThreatBanner(canvas);

        // Корневой объект модального окна
        GameObject root = new GameObject("CyberSecurityModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.8f, 0.9f, 0.6f);
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
        headerTxt.text = "🛡️ КИБЕРБЕЗОПАСНОСТЬ СТУДИИ";
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

        // Active Threat Box
        GameObject threatBox = new GameObject("ThreatBox", typeof(RectTransform), typeof(Image));
        threatBox.transform.SetParent(cardObj.transform, false);
        RectTransform threatRt = threatBox.GetComponent<RectTransform>();
        threatRt.anchorMin = new Vector2(0, 1);
        threatRt.anchorMax = new Vector2(1, 1);
        threatRt.pivot = new Vector2(0.5f, 1);
        threatRt.anchoredPosition = new Vector2(0, -52);
        threatRt.sizeDelta = new Vector2(-36, 100);
        threatBox.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.22f, 0.9f);

        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(threatBox.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -6);
        titleRt.sizeDelta = new Vector2(-16, 36);
        activeThreatTitleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        activeThreatTitleTxt.fontSize = 11;
        activeThreatTitleTxt.alignment = TextAlignmentOptions.Center;
        activeThreatTitleTxt.color = Color.white;

        GameObject timerObj = new GameObject("Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
        timerObj.transform.SetParent(threatBox.transform, false);
        RectTransform timerRt = timerObj.GetComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0, 0);
        timerRt.anchorMax = new Vector2(1, 0);
        timerRt.pivot = new Vector2(0.5f, 0);
        timerRt.anchoredPosition = new Vector2(0, 8);
        timerRt.sizeDelta = new Vector2(-16, 20);
        activeThreatTimerTxt = timerObj.GetComponent<TextMeshProUGUI>();
        activeThreatTimerTxt.fontSize = 11;
        activeThreatTimerTxt.alignment = TextAlignmentOptions.Center;
        activeThreatTimerTxt.color = new Color(0.85f, 0.9f, 0.95f);

        // Mitigate Action Button
        GameObject mitObj = new GameObject("MitigateBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        mitObj.transform.SetParent(cardObj.transform, false);
        RectTransform mitRt = mitObj.GetComponent<RectTransform>();
        mitRt.anchorMin = new Vector2(0, 1);
        mitRt.anchorMax = new Vector2(1, 1);
        mitRt.pivot = new Vector2(0.5f, 1);
        mitRt.anchoredPosition = new Vector2(0, -160);
        mitRt.sizeDelta = new Vector2(-36, 42);
        mitObj.GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.4f, 0.95f);
        mitigateAttackBtn = mitObj.GetComponent<Button>();

        GameObject mitTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        mitTxtObj.transform.SetParent(mitObj.transform, false);
        RectTransform mitTxtRt = mitTxtObj.GetComponent<RectTransform>();
        mitTxtRt.anchorMin = Vector2.zero; mitTxtRt.anchorMax = Vector2.one; mitTxtRt.sizeDelta = Vector2.zero;
        mitigateBtnTxt = mitTxtObj.GetComponent<TextMeshProUGUI>();
        mitigateBtnTxt.fontSize = 12;
        mitigateBtnTxt.fontStyle = FontStyles.Bold;
        mitigateBtnTxt.alignment = TextAlignmentOptions.Center;
        mitigateBtnTxt.color = Color.white;

        // Security Status Summary Box
        GameObject sumObj = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumObj.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -210);
        sumRt.sizeDelta = new Vector2(-36, 32);
        sumObj.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.85f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumObj.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        securityStatusSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        securityStatusSummaryTxt.fontSize = 10;
        securityStatusSummaryTxt.alignment = TextAlignmentOptions.Center;
        securityStatusSummaryTxt.color = new Color(0.85f, 0.95f, 1f);

        // Scroll Container for Defense Systems
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -250);
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
        defenseListContainer = contentObj.transform;

        for (int i = 0; i < systems.Count; i++)
        {
            CreateDefenseCardTemplate(defenseListContainer, systems[i]);
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

    private void CreateDefenseCardTemplate(Transform parent, DefenseSystem sys)
    {
        GameObject card = new GameObject($"DefenseCard_{sys.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 70);
        card.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.95f);

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
        tt.text = $"{sys.icon} <b>{sys.title}</b>";
        tt.fontSize = 12;
        tt.color = new Color(0.9f, 0.95f, 1f);

        // Level text
        GameObject lvlObj = new GameObject("LevelTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        lvlObj.transform.SetParent(card.transform, false);
        RectTransform lvlRt = lvlObj.GetComponent<RectTransform>();
        lvlRt.anchorMin = new Vector2(0.65f, 1);
        lvlRt.anchorMax = new Vector2(1, 1);
        lvlRt.pivot = new Vector2(1, 1);
        lvlRt.anchoredPosition = new Vector2(-10, -6);
        lvlRt.sizeDelta = new Vector2(0, 20);
        TMP_Text lt = lvlObj.GetComponent<TextMeshProUGUI>();
        lt.text = $"Ур. {sys.level}/{sys.maxLevel}";
        lt.fontSize = 11;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = new Color(0.2f, 0.9f, 1f);

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.6f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"<color=#80A0C0>{sys.description}</color>";
        dt.fontSize = 10;

        // Upgrade button
        GameObject b = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, -4);
        brt.sizeDelta = new Vector2(160, 36);
        b.GetComponent<Image>().color = new Color(0.15f, 0.45f, 0.4f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "УСИЛИТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureThreatBanner(Canvas canvas)
    {
        if (threatBannerRoot != null) return;

        GameObject bannerObj = new GameObject("CyberThreatAlertBanner", typeof(RectTransform), typeof(Image), typeof(Button));
        bannerObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = bannerObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0, -132);
        rt.sizeDelta = new Vector2(460, 34);
        bannerObj.GetComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f, 0.95f);
        threatBannerBtn = bannerObj.GetComponent<Button>();

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(bannerObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        threatBannerTxt = txtObj.GetComponent<TextMeshProUGUI>();
        threatBannerTxt.text = "🚨 DDoS-АТАКА! НАЖМИТЕ ДЛЯ ОТРАЖЕНИЯ!";
        threatBannerTxt.fontSize = 11;
        threatBannerTxt.fontStyle = FontStyles.Bold;
        threatBannerTxt.alignment = TextAlignmentOptions.Center;
        threatBannerTxt.color = Color.white;

        threatBannerRoot = bannerObj;
        threatBannerRoot.SetActive(false);
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openSecurityBtn != null) return;

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

        GameObject btnGo = new GameObject("SecurityHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.12f, 0.35f, 0.3f, 0.9f);
        openSecurityBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🛡️ Защита";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
