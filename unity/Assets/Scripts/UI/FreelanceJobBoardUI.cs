using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Фриланс-биржа заказов (Freelance Job Board):
/// - Доска активных контрактов разных уровней сложности (Junior, Middle, Senior, Lead Architect)
/// - Система репутации разработчика (⭐⭐⭐⭐⭐ рейтинг клиентов и бонусы к выплатам)
/// - Срочные заказы с таймерами дедлайна, штрафами за просрочку и премией за быструю сдачу
/// - Подробная карточка заказа с клиентом, ТЗ, прогрессом написания кода и расчётом гонорара
/// </summary>
public class FreelanceJobBoardUI : MonoBehaviour
{
    private static FreelanceJobBoardUI instance;
    public static FreelanceJobBoardUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<FreelanceJobBoardUI>();
            return instance;
        }
    }

    public enum JobTier
    {
        Junior,
        Middle,
        Senior,
        LeadArchitect
    }

    [System.Serializable]
    public class FreelanceJob
    {
        public string id;
        public string clientName;
        public string title;
        public string description;
        public JobTier tier;
        public double requiredCode;
        public double currentCode;
        public double rewardMoney;
        public double bonusCode;
        public float timeLimitSeconds;
        public float timeRemaining;
        public bool isActive;
        public bool isCompleted;
        public bool isFailed;

        public float Progress => (float)Math.Clamp(currentCode / Math.Max(1.0, requiredCode), 0.0, 1.0);
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openBoardBtn;
    [SerializeField] private TMP_Text openBoardBtnText;
    [SerializeField] private Button closeBoardBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Информация о профиле и репутации")]
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text completedStatsText;
    [SerializeField] private TMP_Text ratingBonusText;

    [Header("Список заказов")]
    [SerializeField] private Transform jobCardsContainer;
    [SerializeField] private Button refreshJobsBtn;
    [SerializeField] private TMP_Text refreshBtnText;

    private readonly List<FreelanceJob> availableJobs = new List<FreelanceJob>();
    private float refreshTimer = 0f;
    private const float AutoRefreshInterval = 90f;

    private float developerRating = 4.8f;
    private int totalJobsCompleted = 0;
    private double totalFreelanceEarnings = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public IReadOnlyList<FreelanceJob> AvailableJobs => availableJobs;

    private static readonly (string Client, string Title, string Desc, JobTier Tier, double BaseReq, double BaseMoney)[] JobTemplates = new[]
    {
        ("Студия 'PixelForge'", "Верстка лендинга для инди-игры", "Адаптировать CSS сетку и подключить кнопку скачивания.", JobTier.Junior, 300.0, 4500.0),
        ("Telegram-стартап", "Бот для розыгрыша подписок", "Написать обработчик вебхуков и подключить базу SQLite.", JobTier.Junior, 600.0, 9000.0),
        ("Финтех 'PayFast'", "Модуль расчёта кэшбэка", "Оптимизировать алгоритм транзакций без блокировок потока.", JobTier.Middle, 2500.0, 35000.0),
        ("Издатель 'CyberQuest'", "Оптимизация шейдеров под WebGL", "Снизить Draw Calls и убрать утечки текстурной памяти.", JobTier.Middle, 6000.0, 85000.0),
        ("AI Лаборатория", "Инференс нейросети на GPU", "Реализовать выгрузку весов модели и батчинг запросов.", JobTier.Senior, 18000.0, 280000.0),
        ("Крипто-биржа 'Titan'", "Аудит смарт-контракта пула ликвидности", "Найти уязвимости переполнения и reentrancy-атаки.", JobTier.LeadArchitect, 65000.0, 1100000.0)
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadProfileData();
        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
        GenerateJobBoard();
    }

    private void Start()
    {
        BindButtons();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void BindButtons()
    {
        if (openBoardBtn != null)
        {
            openBoardBtn.onClick.RemoveAllListeners();
            openBoardBtn.onClick.AddListener(OpenModal);
        }
        if (closeBoardBtn != null)
        {
            closeBoardBtn.onClick.RemoveAllListeners();
            closeBoardBtn.onClick.AddListener(CloseModal);
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
        if (refreshJobsBtn != null)
        {
            refreshJobsBtn.onClick.RemoveAllListeners();
            refreshJobsBtn.onClick.AddListener(ManualRefreshJobs);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Таймер автообновления биржи
        refreshTimer += dt;
        if (refreshTimer >= AutoRefreshInterval)
        {
            refreshTimer = 0f;
            GenerateJobBoard();
        }

        // Обновление активных контрактов
        bool stateChanged = false;
        for (int i = 0; i < availableJobs.Count; i++)
        {
            var job = availableJobs[i];
            if (job.isActive && !job.isCompleted && !job.isFailed)
            {
                job.timeRemaining -= dt;
                if (job.timeRemaining <= 0f)
                {
                    job.timeRemaining = 0f;
                    job.isFailed = true;
                    job.isActive = false;
                    developerRating = Math.Max(2.5f, developerRating - 0.2f);
                    SaveProfileData();
                    stateChanged = true;

                    if (ClickJuice.Instance != null && modalRoot != null && modalRoot.activeSelf)
                    {
                        ClickJuice.Instance.SpawnCustomPopup($"❌ Контракт '{job.title}' просрочен!", transform.position, new Color(1f, 0.3f, 0.3f), false);
                    }
                }
                else
                {
                    stateChanged = true;
                }
            }
        }

        if (stateChanged && modalRoot != null && modalRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Начисление строк кода в активные контракты
        for (int i = 0; i < availableJobs.Count; i++)
        {
            var job = availableJobs[i];
            if (job.isActive && !job.isCompleted && !job.isFailed)
            {
                job.currentCode += amount;
                if (job.currentCode >= job.requiredCode)
                {
                    CompleteJob(job);
                }
            }
        }
    }

    private void CompleteJob(FreelanceJob job)
    {
        job.currentCode = job.requiredCode;
        job.isCompleted = true;
        job.isActive = false;

        // Расчёт выплаты с бонусом за репутацию
        float repMultiplier = 1f + (developerRating - 3.0f) * 0.15f;
        double finalMoney = job.rewardMoney * Math.Max(1.0, repMultiplier);

        totalJobsCompleted++;
        totalFreelanceEarnings += finalMoney;
        developerRating = Math.Min(5.0f, developerRating + 0.05f);
        SaveProfileData();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(finalMoney);
            GameManager.Instance.AddLinesOfCode(job.bonusCode);
            GameManager.Instance.AddComboEnergy(0.35f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRushSuccess();
        }
        HapticFeedback.SuccessPattern();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 КОНТРАКТ СДАН!\n+{NumberFormatter.Format(finalMoney)} руб.", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    public void AcceptJob(string jobId)
    {
        var job = availableJobs.Find(j => j.id == jobId);
        if (job == null || job.isActive || job.isCompleted || job.isFailed) return;

        job.isActive = true;
        job.currentCode = 0;
        job.timeRemaining = job.timeLimitSeconds;

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"📋 Взят заказ: {job.title}", transform.position, new Color(0.3f, 0.9f, 1f), false);
        }

        RefreshUI();
    }

    public void GenerateJobBoard()
    {
        availableJobs.Clear();
        double playerCpc = GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() : 1.0;
        double playerCps = GameManager.Instance != null ? GameManager.Instance.GetCodePerSecond() : 0.0;
        int prestige = GameManager.Instance != null ? GameManager.Instance.PrestigeLevel : 0;

        // Генерируем 3-4 разноплановых заказа
        int count = UnityEngine.Random.Range(3, 5);
        for (int i = 0; i < count; i++)
        {
            var tmpl = JobTemplates[UnityEngine.Random.Range(0, JobTemplates.Length)];
            double scale = 1.0 + prestige * 0.5 + Math.Max(1.0, playerCpc * 0.15 + playerCps * 0.05);

            double req = Math.Max(200.0, Math.Round(tmpl.BaseReq * scale / 50.0) * 50.0);
            double money = Math.Max(3000.0, Math.Round(tmpl.BaseMoney * scale / 100.0) * 100.0);
            float duration = UnityEngine.Random.Range(60f, 180f);

            availableJobs.Add(new FreelanceJob()
            {
                id = Guid.NewGuid().ToString().Substring(0, 8),
                clientName = tmpl.Client,
                title = tmpl.Title,
                description = tmpl.Desc,
                tier = tmpl.Tier,
                requiredCode = req,
                currentCode = 0,
                rewardMoney = money,
                bonusCode = Math.Round(req * 0.2),
                timeLimitSeconds = duration,
                timeRemaining = duration,
                isActive = false,
                isCompleted = false,
                isFailed = false
            });
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    public void ManualRefreshJobs()
    {
        refreshTimer = 0f;
        GenerateJobBoard();
        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
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

        HapticFeedback.MediumImpact();
        RefreshUI();
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

    private void RefreshUI()
    {
        if (reputationText != null)
        {
            reputationText.text = $"⭐ Рейтинг: <b>{developerRating:F1} / 5.0</b>";
        }
        if (completedStatsText != null)
        {
            completedStatsText.text = $"Выполнено: {totalJobsCompleted} контрактов | Заработано: {NumberFormatter.Format(totalFreelanceEarnings)} ₽";
        }
        if (ratingBonusText != null)
        {
            float bonusPct = (developerRating - 3.0f) * 15f;
            ratingBonusText.text = bonusPct > 0 ? $"<color=#00FF88>Бонус к выплатам: +{bonusPct:F0}%</color>" : "<color=#AAAAAA>Бонус: 0%</color>";
        }

        if (refreshBtnText != null)
        {
            float rem = Math.Max(0f, AutoRefreshInterval - refreshTimer);
            refreshBtnText.text = $"🔄 ОБНОВИТЬ ({rem:F0}с)";
        }

        // Рендер карточек
        if (jobCardsContainer != null)
        {
            for (int i = jobCardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(jobCardsContainer.GetChild(i).gameObject);
            }

            foreach (var job in availableJobs)
            {
                CreateJobCardUI(job);
            }
        }
    }

    private void CreateJobCardUI(FreelanceJob job)
    {
        GameObject cardGo = new GameObject($"JobCard_{job.id}");
        cardGo.transform.SetParent(jobCardsContainer, false);

        Image cardBg = cardGo.AddComponent<Image>();
        if (job.isCompleted) cardBg.color = new Color(0.08f, 0.35f, 0.20f, 0.85f);
        else if (job.isFailed) cardBg.color = new Color(0.35f, 0.10f, 0.10f, 0.85f);
        else if (job.isActive) cardBg.color = new Color(0.12f, 0.25f, 0.45f, 0.85f);
        else cardBg.color = new Color(0.09f, 0.13f, 0.22f, 0.85f);

        RectTransform rt = cardGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 78);

        VerticalLayoutGroup vlg = cardGo.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 6, 6);
        vlg.spacing = 3;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // Строка 1: Заголовок и Клиент
        GameObject headerGo = new GameObject("Header");
        headerGo.transform.SetParent(cardGo.transform, false);
        HorizontalLayoutGroup hlg = headerGo.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        TMP_Text titleTxt = headerGo.AddComponent<TextMeshProUGUI>();
        string tierBadge = job.tier == JobTier.LeadArchitect ? "<color=#FFD166>[LEAD]</color> " : (job.tier == JobTier.Senior ? "<color=#A855F7>[SENIOR]</color> " : "");
        titleTxt.text = $"{tierBadge}<b>{job.title}</b> <size=10><color=#94A3B8>({job.clientName})</color></size>";
        titleTxt.fontSize = 12;

        // Строка 2: Прогресс и Награда
        GameObject row2Go = new GameObject("Row2");
        row2Go.transform.SetParent(cardGo.transform, false);
        TMP_Text row2Txt = row2Go.AddComponent<TextMeshProUGUI>();
        string timeStr = job.isActive ? $"⏰ {job.timeRemaining:F0}с" : $"⏱ {job.timeLimitSeconds:F0}с";
        string statusStr = job.isCompleted ? "<color=#00FF88>✓ СДАНО</color>" : (job.isFailed ? "<color=#FF5555>ПРОСРОЧЕНО</color>" : (job.isActive ? $"<color=#38BDF8>В РАБОТЕ ({job.currentCode:N0}/{job.requiredCode:N0})</color>" : $"{job.requiredCode:N0} строк"));
        row2Txt.text = $"{statusStr}  |  {timeStr}  |  <color=#FFD166>+{NumberFormatter.Format(job.rewardMoney)} ₽</color>";
        row2Txt.fontSize = 11;

        // Кнопка Взять в работу
        if (!job.isActive && !job.isCompleted && !job.isFailed)
        {
            GameObject btnGo = new GameObject("AcceptBtn");
            btnGo.transform.SetParent(cardGo.transform, false);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.65f, 0.45f, 1f);
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => AcceptJob(job.id));

            TMP_Text btnTxt = new GameObject("Txt").AddComponent<TextMeshProUGUI>();
            btnTxt.transform.SetParent(btnGo.transform, false);
            btnTxt.text = "ВЗЯТЬ В РАБОТУ 🚀";
            btnTxt.fontSize = 11;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.alignment = TextAlignmentOptions.Center;
        }
    }

    private void LoadProfileData()
    {
        developerRating = PlayerPrefs.GetFloat("Dev_FreelanceRating", 4.8f);
        totalJobsCompleted = PlayerPrefs.GetInt("Dev_FreelanceCompleted", 0);
        totalFreelanceEarnings = double.Parse(PlayerPrefs.GetString("Dev_FreelanceEarnings", "0"));
    }

    private void SaveProfileData()
    {
        PlayerPrefs.SetFloat("Dev_FreelanceRating", developerRating);
        PlayerPrefs.SetInt("Dev_FreelanceCompleted", totalJobsCompleted);
        PlayerPrefs.SetString("Dev_FreelanceEarnings", totalFreelanceEarnings.ToString());
        PlayerPrefs.Save();
    }
}
