using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система ежедневных квестов и испытаний для инди-разработчика:
/// 1. «Спринт печати» — развить скорость 250+ CPM
/// 2. «Чистый прод» — устранить 3 бага на экране монитора
/// 3. «Вайб инди-дева» — написать 300 строк под музыку или эмбиент
/// </summary>
public class DailyQuestsUI : MonoBehaviour
{
    private static DailyQuestsUI instance;
    public static DailyQuestsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<DailyQuestsUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модалка")]
    [SerializeField] private Button openQuestsBtn;
    [SerializeField] private TMP_Text openQuestsBtnText;
    [SerializeField] private GameObject questBadgeDot;
    [SerializeField] private Button closeQuestsBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Звук победы")]
    [SerializeField] private AudioClip questCompleteSound;

    [Header("Квест 1: Спринт CPM")]
    [SerializeField] private Image quest1Fill;
    [SerializeField] private TMP_Text quest1ProgressText;
    [SerializeField] private Button quest1ClaimBtn;
    [SerializeField] private TMP_Text quest1ClaimText;

    [Header("Квест 2: Охота на баги")]
    [SerializeField] private Image quest2Fill;
    [SerializeField] private TMP_Text quest2ProgressText;
    [SerializeField] private Button quest2ClaimBtn;
    [SerializeField] private TMP_Text quest2ClaimText;

    [Header("Квест 3: Lo-Fi кодинг")]
    [SerializeField] private Image quest3Fill;
    [SerializeField] private TMP_Text quest3ProgressText;
    [SerializeField] private Button quest3ClaimBtn;
    [SerializeField] private TMP_Text quest3ClaimText;

    // Константы целей
    private const float Quest1TargetCPM = 250f;
    private const int Quest2TargetBugs = 3;
    private const double Quest3TargetCode = 300.0;

