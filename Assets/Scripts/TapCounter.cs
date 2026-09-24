using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Скрипт счётчика тапов "Hello Tap" с поддержкой сохранения в PlayerPrefs,
/// смены цвета текста каждые 10 очков через Color.Lerp и кнопки сброса.
/// </summary>
public class TapCounter : MonoBehaviour
{
    private const string ScoreKey = "TapCounter_Score";

    [Header("UI Ссылки")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Button tapButton;
    [SerializeField] private Button resetButton;

    [Header("Настройки текста")]
    [SerializeField] private string scorePrefix = "Счёт: ";

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
        // Загрузка сохранённого счёта из PlayerPrefs при старте
        LoadScore();
    }

    private void OnEnable()
    {
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
    }

    private void OnDisable()
    {
        // Отписка от событий при отключении компонента
        if (tapButton != null)
        {
            tapButton.onClick.RemoveListener(Increment);
        }

        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(Reset);
        }

        // Сохранение при выгрузке / деактивации объекта
        SaveScore();
    }

    /// <summary>
    /// Сохранение счёта при сворачивании игры на мобильном устройстве (Android).
    /// На мобильных ОС OnApplicationPause вызывается надёжнее, чем OnApplicationQuit.
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveScore();
        }
    }

    /// <summary>
    /// Сохранение счёта при штатном выходе из приложения.
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveScore();
    }

    /// <summary>
    /// Увеличивает счётчик на 1 очко и обновляет UI.
    /// Метод публичный для возможности вызова через Unity Inspector (OnClick).
    /// </summary>
    public void Increment()
    {
        counter++;
        UpdateUI();
    }

    /// <summary>
    /// Сбрасывает счётчик до 0, сохраняет результат и обновляет UI.
    /// Метод публичный для возможности вызова через Unity Inspector (OnClick).
    /// </summary>
    public void Reset()
    {
        counter = 0;
        SaveScore();
        UpdateUI();
    }

    /// <summary>
    /// Сохраняет текущее значение счётчика в постоянное хранилище PlayerPrefs.
    /// </summary>
    public void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, counter);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Загружает счётчик из PlayerPrefs (по умолчанию 0, если данных нет).
    /// </summary>
    public void LoadScore()
    {
        counter = PlayerPrefs.GetInt(ScoreKey, 0);
    }

    /// <summary>
    /// Обновляет текстовое отображение и цвет счёта.
    /// </summary>
    private void UpdateUI()
    {
        if (counterText != null)
        {
            counterText.text = $"{scorePrefix}{counter}";
            UpdateTextColor();
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

        // Текущий цветовой этап (0 при 0..9, 1 при 10..19 и т.д.)
        int currentStage = (counter / 10) % stageColors.Length;
        int nextStage = (currentStage + 1) % stageColors.Length;

        // Доля прогресса внутри текущей десятки [0.0 .. 1.0)
        float t = (counter % 10) / 10f;

        // Плавная интерполяция цвета между этапами
        counterText.color = Color.Lerp(stageColors[currentStage], stageColors[nextStage], t);
    }
}

