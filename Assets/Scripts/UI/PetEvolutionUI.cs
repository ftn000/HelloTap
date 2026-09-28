using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Система эволюции и инкубатора питомцев-компаньонов (Pet Evolution & Incubator):
/// - 5 уникальных питомцев: Кот, Корги, Робо-дрон, Кибер-Уточка, Кибер-Октопус
/// - Система опыта (XP) и уровней (Lv 1 - Lv 10), усиливающих пассивные баффы
/// - Интерактивные действия: «Погладить» (буст счастья) и «Покормить» (мгновенный XP)
/// - Автоматическое начисление опыта при тапах и критах
/// </summary>
public class PetEvolutionUI : MonoBehaviour
{
    private static PetEvolutionUI instance;
    public static PetEvolutionUI Instance
    {
        get
        {
            if (instance == null) instance = FindFirstObjectByType<PetEvolutionUI>();
            return instance;
        }
    }

    public enum PetType
    {
        Cat = 0,
        Corgi = 1,
        RoboDrone = 2,
        RubberDuck = 3,
        CyberOcto = 4
    }

    [System.Serializable]
    public class PetData
    {
        public PetType type;
        public string name;
        public string icon;
        public string perkDescription;
        public int level;
        public int currentXp;
        public int requiredXp;
        public float happiness; // 0..1
        public bool isUnlocked;

        public PetData(PetType t, string n, string ic, string perk, bool unlocked)
        {
            type = t;
            name = n;
            icon = ic;
            perkDescription = perk;
            level = 1;
            currentXp = 0;
            requiredXp = 100;
            happiness = 0.8f;
            isUnlocked = unlocked;
        }

        public float Progress => (float)currentXp / Math.Max(1, requiredXp);
    }

    [Header("Кнопки открытия и модальное окно")]
    [SerializeField] private Button openPetBtn;
    [SerializeField] private TMP_Text openPetBtnText;
    [SerializeField] private Button closePetBtn;
    [SerializeField] private Button closeXBtn;
    [SerializeField] private Button backdropBtn;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private Transform modalCardTransform;

    [Header("Интерфейс выбранного питомца")]
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text petLevelText;
    [SerializeField] private Image petXpBarFill;
    [SerializeField] private TMP_Text petXpText;
    [SerializeField] private TMP_Text petPerkText;
    [SerializeField] private Image petHappinessBarFill;
    [SerializeField] private TMP_Text petHappinessText;

    [Header("Кнопки взаимодействия")]
    [SerializeField] private Button petAnimalBtn;
    [SerializeField] private Button feedAnimalBtn;
    [SerializeField] private TMP_Text feedBtnText;
    [SerializeField] private Button selectActivePetBtn;
    [SerializeField] private TMP_Text selectBtnText;

    [Header("Кнопки переключения питомцев")]
    [SerializeField] private Button prevPetBtn;
    [SerializeField] private Button nextPetBtn;

    private readonly List<PetData> pets = new List<PetData>();
    private int selectedPetIndex = 0;
    private int activeCompanionIndex = 0;

    public bool IsModalOpen => modalRoot != null && modalRoot.activeInHierarchy;
    public PetData ActivePet => pets.Count > activeCompanionIndex ? pets[activeCompanionIndex] : null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        InitializePets();
        LoadPetsData();

