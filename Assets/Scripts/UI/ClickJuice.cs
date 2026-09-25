using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Компонент для "сочности" клика: пружинящая анимация кнопки (squash & stretch),
/// всплывающие циферки и смешные цитаты программиста при тапе.
/// </summary>
public class ClickJuice : MonoBehaviour
{
    public static ClickJuice Instance { get; private set; }

    [Header("Анимация кнопки")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float punchScaleFactor = 0.92f;
    [SerializeField] private float bounceDuration = 0.15f;

    [Header("Всплывающий текст")]
    [SerializeField] private TMP_Text floatingTextPrefab;
    [SerializeField] private Transform floatingTextParent;
    [SerializeField] private float floatDistance = 80f;
    [SerializeField] private float floatDuration = 0.7f;

    [Header("Звук печати (опционально)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typingSounds;

    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    private static readonly string[] FunnyCodeLines = new string[]
    {
        "// works on my machine",
        "git push --force",
        "Console.WriteLine();",
        "Debug.Log(\"test 123\");",
        "TODO: rewrite later",
        "NullReferenceException",
        "npm install",
        ";",
        "while(true) { ... }",
        "404 Bug Not Found",
        "git merge --no-ff"
    };

    private void Awake()
    {
        Instance = this;
        if (targetTransform == null)
        {
            targetTransform = transform;
        }
        originalScale = targetTransform.localScale;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 screenPos)
    {
        PlayPunchEffect();
        PlayTypingSound();

        Transform parent = floatingTextParent != null ? floatingTextParent : transform.parent;
        if (parent != null)
        {
            double combo = GameManager.Instance != null ? GameManager.Instance.GetComboMultiplier() : 1.0;
            string comboTag = combo > 1.01 ? $" <size=75%>(x{combo:F1})</size>" : "";
            string message = isCrit
                ? $"<color=#FF5555>КРИТ! +{NumberFormatter.Format(amount)}</color>{comboTag}"
                : $"+{NumberFormatter.Format(amount)}{comboTag}";

            if (!isCrit && Random.value < 0.15f)
            {
                message = FunnyCodeLines[Random.Range(0, FunnyCodeLines.Length)];
            }

            SpawnFloatingText(message, screenPos, isCrit, parent);
        }
    }

    public void SpawnCustomPopup(string message, Vector2 screenPos, Color color, bool isLarge = true)
    {
        Transform parent = floatingTextParent != null ? floatingTextParent : transform.parent;
        if (parent == null) return;
        SpawnFloatingText(message, screenPos, isLarge, parent, color);
    }

    public void PlayPunchEffect()
    {
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }
        punchCoroutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        targetTransform.localScale = originalScale * punchScaleFactor;
        float elapsed = 0f;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / bounceDuration;
            float bounceT = Mathf.Sin(t * Mathf.PI);
            targetTransform.localScale = Vector3.Lerp(originalScale * punchScaleFactor, originalScale, t) + (originalScale * (bounceT * 0.08f));
            yield return null;
        }

        targetTransform.localScale = originalScale;
    }

    private void SpawnFloatingText(string text, Vector2 spawnPos, bool isCrit, Transform parent, Color? customColor = null)
    {
        TMP_Text instance;
        if (floatingTextPrefab != null)
        {
            instance = Instantiate(floatingTextPrefab, parent);
            instance.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = new GameObject("FloatingText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            instance = go.GetComponent<TextMeshProUGUI>();
            instance.fontSize = 28;
            instance.fontStyle = FontStyles.Bold;
            instance.alignment = TextAlignmentOptions.Center;
            instance.color = new Color(0f, 1f, 0.55f);
            instance.raycastTarget = false;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(360f, 70f);
        }

        if (customColor.HasValue)
        {
            instance.color = customColor.Value;
        }

        if (spawnPos != Vector2.zero)
        {
            instance.transform.position = spawnPos + new Vector2(Random.Range(-25f, 25f), Random.Range(-10f, 10f));
        }
        else
        {
            instance.transform.position = targetTransform.position + new Vector3(Random.Range(-50f, 50f), 50f, 0f);
        }

        instance.text = text;
        if (isCrit)
        {
            instance.fontSize *= 1.25f;
        }

        StartCoroutine(FloatAndFadeRoutine(instance));
    }

    private IEnumerator FloatAndFadeRoutine(TMP_Text txt)
    {
        Vector3 startPos = txt.transform.position;
        Vector3 endPos = startPos + Vector3.up * floatDistance;
        Color startColor = txt.color;
        float elapsed = 0f;

        while (elapsed < floatDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / floatDuration;

            txt.transform.position = Vector3.Lerp(startPos, endPos, t);
            txt.color = new Color(startColor.r, startColor.g, startColor.b, 1f - (t * t));
            yield return null;
        }

        Destroy(txt.gameObject);
    }

    private void PlayTypingSound()
    {
        if (audioSource != null && typingSounds != null && typingSounds.Length > 0)
        {
            AudioClip clip = typingSounds[Random.Range(0, typingSounds.Length)];
            audioSource.pitch = Random.Range(0.92f, 1.08f);
            audioSource.PlayOneShot(clip, 0.7f);
        }
    }
}
