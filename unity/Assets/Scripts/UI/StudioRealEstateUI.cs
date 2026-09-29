using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Расширение офиса и недвижимость студии (Studio Real Estate):
/// - 4 уровня офисов: Родительский гараж, Инди-коворкинг, Лофт в Москва-Сити, IT-Кампус
/// - Увеличение лимита серверных лезвий (от 1 до 4)
/// - Увеличение лимита найма персонала и автоматизации
/// - Глобальный мультипликатор дохода компании (от x1.0 до x2.5)
/// - Визуальная трансформация окружения и атмосферы
/// </summary>
public class StudioRealEstateUI : MonoBehaviour
{
    private static StudioRealEstateUI instance;
    public static StudioRealEstateUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<StudioRealEstateUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class OfficeTier
    {
        public string id;
        public string title;
        public string subtitle;
        public string icon;
        public double price;
        public double incomeMultiplier;
        public int maxServerBlades;
        public int maxStaffHires;
        public Color themeColor;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openRealEstateBtn;
    [SerializeField] private TMP_Text openRealEstateBtnText;
    [SerializeField] private Button closeRealEstateBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Контейнер списка офисов")]
    [SerializeField] private Transform officeListContainer;

    [Header("Информационная плашка текущего офиса")]
    [SerializeField] private TMP_Text currentOfficeTitleText;
    [SerializeField] private TMP_Text currentOfficePerksText;

    private readonly List<OfficeTier> tiers = new List<OfficeTier>();
    private int currentTierIndex = 0;
    private readonly List<int> purchasedTiers = new List<int>();

    private const string PrefCurrentTier = "Studio_RealEstateTier";
    private const string PrefPurchasedPrefix = "Studio_RealEstatePurchased_";

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public int CurrentTierIndex => currentTierIndex;
    public IReadOnlyList<OfficeTier> Tiers => tiers;

    public event Action<int> OnOfficeChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeTiers();
        LoadData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        ApplyOfficeVisuals(false);
    }

    private void InitializeTiers()
    {
        tiers.Clear();

        tiers.Add(new OfficeTier
        {
            id = "office_garage",
            title = "Гараж инди-разработчика",
            subtitle = "Старые коробки, самодельный стол, запах кофе и мечты о первом релизе.",
            icon = "📦",
            price = 0.0,
            incomeMultiplier = 1.0,
            maxServerBlades = 1,
            maxStaffHires = 2,
            themeColor = new Color(0.7f, 0.7f, 0.75f)
        });

        tiers.Add(new OfficeTier
        {
            id = "office_coworking",
            title = "Коворкинг 'Креативный Хаб'",
            subtitle = "Безлимитный кофе, соседи-стартаперы, стеклянные перегородки и скоростной Wi-Fi.",
            icon = "☕",
            price = 50000.0,
            incomeMultiplier = 1.3,
            maxServerBlades = 2,
            maxStaffHires = 6,
            themeColor = new Color(0.2f, 0.85f, 0.6f)
        });

        tiers.Add(new OfficeTier
        {
            id = "office_loft",
            title = "Лофт в Небоскрёбе (Москва-Сити)",
            subtitle = "Панорамный вид с 64 этажа на ночной мегаполис, кресла Herman Miller и серверная стойка.",
            icon = "🌆",
            price = 750000.0,
            incomeMultiplier = 1.8,
            maxServerBlades = 3,
            maxStaffHires = 16,
            themeColor = new Color(0.3f, 0.75f, 1f)
        });

        tiers.Add(new OfficeTier
        {
            id = "office_campus",
            title = "Технопарк 'HelloTap Campus'",
            subtitle = "Собственное 5-этажное здание, столовая с мишленовскими шефами, VR-лаборатория и дата-центр.",
            icon = "🏛️",
            price = 5000000.0,
            incomeMultiplier = 2.5,
            maxServerBlades = 4,
            maxStaffHires = 50,
            themeColor = new Color(1f, 0.84f, 0.2f)
        });
    }

