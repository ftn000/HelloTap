using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Скрипт счётчика тапов "Hello Tap" / "GameDev Clicker".
/// Поддерживает обратную совместимость с базовой лабораторной (смена цвета текста, PlayerPrefs),
/// а также интегрируется с GameManager для системы разработки игр, улучшений и пассивного дохода.
/// </summary>
public class TapCounter : MonoBehaviour
{
    private const string ScoreKey = "TapCounter_Score";

    [Header("Базовые UI Ссылки (Лабораторная работа)")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Button tapButton;
    [SerializeField] private Button resetButton;

    [Header("Настройки текста")]
    [SerializeField] private string scorePrefix = "Строк кода: ";

    [Header("Дополнительные UI Ссылки (GameDev Idle)")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text boostTimerText;
    [SerializeField] private Button energyBoostButton;

    [Header("Цветовая палитра (смена каждые 10 очков)")]
    [SerializeField] private Color[] stageColors = new Color[]
    {
        Color.white,                         // 0-9: Белый
        new Color(1.0f, 0.84f, 0.0f),        // 10-19: Золотой/Жёлтый
        new Color(0.2f, 0.85f, 0.3f),        // 20-29: Зелёный
        new Color(0.2f, 0.75f, 1.0f),        // 30-39: Голубой
        new Color(0.9f, 0.40f, 1.0f),        // 40-49: Фиолетовый
        new Color(1.0f, 0.50f, 0.1f),        // 50-59: Оранжевый
        new Color(1.0f, 0.25f, 0.35f)        // 60+: Кораллово-красный
    };

    private int counter;

    private void Awake()
    {
        EnsureGameManagerExists();
        LoadScore();
    }

    private void EnsureGameManagerExists()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged += UpdateUI;
        }

        UpdateUI();

        // Подписка на нажатия кнопок (с защитой от повторной подписки)
        if (tapButton != null)
        {
            tapButton.onClick.RemoveListener(Increment);
            tapButton.onClick.AddListener(Increment);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(Reset);
            resetButton.onClick.AddListener(Reset);
        }

        if (energyBoostButton != null)
        {
            energyBoostButton.onClick.RemoveListener(OnEnergyBoostClicked);
            energyBoostButton.onClick.AddListener(OnEnergyBoostClicked);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= UpdateUI;
        }

        if (tapButton != null)
        {
            tapButton.onClick.RemoveListener(Increment);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(Reset);
        }

        if (energyBoostButton != null)
        {
            energyBoostButton.onClick.RemoveListener(OnEnergyBoostClicked);
        }

        SaveScore();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveScore();
        }
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }

    /// <summary>
    /// Увеличивает счётчик на 1 клик, передает действие в GameManager и обновляет UI.
    /// </summary>
    public void Increment()
    {
        counter++;

        Vector2 clickPos = tapButton != null ? (Vector2)tapButton.transform.position : Vector2.zero;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClickCode(clickPos);
        }

        UpdateUI();
    }

    /// <summary>
    /// Сбрасывает счётчики, сохраняет результат и обновляет UI.
    /// </summary>
    public void Reset()
    {
        counter = 0;
        SaveScore();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllData();
        }

        UpdateUI();
    }

    private void OnEnergyBoostClicked()
    {
        if (GameManager.Instance != null)
        {
            // Включает буст энергетика (x2 на 30 секунд)
            GameManager.Instance.ActivateEnergyBoost(30f, 2.0);
        }
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, counter);
        PlayerPrefs.Save();
    }

    public void LoadScore()
    {
        counter = PlayerPrefs.GetInt(ScoreKey, 0);
    }

    /// <summary>
    /// Обновляет текстовое отображение счёта, денег и статов.
    /// </summary>
    public void UpdateUI()
    {
        if (counterText != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.CodeLines > 0)
            {
                counterText.text = $"{scorePrefix}{NumberFormatter.Format(GameManager.Instance.CodeLines)}";
            }
            else
            {
                counterText.text = $"{scorePrefix}{counter}";
            }
            UpdateTextColor();
        }

        if (moneyText != null && GameManager.Instance != null)
        {
            moneyText.text = $"Баланс: {NumberFormatter.Format(GameManager.Instance.Money)} ₽";
        }

        if (statsText != null && GameManager.Instance != null)
        {
            double clickPower = GameManager.Instance.GetCodePerClick();
            double perSec = GameManager.Instance.GetCodePerSecond();
            statsText.text = $"+{NumberFormatter.Format(clickPower)} за клик | +{NumberFormatter.Format(perSec)} строк/сек";
        }

        if (boostTimerText != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.IsBoostActive)
            {
                boostTimerText.text = $"ЭНЕРГЕТИК x2: {NumberFormatter.FormatTime(GameManager.Instance.BoostTimeRemaining)}";
                boostTimerText.gameObject.SetActive(true);
            }
            else
            {
                boostTimerText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Меняет цвет текста счёта: каждые 10 очков цвет плавно переходит
    /// к следующему оттенку палитры через Color.Lerp.
    /// </summary>
    private void UpdateTextColor()
    {
        if (counterText == null || stageColors == null || stageColors.Length == 0)
        {
            return;
        }

        if (stageColors.Length == 1)
        {
            counterText.color = stageColors[0];
            return;
        }

        int scoreForColor = GameManager.Instance != null ? (int)(GameManager.Instance.CodeLines) : counter;
        int currentStage = (scoreForColor / 10) % stageColors.Length;
        int nextStage = (currentStage + 1) % stageColors.Length;

        float t = (scoreForColor % 10) / 10f;
        counterText.color = Color.Lerp(stageColors[currentStage], stageColors[nextStage], t);
    }
}
