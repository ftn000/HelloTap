using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Мастерская сообщества и Моддинг (Steam Workshop & Modding API):
/// - Фанатские модификации для игр студии (HD текстуры, Сюжетные аддоны, Стримерские войспаки, Спидран-утилиты)
/// - Модерация и официальный аппрув модов разработчиками студии
/// - Конкурсы мододелов (Mod Contests) и рост сообщества
/// - Глобальные баффы к продажам, клику и пассивному доходу студии
/// </summary>
public class SteamWorkshopModdingUI : MonoBehaviour
{
    private static SteamWorkshopModdingUI instance;
    public static SteamWorkshopModdingUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SteamWorkshopModdingUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class WorkshopMod
    {
        public string id;
        public string title;
        public string author;
        public string category;
        public string icon;
        public string perkDescription;
        public int subscribers;
        public float rating;
        public double approvalCost; // Стоимость верификации/тестирования QA
        public double incomeBonus;  // Множитель дохода (например 0.15 = +15%)
        public double clickBonus;   // Множитель клика (например 0.20 = +20%)
        public bool isApproved;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openWorkshopBtn;
    [SerializeField] private TMP_Text openWorkshopBtnText;
    [SerializeField] private Button closeWorkshopBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка мастерской")]
    [SerializeField] private TMP_Text totalApprovedModsText;
    [SerializeField] private TMP_Text communitySubsText;
    [SerializeField] private TMP_Text workshopBonusText;

    [Header("Контейнер модов")]
    [SerializeField] private Transform modsContainer;

    private readonly List<WorkshopMod> mods = new List<WorkshopMod>();
    private int workshopLevel = 1;

    private const string PrefApprovedPrefix = "Workshop_Approved_";
    private const string PrefSubsPrefix = "Workshop_Subs_";
    private const string PrefLevelKey = "Workshop_Level";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<WorkshopMod> Mods => mods;

    public double GetWorkshopIncomeMultiplier()
    {
        double bonus = 0;
        for (int i = 0; i < mods.Count; i++)
        {
            if (mods[i].isApproved)
            {
                bonus += mods[i].incomeBonus;
            }
        }
        return 1.0 + bonus;
    }

    public double GetWorkshopClickMultiplier()
    {
        double bonus = 0;
        for (int i = 0; i < mods.Count; i++)
        {
            if (mods[i].isApproved)
            {
                bonus += mods[i].clickBonus;
            }
        }
        return 1.0 + bonus;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeMods();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeMods()
    {
        mods.Clear();

        mods.Add(new WorkshopMod
        {
            id = "mod_raytracing",
            title = "Ultra HD 4K Remaster & Raytracing",
            author = "GraphicsWizard",
            category = "Графика и Шейдеры",
            icon = "✨",
            perkDescription = "+20% к доходу со всех выпущенных игр студии",
            subscribers = 42500,
            rating = 4.9f,
            approvalCost = 75000.0,
            incomeBonus = 0.20,
            clickBonus = 0.0,
            isApproved = false
        });

        mods.Add(new WorkshopMod
        {
            id = "mod_fan_story",
            title = "The Lost DLC: Community Questline",
            author = "LoreMaster99",
            category = "Сюжет и Квесты",
            icon = "📜",
            perkDescription = "+15% к глобальному множителю дохода студии",
            subscribers = 28100,
            rating = 4.8f,
            approvalCost = 150000.0,
            incomeBonus = 0.15,
            clickBonus = 0.05,
            isApproved = false
        });

        mods.Add(new WorkshopMod
        {
            id = "mod_vtuber_voices",
            title = "Streamer & VTuber Sound Pack",
            author = "MemeAudioCorp",
            category = "Звуки и Озвучка",
            icon = "🎙️",
            perkDescription = "+30% к объему кода за клик при активной игре",
            subscribers = 65000,
            rating = 4.7f,
            approvalCost = 350000.0,
            incomeBonus = 0.05,
            clickBonus = 0.30,
            isApproved = false
        });

        mods.Add(new WorkshopMod
        {
            id = "mod_speedrun_tools",
            title = "Pro Speedrun HUD & Live Splits",
            author = "AnyPercentKing",
            category = "Интерфейс и Утилиты",
            icon = "⏱️",
            perkDescription = "+25% к приросту комбо-потока и скорости кодинга",
            subscribers = 51200,
            rating = 5.0f,
            approvalCost = 850000.0,
            incomeBonus = 0.10,
            clickBonus = 0.25,
            isApproved = false
        });
    }

    private void LoadData()
    {
        workshopLevel = PlayerPrefs.GetInt(PrefLevelKey, 1);

        for (int i = 0; i < mods.Count; i++)
        {
            mods[i].isApproved = PlayerPrefs.GetInt(PrefApprovedPrefix + mods[i].id, 0) == 1;
            mods[i].subscribers = PlayerPrefs.GetInt(PrefSubsPrefix + mods[i].id, mods[i].subscribers);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefLevelKey, workshopLevel);

        for (int i = 0; i < mods.Count; i++)
        {
            PlayerPrefs.SetInt(PrefApprovedPrefix + mods[i].id, mods[i].isApproved ? 1 : 0);
            PlayerPrefs.SetInt(PrefSubsPrefix + mods[i].id, mods[i].subscribers);
        }
        PlayerPrefs.Save();
    }

    public void ApproveMod(int index)
    {
        if (index < 0 || index >= mods.Count) return;
        var mod = mods[index];
        if (mod.isApproved) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < mod.approvalCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств на QA-верификацию ({NumberFormatter.Format(mod.approvalCost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(mod.approvalCost);
        mod.isApproved = true;
        mod.subscribers += UnityEngine.Random.Range(5000, 15000);
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🛠️ МОД ОДОБРЕН В МАСТЕРСКОЙ!\n{mod.icon} {mod.title}\n{mod.perkDescription}", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }

        UpdateModalUI();
    }

    public void HostModdingContest()
    {
        double contestCost = 200000.0 * workshopLevel;
        if (GameManager.Instance == null || GameManager.Instance.Money < contestCost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Призовой фонд конкурса: {NumberFormatter.Format(contestCost)} ₽!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(contestCost);
        workshopLevel++;

        for (int i = 0; i < mods.Count; i++)
        {
            mods[i].subscribers += UnityEngine.Random.Range(8000, 20000);
        }

        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPrestige();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏆 ХАКАТОН МОДОДЕЛОВ ПРОВЕДЕН!\nМастерская ур. {workshopLevel} | +50 000 новых подписчиков сообщества!", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openWorkshopBtn != null)
        {
            openWorkshopBtn.onClick.RemoveAllListeners();
            openWorkshopBtn.onClick.AddListener(OpenModal);
        }
        if (closeWorkshopBtn != null)
        {
            closeWorkshopBtn.onClick.RemoveAllListeners();
            closeWorkshopBtn.onClick.AddListener(CloseModal);
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
        int approvedCount = 0;
        int totalSubs = 0;
        for (int i = 0; i < mods.Count; i++)
        {
            if (mods[i].isApproved) approvedCount++;
            totalSubs += mods[i].subscribers;
        }

        if (totalApprovedModsText != null)
        {
            totalApprovedModsText.text = $"Официально одобрено: <b><color=#00FF88>{approvedCount}/{mods.Count} модов</color></b>";
        }

        if (communitySubsText != null)
        {
            communitySubsText.text = $"Подписчиков Мастерской: <b><color=#44D0FF>{totalSubs:N0}</color></b>";
        }

        if (workshopBonusText != null)
        {
            double incMult = GetWorkshopIncomeMultiplier();
            double clkMult = GetWorkshopClickMultiplier();
            workshopBonusText.text = $"Баффы сообщества: <b><color=#FFD700>Доход x{incMult:F2}</color></b> | <b><color=#FF77AA>Клик x{clkMult:F2}</color></b>";
        }

        RefreshModsList();
    }

    private void RefreshModsList()
    {
        if (modsContainer == null) return;

        double currentMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;

        for (int i = 0; i < mods.Count; i++)
        {
            int idx = i;
            var mod = mods[i];
            Transform cardTr = modsContainer.Find($"ModCard_{idx}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button actBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text actBtnTxt = actBtn != null ? actBtn.GetComponentInChildren<TMP_Text>() : null;
            TMP_Text ratingTxt = cardTr.Find("RatingText")?.GetComponent<TMP_Text>();

            if (bg != null)
            {
                bg.color = mod.isApproved
                    ? new Color(0.12f, 0.18f, 0.24f, 0.95f)
                    : new Color(0.08f, 0.09f, 0.12f, 0.90f);
            }

            if (ratingTxt != null)
            {
                string status = mod.isApproved ? "<color=#00FF88>✓ В МАСТЕРСКОЙ</color>" : "<color=#FFCC00>⏳ На модерации</color>";
                ratingTxt.text = $"⭐ {mod.rating:F1} | 👥 {mod.subscribers:N0}\n{status}";
            }

            if (actBtn != null && actBtnTxt != null)
            {
                actBtn.onClick.RemoveAllListeners();

                if (mod.isApproved)
                {
                    actBtn.interactable = false;
                    actBtnTxt.text = "✓ ВНЕДРЕНО";
                    actBtn.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.28f);
                }
                else
                {
                    bool canAfford = currentMoney >= mod.approvalCost;
                    actBtn.interactable = canAfford;
                    actBtnTxt.text = $"ОДОБРИТЬ ({NumberFormatter.Format(mod.approvalCost)} ₽)";
                    actBtn.GetComponent<Image>().color = canAfford ? new Color(0.2f, 0.65f, 0.85f) : new Color(0.25f, 0.25f, 0.28f);
                    actBtn.onClick.AddListener(() => ApproveMod(idx));
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("SteamWorkshopModdingModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.7f, 1f, 0.5f);
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
        headerTxt.text = "🔧 МАСТЕРСКАЯ МОДДИНГА (WORKSHOP)";
        headerTxt.fontSize = 17;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.4f, 0.85f, 1f);

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

        // Info Summary Panel
        GameObject infoPanel = new GameObject("InfoPanel", typeof(RectTransform), typeof(Image));
        infoPanel.transform.SetParent(cardObj.transform, false);
        RectTransform infoRt = infoPanel.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0, 1);
        infoRt.anchorMax = new Vector2(1, 1);
        infoRt.pivot = new Vector2(0.5f, 1);
        infoRt.anchoredPosition = new Vector2(0, -60);
        infoRt.sizeDelta = new Vector2(-36, 75);
        infoPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f, 0.9f);

        GameObject modsObj = new GameObject("ModsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        modsObj.transform.SetParent(infoPanel.transform, false);
        RectTransform mRt = modsObj.GetComponent<RectTransform>();
        mRt.anchorMin = new Vector2(0, 0.66f);
        mRt.anchorMax = new Vector2(1, 1);
        mRt.offsetMin = new Vector2(12, 0);
        mRt.offsetMax = new Vector2(-12, -4);
        totalApprovedModsText = modsObj.GetComponent<TextMeshProUGUI>();
        totalApprovedModsText.fontSize = 12;

        GameObject subsObj = new GameObject("SubsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subsObj.transform.SetParent(infoPanel.transform, false);
        RectTransform sRt = subsObj.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0.33f);
        sRt.anchorMax = new Vector2(1, 0.66f);
        sRt.offsetMin = new Vector2(12, 0);
        sRt.offsetMax = new Vector2(-12, 0);
        communitySubsText = subsObj.GetComponent<TextMeshProUGUI>();
        communitySubsText.fontSize = 12;

        GameObject bonusObj = new GameObject("BonusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        bonusObj.transform.SetParent(infoPanel.transform, false);
        RectTransform bRt = bonusObj.GetComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 0);
        bRt.anchorMax = new Vector2(1, 0.33f);
        bRt.offsetMin = new Vector2(12, 4);
        bRt.offsetMax = new Vector2(-12, 0);
        workshopBonusText = bonusObj.GetComponent<TextMeshProUGUI>();
        workshopBonusText.fontSize = 12;

        // Scroll View Container
        GameObject scrollObj = new GameObject("ModsScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 70);
        scrollRt.offsetMax = new Vector2(-18, -145);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform vpRt = viewportObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        modsContainer = contentObj.transform;
        RectTransform contRt = contentObj.GetComponent<RectTransform>();
        contRt.anchorMin = new Vector2(0, 1);
        contRt.anchorMax = new Vector2(1, 1);
        contRt.pivot = new Vector2(0.5f, 1);
        contRt.sizeDelta = new Vector2(0, 400);

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contRt;
        sr.viewport = vpRt;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        for (int i = 0; i < mods.Count; i++)
        {
            CreateModCard(i, mods[i], modsContainer);
        }

        // Bottom Bar: Mod Contest & Close
        GameObject bottomBar = new GameObject("BottomBar", typeof(RectTransform));
        bottomBar.transform.SetParent(cardObj.transform, false);
        RectTransform bbarRt = bottomBar.GetComponent<RectTransform>();
        bbarRt.anchorMin = new Vector2(0, 0);
        bbarRt.anchorMax = new Vector2(1, 0);
        bbarRt.pivot = new Vector2(0.5f, 0);
        bbarRt.anchoredPosition = new Vector2(0, 14);
        bbarRt.sizeDelta = new Vector2(-36, 46);

        GameObject contestBtnObj = new GameObject("ContestBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        contestBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform contestRt = contestBtnObj.GetComponent<RectTransform>();
        contestRt.anchorMin = new Vector2(0, 0);
        contestRt.anchorMax = new Vector2(0.68f, 1);
        contestRt.offsetMin = Vector2.zero;
        contestRt.offsetMax = new Vector2(-6, 0);
        contestBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.45f);
        Button contestBtn = contestBtnObj.GetComponent<Button>();
        contestBtn.onClick.AddListener(HostModdingContest);

        GameObject contestTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        contestTxtObj.transform.SetParent(contestBtnObj.transform, false);
        RectTransform ctRt = contestTxtObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;
        TMP_Text ct = contestTxtObj.GetComponent<TextMeshProUGUI>();
        ct.text = "🏆 ХАКАТОН МОДОДЕЛОВ";
        ct.fontSize = 11;
        ct.fontStyle = FontStyles.Bold;
        ct.alignment = TextAlignmentOptions.Center;
        ct.color = Color.white;

        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(bottomBar.transform, false);
        RectTransform clRt = closeBtnObj.GetComponent<RectTransform>();
        clRt.anchorMin = new Vector2(0.70f, 0);
        clRt.anchorMax = new Vector2(1, 1);
        clRt.offsetMin = Vector2.zero;
        clRt.offsetMax = Vector2.zero;
        closeBtnObj.GetComponent<Image>().color = new Color(0.28f, 0.32f, 0.38f);
        closeWorkshopBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform cltRt = closeTxtObj.GetComponent<RectTransform>();
        cltRt.anchorMin = Vector2.zero;
        cltRt.anchorMax = Vector2.one;
        cltRt.sizeDelta = Vector2.zero;
        TMP_Text clt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        clt.text = "ЗАКРЫТЬ";
        clt.fontSize = 11;
        clt.fontStyle = FontStyles.Bold;
        clt.alignment = TextAlignmentOptions.Center;
        clt.color = Color.white;

        modalRoot = root;
    }

    private void CreateModCard(int index, WorkshopMod mod, Transform parent)
    {
        GameObject card = new GameObject($"ModCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 92);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(1, 1);
        trt.pivot = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(-20, 22);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{mod.icon} <b>{mod.title}</b> <color=#90C0FF>(от {mod.author})</color>";
        tt.fontSize = 12;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.58f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"{mod.category}\n<b>{mod.perkDescription}</b>";
        dt.fontSize = 10;
        dt.color = new Color(0.8f, 0.85f, 0.95f);

        GameObject rat = new GameObject("RatingText", typeof(RectTransform), typeof(TextMeshProUGUI));
        rat.transform.SetParent(card.transform, false);
        RectTransform ratRt = rat.GetComponent<RectTransform>();
        ratRt.anchorMin = new Vector2(0.58f, 1);
        ratRt.anchorMax = new Vector2(1, 1);
        ratRt.pivot = new Vector2(1, 1);
        ratRt.anchoredPosition = new Vector2(-10, -6);
        ratRt.sizeDelta = new Vector2(170, 30);
        TMP_Text ratt = rat.GetComponent<TextMeshProUGUI>();
        ratt.fontSize = 10;
        ratt.alignment = TextAlignmentOptions.Right;

        GameObject b = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0);
        brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(1, 0);
        brt.anchoredPosition = new Vector2(-10, 8);
        brt.sizeDelta = new Vector2(170, 32);
        b.GetComponent<Image>().color = new Color(0.2f, 0.65f, 0.85f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = "ОДОБРИТЬ";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }
}
