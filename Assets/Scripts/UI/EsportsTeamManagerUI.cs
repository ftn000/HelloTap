using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Киберспортивная Команда Студии («Indie Esports Clan & Tournaments»):
/// - 4 ключевые роли в ростере: Капитан (IGL), Энтри-фраггер, Саппорт-аналитик, Медиа-стример
/// - Участие в турнирах: Инди-Ланка -> Кубок СНГ -> Major World Championship
/// - Интерактивная симуляция матчей «СЫГРАТЬ ТУРНИРНЫЙ МАТЧ» с призовыми фондами
/// - Перманентные баффы к спонсорским контрактам, клику и хайпу студии
/// </summary>
public class EsportsTeamManagerUI : MonoBehaviour
{
    private static EsportsTeamManagerUI instance;
    public static EsportsTeamManagerUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<EsportsTeamManagerUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(EsportsTeamManagerUI));
                    instance = go.AddComponent<EsportsTeamManagerUI>();
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
    public class RosterMember
    {
        public string id;
        public string roleName;
        public string nickname;
        public string icon;
        public string description;
        public string perkDesc;
        public int level;
        public int maxLevel;
        public double baseCostMoney;
        public double baseCostCode;

        public double GetCostMoney() => Math.Floor(baseCostMoney * Math.Pow(1.45, level));
        public double GetCostCode() => Math.Floor(baseCostCode * Math.Pow(1.40, level));
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openEsportsBtn;

    [Header("Турнирный блок и матч")]
    [SerializeField] private TMP_Text esportsSummaryTxt;
    [SerializeField] private Button playMatchBtn;
    [SerializeField] private TMP_Text playMatchBtnTxt;
    [SerializeField] private Transform rosterContainer;

    private readonly List<RosterMember> roster = new List<RosterMember>();
    private int currentTournamentTier = 1; // 1, 2, 3
    private bool isMatchRunning = false;

    private const string PrefTournamentTier = "Esports_TournamentTier";
    private const string PrefRosterPrefix = "Esports_RosterLvl_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeRoster();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeRoster()
    {
        roster.Clear();

        roster.Add(new RosterMember
        {
            id = "role_captain",
            roleName = "Капитан & IGL",
            nickname = "f0rest_duck",
            icon = "👑",
            description = "Тактические коллы, координация смоков и таймингов",
            perkDesc = "+4% к доходу от релизов за уровень",
            level = 1,
            maxLevel = 10,
            baseCostMoney = 6000,
            baseCostCode = 1500
        });

        roster.Add(new RosterMember
        {
            id = "role_fragger",
            roleName = "Энтри-Фраггер",
            nickname = "s1mple_tap",
            icon = "🎯",
            description = "Безупречная реакция, ван-тапы и опен-фраги",
            perkDesc = "+3% строк кода за клик за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 15000,
            baseCostCode = 3500
        });

        roster.Add(new RosterMember
        {
            id = "role_support",
            roleName = "Саппорт / Аналитик",
            nickname = "cyber_coach",
            icon = "🧠",
            description = "Анализ демок противников и разбор ошибок",
            perkDesc = "+4% к скорости релизов за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 30000,
            baseCostCode = 7000
        });

        roster.Add(new RosterMember
        {
            id = "role_media",
            roleName = "Медиа-Стример",
            nickname = "hype_streamer",
            icon = "🎙️",
            description = "Трансляции кланваров на Twitch и спонсорские контракты",
            perkDesc = "+5% к пассивному доходу за уровень",
            level = 0,
            maxLevel = 10,
            baseCostMoney = 60000,
            baseCostCode = 14000
        });
    }

    private void LoadData()
    {
        currentTournamentTier = PlayerPrefs.GetInt(PrefTournamentTier, 1);
        for (int i = 0; i < roster.Count; i++)
        {
            int defLvl = roster[i].id == "role_captain" ? 1 : 0;
            roster[i].level = PlayerPrefs.GetInt(PrefRosterPrefix + roster[i].id, defLvl);
            roster[i].level = Mathf.Clamp(roster[i].level, 0, roster[i].maxLevel);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefTournamentTier, currentTournamentTier);
        for (int i = 0; i < roster.Count; i++)
        {
            PlayerPrefs.SetInt(PrefRosterPrefix + roster[i].id, roster[i].level);
        }
        PlayerPrefs.Save();
    }

    public double GetEsportsIncomeMultiplier()
    {
        var media = roster.Find(r => r.id == "role_media");
        int mLvl = media != null ? media.level : 0;
        var cap = roster.Find(r => r.id == "role_captain");
        int cLvl = cap != null ? cap.level : 0;
        return 1.0 + (mLvl * 0.05) + (cLvl * 0.04) + (currentTournamentTier - 1) * 0.08;
    }

    public double GetEsportsClickMultiplier()
    {
        var frag = roster.Find(r => r.id == "role_fragger");
        int fLvl = frag != null ? frag.level : 0;
        return 1.0 + (fLvl * 0.03);
    }

    public string GetTournamentName()
    {
        if (currentTournamentTier >= 3) return "🏆 Major World Championship (Финал Мира)";
        if (currentTournamentTier >= 2) return "🥈 Кубок СНГ по Кибердеву";
        return "🥉 Городская Инди-Ланка";
    }

    public void PlayTournamentMatch()
    {
        if (isMatchRunning) return;
        StartCoroutine(MatchSimulationRoutine());
    }

    private IEnumerator MatchSimulationRoutine()
    {
        isMatchRunning = true;
        if (playMatchBtn != null) playMatchBtn.interactable = false;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (playMatchBtnTxt != null)
        {
            playMatchBtnTxt.text = "⚔️ ИДЕТ МАТЧ: Раунд 12:11... Борьба за плент!";
        }

        yield return new WaitForSecondsRealtime(1.3f);

        double baseMoney = currentTournamentTier == 3 ? 180000.0 : (currentTournamentTier == 2 ? 65000.0 : 20000.0);
        double rewardMoney = baseMoney * (1.0 + roster[0].level * 0.1);
        double rewardCode = Math.Floor(rewardMoney * 0.08);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardMoney);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.AddComboEnergy(0.25f);
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        // Проверяем повышение тира турнира
        int minLvl = 99;
        for (int i = 0; i < roster.Count; i++) if (roster[i].level < minLvl) minLvl = roster[i].level;
        if (minLvl >= 5 && currentTournamentTier < 3) currentTournamentTier = 3;
        else if (minLvl >= 2 && currentTournamentTier < 2) currentTournamentTier = 2;
        SaveData();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏆 ПОБЕДА 16:12 В ТУРНИРЕ!\nПризовые: <b>+{NumberFormatter.Format(rewardMoney)} ₽</b> (+{NumberFormatter.Format(rewardCode)} кода)", transform.position, new Color(1f, 0.84f, 0.2f), true);
        }

        isMatchRunning = false;
        if (playMatchBtn != null) playMatchBtn.interactable = true;
        if (playMatchBtnTxt != null) playMatchBtnTxt.text = "⚔️ СЫГРАТЬ ТУРНИРНЫЙ МАТЧ КЛАНОМ";
        UpdateModalUI();
    }

    public bool TryUpgradeMember(string memberId)
    {
        var m = roster.Find(x => x.id == memberId);
        if (m == null || m.level >= m.maxLevel) return false;

        double costMoney = m.GetCostMoney();
        double costCode = m.GetCostCode();

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

        m.level++;
        SaveData();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎯 ИГРОК ПРОКАЧАН!\n{m.icon} {m.nickname} ({m.roleName}) -> ур. {m.level}", transform.position, new Color(1f, 0.85f, 0.2f), true);
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
        if (openEsportsBtn != null)
        {
            openEsportsBtn.onClick.RemoveAllListeners();
            openEsportsBtn.onClick.AddListener(OpenModal);
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
        if (playMatchBtn != null)
        {
            playMatchBtn.onClick.RemoveAllListeners();
            playMatchBtn.onClick.AddListener(PlayTournamentMatch);
        }
    }

    private void UpdateModalUI()
    {
        if (esportsSummaryTxt != null)
        {
            double incB = (GetEsportsIncomeMultiplier() - 1.0) * 100.0;
            double clkB = (GetEsportsClickMultiplier() - 1.0) * 100.0;
            esportsSummaryTxt.text = $"Турнир: <b><color=#FFD700>{GetTournamentName()}</color></b>\nДоход: <b><color=#00FF88>+{incB:F0}%</color></b> | Клик: <b><color=#00E5FF>+{clkB:F0}%</color></b>";
        }

        RefreshRosterCards();
    }

    private void RefreshRosterCards()
    {
        if (rosterContainer == null) return;

        for (int i = 0; i < roster.Count; i++)
        {
            var m = roster[i];
            Transform cardTr = rosterContainer.Find($"RosterCard_{m.id}");
            if (cardTr == null) continue;

            TMP_Text lvlTxt = cardTr.Find("LevelTxt")?.GetComponent<TMP_Text>();
            if (lvlTxt != null)
            {
                lvlTxt.text = m.level >= m.maxLevel ? "<color=#FFD700>МАКС</color>" : $"Ур. {m.level}/{m.maxLevel}";
            }

            Button upgBtn = cardTr.Find("UpgradeBtn")?.GetComponent<Button>();
            TMP_Text btnTxt = upgBtn != null ? upgBtn.GetComponentInChildren<TMP_Text>() : null;

            if (upgBtn != null && btnTxt != null)
            {
                string mId = m.id;
                upgBtn.onClick.RemoveAllListeners();
                upgBtn.onClick.AddListener(() => TryUpgradeMember(mId));

                if (m.level >= m.maxLevel)
                {
                    upgBtn.interactable = false;
                    btnTxt.text = "МАКС. УРОВЕНЬ";
                }
                else
                {
                    double costMoney = m.GetCostMoney();
                    double costCode = m.GetCostCode();
                    bool canAfford = GameManager.Instance != null &&
                                     GameManager.Instance.Money >= costMoney &&
                                     GameManager.Instance.CodeLines >= costCode;
                    upgBtn.interactable = canAfford;
                    btnTxt.text = $"ТРЕНИРОВАТЬ\n{NumberFormatter.Format(costMoney)} ₽ | {NumberFormatter.Format(costCode)} Кода";
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
        GameObject root = new GameObject("EsportsTeamModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.09f, 0.08f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.75f, 0.1f, 0.6f);
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
        headerTxt.text = "🏆 КИБЕРСПОРТИВНЫЙ КЛАН СТУДИИ";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.85f, 0.2f);

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

        // Esports Summary Box
        GameObject sumBox = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumBox.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumBox.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 60);
        sumBox.GetComponent<Image>().color = new Color(0.14f, 0.12f, 0.18f, 0.9f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumBox.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        esportsSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        esportsSummaryTxt.fontSize = 11;
        esportsSummaryTxt.alignment = TextAlignmentOptions.Center;
        esportsSummaryTxt.color = new Color(1f, 0.95f, 0.8f);

        // Play Match Action Button
        GameObject matchObj = new GameObject("PlayMatchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        matchObj.transform.SetParent(cardObj.transform, false);
        RectTransform matchRt = matchObj.GetComponent<RectTransform>();
        matchRt.anchorMin = new Vector2(0, 1);
        matchRt.anchorMax = new Vector2(1, 1);
        matchRt.pivot = new Vector2(0.5f, 1);
        matchRt.anchoredPosition = new Vector2(0, -120);
        matchRt.sizeDelta = new Vector2(-36, 40);
        matchObj.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.1f, 0.95f);
        playMatchBtn = matchObj.GetComponent<Button>();

        GameObject matchTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        matchTxtObj.transform.SetParent(matchObj.transform, false);
        RectTransform matchTxtRt = matchTxtObj.GetComponent<RectTransform>();
        matchTxtRt.anchorMin = Vector2.zero; matchTxtRt.anchorMax = Vector2.one; matchTxtRt.sizeDelta = Vector2.zero;
        playMatchBtnTxt = matchTxtObj.GetComponent<TextMeshProUGUI>();
        playMatchBtnTxt.fontSize = 11;
        playMatchBtnTxt.fontStyle = FontStyles.Bold;
        playMatchBtnTxt.alignment = TextAlignmentOptions.Center;
        playMatchBtnTxt.text = "⚔️ СЫГРАТЬ ТУРНИРНЫЙ МАТЧ КЛАНОМ";
        playMatchBtnTxt.color = Color.white;

        // Scroll Container for Roster
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -170);
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
        rosterContainer = contentObj.transform;

        for (int i = 0; i < roster.Count; i++)
        {
            CreateRosterCardTemplate(rosterContainer, roster[i]);
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

    private void CreateRosterCardTemplate(Transform parent, RosterMember m)
    {
        GameObject card = new GameObject($"RosterCard_{m.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 72);
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
        tt.text = $"{m.icon} <b>{m.nickname}</b> <color=#90B0D0>[{m.roleName}]</color>";
        tt.fontSize = 12;
        tt.color = new Color(1f, 0.85f, 0.3f);

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
        lt.text = $"Ур. {m.level}/{m.maxLevel}";
        lt.fontSize = 11;
        lt.alignment = TextAlignmentOptions.Right;
        lt.color = new Color(0.2f, 0.85f, 1f);

        // Desc text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.62f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Эффект: <color=#00FF88>{m.perkDesc}</color>";
        dt.fontSize = 10;

        // Upgrade button
        GameObject b = new GameObject("UpgradeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-8, -2);
        brt.sizeDelta = new Vector2(150, 36);
        b.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.65f, 0.95f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ТРЕНИРОВАТЬ";
        btxt.fontSize = 9;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openEsportsBtn != null) return;

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

        GameObject btnGo = new GameObject("EsportsHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.45f, 0.35f, 0.1f, 0.9f);
        openEsportsBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🏆 Киберспорт";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
