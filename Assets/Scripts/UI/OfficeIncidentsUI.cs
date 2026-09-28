using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Случайные офисные происшествия (Office Random Incidents):
/// - Периодические случайные события во время разработки (каждые 90-150 сек)
/// - Дилеммы с выбором решений (рискованный, затратный, креативный)
/// - Влияние на баланс рублей, написанные строки кода, баффы скорости и охлаждение серверов
/// - Попап-окно с атмосферными описаниями и тактильным откликом
/// </summary>
public class OfficeIncidentsUI : MonoBehaviour
{
    private static OfficeIncidentsUI instance;
    public static OfficeIncidentsUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<OfficeIncidentsUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class IncidentChoice
    {
        public string choiceText;
        public string outcomeDesc;
        public double moneyDelta;
        public double codeDelta;
        public float boostSeconds;
        public float boostMultiplier;
        public float rackCoolingAmount;
        public float successChance; // 1.0 = гарантированно
    }

    [System.Serializable]
    public class OfficeIncident
    {
        public string id;
        public string icon;
        public string title;
        public string storyText;
        public IncidentChoice choiceA;
        public IncidentChoice choiceB;
    }

    [Header("Модальное окно инцидента")]
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;
    [SerializeField] private TMP_Text incidentIconText;
    [SerializeField] private TMP_Text incidentTitleText;
    [SerializeField] private TMP_Text incidentStoryText;
    [SerializeField] private Button choiceABtn;
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private Button choiceBBtn;
    [SerializeField] private TMP_Text choiceBText;

