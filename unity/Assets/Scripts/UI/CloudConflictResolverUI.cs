using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Разрешение конфликтов облачных сохранений (Cloud Save Conflict Resolver):
/// - Автоматическое определение расхождений между локальным сохранением и Яндекс Облаком
/// - Наглядное сравнение двух версий (Дата, Баланс ₽, Строки кода, Престиж, Релизы)
/// - Выбор игрока: "Оставить локальное", "Загрузить из облака" или "Умное слияние (Smart Merge)"
/// </summary>
public class CloudConflictResolverUI : MonoBehaviour
{
    private static CloudConflictResolverUI instance;
    public static CloudConflictResolverUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<CloudConflictResolverUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class SaveSnapshot
    {
        public string timestamp;
        public double money;
        public double codeLines;
        public int prestige;
        public int releases;
        public string rawData;
    }

    [Header("Модальное окно конфликта")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private Button closeXBtn;

    [Header("Локальная карточка")]
    [SerializeField] private TMP_Text localDateText;
    [SerializeField] private TMP_Text localMoneyText;
    [SerializeField] private TMP_Text localCodeText;
    [SerializeField] private TMP_Text localPrestigeText;
    [SerializeField] private Button chooseLocalBtn;

    [Header("Облачная карточка")]
    [SerializeField] private TMP_Text cloudDateText;
    [SerializeField] private TMP_Text cloudMoneyText;
    [SerializeField] private TMP_Text cloudCodeText;
    [SerializeField] private TMP_Text cloudPrestigeText;
    [SerializeField] private Button chooseCloudBtn;

    [Header("Кнопка умного слияния")]
    [SerializeField] private Button smartMergeBtn;

    private SaveSnapshot currentLocalSnapshot;
    private SaveSnapshot currentCloudSnapshot;
    private Action onResolutionComplete;

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
        EnsureUIExists();
        BindButtons();
    }

    private void BindButtons()
    {
        if (chooseLocalBtn != null)
        {
            chooseLocalBtn.onClick.RemoveAllListeners();
            chooseLocalBtn.onClick.AddListener(OnChooseLocalClicked);
        }

        if (chooseCloudBtn != null)
        {
            chooseCloudBtn.onClick.RemoveAllListeners();
            chooseCloudBtn.onClick.AddListener(OnChooseCloudClicked);
        }

        if (smartMergeBtn != null)
        {
            smartMergeBtn.onClick.RemoveAllListeners();
            smartMergeBtn.onClick.AddListener(OnSmartMergeClicked);
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
    }

    public void CheckCloudSync(Action onComplete = null)
    {
        onResolutionComplete = onComplete;
        if (YandexSDKBridge.Instance != null)
        {
            YandexSDKBridge.Instance.LoadCloudData(
                onSuccess: (cloudJson) =>
                {
                    EvaluateData(cloudJson);
                },
                onError: (err) =>
                {
                    Debug.LogWarning($"[CloudConflictResolver] LoadCloudData error: {err}");
                    onResolutionComplete?.Invoke();
                }
            );
        }
        else
        {
            // Режим симуляции для отладки
            Debug.Log("[CloudConflictResolver] YandexSDKBridge not found, conflict check skipped.");
            onResolutionComplete?.Invoke();
        }
    }

    public void EvaluateData(string cloudJson)
    {
        if (string.IsNullOrEmpty(cloudJson))
        {
            // Облако пустое - сохраняем текущий прогресс
            TriggerLocalSaveToCloud();
            onResolutionComplete?.Invoke();
            return;
        }

        SaveSnapshot local = CaptureLocalSnapshot();
        SaveSnapshot cloud = ParseSnapshotFromJson(cloudJson);

        if (cloud == null)
        {
            onResolutionComplete?.Invoke();
            return;
        }

        // Проверяем наличие значимого конфликта:
        // Если разница в деньгах или коде более 10% или разница в престиже
        bool hasConflict = false;
        double moneyDiff = Math.Abs(local.money - cloud.money);
        double codeDiff = Math.Abs(local.codeLines - cloud.codeLines);

        if (local.prestige != cloud.prestige) hasConflict = true;
        else if (moneyDiff > Math.Max(500.0, local.money * 0.10)) hasConflict = true;
        else if (codeDiff > Math.Max(200.0, local.codeLines * 0.10)) hasConflict = true;

        if (hasConflict)
        {
            ShowConflictModal(local, cloud, onResolutionComplete);
        }
        else
        {
            // Данные почти идентичны, тихо синхронизируем
            onResolutionComplete?.Invoke();
        }
    }

    public void ShowConflictModal(SaveSnapshot local, SaveSnapshot cloud, Action onComplete = null)
    {
        EnsureUIExists();
        currentLocalSnapshot = local;
        currentCloudSnapshot = cloud;
        onResolutionComplete = onComplete;

        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }
        }

