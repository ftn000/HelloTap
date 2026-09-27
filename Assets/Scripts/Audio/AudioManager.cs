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
    [SerializeField] private AudioClip catPurrSound;
    [SerializeField] private AudioClip sipSound;
    [SerializeField] private AudioClip mouseClickSound;

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
        sfxSource.volume = 0.70f;

        if (typingSource == null)
        {
            typingSource = gameObject.AddComponent<AudioSource>();
            typingSource.playOnAwake = false;
        }
        typingSource.volume = 1.0f;

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

        if (catPurrSound == null)
        {
            catPurrSound = CreateProceduralPurrClip();
        }

        if (sipSound == null)
        {
            sipSound = CreateProceduralSipClip();
        }

        if (mouseClickSound == null)
        {
            mouseClickSound = CreateProceduralMouseClickClip();
        }
    }

    private AudioClip CreateProceduralMouseClickClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.055f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envSnap = Mathf.Exp(-t * 320f);
            float noise = Random.value * 2f - 1f;
            float snap = (Mathf.Sin(2f * Mathf.PI * 5200f * t) + 0.4f * noise) * envSnap * 1.3f;

            float envBody = Mathf.Exp(-t * 160f);
            float body = (Mathf.Sin(2f * Mathf.PI * 850f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.4f) * envBody;

            float tick = 0f;
            if (t >= 0.022f)
            {
                float tTick = t - 0.022f;
                tick = Mathf.Sin(2f * Mathf.PI * 4800f * tTick) * Mathf.Exp(-tTick * 400f) * 0.45f;
            }

            data[i] = Mathf.Clamp((snap + body + tick) * 0.85f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_MouseClick", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralSipClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.36f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float slurp = 0f;
            if (t < 0.14f)
            {
                float tSlurp = t / 0.14f;
                float env = Mathf.Sin(tSlurp * Mathf.PI) * (0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 32f * t));
                float noise = Random.value * 2f - 1f;
                float carrier = Mathf.Sin(2f * Mathf.PI * (1800f + 800f * tSlurp) * t) * 0.6f + noise * 0.4f;
                slurp = carrier * env * 0.55f;
            }

            float gulp = 0f;
            if (t >= 0.10f && t < 0.32f)
            {
                float tGulp = (t - 0.10f) / 0.22f;
                float env = Mathf.Sin(tGulp * Mathf.PI) * Mathf.Exp(-tGulp * 3.5f);
                float curFreq = 520f * (1f - tGulp * 0.60f);
                float sine = Mathf.Sin(2f * Mathf.PI * curFreq * t) + 0.4f * Mathf.Sin(4f * Mathf.PI * curFreq * t);
                gulp = sine * env * 0.85f;
            }

            float droplet = 0f;
            if (t >= 0.24f)
            {
                float tDrop = (t - 0.24f) / 0.12f;
                droplet = Mathf.Sin(2f * Mathf.PI * (1400f - tDrop * 400f) * t) * Mathf.Exp(-tDrop * 18f) * 0.28f;
            }

            data[i] = Mathf.Clamp((slurp + gulp + droplet) * 0.85f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_Sip", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralPurrClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.45f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = t < 0.05f ? t / 0.05f : (t > 0.33f ? (0.45f - t) / 0.12f : 1f);
            float flutter = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 24f * t);
            float freq = t < 0.22f ? 135f + (t / 0.22f) * 75f : 210f - ((t - 0.22f) / 0.23f) * 55f;
            float carrier = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.45f * Mathf.Sin(4f * Mathf.PI * freq * t);
            float breath = (Random.value * 2f - 1f) * 0.12f;
            data[i] = Mathf.Clamp((carrier * flutter + breath) * env * 0.75f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_Purr", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
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

        // 1. Всегда воспроизводим отчетливый стук механического переключателя
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

        // 2. При крите мягко накладываем приятный кристаллический акцент поверх клика клавиатуры
        if (isCrit && critSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(critSound, 0.45f);
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

    public void PlayCatPurr()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (catPurrSound != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.05f);
            sfxSource.PlayOneShot(catPurrSound, 0.75f);
        }
    }

    public void PlaySipSound()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (sipSound != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.08f);
            sfxSource.PlayOneShot(sipSound, 0.85f);
        }
    }

    public void PlayMouseClick()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (mouseClickSound != null)
        {
            sfxSource.pitch = Random.Range(0.97f, 1.04f);
            sfxSource.PlayOneShot(mouseClickSound, 0.90f);
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