        if (modalRoot != null) modalRoot.SetActive(false);
        BindButtons();
    }

    private void Start()
    {
        BindButtons();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked += HandleCodeClicked;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCodeClicked -= HandleCodeClicked;
        }
    }

    private void InitializePets()
    {
        pets.Clear();
        pets.Add(new PetData(PetType.Cat, "Кот-маскот", "🐱", "+25% длительности режима «В Потоке», +10% к пассивному доходу", true));
        pets.Add(new PetData(PetType.Corgi, "Корги-программист", "🐶", "+15% шанс крит-печати, +20% силы клика", true));
        pets.Add(new PetData(PetType.RoboDrone, "Робо-дрон", "🤖", "+20% к скорости генерации кода скриптами и ИИ", true));
        pets.Add(new PetData(PetType.RubberDuck, "Кибер-Уточка", "🦆", "25% шанс авто-устранения багов, +50% награды за баги", true));
        pets.Add(new PetData(PetType.CyberOcto, "Кибер-Октопус", "🐙", "x1.5 бонус к наградам на фриланс-бирже", true));
    }

    private void BindButtons()
    {
        if (openPetBtn != null)
        {
            openPetBtn.onClick.RemoveAllListeners();
            openPetBtn.onClick.AddListener(OpenModal);
        }
        if (closePetBtn != null)
        {
            closePetBtn.onClick.RemoveAllListeners();
            closePetBtn.onClick.AddListener(CloseModal);
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

        if (prevPetBtn != null)
        {
            prevPetBtn.onClick.RemoveAllListeners();
            prevPetBtn.onClick.AddListener(PrevPet);
        }
        if (nextPetBtn != null)
        {
            nextPetBtn.onClick.RemoveAllListeners();
            nextPetBtn.onClick.AddListener(NextPet);
        }

        if (petAnimalBtn != null)
        {
            petAnimalBtn.onClick.RemoveAllListeners();
            petAnimalBtn.onClick.AddListener(OnPetAnimalClicked);
        }
        if (feedAnimalBtn != null)
        {
            feedAnimalBtn.onClick.RemoveAllListeners();
            feedAnimalBtn.onClick.AddListener(OnFeedAnimalClicked);
        }
        if (selectActivePetBtn != null)
        {
            selectActivePetBtn.onClick.RemoveAllListeners();
            selectActivePetBtn.onClick.AddListener(OnSelectActivePetClicked);
        }
    }

    private void HandleCodeClicked(double amount, bool isCrit, Vector2 pos)
    {
        // Активный питомец получает XP от кликов
        PetData active = ActivePet;
        if (active != null)
        {
            int gainedXp = isCrit ? 5 : 1;
            AddXp(active, gainedXp);
        }
    }

    public void AddXp(PetData pet, int xp)
    {
        pet.currentXp += xp;
        if (pet.currentXp >= pet.requiredXp && pet.level < 10)
        {
            pet.currentXp -= pet.requiredXp;
            pet.level++;
            pet.requiredXp = (int)(pet.requiredXp * 1.6f);

            HapticFeedback.SuccessPattern();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRelease();

            if (ClickJuice.Instance != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🎉 {pet.name} повышен до Ур. {pet.level}!", transform.position, new Color(1f, 0.85f, 0.2f), true);
            }
        }
        SavePetsData();
        if (modalRoot != null && modalRoot.activeSelf)
        {
            RefreshUI();
        }
    }

    public void OnPetAnimalClicked()
    {
        PetData p = pets[selectedPetIndex];
        p.happiness = Mathf.Clamp01(p.happiness + 0.15f);
        AddXp(p, 10);

        HapticFeedback.MediumImpact();
        PlayPetSound(p.type);

        if (ClickJuice.Instance != null && petAnimalBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"💖 {p.name} счастлив (+XP)!", petAnimalBtn.transform.position, new Color(1f, 0.4f, 0.7f), false);
        }
        RefreshUI();
    }

    public void OnFeedAnimalClicked()
    {
        double feedCost = 1500.0;
        if (GameManager.Instance != null && GameManager.Instance.Money >= feedCost)
        {
            GameManager.Instance.SpendMoney(feedCost);
            PetData p = pets[selectedPetIndex];
            p.happiness = 1.0f;
            AddXp(p, 60);

            HapticFeedback.MediumImpact();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySipSound();

            if (ClickJuice.Instance != null && feedAnimalBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup($"🍖 {p.name} сыт! (+60 XP)", feedAnimalBtn.transform.position, new Color(0.2f, 1f, 0.6f), false);
            }
            RefreshUI();
        }
        else
        {
            HapticFeedback.WarningHaptic();
            if (ClickJuice.Instance != null && feedAnimalBtn != null)
            {
                ClickJuice.Instance.SpawnCustomPopup("Недостаточно средств (1 500 ₽)", feedAnimalBtn.transform.position, new Color(1f, 0.3f, 0.3f), false);
            }
        }
    }

    public void OnSelectActivePetClicked()
    {
        activeCompanionIndex = selectedPetIndex;
        PlayerPrefs.SetInt("ActiveCompanionIndex", activeCompanionIndex);
        PlayerPrefs.Save();

        HapticFeedback.MediumImpact();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCassetteClick();

        PetData p = pets[activeCompanionIndex];
        if (ClickJuice.Instance != null && selectActivePetBtn != null)
        {
            ClickJuice.Instance.SpawnCustomPopup($"✨ Спутник выбран: {p.name}!", selectActivePetBtn.transform.position, new Color(0.2f, 0.9f, 1f), true);
        }
        RefreshUI();
    }

    public void PrevPet()
    {
        selectedPetIndex = (selectedPetIndex - 1 + pets.Count) % pets.Count;
        HapticFeedback.LightImpact();
        RefreshUI();
    }

    public void NextPet()
    {
        selectedPetIndex = (selectedPetIndex + 1) % pets.Count;
        HapticFeedback.LightImpact();
        RefreshUI();
    }

    private void PlayPetSound(PetType type)
    {
        if (AudioManager.Instance == null) return;
        switch (type)
        {
            case PetType.Cat: AudioManager.Instance.PlayCatPurr(); break;
            case PetType.Corgi: AudioManager.Instance.PlayDogBark(); break;
            case PetType.RoboDrone: AudioManager.Instance.PlayRoboBeep(); break;
            case PetType.RubberDuck: AudioManager.Instance.PlayDuckQuack(); break;
            default: AudioManager.Instance.PlayMouseClick(); break;
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
        RefreshUI();
    }

    public void CloseModal()
    {
        if (modalRoot != null) modalRoot.SetActive(false);
        HapticFeedback.LightImpact();
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

    private void RefreshUI()
    {
        PetData p = pets[selectedPetIndex];
        bool isActive = selectedPetIndex == activeCompanionIndex;

        if (petNameText != null)
        {
            petNameText.text = $"{p.icon} <b>{p.name}</b> {(isActive ? "<color=#00FF88>[АКТИВЕН]</color>" : "")}";
        }

        if (petLevelText != null)
        {
            petLevelText.text = $"Уровень {p.level} / 10";
        }

        if (petXpBarFill != null)
        {
            petXpBarFill.fillAmount = p.Progress;
        }

        if (petXpText != null)
        {
            petXpText.text = $"Опыт: {p.currentXp} / {p.requiredXp} XP ({(p.Progress * 100f):F0}%)";
        }

        if (petPerkText != null)
        {
            float buffMultiplier = 1.0f + (p.level - 1) * 0.15f;
            petPerkText.text = $"<color=#FFD166>Эффект спутника (x{buffMultiplier:F2}):</color>\n{p.perkDescription}";
        }

        if (petHappinessBarFill != null)
        {
            petHappinessBarFill.fillAmount = p.happiness;
        }

        if (petHappinessText != null)
        {
            petHappinessText.text = $"Настроение: {Mathf.RoundToInt(p.happiness * 100)}%";
        }

        if (selectBtnText != null)
        {
            selectBtnText.text = isActive ? "✓ ВЫБРАН СПУТНИКОМ" : "ВЫБРАТЬ СПУТНИКОМ";
        }

        if (selectActivePetBtn != null)
        {
            selectActivePetBtn.interactable = !isActive;
        }
    }

    private void LoadPetsData()
    {
        activeCompanionIndex = PlayerPrefs.GetInt("ActiveCompanionIndex", 0);
        for (int i = 0; i < pets.Count; i++)
        {
            var p = pets[i];
            p.level = PlayerPrefs.GetInt($"Pet_{p.type}_Level", 1);
            p.currentXp = PlayerPrefs.GetInt($"Pet_{p.type}_Xp", 0);
            p.requiredXp = PlayerPrefs.GetInt($"Pet_{p.type}_ReqXp", 100);
            p.happiness = PlayerPrefs.GetFloat($"Pet_{p.type}_Happiness", 0.8f);
        }
    }

    private void SavePetsData()
    {
        for (int i = 0; i < pets.Count; i++)
        {
            var p = pets[i];
            PlayerPrefs.SetInt($"Pet_{p.type}_Level", p.level);
            PlayerPrefs.SetInt($"Pet_{p.type}_Xp", p.currentXp);
            PlayerPrefs.SetInt($"Pet_{p.type}_ReqXp", p.requiredXp);
            PlayerPrefs.SetFloat($"Pet_{p.type}_Happiness", p.happiness);
        }
        PlayerPrefs.Save();
    }
}
