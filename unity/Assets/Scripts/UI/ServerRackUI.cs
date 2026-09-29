using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Серверная стойка и крипто-майнинг (Server Rack / Home Lab Mini-Game):
/// - 4 серверных лезвия (Raspberry Pi кластер, GPU RTX 4090 ферма, ASIC нейро-чип, Квантовое ядро)
/// - Физика температуры, перегрев (>85°C) и троттлинг при 100°C
/// - Мини-игра активного охлаждения (продув вентиляторов кликами)
/// - Майнинг криптовалюты DevCoin с плавающим курсом обмена на рубли
/// - Режим турбо-разгона (Overclocking x2.0)
/// </summary>
public class ServerRackUI : MonoBehaviour
{
    private static ServerRackUI instance;
    public static ServerRackUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<ServerRackUI>();
            return instance;
        }
    }

    [System.Serializable]
    public class ServerBlade
    {
        public string name;
        public string icon;
        public double hashRateMh; // Мегахэшей/сек
        public float baseTemp;
        public int level;
        public double cost;
        public bool isInstalled;
        public bool isThrottling;
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openRackBtn;
    [SerializeField] private TMP_Text openRackBtnText;
    [SerializeField] private Button closeRackBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Показатели стойки")]
    [SerializeField] private TMP_Text totalHashrateText;
    [SerializeField] private TMP_Text devCoinBalanceText;
    [SerializeField] private TMP_Text currentRateText;
    [SerializeField] private TMP_Text rackTempText;
    [SerializeField] private Image rackTempBarFill;
    [SerializeField] private TMP_Text fanSpeedText;

    [Header("Управление и мини-игра")]
    [SerializeField] private Button emergencyCoolingBtn;
    [SerializeField] private Button toggleOverclockBtn;
    [SerializeField] private TMP_Text overclockBtnText;
    [SerializeField] private Button sellCryptoBtn;
    [SerializeField] private TMP_Text sellCryptoBtnText;

    [Header("Контейнер лезвий")]
    [SerializeField] private Transform bladesContainer;

    private readonly List<ServerBlade> blades = new List<ServerBlade>();
    private double devCoinsMined = 0;
    private float currentRackTemp = 42f; // градусы Цельсия
    private bool isOverclockActive = false;
    private double currentMarketRate = 1450.0; // Рублей за 1 DevCoin
    private float rateChangeTimer = 0f;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public double DevCoinsMined => devCoinsMined;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializeBlades();
        LoadServerData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
    }

    private void InitializeBlades()
    {
        blades.Clear();
        blades.Add(new ServerBlade() { name = "Raspberry Pi Cluster", icon = "🍓", hashRateMh = 120.0, baseTemp = 42f, level = 1, cost = 25000.0, isInstalled = true });
        blades.Add(new ServerBlade() { name = "Dual RTX 4090 Blade", icon = "⚡", hashRateMh = 650.0, baseTemp = 64f, level = 1, cost = 120000.0, isInstalled = false });
        blades.Add(new ServerBlade() { name = "ASIC Neural Unit", icon = "🔬", hashRateMh = 2400.0, baseTemp = 76f, level = 1, cost = 650000.0, isInstalled = false });
        blades.Add(new ServerBlade() { name = "Quantum Node 2026", icon = "🌌", hashRateMh = 10500.0, baseTemp = 86f, level = 1, cost = 2500000.0, isInstalled = false });
    }

    private void BindButtons()
    {
        if (openRackBtn != null)
        {
            openRackBtn.onClick.RemoveAllListeners();
            openRackBtn.onClick.AddListener(OpenModal);
        }
        if (closeRackBtn != null)
        {
            closeRackBtn.onClick.RemoveAllListeners();
            closeRackBtn.onClick.AddListener(CloseModal);
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

        if (emergencyCoolingBtn != null)
        {
            emergencyCoolingBtn.onClick.RemoveAllListeners();
            emergencyCoolingBtn.onClick.AddListener(OnEmergencyCoolingClicked);
        }
        if (toggleOverclockBtn != null)
        {
            toggleOverclockBtn.onClick.RemoveAllListeners();
            toggleOverclockBtn.onClick.AddListener(OnToggleOverclockClicked);
        }
        if (sellCryptoBtn != null)
        {
            sellCryptoBtn.onClick.RemoveAllListeners();
            sellCryptoBtn.onClick.AddListener(OnSellCryptoClicked);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Колебание рыночного курса DevCoin
        rateChangeTimer += dt;
        if (rateChangeTimer >= 15f)
        {
            rateChangeTimer = 0f;
            float shift = UnityEngine.Random.Range(-180f, 220f);
            currentMarketRate = Math.Clamp(currentMarketRate + shift, 800.0, 3200.0);
        }

        // 2. Расчет общего хэшрейта
        double totalHash = 0;
        float targetHeat = 30f;
        int activeBladesCount = 0;

        for (int i = 0; i < blades.Count; i++)
        {
            var b = blades[i];
            if (b.isInstalled && !b.isThrottling)
            {
                double bladeHash = b.hashRateMh * (1.0 + (b.level - 1) * 0.25);
                if (isOverclockActive) bladeHash *= 2.0;
                totalHash += bladeHash;
                targetHeat += b.baseTemp * 0.25f;
                activeBladesCount++;
            }
        }

        if (isOverclockActive) targetHeat += 22f;

        // Нагрев / Остывание
        currentRackTemp = Mathf.MoveTowards(currentRackTemp, targetHeat, dt * (isOverclockActive ? 3.5f : 1.5f));

        // Троттлинг при 98°C+
        for (int i = 0; i < blades.Count; i++)
        {
            var b = blades[i];
            if (b.isInstalled)
            {
                if (currentRackTemp >= 95f) b.isThrottling = true;
                else if (currentRackTemp <= 70f) b.isThrottling = false;
            }
        }

        // Начисление намайненных коинов
        if (totalHash > 0)
        {
            double minedThisFrame = (totalHash / 10000.0) * 0.08 * dt;
            devCoinsMined += minedThisFrame;
        }

        if (modalRoot != null && modalRoot.activeSelf)
        {
            UpdateUI(totalHash);
        }
    }

    public void OnEmergencyCoolingClicked()
    {
        // Продув вентиляторов снижает температуру на 14°C
        currentRackTemp = Mathf.Max(32f, currentRackTemp - 14f);

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();

        if (ClickJuice.Instance != null && emergencyCoolingBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"❄️ ПРОДУВ! -14°C (Текущая: {currentRackTemp:F0}°C)", emergencyCoolingBtn.transform.position, new Color(0.2f, 0.85f, 1f), false);
        }
    }

    public void OnToggleOverclockClicked()
    {
        isOverclockActive = !isOverclockActive;
        HapticFeedback.HeavyImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBoost();

        if (ClickJuice.Instance != null && toggleOverclockBtn != null)
        {
            string msg = isOverclockActive ? "🔥 ТУРБО-РАЗГОН ВКЛ (x2.0 ХЭШРЕЙТ)!" : "🛡️ БЕЗОПАСНЫЙ РЕЖИМ ВКЛ";
            Color col = isOverclockActive ? new Color(1f, 0.4f, 0.1f) : new Color(0.2f, 0.9f, 0.6f);
            ClickJuice.Instance.SpawnCustomPopup(msg, toggleOverclockBtn.transform.position, col, true);
        }
    }

    public void OnSellCryptoClicked()
    {
        if (devCoinsMined < 0.01)
        {
            HapticFeedback.WarningHaptic();
            return;
        }

        double earningsRub = devCoinsMined * currentMarketRate;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(earningsRub);
        }

        double soldAmount = devCoinsMined;
        devCoinsMined = 0;
        SaveServerData();

        HapticFeedback.SuccessPattern();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

        if (ClickJuice.Instance != null && sellCryptoBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💰 ПРОДАНО {soldAmount:F2} DevCoins!\n+{NumberFormatter.Format(earningsRub)} ₽", sellCryptoBtn.transform.position, new Color(1f, 0.85f, 0.2f), true);
        }
    }

    public void UpgradeBlade(int index)
    {
        if (index < 0 || index >= blades.Count) return;
        var b = blades[index];

        int installedCount = 0;
        for (int i = 0; i < blades.Count; i++) if (blades[i].isInstalled) installedCount++;

        int maxAllowed = StudioRealEstateUI.Instance != null ? StudioRealEstateUI.Instance.GetMaxServerBlades() : 4;
        if (!b.isInstalled && installedCount >= maxAllowed)
        {
            HapticFeedback.LightImpact();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"⚠️ Лимит серверной ({maxAllowed} шт.)! Переедьте в новый офис в меню «Недвижимость».", transform.position, new Color(1f, 0.45f, 0.2f), false);
            }
            return;
        }

        double upgradeCost = b.cost * Math.Pow(1.5, b.level - 1);
        if (GameManager.Instance != null && GameManager.Instance.Money >= upgradeCost)
        {
            GameManager.Instance.SpendMoney(upgradeCost);
            if (!b.isInstalled) b.isInstalled = true;
            else b.level++;

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayUpgrade();

            SaveServerData();
            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🚀 {b.name} Ур. {b.level} установлен!", transform.position, new Color(0.2f, 1f, 0.6f), true);
            }
            RebuildBladesList();
        }
        else
        {
            HapticFeedback.WarningHaptic();
        }
    }

    public void OpenModal()
    {
        if (modalRoot == null) return;
        modalRoot.SetActive(true);

        if (modalCardTransform != null)
        {
            modalCardTransform.localScale = new Vector3(0.85f, 0.85f, 1f);
            StartCoroutine(PopCardAnim(modalCardTransform));
        }

        HapticFeedback.MediumImpact();
        RebuildBladesList();
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.LightImpact();
        SaveServerData();
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

    private void UpdateUI(double totalHash)
    {
        if (totalHashrateText != null) totalHashrateText.text = $"⚡ Мощность: <b>{totalHash:N0} MH/s</b>";
        if (devCoinBalanceText != null) devCoinBalanceText.text = $"🪙 Намайнено: <b>{devCoinsMined:F3} DevCoin</b>";
        if (currentRateText != null) currentRateText.text = $"Курс: <b>{currentMarketRate:N0} ₽</b> / 1 DVC";

        if (rackTempText != null)
        {
            string tempColor = currentRackTemp > 85f ? "#FF3333" : (currentRackTemp > 65f ? "#FFD166" : "#00FF88");
            string status = currentRackTemp >= 95f ? "[ТРОТТЛИНГ!]" : (isOverclockActive ? "[РАЗГОН]" : "[НОРМА]");
            rackTempText.text = $"Температура стойки: <color={tempColor}><b>{currentRackTemp:F0}°C</b> {status}</color>";
        }

        if (rackTempBarFill != null)
        {
            rackTempBarFill.fillAmount = Mathf.Clamp01((currentRackTemp - 30f) / 70f);
            rackTempBarFill.color = currentRackTemp > 85f ? new Color(1f, 0.2f, 0.2f) : (currentRackTemp > 65f ? new Color(1f, 0.8f, 0.2f) : new Color(0.2f, 0.9f, 0.5f));
        }

        if (fanSpeedText != null)
        {
            int rpm = (int)(1800 + (currentRackTemp / 100f) * 3200);
            if (isOverclockActive) rpm += 1500;
            fanSpeedText.text = $"Кулеры: {rpm} RPM";
        }

        if (overclockBtnText != null)
        {
            overclockBtnText.text = isOverclockActive ? "🔥 РАЗГОН: ВКЛ" : "❄️ РАЗГОН: ВЫКЛ";
        }

        if (sellCryptoBtnText != null)
        {
            double valueRub = devCoinsMined * currentMarketRate;
            sellCryptoBtnText.text = $"ОБМЕНЯТЬ (+{NumberFormatter.Format(valueRub)} ₽)";
        }
    }

    private void RebuildBladesList()
    {
        if (bladesContainer == null) return;
        for (int i = bladesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(bladesContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < blades.Count; i++)
        {
            int idx = i;
            var b = blades[i];

            GameObject row = new GameObject($"BladeRow_{i}");
            row.transform.SetParent(bladesContainer, false);

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = b.isInstalled ? (b.isThrottling ? new Color(0.35f, 0.1f, 0.1f, 0.8f) : new Color(0.08f, 0.14f, 0.24f, 0.8f)) : new Color(0.05f, 0.07f, 0.12f, 0.6f);

            RectTransform rt = row.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 48);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 4, 4);
            hlg.spacing = 8;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Иконка и Название
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(row.transform, false);
            TMP_Text titleTxt = titleGo.AddComponent<TextMeshProUGUI>();
            string status = b.isInstalled ? (b.isThrottling ? "<color=#FF5555>[ТРОТТЛИНГ]</color>" : $"<color=#00FF88>Ур.{b.level}</color>") : "<color=#888888>[НЕ КУПЛЕНО]</color>";
            titleTxt.text = $"{b.icon} <b>{b.name}</b> {status}\n<size=10><color=#94A3B8>+{b.hashRateMh * b.level:F0} MH/s | ~{b.baseTemp:F0}°C</color></size>";
            titleTxt.fontSize = 11;
            titleTxt.rectTransform.sizeDelta = new Vector2(210, 0);

            // Кнопка Улучшить/Купить
            double cost = b.cost * Math.Pow(1.5, b.level - 1);
            GameObject btnGo = new GameObject("UpgBtn");
            btnGo.transform.SetParent(row.transform, false);
            Image btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.65f, 0.45f, 1f);
            Button btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => UpgradeBlade(idx));

            TMP_Text btnTxt = new GameObject("Txt").AddComponent<TextMeshProUGUI>();
            btnTxt.transform.SetParent(btnGo.transform, false);
            btnTxt.text = b.isInstalled ? $"УЛУЧШИТЬ\n{NumberFormatter.Format(cost)} ₽" : $"КУПИТЬ\n{NumberFormatter.Format(cost)} ₽";
            btnTxt.fontSize = 10;
            btnTxt.alignment = TextAlignmentOptions.Center;
        }
    }

    private void LoadServerData()
    {
        devCoinsMined = double.Parse(PlayerPrefs.GetString("Dev_MinedCoins", "0"));
        for (int i = 0; i < blades.Count; i++)
        {
            var b = blades[i];
            b.isInstalled = PlayerPrefs.GetInt($"Blade_{i}_Installed", b.isInstalled ? 1 : 0) == 1;
            b.level = PlayerPrefs.GetInt($"Blade_{i}_Level", b.level);
        }
    }

    private void SaveServerData()
    {
        PlayerPrefs.SetString("Dev_MinedCoins", devCoinsMined.ToString());
        for (int i = 0; i < blades.Count; i++)
        {
            var b = blades[i];
            PlayerPrefs.SetInt($"Blade_{i}_Installed", b.isInstalled ? 1 : 0);
            PlayerPrefs.SetInt($"Blade_{i}_Level", b.level);
        }
        PlayerPrefs.Save();
    }
}