    private void LoadData()
    {
        currentTierIndex = PlayerPrefs.GetInt(PrefCurrentTier, 0);
        currentTierIndex = Mathf.Clamp(currentTierIndex, 0, tiers.Count - 1);

        purchasedTiers.Clear();
        purchasedTiers.Add(0); // Базовый гараж всегда куплен

        for (int i = 1; i < tiers.Count; i++)
        {
            if (PlayerPrefs.GetInt(PrefPurchasedPrefix + i, 0) == 1)
            {
                purchasedTiers.Add(i);
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(PrefCurrentTier, currentTierIndex);
        foreach (int t in purchasedTiers)
        {
            PlayerPrefs.SetInt(PrefPurchasedPrefix + t, 1);
        }
        PlayerPrefs.Save();
    }

    public double GetIncomeMultiplier()
    {
        if (currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            return tiers[currentTierIndex].incomeMultiplier;
        }
        return 1.0;
    }

    public int GetMaxServerBlades()
    {
        if (currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            return tiers[currentTierIndex].maxServerBlades;
        }
        return 1;
    }

    public int GetMaxStaffHires()
    {
        if (currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            return tiers[currentTierIndex].maxStaffHires;
        }
        return 2;
    }

    public string GetCurrentOfficeName()
    {
        if (currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            return tiers[currentTierIndex].title;
        }
        return "Офис";
    }

    public bool IsTierPurchased(int tierIndex)
    {
        return tierIndex == 0 || purchasedTiers.Contains(tierIndex);
    }

    public bool TryBuyOrRelocate(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex >= tiers.Count) return false;
        var tier = tiers[tierIndex];

        if (currentTierIndex == tierIndex)
        {
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🏢 Студия уже находится здесь!", transform.position, Color.yellow, false);
            }
            return false;
        }

        if (IsTierPurchased(tierIndex))
        {
            // Уже куплен - просто переезжаем
            currentTierIndex = tierIndex;
            SaveData();
            OnRelocatedSuccess(tier, false);
            return true;
        }

        // Покупка нового офиса
        if (GameManager.Instance == null || GameManager.Instance.Money < tier.price)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"❌ Недостаточно средств! Нужно {NumberFormatter.Format(tier.price)} ₽", transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
            return false;
        }

        GameManager.Instance.SpendMoney(tier.price);
        purchasedTiers.Add(tierIndex);
        currentTierIndex = tierIndex;
        SaveData();

