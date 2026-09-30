using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет оверлеями мета-прогрессии:
/// 1. Всплывающие плашки достижений (Achievement Toasts)
/// 2. Модальное окно оффлайн-дохода ("Пока вас не было" с выбором x1 / x2)
/// 3. Панель Престижа ("Выход на IPO")
/// </summary>
public class GameOverlaysUI : MonoBehaviour
{
    [Header("Тост Достижений")]
    [SerializeField] private GameObject achievementToastRoot;
    [SerializeField] private TMP_Text achievementToastTitle;
    [SerializeField] private TMP_Text achievementToastDesc;

    [Header("Модальное окно Оффлайн-дохода")]
    [SerializeField] private GameObject offlineModalRoot;
    [SerializeField] private TMP_Text offlineTimeText;
    [SerializeField] private TMP_Text offlineRewardText;
    [SerializeField] private Button claimNormalBtn;
    [SerializeField] private Button claimDoubleBtn;

    [Header("Блок Престижа (Выход на IPO)")]
    [SerializeField] private Button prestigeButton;
    [SerializeField] private TMP_Text prestigeInfoText;
    [SerializeField] private TMP_Text prestigeBtnText;

    private readonly Queue<AchievementData> toastQueue = new Queue<AchievementData>();
    private bool isShowingToast = false;

    public bool IsOfflineModalOpen => offlineModalRoot != null && offlineModalRoot.activeInHierarchy;

    private void Awake()
    {
        if (achievementToastRoot != null)
        {
            achievementToastRoot.SetActive(false);
        }

        if (offlineModalRoot != null)
        {
            offlineModalRoot.SetActive(false);
        }

        BindButtons();
    }

    private void OnEnable()
    {
        SubscribeEvents();
        BindButtons();
        RefreshPrestigeUI();
    }

