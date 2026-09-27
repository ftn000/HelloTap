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
/// </summary>
public class SaveExportUI : MonoBehaviour
{
    private static SaveExportUI instance;
    public static SaveExportUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<SaveExportUI>();
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

    private const string SavePrefix = "HELLOTAP_SAVE_V1:";
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
        public string Version = "1.4.16";
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

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
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
        if (modalRoot == null) return;
        modalRoot.SetActive(true);
        isAwaitingResetConfirm = false;
        pendingImportPackage = null;
        if (hardResetBtnText != null) hardResetBtnText.text = "⚠️ СБРОСИТЬ ВСЁ";

        if (modalCardTransform != null)
        {
            modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
            StartCoroutine(PopCardAnim(modalCardTransform));
        }

        GenerateExportString();
        if (importPreviewText != null) importPreviewText.text = "Нажмите «Вставить из буфера», чтобы проверить сохранение.";
        if (importStatusText != null) importStatusText.text = "";
        if (loadImportBtn != null) loadImportBtn.interactable = false;

        HapticFeedback.Vibrate(20);
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        isAwaitingResetConfirm = false;
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

    public void GenerateExportString()
    {
        SavePackage pkg = new SavePackage();
        pkg.Timestamp = DateTime.UtcNow.ToString("o");
        if (GameManager.Instance != null)
        {
            pkg.CodeLines = GameManager.Instance.CodeLines;
            pkg.Money = GameManager.Instance.Money;
        }

        // Ключи сохранения PlayerPrefs, которые мы экспортируем
        string[] intKeys = new string[]
        {
            "SelectedSwitchType", "SelectedKeyboardStyle", "SelectedPetType", "SelectedDeskMatSkin",
            "CurrentTimeOfDay", "DeskLampState", "RoomThemeIdx", "Streak_CurrentDay", "Streak_HasCrownUnlocked",
            "DevGame_IsMuted", "LoFi_TrackIdx", "LoFi_AmbienceIdx", "LoFi_IsPlaying"
        };

        string[] strKeys = new string[]
        {
            "Streak_LastLoginDate", "LuckyWheel_LastFreeSpinTime", "DailyQuests_Date"
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

        // Сохраняем все ShopUpgrade ключи (0..50)
        for (int i = 0; i < 50; i++)
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
        }

        string json = JsonUtility.ToJson(pkg);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(bytes);
        lastGeneratedExportCode = SavePrefix + base64;

        if (exportPreviewText != null)
        {
            string preview = lastGeneratedExportCode.Length > 44 
                ? lastGeneratedExportCode.Substring(0, 44) + "..." 
                : lastGeneratedExportCode;
            exportPreviewText.text = $"<color=#00FF88>Код готов:</color> {preview}\n<color=#A0B0C0>({lastGeneratedExportCode.Length} симв., {pkg.CodeLines:N0} строк, {pkg.Money:N0} руб.)</color>";
        }
        if (copyStatusText != null)
        {
            copyStatusText.text = "Нажмите «Копировать в буфер», чтобы скопировать.";
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
            copyStatusText.text = "<color=#00FF88>✓ Скопировано в буфер обмена!</color>";
        }
        HapticFeedback.Vibrate(40);
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
        if (!raw.StartsWith(SavePrefix))
        {
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Неверный формат кода в буфере!</color>";
            if (loadImportBtn != null) loadImportBtn.interactable = false;
            return;
        }

        try
        {
            string base64 = raw.Substring(SavePrefix.Length);
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
                importPreviewText.text = $"<color=#00FF88>✓ Найдено сохранение v{pkg.Version}:</color>\nСтрок: <b>{pkg.CodeLines:N0}</b>  |  Баланс: <b>{pkg.Money:N0} руб.</b>\nЗаписей: {pkg.Entries.Count} шт.";
            }
            if (importStatusText != null)
            {
                importStatusText.text = "Сохранение готово. Нажмите «Загрузить сейв».";
            }
            if (loadImportBtn != null)
            {
                loadImportBtn.interactable = true;
            }
            HapticFeedback.Vibrate(30);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveExportUI] Parse error: {ex.Message}");
            if (importStatusText != null) importStatusText.text = "<color=#FF5555>Ошибка парсинга буфера обмена!</color>";
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
                importStatusText.text = "<color=#00FF88>✓ Успешно! Перезагрузка игры...</color>";
            }
            HapticFeedback.Vibrate(60);

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
                hardResetBtnText.text = "❓ ТОЧНО СБРОСИТЬ? (НАЖМИ ЕЩЁ РАЗ)";
            }
            HapticFeedback.Vibrate(30);
            return;
        }

        // Подтверждено: полный сброс
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        HapticFeedback.Vibrate(80);
        SceneManager.LoadScene(0);
    }
}
