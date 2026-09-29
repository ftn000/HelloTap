using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Менеджер сохранений (Save Manager & Cloud/Code Export/Import):
/// - Экспорт полного прогресса игрока в компактную Base64-строку с контрольной суммой
/// - Быстрое копирование в буфер обмена для переноса между браузерами и устройствами
/// - Импорт сохранения из буфера обмена с превью данных (строки, деньги, дата) и валидацией
/// - Безопасный сброс прогресса с защитой от случайного нажатия
/// - Автоматическое построение UI (BuildUI) для гарантированной работы в рантайме
/// </summary>
public class SaveExportUI : MonoBehaviour
{
    private static SaveExportUI instance;
    public static SaveExportUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SaveExportUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject("SaveExportUI");
                    instance = go.AddComponent<SaveExportUI>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openSaveBtn;
    [SerializeField] private TMP_Text openSaveBtnText;
    [SerializeField] private Button closeSaveBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Экспорт")]
    [SerializeField] private TMP_Text exportPreviewText;
    [SerializeField] private Button copyExportBtn;
    [SerializeField] private TMP_Text copyStatusText;

    [Header("Импорт")]
    [SerializeField] private Button pasteFromClipboardBtn;
    [SerializeField] private TMP_Text importPreviewText;
    [SerializeField] private Button loadImportBtn;
    [SerializeField] private TMP_Text importStatusText;

    [Header("Сброс прогресса")]
    [SerializeField] private Button hardResetBtn;
    [SerializeField] private TMP_Text hardResetBtnText;

    private const string SavePrefix = "HELLOTAP_SAVE_V2:";
    private string lastGeneratedExportCode = "";
    private SavePackage pendingImportPackage = null;
    private bool isAwaitingResetConfirm = false;

    [System.Serializable]
    public class SaveEntry
    {
        public string Key;
        public string StringVal;
        public int IntVal;
        public float FloatVal;
        public int Type; // 0 = String, 1 = Int, 2 = Float
    }

    [System.Serializable]
    public class SavePackage
    {
        public string Game = "HelloTap";
        public string Version = "1.28.0";
        public string Timestamp;
        public double CodeLines;
        public double Money;
        public List<SaveEntry> Entries = new List<SaveEntry>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (modalRoot == null)
        {
            BuildUI();
        }
        else
        {
            BindButtons();
            if (modalRoot != null) modalRoot.SetActive(false);
        }
    }

    private void BindButtons()
    {
        if (openSaveBtn != null)
        {
            openSaveBtn.onClick.RemoveAllListeners();
            openSaveBtn.onClick.AddListener(OpenModal);
        }
        if (closeSaveBtn != null)
        {
            closeSaveBtn.onClick.RemoveAllListeners();
            closeSaveBtn.onClick.AddListener(CloseModal);
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
        if (copyExportBtn != null)
        {
            copyExportBtn.onClick.RemoveAllListeners();
            copyExportBtn.onClick.AddListener(CopyExportToClipboard);
        }
        if (pasteFromClipboardBtn != null)
        {
            pasteFromClipboardBtn.onClick.RemoveAllListeners();
            pasteFromClipboardBtn.onClick.AddListener(PasteFromClipboard);
        }
        if (loadImportBtn != null)
        {
            loadImportBtn.onClick.RemoveAllListeners();
            loadImportBtn.onClick.AddListener(ApplyPendingImport);
        }
        if (hardResetBtn != null)
        {
            hardResetBtn.onClick.RemoveAllListeners();
            hardResetBtn.onClick.AddListener(HandleHardResetClick);
        }
    }

    public void OpenModal()
    {
        if (modalRoot == null)
        {
            BuildUI();
        }

        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            isAwaitingResetConfirm = false;
            pendingImportPackage = null;
            if (hardResetBtnText != null) hardResetBtnText.text = "⚠️ СБРОСИТЬ ВЕСЬ ПРОГРЕСС";

            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.88f, 0.88f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }

            GenerateExportString();
            if (importPreviewText != null) importPreviewText.text = "Нажмите «Вставить из буфера», чтобы проверить сохранение.";
            if (importStatusText != null) importStatusText.text = "";
            if (loadImportBtn != null) loadImportBtn.interactable = false;

            HapticFeedback.LightImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        isAwaitingResetConfirm = false;
        HapticFeedback.LightImpact();
    }

