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
    [SerializeField] private AudioClip[] mouseClickSounds;
    [SerializeField] private AudioClip dogBarkSound;
    [SerializeField] private AudioClip roboBeepSound;
    [SerializeField] private AudioClip streakClaimSound;
    [SerializeField] private AudioClip wheelTickSound;
    [SerializeField] private AudioClip wheelWinSound;
    [SerializeField] private AudioClip lampSwitchSound;
    [SerializeField] private AudioClip bugSquashSound;
    [SerializeField] private AudioClip crateCollectSound;
    [SerializeField] private AudioClip buildCompleteSound;
    [SerializeField] private AudioClip cassetteClickSound;

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

        if (mouseClickSounds == null || mouseClickSounds.Length == 0)
        {
            mouseClickSounds = new AudioClip[3];
            mouseClickSounds[0] = mouseClickSound;
            mouseClickSounds[1] = CreateProceduralMouseSwitchClip(1);
            mouseClickSounds[2] = CreateProceduralMouseSwitchClip(2);
        }
        else
        {
            for (int i = 0; i < mouseClickSounds.Length; i++)
            {
                if (mouseClickSounds[i] == null)
                {
                    mouseClickSounds[i] = CreateProceduralMouseSwitchClip(i);
                }
            }
        }

        if (dogBarkSound == null)
        {
            dogBarkSound = CreateProceduralBarkClip();
        }

        if (roboBeepSound == null)
        {
            roboBeepSound = CreateProceduralRoboBeepClip();
        }

        if (streakClaimSound == null)
        {
            streakClaimSound = CreateProceduralStreakClaimClip();
        }

        if (lampSwitchSound == null)
        {
            lampSwitchSound = CreateProceduralLampSwitchClip();
        }

        if (bugSquashSound == null)
        {
            bugSquashSound = CreateProceduralBugSquashClip();
        }

        if (crateCollectSound == null)
        {
            crateCollectSound = CreateProceduralCrateCollectClip();
        }

        if (buildCompleteSound == null)
        {
            buildCompleteSound = CreateProceduralBuildCompleteClip();
        }

        if (cassetteClickSound == null)
        {
            cassetteClickSound = CreateProceduralCassetteClickClip();
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

        int switchType = PlayerPrefs.GetInt("SelectedSwitchType", 0);
        PlayTypingWithSwitch(switchType, isCrit);
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
        PlayMouseClickWithSwitch(0);
    }

    public void PlayMouseClickWithSwitch(int switchType)
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        AudioClip clip = null;
        if (mouseClickSounds != null && mouseClickSounds.Length > 0)
        {
            int idx = Mathf.Clamp(switchType, 0, mouseClickSounds.Length - 1);
            clip = mouseClickSounds[idx];
        }
        if (clip == null) clip = mouseClickSound;
        if (clip != null)
        {
            sfxSource.pitch = Random.Range(0.97f, 1.04f);
            sfxSource.PlayOneShot(clip, 0.90f);
        }
    }

    public void PlayDogBark()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (dogBarkSound != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.05f);
            sfxSource.PlayOneShot(dogBarkSound, 0.90f);
        }
    }

    public void PlayRoboBeep()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (roboBeepSound != null)
        {
            sfxSource.pitch = Random.Range(0.98f, 1.03f);
            sfxSource.PlayOneShot(roboBeepSound, 0.85f);
        }
    }

    public void PlayStreakClaim()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (streakClaimSound != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(streakClaimSound, 0.95f);
        }
    }

    public void PlayWheelTick()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (wheelTickSound != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.05f);
            sfxSource.PlayOneShot(wheelTickSound, 0.70f);
        }
    }

    public void PlayWheelWin()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (wheelWinSound != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(wheelWinSound, 0.95f);
        }
    }

    private AudioClip CreateProceduralMouseSwitchClip(int switchType)
    {
        int sampleRate = 44100;
        if (switchType == 1) // Optical Gaming
        {
            int count = (int)(sampleRate * 0.048f);
            float[] d = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 380f);
                float snap = (Mathf.Sin(2f * Mathf.PI * 6800f * t) * 0.8f + Mathf.Sin(2f * Mathf.PI * 4200f * t) * 0.5f + (Random.value * 2f - 1f) * 0.5f) * env * 1.4f;
                float ring = Mathf.Sin(2f * Mathf.PI * 2100f * t) * Mathf.Exp(-t * 190f) * 0.35f;
                d[i] = Mathf.Clamp(snap + ring, -1f, 1f);
            }
            AudioClip c = AudioClip.Create("Procedural_Mouse_Optical", count, 1, sampleRate, false);
            c.SetData(d, 0);
            return c;
        }
        else if (switchType == 2) // Silent Office
        {
            int count = (int)(sampleRate * 0.065f);
            float[] d = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 140f);
                float thud = (Mathf.Sin(2f * Mathf.PI * 380f * t) * 0.8f + Mathf.Sin(2f * Mathf.PI * 210f * t) * 0.6f + (Random.value * 2f - 1f) * 0.15f) * env * 0.75f;
                d[i] = Mathf.Clamp(thud, -1f, 1f);
            }
            AudioClip c = AudioClip.Create("Procedural_Mouse_Silent", count, 1, sampleRate, false);
            c.SetData(d, 0);
            return c;
        }
        else // Classic Omron
        {
            return CreateProceduralMouseClickClip();
        }
    }

    private AudioClip CreateProceduralBarkClip()
    {
        int sampleRate = 44100;
        int count = (int)(sampleRate * 0.36f);
        float[] d = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float bark = 0f;
            if (t < 0.12f)
            {
                float tn = t / 0.12f;
                float env = Mathf.Sin(tn * Mathf.PI) * Mathf.Exp(-tn * 2.8f) * 0.85f;
                bark += (Mathf.Sin(2f * Mathf.PI * 420f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 840f * t) + (Random.value * 2f - 1f) * 0.18f) * env;
            }
            if (t >= 0.16f && t < 0.30f)
            {
                float t2 = (t - 0.16f);
                float tn2 = t2 / 0.14f;
                float env2 = Mathf.Sin(tn2 * Mathf.PI) * Mathf.Exp(-tn2 * 2.8f) * 0.85f;
                bark += (Mathf.Sin(2f * Mathf.PI * 460f * t2) + 0.5f * Mathf.Sin(2f * Mathf.PI * 920f * t2) + (Random.value * 2f - 1f) * 0.18f) * env2;
            }
            d[i] = Mathf.Clamp(bark, -1f, 1f);
        }
        AudioClip c = AudioClip.Create("Procedural_DogBark", count, 1, sampleRate, false);
        c.SetData(d, 0);
        return c;
    }

    private AudioClip CreateProceduralRoboBeepClip()
    {
        int sampleRate = 44100;
        int count = (int)(sampleRate * 0.38f);
        float[] d = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float beep = 0f;
            if (t < 0.08f)
            {
                float env = Mathf.Sin((t / 0.08f) * Mathf.PI) * 0.42f;
                beep += Mathf.Sin(2f * Mathf.PI * 880f * t) * env;
            }
            else if (t >= 0.09f && t < 0.18f)
            {
                float t2 = t - 0.09f;
                float env = Mathf.Sin((t2 / 0.09f) * Mathf.PI) * 0.42f;
                beep += Mathf.Sin(2f * Mathf.PI * 1320f * t2) * env;
            }
            else if (t >= 0.19f && t < 0.35f)
            {
                float t3 = t - 0.19f;
                float env = Mathf.Sin((t3 / 0.16f) * Mathf.PI) * 0.42f;
                beep += Mathf.Sin(2f * Mathf.PI * 1760f * t3) * env;
            }
            d[i] = Mathf.Clamp(beep, -1f, 1f);
        }
        AudioClip c = AudioClip.Create("Procedural_RoboBeep", count, 1, sampleRate, false);
        c.SetData(d, 0);
        return c;
    }

    private AudioClip CreateProceduralStreakClaimClip()
    {
        int sampleRate = 44100;
        int count = (int)(sampleRate * 0.95f);
        float[] d = new float[count];
        (float, float)[] notes = new (float, float)[] { (0f, 349.23f), (0.10f, 440f), (0.20f, 523.25f), (0.32f, 698.46f), (0.44f, 880f), (0.44f, 1046.50f) };
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            float sig = 0f;
            foreach (var n in notes)
            {
                if (t >= n.Item1)
                {
                    float localT = t - n.Item1;
                    float env = Mathf.Exp(-localT * 5f) * 0.32f;
                    sig += (Mathf.Sin(2f * Mathf.PI * n.Item2 * localT) + 0.4f * Mathf.Sin(4f * Mathf.PI * n.Item2 * localT)) * env;
                }
            }
            if (t >= 0.44f)
            {
                float localT = t - 0.44f;
                sig += Mathf.Sin(2f * Mathf.PI * 2400f * localT) * Mathf.Exp(-localT * 4.5f) * 0.15f;
            }
            d[i] = Mathf.Clamp(sig, -1f, 1f);
        }
        AudioClip c = AudioClip.Create("Procedural_StreakClaim", count, 1, sampleRate, false);
        c.SetData(d, 0);
        return c;
    }

    private AudioClip CreateProceduralLampSwitchClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.08f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 220f);
            float snap = Mathf.Sin(2f * Mathf.PI * 3200f * t) * env * 0.9f;
            float body = Mathf.Sin(2f * Mathf.PI * 420f * t) * Mathf.Exp(-t * 120f) * 0.6f;
            data[i] = Mathf.Clamp(snap + body, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_LampSwitch", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralBugSquashClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.12f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 80f);
            float noise = (Random.value * 2f - 1f) * 0.4f;
            float f = Mathf.Max(180f, 1200f - t * 8000f);
            float pop = (Mathf.Sin(2f * Mathf.PI * f * t) + noise) * env;
            data[i] = Mathf.Clamp(pop * 0.9f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_BugSquash", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralCrateCollectClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.22f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = t < 0.07f ? 523.25f : (t < 0.14f ? 659.25f : 783.99f);
            float localT = t < 0.07f ? t : (t < 0.14f ? t - 0.07f : t - 0.14f);
            float env = Mathf.Exp(-localT * 35f);
            float chime = (Mathf.Sin(2f * Mathf.PI * f * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * f * t)) * env;
            data[i] = Mathf.Clamp(chime * 0.8f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_CrateCollect", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralBuildCompleteClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.45f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = t < 0.10f ? 523.25f : (t < 0.20f ? 659.25f : (t < 0.30f ? 783.99f : 1046.50f));
            float offset = t < 0.10f ? 0f : (t < 0.20f ? 0.10f : (t < 0.30f ? 0.20f : 0.30f));
            float decay = t >= 0.30f ? 10f : 15f;
            float env = Mathf.Exp(-(t - offset) * decay);
            float bell = (Mathf.Sin(2f * Mathf.PI * f * t) + 0.35f * Mathf.Sin(4f * Mathf.PI * f * t) + 0.15f * Mathf.Sin(6f * Mathf.PI * f * t)) * env;
            data[i] = Mathf.Clamp(bell * 0.9f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_BuildComplete", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public void PlayLampSwitch()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (lampSwitchSound != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.04f);
            sfxSource.PlayOneShot(lampSwitchSound, 0.85f);
        }
    }

    public void PlayBugSquash()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (bugSquashSound != null)
        {
            sfxSource.pitch = Random.Range(0.92f, 1.10f);
            sfxSource.PlayOneShot(bugSquashSound, 0.90f);
        }
    }

    public void PlayCrateCollect()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (crateCollectSound != null)
        {
            sfxSource.pitch = Random.Range(0.98f, 1.05f);
            sfxSource.PlayOneShot(crateCollectSound, 0.90f);
        }
    }

    public void PlayBuildComplete()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (buildCompleteSound != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(buildCompleteSound, 1.0f);
        }
    }

    private AudioClip CreateProceduralCassetteClickClip()
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * 0.075f);
        float[] data = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 190f);
            float click = Mathf.Sin(2f * Mathf.PI * 2400f * t) * env * 0.9f;
            float thud = Mathf.Sin(2f * Mathf.PI * 280f * t) * Mathf.Exp(-t * 90f) * 0.7f;
            data[i] = Mathf.Clamp(click + thud, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("Procedural_CassetteClick", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    public void PlayCassetteClick()
    {
        if (isMuted || isFocusLost || sfxSource == null) return;
        if (cassetteClickSound != null)
        {
            sfxSource.pitch = Random.Range(0.96f, 1.04f);
            sfxSource.PlayOneShot(cassetteClickSound, 0.85f);
        }
    }

    public void PlayTypingWithSwitch(int switchType, bool isCrit = false)
    {
        if (isMuted || isFocusLost) return;
        AudioListener.pause = false;

        if (typingSounds != null && typingSounds.Length > 0)
        {
            int index = Mathf.Clamp(switchType, 0, typingSounds.Length - 1);
            AudioClip clip = typingSounds[index];
            if (clip != null)
            {
                typingSource.pitch = Random.Range(minTypingPitch, maxTypingPitch);
                typingSource.PlayOneShot(clip, 1.0f);
            }
        }

        if (isCrit && critSound != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(1.02f, 1.15f);
            sfxSource.PlayOneShot(critSound, 0.40f);
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
