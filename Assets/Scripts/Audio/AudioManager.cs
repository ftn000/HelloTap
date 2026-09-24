using UnityEngine;

/// <summary>
/// Менеджер звуков для HelloTap GameDev Idle.
/// Отвечает за звуки механической клавиатуры, криты, звуки покупки железа/помощников,
/// кассовый звук релиза проектов и активации буста энергетика.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

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

    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Создаем источники звука, если не назначены
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
        // Подписка на события GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
        }
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

    public void PlayTyping(bool isCrit = false)
    {
        if (isMuted) return;

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
        if (isMuted || upgradeSound == null) return;
        sfxSource.PlayOneShot(upgradeSound, 0.85f);
    }

    public void PlayRelease()
    {
        if (isMuted || releaseSound == null) return;
        sfxSource.PlayOneShot(releaseSound, 1.0f);
    }

    public void PlayBoost()
    {
        if (isMuted || boostSound == null) return;
        sfxSource.PlayOneShot(boostSound, 0.9f);
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
        if (sfxSource != null) sfxSource.mute = isMuted;
        if (typingSource != null) typingSource.mute = isMuted;
    }
}
