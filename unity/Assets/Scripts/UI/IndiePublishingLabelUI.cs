using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Инди-Издательство и Подписание сторонних проектов (Indie Publishing Label):
/// - Студия выступает издателем для начинающих разработчиков
/// - 4 сторонних проекта с разным бюджетом аванса и процентом роялти
/// - Маркетинговые издательские кампании (Steam Festival, Азиатская локализация, PR)
/// - Пассивный доход от роялти с продаж игр по всему миру
/// </summary>
public class IndiePublishingLabelUI : MonoBehaviour
{
    private static IndiePublishingLabelUI instance;
    public static IndiePublishingLabelUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<IndiePublishingLabelUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class SignedGame
    {
        public string id;
        public string title;
        public string devName;
        public string genre;
        public string icon;
        public double advanceFee;
        public double royaltyPercent;
        public double passiveIncomePerSec;
        public int marketingLevel;
        public bool isSigned;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openLabelBtn;
    [SerializeField] private TMP_Text openLabelBtnText;
    [SerializeField] private Button closeLabelBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Сводка издательства")]
    [SerializeField] private TMP_Text totalSignedText;
    [SerializeField] private TMP_Text totalRoyaltiesIncomeText;

    [Header("Контейнер проектов")]
    [SerializeField] private Transform projectsContainer;

    private readonly List<SignedGame> games = new List<SignedGame>();

    private const string PrefSignedPrefix = "Studio_LabelSigned_";
    private const string PrefMktPrefix = "Studio_LabelMkt_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<SignedGame> Games => games;

    public double GetTotalRoyaltiesPerSec()
    {
        double total = 0;
        for (int i = 0; i < games.Count; i++)
        {
            if (games[i].isSigned)
            {
                double mktMult = 1.0 + (games[i].marketingLevel * 0.25);
                total += games[i].passiveIncomePerSec * mktMult;
            }
        }
        return total;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeCatalog();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
    }

    private void Update()
    {
        // Пассивное начисление роялти каждую секунду
        double royalties = GetTotalRoyaltiesPerSec();
        if (royalties > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(royalties * Time.deltaTime);
        }

        if (IsModalOpen && totalRoyaltiesIncomeText != null)
        {
            totalRoyaltiesIncomeText.text = $"Роялти от сторонних игр: <b><color=#00FF88>+{NumberFormatter.Format(royalties)} ₽/сек</color></b>";
        }
    }

    private void InitializeCatalog()
    {
        games.Clear();

        games.Add(new SignedGame
        {
            id = "pub_roguelite",
            title = "Pixel Crypt Roguelite",
            devName = "Solo Dev 'NeonBlade'",
            genre = "2D Action Roguelike",
            icon = "🗡️",
            advanceFee = 45000.0,
            royaltyPercent = 35.0,
            passiveIncomePerSec = 1400.0,
            marketingLevel = 0,
            isSigned = false
        });

        games.Add(new SignedGame
        {
            id = "pub_cozy_farm",
            title = "Cozy Valley Farm",
            devName = "Duo Studio 'TeaTime'",
            genre = "Cozy Life Sim",
            icon = "🌻",
            advanceFee = 160000.0,
            royaltyPercent = 25.0,
            passiveIncomePerSec = 5200.0,
            marketingLevel = 0,
            isSigned = false
        });

        games.Add(new SignedGame
        {
            id = "pub_cyber_vr",
            title = "Neon Detective VR",
            devName = "CyberTech Labs",
            genre = "VR Cyberpunk Noir",
            icon = "🕶️",
            advanceFee = 550000.0,
            royaltyPercent = 20.0,
            passiveIncomePerSec = 19500.0,
            marketingLevel = 0,
            isSigned = false
        });

        games.Add(new SignedGame
        {
            id = "pub_soulslike",
            title = "Eldritch Shadows: Reborn",
            devName = "DarkForge Games",
            genre = "Hardcore Soulslike RPG",
            icon = "💀",
            advanceFee = 2200000.0,
            royaltyPercent = 15.0,
            passiveIncomePerSec = 95000.0,
            marketingLevel = 0,
            isSigned = false
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < games.Count; i++)
        {
            games[i].isSigned = PlayerPrefs.GetInt(PrefSignedPrefix + games[i].id, 0) == 1;
            games[i].marketingLevel = PlayerPrefs.GetInt(PrefMktPrefix + games[i].id, 0);
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < games.Count; i++)
        {
            PlayerPrefs.SetInt(PrefSignedPrefix + games[i].id, games[i].isSigned ? 1 : 0);
            PlayerPrefs.SetInt(PrefMktPrefix + games[i].id, games[i].marketingLevel);
        }
        PlayerPrefs.Save();
    }

    public void SignProject(int index)
    {
        if (index < 0 || index >= games.Count) return;
        var game = games[index];

        if (game.isSigned) return;

        if (GameManager.Instance == null || GameManager.Instance.Money < game.advanceFee)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств для аванса ({NumberFormatter.Format(game.advanceFee)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(game.advanceFee);
        game.isSigned = true;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📝 КОНТРАКТ ПОДПИСАН!\n{game.icon} '{game.title}'\nРоялти: +{NumberFormatter.Format(game.passiveIncomePerSec)} ₽/сек", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        UpdateModalUI();
    }

    public void BoostMarketing(int index)
    {
        if (index < 0 || index >= games.Count) return;
        var game = games[index];

        if (!game.isSigned) return;

        double cost = 20000.0 * Math.Pow(1.8, game.marketingLevel);
        if (GameManager.Instance == null || GameManager.Instance.Money < cost)
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств ({NumberFormatter.Format(cost)} ₽)!", transform.position, new Color(1f, 0.4f, 0.4f), false);
            }
            return;
        }

        GameManager.Instance.SpendMoney(cost);
        game.marketingLevel++;
        SaveData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📢 МАРКЕТИНГ УСИЛЕН! (Ур. {game.marketingLevel})\nРоялти игры увеличены на +25%!", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateModalUI();
    }

    private void BindButtons()
    {
        if (openLabelBtn != null)
        {
            openLabelBtn.onClick.RemoveAllListeners();
            openLabelBtn.onClick.AddListener(OpenModal);
        }
        if (closeLabelBtn != null)
        {
            closeLabelBtn.onClick.RemoveAllListeners();
            closeLabelBtn.onClick.AddListener(CloseModal);
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
        int countSigned = 0;
        for (int i = 0; i < games.Count; i++) if (games[i].isSigned) countSigned++;

        if (totalSignedText != null)
        {
            totalSignedText.text = $"📚 Издано игр: <b>{countSigned}/{games.Count}</b>";
        }

        if (totalRoyaltiesIncomeText != null)
        {
            double r = GetTotalRoyaltiesPerSec();
            totalRoyaltiesIncomeText.text = $"Роялти от сторонних игр: <b><color=#00FF88>+{NumberFormatter.Format(r)} ₽/сек</color></b>";
        }

        RefreshProjectsList();
    }

    private void RefreshProjectsList()
    {
        if (projectsContainer == null) return;

        double currentMoney = GameManager.Instance != null ? GameManager.Instance.Money : 0;

        for (int i = 0; i < games.Count; i++)
        {
            int idx = i;
            var g = games[i];
            Transform cardTr = projectsContainer.Find($"GameCard_{idx}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            Button signBtn = cardTr.Find("SignBtn")?.GetComponent<Button>();
            TMP_Text signBtnTxt = signBtn != null ? signBtn.GetComponentInChildren<TMP_Text>() : null;
            Button mktBtn = cardTr.Find("MktBtn")?.GetComponent<Button>();
            TMP_Text mktBtnTxt = mktBtn != null ? mktBtn.GetComponentInChildren<TMP_Text>() : null;

            if (bg != null)
            {
                bg.color = g.isSigned 
                    ? new Color(0.11f, 0.22f, 0.16f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.92f);
            }

            if (signBtn != null && signBtnTxt != null)
            {
                signBtn.onClick.RemoveAllListeners();
                signBtn.onClick.AddListener(() => SignProject(idx));

                if (g.isSigned)
                {
                    signBtn.interactable = false;
                    signBtnTxt.text = "✓ ИЗДАЁТСЯ";
                    signBtn.GetComponent<Image>().color = new Color(0.12f, 0.45f, 0.25f);
                }
                else
                {
                    bool canAfford = currentMoney >= g.advanceFee;
                    signBtn.interactable = canAfford;
                    signBtnTxt.text = $"ПОДПИСАТЬ ({NumberFormatter.Format(g.advanceFee)} ₽)";
                    signBtn.GetComponent<Image>().color = canAfford ? new Color(0.15f, 0.55f, 0.75f) : new Color(0.2f, 0.25f, 0.32f);
                }
            }

            if (mktBtn != null && mktBtnTxt != null)
            {
                mktBtn.gameObject.SetActive(g.isSigned);
                if (g.isSigned)
                {
                    double mktCost = 20000.0 * Math.Pow(1.8, g.marketingLevel);
                    bool canAffordMkt = currentMoney >= mktCost;
                    mktBtn.interactable = canAffordMkt;
                    mktBtnTxt.text = $"📢 ПР (+25%): {NumberFormatter.Format(mktCost)} ₽";
                    mktBtn.onClick.RemoveAllListeners();
                    mktBtn.onClick.AddListener(() => BoostMarketing(idx));
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("IndiePublishingModal", typeof(RectTransform));
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
        outline.effectColor = new Color(0.3f, 0.8f, 1f, 0.5f);
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
        headerTxt.text = "📚 ИНДИ-ИЗДАТЕЛЬСТВО СТУДИИ";
        headerTxt.fontSize = 18;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(0.35f, 0.85f, 1f);

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
        infoRt.anchoredPosition = new Vector2(0, -56);
        infoRt.sizeDelta = new Vector2(-36, 68);
        infoPanel.GetComponent<Image>().color = new Color(0.11f, 0.15f, 0.22f, 0.95f);

        GameObject totObj = new GameObject("TotalSigned", typeof(RectTransform), typeof(TextMeshProUGUI));
        totObj.transform.SetParent(infoPanel.transform, false);
        RectTransform totRt = totObj.GetComponent<RectTransform>();
        totRt.anchorMin = new Vector2(0, 0.5f);
        totRt.anchorMax = new Vector2(1, 1);
        totRt.offsetMin = new Vector2(12, 0);
        totRt.offsetMax = new Vector2(-12, -4);
        totalSignedText = totObj.GetComponent<TextMeshProUGUI>();
        totalSignedText.fontSize = 13;
        totalSignedText.fontStyle = FontStyles.Bold;

        GameObject royObj = new GameObject("Royalties", typeof(RectTransform), typeof(TextMeshProUGUI));
        royObj.transform.SetParent(infoPanel.transform, false);
        RectTransform royRt = royObj.GetComponent<RectTransform>();
        royRt.anchorMin = new Vector2(0, 0);
        royRt.anchorMax = new Vector2(1, 0.5f);
        royRt.offsetMin = new Vector2(12, 4);
        royRt.offsetMax = new Vector2(-12, 0);
        totalRoyaltiesIncomeText = royObj.GetComponent<TextMeshProUGUI>();
        totalRoyaltiesIncomeText.fontSize = 12;

        // Scroll View with Games
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 68);
        scrollRt.offsetMax = new Vector2(-18, -135);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        projectsContainer = contentObj.transform;
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

        for (int i = 0; i < games.Count; i++)
        {
            CreateGameCardTemplate(projectsContainer, games[i], i);
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
        closeLabelBtn = closeBtnObj.GetComponent<Button>();

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

    private void CreateGameCardTemplate(Transform parent, SignedGame game, int index)
    {
        GameObject card = new GameObject($"GameCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 94);
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
        tt.text = $"{game.icon} <b>{game.title}</b> — <color=#A0C0E0>{game.devName}</color>";
        tt.fontSize = 13;

        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.6f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -30);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Жанр: {game.genre}\nРоялти: <color=#00FF88>+{NumberFormatter.Format(game.passiveIncomePerSec)} ₽/с</color> (Доля: {game.royaltyPercent:F0}%)";
        dt.fontSize = 11;

        // Sign Button
        GameObject b = new GameObject("SignBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(card.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1, 0.5f);
        brt.anchorMax = new Vector2(1, 0.5f);
        brt.pivot = new Vector2(1, 0.5f);
        brt.anchoredPosition = new Vector2(-10, 12);
        brt.sizeDelta = new Vector2(160, 30);
        b.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.75f);

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = $"ПОДПИСАТЬ ({NumberFormatter.Format(game.advanceFee)})";
        btxt.fontSize = 10;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;

        // Marketing Boost Button
        GameObject mb = new GameObject("MktBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        mb.transform.SetParent(card.transform, false);
        RectTransform mbrt = mb.GetComponent<RectTransform>();
        mbrt.anchorMin = new Vector2(1, 0.5f);
        mbrt.anchorMax = new Vector2(1, 0.5f);
        mbrt.pivot = new Vector2(1, 0.5f);
        mbrt.anchoredPosition = new Vector2(-10, -22);
        mbrt.sizeDelta = new Vector2(160, 26);
        mb.GetComponent<Image>().color = new Color(0.75f, 0.55f, 0.15f);

        GameObject mbt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        mbt.transform.SetParent(mb.transform, false);
        RectTransform mbtrt = mbt.GetComponent<RectTransform>();
        mbtrt.anchorMin = Vector2.zero;
        mbtrt.anchorMax = Vector2.one;
        mbtrt.sizeDelta = Vector2.zero;
        TMP_Text mbtxt = mbt.GetComponent<TextMeshProUGUI>();
        mbtxt.text = "📢 ПР МАРКЕТИНГ";
        mbtxt.fontSize = 10;
        mbtxt.fontStyle = FontStyles.Bold;
        mbtxt.alignment = TextAlignmentOptions.Center;
        mbtxt.color = Color.white;
        mb.SetActive(false);
    }
}
