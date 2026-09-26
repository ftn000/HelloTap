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
        EnsureClips();
        ApplyMute();
    }

    private void EnsureClips()
    {
        if (typingSounds == null || typingSounds.Length == 0)
        {
            typingSounds = new AudioClip[4];
            typingSounds[0] = CreateProceduralSwitchClip("Blue_Click", 4200f, 240f);
            typingSounds[1] = CreateProceduralSwitchClip("Brown_Bump", 2900f, 210f);
            typingSounds[2] = CreateProceduralSwitchClip("Speed_Snap", 3800f, 260f);
            typingSounds[3] = CreateProceduralSwitchClip("Spacebar_Thud", 2200f, 160f);
        }
        else
        {
            for (int i = 0; i < typingSounds.Length; i++)
            {
                if (typingSounds[i] == null)
                {
                    typingSounds[i] = CreateProceduralSwitchClip($"Procedural_Key_{i}", 3500f + i * 200f, 220f);
                }
            }
        }
    }

    private AudioClip CreateProceduralSwitchClip(string clipName, float clickFreq, float thudFreq)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.075f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float click = (Mathf.Sin(2f * Mathf.PI * clickFreq * t) + (Random.value * 2f - 1f) * 0.45f) * Mathf.Exp(-t * 240f);
            float thud = t >= 0.003f ? Mathf.Sin(2f * Mathf.PI * thudFreq * (t - 0.003f)) * Mathf.Exp(-(t - 0.003f) * 65f) * 0.7f : 0f;
            data[i] = Mathf.Clamp(click + thud, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
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
        AudioListener.pause = false;

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
                typingSource.PlayOneShot(clip, 1.0f);
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