    private void Start()
    {
        SubscribeEvents();
        BindButtons();
        RefreshPrestigeUI();

        if (GameManager.Instance != null && GameManager.Instance.HasPendingOfflineEarnings)
        {
            ShowOfflineModal();
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
            GameManager.Instance.OnOfflineEarningsReady -= ShowOfflineModal;
            GameManager.Instance.OnCurrenciesChanged -= RefreshPrestigeUI;
        }
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
            GameManager.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;
            GameManager.Instance.OnOfflineEarningsReady -= ShowOfflineModal;
            GameManager.Instance.OnOfflineEarningsReady += ShowOfflineModal;
            GameManager.Instance.OnCurrenciesChanged -= RefreshPrestigeUI;
            GameManager.Instance.OnCurrenciesChanged += RefreshPrestigeUI;
        }
    }

    private void BindButtons()
    {
        if (claimNormalBtn != null)
        {
            claimNormalBtn.onClick.RemoveAllListeners();
            claimNormalBtn.onClick.AddListener(() => OnClaimOfflineClicked(1.0));
        }

        if (claimDoubleBtn != null)
        {
            claimDoubleBtn.onClick.RemoveAllListeners();
            claimDoubleBtn.onClick.AddListener(() => OnClaimOfflineClicked(2.0));
        }

        if (prestigeButton != null)
        {
            prestigeButton.onClick.RemoveAllListeners();
            prestigeButton.onClick.AddListener(OnPrestigeClicked);
        }
    }

    #region Всплывающие Тосты Достижений

    private void HandleAchievementUnlocked(AchievementData ach)
    {
        if (ach == null) return;
        toastQueue.Enqueue(ach);
        if (!isShowingToast)
        {
            StartCoroutine(ProcessToastQueueRoutine());
        }
    }

    private IEnumerator ProcessToastQueueRoutine()
    {
        isShowingToast = true;

        while (toastQueue.Count > 0)
        {
            AchievementData ach = toastQueue.Dequeue();
            if (achievementToastRoot != null)
            {
                if (achievementToastTitle != null)
                {
                    achievementToastTitle.text = $"ДОСТИЖЕНИЕ: {ach.Title} (+{ach.BonusFraction * 100:F0}% к доходу)";
                }
                if (achievementToastDesc != null)
                {
                    achievementToastDesc.text = ach.Description;
                }

                achievementToastRoot.SetActive(true);
                achievementToastRoot.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                float animTime = 0.18f;
                float elapsed = 0f;
                while (elapsed < animTime)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float s = Mathf.Lerp(0.85f, 1.0f, elapsed / animTime);
                    achievementToastRoot.transform.localScale = new Vector3(s, s, 1f);
                    yield return null;
                }
                achievementToastRoot.transform.localScale = Vector3.one;

                yield return new WaitForSecondsRealtime(3.0f);
                achievementToastRoot.SetActive(false);
                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        isShowingToast = false;
    }

    #endregion

    #region Оффлайн-доход ("Пока вас не было")

    public void ShowOfflineModal()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasPendingOfflineEarnings) return;
        if (offlineModalRoot == null) return;

        float sec = GameManager.Instance.PendingOfflineSeconds;
        double code = GameManager.Instance.PendingOfflineCode;
        double money = GameManager.Instance.PendingOfflineMoney;

        if (offlineTimeText != null)
        {
            offlineTimeText.text = $"Вы отсутствовали: {NumberFormatter.FormatTime(sec)}";
        }

        if (offlineRewardText != null)
        {
            offlineRewardText.text =
                $"Ваша команда намайнила:\n" +
                $"<color=#00FF88>+{NumberFormatter.Format(code)} строк кода</color>\n" +
                $"<color=#FFD166>+{NumberFormatter.Format(money)} руб.</color>";
        }

        offlineModalRoot.SetActive(true);
    }

    private void OnClaimOfflineClicked(double multiplier)
    {
        if (multiplier > 1.0)
        {
            // Показ Rewarded видеорекламы для получения x2 бонуса
            YandexSDKBridge.Instance.ShowRewardedAd(
                "offline_double",
                onRewarded: () =>
                {
                    ClaimOfflineBonus(multiplier);
                },
                onClose: null,
                onError: (err) =>
                {
                    // Fallback при блокировщиках рекламы или отсутствии интернета
                    ClaimOfflineBonus(multiplier);
                });
        }
        else
        {
            ClaimOfflineBonus(1.0);
        }
    }

    private void ClaimOfflineBonus(double multiplier)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClaimOfflineEarnings(multiplier);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRelease();
            }
            HapticFeedback.SuccessPattern();
        }

        if (offlineModalRoot != null)
        {
            offlineModalRoot.SetActive(false);
        }
    }

    #endregion

    #region Система Престижа (IPO)

    public void RefreshPrestigeUI()
    {
        if (GameManager.Instance == null) return;

        bool canPrestige = GameManager.Instance.CanPrestige();
        int curLvl = GameManager.Instance.PrestigeLevel;
        double curMult = GameManager.Instance.GetPrestigeMultiplier();
        double nextMult = 1.0 + (curLvl + 1) * 0.5;
        double ipoGrant = Math.Max(5000.0 * (curLvl + 1), GameManager.Instance.TotalMoneyEarned * 0.10);

        if (prestigeInfoText != null)
        {
            prestigeInfoText.text =
                $"ПРЕСТИЖ СТУДИИ (IPO Ур. {curLvl})\n" +
                $"Бонус к доходу: x{curMult:F1} -> x{nextMult:F1}\n" +
                $"Инвест-капитал IPO: +{NumberFormatter.Format(ipoGrant)} ₽ (деньги сохраняются!)\n" +
                (canPrestige ? "<color=#00FF88>Доступен выход на биржу!</color>" : "<color=#AAAAAA>Требуется 2 релиза или 10K всего строк кода</color>");
        }

        if (prestigeButton != null)
        {
            prestigeButton.interactable = canPrestige;
        }

        if (prestigeBtnText != null)
        {
            prestigeBtnText.text = canPrestige ? "ВЫЙТИ НА IPO" : "ЗАКРЫТО";
        }
    }

    private void OnPrestigeClicked()
    {
        if (GameManager.Instance == null) return;
        double earnedBefore = GameManager.Instance.TotalMoneyEarned;
        int nextLvl = GameManager.Instance.PrestigeLevel + 1;
        double bonusGranted = Math.Max(5000.0 * nextLvl, earnedBefore * 0.10);

        if (GameManager.Instance.PerformPrestige())
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRelease();
            }
            HapticFeedback.SuccessPattern();

            // Показ полноэкранной рекламы при крупном достижении (IPO)
            YandexSDKBridge.Instance.ShowFullscreenAd();

            if (ClickJuice.Instance != null && prestigeButton != null)
            {
                ClickJuice.Instance.SpawnCustomPopup(
                    $"IPO УСПЕШНО!\n+{NumberFormatter.Format(bonusGranted)} ₽ ИНВЕСТ-КАПИТАЛ!\nМНОЖИТЕЛЬ x{GameManager.Instance.GetPrestigeMultiplier():F1}!",
                    prestigeButton.transform.position,
                    new Color(1f, 0.85f, 0.25f),
                    true);
            }
            RefreshPrestigeUI();
        }
    }

    #endregion
}
