using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Колесо фортуны разработчика (Dev Lucky Wheel):
/// - 8 секторов с разнообразными ценными наградами (рубли, строки кода, бусты, джекпот)
/// - Ежедневный бесплатный спин (с таймером кулдауна) + покупной спин за рубли
/// - Плавное вращение с физическим замедлением, тиканьем спиц и тактильным откликом
/// - Торжественные фанфары при выигрыше и всплывающие попап-сообщения
/// </summary>
public class LuckyWheelUI : MonoBehaviour
{
    private static LuckyWheelUI instance;
    public static LuckyWheelUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<LuckyWheelUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openWheelBtn;
    [SerializeField] private TMP_Text openWheelBtnText;
    [SerializeField] private GameObject wheelBadgeDot;
    [SerializeField] private Button closeWheelBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Диск колеса")]
    [SerializeField] private RectTransform wheelDiscTransform;

    [Header("Кнопки вращения")]
    [SerializeField] private Button freeSpinBtn;
    [SerializeField] private TMP_Text freeSpinBtnText;
    [SerializeField] private Button paidSpinBtn;
    [SerializeField] private TMP_Text paidSpinBtnText;
    [SerializeField] private TMP_Text statusDescText;

    [Header("Звуки")]
    [SerializeField] private AudioClip wheelTickSound;
    [SerializeField] private AudioClip wheelWinSound;

    public struct SectorReward
    {
        public string Title;
        public double Money;
        public double Code;
        public float BoostSeconds;
        public int Weight; // Для весов вероятностей

        public SectorReward(string title, double money, double code, float boost, int weight)
        {
            Title = title;
            Money = money;
            Code = code;
            BoostSeconds = boost;
            Weight = weight;
        }
    }

    // 8 секторов (угол каждого сектора = 45 градусов)
    private static readonly SectorReward[] Sectors = new SectorReward[]
    {
        new SectorReward("+5 000 руб. 💰", 5000.0, 0.0, 0f, 25),
        new SectorReward("+2 000 строк 💻", 0.0, 2000.0, 0f, 25),
        new SectorReward("⚡ БУСТ x2 (30с)", 0.0, 0.0, 30f, 15),
        new SectorReward("+25 000 руб. 💰", 25000.0, 0.0, 0f, 12),
        new SectorReward("+10 000 строк 💻", 0.0, 10000.0, 0f, 12),
        new SectorReward("🎁 ДЖЕКПОТ 100K!", 100000.0, 25000.0, 60f, 3),
        new SectorReward("⚡ МЕГА-БУСТ x3 (60с)", 0.0, 0.0, 60f, 4),
        new SectorReward("🐛 БАГ-КЛИНИНГ +15K", 15000.0, 5000.0, 0f, 10)
    };

    private const string PrefLastFreeSpin = "LuckyWheel_LastFreeSpinTime";
    private const double PaidSpinCost = 10000.0;
    private const float FreeSpinCooldownHours = 18f;