    private readonly List<OfficeIncident> incidentPool = new List<OfficeIncident>();
    private float nextIncidentTimer = 90f;
    private OfficeIncident activeIncident = null;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializePool();
        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        EnsureUIExists();
        BindButtons();
        nextIncidentTimer = UnityEngine.Random.Range(70f, 130f);
    }

    private void Update()
    {
        if (IsModalOpen) return;

        nextIncidentTimer -= Time.deltaTime;
        if (nextIncidentTimer <= 0f)
        {
            nextIncidentTimer = UnityEngine.Random.Range(90f, 160f);
            TriggerRandomIncident();
        }
    }

    private void InitializePool()
    {
        incidentPool.Clear();

        // 1. Стажер и master
        incidentPool.Add(new OfficeIncident
        {
            id = "inc_git_master",
            icon = "💥",
            title = "Стажёр запушил в master в пятницу!",
            storyText = "В 18:05 в репозиторий прилетел коммит 'fix all bugs', ломающий прод и половину игровых механик. Что делаем?",
            choiceA = new IncidentChoice
            {
                choiceText = "⚡ Срочный откат и хотфикс (-1500 строк, бафф x1.5 на 45с)",
                outcomeDesc = "Билд спасен ценой бессонной ночи! Команда на кураже строчит код с удвоенной скоростью.",
                codeDelta = -1500.0,
                boostSeconds = 45f,
                boostMultiplier = 1.5f,
                successChance = 1.0f
            },
            choiceB = new IncidentChoice
            {
                choiceText = "🎲 Оставить как есть и молиться (Шанс хайпа или штрафа)",
                outcomeDesc = "Случайный баг превратился в вирусную фичу! Игроки постят мемы, а продажи подскочили!",
                moneyDelta = 12000.0,
                codeDelta = 2000.0,
                successChance = 0.65f
            }
        });

        // 2. Кофемашина
        incidentPool.Add(new OfficeIncident
        {
            id = "inc_coffee_broken",
            icon = "☕",
            title = "Сломалась офисная кофемашина!",
            storyText = "Жизненно важный генератор бодрости издал прощальный писк и пустил пар. Глаза разработчиков слипаются.",
            choiceA = new IncidentChoice
            {
                choiceText = "🛵 Заказать спешелти раф курьером (-2000 ₽, МЕГА-БУСТ x2.0 на 60с)",
                outcomeDesc = "Двойной эспрессо разогнал пульс разработчиков до максимума! Пальцы летают по клавишам!",
                moneyDelta = -2000.0,
                boostSeconds = 60f,
                boostMultiplier = 2.0f,
                successChance = 1.0f
            },
            choiceB = new IncidentChoice
            {
                choiceText = "🍵 Заварить зеленый чай (Бесплатно, легкий дзен +1000 строк)",
                outcomeDesc = "Спокойное чаепитие привело мысли в порядок. Написана аккуратная и чистая архитектура.",
                codeDelta = 1000.0,
                successChance = 1.0f
            }
        });

        // 3. Кот на клавиатуре
        incidentPool.Add(new OfficeIncident
        {
            id = "inc_cat_keyboard",
            icon = "🐱",
            title = "Офисный кот заснул на клавиатуре!",
            storyText = "Пушистый архитектор лег поперек клавиш, зажав пробел и случайно сгенерировав 3 страницы кода.",
            choiceA = new IncidentChoice
            {
                choiceText = "💤 Не беспокоить пушистика (+3500 строк сгенерировано котиком)",
                outcomeDesc = "Кошачий код чудесным образом скомпилировался без ошибок! Преподаватель и игроки в восторге.",
                codeDelta = 3500.0,
                successChance = 1.0f
            },
            choiceB = new IncidentChoice
            {
                choiceText = "✋ Аккуратно переложить (+1500 ₽ найдено под столом)",
                outcomeDesc = "Под клавиатурой обнаружилась забытая заначка на обед! Кот довольно замурчал на плече.",
                moneyDelta = 1500.0,
                successChance = 1.0f
            }
        });

        // 4. DDOS конкурентов
        incidentPool.Add(new OfficeIncident
        {
            id = "inc_ddos_attack",
            icon = "🛡️",
            title = "Серверная под DDoS-атакой конкурентов!",
            storyText = "Графики нагрузки взлетели в космос, вентиляторы серверной стойки завыли на максимальных оборотах.",
            choiceA = new IncidentChoice
            {
                choiceText = "🔒 Врубить Cloudflare защиту (-4000 ₽, охлаждение стойки до нормы)",
                outcomeDesc = "Атака отбита на внешнем периметре! Сервера мгновенно остыли, а майнинг DevCoin стабилен.",
                moneyDelta = -4000.0,
                rackCoolingAmount = 35f,
                successChance = 1.0f
            },
            choiceB = new IncidentChoice
            {
                choiceText = "⚔️ Отразить атаку через iptables (+15000 ₽ репутация, +5000 строк)",
                outcomeDesc = "Вы лично вычислили ботнет и перенаправили трафик! Новость разлетелась по Хабру!",
                moneyDelta = 15000.0,
                codeDelta = 5000.0,
                successChance = 0.75f
            }
        });

        // 5. Топ-стример
        incidentPool.Add(new OfficeIncident
        {
            id = "inc_streamer_hype",
            icon = "🎥",
            title = "Популярный стример запустил вашу игру!",
            storyText = "Стример с аудиторией 80 000 зрителей в прямом эфире смеется над физикой и хвалит геймплей!",
            choiceA = new IncidentChoice
            {
                choiceText = "📢 Включить скидочную акцию (+35 000 ₽ мгновенных покупок)",
                outcomeDesc = "Чат ломанулся раскупать игру! Касса студии трещит от входящих донатов и покупок!",
                moneyDelta = 35000.0,
                successChance = 1.0f
            },
            choiceB = new IncidentChoice
            {
                choiceText = "🎁 Задонатить стримеру со словами 'Спасибо от автора!' (-5000 ₽, +80 000 ₽)",
                outcomeDesc = "Стример прикрепил ссылку в шапку стрима и устроил марафон прохождения! Грандиозный триумф!",
                moneyDelta = 75000.0,
                boostSeconds = 90f,
                boostMultiplier = 1.8f,
                successChance = 0.85f
            }
        });
    }

    public void TriggerRandomIncident()
    {
        if (incidentPool.Count == 0) return;
        var inc = incidentPool[UnityEngine.Random.Range(0, incidentPool.Count)];
        ShowIncident(inc);
    }

    public void ShowIncident(OfficeIncident incident)
    {
        EnsureUIExists();
        activeIncident = incident;

        if (incidentIconText != null) incidentIconText.text = incident.icon;
        if (incidentTitleText != null) incidentTitleText.text = incident.title;
        if (incidentStoryText != null) incidentStoryText.text = incident.storyText;

        if (choiceAText != null) choiceAText.text = incident.choiceA.choiceText;
        if (choiceBText != null) choiceBText.text = incident.choiceB.choiceText;

        if (modalRoot != null)
        {
            modalRoot.SetActive(true);
            if (modalCardTransform != null)
            {
                modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
                StartCoroutine(PopCardAnim(modalCardTransform));
            }
        }

        HapticFeedback.NotificationPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRushAlert();
    }

    public void ChooseOption(bool isA)
    {
        if (activeIncident == null) return;
        var choice = isA ? activeIncident.choiceA : activeIncident.choiceB;

        bool isSuccess = UnityEngine.Random.value <= choice.successChance;
        string resultText = choice.outcomeDesc;

        if (!isSuccess)
        {
            resultText = "⚠️ План пошел не так гладко, как ожидалось, но студия получила бесценный опыт!";
        }

        if (GameManager.Instance != null)
        {
            if (choice.moneyDelta != 0) GameManager.Instance.AddMoney(choice.moneyDelta);
            if (choice.codeDelta != 0) GameManager.Instance.AddLinesOfCode(choice.codeDelta);
            if (choice.boostSeconds > 0) GameManager.Instance.ActivateEnergyBoost(choice.boostSeconds, choice.boostMultiplier);
        }

        if (choice.rackCoolingAmount > 0 && ServerRackUI.Instance != null)
        {
            ServerRackUI.Instance.OnEmergencyCoolingClicked();
        }

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null)
        {
            Color col = isSuccess ? new Color(0.2f, 1f, 0.6f) : new Color(1f, 0.5f, 0.2f);
            ClickJuice.Instance.SpawnCustomPopup($"📋 ИТОГ ИНЦИДЕНТА:\n{resultText}", transform.position, col, true);
        }

        CloseModal();
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        activeIncident = null;
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

    private void BindButtons()
    {
        if (choiceABtn != null)
        {
            choiceABtn.onClick.RemoveAllListeners();
            choiceABtn.onClick.AddListener(() => ChooseOption(true));
        }

        if (choiceBBtn != null)
        {
            choiceBBtn.onClick.RemoveAllListeners();
            choiceBBtn.onClick.AddListener(() => ChooseOption(false));
        }
    }

    private void EnsureUIExists()
    {
        if (modalRoot != null) return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = new GameObject("OfficeIncidentsModal", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;

        // Backdrop
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(root.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.82f);

        // Card
        GameObject cardObj = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        cardObj.transform.SetParent(root.transform, false);
        modalCardTransform = cardObj.transform;
        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(460, 460);
        cardObj.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.16f, 0.98f);
        var outline = cardObj.GetComponent<Outline>();
        outline.effectColor = new Color(1f, 0.5f, 0.1f, 0.6f);
        outline.effectDistance = new Vector2(2, -2);

        // Icon
        GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(TextMeshProUGUI));
        iconObj.transform.SetParent(cardObj.transform, false);
        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 1);
        iconRt.anchorMax = new Vector2(0.5f, 1);
        iconRt.pivot = new Vector2(0.5f, 1);
        iconRt.anchoredPosition = new Vector2(0, -18);
        iconRt.sizeDelta = new Vector2(60, 50);
        incidentIconText = iconObj.GetComponent<TextMeshProUGUI>();
        incidentIconText.fontSize = 38;
        incidentIconText.alignment = TextAlignmentOptions.Center;
        incidentIconText.text = "⚡";

        // Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -70);
        titleRt.sizeDelta = new Vector2(-40, 36);
        incidentTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        incidentTitleText.fontSize = 17;
        incidentTitleText.fontStyle = FontStyles.Bold;
        incidentTitleText.alignment = TextAlignmentOptions.Center;
        incidentTitleText.color = new Color(1f, 0.85f, 0.3f);

        // Story Text
        GameObject storyObj = new GameObject("Story", typeof(RectTransform), typeof(TextMeshProUGUI));
        storyObj.transform.SetParent(cardObj.transform, false);
        RectTransform storyRt = storyObj.GetComponent<RectTransform>();
        storyRt.anchorMin = new Vector2(0, 1);
        storyRt.anchorMax = new Vector2(1, 1);
        storyRt.pivot = new Vector2(0.5f, 1);
        storyRt.anchoredPosition = new Vector2(0, -112);
        storyRt.sizeDelta = new Vector2(-44, 90);
        incidentStoryText = storyObj.GetComponent<TextMeshProUGUI>();
        incidentStoryText.fontSize = 13;
        incidentStoryText.alignment = TextAlignmentOptions.Center;
        incidentStoryText.color = new Color(0.85f, 0.9f, 0.95f);

        // Choice A Button
        choiceABtn = CreateChoiceButton(cardObj.transform, "Вариант А", -225, new Color(0.12f, 0.5f, 0.65f), out choiceAText);

        // Choice B Button
        choiceBBtn = CreateChoiceButton(cardObj.transform, "Вариант Б", -300, new Color(0.15f, 0.6f, 0.38f), out choiceBText);

        modalRoot = root;
        modalRoot.SetActive(false);
    }

    private Button CreateChoiceButton(Transform parent, string label, float posY, Color col, out TMP_Text textComp)
    {
        GameObject b = new GameObject("ChoiceBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        b.transform.SetParent(parent, false);
        RectTransform rt = b.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, posY);
        rt.sizeDelta = new Vector2(400, 60);
        b.GetComponent<Image>().color = col;

        GameObject to = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        to.transform.SetParent(b.transform, false);
        RectTransform trt = to.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = new Vector2(-16, 0);
        textComp = to.GetComponent<TextMeshProUGUI>();
        textComp.text = label;
        textComp.fontSize = 12;
        textComp.fontStyle = FontStyles.Bold;
        textComp.alignment = TextAlignmentOptions.Center;
        textComp.color = Color.white;

        return b.GetComponent<Button>();
    }
}
