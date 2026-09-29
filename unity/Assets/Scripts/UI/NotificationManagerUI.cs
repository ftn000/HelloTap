using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Менеджер оффлайн-уведомлений и ярлыка игры (Push & Shortcut Manager):
/// - Добавление ярлыка на рабочий стол / в панель Яндекс Игр с бонусом +10 000 руб.
/// - Планирование и отправка локальных уведомлений в браузере (накоплен оффлайн доход, готов спин колеса)
/// - Модальное окно настроек уведомлений и ярлыка
/// </summary>
public class NotificationManagerUI : MonoBehaviour
{
    private static NotificationManagerUI instance;
    public static NotificationManagerUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<NotificationManagerUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openNotifyModalBtn;
    [SerializeField] private Button closeNotifyModalBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Ярлык на рабочий стол (Shortcut)")]
    [SerializeField] private Button addShortcutBtn;
    [SerializeField] private TMP_Text addShortcutBtnText;
    [SerializeField] private TMP_Text shortcutStatusText;

    [Header("Тумблеры уведомлений")]
    [SerializeField] private Button toggleOfflineFullBtn;
    [SerializeField] private TMP_Text toggleOfflineFullText;
    [SerializeField] private Button toggleWheelReadyBtn;
    [SerializeField] private TMP_Text toggleWheelReadyText;
    [SerializeField] private Button toggleHackathonBtn;
    [SerializeField] private TMP_Text toggleHackathonText;

    private bool notifyOfflineFull = true;
    private bool notifyWheelReady = true;
    private bool notifyHackathon = true;
    private bool shortcutRewardClaimed = false;

