using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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
    private int lastClickFrame = -1;
    private DevShopUI cachedShopUI;

    private void Awake()
    {
        EnsureGameManagerExists();
        EnsureEventSystemExists();
        cachedShopUI = Object.FindFirstObjectByType<DevShopUI>();
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

    private void EnsureEventSystemExists()
    {
        EventSystem existing = Object.FindFirstObjectByType<EventSystem>();
        if (existing == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }
        else if (existing.GetComponent<BaseInputModule>() == null)
        {
            existing.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= UpdateUI;
            GameManager.Instance.OnCurrenciesChanged += UpdateUI;
        }

        BindButtons();
        UpdateUI();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= UpdateUI;
            GameManager.Instance.OnCurrenciesChanged += UpdateUI;
        }

        BindButtons();
        UpdateUI();
    }

    private void BindButtons()
    {
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

    private void LateUpdate()
    {
        if (cachedShopUI == null)
        {
            cachedShopUI = Object.FindFirstObjectByType<DevShopUI>();
        }

        // Если открыто модальное окно магазина/проектов — не начисляем тапы по фону
        if (cachedShopUI != null && cachedShopUI.IsModalOpen)
        {
            return;
        }

        // Если клик уже сработал через tapButton.onClick в этом кадре — не дублируем
        if (lastClickFrame == Time.frameCount)
        {
            return;
        }

        // 1. Нажатие любой клавиши на клавиатуре (печать кода!)
        bool keyPressed = false;
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame &&
            !Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            keyPressed = true;
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!keyPressed && Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape) &&
            !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
        {
            keyPressed = true;
        }
#endif
        if (keyPressed)
        {
            IncrementWithPosition(Vector2.zero);
            return;
        }

        // 2. Проверка клика мыши или касания экрана (Touch)
        bool pointerPressed = false;
        Vector2 pointerPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            pointerPressed = true;
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointerPressed = true;
            pointerPos = Mouse.current.position.ReadValue();
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        if (!pointerPressed && Input.GetMouseButtonDown(0))
        {
            pointerPressed = true;
            pointerPos = Input.mousePosition;
        }
#endif

        if (pointerPressed)
        {
            // Проверяем, не нажал ли игрок на служебную кнопку (Магазин, Проекты, Звук, Сброс, Энергетик)
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if (tapButton != null && selected != tapButton.gameObject && selected.GetComponent<Button>() != null)
                {
                    return;
                }
            }

            // Проверяем попадание в центральную рабочую зону 9:16 (TapButton)
            if (tapButton != null)
            {
                RectTransform tapRt = tapButton.GetComponent<RectTransform>();
                if (tapRt != null && RectTransformUtility.RectangleContainsScreenPoint(tapRt, pointerPos, null))
                {
                    IncrementWithPosition(pointerPos);
                }
            }
            else
            {
                float normalizedY = Screen.height > 0 ? pointerPos.y / Screen.height : 0.5f;
                if (normalizedY >= 0.10f && normalizedY <= 0.86f)
                {
                    IncrementWithPosition(pointerPos);
                }
            }
        }
    }

    /// <summary>
    /// Увеличивает счётчик на 1 клик с указанием позиции нажатия.
    /// </summary>
    public void IncrementWithPosition(Vector2 clickPos)
    {
        lastClickFrame = Time.frameCount;
        counter++;

        if (clickPos == Vector2.zero && tapButton != null)
        {
            clickPos = (Vector2)tapButton.transform.position;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClickCode(clickPos);
        }

        UpdateUI();
    }

    /// <summary>
    /// Увеличивает счётчик на 1 клик, передает действие в GameManager и обновляет UI.
    /// </summary>
    public void Increment()
    {
        Vector2 clickPos = Vector2.zero;
        if (Pointer.current != null)
        {
            clickPos = Pointer.current.position.ReadValue();
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        if (clickPos == Vector2.zero)
        {
            clickPos = Input.mousePosition;
        }
#endif
        if (clickPos == Vector2.zero && tapButton != null)
        {
            clickPos = (Vector2)tapButton.transform.position;
        }
        IncrementWithPosition(clickPos);
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBoost();
            }
            UpdateUI();
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
            double passiveMoney = GameManager.Instance.GetMoneyPerSecond();
            if (passiveMoney > 0)
            {
                moneyText.text = $"Баланс: {NumberFormatter.Format(GameManager.Instance.Money)} руб. (+{NumberFormatter.Format(passiveMoney)}/сек)";
            }
            else
            {
                moneyText.text = $"Баланс: {NumberFormatter.Format(GameManager.Instance.Money)} руб.";
            }
        }

        if (statsText != null && GameManager.Instance != null)
        {
            double clickPower = GameManager.Instance.GetCodePerClick();
            double perSec = GameManager.Instance.GetCodePerSecond();
            statsText.text = $"+{NumberFormatter.Format(clickPower)} за клик | +{NumberFormatter.Format(perSec)} строк/сек";
        }

        if (boostTimerText != null && GameManager.Instance != null)
        {
            boostTimerText.gameObject.SetActive(true);
            if (GameManager.Instance.IsBoostActive)
            {
                boostTimerText.text = $"АКТИВНО: {NumberFormatter.FormatTime(GameManager.Instance.BoostTimeRemaining)}";
                boostTimerText.color = new Color(0.2f, 1.0f, 0.55f, 1f);
            }
            else
            {
                boostTimerText.text = "ГОТОВО (30 СЕК)";
                boostTimerText.color = new Color(1.0f, 0.9f, 0.4f, 1f);
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
