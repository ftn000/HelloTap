using UnityEngine;

/// <summary>
/// Менеджер звуков для HelloTap GameDev Idle.
/// Отвечает за звуки механической клавиатуры, криты, звуки покупки железа/помощников,
/// кассовый звук релиза проектов, бусты и авто-приглушение при сворачивании вкладки (Yandex Games).
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
            }
            return instance;
        }
    }

    private const string MutePrefKey = "DevGame_IsMuted";

    [Header("Звуковые эффекты")]
    [SerializeField] private AudioClip[] typingSounds;
    [SerializeField] private AudioClip critSound;
    [SerializeField] private AudioClip upgradeSound;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioClip boostSound;

    [Header("Источники звука")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource typingSource;

    [Header("Настройки клавиатуры")]
    [Range(0.8f, 1.2f)] [SerializeField] private float minTypingPitch = 0.94f;
    [Range(0.8f, 1.2f)] [SerializeField] private float maxTypingPitch = 1.06f;

    private bool isMuted = false;
    private bool isFocusLost = false;

    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (typingSource == null)
        {
            typingSource = gameObject.AddComponent<AudioSource>();
            typingSource.playOnAwake = false;
        }

        isMuted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
        ApplyMute();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
            GameManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
            GameManager.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
            GameManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        isFocusLost = !hasFocus;
        ApplyMute();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        isFocusLost = pauseStatus;
        ApplyMute();
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        PlayTyping(isCrit);
    }

    private void HandleUpgradePurchased(UpgradeItem item)
    {
        PlayUpgrade();
    }

    private void HandleProjectCompleted(GameProjectData project)
    {
        PlayRelease();
    }

    private void HandleAchievementUnlocked(AchievementData achievement)
    {
        PlayRelease();
    }

    public void PlayTyping(bool isCrit = false)
    {
        if (isMuted || isFocusLost) return;

        if (isCrit && critSound != null)
        {
            sfxSource.PlayOneShot(critSound, 0.9f);
            return;
        }

        if (typingSounds != null && typingSounds.Length > 0)
        {
            int index = Random.Range(0, typingSounds.Length);
            AudioClip clip = typingSounds[index];
            if (clip != null)
            {
                typingSource.pitch = Random.Range(minTypingPitch, maxTypingPitch);
                typingSource.PlayOneShot(clip, 0.75f);
            }
        }
    }

    public void PlayUpgrade()
    {
        if (isMuted || isFocusLost || upgradeSound == null) return;
        sfxSource.PlayOneShot(upgradeSound, 0.85f);
    }

    public void PlayRelease()
    {
        if (isMuted || isFocusLost || releaseSound == null) return;
        sfxSource.PlayOneShot(releaseSound, 1.0f);
    }

    public void PlayBoost()
    {
        if (isMuted || isFocusLost || boostSound == null) return;
        sfxSource.PlayOneShot(boostSound, 0.9f);
    }

    public void PlayBugHit(bool killed)
    {
        if (isMuted || isFocusLost) return;
        if (killed && releaseSound != null)
        {
            sfxSource.PlayOneShot(releaseSound, 0.95f);
        }
        else if (critSound != null)
        {
            sfxSource.PlayOneShot(critSound, 0.8f);
        }
    }

    public bool ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(MutePrefKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
        return isMuted;
    }

    private void ApplyMute()
    {
        bool effectiveMute = isMuted || isFocusLost;
        if (sfxSource != null) sfxSource.mute = effectiveMute;
        if (typingSource != null) typingSource.mute = effectiveMute;
        AudioListener.pause = isFocusLost;
    }
}
