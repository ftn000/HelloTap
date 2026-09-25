using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Переключатель звука (Mute/Unmute) с динамической иконкой 🔊 / 🔇.
/// </summary>
public class AudioToggleButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }
        UpdateVisual();
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute();
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (buttonText != null && AudioManager.Instance != null)
        {
            buttonText.text = AudioManager.Instance.IsMuted ? "ЗВУК: ВЫКЛ" : "ЗВУК: ВКЛ";
            buttonText.color = AudioManager.Instance.IsMuted ? new Color(1f, 0.45f, 0.45f) : new Color(0.2f, 0.95f, 0.65f);
        }
    }
}
