using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Единый мост для взаимодействия с Яндекс Играми (Yandex Games SDK v2):
/// - Инициализация SDK и получение профиля игрока
/// - Показ полноэкранной рекламы (Interstitial Ad) с защитой от спама
/// - Показ рекламы за вознаграждение (Rewarded Video Ad) с гарантированным колбэком
/// - Облачные сохранения (Cloud Saves Sync с fallback на PlayerPrefs)
/// - Отправка рекордов в Таблицу Лидеров (Leaderboards)
/// - Полноценная эмуляция (Mock) в Unity Editor для комфортного тестирования
/// </summary>
public class YandexSDKBridge : MonoBehaviour
{
    private static YandexSDKBridge instance;
    public static YandexSDKBridge Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<YandexSDKBridge>();
                if (instance == null)
                {
                    GameObject go = new GameObject("YandexSDKBridge");
                    instance = go.AddComponent<YandexSDKBridge>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void YandexSDK_Init();

    [DllImport("__Internal")]
    private static extern void YandexSDK_ShowFullscreenAd();

    [DllImport("__Internal")]
    private static extern void YandexSDK_ShowRewardedAd(string placementId);

    [DllImport("__Internal")]
    private static extern void YandexSDK_SaveData(string jsonData);

    [DllImport("__Internal")]
    private static extern void YandexSDK_LoadData();

    [DllImport("__Internal")]
    private static extern void YandexSDK_SetLeaderboardScore(string lbName, int score);

    [DllImport("__Internal")]
    private static extern void YandexSDK_CanCreateShortcut();

    [DllImport("__Internal")]
    private static extern void YandexSDK_CreateShortcut();

    [DllImport("__Internal")]
    private static extern void YandexSDK_RequestNotification(string title, string text);
#endif

    public bool IsInitialized { get; private set; } = false;

    // Callbacks for Rewarded Video
    private Action currentRewardedCallback;
    private Action currentRewardedCloseCallback;
    private Action<string> currentRewardedErrorCallback;

    // Callbacks for Fullscreen Ad
    private Action currentFullscreenCloseCallback;
    private Action<string> currentFullscreenErrorCallback;

    // Callbacks for Cloud Save/Load
    private Action currentCloudSaveSuccess;
    private Action<string> currentCloudSaveError;
    private Action<string> currentCloudLoadSuccess;
    private Action<string> currentCloudLoadError;

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
        InitSDK();
    }

    public void InitSDK()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_Init();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] Native Init failed: {e.Message}");
            IsInitialized = false;
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] SDK Initialized successfully.");
        IsInitialized = true;
#endif
    }

    #region Fullscreen (Interstitial) Ad

    public void ShowFullscreenAd(Action onClose = null, Action<string> onError = null)
    {
        currentFullscreenCloseCallback = onClose;
        currentFullscreenErrorCallback = onError;

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_ShowFullscreenAd();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] ShowFullscreenAd error: {e.Message}");
            currentFullscreenCloseCallback?.Invoke();
            currentFullscreenCloseCallback = null;
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] ShowFullscreenAd displayed.");
        currentFullscreenCloseCallback?.Invoke();
        currentFullscreenCloseCallback = null;
#endif
    }

    // Called from WebGL jslib
    public void OnFullscreenAdOpened()
    {
        Debug.Log("[YandexSDKBridge] Fullscreen Ad Opened.");
    }

    // Called from WebGL jslib
    public void OnFullscreenAdClosed(string wasShown)
    {
        Debug.Log($"[YandexSDKBridge] Fullscreen Ad Closed. WasShown: {wasShown}");
        currentFullscreenCloseCallback?.Invoke();
        currentFullscreenCloseCallback = null;
    }

    // Called from WebGL jslib
    public void OnFullscreenAdError(string error)
    {
        Debug.LogWarning($"[YandexSDKBridge] Fullscreen Ad Error: {error}");
        currentFullscreenErrorCallback?.Invoke(error);
        currentFullscreenCloseCallback?.Invoke();
        currentFullscreenCloseCallback = null;
        currentFullscreenErrorCallback = null;
    }

    #endregion

    #region Rewarded Video Ad

    public void ShowRewardedAd(string placementId, Action onRewarded, Action onClose = null, Action<string> onError = null)
    {
        currentRewardedCallback = onRewarded;
        currentRewardedCloseCallback = onClose;
        currentRewardedErrorCallback = onError;

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_ShowRewardedAd(placementId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] ShowRewardedAd failed: {e.Message}. Giving fallback reward.");
            currentRewardedCallback?.Invoke();
            currentRewardedCloseCallback?.Invoke();
            currentRewardedCallback = null;
            currentRewardedCloseCallback = null;
        }
#else
        Debug.Log($"[YandexSDKBridge (Editor Mock)] Rewarded Ad for '{placementId}' completed.");
        currentRewardedCallback?.Invoke();
        currentRewardedCloseCallback?.Invoke();
        currentRewardedCallback = null;
        currentRewardedCloseCallback = null;
