using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Шоукейс наград «Game of the Year Awards» (Studio Trophy Cabinet):
/// - 5 престижных номинаций игровой индустрии (GotY, Инди-Шедевр, Технопрорыв, Лучший Саундтрек, Выбор Игроков)
/// - Интерактивная витрина кубков с золотыми рамками и историей наград
/// - Церемония вручения наград с овациями, конфетти-попапами и фанфарами
/// - Перманентные пассивные бонусы к доходу компании, силе клика и престижу
/// </summary>
public class StudioTrophyCabinetUI : MonoBehaviour
{
    private static StudioTrophyCabinetUI instance;
    public static StudioTrophyCabinetUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<StudioTrophyCabinetUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(StudioTrophyCabinetUI));
                    instance = go.AddComponent<StudioTrophyCabinetUI>();
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
    public class StudioTrophy
    {
        public string id;
        public string title;
        public string category;
        public string icon;
        public string conditionDesc;
        public string perkDesc;
        public double bonusMultiplier;
        public bool isUnlocked;
        public string dateUnlocked;
    }

    [Header("UI элементы модального окна")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button openTrophyBtn;

    [Header("Витрина и церемония")]
    [SerializeField] private TMP_Text trophyHeaderSummaryTxt;
    [SerializeField] private Button nominateCeremonyBtn;
    [SerializeField] private TMP_Text nominateCeremonyBtnTxt;
    [SerializeField] private Transform trophyListContainer;

    private readonly List<StudioTrophy> trophies = new List<StudioTrophy>();

    private const string PrefTrophyPrefix = "Studio_TrophyGotY_";
    private const string PrefTrophyDatePrefix = "Studio_TrophyDate_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<StudioTrophy> Trophies => trophies;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTrophies();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        CheckAllTrophiesEligibility();
    }

    private void InitializeTrophies()
    {
        trophies.Clear();

        trophies.Add(new StudioTrophy
        {
            id = "goty",
            title = "Игра Года (Game of the Year)",
            category = "Главная Номинация Церемонии",
            icon = "🏆",
            conditionDesc = "Выпустить минимум 4 проекта и заработать от 300 000 ₽",
            perkDesc = "+50% ко всем доходам студии перманентно",
            bonusMultiplier = 0.50,
            isUnlocked = false
        });

        trophies.Add(new StudioTrophy
        {
            id = "indie_darling",
            title = "Лучший Инди-шедевр (Indie Darling)",
            category = "Инновации и Душевность",
            icon = "💎",
            conditionDesc = "Написать более 20 000 строк чистого кода",
            perkDesc = "+30% строк кода за клик",
            bonusMultiplier = 0.30,
            isUnlocked = false
        });

        trophies.Add(new StudioTrophy
        {
            id = "tech_breakthrough",
            title = "Технологический Прорыв (Tech Breakthrough)",
            category = "Движок и Архитектура",
            icon = "🚀",
            conditionDesc = "Прокачать движок TapEngine минимум до версии v2.0",
            perkDesc = "+25% к скорости релизов и оффлайн доходу",
            bonusMultiplier = 0.25,
            isUnlocked = false
        });

        trophies.Add(new StudioTrophy
        {
            id = "best_audio",
            title = "Лучший Саундтрек (Best Audio & Atmosphere)",
            category = "Музыка и Звуковой Дизайн",
            icon = "🎧",
            conditionDesc = "Прокачать аудио-модуль движка или включить Lo-Fi плеер",
            perkDesc = "+20% к бонусам комбо и энергии",
            bonusMultiplier = 0.20,
            isUnlocked = false
        });

        trophies.Add(new StudioTrophy
        {
            id = "players_choice",
            title = "Выбор Игроков (Players' Choice Award)",
            category = "Народное Признание",
            icon = "👑",
            conditionDesc = "Совершить хотя бы 1 IPO (Престиж) или сделать 500 кликов",
            perkDesc = "+35% к очкам престижа IPO при перезапуске",
            bonusMultiplier = 0.35,
            isUnlocked = false
        });
    }

    private void LoadData()
    {
        for (int i = 0; i < trophies.Count; i++)
        {
            trophies[i].isUnlocked = PlayerPrefs.GetInt(PrefTrophyPrefix + trophies[i].id, 0) == 1;
            trophies[i].dateUnlocked = PlayerPrefs.GetString(PrefTrophyDatePrefix + trophies[i].id, "");
        }
    }

    private void SaveData()
    {
        for (int i = 0; i < trophies.Count; i++)
        {
            PlayerPrefs.SetInt(PrefTrophyPrefix + trophies[i].id, trophies[i].isUnlocked ? 1 : 0);
            PlayerPrefs.SetString(PrefTrophyDatePrefix + trophies[i].id, trophies[i].dateUnlocked);
        }
        PlayerPrefs.Save();
    }

    public double GetTrophyMultiplier()
    {
        double mult = 1.0;
        int count = 0;
        for (int i = 0; i < trophies.Count; i++)
        {
            if (trophies[i].isUnlocked)
            {
                count++;
                mult += trophies[i].bonusMultiplier;
            }
        }
        return mult;
    }

    public int GetUnlockedTrophiesCount()
    {
        int count = 0;
        for (int i = 0; i < trophies.Count; i++)
        {
            if (trophies[i].isUnlocked) count++;
        }
        return count;
    }

    public bool CheckEligibility(StudioTrophy trophy)
    {
        if (trophy.isUnlocked) return true;
        if (GameManager.Instance == null) return false;

        switch (trophy.id)
        {
            case "goty":
                int completedProjects = 0;
                foreach (var p in GameManager.Instance.Projects)
                {
                    if (p.IsCompleted) completedProjects++;
                }
                return completedProjects >= 4 && GameManager.Instance.TotalMoneyEarned >= 300000;

            case "indie_darling":
                return GameManager.Instance.TotalCodeWritten >= 20000;

            case "tech_breakthrough":
                int engineVer = CustomGameEngineUI.Instance != null ? CustomGameEngineUI.Instance.CurrentMajorVersion : 1;
                return engineVer >= 2;

            case "best_audio":
                int audioLvl = CustomGameEngineUI.Instance != null ? CustomGameEngineUI.Instance.GetModuleLevel("audio") : 0;
                bool isLoFiPlaying = LoFiPlayerUI.Instance != null && LoFiPlayerUI.Instance.IsMusicOrAmbienceActive;
                return audioLvl >= 1 || isLoFiPlaying;

            case "players_choice":
                return GameManager.Instance.PrestigeLevel >= 1 || GameManager.Instance.TotalCodeWritten >= 1000;

            default:
                return false;
        }
    }

    public int CheckAllTrophiesEligibility()
    {
        int readyToClaim = 0;
        for (int i = 0; i < trophies.Count; i++)
        {
            if (!trophies[i].isUnlocked && CheckEligibility(trophies[i]))
            {
                readyToClaim++;
            }
        }
        return readyToClaim;
    }

    public void HostCeremonyAndClaimTrophies()
    {
        int awardedCount = 0;
        string awardedNames = "";

        for (int i = 0; i < trophies.Count; i++)
        {
            var tr = trophies[i];
            if (!tr.isUnlocked && CheckEligibility(tr))
            {
                tr.isUnlocked = true;
                tr.dateUnlocked = DateTime.Now.ToString("yyyy-MM-dd");
                awardedCount++;
                awardedNames += $"\n{tr.icon} {tr.title}";
            }
        }

        if (awardedCount > 0)
        {
            SaveData();
            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎉 ТРИУМФ НА ЦЕРЕМОНИИ GOTY!\nЗавоевано кубков: {awardedCount}{awardedNames}", transform.position, new Color(1f, 0.85f, 0.2f), true);
            }
        }
        else
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("📋 Новых номинаций пока нет! Выполняйте условия кубков.", transform.position, Color.yellow, false);
            }
        }

        UpdateModalUI();
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
        if (openTrophyBtn != null)
        {
            openTrophyBtn.onClick.RemoveAllListeners();
            openTrophyBtn.onClick.AddListener(OpenModal);
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
        if (nominateCeremonyBtn != null)
        {
            nominateCeremonyBtn.onClick.RemoveAllListeners();
            nominateCeremonyBtn.onClick.AddListener(HostCeremonyAndClaimTrophies);
        }
    }

    private void UpdateModalUI()
    {
        int unlockedCount = GetUnlockedTrophiesCount();
        int pendingCount = CheckAllTrophiesEligibility();

        if (trophyHeaderSummaryTxt != null)
        {
            double multBonus = (GetTrophyMultiplier() - 1.0) * 100.0;
            trophyHeaderSummaryTxt.text = $"Завоевано наград: <b><color=#FFD700>{unlockedCount}/{trophies.Count}</color></b> | Суммарный бонус: <b><color=#00FF88>+{multBonus:F0}%</color></b> ко всем доходам";
        }

        if (nominateCeremonyBtn != null && nominateCeremonyBtnTxt != null)
        {
            if (pendingCount > 0)
            {
                nominateCeremonyBtn.interactable = true;
                nominateCeremonyBtnTxt.text = $"🏆 ПОЛУЧИТЬ НАГРАДЫ ЦЕРЕМОНИИ ({pendingCount} ГОТОВО!)";
            }
            else if (unlockedCount == trophies.Count)
            {
                nominateCeremonyBtn.interactable = false;
                nominateCeremonyBtnTxt.text = "👑 ВСЕ КУБКИ GOTY ЗАВОЕВАНЫ!";
            }
            else
            {
                nominateCeremonyBtn.interactable = false;
                nominateCeremonyBtnTxt.text = "ВЫПОЛНЯЙТЕ УСЛОВИЯ НОМИНАЦИЙ";
            }
        }

        RefreshTrophyCards();
    }

    private void RefreshTrophyCards()
    {
        if (trophyListContainer == null) return;

        for (int i = 0; i < trophies.Count; i++)
        {
            var tr = trophies[i];
            Transform cardTr = trophyListContainer.Find($"TrophyCard_{tr.id}");
            if (cardTr == null) continue;

            Image bg = cardTr.GetComponent<Image>();
            TMP_Text statusTxt = cardTr.Find("StatusTxt")?.GetComponent<TMP_Text>();

            bool isReady = !tr.isUnlocked && CheckEligibility(tr);

            if (bg != null)
            {
                if (tr.isUnlocked)
                {
                    bg.color = new Color(0.18f, 0.16f, 0.08f, 0.95f);
                }
                else if (isReady)
                {
                    bg.color = new Color(0.12f, 0.22f, 0.15f, 0.95f);
                }
                else
                {
                    bg.color = new Color(0.08f, 0.10f, 0.14f, 0.9f);
                }
            }

            if (statusTxt != null)
            {
                if (tr.isUnlocked)
                {
                    statusTxt.text = $"<color=#FFD700>★ ЗАВОЕВАНО</color>\n<size=9>{tr.dateUnlocked}</size>";
                }
                else if (isReady)
                {
                    statusTxt.text = "<color=#00FF88>ГОТОВО К ВРУЧЕНИЮ!</color>";
                }
                else
                {
                    statusTxt.text = "<color=#8090A0>В ПРОЦЕССЕ</color>";
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
        GameObject root = new GameObject("StudioTrophyModal", typeof(RectTransform));
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
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.84f, 0.2f, 0.6f);
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
        headerTxt.text = "🏆 ЗАЛ СЛАВЫ «GAME OF THE YEAR»";
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

        // Trophy Summary Box
        GameObject sumObj = new GameObject("SummaryBox", typeof(RectTransform), typeof(Image));
        sumObj.transform.SetParent(cardObj.transform, false);
        RectTransform sumRt = sumObj.GetComponent<RectTransform>();
        sumRt.anchorMin = new Vector2(0, 1);
        sumRt.anchorMax = new Vector2(1, 1);
        sumRt.pivot = new Vector2(0.5f, 1);
        sumRt.anchoredPosition = new Vector2(0, -52);
        sumRt.sizeDelta = new Vector2(-36, 40);
        sumObj.GetComponent<Image>().color = new Color(0.14f, 0.12f, 0.08f, 0.9f);

        GameObject sumTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        sumTxtObj.transform.SetParent(sumObj.transform, false);
        RectTransform sumTxtRt = sumTxtObj.GetComponent<RectTransform>();
        sumTxtRt.anchorMin = Vector2.zero; sumTxtRt.anchorMax = Vector2.one; sumTxtRt.sizeDelta = new Vector2(-12, 0);
        trophyHeaderSummaryTxt = sumTxtObj.GetComponent<TextMeshProUGUI>();
        trophyHeaderSummaryTxt.fontSize = 11;
        trophyHeaderSummaryTxt.alignment = TextAlignmentOptions.Center;
        trophyHeaderSummaryTxt.color = new Color(1f, 0.95f, 0.8f);

        // Nominate / Ceremony Button
        GameObject cerObj = new GameObject("CeremonyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        cerObj.transform.SetParent(cardObj.transform, false);
        RectTransform cerRt = cerObj.GetComponent<RectTransform>();
        cerRt.anchorMin = new Vector2(0, 1);
        cerRt.anchorMax = new Vector2(1, 1);
        cerRt.pivot = new Vector2(0.5f, 1);
        cerRt.anchoredPosition = new Vector2(0, -100);
        cerRt.sizeDelta = new Vector2(-36, 42);
        cerObj.GetComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f, 0.95f);
        nominateCeremonyBtn = cerObj.GetComponent<Button>();

        GameObject cerTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        cerTxtObj.transform.SetParent(cerObj.transform, false);
        RectTransform cerTxtRt = cerTxtObj.GetComponent<RectTransform>();
        cerTxtRt.anchorMin = Vector2.zero; cerTxtRt.anchorMax = Vector2.one; cerTxtRt.sizeDelta = Vector2.zero;
        nominateCeremonyBtnTxt = cerTxtObj.GetComponent<TextMeshProUGUI>();
        nominateCeremonyBtnTxt.fontSize = 12;
        nominateCeremonyBtnTxt.fontStyle = FontStyles.Bold;
        nominateCeremonyBtnTxt.alignment = TextAlignmentOptions.Center;
        nominateCeremonyBtnTxt.color = Color.white;

        // Scroll Container for Trophies
        GameObject scrollObj = new GameObject("ScrollRect", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(18, 56);
        scrollRt.offsetMax = new Vector2(-18, -152);
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
        trophyListContainer = contentObj.transform;

        for (int i = 0; i < trophies.Count; i++)
        {
            CreateTrophyCardTemplate(trophyListContainer, trophies[i]);
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
        closeBtnObj.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.3f, 0.95f);
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

    private void CreateTrophyCardTemplate(Transform parent, StudioTrophy tr)
    {
        GameObject card = new GameObject($"TrophyCard_{tr.id}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(440, 72);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.9f);

        // Title and icon
        GameObject t = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        t.transform.SetParent(card.transform, false);
        RectTransform trt = t.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0, 1);
        trt.anchorMax = new Vector2(0.72f, 1);
        trt.pivot = new Vector2(0, 1);
        trt.anchoredPosition = new Vector2(10, -6);
        trt.sizeDelta = new Vector2(0, 20);
        TMP_Text tt = t.GetComponent<TextMeshProUGUI>();
        tt.text = $"{tr.icon} <b>{tr.title}</b>";
        tt.fontSize = 12;
        tt.color = new Color(1f, 0.9f, 0.4f);

        // Status text
        GameObject statObj = new GameObject("StatusTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        statObj.transform.SetParent(card.transform, false);
        RectTransform statRt = statObj.GetComponent<RectTransform>();
        statRt.anchorMin = new Vector2(0.72f, 0.5f);
        statRt.anchorMax = new Vector2(1, 0.5f);
        statRt.pivot = new Vector2(1, 0.5f);
        statRt.anchoredPosition = new Vector2(-10, 0);
        statRt.sizeDelta = new Vector2(0, 36);
        TMP_Text st = statObj.GetComponent<TextMeshProUGUI>();
        st.text = "<color=#8090A0>В ПРОЦЕССЕ</color>";
        st.fontSize = 10;
        st.alignment = TextAlignmentOptions.Right;

        // Condition and perk text
        GameObject d = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(card.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 0);
        drt.anchorMax = new Vector2(0.72f, 1);
        drt.offsetMin = new Vector2(10, 6);
        drt.offsetMax = new Vector2(0, -26);
        TMP_Text dt = d.GetComponent<TextMeshProUGUI>();
        dt.text = $"Условие: <color=#B0C0D0>{tr.conditionDesc}</color>\nЭффект: <color=#00FF88>{tr.perkDesc}</color>";
        dt.fontSize = 9;
    }

    private void EnsureFloatingButton(Canvas canvas)
    {
        if (openTrophyBtn != null) return;

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

        GameObject btnGo = new GameObject("TrophyHubBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(hubBar, false);
        btnGo.GetComponent<Image>().color = new Color(0.4f, 0.32f, 0.1f, 0.9f);
        openTrophyBtn = btnGo.GetComponent<Button>();

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one; txtRt.sizeDelta = Vector2.zero;
        TMP_Text txt = txtGo.GetComponent<TextMeshProUGUI>();
        txt.text = "🏆 Зал Слав";
        txt.fontSize = 11;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
    }
}