        OnRelocatedSuccess(tier, true);
        return true;
    }

    private void OnRelocatedSuccess(OfficeTier tier, bool isNewPurchase)
    {
        ApplyOfficeVisuals(true);
        OnOfficeChanged?.Invoke(currentTierIndex);

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        string actionText = isNewPurchase ? "🎉 ПРИОБРЕТЕН НОВЫЙ ОФИС!" : "🚚 ПЕРЕЕЗД ВЫПОЛНЕН!";
        if (ClickJuice.Instance != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"{actionText}\n{tier.icon} {tier.title}\nДоход x{tier.incomeMultiplier:F1} | Серверов: {tier.maxServerBlades}", transform.position, tier.themeColor, true);
        }

        UpdateModalUI();
    }

    private void ApplyOfficeVisuals(bool animate)
    {
        // Влияние на фоновые обои и окружение рабочего места
        var visuals = FindFirstObjectByType<WorkplaceVisuals>();
        if (visuals != null)
        {
            // Обновляем заголовок на неоновой табличке или статус
            if (openRealEstateBtnText != null)
            {
                openRealEstateBtnText.text = $"🏢 {tiers[currentTierIndex].title}";
            }
        }
    }

    private void BindButtons()
    {
        if (openRealEstateBtn != null)
        {
            openRealEstateBtn.onClick.RemoveAllListeners();
            openRealEstateBtn.onClick.AddListener(OpenModal);
        }
        if (closeRealEstateBtn != null)
        {
            closeRealEstateBtn.onClick.RemoveAllListeners();
            closeRealEstateBtn.onClick.AddListener(CloseModal);
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

    public void OpenModal()
    {
        EnsureUIExists();
        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.88f, 0.88f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }
        }
        UpdateModalUI();
        HapticFeedback.Vibrate(20);
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
            float scale = Mathf.Lerp(0.88f, 1f, Mathf.Sin(t * Mathf.PI * 0.5f));
            if (card != null) card.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }
        if (card != null) card.localScale = Vector3.one;
    }

    private void UpdateModalUI()
    {
        if (currentOfficeTitleText != null && currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            var cur = tiers[currentTierIndex];
            currentOfficeTitleText.text = $"{cur.icon} <color=#{ColorUtility.ToHtmlStringRGB(cur.themeColor)}>{cur.title}</color>";
        }

        if (currentOfficePerksText != null && currentTierIndex >= 0 && currentTierIndex < tiers.Count)
        {
            var cur = tiers[currentTierIndex];
            currentOfficePerksText.text = $"Мультипликатор дохода: <b>x{cur.incomeMultiplier:F1}</b> | Серверная стойка: <b>{cur.maxServerBlades}/4 лезвий</b> | Персонал: <b>до {cur.maxStaffHires} чел.</b>";
        }

        RefreshOfficeCards();
    }

    private void RefreshOfficeCards()
    {
        if (officeListContainer == null) return;

        // Обновляем каждую карточку офиса
        for (int i = 0; i < tiers.Count; i++)
        {
            int tierIdx = i;
            var tier = tiers[i];
            Transform cardTr = officeListContainer.Find($"OfficeCard_{tierIdx}");
            if (cardTr == null) continue;

            Button actionBtn = cardTr.Find("ActionBtn")?.GetComponent<Button>();
            TMP_Text actionBtnText = actionBtn != null ? actionBtn.GetComponentInChildren<TMP_Text>() : null;
            Image cardBg = cardTr.GetComponent<Image>();

            bool isCurrent = currentTierIndex == tierIdx;
            bool isPurchased = IsTierPurchased(tierIdx);

            if (cardBg != null)
            {
                cardBg.color = isCurrent 
                    ? new Color(0.12f, 0.22f, 0.32f, 0.95f) 
                    : new Color(0.08f, 0.10f, 0.15f, 0.92f);
            }

            if (actionBtn != null && actionBtnText != null)
            {
                actionBtn.onClick.RemoveAllListeners();
                actionBtn.onClick.AddListener(() => TryBuyOrRelocate(tierIdx));

                if (isCurrent)
                {
                    actionBtn.interactable = false;
                    actionBtnText.text = "⭐ ТЕКУЩИЙ ОФИС";
                }
                else if (isPurchased)
                {
                    actionBtn.interactable = true;
                    actionBtnText.text = "🚚 ПЕРЕЕХАТЬ СЮДА";
                }
                else
                {
                    bool canAfford = GameManager.Instance != null && GameManager.Instance.Money >= tier.price;
                    actionBtn.interactable = canAfford;
                    actionBtnText.text = $"КУПИТЬ ЗА {NumberFormatter.Format(tier.price)} ₽";
                }
            }
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Создаем корневой объект модального окна
        GameObject root = new GameObject("StudioRealEstateModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        // Затемняющий фон (Backdrop)
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.78f);
        backdropBtn = bgObj.GetComponent<Button>();

        // Карточка модалки
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(480, 680);
        Image cardImg = cardObj.GetComponent<Image>();
        cardImg.color = new Color(0.06f, 0.08f, 0.12f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.85f, 1f, 0.45f);
        outline.effectDistance = new Vector2(2, -2);

        // Заголовок
        GameObject headerObj = new GameObject("Header", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(cardObj.transform, false);
        RectTransform headerRt = headerObj.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0, 1);
        headerRt.anchorMax = new Vector2(1, 1);
        headerRt.pivot = new Vector2(0.5f, 1);
        headerRt.anchoredPosition = new Vector2(0, -18);
        headerRt.sizeDelta = new Vector2(-40, 40);
        TMP_Text headerText = headerObj.GetComponent<TextMeshProUGUI>();
        headerText.text = "🏢 НЕДВИЖИМОСТЬ СТУДИИ";
        headerText.fontSize = 22;
        headerText.fontStyle = FontStyles.Bold;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = new Color(0f, 0.95f, 1f);

        // Кнопка закрытия [X]
        GameObject closeXObj = new GameObject("CloseXBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeXObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeXRt = closeXObj.GetComponent<RectTransform>();
        closeXRt.anchorMin = new Vector2(1, 1);
        closeXRt.anchorMax = new Vector2(1, 1);
        closeXRt.anchoredPosition = new Vector2(-28, -28);
        closeXRt.sizeDelta = new Vector2(36, 36);
        closeXObj.GetComponent<Image>().color = new Color(0.3f, 0.1f, 0.15f, 0.8f);
        closeXBtn = closeXObj.GetComponent<Button>();

        GameObject closeXTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeXTxtObj.transform.SetParent(closeXObj.transform, false);
        TMP_Text closeXTxt = closeXTxtObj.GetComponent<TextMeshProUGUI>();
        closeXTxt.text = "✕";
        closeXTxt.fontSize = 18;
        closeXTxt.alignment = TextAlignmentOptions.Center;
        closeXTxt.color = Color.white;

        // Панель текущего статуса
        GameObject statusPanel = new GameObject("CurrentStatusPanel", typeof(RectTransform), typeof(Image));
        statusPanel.transform.SetParent(cardObj.transform, false);
        RectTransform statusRt = statusPanel.GetComponent<RectTransform>();
        statusRt.anchorMin = new Vector2(0, 1);
        statusRt.anchorMax = new Vector2(1, 1);
        statusRt.pivot = new Vector2(0.5f, 1);
        statusRt.anchoredPosition = new Vector2(0, -65);
        statusRt.sizeDelta = new Vector2(-40, 68);
        statusPanel.GetComponent<Image>().color = new Color(0.1f, 0.14f, 0.20f, 0.9f);

        GameObject curTitleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        curTitleObj.transform.SetParent(statusPanel.transform, false);
        RectTransform curTitleRt = curTitleObj.GetComponent<RectTransform>();
        curTitleRt.anchorMin = new Vector2(0, 0.5f);
        curTitleRt.anchorMax = new Vector2(1, 1);
        curTitleRt.offsetMin = new Vector2(12, 0);
        curTitleRt.offsetMax = new Vector2(-12, -4);
        currentOfficeTitleText = curTitleObj.GetComponent<TextMeshProUGUI>();
        currentOfficeTitleText.fontSize = 16;
        currentOfficeTitleText.fontStyle = FontStyles.Bold;

        GameObject curPerksObj = new GameObject("Perks", typeof(RectTransform), typeof(TextMeshProUGUI));
        curPerksObj.transform.SetParent(statusPanel.transform, false);
        RectTransform curPerksRt = curPerksObj.GetComponent<RectTransform>();
        curPerksRt.anchorMin = new Vector2(0, 0);
        curPerksRt.anchorMax = new Vector2(1, 0.5f);
        curPerksRt.offsetMin = new Vector2(12, 4);
        curPerksRt.offsetMax = new Vector2(-12, 0);
        currentOfficePerksText = curPerksObj.GetComponent<TextMeshProUGUI>();
        currentOfficePerksText.fontSize = 12;
        currentOfficePerksText.color = new Color(0.75f, 0.85f, 0.95f);

        // Список карточек офисов
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(20, 70);
        scrollRt.offsetMax = new Vector2(-20, -145);
        scrollObj.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.5f);

        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(scrollObj.transform, false);
        officeListContainer = contentObj.transform;
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 0);

        var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        var csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = scrollObj.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.horizontal = false;
        sr.vertical = true;

        // Создаем шаблонные карточки для всех 4 уровней
        for (int i = 0; i < tiers.Count; i++)
        {
            CreateOfficeCardTemplate(officeListContainer, tiers[i], i);
        }

        // Кнопка Закрыть внизу
        GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0, 0);
        closeBtnRt.anchorMax = new Vector2(1, 0);
        closeBtnRt.pivot = new Vector2(0.5f, 0);
        closeBtnRt.anchoredPosition = new Vector2(0, 16);
        closeBtnRt.sizeDelta = new Vector2(-40, 42);
        closeBtnObj.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.95f);
        closeRealEstateBtn = closeBtnObj.GetComponent<Button>();

        GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero;
        closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.sizeDelta = Vector2.zero;
        TMP_Text closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTxt.text = "ЗАКРЫТЬ";
        closeTxt.fontSize = 15;
        closeTxt.fontStyle = FontStyles.Bold;
        closeTxt.alignment = TextAlignmentOptions.Center;
        closeTxt.color = Color.white;

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private void CreateOfficeCardTemplate(Transform parent, OfficeTier tier, int index)
    {
        GameObject card = new GameObject($"OfficeCard_{index}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform cardRt = card.GetComponent<RectTransform>();
        cardRt.sizeDelta = new Vector2(430, 105);
        card.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.92f);

        // Иконка и Название
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(card.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0, 1);
        titleRt.anchoredPosition = new Vector2(12, -8);
        titleRt.sizeDelta = new Vector2(-24, 22);
        TMP_Text titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = $"{tier.icon} <color=#{ColorUtility.ToHtmlStringRGB(tier.themeColor)}>{tier.title}</color>";
        titleTxt.fontSize = 15;
        titleTxt.fontStyle = FontStyles.Bold;

        // Описание / Subtitle
        GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        descObj.transform.SetParent(card.transform, false);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 1);
        descRt.anchorMax = new Vector2(1, 1);
        descRt.pivot = new Vector2(0, 1);
        descRt.anchoredPosition = new Vector2(12, -30);
        descRt.sizeDelta = new Vector2(-24, 30);
        TMP_Text descTxt = descObj.GetComponent<TextMeshProUGUI>();
        descTxt.text = tier.subtitle;
        descTxt.fontSize = 11;
        descTxt.color = new Color(0.7f, 0.75f, 0.85f);

        // Характеристики
        GameObject statsObj = new GameObject("Stats", typeof(RectTransform), typeof(TextMeshProUGUI));
        statsObj.transform.SetParent(card.transform, false);
        RectTransform statsRt = statsObj.GetComponent<RectTransform>();
        statsRt.anchorMin = new Vector2(0, 0);
        statsRt.anchorMax = new Vector2(0.65f, 0);
        statsRt.pivot = new Vector2(0, 0);
        statsRt.anchoredPosition = new Vector2(12, 8);
        statsRt.sizeDelta = new Vector2(0, 24);
        TMP_Text statsTxt = statsObj.GetComponent<TextMeshProUGUI>();
        statsTxt.text = $"⚡ x{tier.incomeMultiplier:F1} | 🖥️ {tier.maxServerBlades}/4 | 👥 {tier.maxStaffHires}";
        statsTxt.fontSize = 12;
        statsTxt.fontStyle = FontStyles.Bold;
        statsTxt.color = new Color(0.2f, 1f, 0.6f);

        // Кнопка Действия
        GameObject actBtnObj = new GameObject("ActionBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        actBtnObj.transform.SetParent(card.transform, false);
        RectTransform actBtnRt = actBtnObj.GetComponent<RectTransform>();
        actBtnRt.anchorMin = new Vector2(1, 0);
        actBtnRt.anchorMax = new Vector2(1, 0);
        actBtnRt.pivot = new Vector2(1, 0);
        actBtnRt.anchoredPosition = new Vector2(-10, 8);
        actBtnRt.sizeDelta = new Vector2(170, 30);
        actBtnObj.GetComponent<Image>().color = new Color(0f, 0.6f, 0.85f);

        GameObject actTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        actTxtObj.transform.SetParent(actBtnObj.transform, false);
        RectTransform actTxtRt = actTxtObj.GetComponent<RectTransform>();
        actTxtRt.anchorMin = Vector2.zero;
        actTxtRt.anchorMax = Vector2.one;
        actTxtRt.sizeDelta = Vector2.zero;
        TMP_Text actTxt = actTxtObj.GetComponent<TextMeshProUGUI>();
        actTxt.text = index == 0 ? "ТЕКУЩИЙ ОФИС" : $"КУПИТЬ ЗА {NumberFormatter.Format(tier.price)} ₽";
        actTxt.fontSize = 11;
        actTxt.fontStyle = FontStyles.Bold;
        actTxt.alignment = TextAlignmentOptions.Center;
        actTxt.color = Color.white;
    }
}