    private bool isSpinning = false;
    private Coroutine spinCoroutine;

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
        BindButtons();
        UpdateUI();
    }

    private void Update()
    {
        // Если модалка открыта или проверяем таймер бесплатного спина
        if (openWheelBtnText != null || (modalRoot != null && modalRoot.activeInHierarchy))
        {
            UpdateBadgeDot();
        }
    }

    private void BindButtons()
    {
        if (openWheelBtn != null)
        {
            openWheelBtn.onClick.RemoveAllListeners();
            openWheelBtn.onClick.AddListener(OpenModal);
        }
        if (closeWheelBtn != null)
        {
            closeWheelBtn.onClick.RemoveAllListeners();
            closeWheelBtn.onClick.AddListener(CloseModal);
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
        if (freeSpinBtn != null)
        {
            freeSpinBtn.onClick.RemoveAllListeners();
            freeSpinBtn.onClick.AddListener(() => StartSpin(true));
        }
        if (paidSpinBtn != null)
        {
            paidSpinBtn.onClick.RemoveAllListeners();
            paidSpinBtn.onClick.AddListener(() => StartSpin(false));
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
        HapticFeedback.Vibrate(20);
        UpdateUI();
    }

    public void CloseModal()
    {
        if (isSpinning) return; // Нельзя закрывать во время вращения
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
            float scale = Mathf.Lerp(0.85f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    public bool CanFreeSpin()
    {
        string lastStr = PlayerPrefs.GetString(PrefLastFreeSpin, "");
        if (string.IsNullOrEmpty(lastStr)) return true;

        if (DateTime.TryParse(lastStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            TimeSpan diff = DateTime.UtcNow - lastTime;
            return diff.TotalHours >= FreeSpinCooldownHours;
        }
        return true;
    }

    public TimeSpan GetFreeSpinRemainingTime()
    {
        string lastStr = PlayerPrefs.GetString(PrefLastFreeSpin, "");
        if (string.IsNullOrEmpty(lastStr)) return TimeSpan.Zero;

        if (DateTime.TryParse(lastStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastTime))
        {
            TimeSpan diff = DateTime.UtcNow - lastTime;
            TimeSpan target = TimeSpan.FromHours(FreeSpinCooldownHours);
            if (diff < target)
            {
                return target - diff;
            }
        }
        return TimeSpan.Zero;
    }

    public void StartSpin(bool isFree)
    {
        if (isSpinning) return;

        if (isFree)
        {
            if (!CanFreeSpin())
            {
                // На кулдауне: предлагаем крутить за просмотр рекламы
                YandexSDKBridge.Instance.ShowRewardedAd(
                    "lucky_wheel_ad_spin",
                    onRewarded: () =>
                    {
                        ExecuteSpinReward();
                    },
                    onClose: null,
                    onError: (err) =>
                    {
                        ExecuteSpinReward();
                    });
                return;
            }

            PlayerPrefs.SetString(PrefLastFreeSpin, DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            ExecuteSpinReward();
        }
        else
        {
            if (GameManager.Instance == null || GameManager.Instance.Money < PaidSpinCost)
            {
                if (statusDescText != null)
                {
                    statusDescText.text = $"<color=#FF5555>Недостаточно средств! Требуется {PaidSpinCost:N0} руб.</color>";
                }
                HapticFeedback.WarningHaptic();
                return;
            }
            GameManager.Instance.SpendMoney(PaidSpinCost);
            ExecuteSpinReward();
        }
    }

    private void ExecuteSpinReward()
    {
        // Выбираем сектор с учётом весов
        int targetSector = PickWeightedSector();
        if (spinCoroutine != null) StopCoroutine(spinCoroutine);
        spinCoroutine = StartCoroutine(SpinRoutine(targetSector));
    }

    private int PickWeightedSector()
    {
        int totalWeight = 0;
        for (int i = 0; i < Sectors.Length; i++) totalWeight += Sectors[i].Weight;

        int rnd = UnityEngine.Random.Range(0, totalWeight);
        int acc = 0;
        for (int i = 0; i < Sectors.Length; i++)
        {
            acc += Sectors[i].Weight;
            if (rnd < acc) return i;
        }
        return 0;
    }

    private IEnumerator SpinRoutine(int targetSector)
    {
        isSpinning = true;
        UpdateButtonsInteractable(false);

        if (statusDescText != null)
        {
            statusDescText.text = "🎡 Колесо фортуны вращается... Удачи!";
        }

        float startAngle = wheelDiscTransform != null ? wheelDiscTransform.eulerAngles.z : 0f;
        // Стрелка указателя сверху (угол 90 градусов)
        // Чтобы целевой сектор targetSector оказался под стрелкой:
        // Sector i занимает угол i * 45 градусов
        float sectorAngle = targetSector * 45f;
        int fullRevolutions = UnityEngine.Random.Range(5, 7);
        float totalRotation = (fullRevolutions * 360f) + sectorAngle;

        float duration = 4.0f;
        float elapsed = 0f;
        float lastTickAngle = startAngle;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Плавное замедление (Кубический Ease-Out)
            float ease = 1f - Mathf.Pow(1f - t, 3.5f);
            float currentAngle = startAngle + totalRotation * ease;

            if (wheelDiscTransform != null)
            {
                wheelDiscTransform.eulerAngles = new Vector3(0, 0, currentAngle);
            }

            // Тиканье при прохождении каждых 45 градусов
            if (Mathf.Abs(currentAngle - lastTickAngle) >= 45f)
            {
                lastTickAngle = currentAngle;
                PlayTick();
                HapticFeedback.Vibrate(12);
            }

            yield return null;
        }

        float finalAngle = startAngle + totalRotation;
        if (wheelDiscTransform != null)
        {
            wheelDiscTransform.eulerAngles = new Vector3(0, 0, finalAngle);
        }

        // Выдача награды
        SectorReward reward = Sectors[targetSector];
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(reward.Code, reward.Money);
            if (reward.BoostSeconds > 0f)
            {
                GameManager.Instance.ActivateEnergyBoost(reward.BoostSeconds);
            }
        }

        // Звук победы
        PlayWin();
        HapticFeedback.Vibrate(50);

        if (ClickJuice.Instance != null && wheelDiscTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 {reward.Title}", wheelDiscTransform.position, new Color(1f, 0.9f, 0.2f), true);
        }

        if (statusDescText != null)
        {
            statusDescText.text = $"<color=#00FF88>Выигрыш:</color> <b>{reward.Title}</b>!";
        }

        isSpinning = false;
        UpdateButtonsInteractable(true);
        UpdateUI();
    }

    private void PlayTick()
    {
        if (wheelTickSound != null)
        {
            AudioSource.PlayClipAtPoint(wheelTickSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.6f);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWheelTick();
        }
    }

    private void PlayWin()
    {
        if (wheelWinSound != null)
        {
            AudioSource.PlayClipAtPoint(wheelWinSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.95f);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWheelWin();
        }
    }

    private void UpdateButtonsInteractable(bool interactable)
    {
        if (freeSpinBtn != null) freeSpinBtn.interactable = interactable && CanFreeSpin();
        if (paidSpinBtn != null)
        {
            bool hasMoney = GameManager.Instance != null && GameManager.Instance.Money >= PaidSpinCost;
            paidSpinBtn.interactable = interactable && hasMoney;
        }
        if (closeWheelBtn != null) closeWheelBtn.interactable = interactable;
        if (closeXBtn != null) closeXBtn.interactable = interactable;
    }

    public void UpdateUI()
    {
        bool canFree = CanFreeSpin();
        if (freeSpinBtnText != null)
        {
            if (canFree)
            {
                freeSpinBtnText.text = "КРУТИТЬ БЕСПЛАТНО! 🎁";
            }
            else
            {
                TimeSpan rem = GetFreeSpinRemainingTime();
                freeSpinBtnText.text = $"🎬 СПИН ЗА РЕКЛАМУ ({rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2})";
            }
        }
        if (freeSpinBtn != null && !isSpinning)
        {
            freeSpinBtn.interactable = true; // Можно крутить бесплатно по кулдауну или через рекламу
        }

        if (paidSpinBtnText != null)
        {
            paidSpinBtnText.text = $"СПИН ЗА {PaidSpinCost:N0} РУБ. 💰";
        }
        if (paidSpinBtn != null && !isSpinning)
        {
            bool hasMoney = GameManager.Instance != null && GameManager.Instance.Money >= PaidSpinCost;
            paidSpinBtn.interactable = hasMoney;
        }

        UpdateBadgeDot();
    }

    private void UpdateBadgeDot()
    {
        bool canFree = CanFreeSpin();
        if (wheelBadgeDot != null)
        {
            wheelBadgeDot.SetActive(canFree);
        }
        if (openWheelBtnText != null)
        {
            openWheelBtnText.text = canFree ? "🎡 СПИН!" : "🎡 СПИН";
        }
    }
}
