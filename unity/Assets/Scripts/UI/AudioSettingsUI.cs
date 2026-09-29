using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Окно настроек звука (Audio Settings Modal):
/// - 4 независимых регулятора громкости: Общая (Master), Эффекты (SFX), Музыка (Music), Эмбиент (Ambience)
/// - Быстрый Mute-переключатель с цветовой индикацией
/// - Звуковые профили (Пресеты): «Стандарт», «Ночной релакс», «Максимальный фокус»
/// - Проигрывание тестового клика при настройке SFX
/// - Плавная анимация модального окна и тактильный отклик
/// </summary>
public class AudioSettingsUI : MonoBehaviour
{
    private static AudioSettingsUI instance;
    public static AudioSettingsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<AudioSettingsUI>();
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openSettingsBtn;
    [SerializeField] private Button closeSettingsBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Регуляторы громкости (Ползунки)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterValText;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValText;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicValText;
    [SerializeField] private Slider ambienceSlider;
    [SerializeField] private TMP_Text ambienceValText;

    [Header("Быстрый Mute и Пресеты")]
    [SerializeField] private Button muteToggleBtn;
    [SerializeField] private TMP_Text muteToggleText;
    [SerializeField] private Button presetDefaultBtn;
    [SerializeField] private Button presetNightBtn;
    [SerializeField] private Button presetFocusBtn;

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
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
        LoadCurrentVolumes();
    }

    private void BindButtons()
    {
        if (openSettingsBtn != null)
        {
            openSettingsBtn.onClick.RemoveAllListeners();
            openSettingsBtn.onClick.AddListener(OpenModal);
        }
        if (closeSettingsBtn != null)
        {
            closeSettingsBtn.onClick.RemoveAllListeners();
            closeSettingsBtn.onClick.AddListener(CloseModal);
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

        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }
        if (ambienceSlider != null)
        {
            ambienceSlider.onValueChanged.RemoveAllListeners();
            ambienceSlider.onValueChanged.AddListener(OnAmbienceSliderChanged);
        }

        if (muteToggleBtn != null)
        {
            muteToggleBtn.onClick.RemoveAllListeners();
            muteToggleBtn.onClick.AddListener(OnMuteToggleClicked);
        }

        if (presetDefaultBtn != null)
        {
            presetDefaultBtn.onClick.RemoveAllListeners();
            presetDefaultBtn.onClick.AddListener(() => ApplyPreset(1.0f, 0.85f, 0.45f, 0.35f, "Баланс"));
        }
        if (presetNightBtn != null)
        {
            presetNightBtn.onClick.RemoveAllListeners();
            presetNightBtn.onClick.AddListener(() => ApplyPreset(0.6f, 0.35f, 0.15f, 0.60f, "Ночной чилл"));
        }
        if (presetFocusBtn != null)
        {
            presetFocusBtn.onClick.RemoveAllListeners();
            presetFocusBtn.onClick.AddListener(() => ApplyPreset(0.9f, 1.0f, 0.65f, 0.0f, "Макс. фокус"));
        }
    }

    public void OpenModal()
    {
        if (modalRoot == null) return;
        modalRoot.SetActive(true);
        LoadCurrentVolumes();

        if (modalCardTransform != null)
        {
            modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
            StartCoroutine(PopCardAnim(modalCardTransform));
        }
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

    private void LoadCurrentVolumes()
    {
        float master = AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : 1.0f;
        float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.85f;
        float music = LoFiPlayerUI.Instance != null ? LoFiPlayerUI.Instance.MusicVolume : 0.45f;
        float amb = LoFiPlayerUI.Instance != null ? LoFiPlayerUI.Instance.AmbienceVolume : 0.35f;

        if (masterSlider != null) masterSlider.SetValueWithoutNotify(master);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(music);
        if (ambienceSlider != null) ambienceSlider.SetValueWithoutNotify(amb);

        UpdateValueLabels(master, sfx, music, amb);
        UpdateMuteButtonVisual();
    }

    private void UpdateValueLabels(float master, float sfx, float music, float amb)
    {
        if (masterValText != null) masterValText.text = $"{Mathf.RoundToInt(master * 100)}%";
        if (sfxValText != null) sfxValText.text = $"{Mathf.RoundToInt(sfx * 100)}%";
        if (musicValText != null) musicValText.text = $"{Mathf.RoundToInt(music * 100)}%";
        if (ambienceValText != null) ambienceValText.text = $"{Mathf.RoundToInt(amb * 100)}%";
    }

    private void OnMasterSliderChanged(float val)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = val;
        UpdateValueLabels(val, sfxSlider != null ? sfxSlider.value : 0.85f, musicSlider != null ? musicSlider.value : 0.45f, ambienceSlider != null ? ambienceSlider.value : 0.35f);
        HapticFeedback.LightImpact();
    }

    private void OnSfxSliderChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxVolume = val;
            AudioManager.Instance.PlayTyping();
        }
        UpdateValueLabels(masterSlider != null ? masterSlider.value : 1.0f, val, musicSlider != null ? musicSlider.value : 0.45f, ambienceSlider != null ? ambienceSlider.value : 0.35f);
        HapticFeedback.LightImpact();
    }

    private void OnMusicSliderChanged(float val)
    {
        if (LoFiPlayerUI.Instance != null) LoFiPlayerUI.Instance.MusicVolume = val;
        UpdateValueLabels(masterSlider != null ? masterSlider.value : 1.0f, sfxSlider != null ? sfxSlider.value : 0.85f, val, ambienceSlider != null ? ambienceSlider.value : 0.35f);
        HapticFeedback.LightImpact();
    }

    private void OnAmbienceSliderChanged(float val)
    {
        if (LoFiPlayerUI.Instance != null) LoFiPlayerUI.Instance.AmbienceVolume = val;
        UpdateValueLabels(masterSlider != null ? masterSlider.value : 1.0f, sfxSlider != null ? sfxSlider.value : 0.85f, musicSlider != null ? musicSlider.value : 0.45f, val);
        HapticFeedback.LightImpact();
    }

    private void OnMuteToggleClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateMuteButtonVisual();
            HapticFeedback.MediumImpact();
        }
    }

    private void UpdateMuteButtonVisual()
    {
        if (AudioManager.Instance == null) return;
        bool isMuted = AudioManager.Instance.IsMuted;
        if (muteToggleText != null)
        {
            muteToggleText.text = isMuted ? "🔇 ЗВУК: ВЫКЛЮЧЕН" : "🔊 ЗВУК: ВКЛЮЧЕН";
            muteToggleText.color = isMuted ? new Color(1f, 0.4f, 0.4f) : new Color(0.2f, 1f, 0.6f);
        }
    }

    private void ApplyPreset(float master, float sfx, float music, float amb, string presetName)
    {
        if (masterSlider != null) masterSlider.value = master;
        if (sfxSlider != null) sfxSlider.value = sfx;
        if (musicSlider != null) musicSlider.value = music;
        if (ambienceSlider != null) ambienceSlider.value = amb;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MasterVolume = master;
            AudioManager.Instance.SfxVolume = sfx;
        }
        if (LoFiPlayerUI.Instance != null)
        {
            LoFiPlayerUI.Instance.MusicVolume = music;
            LoFiPlayerUI.Instance.AmbienceVolume = amb;
        }

        UpdateValueLabels(master, sfx, music, amb);
        HapticFeedback.MediumImpact();

        if (ClickJuice.Instance != null && modalCardTransform != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"🎚️ Пресет: {presetName}", modalCardTransform.position + Vector3.up * 40f, new Color(0.3f, 0.9f, 1f), false);
        }
    }
}