        UpdateCardsUI();
        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    private void UpdateCardsUI()
    {
        if (currentLocalSnapshot != null)
        {
            if (localDateText != null) localDateText.text = $"🕒 {FormatDate(currentLocalSnapshot.timestamp)}";
            if (localMoneyText != null) localMoneyText.text = $"💰 <b>{NumberFormatter.Format(currentLocalSnapshot.money)} ₽</b>";
            if (localCodeText != null) localCodeText.text = $"💻 {NumberFormatter.Format(currentLocalSnapshot.codeLines)} строк";
            if (localPrestigeText != null) localPrestigeText.text = $"⭐ Престиж: ур. {currentLocalSnapshot.prestige} ({currentLocalSnapshot.releases} рел.)";
        }

        if (currentCloudSnapshot != null)
        {
            if (cloudDateText != null) cloudDateText.text = $"🕒 {FormatDate(currentCloudSnapshot.timestamp)}";
            if (cloudMoneyText != null) cloudMoneyText.text = $"💰 <b>{NumberFormatter.Format(currentCloudSnapshot.money)} ₽</b>";
            if (cloudCodeText != null) cloudCodeText.text = $"💻 {NumberFormatter.Format(currentCloudSnapshot.codeLines)} строк";
            if (cloudPrestigeText != null) cloudPrestigeText.text = $"⭐ Престиж: ур. {currentCloudSnapshot.prestige} ({currentCloudSnapshot.releases} рел.)";
        }
    }

    private string FormatDate(string isoDate)
    {
        if (DateTime.TryParse(isoDate, out DateTime dt))
        {
            return dt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        }
        return "Неизвестно";
    }

    public void OnChooseLocalClicked()
    {
        // Перезаписываем облако локальными данными
        TriggerLocalSaveToCloud();
        CloseModal();
        HapticFeedback.SuccessPattern();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("☁️ Локальное сохранение выгружено в облако!", transform.position, new Color(0.2f, 1f, 0.6f), true);
        }

