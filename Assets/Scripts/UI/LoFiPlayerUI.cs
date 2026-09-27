using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Плеер фоновой музыки:
/// - Переключение между треками: Chill Lo-Fi и Neon Synthwave
/// - Анимированный эквалайзер (прыгающие столбики спектра)
/// - Кнопки Play/Pause, Next Track, отображение названия трека
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

    [Header("Элементы управления")]
    [SerializeField] private Button playPauseButton;
    [SerializeField] private Button nextTrackButton;
    [SerializeField] private TMP_Text trackTitleText;
    [SerializeField] private TMP_Text playPauseIconText;

    [Header("Эквалайзер визуализатора")]
    [SerializeField] private RectTransform[] eqBars;

    private int currentTrackIndex = 0; // 0 = Lo-Fi, 1 = Synthwave
    private bool isPlaying = false;
    private readonly string[] TrackNames = new string[]
    {
        "☕ Lo-Fi Chill Beats",
        "🌆 Synthwave Night"
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.45f;
        }

        BindButtons();
    }

    private void Start()
    {
        currentTrackIndex = PlayerPrefs.GetInt("Dev_LoFiTrackIndex", 0);
        isPlaying = PlayerPrefs.GetInt("Dev_LoFiIsPlaying", 1) == 1;

        BindButtons();
        ApplyTrack(isPlaying);
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
                    float speed = currentTrackIndex == 1 ? 9.5f : 6.0f;
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
        HapticFeedback.Vibrate(25);

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
        HapticFeedback.Vibrate(25);

        ApplyTrack(isPlaying);

        if (ClickJuice.Instance != null && nextTrackButton != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎵 {TrackNames[currentTrackIndex]}", nextTrackButton.transform.position + Vector3.up * 35f, new Color(0f, 0.85f, 1f), false);
        }
    }

    private void ApplyTrack(bool play)
    {
        if (musicSource == null) return;

        AudioClip clipToPlay = currentTrackIndex == 1 ? synthwaveTrack : lofiTrack;
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
