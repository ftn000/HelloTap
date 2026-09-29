using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальный Рейд на Мега-Проект («Open-Source Community Raid Boss»):
/// - Совместная разработка мирового открытого игрового супер-движка «OmniEngine v5.0 OpenSource»
/// - Цель рейда: 50,000,000 строк кода с 4 контрольными вехами (Milestones: 25%, 50%, 75%, 100%)
/// - Каждая пройденная веха активирует мощный перманентный бонус студии:
///   1. 25% — Modular Vulkan/DirectX Renderer (+20% доход студии)
///   2. 50% — High-Performance ECS Core (+25% к клику)
///   3. 75% — Distributed Cloud Mesh (+20% оффлайн доход)
///   4. 100% — OmniEngine v5.0 Gold Master (Легендарный глобальный x1.40 множитель ко всему)
/// - Интерактивный коммит в рейд: «🚀 ЗАПУШИТЬ PULL REQUEST В РЕЙД»
/// </summary>
public class OpenSourceCommunityRaidUI : MonoBehaviour
{
    private static OpenSourceCommunityRaidUI instance;
    public static OpenSourceCommunityRaidUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<OpenSourceCommunityRaidUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(OpenSourceCommunityRaidUI));
                    instance = go.AddComponent<OpenSourceCommunityRaidUI>();
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
    public class RaidMilestone
    {
        public int percent;
        public double codeThreshold;
        public string title;
        public string perkDesc;
        public bool isUnlocked;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openRaidBtn;

    [Header("Прогресс рейда и коммиты")]
    [SerializeField] private TMP_Text raidHeaderStatsTxt;
    [SerializeField] private Slider raidProgressBar;
    [SerializeField] private TMP_Text raidProgressPercentTxt;
    [SerializeField] private Button pushPrBtn;
    [SerializeField] private TMP_Text pushPrBtnTxt;
    [SerializeField] private Transform milestonesContainer;

    private readonly List<RaidMilestone> milestones = new List<RaidMilestone>();
    private double totalContributedCode = 0;
    private const double RaidTargetCode = 50000000.0; // 50M строк кода

    private const string PrefContributedCode = "Raid_TotalContributedCode";
    private const string PrefMilestoneUnlocked = "Raid_MilestoneUnlocked_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeMilestones();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void InitializeMilestones()
    {
        if (milestones.Count > 0) return;

        milestones.Add(new RaidMilestone
        {
            percent = 25,
            codeThreshold = RaidTargetCode * 0.25,
            title = "Веха 1 (25%): Modular Render Pipeline",
            perkDesc = "+20% к общему доходу студии",
            isUnlocked = false
        });

        milestones.Add(new RaidMilestone
        {
            percent = 50,
            codeThreshold = RaidTargetCode * 0.50,
            title = "Веха 2 (50%): Zero-Allocation ECS Core",
            perkDesc = "+25% к строкам кода за клик",
            isUnlocked = false
        });

        milestones.Add(new RaidMilestone
        {
            percent = 75,
            codeThreshold = RaidTargetCode * 0.75,
            title = "Веха 3 (75%): Distributed Cloud Mesh",
            perkDesc = "+20% к оффлайн-доходу студии",
            isUnlocked = false
        });

        milestones.Add(new RaidMilestone
        {
            percent = 100,
            codeThreshold = RaidTargetCode,
            title = "Веха 4 (100%): OmniEngine 5.0 Release",
            perkDesc = "Легендарный x1.40 множитель ко всей студии",
            isUnlocked = false
        });
    }

    private void LoadData()
    {
        totalContributedCode = PlayerPrefs.GetFloat(PrefContributedCode, 150000f);
        for (int i = 0; i < milestones.Count; i++)
        {
            milestones[i].isUnlocked = PlayerPrefs.GetInt(PrefMilestoneUnlocked + i, 0) == 1;
        }
        CheckMilestoneUnlocks(false);
    }

    private void SaveData()
    {
        PlayerPrefs.SetFloat(PrefContributedCode, (float)totalContributedCode);
        for (int i = 0; i < milestones.Count; i++)
        {
            PlayerPrefs.SetInt(PrefMilestoneUnlocked + i, milestones[i].isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void CheckMilestoneUnlocks(bool notify)
    {
        for (int i = 0; i < milestones.Count; i++)
        {
            var m = milestones[i];
            if (!m.isUnlocked && totalContributedCode >= m.codeThreshold)
            {
                m.isUnlocked = true;
                if (notify)
                {
                    HapticFeedback.SuccessPattern();
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();
                    if (ClickJuice.Instance != null)
                    {
                        ClickJuice.Instance.SpawnCustomPopup($"🎉 ВЕХА РЕЙДА ДОСТИГНУТА!\n{m.title}\n{m.perkDesc}", transform.position, new Color(0.2f, 1f, 0.5f), true);
                    }
                }
            }
        }
    }

    public double GetRaidIncomeMultiplier()
    {
        double mult = 1.0;
        if (milestones.Count > 0 && milestones[0].isUnlocked) mult += 0.20;
        if (milestones.Count > 3 && milestones[3].isUnlocked) mult += 0.40;
        return mult;
    }

    public double GetRaidClickMultiplier()
    {
        double mult = 1.0;
        if (milestones.Count > 1 && milestones[1].isUnlocked) mult += 0.25;
        if (milestones.Count > 3 && milestones[3].isUnlocked) mult += 0.30;
        return mult;
    }

    public void PushPullRequest()
    {
        double curCode = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;
        double packageSize = Math.Min(curCode * 0.40, 250000.0);
        packageSize = Math.Max(packageSize, 1000.0);

        if (curCode < packageSize)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Нужно минимум {NumberFormatter.Format(packageSize)} C# для коммита в мега-рейд!", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendLinesOfCode(packageSize);
        }

        totalContributedCode += packageSize;
        CheckMilestoneUnlocks(true);

        // Награда от спонсоров открытого исходного кода
        double sponsorGrantRub = Math.Floor(packageSize * 2.2);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(sponsorGrantRub);
            GameManager.Instance.AddComboEnergy(0.20f);
        }

        SaveData();
        RefreshUI();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🚀 PR СЛИТ В OMNIENGINE!\nВлито: <b>+{NumberFormatter.Format(packageSize)} C#</b> | Грант: <b>+{NumberFormatter.Format(sponsorGrantRub)} ₽</b>", transform.position, new Color(0.2f, 0.85f, 1f), true);
        }
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
        if (pushPrBtn != null)
        {
            pushPrBtn.onClick.RemoveAllListeners();
            pushPrBtn.onClick.AddListener(PushPullRequest);
        }
        if (openRaidBtn != null)
        {
            openRaidBtn.onClick.RemoveAllListeners();
            openRaidBtn.onClick.AddListener(OpenModal);
        }
    }

    public void RefreshUI()
    {
        float ratio = Mathf.Clamp01((float)(totalContributedCode / RaidTargetCode));
        float percent = ratio * 100f;

        if (raidHeaderStatsTxt != null)
        {
            double incMult = GetRaidIncomeMultiplier();
            double clkMult = GetRaidClickMultiplier();
            raidHeaderStatsTxt.text = $"Рейд-босс: <color=#00FFAA>«OmniEngine v5.0 OpenSource»</color>\nВклад студии: <b>{NumberFormatter.Format(totalContributedCode)} / {NumberFormatter.Format(RaidTargetCode)} C#</b>\nМножитель дохода: <color=#00FFAA>x{incMult:0.00}</color> | Клик: <color=#FFD700>x{clkMult:0.00}</color>";
        }

        if (raidProgressBar != null)
        {
            raidProgressBar.value = ratio;
        }

        if (raidProgressPercentTxt != null)
        {
            raidProgressPercentTxt.text = $"{percent:0.0}%";
        }

        if (pushPrBtnTxt != null)
        {
            pushPrBtnTxt.text = "🚀 ЗАПУШИТЬ PULL REQUEST В РЕЙД (Сдать код)";
        }

        if (milestonesContainer == null) return;

        for (int i = 0; i < milestones.Count; i++)
        {
            var m = milestones[i];
            Transform child = i < milestonesContainer.childCount ? milestonesContainer.GetChild(i) : null;
            if (child == null) continue;

            TMP_Text titleTxt = child.Find("Header/TitleTxt")?.GetComponent<TMP_Text>();
            TMP_Text descTxt = child.Find("Body/DescTxt")?.GetComponent<TMP_Text>();
            Image bg = child.GetComponent<Image>();

            if (titleTxt != null)
            {
                titleTxt.text = $"{m.title} {(m.isUnlocked ? "✓ (РАЗБЛОКИРОВАНО)" : "🔒")}";
                titleTxt.color = m.isUnlocked ? new Color(0.2f, 1f, 0.5f) : new Color(0.8f, 0.8f, 0.8f);
            }
            if (descTxt != null)
            {
                descTxt.text = $"Эффект: <b>{m.perkDesc}</b>";
            }
            if (bg != null)
            {
                bg.color = m.isUnlocked ? new Color(0.12f, 0.22f, 0.18f, 1f) : new Color(0.14f, 0.15f, 0.22f, 1f);
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("OpenSourceRaid_ModalRoot", typeof(RectTransform));
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

        // Card
        GameObject card = new GameObject("RaidCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(root.transform, false);
        modalCardTransform = card.transform;
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0.5f, 0.5f);
        cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(620, 710);

        Image cImg = card.GetComponent<Image>();
        cImg.color = new Color(0.11f, 0.13f, 0.20f, 0.98f);

        // Header
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(Image));
        header.transform.SetParent(card.transform, false);
        RectTransform hRect = header.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(0, 68);
        hRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(0.14f, 0.20f, 0.35f, 1f);

        GameObject titleObj = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(header.transform, false);
        TextMeshProUGUI tTxt = titleObj.GetComponent<TextMeshProUGUI>();
        tTxt.text = "🌐 МЕГА-РЕЙД СООБЩЕСТВА: OMNIENGINE";
        tTxt.fontSize = 18;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.alignment = TextAlignmentOptions.MidlineLeft;
        tTxt.color = new Color(0.2f, 0.9f, 1f);
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

        // Subheader Stats
        GameObject statsObj = new GameObject("RaidStatsSummaryTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform stRect = statsObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0f, 1f);
        stRect.anchorMax = new Vector2(1f, 1f);
        stRect.pivot = new Vector2(0.5f, 1f);
        stRect.sizeDelta = new Vector2(-30, 52);
        stRect.anchoredPosition = new Vector2(0, -74);
        raidHeaderStatsTxt = statsObj.GetComponent<TextMeshProUGUI>();
        raidHeaderStatsTxt.fontSize = 13;
        raidHeaderStatsTxt.alignment = TextAlignmentOptions.Center;
        raidHeaderStatsTxt.color = new Color(0.92f, 0.94f, 1f);

        // Progress Bar Panel
        GameObject pbPanel = new GameObject("PbPanel", typeof(RectTransform), typeof(Image));
        pbPanel.transform.SetParent(card.transform, false);
        RectTransform ppRect = pbPanel.GetComponent<RectTransform>();
        ppRect.anchorMin = new Vector2(0f, 1f);
        ppRect.anchorMax = new Vector2(1f, 1f);
        ppRect.pivot = new Vector2(0.5f, 1f);
        ppRect.sizeDelta = new Vector2(-30, 56);
        ppRect.anchoredPosition = new Vector2(0, -130);
        pbPanel.GetComponent<Image>().color = new Color(0.15f, 0.17f, 0.25f, 1f);

        GameObject sliderObj = new GameObject("RaidSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(pbPanel.transform, false);
        RectTransform slRect = sliderObj.GetComponent<RectTransform>();
        slRect.anchorMin = new Vector2(0f, 0.5f);
        slRect.anchorMax = new Vector2(0.80f, 0.5f);
        slRect.pivot = new Vector2(0f, 0.5f);
        slRect.offsetMin = new Vector2(14, -8);
        slRect.offsetMax = new Vector2(0, 8);
        raidProgressBar = sliderObj.GetComponent<Slider>();

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
        raidProgressBar.fillRect = fRect;

        GameObject pctObj = new GameObject("PercentTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        pctObj.transform.SetParent(pbPanel.transform, false);
        RectTransform pctRect = pctObj.GetComponent<RectTransform>();
        pctRect.anchorMin = new Vector2(0.82f, 0f);
        pctRect.anchorMax = new Vector2(1f, 1f);
        pctRect.offsetMin = Vector2.zero;
        pctRect.offsetMax = Vector2.zero;
        raidProgressPercentTxt = pctObj.GetComponent<TextMeshProUGUI>();
        raidProgressPercentTxt.fontSize = 15;
        raidProgressPercentTxt.fontStyle = FontStyles.Bold;
        raidProgressPercentTxt.alignment = TextAlignmentOptions.Center;
        raidProgressPercentTxt.color = new Color(0.2f, 1f, 0.5f);

        // Push PR Action Button
        GameObject actPanel = new GameObject("ActPanel", typeof(RectTransform), typeof(Image));
        actPanel.transform.SetParent(card.transform, false);
        RectTransform apRect = actPanel.GetComponent<RectTransform>();
        apRect.anchorMin = new Vector2(0f, 1f);
        apRect.anchorMax = new Vector2(1f, 1f);
        apRect.pivot = new Vector2(0.5f, 1f);
        apRect.sizeDelta = new Vector2(-30, 48);
        apRect.anchoredPosition = new Vector2(0, -192);
        actPanel.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 1f);

        GameObject pBtnObj = new GameObject("PushPrBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pBtnObj.transform.SetParent(actPanel.transform, false);
        RectTransform pbr = pBtnObj.GetComponent<RectTransform>();
        pbr.anchorMin = Vector2.zero;
        pbr.anchorMax = Vector2.one;
        pbr.offsetMin = new Vector2(6, 6);
        pbr.offsetMax = new Vector2(-6, -6);
        pBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.60f, 0.90f, 1f);
        pushPrBtn = pBtnObj.GetComponent<Button>();

        GameObject ptTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        ptTxtObj.transform.SetParent(pBtnObj.transform, false);
        pushPrBtnTxt = ptTxtObj.GetComponent<TextMeshProUGUI>();
        pushPrBtnTxt.text = "🚀 ЗАПУШИТЬ PULL REQUEST В РЕЙД (Сдать код)";
        pushPrBtnTxt.fontSize = 13;
        pushPrBtnTxt.fontStyle = FontStyles.Bold;
        pushPrBtnTxt.alignment = TextAlignmentOptions.Center;
        pushPrBtnTxt.color = Color.white;
        RectTransform ptr = ptTxtObj.GetComponent<RectTransform>();
        ptr.anchorMin = Vector2.zero;
        ptr.anchorMax = Vector2.one;
        ptr.offsetMin = Vector2.zero;
        ptr.offsetMax = Vector2.zero;

        // Milestones Scroll View
        GameObject scrollObj = new GameObject("MilestonesScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(card.transform, false);
        RectTransform sRect = scrollObj.GetComponent<RectTransform>();
        sRect.anchorMin = new Vector2(0f, 0f);
        sRect.anchorMax = new Vector2(1f, 1f);
        sRect.offsetMin = new Vector2(15, 60);
        sRect.offsetMax = new Vector2(-15, -248);
        scrollObj.GetComponent<Image>().color = new Color(0.06f, 0.08f, 0.12f, 0.5f);

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
        milestonesContainer = content.transform;
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
        vlg.spacing = 8f;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.viewport = vRect;
        sr.content = ctnRect;
        sr.horizontal = false;
        sr.vertical = true;

        foreach (var m in milestones)
        {
            CreateMilestoneCardUI(content.transform, m);
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
        botCloseObj.GetComponent<Image>().color = new Color(0.20f, 0.26f, 0.38f, 1f);
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

    private void CreateMilestoneCardUI(Transform parent, RaidMilestone m)
    {
        GameObject card = new GameObject("MilestoneCard_" + m.percent, typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0, 75);
        card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.24f, 1f);

        // Header
        GameObject hdr = new GameObject("Header", typeof(RectTransform));
        hdr.transform.SetParent(card.transform, false);
        RectTransform hr = hdr.GetComponent<RectTransform>();
        hr.anchorMin = new Vector2(0f, 1f);
        hr.anchorMax = new Vector2(1f, 1f);
        hr.pivot = new Vector2(0.5f, 1f);
        hr.sizeDelta = new Vector2(-20, 24);
        hr.anchoredPosition = new Vector2(10, -6);

        GameObject title = new GameObject("TitleTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(hdr.transform, false);
        TextMeshProUGUI tTxt = title.GetComponent<TextMeshProUGUI>();
        tTxt.fontSize = 13;
        tTxt.fontStyle = FontStyles.Bold;
        tTxt.text = m.title;
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
        br.offsetMax = new Vector2(-10, -30);

        GameObject desc = new GameObject("DescTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        desc.transform.SetParent(body.transform, false);
        TextMeshProUGUI dTxt = desc.GetComponent<TextMeshProUGUI>();
        dTxt.fontSize = 11;
        dTxt.text = $"Эффект: {m.perkDesc}";
        dTxt.color = new Color(0.2f, 1f, 0.6f);
        RectTransform dr = desc.GetComponent<RectTransform>();
        dr.anchorMin = Vector2.zero;
        dr.anchorMax = Vector2.one;
        dr.offsetMin = Vector2.zero;
        dr.offsetMax = Vector2.zero;
    }
}
