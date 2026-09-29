using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Визуальная эволюция главного экрана («Dynamic Office Background Evolution»):
/// - Динамически обновляет атмосферу, фоновый амбиент и статус штаб-квартиры студии
/// - 5 эпох развития инди-разработчика:
///   1. 📦 Студенческий Гараж (Гаражный старт в Ростове)
///   2. 🏢 Коворкинг IT-Парка (Командный лофт)
///   3. 🌆 Небоскреб Silicon Tower (Пентхаус с панорамным видом)
///   4. ⚛️ Квантовый Технополис (Глобальный хайтек ЦОД)
///   5. 🔴 Марсианский Биокупол Олимп (Межпланетный холдинг)
/// - Элегантный бейдж локации на главном экране с бонусом текущей эпохи
/// </summary>
public class DynamicOfficeEnvironmentUI : MonoBehaviour
{
    private static DynamicOfficeEnvironmentUI instance;
    public static DynamicOfficeEnvironmentUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DynamicOfficeEnvironmentUI>();
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(DynamicOfficeEnvironmentUI));
                    instance = go.AddComponent<DynamicOfficeEnvironmentUI>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
            }
            return instance;
        }
    }

    [System.Serializable]
    public class StudioEra
    {
        public int eraIndex;
        public string eraName;
        public string locationTitle;
        public string icon;
        public Color ambientColor;
        public Color badgeColor;
        public double reqTotalCode;
        public int reqPrestige;
        public string bonusDescription;
    }

    [Header("UI элементы")]
    [SerializeField] private GameObject hqBadgeRoot;
    [SerializeField] private TMP_Text hqLocationTxt;
    [SerializeField] private TMP_Text hqBonusTxt;
    [SerializeField] private Image hqBadgeBg;
    [SerializeField] private Image ambientTintImage;

    [Header("Эпохи Студии")]
    [SerializeField] private List<StudioEra> eras = new List<StudioEra>();

    private int currentEraIndex = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        InitErasList();
    }

    private void Start()
    {
        if (hqBadgeRoot == null)
        {
            BuildUI();
        }

        UpdateCurrentEra(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged += OnCurrenciesChanged;
            GameManager.Instance.OnPrestigeCompleted += OnPrestige;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= OnCurrenciesChanged;
            GameManager.Instance.OnPrestigeCompleted -= OnPrestige;
        }
    }

    private void InitErasList()
    {
        eras = new List<StudioEra>
        {
            new StudioEra
            {
                eraIndex = 0,
                eraName = "Гаражный Старт",
                locationTitle = "📦 Студенческий Гараж",
                icon = "📦",
                ambientColor = new Color(0.10f, 0.12f, 0.18f, 0.20f),
                badgeColor = new Color(0.18f, 0.22f, 0.32f, 0.90f),
                reqTotalCode = 0,
                reqPrestige = 0,
                bonusDescription = "Теплый чайник и лампа дебага"
            },
            new StudioEra
            {
                eraIndex = 1,
                eraName = "Коворкинг IT-Парка",
                locationTitle = "🏢 Коворкинг IT-Парка",
                icon = "🏢",
                ambientColor = new Color(0.08f, 0.18f, 0.24f, 0.25f),
                badgeColor = new Color(0.12f, 0.35f, 0.45f, 0.90f),
                reqTotalCode = 5000,
                reqPrestige = 0,
                bonusDescription = "+10% к силе клика"
            },
            new StudioEra
            {
                eraIndex = 2,
                eraName = "Silicon Tower Пентхаус",
                locationTitle = "🌆 Небоскреб Silicon Tower",
                icon = "🌆",
                ambientColor = new Color(0.15f, 0.10f, 0.25f, 0.28f),
                badgeColor = new Color(0.35f, 0.18f, 0.55f, 0.90f),
                reqTotalCode = 30000,
                reqPrestige = 0,
                bonusDescription = "+20% к доходу студии"
            },
            new StudioEra
            {
                eraIndex = 3,
                eraName = "Квантовый Технополис",
                locationTitle = "⚛️ Квантовый Технополис",
                icon = "⚛️",
                ambientColor = new Color(0.06f, 0.16f, 0.28f, 0.32f),
                badgeColor = new Color(0.12f, 0.45f, 0.85f, 0.92f),
                reqTotalCode = 100000,
                reqPrestige = 0,
                bonusDescription = "+30% к скорости C#"
            },
            new StudioEra
            {
                eraIndex = 4,
                eraName = "Марсианская Цитадель",
                locationTitle = "🔴 Марсианский Биокупол Олимп",
                icon = "🔴",
                ambientColor = new Color(0.24f, 0.08f, 0.04f, 0.35f),
                badgeColor = new Color(0.75f, 0.25f, 0.12f, 0.95f),
                reqTotalCode = 200000,
                reqPrestige = 1,
                bonusDescription = "Межпланетный продакшен x1.50"
            }
        };
    }

    private void OnCurrenciesChanged()
    {
        UpdateCurrentEra(false);
    }

    private void OnPrestige(int newPrestige)
    {
        UpdateCurrentEra(false);
    }

    public void UpdateCurrentEra(bool force)
    {
        double code = GameManager.Instance != null ? GameManager.Instance.TotalCodeWritten : 0;
        int prestige = GameManager.Instance != null ? GameManager.Instance.PrestigeLevel : 0;

        int targetEra = 0;
        for (int i = eras.Count - 1; i >= 0; i--)
        {
            if (code >= eras[i].reqTotalCode && prestige >= eras[i].reqPrestige)
            {
                targetEra = i;
                break;
            }
        }

        if (targetEra != currentEraIndex || force)
        {
            int oldEra = currentEraIndex;
            currentEraIndex = targetEra;

            ApplyEraVisuals(eras[currentEraIndex]);

            if (!force && targetEra > oldEra)
            {
                HapticFeedback.HeavyImpact();
                if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();
                if (ClickJuice.Instance != null)
                {
                    ClickJuice.Instance.SpawnCustomPopup($"🎉 ПЕРЕЕЗД СТУДИИ!\nНовая локация: <b>{eras[currentEraIndex].locationTitle}</b>\n{eras[currentEraIndex].bonusDescription}", transform.position, new Color(0.3f, 0.95f, 1f), true);
                }
            }
        }
    }

    private void ApplyEraVisuals(StudioEra era)
    {
        if (hqLocationTxt != null)
        {
            hqLocationTxt.text = era.locationTitle;
        }

        if (hqBonusTxt != null)
        {
            hqBonusTxt.text = era.bonusDescription;
        }

        if (hqBadgeBg != null)
        {
            hqBadgeBg.color = era.badgeColor;
        }

        if (ambientTintImage != null)
        {
            ambientTintImage.color = era.ambientColor;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Ambient Tint
        GameObject ambientObj = new GameObject("OfficeAmbientTint", typeof(RectTransform), typeof(Image));
        ambientObj.transform.SetParent(canvas.transform, false);
        ambientObj.transform.SetAsFirstSibling();
        RectTransform ar = ambientObj.GetComponent<RectTransform>();
        ar.anchorMin = Vector2.zero;
        ar.anchorMax = Vector2.one;
        ar.offsetMin = Vector2.zero;
        ar.offsetMax = Vector2.zero;
        ambientTintImage = ambientObj.GetComponent<Image>();
        ambientTintImage.raycastTarget = false;
        ambientTintImage.color = eras[0].ambientColor;

        // HQ Badge (Верхняя центральная область под топбаром)
        hqBadgeRoot = new GameObject("HQLocationBadge", typeof(RectTransform), typeof(Image));
        hqBadgeRoot.transform.SetParent(canvas.transform, false);
        RectTransform br = hqBadgeRoot.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 1f);
        br.anchorMax = new Vector2(0.5f, 1f);
        br.pivot = new Vector2(0.5f, 1f);
        br.sizeDelta = new Vector2(240, 36);
        br.anchoredPosition = new Vector2(0, -96);
        hqBadgeBg = hqBadgeRoot.GetComponent<Image>();
        hqBadgeBg.color = eras[0].badgeColor;

        GameObject locTxtObj = new GameObject("LocationTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        locTxtObj.transform.SetParent(hqBadgeRoot.transform, false);
        hqLocationTxt = locTxtObj.GetComponent<TextMeshProUGUI>();
        hqLocationTxt.text = eras[0].locationTitle;
        hqLocationTxt.fontSize = 12;
        hqLocationTxt.fontStyle = FontStyles.Bold;
        hqLocationTxt.alignment = TextAlignmentOptions.Center;
        hqLocationTxt.color = Color.white;
        RectTransform lr = locTxtObj.GetComponent<RectTransform>();
        lr.anchorMin = new Vector2(0f, 0.4f);
        lr.anchorMax = new Vector2(1f, 1f);
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = Vector2.zero;

        GameObject bonTxtObj = new GameObject("BonusTxt", typeof(RectTransform), typeof(TextMeshProUGUI));
        bonTxtObj.transform.SetParent(hqBadgeRoot.transform, false);
        hqBonusTxt = bonTxtObj.GetComponent<TextMeshProUGUI>();
        hqBonusTxt.text = eras[0].bonusDescription;
        hqBonusTxt.fontSize = 9;
        hqBonusTxt.alignment = TextAlignmentOptions.Center;
        hqBonusTxt.color = new Color(0.85f, 0.95f, 1f);
        RectTransform bnr = bonTxtObj.GetComponent<RectTransform>();
        bnr.anchorMin = new Vector2(0f, 0f);
        bnr.anchorMax = new Vector2(1f, 0.45f);
        bnr.offsetMin = Vector2.zero;
        bnr.offsetMax = Vector2.zero;
    }
}