    private const double ShortcutRewardAmount = 10000.0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadPrefs();
        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
        CheckShortcutAvailability();
    }

    private void BindButtons()
    {
        if (openNotifyModalBtn != null)
        {
            openNotifyModalBtn.onClick.RemoveAllListeners();
            openNotifyModalBtn.onClick.AddListener(OpenModal);
        }
        if (closeNotifyModalBtn != null)
        {
            closeNotifyModalBtn.onClick.RemoveAllListeners();
            closeNotifyModalBtn.onClick.AddListener(CloseModal);
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

        if (addShortcutBtn != null)
        {
            addShortcutBtn.onClick.RemoveAllListeners();
            addShortcutBtn.onClick.AddListener(OnAddShortcutClicked);
        }

        if (toggleOfflineFullBtn != null)
        {
            toggleOfflineFullBtn.onClick.RemoveAllListeners();
            toggleOfflineFullBtn.onClick.AddListener(ToggleOfflineNotify);
        }
        if (toggleWheelReadyBtn != null)
        {
            toggleWheelReadyBtn.onClick.RemoveAllListeners();
            toggleWheelReadyBtn.onClick.AddListener(ToggleWheelNotify);
        }
        if (toggleHackathonBtn != null)
        {
            toggleHackathonBtn.onClick.RemoveAllListeners();
            toggleHackathonBtn.onClick.AddListener(ToggleHackathonNotify);
        }
    }

    private void CheckShortcutAvailability()
    {
        YandexSDKBridge.Instance.CheckCanCreateShortcut((can) =>
        {
            if (addShortcutBtn != null)
            {
                addShortcutBtn.interactable = can && !shortcutRewardClaimed;
            }
            if (shortcutStatusText != null)
            {
                if (shortcutRewardClaimed)
                {
                    shortcutStatusText.text = "<color=#00FF88>✓ Ярлык добавлен (+10 000 ₽ получено)</color>";
                }
                else if (can)
                {
                    shortcutStatusText.text = "Добавьте ярлык для мгновенного доступа и получите <b>+10 000 ₽</b>!";
                }
                else
                {
                    shortcutStatusText.text = "<color=#94A3B8>Ярлык уже установлен или не поддерживается вашим браузером.</color>";
                }
            }
        });
    }

    public void OnAddShortcutClicked()
    {
        YandexSDKBridge.Instance.CreateShortcut((accepted) =>
        {
            if (accepted)
            {
                if (!shortcutRewardClaimed)
                {
                    shortcutRewardClaimed = true;
                    PlayerPrefs.SetInt("Dev_ShortcutClaimed", 1);
                    PlayerPrefs.Save();

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddMoney(ShortcutRewardAmount);
                    }

                    if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();
                    HapticFeedback.SuccessPattern();

                    if (ClickJuice.Instance != null && addShortcutBtn != null)
                    {
                        ClickJuice.Instance.SpawnCustomPopup($"🎉 ЯРЛЫК СОЗДАН!\n+{ShortcutRewardAmount:N0} ₽", addShortcutBtn.transform.position, new Color(0.2f, 1f, 0.6f), true);
                    }
                }
            }
            CheckShortcutAvailability();
        });
    }

    public void TriggerOfflineIncomeReminder(double income)
    {
        if (!notifyOfflineFull) return;
        YandexSDKBridge.Instance.SendLocalNotification(
            "HelloTap: Оффлайн-доход ждёт!",
            $"Ваша студия намайнила {NumberFormatter.Format(income)} строк кода. Зайдите удвоить награду!");
    }

    public void TriggerLuckyWheelReminder()
    {
        if (!notifyWheelReady) return;
        YandexSDKBridge.Instance.SendLocalNotification(
            "HelloTap: Колесо Фортуны готово!",
            "Ваш бесплатный ежедневный спин уже доступен. Крутите и выиграйте джекпот!");
    }

    public void TriggerHackathonReminder(string theme)
    {
        if (!notifyHackathon) return;
        YandexSDKBridge.Instance.SendLocalNotification(
            "HelloTap: Хакатон начался! 🚀",
            $"Тема 48-часового джема: '{theme}'. Успейте разработать прототип за выходные!");
    }

    private void ToggleOfflineNotify()
    {
        notifyOfflineFull = !notifyOfflineFull;
        SavePrefs();
        UpdateUI();
        HapticFeedback.LightImpact();
    }

    private void ToggleWheelNotify()
    {
        notifyWheelReady = !notifyWheelReady;
        SavePrefs();
        UpdateUI();
        HapticFeedback.LightImpact();
    }

    private void ToggleHackathonNotify()
    {
        notifyHackathon = !notifyHackathon;
        SavePrefs();
        UpdateUI();
        HapticFeedback.LightImpact();
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

        CheckShortcutAvailability();
        UpdateUI();
        HapticFeedback.MediumImpact();
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

    private void UpdateUI()
    {
        if (toggleOfflineFullText != null)
        {
            toggleOfflineFullText.text = notifyOfflineFull ? "🔔 Оффлайн-доход: ВКЛ" : "🔕 Оффлайн-доход: ВЫКЛ";
            toggleOfflineFullText.color = notifyOfflineFull ? new Color(0.2f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
        }
        if (toggleWheelReadyText != null)
        {
            toggleWheelReadyText.text = notifyWheelReady ? "🔔 Колесо фортуны: ВКЛ" : "🔕 Колесо фортуны: ВЫКЛ";
            toggleWheelReadyText.color = notifyWheelReady ? new Color(0.2f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
        }
        if (toggleHackathonText != null)
        {
            toggleHackathonText.text = notifyHackathon ? "🔔 Хакатоны: ВКЛ" : "🔕 Хакатоны: ВЫКЛ";
            toggleHackathonText.color = notifyHackathon ? new Color(0.2f, 1f, 0.6f) : new Color(0.7f, 0.7f, 0.7f);
        }
    }

    private void LoadPrefs()
    {
        notifyOfflineFull = PlayerPrefs.GetInt("Dev_NotifyOffline", 1) == 1;
        notifyWheelReady = PlayerPrefs.GetInt("Dev_NotifyWheel", 1) == 1;
        notifyHackathon = PlayerPrefs.GetInt("Dev_NotifyHackathon", 1) == 1;
        shortcutRewardClaimed = PlayerPrefs.GetInt("Dev_ShortcutClaimed", 0) == 1;
    }

    private void SavePrefs()
    {
        PlayerPrefs.SetInt("Dev_NotifyOffline", notifyOfflineFull ? 1 : 0);
        PlayerPrefs.SetInt("Dev_NotifyWheel", notifyWheelReady ? 1 : 0);
        PlayerPrefs.SetInt("Dev_NotifyHackathon", notifyHackathon ? 1 : 0);
        PlayerPrefs.Save();
    }
}
