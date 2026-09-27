using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система ежедневных серий входов (Daily Streak & Login Rewards):
/// - 7-дневный календарь наград с возрастающими бонусами
/// - Отслеживание серий дней подряд (сохранение в PlayerPrefs)
/// - Награда 7-го дня: Золотая корона «STREAK MASTER» и мега-бонус валюты
/// - Индикатор пламени 🔥 и бейдж уведомления в верхней панели
/// </summary>
public class DailyStreakUI : MonoBehaviour
{
    private static DailyStreakUI instance;
    public static DailyStreakUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<DailyStreakUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openStreakBtn;
    [SerializeField] private TMP_Text openStreakBtnText;
    [SerializeField] private GameObject streakBadgeDot;
    [SerializeField] private Button closeStreakBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Заголовок и статус")]
    [SerializeField] private TMP_Text streakDaysText;
    [SerializeField] private TMP_Text streakStatusDescText;

    [Header("Карточки 7 дней")]
    [SerializeField] private GameObject[] dayCardRoots;
    [SerializeField] private TMP_Text[] dayNumberTexts;
    [SerializeField] private TMP_Text[] dayRewardTexts;
    [SerializeField] private Image[] dayCheckmarkIcons;
    [SerializeField] private GameObject[] dayHighlightFrames;

    [Header("Кнопка сбора награды дня")]
    [SerializeField] private Button claimTodayBtn;
    [SerializeField] private TMP_Text claimTodayBtnText;

    [Header("Корона 7-го дня")]
    [SerializeField] private GameObject crownStickerObj;

    [Header("Звук сбора")]
    [SerializeField] private AudioClip streakClaimSound;

    // Конфигурация наград для 7 дней
    public struct DayReward
    {
        public double Money;
        public double Code;
        public float BoostSeconds;
        public string Desc;
        public DayReward(double m, double c, float b, string d)
        {
            Money = m;
            Code = c;
            BoostSeconds = b;
            Desc = d;
        }
    }

    private static readonly DayReward[] Rewards = new DayReward[]
    {
        new DayReward(500.0, 200.0, 0f, "+500 руб.\n+200 строк"),
        new DayReward(1500.0, 500.0, 0f, "+1 500 руб.\n+500 строк"),
        new DayReward(3000.0, 1000.0, 30f, "+3 000 руб. | +1K кода\n⚡ БУСТ x2 (30с)"),
        new DayReward(6000.0, 2500.0, 0f, "+6 000 руб.\n+2.5K строк"),
        new DayReward(10000.0, 5000.0, 45f, "+10 000 руб. | +5K кода\n⚡ БУСТ x2 (45с)"),
        new DayReward(20000.0, 10000.0, 0f, "+20 000 руб.\n+10K строк"),
        new DayReward(50000.0, 25000.0, 60f, "👑 STREAK MASTER!\n+50K руб. | +25K кода")
    };

    private const string PrefCurrentStreak = "Streak_CurrentDay";
    private const string PrefLastDate = "Streak_LastLoginDate";
    private const string PrefHasCrown = "Streak_HasCrownUnlocked";

    private int currentStreakDay = 1; // 1 to 7
    private bool canClaimToday = false;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public bool HasCrownUnlocked => PlayerPrefs.GetInt(PrefHasCrown, 0) == 1;

