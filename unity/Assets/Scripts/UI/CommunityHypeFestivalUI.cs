using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Краудфандинговый Стретч-Гол Фестиваль («Community Hype Festival»):
/// - 4 фестивальных стретч-гола: Артбук, Lo-Fi винил, Неоновое оформление, Золотой Монумент
/// - Накопление очков хайпа (Hype Points) через релизы, клики и кнопку «РАЗОГНАТЬ ХАЙП»
/// - Праздничные конфетти-попапы, фанфары и награды сообщества
/// - Перманентные множители к престижу, комбо и доходам компании
/// </summary>
public class CommunityHypeFestivalUI : MonoBehaviour
{
    private static CommunityHypeFestivalUI instance;
    public static CommunityHypeFestivalUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<CommunityHypeFestivalUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(CommunityHypeFestivalUI));
                    instance = go.AddComponent<CommunityHypeFestivalUI>();
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
    public class FestivalGoal
    {
        public string id;
        public string title;
        public string icon;
        public string description;
        public string perkDesc;
        public double targetHype;
        public double rewardMoney;
        public double rewardCode;
        public bool isUnlocked;
        public Color themeColor;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openFestivalBtn;

    [Header("Прогресс хайпа и цели")]
    [SerializeField] private TMP_Text totalHypeTxt;
    [SerializeField] private Image hypeProgressBarFill;
    [SerializeField] private TMP_Text festivalStatusSummaryTxt;
    [SerializeField] private Button generateHypeBtn;
    [SerializeField] private TMP_Text generateHypeBtnTxt;
    [SerializeField] private Transform goalsContainer;

    private readonly List<FestivalGoal> goals = new List<FestivalGoal>();
    private double currentHypePoints = 0;

    private const string PrefCurrentHype = "Festival_CurrentHype";
    private const string PrefGoalUnlockedPrefix = "Festival_GoalUnlocked_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public double CurrentHype => currentHypePoints;
    public IReadOnlyList<FestivalGoal> Goals => goals;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeGoals();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeGoals()
    {
        goals.Clear();

        goals.Add(new FestivalGoal
        {
            id = "goal_artbook",
            title = "Цифровой Артбук 'The Art of HelloTap'",
            icon = "🎨",
            description = "Коллекция концепт-артов, ранних скетчей уточки и интерьеров офиса.",
            perkDesc = "+15% к очкам престижа при выходе на IPO",
            targetHype = 2500,
            rewardMoney = 35000,
            rewardCode = 5000,
            isUnlocked = false,
            themeColor = new Color(0.9f, 0.4f, 1f)
        });

        goals.Add(new FestivalGoal
        {
            id = "goal_lofi_ep",
            title = "Виниловый Lo-Fi EP Саундтрек",
            icon = "🎵",
            description = "Уютные авторские треки для ночного кодинга от композитора студии.",
            perkDesc = "+20% к длительности комбо и энергии",
            targetHype = 8000,
            rewardMoney = 90000,
            rewardCode = 15000,
            isUnlocked = false,
            themeColor = new Color(0.2f, 0.85f, 1f)
        });

        goals.Add(new FestivalGoal
        {
            id = "goal_neon_skin",
            title = "Неоновый Стиль 'Cyber Glow'",
            icon = "✨",
            description = "Атмосферная неоновая подсветка рабочего места и монитора.",
            perkDesc = "+25% к пассивному доходу студии в секунду",
            targetHype = 20000,
            rewardMoney = 200000,
            rewardCode = 35000,
            isUnlocked = false,
            themeColor = new Color(0.2f, 1f, 0.5f)
        });

        goals.Add(new FestivalGoal
        {
            id = "goal_golden_statue",
            title = "Монумент Сообщества 'Golden Dev'",
            icon = "🏆",
            description = "Золотой монумент в честь фанатов и разработчиков инди-экосистемы.",
            perkDesc = "+35% ко всем денежным доходам студии перманентно",
            targetHype = 50000,
            rewardMoney = 500000,
            rewardCode = 100000,
            isUnlocked = false,
            themeColor = new Color(1f, 0.84f, 0f)
        });
    }

    private void LoadData()
    {
        double.TryParse(PlayerPrefs.GetString(PrefCurrentHype, "0"), out currentHypePoints);
        for (int i = 0; i < goals.Count; i++)
        {
            goals[i].isUnlocked = PlayerPrefs.GetInt(PrefGoalUnlockedPrefix + goals[i].id, 0) == 1;
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(PrefCurrentHype, currentHypePoints.ToString("F0"));
        for (int i = 0; i < goals.Count; i++)
        {
            PlayerPrefs.SetInt(PrefGoalUnlockedPrefix + goals[i].id, goals[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    public double GetHypeFestivalMultiplier()
    {
        double mult = 1.0;
        var statue = goals.Find(g => g.id == "goal_golden_statue");
        if (statue != null && statue.isUnlocked) mult += 0.35;

        var neon = goals.Find(g => g.id == "goal_neon_skin");
        if (neon != null && neon.isUnlocked) mult += 0.25;

        var art = goals.Find(g => g.id == "goal_artbook");
        if (art != null && art.isUnlocked) mult += 0.15;

        return mult;
    }

    public void AddHype(double amount)
    {
        currentHypePoints += amount;
        CheckGoalUnlocks();
        SaveData();
        UpdateModalUI();
    }

    public void GenerateHypeBoost()
    {
        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        double addedHype = Math.Max(25.0, (GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() * 2.0 : 50.0));
        currentHypePoints += addedHype;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddComboEnergy(0.12f);
        }

        CheckGoalUnlocks();
        SaveData();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 +{NumberFormatter.Format(addedHype)} ОЧКОВ ХАЙПА!", transform.position + Vector3.up * 25f, new Color(1f, 0.6f, 0.1f), false);
        }

        UpdateModalUI();
    }

    private void CheckGoalUnlocks()
    {
        for (int i = 0; i < goals.Count; i++)
        {
            var g = goals[i];
            if (!g.isUnlocked && currentHypePoints >= g.targetHype)
            {
                g.isUnlocked = true;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddMoney(g.rewardMoney);
                    GameManager.Instance.AddLinesOfCode(g.rewardCode);
                }

                HapticFeedback.SuccessPattern();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"🎊 СТРЕТЧ-ГОЛ ФЕСТИВАЛЯ ВЗЯТ!\n{g.icon} {g.title}\nНаграда: <b>+{NumberFormatter.Format(g.rewardMoney)} ₽</b> (+{NumberFormatter.Format(g.rewardCode)} кода)", transform.position, g.themeColor, true);
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
        if (openFestivalBtn != null)
        {
            openFestivalBtn.onClick.RemoveAllListeners();
            openFestivalBtn.onClick.AddListener(OpenModal);
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
        if (generateHypeBtn != null)
        {
            generateHypeBtn.onClick.RemoveAllListeners();
            generateHypeBtn.onClick.AddListener(GenerateHypeBoost);
        }
    }

    private void UpdateModalUI()
    {
        double maxTarget = goals[goals.Count - 1].targetHype;

        if (totalHypeTxt != null)
        {
            totalHypeTxt.text = $"Хайп сообщества: <b><color=#FFD700>{NumberFormatter.Format(currentHypePoints)}</color></b> / {NumberFormatter.Format(maxTarget)} Hype Points";
        }

        if (hypeProgressBarFill != null && maxTarget > 0)
        {
            hypeProgressBarFill.fillAmount = Mathf.Clamp01((float)(currentHypePoints / maxTarget));
        }

        if (festivalStatusSummaryTxt != null)
        {
            double multBonus = (GetHypeFestivalMultiplier() - 1.0) * 100.0;
            int unlockedCount = 0;
            for (int i = 0; i < goals.Count; i++) if (goals[i].isUnlocked) unlockedCount++;
            festivalStatusSummaryTxt.text = $"Стретч-голов взято: <b><color=#00E5FF>{unlockedCount}/{goals.Count}</color></b> | Суммарный бонус: <b><color=#00FF88>+{multBonus:F0}%</color></b> ко всем доходам";
        }

        RefreshGoalCards();
    }

    private void RefreshGoalCards()
    {
        if (goalsContainer == null) return;

        for (int i = 0; i < goals.Count; i++)
        {
            var g = goals[i];
            Transform cardTr = goalsContainer.Find($"GoalCard_{g.id}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            TMP_Text statTxt = cardTr.Find("StatusTxt")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = g.isUnlocked 
                    ? new Color(0.18f, 0.16f, 0.10f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.14f, 0.92f);
            }

            if (statTxt != null)
            {
                if (g.isUnlocked)
                {
                    statTxt.text = "<color=#FFD700>★ РАЗБЛОКИРОВАНО</color>";
                }
                else
                {
                    double remaining = Math.Max(0, g.targetHype - currentHypePoints);
                    statTxt.text = $"<color=#00E5FF>{NumberFormatter.Format(g.targetHype)} Хайпа</color>\n<size=9>Осталось: {NumberFormatter.Format(remaining)}</size>";
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
        GameObject root = new GameObject("CommunityHypeFestivalModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.6f, 0.2f, 0.6f);
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
        headerTxt.text = "🎉 КРАУДФАНДИНГ ХАЙП-ФЕСТИВАЛЬ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.75f, 0.2f);

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

        // Hype Summary Box
        GameObject sumBox = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumBox.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumBox.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 76);
        sumBox.GetComponent<Image>().color = new Color(0.14f, 0.12f, 0.18f, 0.9f);

        GameObject hypeTxtObj = new GameObject("TotalHypeTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        hypeTxtObj.transform.SetParent(sumBox.transform, false);
        RectTransform hypeRt = hypeTxtObj.GetComponent<RectTransform>();
        hypeRt.anchorMin = new Vector2(0, 1);
        hypeRt.anchorMax = new Vector2(1, 1);
        hypeRt.pivot = new Vector2(0.5f, 1);
        hypeRt.anchoredPosition = new Vector2(0, -6);
        hypeRt.sizeDelta = new Vector2(-16, 20);
        totalHypeTxt = hypeTxtObj.GetComponent<TextMeshProUGUI>();
        totalHypeTxt.fontSize = 11;
        totalHypeTxt.alignment = TextAlignmentOptions.Center;
        totalHypeTxt.color = new Color(1f, 0.95f, 0.8f);

        GameObject festSumObj = new GameObject("FestSummary", typeof(RectTransform), typeof(TextMeshProUGUI));
        festSumObj.transform.SetParent(sumBox.transform, false);
        RectTransform festRt = festSumObj.GetComponent<RectTransform>();
        festRt.anchorMin = new Vector2(0, 0);
        festRt.anchorMax = new Vector2(1, 0);
        festRt.pivot = new Vector2(0.5f, 0);
        festRt.anchoredPosition = new Vector2(0, 8);
        festRt.sizeDelta = new Vector2(-16, 20);
        festivalStatusSummaryTxt = festSumObj.GetComponent<TextMeshProUGUI>();
        festivalStatusSummaryTxt.fontSize = 10;
        festivalStatusSummaryTxt.alignment = TextAlignmentOptions.Center;
        festivalStatusSummaryTxt.color = new Color(0.85f, 0.95f, 1f);

        // Generate Hype Button
        GameObject hypeBtnObj = new GameObject("GenerateHypeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        hypeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform hypeBtnRt = hypeBtnObj.GetComponent<RectTransform>();
        hypeBtnRt.anchorMin = new Vector2(0, 1);
        hypeBtnRt.anchorMax = new Vector2(1, 1);
        hypeBtnRt.pivot = new Vector2(0.5f, 1);
        hypeBtnRt.anchoredPosition = new Vector2(0, -136);
        hypeBtnRt.sizeDelta = new Vector2(-36, 40);
        hypeBtnObj.GetComponent<Image>().color = new Color(0.9f, 0.55f, 0.1f, 0.95f);
        generateHypeBtn = hypeBtnObj.GetComponent<Button>();

        GameObject hypeBtnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        hypeBtnTxtObj.transform.SetParent(hypeBtnObj.transform, false);
        RectTransform hbtRt = hypeBtnTxtObj.GetComponent<RectTransform>();
        hbtRt.anchorMin = Vector2.zero; hbtRt.anchorMax = Vector2.one; hbtRt.sizeDelta = Vector2.zero;
        generateHypeBtnTxt = hypeBtnTxtObj.GetComponent<TextMeshProUGUI>();
        generateHypeBtnTxt.fontSize = 11;
        generateHypeBtnTxt.fontStyle = FontStyles.Bold;
        generateHypeBtnTxt.alignment = TextAlignmentOptions.Center;
        generateHypeBtnTxt.text = "🔥 РАЗОГНАТЬ ХАЙП ФЕСТИВАЛЯ (+HYPE BOOST)";
        generateHypeBtnTxt.color = Color.white;

        // Scroll Container for Goals
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -186);
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
        goalsContainer = contentObj.transform;

        for (int i = 0; i < goals.Count; i++)
        {
            CreateGoalCardTemplate(goalsContainer, goals[i]);
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

    private void CreateGoalCardTemplate(Transform parent, FestivalGoal goal)
    {
        GameObject card = new GameObject($"GoalCard_{goal.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 72);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.92f);

        // Title and icon
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(0.68f, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(0, 20);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{goal.icon} <b>{goal.title}</b>";
        tt.fontSize = 12;
        tt.color = goal.themeColor;

        // Status text
        GameObject statObj = new GameObject("StatusTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statObj.transform.SetParent(card.transform, false);
        RectTransform statRt = statObj.GetComponent<RectTransform>();
        statRt.anchorMin = new Vector2(0.68f, 0.5f);
        statRt.anchorMax = new Vector2(1, 0.5f);
        statRt.pivot = new Vector2(1, 0.5f);
        statRt.anchoredPosition = new Vector2(-10, 0);
        statRt.sizeDelta = new Vector2(0, 36);
        TMP_Text st = statObj.GetComponent<TextMeshProUGUI>();
        st.fontSize = 10;
        st.alignment = TextAlignmentOptions.Right;

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.68f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{goal.perkDesc}</color>";
        dt.fontSize = 10;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openFestivalBtn != null) return;

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

        GameObject btnGo = new GameObject("FestivalHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.45f, 0.25f, 0.1f, 0.9f);
        openFestivalBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🎉 Фестиваль";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