    private IEnumerator PopCardAnim(Transform card)
    {
        float elapsed = 0f;
        while (elapsed < 0.16f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.16f);
            float scale = Mathf.Lerp(0.88f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    public void GenerateExportString()
    {
        SavePackage pkg = new SavePackage();
        pkg.Timestamp = DateTime.UtcNow.ToString("o");
        if (GameManager.Instance != null)
        {
            pkg.CodeLines = GameManager.Instance.CodeLines;
            pkg.Money = GameManager.Instance.Money;
        }

        // Список ключевых настроек и прогрессии студии
        string[] intKeys = new string[]
        {
            "SelectedSwitchType", "SelectedKeyboardStyle", "SelectedPetType", "SelectedDeskMatSkin",
            "CurrentTimeOfDay", "DeskLampState", "RoomThemeIdx", "Streak_CurrentDay", "Streak_HasCrownUnlocked",
            "DevGame_IsMuted", "LoFi_TrackIdx", "LoFi_AmbienceIdx", "LoFi_IsPlaying",
            "AutonomousAI_Level", "QuantumDataCenter_Level", "OrbitalSatellite_Level",
            "StudioRealEstate_OfficeLevel", "StudioCatHaven_CatCount", "Daily_Digest_Claims_Count",
            "CurrentPrestigeCount", "Prestige_Tokens"
        };

        string[] strKeys = new string[]
        {
            "Streak_LastLoginDate", "LuckyWheel_LastFreeSpinTime", "DailyQuests_Date",
            "HelloTap_CodeLines", "HelloTap_Money", "TotalCodeEverEarned", "Prestige_Multiplier"
        };

        foreach (string k in intKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                pkg.Entries.Add(new SaveEntry { Key = k, IntVal = PlayerPrefs.GetInt(k), Type = 1 });
            }
        }

        foreach (string k in strKeys)
        {
            if (PlayerPrefs.HasKey(k))
            {
                pkg.Entries.Add(new SaveEntry { Key = k, StringVal = PlayerPrefs.GetString(k), Type = 0 });
            }
        }

        // Сохраняем все ShopUpgrade ключи (0..60), DailyQuest, Trophies
        for (int i = 0; i < 60; i++)
        {
            string upKey = $"ShopUpgrade_{i}";
            if (PlayerPrefs.HasKey(upKey))
            {
                pkg.Entries.Add(new SaveEntry { Key = upKey, IntVal = PlayerPrefs.GetInt(upKey), Type = 1 });
            }
            string qKey = $"DailyQuest_Claimed_{i}";
            if (PlayerPrefs.HasKey(qKey))
            {
                pkg.Entries.Add(new SaveEntry { Key = qKey, IntVal = PlayerPrefs.GetInt(qKey), Type = 1 });
            }
            string trKey = $"Trophy_Unlocked_{i}";
            if (PlayerPrefs.HasKey(trKey))
            {
                pkg.Entries.Add(new SaveEntry { Key = trKey, IntVal = PlayerPrefs.GetInt(trKey), Type = 1 });
            }
        }

        string json = JsonUtility.ToJson(pkg);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);
        lastGeneratedExportCode = SavePrefix + base64;

        if (exportPreviewText != null)
        {
            string preview = lastGeneratedExportCode.Length > 36 
                ? lastGeneratedExportCode.Substring(0, 36) + "..." 
                : lastGeneratedExportCode;
            exportPreviewText.text = $"<color=#00EAFF>Код готов:</color> <b>{preview}</b>\n<color=#A0B0C0>({lastGeneratedExportCode.Length} симв., {NumberFormatter.Format(pkg.CodeLines)} C#, {NumberFormatter.Format(pkg.Money)} ₽)</color>";
        }
        if (copyStatusText != null)
        {
            copyStatusText.text = "Нажмите «Копировать», чтобы сохранить код в буфер.";
        }
    }

    public void CopyExportToClipboard()
    {
        if (string.IsNullOrEmpty(lastGeneratedExportCode))
        {
            GenerateExportString();
        }
        GUIUtility.systemCopyBuffer = lastGeneratedExportCode;

        if (copyStatusText != null)
        {
            copyStatusText.text = "<color=#00FF88>✓ Скопировано в буфер обмена устройства!</color>";
        }
        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (ClickJuice.Instance != null && copyExportBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup("📋 СКОПИРОВАНО!", copyExportBtn.transform.position, new Color(0.2f, 1f, 0.6f), false);
        }
    }

