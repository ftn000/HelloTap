using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система срочных фриланс-контрактов и дедлайнов (Freelance Rush Orders).
/// Генерирует горящие заказы от клиентов с таймером на 1.5 - 3.5 минуты.
/// За успешную сдачу в срок игрок получает солидный денежный бонус и строки кода.
/// </summary>
public class FreelanceRushUI : MonoBehaviour
{
    private static FreelanceRushUI instance;
    public static FreelanceRushUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<FreelanceRushUI>();
            return instance;
        }
    }

    public enum RushState
    {
        Idle,           // Ожидание следующего заказа
        OfferPending,   // Заказ поступил, ожидает принятия игроком
        Active,         // Дедлайн тикает, игрок пишет код
        Completed       // Заказ выполнен, ожидает забора награды
    }

    [System.Serializable]
    public class RushContractData
    {
        public string clientName;
        public string title;
        public string description;
        public double targetCode;
        public float totalDuration;
        public double rewardMoney;
        public double rewardCode;
    }

    [Header("Баннер на рабочем столе (Rush Banner Dock)")]
    [SerializeField] private GameObject bannerRoot;
    [SerializeField] private TMP_Text bannerTitleText;
    [SerializeField] private TMP_Text bannerProgressText;
    [SerializeField] private TMP_Text bannerTimerText;
    [SerializeField] private Image bannerProgressFill;
    [SerializeField] private Button bannerActionBtn;
    [SerializeField] private TMP_Text bannerActionBtnText;
    [SerializeField] private Graphic bannerGlowGraphic;

    [Header("Модальное окно контракта (Modal Root)")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCard;
    [SerializeField] private Button closeModalBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;

    [Header("Элементы карточки заказа")]
    [SerializeField] private TMP_Text modalClientText;
    [SerializeField] private TMP_Text modalTitleText;
    [SerializeField] private TMP_Text modalDescriptionText;
    [SerializeField] private TMP_Text modalTargetText;
    [SerializeField] private Image modalProgressFill;
    [SerializeField] private TMP_Text modalProgressPercentText;
    [SerializeField] private TMP_Text modalTimerText;
    [SerializeField] private TMP_Text modalRewardMoneyText;
    [SerializeField] private TMP_Text modalRewardCodeText;
    [SerializeField] private Button modalAcceptBtn;
    [SerializeField] private TMP_Text modalAcceptBtnText;
    [SerializeField] private Button modalRejectBtn;
    [SerializeField] private TMP_Text statsSummaryText;

    private RushState currentState = RushState.Idle;
    private RushContractData activeContract;
    private float stateTimer = 0f;
    private double currentCodeContributed = 0;
    private double lastTotalCode = 0;

    // Статистика
    private int completedContractsCount = 0;
    private double totalFreelanceEarned = 0;

    private static readonly string[] ClientNames = new string[]
    {
        "Финтех 'CryptoPay'",
        "Стартап 'NeuroPulse AI'",
        "Издатель 'PixelForge'",
        "Банк 'Титан Онлайн'",
        "Студия 'CyberQuest'",
        "Агентство 'WebRocket'"
    };

    private static readonly string[] OrderTitles = new string[]
    {
        "Срочный хотфикс краша в проде",
        "MVP стартапа к демо-дню",
        "Оптимизация рендера шейдеров",
        "Интеграция платёжного шлюза",
        "Бэкенд мультиплеера к бета-тесту",
        "Патч утечки памяти GC"
    };

    private static readonly string[] Descriptions = new string[]
    {
        "Инвесторы уже сидят в переговорной! Нужен стабильный билд без крашей.",
        "Прод упал под наплывом трафика. Требуется срочная оптимизация логики!",
        "Издатель выставил жёсткий дедлайн до конца спринта. Гонорар удвоен.",
        "Критический баг в расчётах баланса. Исправь код до открытия биржи!",
        "Релиз через несколько минут, команда не успевает закрыть таски."
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        completedContractsCount = PlayerPrefs.GetInt("Freelance_CompletedCount", 0);
        totalFreelanceEarned = double.Parse(PlayerPrefs.GetString("Freelance_TotalEarned", "0"));

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
        ScheduleNextOrder(12f); // Первый заказ появится быстро (через 12 секунд)
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            lastTotalCode = GameManager.Instance.TotalCodeWritten;
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
        UpdateBannerDisplay();
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
        if (bannerActionBtn != null)
        {
            bannerActionBtn.onClick.RemoveAllListeners();
            bannerActionBtn.onClick.AddListener(OnBannerActionClicked);
        }

        if (closeModalBtn != null)
        {
            closeModalBtn.onClick.RemoveAllListeners();
            closeModalBtn.onClick.AddListener(CloseModal);
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

        if (modalAcceptBtn != null)
        {
            modalAcceptBtn.onClick.RemoveAllListeners();
            modalAcceptBtn.onClick.AddListener(OnModalAcceptClicked);
        }

        if (modalRejectBtn != null)
        {
            modalRejectBtn.onClick.RemoveAllListeners();
            modalRejectBtn.onClick.AddListener(OnModalRejectClicked);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Отслеживаем написанный код (как клики, так и пассивный авто-код)
        if (currentState == RushState.Active && GameManager.Instance != null)
        {
            double currentTotal = GameManager.Instance.TotalCodeWritten;
            double delta = currentTotal - lastTotalCode;
            if (delta > 0)
            {
                currentCodeContributed += delta;
            }
            lastTotalCode = currentTotal;

            if (currentCodeContributed >= activeContract.targetCode)
            {
                CompleteContract();
                return;
            }
        }
        else if (GameManager.Instance != null)
        {
            lastTotalCode = GameManager.Instance.TotalCodeWritten;
        }

        // Таймеры состояний
        if (stateTimer > 0f)
        {
            stateTimer -= dt;
            if (stateTimer <= 0f)
            {
                stateTimer = 0f;
                OnTimerExpired();
            }
        }

        UpdateBannerDisplay();
        if (modalRoot != null && modalRoot.activeSelf)
        {
            UpdateModalDisplay();
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Непосредственный отклик клика для синхронизации
        if (currentState == RushState.Active)
        {
            // lastTotalCode учтёт это в Update
        }
    }

    private void OnTimerExpired()
    {
        switch (currentState)
        {
            case RushState.Idle:
                GenerateNewOffer();
                break;

            case RushState.OfferPending:
                // Игрок не успел принять оферту
                ScheduleNextOrder(75f);
                if (ClickJuice.Instance != null && bannerRoot != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup("Заказ истёк! Новый скоро поступит.", bannerRoot.transform.position, new Color(0.7f, 0.7f, 0.7f), false);
                }
                break;

            case RushState.Active:
                // Дедлайн сорван
                FailContract();
                break;
        }
    }

    private void GenerateNewOffer()
    {
        currentState = RushState.OfferPending;
        stateTimer = 60f; // 60 секунд на принятие предложения

        // Масштабирование требований под прогресс игрока
        double cpc = GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() : 1.0;
        double cps = GameManager.Instance != null ? GameManager.Instance.GetCodePerSecond() : 0.0;
        float duration = UnityEngine.Random.Range(100f, 210f); // 1.5 - 3.5 мин

        double estimatedOutput = (cpc * 70.0) + (cps * duration * 0.45);
        double targetCode = Math.Max(250.0, Math.Round(estimatedOutput / 50.0) * 50.0);

        // Награды: рубли и бонусный код
        double baseMoney = targetCode * 18.0 + (cps * 120.0) + UnityEngine.Random.Range(5000f, 25000f);
        double rewardMoney = Math.Max(10000.0, Math.Round(baseMoney / 1000.0) * 1000.0);
        double rewardCode = Math.Round(targetCode * 0.25);

        activeContract = new RushContractData()
        {
            clientName = ClientNames[UnityEngine.Random.Range(0, ClientNames.Length)],
            title = OrderTitles[UnityEngine.Random.Range(0, OrderTitles.Length)],
            description = Descriptions[UnityEngine.Random.Range(0, Descriptions.Length)],
            targetCode = targetCode,
            totalDuration = duration,
            rewardMoney = rewardMoney,
            rewardCode = rewardCode
        };

        currentCodeContributed = 0;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRushAlert();
        }
        HapticFeedback.Vibrate(40);

        if (ClickJuice.Instance != null && bannerRoot != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("⚡ ГОРЯЩИЙ ЗАКАЗ ОТ КЛИЕНТА!", bannerRoot.transform.position, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    public void AcceptContract()
    {
        if (currentState != RushState.OfferPending || activeContract == null) return;

        currentState = RushState.Active;
        stateTimer = activeContract.totalDuration;
        currentCodeContributed = 0;
        if (GameManager.Instance != null)
        {
            lastTotalCode = GameManager.Instance.TotalCodeWritten;
        }

        HapticFeedback.Vibrate(35);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCassetteClick();
        }

        if (ClickJuice.Instance != null && bannerRoot != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("⏱ ДЕДЛАЙН ПОШЁЛ! ТАПАЙ БЫСТРО!", bannerRoot.transform.position, new Color(0.2f, 1f, 0.8f), true);
        }
    }

    private void CompleteContract()
    {
        currentState = RushState.Completed;
        stateTimer = 0f;

        completedContractsCount++;
        totalFreelanceEarned += activeContract.rewardMoney;

        PlayerPrefs.SetInt("Freelance_CompletedCount", completedContractsCount);
        PlayerPrefs.SetString("Freelance_TotalEarned", totalFreelanceEarned.ToString());
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(activeContract.rewardMoney);
            GameManager.Instance.AddLinesOfCode(activeContract.rewardCode);
            GameManager.Instance.AddComboEnergy(0.40f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRushSuccess();
        }
        HapticFeedback.Vibrate(50);

        if (ClickJuice.Instance != null && bannerRoot != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 КОНТРАКТ СДАН В СРОК!\n+{activeContract.rewardMoney:N0} ₽", bannerRoot.transform.position, new Color(0.3f, 1f, 0.5f), true);
        }

        ScheduleNextOrder(90f);
    }

    private void FailContract()
    {
        currentState = RushState.Idle;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCassetteClick();
        }

        if (ClickJuice.Instance != null && bannerRoot != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("⏰ ДЕДЛАЙН СОРВАН! Заказ отменён.", bannerRoot.transform.position, new Color(1f, 0.35f, 0.35f), false);
        }

        ScheduleNextOrder(45f);
    }

    private void ScheduleNextOrder(float delay)
    {
        currentState = RushState.Idle;
        stateTimer = delay;
        activeContract = null;
        currentCodeContributed = 0;
    }

    public void RequestInstantOrder()
    {
        if (currentState == RushState.Active) return;
        GenerateNewOffer();
    }

    #region UI Presentation

    private void UpdateBannerDisplay()
    {
        if (bannerRoot == null) return;

        switch (currentState)
        {
            case RushState.Idle:
                if (bannerTitleText != null) bannerTitleText.text = "⚡ БИРЖА ФРИЛАНСА";
                if (bannerProgressText != null) bannerProgressText.text = $"Поиск заказов... ({Mathf.CeilToInt(stateTimer)}с)";
                if (bannerTimerText != null) bannerTimerText.text = "ОЖИДАНИЕ";
                if (bannerProgressFill != null) bannerProgressFill.fillAmount = 1f - (stateTimer / 90f);
                if (bannerActionBtnText != null) bannerActionBtnText.text = "НАЙТИ";
                if (bannerGlowGraphic != null) bannerGlowGraphic.color = new Color(0.2f, 0.6f, 1f, 0.25f);
                break;

            case RushState.OfferPending:
                if (bannerTitleText != null) bannerTitleText.text = $"⚡ СРОЧНЫЙ: {activeContract?.title}";
                if (bannerProgressText != null) bannerProgressText.text = $"Награда: +{activeContract?.rewardMoney:N0} ₽ ({activeContract?.targetCode:N0} строк)";
                if (bannerTimerText != null) bannerTimerText.text = $"ПРЕДЛОЖЕНИЕ [{Mathf.CeilToInt(stateTimer)}с]";
                if (bannerProgressFill != null) bannerProgressFill.fillAmount = stateTimer / 60f;
                if (bannerActionBtnText != null) bannerActionBtnText.text = "ПРИНЯТЬ";
                if (bannerGlowGraphic != null)
                {
                    float pulse = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
                    bannerGlowGraphic.color = new Color(1f, 0.85f, 0.2f, 0.35f + pulse * 0.45f);
                }
                break;

            case RushState.Active:
                float rem = Mathf.Max(0f, stateTimer);
                int mm = Mathf.FloorToInt(rem / 60f);
                int ss = Mathf.FloorToInt(rem % 60f);
                double target = activeContract != null ? activeContract.targetCode : 1.0;
                float progressRatio = Mathf.Clamp01((float)(currentCodeContributed / target));

                if (bannerTitleText != null) bannerTitleText.text = $"⏱ ДЕДЛАЙН: {activeContract?.title}";
                if (bannerProgressText != null) bannerProgressText.text = $"{currentCodeContributed:N0} / {target:N0} строк ({(progressRatio * 100f):F0}%)";
                if (bannerTimerText != null) bannerTimerText.text = $"{mm:00}:{ss:00}";
                if (bannerProgressFill != null) bannerProgressFill.fillAmount = progressRatio;
                if (bannerActionBtnText != null) bannerActionBtnText.text = "ОБЗОР";

                if (bannerGlowGraphic != null)
                {
                    if (rem < 25f)
                    {
                        float redPulse = (Mathf.Sin(Time.time * 9f) + 1f) * 0.5f;
                        bannerGlowGraphic.color = new Color(1f, 0.2f, 0.2f, 0.4f + redPulse * 0.5f);
                    }
                    else
                    {
                        bannerGlowGraphic.color = new Color(0.2f, 0.95f, 0.65f, 0.35f);
                    }
                }
                break;

            case RushState.Completed:
                if (bannerTitleText != null) bannerTitleText.text = "🎉 ЗАКАЗ ВЫПОЛНЕН!";
                if (bannerProgressText != null) bannerProgressText.text = "Гонорар начислен на баланс!";
                if (bannerTimerText != null) bannerTimerText.text = "УСПЕХ";
                if (bannerProgressFill != null) bannerProgressFill.fillAmount = 1f;
                if (bannerActionBtnText != null) bannerActionBtnText.text = "ОТЛИЧНО";
                if (bannerGlowGraphic != null) bannerGlowGraphic.color = new Color(0.2f, 1f, 0.5f, 0.5f);
                break;
        }
    }

    private void UpdateModalDisplay()
    {
        if (modalClientText != null) modalClientText.text = activeContract != null ? $"🏢 {activeContract.clientName}" : "🏢 Биржа Dev Freelance";
        if (modalTitleText != null) modalTitleText.text = activeContract != null ? activeContract.title : "Горящих заказов пока нет";
        if (modalDescriptionText != null) modalDescriptionText.text = activeContract != null ? activeContract.description : "Срочные заказы поступают автоматически. Вы также можете нажать кнопку ниже для ручного поиска.";

        double target = activeContract != null ? activeContract.targetCode : 1.0;
        float ratio = Mathf.Clamp01((float)(currentCodeContributed / target));

        if (modalTargetText != null) modalTargetText.text = activeContract != null ? $"Цель: {activeContract.targetCode:N0} строк кода" : "Цель: —";
        if (modalProgressFill != null) modalProgressFill.fillAmount = ratio;
        if (modalProgressPercentText != null) modalProgressPercentText.text = activeContract != null ? $"{currentCodeContributed:N0} / {target:N0} ({(ratio * 100f):F0}%)" : "0%";

        if (modalRewardMoneyText != null) modalRewardMoneyText.text = activeContract != null ? $"+{activeContract.rewardMoney:N0} ₽" : "+0 ₽";
        if (modalRewardCodeText != null) modalRewardCodeText.text = activeContract != null ? $"+{activeContract.rewardCode:N0} строк" : "+0 строк";

        if (modalTimerText != null)
        {
            if (currentState == RushState.Active)
            {
                float rem = Mathf.Max(0f, stateTimer);
                int mm = Mathf.FloorToInt(rem / 60f);
                int ss = Mathf.FloorToInt(rem % 60f);
                modalTimerText.text = $"⏰ До дедлайна: {mm:00}:{ss:00}";
            }
            else if (currentState == RushState.OfferPending)
            {
                modalTimerText.text = $"⏱ Время на ответ: {Mathf.CeilToInt(stateTimer)}с";
            }
            else
            {
                modalTimerText.text = $"💤 Поиск заказа ({Mathf.CeilToInt(stateTimer)}с)";
            }
        }

        if (modalAcceptBtnText != null)
        {
            if (currentState == RushState.OfferPending) modalAcceptBtnText.text = "ВЗЯТЬ В РАБОТУ";
            else if (currentState == RushState.Active) modalAcceptBtnText.text = "ТАПАТЬ КОД";
            else modalAcceptBtnText.text = "НАЙТИ ЗАКАЗ";
        }

        if (modalRejectBtn != null)
        {
            modalRejectBtn.gameObject.SetActive(currentState == RushState.OfferPending || currentState == RushState.Active);
        }

        if (statsSummaryText != null)
        {
            statsSummaryText.text = $"📊 Карьера: {completedContractsCount} выполненных контрактов | Заработано: {totalFreelanceEarned:N0} ₽";
        }
    }

    #endregion

    #region Modal Actions

    public void OpenModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCard != null) StartCoroutine(PopInRoutine(modalCard));
        }
        UpdateModalDisplay();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
    }

    private void OnBannerActionClicked()
    {
        if (currentState == RushState.OfferPending)
        {
            AcceptContract();
        }
        else if (currentState == RushState.Idle)
        {
            RequestInstantOrder();
        }
        else
        {
            OpenModal();
        }
    }

    private void OnModalAcceptClicked()
    {
        if (currentState == RushState.OfferPending)
        {
            AcceptContract();
            CloseModal();
        }
        else if (currentState == RushState.Idle)
        {
            RequestInstantOrder();
        }
        else
        {
            CloseModal();
        }
    }

    private void OnModalRejectClicked()
    {
        if (currentState == RushState.OfferPending)
        {
            ScheduleNextOrder(30f);
            CloseModal();
        }
        else if (currentState == RushState.Active)
        {
            FailContract();
            CloseModal();
        }
    }

    private IEnumerator PopInRoutine(Transform target)
    {
        if (target == null) yield break;
        target.localScale = Vector3.one * 0.88f;
        float duration = 0.22f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            target.localScale = Vector3.Lerp(Vector3.one * 0.88f, Vector3.one, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    #endregion
}
