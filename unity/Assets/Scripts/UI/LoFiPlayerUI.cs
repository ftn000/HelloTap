using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Плеер фоновой музыки и генератор атмосферы инди-студии:
/// - 3 Музыкальных трека: «Lo-Fi Chill Beats», «Synthwave Night», «Sunset Code Jam»
/// - 4 Эмбиент-слоя: «Выкл», «Дождь за окном», «Ночной город», «Уютная кофейня»
/// - Независимый контроль громкости (MusicVolume, AmbienceVolume)
/// - Анимированный эквалайзер в такт музыке
/// - Сохранение состояния в PlayerPrefs
/// </summary>
public class LoFiPlayerUI : MonoBehaviour
{
    private static LoFiPlayerUI instance;
    public static LoFiPlayerUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<LoFiPlayerUI>();
            return instance;
        }
    }

    [Header("Источники и Аудиоклипы")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip lofiTrack;
    [SerializeField] private AudioClip synthwaveTrack;
    [SerializeField] private AudioClip sunsetTrack;

    [Header("Эмбиент-слои окружения")]
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip rainAmbience;
    [SerializeField] private AudioClip nightAmbience;
    [SerializeField] private AudioClip cafeAmbience;
    [SerializeField] private Button ambienceButton;
    [SerializeField] private TMP_Text ambienceButtonText;

    [Header("Элементы управления")]
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Button nextTrackButton;
    [SerializeField] private TMP_Text trackTitleText;
    [SerializeField] private TMP_Text playPauseIconText;

    [Header("Эквалайзер визуализатора")]
    [SerializeField] private RectTransform[] eqBars;

    private int currentTrackIndex = 0; // 0 = Lo-Fi, 1 = Synthwave, 2 = Sunset Code Jam
    private int currentAmbienceMode = 0; // 0 = Off, 1 = Rain, 2 = Night, 3 = Cafe
    private bool isPlaying = false;

    private float musicVolume = 0.45f;
    private float ambienceVolume = 0.35f;

    public bool IsMusicOrAmbienceActive => isPlaying || (currentAmbienceMode > 0);

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            if (musicSource != null) musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat("Dev_MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }
    }

    public float AmbienceVolume
    {
        get => ambienceVolume;
        set
        {
            ambienceVolume = Mathf.Clamp01(value);
            if (ambienceSource != null) ambienceSource.volume = ambienceVolume;
            PlayerPrefs.SetFloat("Dev_AmbienceVolume", ambienceVolume);
            PlayerPrefs.Save();
        }
    }

    private readonly string[] TrackNames = new string[]
    {
        "☕ Lo-Fi Chill Beats",
        "🌆 Synthwave Night",
        "🌅 Sunset Code Jam"
    };

    private readonly string[] AmbienceNames = new string[]
    {
        "🔇 ЭМБИЕНТ: ВЫКЛ",
        "🌧 ДОЖДЬ ЗА ОКНОМ",
        "🌙 НОЧНОЙ ГОРОД",
        "☕ УЮТНАЯ КОФЕЙНЯ"
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        musicVolume = PlayerPrefs.GetFloat("Dev_MusicVolume", 0.45f);
        ambienceVolume = PlayerPrefs.GetFloat("Dev_AmbienceVolume", 0.35f);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        musicSource.volume = musicVolume;

        if (ambienceSource == null)
        {
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
        }
        ambienceSource.volume = ambienceVolume;

        EnsureProceduralClips();
        BindButtons();
    }

    private void EnsureProceduralClips()
    {
        if (sunsetTrack == null)
        {
            sunsetTrack = CreateProceduralSunsetTrack();
        }

        if (cafeAmbience == null)
        {
            cafeAmbience = CreateProceduralCafeAmbience();
        }
    }

    private AudioClip CreateProceduralSunsetTrack()
    {
        int sampleRate = 44100;
        int durationSec = 6;
        int count = sampleRate * durationSec;
        float[] data = new float[count];

        // Lo-Fi прогрессия: Fmaj7 -> G7 -> Em7 -> Am7
        float[] baseFreqs = new float[] { 174.61f, 196.00f, 164.81f, 220.00f };

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;
            int chordIdx = (int)(t / 1.5f) % baseFreqs.Length;
            float root = baseFreqs[chordIdx];
            float localT = t % 1.5f;

            float env = Mathf.Exp(-localT * 1.8f) * 0.32f;
            float pad = (Mathf.Sin(2f * Mathf.PI * root * t) * 0.4f +
                         Mathf.Sin(2f * Mathf.PI * root * 1.25f * t) * 0.3f +
                         Mathf.Sin(2f * Mathf.PI * root * 1.5f * t) * 0.2f +
                         Mathf.Sin(2f * Mathf.PI * root * 1.875f * t) * 0.15f) * env;

            // Мягкий Lo-Fi винил-шум
            float vinyl = (Random.value * 2f - 1f) * 0.018f;

            data[i] = Mathf.Clamp(pad + vinyl, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_SunsetTrack", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateProceduralCafeAmbience()
    {
        int sampleRate = 44100;
        int durationSec = 5;
        int count = sampleRate * durationSec;
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / sampleRate;

            // Фоновый тёплый гул (Warm room murmur / low rumble)
            float rumble = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.12f +
                           Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.08f;

            // Шум пара эспрессо-машины (Espresso machine hiss)
            float steamEnv = Mathf.Sin(t * 0.6f) * 0.5f + 0.5f;
            float steam = (Random.value * 2f - 1f) * 0.04f * steamEnv;

            // Звон чашек и ложечек (Cup/Spoon clinks)
            float clink = 0f;
            if (t > 1.2f && t < 1.35f)
            {
                float tc = t - 1.2f;
                clink += Mathf.Sin(2f * Mathf.PI * 2400f * tc) * Mathf.Exp(-tc * 35f) * 0.18f;
            }
            if (t > 3.4f && t < 3.55f)
            {
                float tc = t - 3.4f;
                clink += Mathf.Sin(2f * Mathf.PI * 3100f * tc) * Mathf.Exp(-tc * 40f) * 0.15f;
            }

            data[i] = Mathf.Clamp((rumble + steam + clink) * 0.7f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_CafeAmbience", count, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void Start()
    {
        currentTrackIndex = PlayerPrefs.GetInt("Dev_LoFiTrackIndex", 0);
        isPlaying = PlayerPrefs.GetInt("Dev_LoFiIsPlaying", 1) == 1;
        currentAmbienceMode = PlayerPrefs.GetInt("Dev_AmbienceMode", 0);

        BindButtons();
        ApplyTrack(isPlaying);
        ApplyAmbience(currentAmbienceMode, false);
    }

    private void BindButtons()
    {
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveAllListeners();
            playPauseButton.onClick.AddListener(OnPlayPauseClicked);
        }

        if (nextTrackButton != null)
        {
            nextTrackButton.onClick.RemoveAllListeners();
            nextTrackButton.onClick.AddListener(OnNextTrackClicked);
        }

        if (ambienceButton != null)
        {
            ambienceButton.onClick.RemoveAllListeners();
            ambienceButton.onClick.AddListener(OnAmbienceClicked);
        }
    }

    public void OnAmbienceClicked()
    {
        currentAmbienceMode = (currentAmbienceMode + 1) % AmbienceNames.Length;
        PlayerPrefs.SetInt("Dev_AmbienceMode", currentAmbienceMode);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        HapticFeedback.MediumImpact();

        ApplyAmbience(currentAmbienceMode, true);

        if (ClickJuice.Instance != null && ambienceButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup(AmbienceNames[currentAmbienceMode], ambienceButton.transform.position + Vector3.up * 35f, new Color(0.35f, 0.85f, 1f), false);
        }
    }

    private void ApplyAmbience(int mode, bool animate)
    {
        if (ambienceSource == null) return;

        AudioClip targetClip = null;
        if (mode == 1) targetClip = rainAmbience;
        else if (mode == 2) targetClip = nightAmbience;
        else if (mode == 3) targetClip = cafeAmbience;

        if (targetClip != null)
        {
            ambienceSource.clip = targetClip;
            ambienceSource.Play();
        }
        else
        {
            ambienceSource.Stop();
        }

        if (ambienceButtonText != null)
        {
            switch (mode)
            {
                case 1: ambienceButtonText.text = "🌧 ДОЖДЬ"; break;
                case 2: ambienceButtonText.text = "🌙 НОЧЬ"; break;
                case 3: ambienceButtonText.text = "☕ КОФЕ"; break;
                default: ambienceButtonText.text = "🔇 ЭМБИЕНТ"; break;
            }
        }
    }

    private void Update()
    {
        // Анимация столбиков эквалайзера в такт музыке
        if (eqBars != null && eqBars.Length > 0)
        {
            for (int i = 0; i < eqBars.Length; i++)
            {
                if (eqBars[i] == null) continue;
                float targetH = 6f;
                if (isPlaying)
                {
                    float speed = currentTrackIndex == 1 ? 9.5f : (currentTrackIndex == 2 ? 7.5f : 6.0f);
                    float wave = Mathf.Sin(Time.time * speed + i * 1.35f) * 0.5f + 0.5f;
                    float wave2 = Mathf.Cos(Time.time * (speed * 0.7f) + i * 2.1f) * 0.3f;
                    targetH = Mathf.Clamp(8f + (wave + wave2) * 22f, 6f, 32f);
                }
                Vector2 cur = eqBars[i].sizeDelta;
                eqBars[i].sizeDelta = new Vector2(cur.x, Mathf.Lerp(cur.y, targetH, Time.deltaTime * 14f));
            }
        }
    }

    public void OnPlayPauseClicked()
    {
        isPlaying = !isPlaying;
        PlayerPrefs.SetInt("Dev_LoFiIsPlaying", isPlaying ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        HapticFeedback.MediumImpact();

        if (isPlaying)
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            if (musicSource != null)
            {
                musicSource.Pause();
            }
        }

        UpdateVisuals();

        if (ClickJuice.Instance != null && playPauseButton != null)
        {
            string msg = isPlaying ? "▶ Музыка ВКЛ" : "⏸ Музыка ПАУЗА";
            ClickJuice.Instance.SpawnCustomPopup(msg, playPauseButton.transform.position + Vector3.up * 35f, new Color(0.2f, 0.95f, 0.7f), false);
        }
    }

    public void OnNextTrackClicked()
    {
        currentTrackIndex = (currentTrackIndex + 1) % TrackNames.Length;
        PlayerPrefs.SetInt("Dev_LoFiTrackIndex", currentTrackIndex);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();
        HapticFeedback.MediumImpact();

        ApplyTrack(isPlaying);

        if (ClickJuice.Instance != null && nextTrackButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎵 {TrackNames[currentTrackIndex]}", nextTrackButton.transform.position + Vector3.up * 35f, new Color(0f, 0.85f, 1f), false);
        }
    }

    private void ApplyTrack(bool play)
    {
        if (musicSource == null) return;

        AudioClip clipToPlay = lofiTrack;
        if (currentTrackIndex == 1) clipToPlay = synthwaveTrack;
        else if (currentTrackIndex == 2) clipToPlay = sunsetTrack;

        if (clipToPlay != null)
        {
            musicSource.clip = clipToPlay;
            if (play) musicSource.Play();
            else musicSource.Stop();
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (trackTitleText != null)
        {
            trackTitleText.text = TrackNames[currentTrackIndex % TrackNames.Length];
        }

        if (playPauseIconText != null)
        {
            playPauseIconText.text = isPlaying ? "❚❚" : "▶";
        }
    }
}
