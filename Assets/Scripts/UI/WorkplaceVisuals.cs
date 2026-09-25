using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Управляет атмосферой рабочего места инди-разработчика:
/// анимация бегущего кода на мониторе, подсветка клавиатуры при кликах,
/// покачивание кружки кофе и мерцание курсора терминала.
/// </summary>
public class WorkplaceVisuals : MonoBehaviour
{
    [Header("Монитор и Код")]
    [SerializeField] private TMP_Text monitorCodeText;
    [SerializeField] private Graphic monitorScreenGlow;
    [SerializeField] private int maxVisibleLines = 14;

    [Header("Клавиатура")]
    [SerializeField] private Transform keyboardTransform;
    [SerializeField] private Graphic keyboardGlowImage;
    [SerializeField] private Color keyGlowNormal = new Color(0, 0.89f, 1f, 0f);
    [SerializeField] private Color keyGlowActive = new Color(0, 1f, 0.55f, 0.35f);

    [Header("Кружка кофе / Энергетик")]
    [SerializeField] private Transform coffeeMugTransform;
    [SerializeField] private Transform energyCanTransform;

    [Header("Кот-маскот")]
    [SerializeField] private Transform catTransform;

    private readonly System.Collections.Generic.List<string> terminalHistory = new System.Collections.Generic.List<string>();
    private float cursorTimer = 0f;
    private bool cursorVisible = true;
    private Coroutine glowCoroutine;

    private static readonly string[] CodeSnippets = new string[]
    {
        "<color=#569CD6>public class</color> <color=#4EC9B0>GameManager</color> : <color=#4EC9B0>MonoBehaviour</color>",
        "  [<color=#4EC9B0>SerializeField</color>] <color=#569CD6>private double</color> codeLines;",
        "  <color=#569CD6>void</color> <color=#DCDCAA>Update</color>() => <color=#DCDCAA>ProcessIncome</color>();",
        "  <color=#569CD6>if</color> (isCrit) <color=#DCDCAA>PlayPunchEffect</color>();",
        "  <color=#CE9178>\"Build succeeded in 0.42s\"</color>",
        "  <color=#6A9955>// TODO: optimize draw calls</color>",
        "  <color=#4EC9B0>Instantiate</color>(coinPrefab, mousePos);",
        "  <color=#569CD6>var</color> profit = income * <color=#B5CEA8>2.0</color>;",
        "  <color=#4EC9B0>PlayerPrefs</color>.<color=#DCDCAA>Save</color>();",
        "  <color=#CE9178>\"Deploying to Yandex Games...\"</color>",
        "  <color=#569CD6>yield return new</color> <color=#4EC9B0>WaitForSeconds</color>(<color=#B5CEA8>0.1f</color>);",
        "  <color=#DCDCAA>Debug</color>.<color=#DCDCAA>Log</color>(<color=#CE9178>\"Release published!\"</color>);",
        "  <color=#569CD6>float</color> fps = <color=#B5CEA8>1.0f</color> / <color=#4EC9B0>Time</color>.deltaTime;",
        "  <color=#569CD6>git</color> commit -m <color=#CE9178>\"feat: add juice\"</color>"
    };

    private void Awake()
    {
        // Инициализируем стартовый код
        terminalHistory.Add("<color=#6A9955>// HelloTap Dev Terminal v1.0.0</color>");
        terminalHistory.Add("<color=#569CD6>using</color> UnityEngine;");
        terminalHistory.Add("<color=#569CD6>using</color> System.Collections;");
        terminalHistory.Add("");
        UpdateTerminalDisplay();
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

    private void Update()
    {
        // Мигание курсора в терминале
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= 0.5f)
        {
            cursorTimer = 0f;
            cursorVisible = !cursorVisible;
            UpdateTerminalDisplay();
        }

        // Дыхание кота (плавное покачивание)
        if (catTransform != null)
        {
            float breath = 1f + Mathf.Sin(Time.time * 2.5f) * 0.025f;
            catTransform.localScale = new Vector3(breath, 2f - breath, 1f);
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 screenPos)
    {
        // Добавляем случайную строчку кода в монитор
        string nextLine = CodeSnippets[Random.Range(0, CodeSnippets.Length)];
        terminalHistory.Add(nextLine);

        while (terminalHistory.Count > maxVisibleLines)
        {
            terminalHistory.RemoveAt(0);
        }
        UpdateTerminalDisplay();

        // Подсветка клавиатуры
        if (keyboardGlowImage != null)
        {
            if (glowCoroutine != null) StopCoroutine(glowCoroutine);
            glowCoroutine = StartCoroutine(KeyboardGlowRoutine(isCrit));
        }

        // Отдача клавиатуры
        if (keyboardTransform != null)
        {
            StartCoroutine(KeyboardTapRoutine());
        }

        // Легкое покачивание чашки кофе
        if (coffeeMugTransform != null)
        {
            coffeeMugTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-2.5f, 2.5f));
        }
    }

    private void UpdateTerminalDisplay()
    {
        if (monitorCodeText == null) return;

        string cursor = cursorVisible ? "<color=#00FF88>_</color>" : " ";
        string fullCode = string.Join("\n", terminalHistory) + "\n> " + cursor;
        monitorCodeText.text = fullCode;
    }

    private IEnumerator KeyboardGlowRoutine(bool isCrit)
    {
        keyboardGlowImage.color = isCrit ? Color.yellow : keyGlowActive;
        float elapsed = 0f;
        float duration = isCrit ? 0.25f : 0.12f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            keyboardGlowImage.color = Color.Lerp(keyboardGlowImage.color, keyGlowNormal, elapsed / duration);
            yield return null;
        }

        keyboardGlowImage.color = keyGlowNormal;
    }

    private IEnumerator KeyboardTapRoutine()
    {
        Vector3 baseScale = Vector3.one;
        keyboardTransform.localScale = new Vector3(0.98f, 0.96f, 1f);
        yield return new WaitForSecondsRealtime(0.06f);
        keyboardTransform.localScale = baseScale;
    }
}