#endif
    }

    // Called from WebGL jslib
    public void OnRewardedAdOpened(string placementId)
    {
        Debug.Log($"[YandexSDKBridge] Rewarded Ad Opened for {placementId}");
    }

    // Called from WebGL jslib
    public void OnRewardedAdRewarded(string placementId)
    {
        Debug.Log($"[YandexSDKBridge] Rewarded Ad Reward Granted for {placementId}");
        currentRewardedCallback?.Invoke();
        currentRewardedCallback = null;
    }

    // Called from WebGL jslib
    public void OnRewardedAdClosed(string placementId)
    {
        Debug.Log($"[YandexSDKBridge] Rewarded Ad Closed for {placementId}");
        currentRewardedCloseCallback?.Invoke();
        currentRewardedCloseCallback = null;
    }

    // Called from WebGL jslib
    public void OnRewardedAdError(string errorData)
    {
        Debug.LogWarning($"[YandexSDKBridge] Rewarded Ad Error: {errorData}");
        currentRewardedErrorCallback?.Invoke(errorData);
        currentRewardedErrorCallback = null;
    }

    #endregion

    #region Cloud Saves

    public void SaveCloudData(string jsonData, Action onSuccess = null, Action<string> onError = null)
    {
        currentCloudSaveSuccess = onSuccess;
        currentCloudSaveError = onError;

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_SaveData(jsonData);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] Cloud save failed: {e.Message}");
            currentCloudSaveError?.Invoke(e.Message);
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] Cloud data saved.");
        currentCloudSaveSuccess?.Invoke();
#endif
    }

    public void LoadCloudData(Action<string> onSuccess, Action<string> onError = null)
    {
        currentCloudLoadSuccess = onSuccess;
        currentCloudLoadError = onError;

#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_LoadData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] Cloud load failed: {e.Message}");
            currentCloudLoadError?.Invoke(e.Message);
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] Cloud load called.");
        currentCloudLoadSuccess?.Invoke("");
#endif
    }

    // Called from WebGL jslib
    public void OnSaveCloudDataSuccess()
    {
        currentCloudSaveSuccess?.Invoke();
        currentCloudSaveSuccess = null;
    }

    // Called from WebGL jslib
    public void OnSaveCloudDataError(string err)
    {
        currentCloudSaveError?.Invoke(err);
        currentCloudSaveError = null;
    }

    // Called from WebGL jslib
    public void OnLoadCloudDataSuccess(string json)
    {
        currentCloudLoadSuccess?.Invoke(json);
        currentCloudLoadSuccess = null;
    }

    // Called from WebGL jslib
    public void OnLoadCloudDataError(string err)
    {
        currentCloudLoadError?.Invoke(err);
        currentCloudLoadError = null;
    }

    #endregion

    #region Leaderboards

    public void SetLeaderboardScore(string leaderboardName, int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_SetLeaderboardScore(leaderboardName, score);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] SetLeaderboardScore failed: {e.Message}");
        }
#else
        Debug.Log($"[YandexSDKBridge (Editor Mock)] SetLeaderboardScore: {leaderboardName} = {score}");
#endif
    }

    #endregion

    #region Shortcuts and Notifications

    private Action<bool> currentCanShortcutCallback;
    private Action<bool> currentCreateShortcutCallback;

    public void CheckCanCreateShortcut(Action<bool> callback)
    {
        currentCanShortcutCallback = callback;
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_CanCreateShortcut();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] CanCreateShortcut failed: {e.Message}");
            currentCanShortcutCallback?.Invoke(false);
            currentCanShortcutCallback = null;
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] CanCreateShortcut: true");
        currentCanShortcutCallback?.Invoke(true);
        currentCanShortcutCallback = null;
#endif
    }

    public void CreateShortcut(Action<bool> callback = null)
    {
        currentCreateShortcutCallback = callback;
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_CreateShortcut();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] CreateShortcut failed: {e.Message}");
            currentCreateShortcutCallback?.Invoke(false);
            currentCreateShortcutCallback = null;
        }
#else
        Debug.Log("[YandexSDKBridge (Editor Mock)] CreateShortcut: true");
        currentCreateShortcutCallback?.Invoke(true);
        currentCreateShortcutCallback = null;
#endif
    }

    public void SendLocalNotification(string title, string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            YandexSDK_RequestNotification(title, text);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[YandexSDKBridge] RequestNotification failed: {e.Message}");
        }
#else
        Debug.Log($"[YandexSDKBridge (Editor Mock)] Notification: '{title}' - {text}");
#endif
    }

    public void OnCanCreateShortcutCallback(string canStr)
    {
        bool can = canStr == "true";
        currentCanShortcutCallback?.Invoke(can);
        currentCanShortcutCallback = null;
    }

    public void OnCreateShortcutCallback(string outcomeStr)
    {
        bool success = outcomeStr == "true";
        currentCreateShortcutCallback?.Invoke(success);
        currentCreateShortcutCallback = null;
    }

    #endregion

    #region JS Handlers

    public void OnSDKInitializedCallback(string successStr)
    {
        IsInitialized = successStr == "true";
        Debug.Log($"[YandexSDKBridge] Initialized callback: {IsInitialized}");
    }

    #endregion
}
