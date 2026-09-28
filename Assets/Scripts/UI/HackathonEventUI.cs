using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Глобальное событие «Хакатон выходного дня» (Weekend 48h Hackathon):
/// - 48-часовой тайм-лимит с генерацией уникальной темы джема
/// - 3 последовательные фазы: Прототип -> Полишинг -> Питч и Демо-День
/// - Оценка проекта жюри по 3 критериям (Геймплей, Графика, Инновации)
/// - Награда: Золотой/Серебряный кубок, денежный грант и 24ч буст дохода
/// </summary>
public class HackathonEventUI : MonoBehaviour
{
    private static HackathonEventUI instance;
    public static HackathonEventUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<HackathonEventUI>();
            return instance;
        }
    }

    public enum HackathonPhase
    {
        NotStarted,
        Phase1_Prototype,
        Phase2_AlphaPolish,
        Phase3_PitchDemo,
        Completed
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openHackathonBtn;
    [SerializeField] private TMP_Text openHackathonBtnText;
    [SerializeField] private Button closeHackathonBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Информация о хакатоне")]
    [SerializeField] private TMP_Text hackathonThemeText;
    [SerializeField] private TMP_Text hackathonTimerText;
    [SerializeField] private TMP_Text currentPhaseText;
    [SerializeField] private Image phaseProgressFill;
    [SerializeField] private TMP_Text phaseProgressText;

    [Header("Оценки жюри и результат")]
    [SerializeField] private GameObject juryScoresRoot;
    [SerializeField] private TMP_Text juryGameplayText;
    [SerializeField] private TMP_Text juryGraphicsText;
    [SerializeField] private TMP_Text juryInnovationText;
    [SerializeField] private TMP_Text finalRankAwardText;

    [Header("Кнопки участия")]
    [SerializeField] private Button contributeCodeBtn;
    [SerializeField] private TMP_Text contributeBtnText;
    [SerializeField] private Button claimRewardBtn;
    [SerializeField] private TMP_Text claimRewardBtnText;

    private HackathonPhase currentPhase = HackathonPhase.NotStarted;
    private string currentTheme = "AI Roguelike in 48h";
    private float hackathonRemainingSeconds = 172800f; // 48 часов
    private double phaseCurrentCode = 0;
    private double phaseTargetCode = 5000;
    private bool isRewardClaimed = false;

    private int scoreGameplay = 5;
    private int scoreGraphics = 4;
    private int scoreInnovation = 5;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private static readonly string[] HackathonThemes = new string[]
    {
        "AI Roguelike in 48h",
        "Retro Cyberpunk Idle",
        "VR Rubber Duck Simulator",
        "Pixel Physics Platformer",
        "Cozy Coffee Shop Tycoon",
        "Multiplayer Terminal Hacker"
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        LoadHackathonState();
        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
        if (GameManager.Instance != null)
        {
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
        if (openHackathonBtn != null)
        {
            openHackathonBtn.onClick.RemoveAllListeners();
            openHackathonBtn.onClick.AddListener(OpenModal);
        }
        if (closeHackathonBtn != null)
        {
            closeHackathonBtn.onClick.RemoveAllListeners();
            closeHackathonBtn.onClick.AddListener(CloseModal);
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

        if (contributeCodeBtn != null)
        {
            contributeCodeBtn.onClick.RemoveAllListeners();
            contributeCodeBtn.onClick.AddListener(OnContributeCodeClicked);
        }
        if (claimRewardBtn != null)
        {
            claimRewardBtn.onClick.RemoveAllListeners();
            claimRewardBtn.onClick.AddListener(OnClaimRewardClicked);
        }
    }

    private void Update()
    {
        if (currentPhase != HackathonPhase.NotStarted && currentPhase != HackathonPhase.Completed)
        {
            hackathonRemainingSeconds = Mathf.Max(0f, hackathonRemainingSeconds - Time.deltaTime);
            if (hackathonRemainingSeconds <= 0f)
            {
                FinishHackathonEvent();
            }
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            UpdateUI();
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Каждое написание кода даёт вклад в текущую фазу хакатона
        if (currentPhase == HackathonPhase.Phase1_Prototype ||
            currentPhase == HackathonPhase.Phase2_AlphaPolish ||
            currentPhase == HackathonPhase.Phase3_PitchDemo)
        {
            phaseCurrentCode += amount;
            if (phaseCurrentCode >= phaseTargetCode)
            {
                AdvancePhase();
            }
        }
    }

    public void StartNewHackathon()
    {
        currentTheme = HackathonThemes[UnityEngine.Random.Range(0, HackathonThemes.Length)];
        currentPhase = HackathonPhase.Phase1_Prototype;
        hackathonRemainingSeconds = 172800f; // 48ч
        phaseCurrentCode = 0;
        phaseTargetCode = CalculatePhaseTarget(1);
        isRewardClaimed = false;

        SaveHackathonState();
        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();

        if (NotificationManagerUI.Instance != null)
        {
            NotificationManagerUI.Instance.TriggerHackathonReminder(currentTheme);
        }

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🏆 ХАКАТОН СТАРТОВАЛ!\nТема: '{currentTheme}'", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    private double CalculatePhaseTarget(int phaseNum)
    {
        double cpc = GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() : 1.0;
        double cps = GameManager.Instance != null ? GameManager.Instance.GetCodePerSecond() : 0.0;
        return Math.Max(1500.0, (cpc * 150.0 + cps * 120.0) * phaseNum);
    }

    private void AdvancePhase()
    {
        phaseCurrentCode = 0;
        if (currentPhase == HackathonPhase.Phase1_Prototype)
        {
            currentPhase = HackathonPhase.Phase2_AlphaPolish;
            phaseTargetCode = CalculatePhaseTarget(2);
        }
        else if (currentPhase == HackathonPhase.Phase2_AlphaPolish)
        {
            currentPhase = HackathonPhase.Phase3_PitchDemo;
            phaseTargetCode = CalculatePhaseTarget(3);
        }
        else if (currentPhase == HackathonPhase.Phase3_PitchDemo)
        {
            FinishHackathonEvent();
            return;
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBuildComplete();

        SaveHackathonState();
        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎉 ФАЗА СДАНА! Переход к следующему этапу.", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }
    }

    private void FinishHackathonEvent()
    {
        currentPhase = HackathonPhase.Completed;
        scoreGameplay = UnityEngine.Random.Range(4, 6);
        scoreGraphics = UnityEngine.Random.Range(4, 6);
        scoreInnovation = UnityEngine.Random.Range(4, 6);

        SaveHackathonState();
        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();
    }

    public void OnContributeCodeClicked()
    {
        if (currentPhase == HackathonPhase.NotStarted)
        {
            StartNewHackathon();
            return;
        }

        // Вливание порции кода по клику
        double contribution = GameManager.Instance != null ? GameManager.Instance.GetCodePerClick() * 5.0 : 10.0;
        HandleCodeClicked(contribution, false, Vector2.zero);
        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayTyping();
    }

    public void OnClaimRewardClicked()
    {
        if (currentPhase != HackathonPhase.Completed || isRewardClaimed) return;

        isRewardClaimed = true;
        double rewardRub = 350000.0;
        double rewardCode = 50000.0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(rewardRub);
            GameManager.Instance.AddLinesOfCode(rewardCode);
            GameManager.Instance.ActivateEnergyBoost(3600f, 1.5); // 1 час x1.5 буста
        }

        SaveHackathonState();
        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null && claimRewardBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🥇 КУБОК ПОБЕДИТЕЛЯ ХАКАТОНА!\n+{NumberFormatter.Format(rewardRub)} ₽", claimRewardBtn.transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        UpdateUI();
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

        HapticFeedback.MediumImpact();
        UpdateUI();
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
        if (hackathonThemeText != null)
        {
            hackathonThemeText.text = $"Тема: <b>«{currentTheme}»</b>";
        }

        if (hackathonTimerText != null)
        {
            int hours = Mathf.FloorToInt(hackathonRemainingSeconds / 3600f);
            int minutes = Mathf.FloorToInt((hackathonRemainingSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(hackathonRemainingSeconds % 60f);
            hackathonTimerText.text = $"⏱ До конца хакатона: <b>{hours:D2}:{minutes:D2}:{seconds:D2}</b>";
        }

        float progress = (float)Math.Clamp(phaseCurrentCode / Math.Max(1.0, phaseTargetCode), 0.0, 1.0);
        if (phaseProgressFill != null)
        {
            phaseProgressFill.fillAmount = (currentPhase == HackathonPhase.Completed) ? 1f : progress;
        }

        if (phaseProgressText != null)
        {
            phaseProgressText.text = (currentPhase == HackathonPhase.Completed) ? "ВСЕ ЭТАПЫ СДАНЫ!" : $"{phaseCurrentCode:N0} / {phaseTargetCode:N0} строк ({(progress * 100f):F0}%)";
        }

        if (currentPhaseText != null)
        {
            switch (currentPhase)
            {
                case HackathonPhase.NotStarted: currentPhaseText.text = "Статус: Ожидание начала"; break;
                case HackathonPhase.Phase1_Prototype: currentPhaseText.text = "Этап 1: Прототипирование механик 💡"; break;
                case HackathonPhase.Phase2_AlphaPolish: currentPhaseText.text = "Этап 2: Альфа и Полишинг багов 🛠️"; break;
                case HackathonPhase.Phase3_PitchDemo: currentPhaseText.text = "Этап 3: Питч и Презентация жюри 🎤"; break;
                case HackathonPhase.Completed: currentPhaseText.text = "🏆 ХАКАТОН ЗАВЕРШЁН! ИТОГИ ПОДВЕДЕНЫ"; break;
            }
        }

        bool isDone = currentPhase == HackathonPhase.Completed;
        if (juryScoresRoot != null) juryScoresRoot.SetActive(isDone);
        if (claimRewardBtn != null) claimRewardBtn.gameObject.SetActive(isDone && !isRewardClaimed);
        if (contributeCodeBtn != null) contributeCodeBtn.gameObject.SetActive(!isDone);

        if (isDone)
        {
            if (juryGameplayText != null) juryGameplayText.text = $"Геймплей: {new string('⭐', scoreGameplay)}";
            if (juryGraphicsText != null) juryGraphicsText.text = $"Графика: {new string('⭐', scoreGraphics)}";
            if (juryInnovationText != null) juryInnovationText.text = $"Инновации: {new string('⭐', scoreInnovation)}";
            if (finalRankAwardText != null)
            {
                int total = scoreGameplay + scoreGraphics + scoreInnovation;
                string rank = total >= 14 ? "🥇 1 МЕСТО (ГРАНТ 350K ₽ + КУБОК)" : "🥈 2 МЕСТО (ГРАНТ 200K ₽)";
                finalRankAwardText.text = rank;
            }
        }
    }

    private void LoadHackathonState()
    {
        currentPhase = (HackathonPhase)PlayerPrefs.GetInt("Dev_HackathonPhase", 0);
        currentTheme = PlayerPrefs.GetString("Dev_HackathonTheme", "AI Roguelike in 48h");
        hackathonRemainingSeconds = PlayerPrefs.GetFloat("Dev_HackathonTimer", 172800f);
        phaseCurrentCode = double.Parse(PlayerPrefs.GetString("Dev_HackathonCode", "0"));
        phaseTargetCode = double.Parse(PlayerPrefs.GetString("Dev_HackathonTarget", "5000"));
        isRewardClaimed = PlayerPrefs.GetInt("Dev_HackathonClaimed", 0) == 1;
    }

    private void SaveHackathonState()
    {
        PlayerPrefs.SetInt("Dev_HackathonPhase", (int)currentPhase);
        PlayerPrefs.SetString("Dev_HackathonTheme", currentTheme);
        PlayerPrefs.SetFloat("Dev_HackathonTimer", hackathonRemainingSeconds);
        PlayerPrefs.SetString("Dev_HackathonCode", phaseCurrentCode.ToString());
        PlayerPrefs.SetString("Dev_HackathonTarget", phaseTargetCode.ToString());
        PlayerPrefs.SetInt("Dev_HackathonClaimed", isRewardClaimed ? 1 : 0);
        PlayerPrefs.Save();
    }
}
