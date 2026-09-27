using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Аналитика сессии и график продуктивности разработчика:
/// - Подсчет времени сессии (HH:MM:SS)
/// - Строк кода и рублей за текущую сессию
/// - Расчет текущего и пикового CPM (Clicks Per Minute)
/// - График продуктивности за последние 8 минут (Bar Chart)
/// - Модальное окно дашборда
/// </summary>
public class SessionAnalyticsUI : MonoBehaviour
{
    private static SessionAnalyticsUI instance;
    public static SessionAnalyticsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SessionAnalyticsUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и закрытия")]
    [SerializeField] private Button openAnalyticsBtn;
    [SerializeField] private Button closeAnalyticsBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;

    [Header("Текстовые метрики")]
    [SerializeField] private TMP_Text timeInFlowText;
    [SerializeField] private TMP_Text sessionCodeText;
    [SerializeField] private TMP_Text sessionMoneyText;
    [SerializeField] private TMP_Text currentCpmText;
    [SerializeField] private TMP_Text peakCpmText;
    [SerializeField] private TMP_Text bugsAndProjectsText;

    [Header("Столбчатый график продуктивности")]
    [SerializeField] private RectTransform[] chartBars;
    [SerializeField] private TMP_Text[] chartBarLabels;

    private float sessionStartTime = 0f;
    private double startSessionCode = 0;
    private double startSessionMoney = 0;
    private bool isInitialized = false;

    // Расчет CPM
    private readonly Queue<float> clickTimestamps = new Queue<float>();
    private float currentCpm = 0f;
    private float peakCpm = 0f;

    // История продуктивности (8 бакетов по 60 секунд)
    private readonly float[] minuteCodeHistory = new float[8];
    private float minuteTimer = 0f;
    private float currentMinuteCode = 0f;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

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
        sessionStartTime = Time.time;
        BindButtons();
        if (GameManager.Instance != null)
        {
            startSessionCode = GameManager.Instance.TotalCodeWritten;
            startSessionMoney = GameManager.Instance.TotalMoneyEarned;
            isInitialized = true;
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
        if (openAnalyticsBtn != null)
        {
            openAnalyticsBtn.onClick.RemoveAllListeners();
            openAnalyticsBtn.onClick.AddListener(OpenModal);
        }

        if (closeAnalyticsBtn != null)
        {
            closeAnalyticsBtn.onClick.RemoveAllListeners();
            closeAnalyticsBtn.onClick.AddListener(CloseModal);
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

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        float now = Time.time;
        clickTimestamps.Enqueue(now);
        currentMinuteCode += (float)amount;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float now = Time.time;

        // 1. Очистка старых кликов за пределами 5 секунд для расчета CPM
        while (clickTimestamps.Count > 0 && now - clickTimestamps.Peek() > 5.0f)
        {
            clickTimestamps.Dequeue();
        }

        // CPM = (clicks in 5s window / 5.0) * 60
        currentCpm = (clickTimestamps.Count / 5.0f) * 60f;
        if (currentCpm > peakCpm)
        {
            peakCpm = currentCpm;
        }

        // 2. Бакеты продуктивности по минутам
        minuteTimer += dt;
        if (minuteTimer >= 60f)
        {
            minuteTimer = 0f;
            // Сдвиг истории влево
            for (int i = 0; i < minuteCodeHistory.Length - 1; i++)
            {
                minuteCodeHistory[i] = minuteCodeHistory[i + 1];
            }
            minuteCodeHistory[minuteCodeHistory.Length - 1] = currentMinuteCode;
            currentMinuteCode = 0f;
        }

        if (IsModalOpen)
        {
            RefreshUI();
        }
    }

    public void OpenModal()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            modalRoot.transform.SetAsLastSibling();
        }
        RefreshUI();
    }

    public void CloseModal()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMouseClick();
        if (modalRoot != null) modalRoot.SetActive(false);
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null) return;
        if (!isInitialized)
        {
            startSessionCode = GameManager.Instance.TotalCodeWritten;
            startSessionMoney = GameManager.Instance.TotalMoneyEarned;
            isInitialized = true;
        }

        float sessionSeconds = Time.time - sessionStartTime;
        int hours = (int)(sessionSeconds / 3600);
        int mins = (int)((sessionSeconds % 3600) / 60);
        int secs = (int)(sessionSeconds % 60);

        if (timeInFlowText != null)
        {
            timeInFlowText.text = $"⏱ Время в потоке: {hours:D2}:{mins:D2}:{secs:D2}";
        }

        double sessionCode = Math.Max(0, GameManager.Instance.TotalCodeWritten - startSessionCode);
        double sessionMoney = Math.Max(0, GameManager.Instance.TotalMoneyEarned - startSessionMoney);

        if (sessionCodeText != null)
        {
            sessionCodeText.text = $"{NumberFormatter.Format(sessionCode)} строк";
        }

        if (sessionMoneyText != null)
        {
            sessionMoneyText.text = $"{NumberFormatter.Format(sessionMoney)} руб.";
        }

        if (currentCpmText != null)
        {
            currentCpmText.text = $"{currentCpm:F0} кликов/мин";
        }

        if (peakCpmText != null)
        {
            peakCpmText.text = $"{peakCpm:F0} кликов/мин";
        }

        if (bugsAndProjectsText != null)
        {
            bugsAndProjectsText.text = $"Багов устранено: {GameManager.Instance.BugsFixedCount}  |  Релизов: {GameManager.Instance.TotalReleasesCount}";
        }

        // Обновление графика
        UpdateChartBars();
    }

    private void UpdateChartBars()
    {
        if (chartBars == null || chartBars.Length == 0) return;

        float maxVal = 10f;
        for (int i = 0; i < minuteCodeHistory.Length; i++)
        {
            if (minuteCodeHistory[i] > maxVal) maxVal = minuteCodeHistory[i];
        }

        float maxBarHeight = 120f;
        for (int i = 0; i < chartBars.Length; i++)
        {
            if (chartBars[i] == null) continue;
            float val = i < minuteCodeHistory.Length ? minuteCodeHistory[i] : 0f;
            if (i == chartBars.Length - 1) val = Mathf.Max(val, currentMinuteCode);

            float targetH = Mathf.Clamp((val / maxVal) * maxBarHeight, 6f, maxBarHeight);
            chartBars[i].sizeDelta = new Vector2(chartBars[i].sizeDelta.x, targetH);

            if (chartBarLabels != null && i < chartBarLabels.Length && chartBarLabels[i] != null)
            {
                chartBarLabels[i].text = NumberFormatter.Format(val);
            }
        }
    }
}