        onResolutionComplete?.Invoke();
    }

    public void OnChooseCloudClicked()
    {
        if (currentCloudSnapshot != null && !string.IsNullOrEmpty(currentCloudSnapshot.rawData))
        {
            ApplySnapshotToGame(currentCloudSnapshot);
            CloseModal();
            HapticFeedback.SuccessPattern();

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("☁️ Облачное сохранение успешно загружено!", transform.position, new Color(0f, 0.85f, 1f), true);
            }

            // Перезагрузка сцены для гарантированного обновления всех счетчиков и компонентов
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void OnSmartMergeClicked()
    {
        if (currentLocalSnapshot == null || currentCloudSnapshot == null) return;

        // Создаем объединенный снимок с лучшими показателями
        SaveSnapshot merged = new SaveSnapshot();
        merged.timestamp = DateTime.UtcNow.ToString("o");
        merged.money = Math.Max(currentLocalSnapshot.money, currentCloudSnapshot.money);
        merged.codeLines = Math.Max(currentLocalSnapshot.codeLines, currentCloudSnapshot.codeLines);
        merged.prestige = Math.Max(currentLocalSnapshot.prestige, currentCloudSnapshot.prestige);
        merged.releases = Math.Max(currentLocalSnapshot.releases, currentCloudSnapshot.releases);

        // Применяем в GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(merged.money - GameManager.Instance.Money);
            GameManager.Instance.AddLinesOfCode(merged.codeLines - GameManager.Instance.CodeLines);
            GameManager.Instance.SaveGame();
        }

        // Выгружаем объединенное состояние в облако
        TriggerLocalSaveToCloud();
        CloseModal();
        HapticFeedback.SuccessPattern();

        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("🧠 Умное слияние завершено! Сохранены лучшие показатели.", transform.position, new Color(1f, 0.85f, 0.2f), true);
        }

        onResolutionComplete?.Invoke();
    }

    private void TriggerLocalSaveToCloud()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
        }

        SaveSnapshot snap = CaptureLocalSnapshot();
        string json = JsonUtility.ToJson(snap);

        if (YandexSDKBridge.Instance != null)
        {
            YandexSDKBridge.Instance.SaveCloudData(json,
                onSuccess: () => Debug.Log("[CloudConflictResolver] Cloud save synced successfully."),
                onError: (e) => Debug.LogWarning($"[CloudConflictResolver] Cloud sync failed: {e}")
            );
        }
    }

    private SaveSnapshot CaptureLocalSnapshot()
    {
        SaveSnapshot snap = new SaveSnapshot();
        snap.timestamp = DateTime.UtcNow.ToString("o");
        snap.money = GameManager.Instance != null ? GameManager.Instance.Money : 0;
        snap.codeLines = GameManager.Instance != null ? GameManager.Instance.CodeLines : 0;
        snap.prestige = GameManager.Instance != null ? GameManager.Instance.PrestigeLevel : 0;
        snap.releases = GameManager.Instance != null ? GameManager.Instance.TotalReleasesCount : 0;
        snap.rawData = "";
        return snap;
    }

    private SaveSnapshot ParseSnapshotFromJson(string json)
    {
        try
        {
            var snap = JsonUtility.FromJson<SaveSnapshot>(json);
            if (snap != null)
            {
                snap.rawData = json;
                return snap;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CloudConflictResolver] Failed to parse cloud json: {ex.Message}");
        }
        return null;
    }

    private void ApplySnapshotToGame(SaveSnapshot snap)
    {
        if (snap == null) return;
        PlayerPrefs.SetString("DevGame_Code", snap.codeLines.ToString("R"));
        PlayerPrefs.SetString("DevGame_Money", snap.money.ToString("R"));
        PlayerPrefs.SetInt("DevGame_Prestige", snap.prestige);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGame();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.Vibrate(15);
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

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("CloudConflictResolverModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        // Backdrop
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.82f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(500, 520);
        cardObj.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.13f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.6f, 0.1f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Header
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -18);
        headerRt.sizeDelta = new Vector2(-40, 34);
        TMP_Text headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "⚠️ КОНФЛИКТ СОХРАНЕНИЙ";
        headerTxt.fontSize = 20;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.alignment = TextAlignmentOptions.Center;
        headerTxt.color = new Color(1f, 0.75f, 0.2f);

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(cardObj.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0, 1);
        subRt.anchorMax = new Vector2(1, 1);
        subRt.pivot = new Vector2(0.5f, 1);
        subRt.anchoredPosition = new Vector2(0, -52);
        subRt.sizeDelta = new Vector2(-40, 36);
        TMP_Text subTxt = subObj.GetComponent<TextMeshProUGUI>();
        subTxt.text = "Данные на этом устройстве отличаются от облака Яндекс Игр. Выберите, какую версию оставить:";
        subTxt.fontSize = 12;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.75f, 0.8f, 0.9f);

        // Close X
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeXRt = closeXObj.GetComponent<RectTransform>();
        closeXRt.anchorMin = new Vector2(1, 1);
        closeXRt.anchorMax = new Vector2(1, 1);
        closeXRt.anchoredPosition = new Vector2(-25, -25);
        closeXRt.sizeDelta = new Vector2(34, 34);
        closeXObj.GetComponent<Image>().color = new Color(0.25f, 0.1f, 0.12f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject closeXTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeXTxtObj.transform.SetParent(closeXObj.transform, false);
        TMP_Text cTxt = closeXTxtObj.GetComponent<TextMeshProUGUI>();
        cTxt.text = "✕";
        cTxt.fontSize = 16;
        cTxt.alignment = TextAlignmentOptions.Center;
        cTxt.color = Color.white;

        // Container for 2 cards
        GameObject cardsRow = new GameObject("CardsRow", typeof(RectTransform));
        cardsRow.transform.SetParent(cardObj.transform, false);
        RectTransform rowRt = cardsRow.GetComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0, 1);
        rowRt.anchorMax = new Vector2(1, 1);
        rowRt.pivot = new Vector2(0.5f, 1);
        rowRt.anchoredPosition = new Vector2(0, -96);
        rowRt.sizeDelta = new Vector2(-30, 260);

        // Left Card: Local
        CreateVersionCard(cardsRow.transform, true, out localDateText, out localMoneyText, out localCodeText, out localPrestigeText, out chooseLocalBtn);

        // Right Card: Cloud
        CreateVersionCard(cardsRow.transform, false, out cloudDateText, out cloudMoneyText, out cloudCodeText, out cloudPrestigeText, out chooseCloudBtn);

        // Smart Merge Button at bottom
        GameObject mergeBtnObj = new GameObject("SmartMergeBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        mergeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform mergeRt = mergeBtnObj.GetComponent<RectTransform>();
        mergeRt.anchorMin = new Vector2(0, 0);
        mergeRt.anchorMax = new Vector2(1, 0);
        mergeRt.pivot = new Vector2(0.5f, 0);
        mergeRt.anchoredPosition = new Vector2(0, 20);
        mergeRt.sizeDelta = new Vector2(-40, 48);
        mergeBtnObj.GetComponent<Image>().color = new Color(0.1f, 0.65f, 0.45f);
        smartMergeBtn = mergeBtnObj.GetComponent<Button>();

        GameObject mergeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        mergeTxtObj.transform.SetParent(mergeBtnObj.transform, false);
        RectTransform mergeTxtRt = mergeTxtObj.GetComponent<RectTransform>();
        mergeTxtRt.anchorMin = Vector2.zero;
        mergeTxtRt.anchorMax = Vector2.one;
        mergeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text mergeTxt = mergeTxtObj.GetComponent<TextMeshProUGUI>();
        mergeTxt.text = "🧠 УМНОЕ СЛИЯНИЕ (Объединить лучшее)";
        mergeTxt.fontSize = 14;
        mergeTxt.fontStyle = FontStyles.Bold;
        mergeTxt.alignment = TextAlignmentOptions.Center;
        mergeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private void CreateVersionCard(Transform parent, bool isLocal, out TMP_Text dateTxt, out TMP_Text moneyTxt, out TMP_Text codeTxt, out TMP_Text presTxt, out Button actionBtn)
    {
        GameObject c = new GameObject(isLocal ? "LocalCard" : "CloudCard", typeof(RectTransform), typeof(Image));
        c.transform.SetParent(parent, false);
        RectTransform crt = c.GetComponent<RectTransform>();
        crt.anchorMin = isLocal ? new Vector2(0, 0) : new Vector2(0.52f, 0);
        crt.anchorMax = isLocal ? new Vector2(0.48f, 1) : new Vector2(1, 1);
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;
        c.GetComponent<Image>().color = isLocal ? new Color(0.11f, 0.15f, 0.22f) : new Color(0.12f, 0.18f, 0.26f);

        // Header
        GameObject h = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        h.transform.SetParent(c.transform, false);
        RectTransform hrt = h.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0, 1);
        hrt.anchorMax = new Vector2(1, 1);
        hrt.pivot = new Vector2(0.5f, 1);
        hrt.anchoredPosition = new Vector2(0, -10);
        hrt.sizeDelta = new Vector2(-16, 24);
        TMP_Text ht = h.GetComponent<TextMeshProUGUI>();
        ht.text = isLocal ? "📱 НА УСТРОЙСТВЕ" : "☁️ В ОБЛАКЕ";
        ht.fontSize = 14;
        ht.fontStyle = FontStyles.Bold;
        ht.alignment = TextAlignmentOptions.Center;
        ht.color = isLocal ? new Color(0.4f, 0.85f, 1f) : new Color(0.2f, 1f, 0.6f);

        // Date
        GameObject d = new GameObject("Date", typeof(RectTransform), typeof(TextMeshProUGUI));
        d.transform.SetParent(c.transform, false);
        RectTransform drt = d.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0, 1);
        drt.anchorMax = new Vector2(1, 1);
        drt.pivot = new Vector2(0.5f, 1);
        drt.anchoredPosition = new Vector2(0, -36);
        drt.sizeDelta = new Vector2(-16, 20);
        dateTxt = d.GetComponent<TextMeshProUGUI>();
        dateTxt.fontSize = 10;
        dateTxt.alignment = TextAlignmentOptions.Center;
        dateTxt.color = new Color(0.7f, 0.75f, 0.85f);

        // Money
        GameObject m = new GameObject("Money", typeof(RectTransform), typeof(TextMeshProUGUI));
        m.transform.SetParent(c.transform, false);
        RectTransform mrt = m.GetComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0, 1);
        mrt.anchorMax = new Vector2(1, 1);
        mrt.pivot = new Vector2(0.5f, 1);
        mrt.anchoredPosition = new Vector2(0, -68);
        mrt.sizeDelta = new Vector2(-16, 24);
        moneyTxt = m.GetComponent<TextMeshProUGUI>();
        moneyTxt.fontSize = 13;
        moneyTxt.alignment = TextAlignmentOptions.Center;

        // Code
        GameObject cd = new GameObject("Code", typeof(RectTransform), typeof(TextMeshProUGUI));
        cd.transform.SetParent(c.transform, false);
        RectTransform cdrt = cd.GetComponent<RectTransform>();
        cdrt.anchorMin = new Vector2(0, 1);
        cdrt.anchorMax = new Vector2(1, 1);
        cdrt.pivot = new Vector2(0.5f, 1);
        cdrt.anchoredPosition = new Vector2(0, -96);
        cdrt.sizeDelta = new Vector2(-16, 22);
        codeTxt = cd.GetComponent<TextMeshProUGUI>();
        codeTxt.fontSize = 11;
        codeTxt.alignment = TextAlignmentOptions.Center;
        codeTxt.color = new Color(0.85f, 0.9f, 0.95f);

        // Prestige
        GameObject p = new GameObject("Prestige", typeof(RectTransform), typeof(TextMeshProUGUI));
        p.transform.SetParent(c.transform, false);
        RectTransform prt = p.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 1);
        prt.anchorMax = new Vector2(1, 1);
        prt.pivot = new Vector2(0.5f, 1);
        prt.anchoredPosition = new Vector2(0, -122);
        prt.sizeDelta = new Vector2(-16, 22);
        presTxt = p.GetComponent<TextMeshProUGUI>();
        presTxt.fontSize = 11;
        presTxt.alignment = TextAlignmentOptions.Center;
        presTxt.color = new Color(1f, 0.85f, 0.3f);

        // Action Button
        GameObject b = new GameObject("ChooseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(c.transform, false);
        RectTransform brt = b.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 0);
        brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(0.5f, 0);
        brt.anchoredPosition = new Vector2(0, 12);
        brt.sizeDelta = new Vector2(-20, 36);
        b.GetComponent<Image>().color = isLocal ? new Color(0.18f, 0.45f, 0.85f) : new Color(0.15f, 0.65f, 0.55f);
        actionBtn = b.GetComponent<Button>();

        GameObject bt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        bt.transform.SetParent(b.transform, false);
        RectTransform btrt = bt.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TMP_Text btxt = bt.GetComponent<TextMeshProUGUI>();
        btxt.text = isLocal ? "ВЫБРАТЬ ЭТО" : "ЗАГРУЗИТЬ";
        btxt.fontSize = 12;
        btxt.fontStyle = FontStyles.Bold;
        btxt.alignment = TextAlignmentOptions.Center;
        btxt.color = Color.white;
    }
}
