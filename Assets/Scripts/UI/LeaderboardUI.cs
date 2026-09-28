using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Таблица рекордов разработчиков (In-Game Leaderboard):
/// - Отображение Топ-10 лучших разработчиков платформы (Код / Капитализация)
/// - Закреплённая плашка текущего игрока с его рангом и статусом
/// - Переключение вкладок: «💻 ТОП КОДА» и «💰 КАПИТАЛИЗАЦИЯ»
/// - Автоматическая отправка рекордов в Яндекс Игры через YandexSDKBridge
/// - Реалистичные данные соперников и динамический пересчёт позиции игрока
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    private static LeaderboardUI instance;
    public static LeaderboardUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<LeaderboardUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openLeaderboardBtn;
    [SerializeField] private TMP_Text openLeaderboardBtnText;
    [SerializeField] private Button closeLeaderboardBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Вкладки режимов")]
    [SerializeField] private Button tabCodeBtn;
    [SerializeField] private Button tabMoneyBtn;
    [SerializeField] private TMP_Text tabCodeText;
    [SerializeField] private TMP_Text tabMoneyText;

    [Header("Список лидеров")]
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TMP_Text playerStatsBannerText;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private int activeTab = 0; // 0 = Code, 1 = Money

    public struct LeaderboardDevEntry
    {
        public string Name;
        public string Title;
        public double CodeScore;
        public double MoneyScore;
        public int Prestige;

        public LeaderboardDevEntry(string name, string title, double code, double money, int prestige)
        {
            Name = name;
            Title = title;
            CodeScore = code;
            MoneyScore = money;
            Prestige = prestige;
        }
    }

    private static readonly LeaderboardDevEntry[] MockRivals = new LeaderboardDevEntry[]
    {
        new LeaderboardDevEntry("CyberArchitect_Pro", "Tech Lead (IPO IV)", 8500000.0, 420000000.0, 4),
        new LeaderboardDevEntry("VoxelMaster_99", "Principal Dev (IPO III)", 4200000.0, 185000000.0, 3),
        new LeaderboardDevEntry("PixelSamurai", "Lead Developer (IPO II)", 1950000.0, 78000000.0, 2),
        new LeaderboardDevEntry("NullPointerHero", "Senior Backend (IPO I)", 850000.0, 24000000.0, 1),
        new LeaderboardDevEntry("ShaderWizard", "Senior Graphics Dev", 450000.0, 9500000.0, 0),
        new LeaderboardDevEntry("CodeMonkey_42", "Middle GameDev", 210000.0, 3200000.0, 0),
        new LeaderboardDevEntry("CoffeeOverflower", "Junior Pythonist", 85000.0, 850000.0, 0),
        new LeaderboardDevEntry("BugHunter_2026", "QA Automation", 32000.0, 240000.0, 0),
        new LeaderboardDevEntry("GitPushForce", "Стажёр на испытательном", 12000.0, 45000.0, 0),
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
    }

    private void BindButtons()
    {
        if (openLeaderboardBtn != null)
        {
            openLeaderboardBtn.onClick.RemoveAllListeners();
            openLeaderboardBtn.onClick.AddListener(OpenModal);
        }
        if (closeLeaderboardBtn != null)
        {
            closeLeaderboardBtn.onClick.RemoveAllListeners();
            closeLeaderboardBtn.onClick.AddListener(CloseModal);
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

        if (tabCodeBtn != null)
        {
            tabCodeBtn.onClick.RemoveAllListeners();
            tabCodeBtn.onClick.AddListener(() => SwitchTab(0));
        }
        if (tabMoneyBtn != null)
        {
            tabMoneyBtn.onClick.RemoveAllListeners();
            tabMoneyBtn.onClick.AddListener(() => SwitchTab(1));
        }
    }

    public void OpenModal()
    {
        if (modalRoot == null) return;
        modalRoot.SetActive(true);

        if (modalCardTransform != null)
        {
            modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
            StartCoroutine(PopCardAnim(modalCardTransform));
        }

        // Синхронизация рекордов в Яндекс Игры
        SyncWithYandexLeaderboards();

        HapticFeedback.MediumImpact();
        RefreshLeaderboardUI();
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.LightImpact();
    }

    private IEnumerator PopCardAnim(Transform card)
    {
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.18f);
            float scale = Mathf.Lerp(0.85f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    public void SwitchTab(int tab)
    {
        activeTab = tab;
        HapticFeedback.LightImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();
        RefreshLeaderboardUI();
    }

    private void SyncWithYandexLeaderboards()
    {
        if (GameManager.Instance == null) return;

        double totalCode = GameManager.Instance.TotalCodeWritten;
        double totalMoney = GameManager.Instance.TotalMoneyEarned;

        int codeScore = (int)Math.Min(totalCode, int.MaxValue);
        int moneyScore = (int)Math.Min(totalMoney, int.MaxValue);

        YandexSDKBridge.Instance.SetLeaderboardScore("HelloTap_CodeLines", codeScore);
        YandexSDKBridge.Instance.SetLeaderboardScore("HelloTap_Capitalization", moneyScore);
    }

    public void RefreshLeaderboardUI()
    {
        // Обновление вкладок
        if (tabCodeText != null)
        {
            tabCodeText.color = activeTab == 0 ? new Color(0f, 1f, 0.7f) : new Color(0.6f, 0.7f, 0.8f);
        }
        if (tabMoneyText != null)
        {
            tabMoneyText.color = activeTab == 1 ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 0.7f, 0.8f);
        }

        // Собираем список участников включая игрока
        double playerCode = GameManager.Instance != null ? GameManager.Instance.TotalCodeWritten : 0;
        double playerMoney = GameManager.Instance != null ? GameManager.Instance.TotalMoneyEarned : 0;
        int playerPrestige = GameManager.Instance != null ? GameManager.Instance.PrestigeLevel : 0;
        string playerRank = GameManager.Instance != null ? GameManager.Instance.GetDeveloperRankTitle() : "Новичок";

        List<LeaderboardDevEntry> allEntries = new List<LeaderboardDevEntry>(MockRivals);
        allEntries.Add(new LeaderboardDevEntry("ВЫ (Разработчик)", playerRank, playerCode, playerMoney, playerPrestige));

        if (activeTab == 0)
        {
            allEntries.Sort((a, b) => b.CodeScore.CompareTo(a.CodeScore));
        }
        else
        {
            allEntries.Sort((a, b) => b.MoneyScore.CompareTo(a.MoneyScore));
        }

        // Находим место игрока
        int playerPlace = 1;
        for (int i = 0; i < allEntries.Count; i++)
        {
            if (allEntries[i].Name.StartsWith("ВЫ"))
            {
                playerPlace = i + 1;
                break;
            }
        }

        if (playerStatsBannerText != null)
        {
            string scoreFormatted = activeTab == 0 
                ? $"{NumberFormatter.Format(playerCode)} строк" 
                : $"{NumberFormatter.Format(playerMoney)} руб.";
            
            playerStatsBannerText.text =
                $"<color=#00FF88>★ ВАША ПОЗИЦИЯ: #{playerPlace}</color>  |  " +
                $"<color=#FFD166>{scoreFormatted}</color>  |  " +
                $"<color=#38BDF8>{playerRank}</color>";
        }

        // Заполнение контейнера
        if (entriesContainer != null)
        {
            // Очищаем старые записи
            for (int i = entriesContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(entriesContainer.GetChild(i).gameObject);
            }

            int countToShow = Math.Min(10, allEntries.Count);
            for (int i = 0; i < countToShow; i++)
            {
                var dev = allEntries[i];
                bool isMe = dev.Name.StartsWith("ВЫ");
                CreateEntryRow(i + 1, dev, isMe);
            }
        }
    }

    private void CreateEntryRow(int rank, LeaderboardDevEntry dev, bool isPlayer)
    {
        if (entriesContainer == null) return;

        GameObject rowGo = new GameObject($"LeaderRow_{rank}");
        rowGo.transform.SetParent(entriesContainer, false);

        Image rowBg = rowGo.AddComponent<Image>();
        if (isPlayer)
        {
            rowBg.color = new Color(0.12f, 0.45f, 0.35f, 0.75f);
        }
        else if (rank == 1)
        {
            rowBg.color = new Color(0.40f, 0.32f, 0.08f, 0.65f); // Золото
        }
        else if (rank == 2)
        {
            rowBg.color = new Color(0.28f, 0.32f, 0.38f, 0.65f); // Серебро
        }
        else if (rank == 3)
        {
            rowBg.color = new Color(0.35f, 0.22f, 0.12f, 0.65f); // Бронза
        }
        else
        {
            rowBg.color = (rank % 2 == 0) ? new Color(0.08f, 0.11f, 0.18f, 0.60f) : new Color(0.05f, 0.07f, 0.12f, 0.50f);
        }

        RectTransform rt = rowGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 36);

        HorizontalLayoutGroup hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(12, 12, 4, 4);
        hlg.spacing = 8;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // 1. Номер / Медаль
        string medal = rank == 1 ? "🥇 1" : (rank == 2 ? "🥈 2" : (rank == 3 ? "🥉 3" : $"#{rank}"));
        GameObject rankGo = new GameObject("Rank");
        rankGo.transform.SetParent(rowGo.transform, false);
        TMP_Text rankText = rankGo.AddComponent<TextMeshProUGUI>();
        rankText.text = medal;
        rankText.fontSize = 13;
        rankText.fontStyle = FontStyles.Bold;
        rankText.alignment = TextAlignmentOptions.MidlineLeft;
        rankText.rectTransform.sizeDelta = new Vector2(46, 0);

        // 2. Имя и титул
        GameObject nameGo = new GameObject("NameTitle");
        nameGo.transform.SetParent(rowGo.transform, false);
        TMP_Text nameText = nameGo.AddComponent<TextMeshProUGUI>();
        nameText.text = isPlayer ? $"<color=#00FF88><b>{dev.Name}</b></color> <size=10><color=#94A3B8>({dev.Title})</color></size>" : $"{dev.Name} <size=10><color=#64748B>({dev.Title})</color></size>";
        nameText.fontSize = 12;
        nameText.alignment = TextAlignmentOptions.MidlineLeft;
        nameText.overflowMode = TextOverflowModes.Ellipsis;
        nameText.rectTransform.sizeDelta = new Vector2(170, 0);

        // 3. Результат
        string valStr = activeTab == 0 ? $"{NumberFormatter.Format(dev.CodeScore)} строк" : $"{NumberFormatter.Format(dev.MoneyScore)} руб.";
        GameObject scoreGo = new GameObject("Score");
        scoreGo.transform.SetParent(rowGo.transform, false);
        TMP_Text scoreText = scoreGo.AddComponent<TextMeshProUGUI>();
        scoreText.text = isPlayer ? $"<color=#FFD166><b>{valStr}</b></color>" : $"<color=#E2E8F0>{valStr}</color>";
        scoreText.fontSize = 12;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.MidlineRight;
        scoreText.rectTransform.sizeDelta = new Vector2(90, 0);
    }
}