    public double GetStreakMultiplier()
    {
        return 1.0 + (Mathf.Clamp(currentStreakDay, 1, 7) - 1) * 0.05;
    }

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
        EvaluateStreakStatus();
        UpdateUI();
    }

    private void Update()
    {
        if (IsModalOpen && !canClaimToday && streakStatusDescText != null)
        {
            DateTime nextDay = DateTime.UtcNow.Date.AddDays(1);
            TimeSpan rem = nextDay - DateTime.UtcNow;
            if (rem.TotalSeconds > 0)
            {
                int bonusPct = (currentStreakDay - 1) * 5;
                streakStatusDescText.text = $"День {currentStreakDay} получен! Активен суточный бонус: <color=#39FF14>+{bonusPct}%</color> ко всему фарму!\n⏳ До следующего дня: <color=#00E5FF>{rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2}</color>";
            }
        }
    }

    private void BindButtons()
    {
        if (openStreakBtn != null)
        {
            openStreakBtn.onClick.RemoveAllListeners();
            openStreakBtn.onClick.AddListener(OpenModal);
        }
        if (closeStreakBtn != null)
        {
            closeStreakBtn.onClick.RemoveAllListeners();
            closeStreakBtn.onClick.AddListener(CloseModal);
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
        if (claimTodayBtn != null)
        {
            claimTodayBtn.onClick.RemoveAllListeners();
            claimTodayBtn.onClick.AddListener(ClaimTodayReward);
        }
    }

    public void EvaluateStreakStatus()
    {
        currentStreakDay = PlayerPrefs.GetInt(PrefCurrentStreak, 1);
        if (currentStreakDay < 1) currentStreakDay = 1;
        if (currentStreakDay > 7) currentStreakDay = 7;

        string todayStr = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        string lastDateStr = PlayerPrefs.GetString(PrefLastDate, "");

        if (string.IsNullOrEmpty(lastDateStr))
        {
            // Первый вход
            currentStreakDay = 1;
            canClaimToday = true;
        }
        else if (lastDateStr == todayStr)
        {
            // Уже забрал сегодня
            canClaimToday = false;
        }
        else
        {
            // Проверяем разницу в днях
            if (DateTime.TryParseExact(lastDateStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastDt))
            {
                int daysDiff = (DateTime.UtcNow.Date - lastDt.Date).Days;
                if (daysDiff == 1)
                {
                    // Последовательный вход
                    if (currentStreakDay >= 7)
                    {
                        currentStreakDay = 1; // Цикл заново
                    }
                    else
                    {
                        currentStreakDay++;
                    }
                    canClaimToday = true;
                }
                else if (daysDiff > 1)
                {
                    // Серия прервалась
                    currentStreakDay = 1;
                    canClaimToday = true;
                }
                else
                {
                    // Тот же день
                    canClaimToday = false;
                }
            }
            else
            {
                currentStreakDay = 1;
                canClaimToday = true;
            }
        }

        PlayerPrefs.SetInt(PrefCurrentStreak, currentStreakDay);
        PlayerPrefs.Save();

        UpdateBadgeDot();
        if (crownStickerObj != null)
        {
            crownStickerObj.SetActive(HasCrownUnlocked);
        }
    }

    public void ClaimTodayReward()
    {
        if (!canClaimToday) return;

        canClaimToday = false;
        string todayStr = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        PlayerPrefs.GetString(PrefLastDate, todayStr);
        PlayerPrefs.SetString(PrefLastDate, todayStr);
        PlayerPrefs.SetInt(PrefCurrentStreak, currentStreakDay);

        int rewardIdx = Mathf.Clamp(currentStreakDay - 1, 0, Rewards.Length - 1);
        DayReward reward = Rewards[rewardIdx];

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(reward.Code, reward.Money);
            if (reward.BoostSeconds > 0f)
            {
                GameManager.Instance.ActivateEnergyBoost(reward.BoostSeconds);
            }
        }

        // Если это день 7 - разблокируем корону
        if (currentStreakDay == 7)
        {
            PlayerPrefs.SetInt(PrefHasCrown, 1);
            if (crownStickerObj != null) crownStickerObj.SetActive(true);
        }
        PlayerPrefs.Save();

        // Звук и тактильный отклик
        HapticFeedback.Vibrate(50);
        if (streakClaimSound != null)
        {
            AudioSource.PlayClipAtPoint(streakClaimSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.95f);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStreakClaim();
        }

        if (ClickJuice.Instance != null && claimTodayBtn != null)
        {
            string bonusText = currentStreakDay == 7 ? "👑 STREAK MASTER! КОРОНА ОТКРЫТА!" : $"🔥 ДЕНЬ {currentStreakDay} ЗАБРАН!";
            ClickJuice.Instance.SpawnCustomPopup(bonusText, claimTodayBtn.transform.position + Vector3.up * 60f, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateBadgeDot();
        UpdateUI();
    }

    private void UpdateBadgeDot()
    {
        if (streakBadgeDot != null)
        {
            streakBadgeDot.SetActive(canClaimToday);
        }
        if (openStreakBtnText != null)
        {
            openStreakBtnText.text = $"🔥 {currentStreakDay} ДН.";
        }
    }

    public void UpdateUI()
    {
        if (streakDaysText != null)
        {
            int bonusPct = (currentStreakDay - 1) * 5;
            streakDaysText.text = $"🔥 СЕРИЯ: {currentStreakDay} ДНЕЙ (+{bonusPct}% К ДОХОДУ)";
        }
        if (streakStatusDescText != null)
        {
            if (canClaimToday)
            {
                streakStatusDescText.text = $"Награда за День {currentStreakDay} готова к выдаче!";
            }
            else
            {
                streakStatusDescText.text = $"День {currentStreakDay} получен! Возвращайся завтра за следующим бонусом.";
            }
        }

        // Обновляем 7 карточек дней
        for (int i = 0; i < 7; i++)
        {
            int dayNumber = i + 1;
            bool isPast = dayNumber < currentStreakDay || (dayNumber == currentStreakDay && !canClaimToday);
            bool isCurrent = dayNumber == currentStreakDay && canClaimToday;
            bool isFuture = dayNumber > currentStreakDay;

            if (dayCheckmarkIcons != null && i < dayCheckmarkIcons.Length && dayCheckmarkIcons[i] != null)
            {
                dayCheckmarkIcons[i].gameObject.SetActive(isPast);
            }

            if (dayHighlightFrames != null && i < dayHighlightFrames.Length && dayHighlightFrames[i] != null)
            {
                dayHighlightFrames[i].SetActive(isCurrent);
            }

            if (dayRewardTexts != null && i < dayRewardTexts.Length && dayRewardTexts[i] != null)
            {
                dayRewardTexts[i].text = Rewards[i].Desc;
                if (isPast)
                {
                    dayRewardTexts[i].color = new Color(0.5f, 0.9f, 0.6f, 0.9f);
                }
                else if (isCurrent)
                {
                    dayRewardTexts[i].color = new Color(1f, 0.9f, 0.3f, 1f);
                }
                else
                {
                    dayRewardTexts[i].color = new Color(0.65f, 0.75f, 0.85f, 0.8f);
                }
            }
        }

        // Обновляем кнопку сбора
        if (claimTodayBtn != null)
        {
            claimTodayBtn.interactable = canClaimToday;
        }
        if (claimTodayBtnText != null)
        {
            if (canClaimToday)
            {
                claimTodayBtnText.text = $"🔥 ЗАБРАТЬ ДЕНЬ {currentStreakDay} 🎁";
            }
            else
            {
                claimTodayBtnText.text = "СЕГОДНЯ УЖЕ ПОЛУЧЕНО ✔";
            }
        }
    }

    public void OpenModal()
    {
        EvaluateStreakStatus();
        UpdateUI();

        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                StartCoroutine(PopInRoutine(modalCardTransform));
            }
        }

        HapticFeedback.Vibrate(25);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCassetteClick();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null)
        {
            modalRoot.SetActive(false);
        }
        HapticFeedback.Vibrate(15);
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCassetteClick();
        }
    }

    private IEnumerator PopInRoutine(Transform target)
    {
        if (target == null) yield break;
        target.localScale = new Vector3(0.85f, 0.85f, 1f);
        float elapsed = 0f;
        float dur = 0.22f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float scale = 1f + 0.12f * Mathf.Sin(t * Mathf.PI);
            target.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