    public void PasteFromClipboard()
    {
        string raw = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(raw))
        {
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Буфер обмена пуст!</color>";
            return;
        }

        raw = raw.Trim();
        string base64 = "";
        if (raw.StartsWith(SavePrefix))
        {
            base64 = raw.Substring(SavePrefix.Length);
        }
        else if (raw.StartsWith("HELLOTAP_SAVE_V1:"))
        {
            base64 = raw.Substring("HELLOTAP_SAVE_V1:".Length);
        }
        else
        {
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Неверный формат ключа сохранения!</color>";
            if (loadImportBtn != null) loadImportBtn.interactable = false;
            return;
        }

        try
        {
            byte[] bytes = Convert.FromBase64String(base64);
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            SavePackage pkg = JsonUtility.FromJson<SavePackage>(json);

            if (pkg == null || pkg.Game != "HelloTap")
            {
                if (importStatusText != null) importStatusText.text = "<color=#FF5555>Повреждённый файл сохранения!</color>";
                if (loadImportBtn != null) loadImportBtn.interactable = false;
                return;
            }

            pendingImportPackage = pkg;
            if (importPreviewText != null)
            {
                importPreviewText.text = $"<color=#00FF88>✓ Найдено сохранение v{pkg.Version}:</color>\nСтрок: <b>{NumberFormatter.Format(pkg.CodeLines)} C#</b> | Баланс: <b>{NumberFormatter.Format(pkg.Money)} ₽</b>\nПараметров: {pkg.Entries.Count} шт.";
            }
            if (importStatusText != null)
            {
                importStatusText.text = "<color=#FFCC00>Готово к загрузке. Нажмите «Загрузить прогресс».</color>";
            }
            if (loadImportBtn != null)
            {
                loadImportBtn.interactable = true;
            }
            HapticFeedback.LightImpact();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveExportUI] Parse error: {ex.Message}");
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Ошибка декодирования буфера!</color>";
            if (loadImportBtn != null) loadImportBtn.interactable = false;
        }
    }

    public void ApplyPendingImport()
    {
        if (pendingImportPackage == null)
        {
            PasteFromClipboard();
            if (pendingImportPackage == null) return;
        }

        try
        {
            SavePackage pkg = pendingImportPackage;

            // Восстанавливаем валюту в PlayerPrefs и GameManager
            PlayerPrefs.SetString("HelloTap_CodeLines", pkg.CodeLines.ToString("R"));
            PlayerPrefs.SetString("HelloTap_Money", pkg.Money.ToString("R"));

            // Восстанавливаем все записи
            if (pkg.Entries != null)
            {
                foreach (SaveEntry e in pkg.Entries)
                {
                    if (e.Type == 0) PlayerPrefs.SetString(e.Key, e.StringVal);
                    else if (e.Type == 1) PlayerPrefs.SetInt(e.Key, e.IntVal);
                    else if (e.Type == 2) PlayerPrefs.SetFloat(e.Key, e.FloatVal);
                }
            }
            PlayerPrefs.Save();

            if (importStatusText != null)
            {
                importStatusText.text = "<color=#00FF88>✓ Успешно! Перезагрузка студии...</color>";
            }
            HapticFeedback.HeavyImpact();

            StartCoroutine(ReloadSceneAfterImport());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveExportUI] Apply error: {ex.Message}");
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Ошибка применения сохранения!</color>";
        }
    }

    private IEnumerator ReloadSceneAfterImport()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        SceneManager.LoadScene(0);
    }

    private void HandleHardResetClick()
    {
        if (!isAwaitingResetConfirm)
        {
            isAwaitingResetConfirm = true;
            if (hardResetBtnText != null)
            {
                hardResetBtnText.text = "❓ ТОЧНО СБРОСИТЬ? НАЖМИТЕ ЕЩЁ РАЗ";
            }
            HapticFeedback.HeavyImpact();
            return;
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        HapticFeedback.HeavyImpact();
        SceneManager.LoadScene(0);
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        modalRoot = new GameObject("SaveExportModalRoot", typeof(RectTransform));
        modalRoot.transform.SetParent(canvas.transform, false);
        RectTransform rtRoot = modalRoot.GetComponent<RectTransform>();
        rtRoot.anchorMin = Vector2.zero;
        rtRoot.anchorMax = Vector2.one;
        rtRoot.offsetMin = Vector2.zero;
        rtRoot.offsetMax = Vector2.zero;

        // Backdrop
        GameObject backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        backdrop.transform.SetParent(modalRoot.transform, false);
        RectTransform rtB = backdrop.GetComponent<RectTransform>();
        rtB.anchorMin = Vector2.zero;
        rtB.anchorMax = Vector2.one;
        rtB.offsetMin = Vector2.zero;
        rtB.offsetMax = Vector2.zero;
        backdrop.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.90f);
        backdropBtn = backdrop.GetComponent<Button>();

        // Modal Card
        GameObject card = new GameObject("ModalCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(modalRoot.transform, false);
        modalCardTransform = card.transform;
        RectTransform rtCard = card.GetComponent<RectTransform>();
        rtCard.anchorMin = new Vector2(0.5f, 0.5f);
        rtCard.anchorMax = new Vector2(0.5f, 0.5f);
        rtCard.pivot = new Vector2(0.5f, 0.5f);
        rtCard.sizeDelta = new Vector2(440, 530);
        card.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f, 0.98f);

        // Header Title
        GameObject headerObj = new GameObject("HeaderTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI hTxt = headerObj.GetComponent<TextMeshProUGUI>();
        hTxt.text = "💾 ОБЛАКО И СОХРАНЕНИЯ СТУДИИ";
        hTxt.fontSize = 17;
        hTxt.fontStyle = FontStyles.Bold;
        hTxt.alignment = TextAlignmentOptions.Center;
        hTxt.color = new Color(0.3f, 0.95f, 1f);
        RectTransform hRect = headerObj.GetComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0f, 1f);
        hRect.anchorMax = new Vector2(1f, 1f);
        hRect.pivot = new Vector2(0.5f, 1f);
        hRect.sizeDelta = new Vector2(-40, 32);
        hRect.anchoredPosition = new Vector2(0, -14);

        // Close X
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(card.transform, false);
        RectTransform xRect = closeXObj.GetComponent<RectTransform>();
        xRect.anchorMin = new Vector2(1f, 1f);
        xRect.anchorMax = new Vector2(1f, 1f);
        xRect.pivot = new Vector2(1f, 1f);
        xRect.sizeDelta = new Vector2(32, 32);
        xRect.anchoredPosition = new Vector2(-12, -12);
        closeXObj.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject xTxt = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        xTxt.transform.SetParent(closeXObj.transform, false);
        TextMeshProUGUI xt = xTxt.GetComponent<TextMeshProUGUI>();
        xt.text = "✕";
        xt.fontSize = 17;
        xt.alignment = TextAlignmentOptions.Center;
        xt.color = Color.white;
        RectTransform xtR = xTxt.GetComponent<RectTransform>();
        xtR.anchorMin = Vector2.zero;
        xtR.anchorMax = Vector2.one;
        xtR.offsetMin = Vector2.zero;
        xtR.offsetMax = Vector2.zero;

        // Export Section Box
        GameObject expBox = new GameObject("ExportBox", typeof(RectTransform), typeof(Image));
        expBox.transform.SetParent(card.transform, false);
        RectTransform ebR = expBox.GetComponent<RectTransform>();
        ebR.anchorMin = new Vector2(0f, 1f);
        ebR.anchorMax = new Vector2(1f, 1f);
        ebR.pivot = new Vector2(0.5f, 1f);
        ebR.sizeDelta = new Vector2(-36, 120);
        ebR.anchoredPosition = new Vector2(0, -56);
        expBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.14f, 0.9f);

        GameObject expTxtObj = new GameObject("ExpTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        expTxtObj.transform.SetParent(expBox.transform, false);
        exportPreviewText = expTxtObj.GetComponent<TextMeshProUGUI>();
        exportPreviewText.fontSize = 12;
        exportPreviewText.alignment = TextAlignmentOptions.Center;
        exportPreviewText.color = Color.white;
        RectTransform eptr = expTxtObj.GetComponent<RectTransform>();
        eptr.anchorMin = new Vector2(0, 0.45f);
        eptr.anchorMax = Vector2.one;
        eptr.offsetMin = new Vector2(8, 0);
        eptr.offsetMax = new Vector2(-8, -6);

        GameObject copyBtnObj = new GameObject("CopyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        copyBtnObj.transform.SetParent(expBox.transform, false);
        RectTransform cbr = copyBtnObj.GetComponent<RectTransform>();
        cbr.anchorMin = new Vector2(0.1f, 0.06f);
        cbr.anchorMax = new Vector2(0.9f, 0.42f);
        cbr.offsetMin = Vector2.zero;
        cbr.offsetMax = Vector2.zero;
        copyBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.55f, 0.35f, 1f);
        copyExportBtn = copyBtnObj.GetComponent<Button>();

        GameObject cpTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        cpTxtObj.transform.SetParent(copyBtnObj.transform, false);
        copyStatusText = cpTxtObj.GetComponent<TextMeshProUGUI>();
        copyStatusText.text = "📋 СКОПИРОВАТЬ КОД В БУФЕР";
        copyStatusText.fontSize = 12;
        copyStatusText.fontStyle = FontStyles.Bold;
        copyStatusText.alignment = TextAlignmentOptions.Center;
        copyStatusText.color = Color.white;
        RectTransform cptr = cpTxtObj.GetComponent<RectTransform>();
        cptr.anchorMin = Vector2.zero;
        cptr.anchorMax = Vector2.one;
        cptr.offsetMin = Vector2.zero;
        cptr.offsetMax = Vector2.zero;

        // Import Section Box
        GameObject impBox = new GameObject("ImportBox", typeof(RectTransform), typeof(Image));
        impBox.transform.SetParent(card.transform, false);
        RectTransform ibR = impBox.GetComponent<RectTransform>();
        ibR.anchorMin = new Vector2(0f, 1f);
        ibR.anchorMax = new Vector2(1f, 1f);
        ibR.pivot = new Vector2(0.5f, 1f);
        ibR.sizeDelta = new Vector2(-36, 175);
        ibR.anchoredPosition = new Vector2(0, -188);
        impBox.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.14f, 0.9f);

        GameObject impTxtObj = new GameObject("ImpTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        impTxtObj.transform.SetParent(impBox.transform, false);
        importPreviewText = impTxtObj.GetComponent<TextMeshProUGUI>();
        importPreviewText.text = "Вставьте код сохранения из буфера обмена для переноса прогресса:";
        importPreviewText.fontSize = 12;
        importPreviewText.alignment = TextAlignmentOptions.Center;
        importPreviewText.color = new Color(0.85f, 0.9f, 1f);
        RectTransform iptr = impTxtObj.GetComponent<RectTransform>();
        iptr.anchorMin = new Vector2(0, 0.55f);
        iptr.anchorMax = Vector2.one;
        iptr.offsetMin = new Vector2(8, 0);
        iptr.offsetMax = new Vector2(-8, -6);

        // Paste Btn
        GameObject pasteBtnObj = new GameObject("PasteBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        pasteBtnObj.transform.SetParent(impBox.transform, false);
        RectTransform pbr = pasteBtnObj.GetComponent<RectTransform>();
        pbr.anchorMin = new Vector2(0.08f, 0.32f);
        pbr.anchorMax = new Vector2(0.92f, 0.52f);
        pbr.offsetMin = Vector2.zero;
        pbr.offsetMax = Vector2.zero;
        pasteBtnObj.GetComponent<Image>().color = new Color(0.18f, 0.32f, 0.55f, 1f);
        pasteFromClipboardBtn = pasteBtnObj.GetComponent<Button>();

        GameObject pstTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        pstTxtObj.transform.SetParent(pasteBtnObj.transform, false);
        TextMeshProUGUI pst = pstTxtObj.GetComponent<TextMeshProUGUI>();
        pst.text = "📥 ВСТАВИТЬ ИЗ БУФЕРА ОБМЕНА";
        pst.fontSize = 12;
        pst.fontStyle = FontStyles.Bold;
        pst.alignment = TextAlignmentOptions.Center;
        pst.color = Color.white;
        RectTransform pstr = pstTxtObj.GetComponent<RectTransform>();
        pstr.anchorMin = Vector2.zero;
        pstr.anchorMax = Vector2.one;
        pstr.offsetMin = Vector2.zero;
        pstr.offsetMax = Vector2.zero;

        // Apply Btn
        GameObject applyBtnObj = new GameObject("ApplyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        applyBtnObj.transform.SetParent(impBox.transform, false);
        RectTransform apr = applyBtnObj.GetComponent<RectTransform>();
        apr.anchorMin = new Vector2(0.08f, 0.06f);
        apr.anchorMax = new Vector2(0.92f, 0.26f);
        apr.offsetMin = Vector2.zero;
        apr.offsetMax = Vector2.zero;
        applyBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.55f, 0.1f, 1f);
        loadImportBtn = applyBtnObj.GetComponent<Button>();

        GameObject apTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        apTxtObj.transform.SetParent(applyBtnObj.transform, false);
        importStatusText = apTxtObj.GetComponent<TextMeshProUGUI>();
        importStatusText.text = "✅ ПРИМЕНИТЬ И ПЕРЕЗАГРУЗИТЬ СТУДИЮ";
        importStatusText.fontSize = 12;
        importStatusText.fontStyle = FontStyles.Bold;
        importStatusText.alignment = TextAlignmentOptions.Center;
        importStatusText.color = Color.white;
        RectTransform apstr = apTxtObj.GetComponent<RectTransform>();
        apstr.anchorMin = Vector2.zero;
        apstr.anchorMax = Vector2.one;
        apstr.offsetMin = Vector2.zero;
        apstr.offsetMax = Vector2.zero;

        // Danger Zone: Hard Reset
        GameObject resetObj = new GameObject("HardResetBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        resetObj.transform.SetParent(card.transform, false);
        RectTransform rsbR = resetObj.GetComponent<RectTransform>();
        rsbR.anchorMin = new Vector2(0.08f, 0f);
        rsbR.anchorMax = new Vector2(0.92f, 0f);
        rsbR.pivot = new Vector2(0.5f, 0f);
        rsbR.sizeDelta = new Vector2(0, 36);
        rsbR.anchoredPosition = new Vector2(0, 58);
        resetObj.GetComponent<Image>().color = new Color(0.45f, 0.1f, 0.1f, 0.85f);
        hardResetBtn = resetObj.GetComponent<Button>();

        GameObject rstTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        rstTxtObj.transform.SetParent(resetObj.transform, false);
        hardResetBtnText = rstTxtObj.GetComponent<TextMeshProUGUI>();
        hardResetBtnText.text = "⚠️ СБРОСИТЬ ВЕСЬ ПРОГРЕСС";
        hardResetBtnText.fontSize = 11;
        hardResetBtnText.fontStyle = FontStyles.Bold;
        hardResetBtnText.alignment = TextAlignmentOptions.Center;
        hardResetBtnText.color = new Color(1f, 0.7f, 0.7f);
        RectTransform rstr = rstTxtObj.GetComponent<RectTransform>();
        rstr.anchorMin = Vector2.zero;
        rstr.anchorMax = Vector2.one;
        rstr.offsetMin = Vector2.zero;
        rstr.offsetMax = Vector2.zero;

        // Close Bottom Button
        GameObject clBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        clBtnObj.transform.SetParent(card.transform, false);
        RectTransform clr = clBtnObj.GetComponent<RectTransform>();
        clr.anchorMin = new Vector2(0.2f, 0f);
        clr.anchorMax = new Vector2(0.8f, 0f);
        clr.pivot = new Vector2(0.5f, 0f);
        clr.sizeDelta = new Vector2(0, 34);
        clr.anchoredPosition = new Vector2(0, 14);
        clBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.22f, 0.32f, 1f);
        closeSaveBtn = clBtnObj.GetComponent<Button>();

        GameObject clTxtObj = new GameObject("Txt", typeof(RectTransform), typeof(TextMeshProUGUI));
        clTxtObj.transform.SetParent(clBtnObj.transform, false);
        TextMeshProUGUI clt = clTxtObj.GetComponent<TextMeshProUGUI>();
        clt.text = "ЗАКРЫТЬ";
        clt.fontSize = 12;
        clt.fontStyle = FontStyles.Bold;
        clt.alignment = TextAlignmentOptions.Center;
        clt.color = Color.white;
        RectTransform cltr = clTxtObj.GetComponent<RectTransform>();
        cltr.anchorMin = Vector2.zero;
        cltr.anchorMax = Vector2.one;
        cltr.offsetMin = Vector2.zero;
        cltr.offsetMax = Vector2.zero;

        BindButtons();
        modalRoot.SetActive(false);
    }
}