    // Сохраненное состояние
    private float currentMaxCPM = 0f;
    private int bugsFixedToday = 0;
    private double lofiCodeWrittenToday = 0.0;
    private bool quest1Claimed = false;
    private bool quest2Claimed = false;
    private bool quest3Claimed = false;

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
        CheckDayReset();
        LoadState();
        BindButtons();
    }

    private void Start()
    {
        CheckDayReset();
        LoadState();
        BindButtons();
        SubscribeGameEvents();
        UpdateUI();
    }

    private void OnDestroy()
    {
        UnsubscribeGameEvents();
    }

    private void BindButtons()
    {
        if (openQuestsBtn != null)
        {
            openQuestsBtn.onClick.RemoveAllListeners();
            openQuestsBtn.onClick.AddListener(OpenModal);
        }

        if (closeQuestsBtn != null)
        {
            closeQuestsBtn.onClick.RemoveAllListeners();
            closeQuestsBtn.onClick.AddListener(CloseModal);
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

        if (quest1ClaimBtn != null)
        {
            quest1ClaimBtn.onClick.RemoveAllListeners();
            quest1ClaimBtn.onClick.AddListener(ClaimQuest1);
        }

        if (quest2ClaimBtn != null)
        {
            quest2ClaimBtn.onClick.RemoveAllListeners();
            quest2ClaimBtn.onClick.AddListener(ClaimQuest2);
        }

        if (quest3ClaimBtn != null)
        {
            quest3ClaimBtn.onClick.RemoveAllListeners();
            quest3ClaimBtn.onClick.AddListener(ClaimQuest3);
        }
    }

    private void SubscribeGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void UnsubscribeGameEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void CheckDayReset()
    {
        int today = DateTime.UtcNow.DayOfYear;
        int lastDay = PlayerPrefs.GetInt("Daily_LastDay", -1);
        if (lastDay != today)
        {
            // Новый день — сброс квестов
            PlayerPrefs.SetInt("Daily_LastDay", today);
            PlayerPrefs.SetFloat("Daily_MaxCPM", 0f);
            PlayerPrefs.SetInt("Daily_BugsFixed", 0);
            PlayerPrefs.SetFloat("Daily_LoFiCode", 0f);
            PlayerPrefs.SetInt("Daily_Q1Claimed", 0);
            PlayerPrefs.SetInt("Daily_Q2Claimed", 0);
            PlayerPrefs.SetInt("Daily_Q3Claimed", 0);
            PlayerPrefs.Save();
        }
    }

    private void LoadState()
    {
        currentMaxCPM = PlayerPrefs.GetFloat("Daily_MaxCPM", 0f);
        bugsFixedToday = PlayerPrefs.GetInt("Daily_BugsFixed", 0);
        lofiCodeWrittenToday = PlayerPrefs.GetFloat("Daily_LoFiCode", 0f);
        quest1Claimed = PlayerPrefs.GetInt("Daily_Q1Claimed", 0) == 1;
        quest2Claimed = PlayerPrefs.GetInt("Daily_Q2Claimed", 0) == 1;
        quest3Claimed = PlayerPrefs.GetInt("Daily_Q3Claimed", 0) == 1;
    }

    private void SaveState()
    {
        PlayerPrefs.SetFloat("Daily_MaxCPM", currentMaxCPM);
        PlayerPrefs.SetInt("Daily_BugsFixed", bugsFixedToday);
        PlayerPrefs.SetFloat("Daily_LoFiCode", (float)lofiCodeWrittenToday);
        PlayerPrefs.SetInt("Daily_Q1Claimed", quest1Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Daily_Q2Claimed", quest2Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Daily_Q3Claimed", quest3Claimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Проверяем Lo-Fi музыку или эмбиент
        bool isLoFiActive = LoFiPlayerUI.Instance != null && LoFiPlayerUI.Instance.IsMusicOrAmbienceActive;
        if (isLoFiActive && !quest3Claimed)
        {
            lofiCodeWrittenToday += amount;
            PlayerPrefs.SetFloat("Daily_LoFiCode", (float)lofiCodeWrittenToday);
        }

        // Проверяем CPM из SessionAnalyticsUI
        if (SessionAnalyticsUI.Instance != null && !quest1Claimed)
        {
            float peak = SessionAnalyticsUI.Instance.PeakCpm;
            if (peak > currentMaxCPM)
            {
                currentMaxCPM = peak;
                PlayerPrefs.SetFloat("Daily_MaxCPM", currentMaxCPM);
            }
        }

        UpdateBadgeDot();
    }

    public void OnBugSquashed()
    {
        if (!quest2Claimed)
        {
            bugsFixedToday++;
            PlayerPrefs.SetInt("Daily_BugsFixed", bugsFixedToday);
            PlayerPrefs.Save();
            UpdateBadgeDot();
            if (IsModalOpen) UpdateUI();
        }
    }

    private void Update()
    {
        if (IsModalOpen)
        {
            // Обновляем текущие значения
            if (SessionAnalyticsUI.Instance != null && !quest1Claimed)
            {
                float peak = SessionAnalyticsUI.Instance.PeakCpm;
                if (peak > currentMaxCPM)
                {
                    currentMaxCPM = peak;
                }
            }
            UpdateUI();
        }
    }

    public void OpenModal()
    {
        if (modalRoot != null) modalRoot.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        HapticFeedback.Vibrate(25);
        UpdateUI();

        if (modalCardTransform != null)
        {
            StartCoroutine(PopInRoutine(modalCardTransform));
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        HapticFeedback.Vibrate(20);
        UpdateBadgeDot();
    }

    private void UpdateBadgeDot()
    {
        bool hasRewardReady = (!quest1Claimed && currentMaxCPM >= Quest1TargetCPM) ||
                              (!quest2Claimed && bugsFixedToday >= Quest2TargetBugs) ||
                              (!quest3Claimed && lofiCodeWrittenToday >= Quest3TargetCode);

        if (questBadgeDot != null)
        {
            questBadgeDot.SetActive(hasRewardReady);
        }
    }

    private void UpdateUI()
    {
        // 1. Квест CPM
        float q1Frac = Mathf.Clamp01(currentMaxCPM / Quest1TargetCPM);
        if (quest1Fill != null) quest1Fill.fillAmount = q1Frac;
        if (quest1ProgressText != null)
        {
            quest1ProgressText.text = quest1Claimed ? "<color=#4EC9B0>ВЫПОЛНЕНО ✓</color>" : $"{currentMaxCPM:F0} / {Quest1TargetCPM:F0} CPM";
        }
        if (quest1ClaimBtn != null)
        {
            quest1ClaimBtn.interactable = !quest1Claimed && currentMaxCPM >= Quest1TargetCPM;
            if (quest1ClaimText != null)
            {
                quest1ClaimText.text = quest1Claimed ? "ПОЛУЧЕНО" : (currentMaxCPM >= Quest1TargetCPM ? "ЗАБРАТЬ НАГРАДУ!" : "В ПРОЦЕССЕ");
            }
        }

        // 2. Квест Баги
        float q2Frac = Mathf.Clamp01((float)bugsFixedToday / Quest2TargetBugs);
        if (quest2Fill != null) quest2Fill.fillAmount = q2Frac;
        if (quest2ProgressText != null)
        {
            quest2ProgressText.text = quest2Claimed ? "<color=#4EC9B0>ВЫПОЛНЕНО ✓</color>" : $"{bugsFixedToday} / {Quest2TargetBugs} багов";
        }
        if (quest2ClaimBtn != null)
        {
            quest2ClaimBtn.interactable = !quest2Claimed && bugsFixedToday >= Quest2TargetBugs;
            if (quest2ClaimText != null)
            {
                quest2ClaimText.text = quest2Claimed ? "ПОЛУЧЕНО" : (bugsFixedToday >= Quest2TargetBugs ? "ЗАБРАТЬ НАГРАДУ!" : "В ПРОЦЕССЕ");
            }
        }

        // 3. Квест Lo-Fi
        float q3Frac = Mathf.Clamp01((float)(lofiCodeWrittenToday / Quest3TargetCode));
        if (quest3Fill != null) quest3Fill.fillAmount = q3Frac;
        if (quest3ProgressText != null)
        {
            quest3ProgressText.text = quest3Claimed ? "<color=#4EC9B0>ВЫПОЛНЕНО ✓</color>" : $"{lofiCodeWrittenToday:F0} / {Quest3TargetCode:F0} строк";
        }
        if (quest3ClaimBtn != null)
        {
            quest3ClaimBtn.interactable = !quest3Claimed && lofiCodeWrittenToday >= Quest3TargetCode;
            if (quest3ClaimText != null)
            {
                quest3ClaimText.text = quest3Claimed ? "ПОЛУЧЕНО" : (lofiCodeWrittenToday >= Quest3TargetCode ? "ЗАБРАТЬ НАГРАДУ!" : "В ПРОЦЕССЕ");
            }
        }

        UpdateBadgeDot();
    }

    public void ClaimQuest1()
    {
        if (quest1Claimed || currentMaxCPM < Quest1TargetCPM) return;
        quest1Claimed = true;
        SaveState();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(1000.0, 5000.0);
        }

        PlayClaimEffects("⚡ СПРИНТ ВЫПОЛНЕН! +5 000 руб. | +1 000 кода");
        UpdateUI();
    }

    public void ClaimQuest2()
    {
        if (quest2Claimed || bugsFixedToday < Quest2TargetBugs) return;
        quest2Claimed = true;
        SaveState();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(0.0, 10000.0);
            GameManager.Instance.ActivateEnergyBoost(30f);
        }

        PlayClaimEffects("🐛 БАГИ УСТРАНЕНЫ! +10 000 руб. | БУСТ 30с");
        UpdateUI();
    }

    public void ClaimQuest3()
    {
        if (quest3Claimed || lofiCodeWrittenToday < Quest3TargetCode) return;
        quest3Claimed = true;
        SaveState();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddDirectCurrencies(2000.0, 15000.0);
        }

        PlayClaimEffects("☕ LO-FI ВАЙБ! +15 000 руб. | +2 000 кода");
        UpdateUI();
    }

    private void PlayClaimEffects(string message)
    {
        HapticFeedback.Vibrate(45);
        if (questCompleteSound != null && AudioManager.Instance != null)
        {
            AudioSource.PlayClipAtPoint(questCompleteSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, 0.9f);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRelease();
        }

        if (ClickJuice.Instance != null && modalRoot != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(message, Vector3.zero, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    private IEnumerator PopInRoutine(Transform target)
    {
        if (target == null) yield break;
        target.localScale = Vector3.one * 0.92f;
        float elapsed = 0f;
        while (elapsed < 0.16f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.16f;
            target.localScale = Vector3.Lerp(Vector3.one * 0.92f, Vector3.one, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
