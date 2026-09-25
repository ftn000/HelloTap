using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI контроллер для магазина улучшений и релизов проектов.
/// Позволяет как вручную привязать кнопки в Инспекторе, так и генерировать список динамически.
/// </summary>
public class DevShopUI : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeButtonBinding
    {
        public string upgradeId;
        public Button buyButton;
        public TMP_Text titleText;
        public TMP_Text costText;
        public TMP_Text levelText;
    }

    [System.Serializable]
    public class ProjectButtonBinding
    {
        public string projectId;
        public Button releaseButton;
        public TMP_Text titleText;
        public TMP_Text reqText;
        public TMP_Text rewardText;
    }

    [Header("Привязанные кнопки улучшений")]
    [SerializeField] private List<UpgradeButtonBinding> upgradeBindings = new List<UpgradeButtonBinding>();

    [Header("Привязанные кнопки проектов")]
    [SerializeField] private List<ProjectButtonBinding> projectBindings = new List<ProjectButtonBinding>();

    [Header("Вкладки магазина")]
    [SerializeField] private Button tabHardwareBtn;
    [SerializeField] private Button tabStaffBtn;
    [SerializeField] private Button tabProjectsBtn;
    [SerializeField] private GameObject hardwareView;
    [SerializeField] private GameObject staffView;
    [SerializeField] private GameObject projectsView;
    [SerializeField] private ScrollRect shopScrollRect;

    [Header("Модальное окно")]
    [SerializeField] private GameObject shopModalRoot;
    [SerializeField] private Button openShopBtn;
    [SerializeField] private Button openStaffBtn;
    [SerializeField] private Button openProjectsBtn;
    [SerializeField] private Button closeShopBtn;
    [SerializeField] private Button backdropCloseBtn;

    public List<UpgradeButtonBinding> UpgradeBindings => upgradeBindings;
    public List<ProjectButtonBinding> ProjectBindings => projectBindings;
    public bool IsModalOpen => shopModalRoot != null && shopModalRoot.activeInHierarchy;

    private int activeTab = 0; // 0: Железо, 1: Персонал, 2: Проекты

    private void Awake()
    {
        if (shopModalRoot != null)
        {
            shopModalRoot.SetActive(false);
        }
        BindModalButtons();
        BindTabButtons();
    }

    public void OpenShop(int tab = 0)
    {
        if (shopModalRoot != null)
        {
            if (shopModalRoot.activeSelf && activeTab == tab)
            {
                CloseShop();
                return;
            }
            shopModalRoot.SetActive(true);
        }
        SelectTab(tab);
        RefreshUI();
    }

    public void CloseShop()
    {
        if (shopModalRoot != null) shopModalRoot.SetActive(false);
    }

    public void SetupModal(GameObject modalRoot, Button openShop, Button openProj, Button closeShop, Button openStaff = null, Button backdropClose = null, ScrollRect scrollRect = null)
    {
        shopModalRoot = modalRoot;
        openShopBtn = openShop;
        openStaffBtn = openStaff;
        openProjectsBtn = openProj;
        closeShopBtn = closeShop;
        backdropCloseBtn = backdropClose;
        if (scrollRect != null) shopScrollRect = scrollRect;
        if (shopModalRoot != null) shopModalRoot.SetActive(false);
        BindModalButtons();
    }

    private void BindModalButtons()
    {
        if (openShopBtn != null)
        {
            openShopBtn.onClick.RemoveAllListeners();
            openShopBtn.onClick.AddListener(() => OpenShop(0));
        }
        if (openStaffBtn != null)
        {
            openStaffBtn.onClick.RemoveAllListeners();
            openStaffBtn.onClick.AddListener(() => OpenShop(1));
        }
        if (openProjectsBtn != null)
        {
            openProjectsBtn.onClick.RemoveAllListeners();
            openProjectsBtn.onClick.AddListener(() => OpenShop(2));
        }
        if (closeShopBtn != null)
        {
            closeShopBtn.onClick.RemoveAllListeners();
            closeShopBtn.onClick.AddListener(CloseShop);
        }
        if (backdropCloseBtn != null)
        {
            backdropCloseBtn.onClick.RemoveAllListeners();
            backdropCloseBtn.onClick.AddListener(CloseShop);
        }
    }

    private void Start()
    {
        SubscribeEvents();
        BindModalButtons();
        BindTabButtons();
        BindEvents();
        SelectTab(activeTab);
        RefreshUI();
    }

    private void Update()
    {
        if (IsModalOpen && UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseShop();
        }
    }

    public void SetupTabs(Button bHw, Button bSt, Button bPr, GameObject vHw, GameObject vSt, GameObject vPr)
    {
        tabHardwareBtn = bHw;
        tabStaffBtn = bSt;
        tabProjectsBtn = bPr;
        hardwareView = vHw;
        staffView = vSt;
        projectsView = vPr;
        BindTabButtons();
        SelectTab(0);
    }

    private void BindTabButtons()
    {
        if (tabHardwareBtn != null)
        {
            tabHardwareBtn.onClick.RemoveAllListeners();
            tabHardwareBtn.onClick.AddListener(() => SelectTab(0));
        }
        if (tabStaffBtn != null)
        {
            tabStaffBtn.onClick.RemoveAllListeners();
            tabStaffBtn.onClick.AddListener(() => SelectTab(1));
        }
        if (tabProjectsBtn != null)
        {
            tabProjectsBtn.onClick.RemoveAllListeners();
            tabProjectsBtn.onClick.AddListener(() => SelectTab(2));
        }
    }

    public void SelectTab(int tabIndex)
    {
        activeTab = tabIndex;
        if (hardwareView != null) hardwareView.SetActive(activeTab == 0);
        if (staffView != null) staffView.SetActive(activeTab == 1);
        if (projectsView != null) projectsView.SetActive(activeTab == 2);

        SetTabButtonVisual(tabHardwareBtn, activeTab == 0);
        SetTabButtonVisual(tabStaffBtn, activeTab == 1);
        SetTabButtonVisual(tabProjectsBtn, activeTab == 2);

        if (shopScrollRect != null)
        {
            if (activeTab == 0 && hardwareView != null)
                shopScrollRect.content = hardwareView.GetComponent<RectTransform>();
            else if (activeTab == 1 && staffView != null)
                shopScrollRect.content = staffView.GetComponent<RectTransform>();
            else if (activeTab == 2 && projectsView != null)
                shopScrollRect.content = projectsView.GetComponent<RectTransform>();
        }
    }

    private void SetTabButtonVisual(Button btn, bool isActive)
    {
        if (btn == null || btn.targetGraphic == null) return;
        btn.targetGraphic.color = isActive ? Color.white : new Color(0.60f, 0.68f, 0.78f, 0.85f);
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= RefreshUI;
            GameManager.Instance.OnCurrenciesChanged += RefreshUI;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
        }
    }

    private void OnEnable()
    {
        BindModalButtons();
        BindTabButtons();
        SelectTab(activeTab);
        SubscribeEvents();
        BindEvents();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged -= RefreshUI;
            GameManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted -= HandleProjectCompleted;
        }
    }

    public void RebindAndRefresh()
    {
        BindModalButtons();
        BindTabButtons();
        BindEvents();
        RefreshUI();
    }

    private void BindEvents()
    {
        foreach (var binding in upgradeBindings)
        {
            if (binding.buyButton != null)
            {
                string id = binding.upgradeId;
                binding.buyButton.onClick.RemoveAllListeners();
                binding.buyButton.onClick.AddListener(() => OnBuyClicked(id));
            }
        }

        foreach (var pBinding in projectBindings)
        {
            if (pBinding.releaseButton != null)
            {
                string id = pBinding.projectId;
                pBinding.releaseButton.onClick.RemoveAllListeners();
                pBinding.releaseButton.onClick.AddListener(() => OnReleaseClicked(id));
            }
        }
    }

    private void OnBuyClicked(string id)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BuyUpgrade(id);
        }
    }

    private void OnReleaseClicked(string id)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryReleaseProject(id);
        }
    }

    private void HandleUpgradePurchased(UpgradeItem item)
    {
        RefreshUI();
    }

    private void HandleProjectCompleted(GameProjectData project)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (GameManager.Instance == null) return;

        double money = GameManager.Instance.Money;
        double code = GameManager.Instance.CodeLines;

        // Обновление кнопок улучшений
        foreach (var binding in upgradeBindings)
        {
            var upg = FindUpgrade(binding.upgradeId);
            if (upg != null)
            {
                if (binding.titleText != null) binding.titleText.text = upg.Title;
                if (binding.costText != null) binding.costText.text = $"{NumberFormatter.Format(upg.GetCurrentCost())} руб.";
                if (binding.levelText != null) binding.levelText.text = $"Ур. {upg.CurrentLevel}";

                if (binding.buyButton != null)
                {
                    binding.buyButton.interactable = upg.CanAfford(money);
                }
            }
        }

        // Обновление кнопок релизов проектов
        foreach (var pBinding in projectBindings)
        {
            var prj = FindProject(pBinding.projectId);
            if (prj != null)
            {
                if (pBinding.titleText != null) pBinding.titleText.text = prj.Title;
                if (pBinding.rewardText != null) pBinding.rewardText.text = $"+{NumberFormatter.Format(prj.RewardMoney)} руб. (+{prj.PassiveMoneyIncomePerSec}/сек)";

                if (prj.IsCompleted)
                {
                    if (pBinding.reqText != null) pBinding.reqText.text = "РЕЛИЗНУТО!";
                    if (pBinding.releaseButton != null) pBinding.releaseButton.interactable = false;
                }
                else
                {
                    if (pBinding.reqText != null) pBinding.reqText.text = $"{NumberFormatter.Format(code)} / {NumberFormatter.Format(prj.RequiredCodeLines)} строк";
                    if (pBinding.releaseButton != null) pBinding.releaseButton.interactable = code >= prj.RequiredCodeLines;
                }
            }
        }
    }

    private UpgradeItem FindUpgrade(string id)
    {
        foreach (var u in GameManager.Instance.Upgrades)
        {
            if (u.Id == id) return u;
        }
        return null;
    }

    private GameProjectData FindProject(string id)
    {
        foreach (var p in GameManager.Instance.Projects)
        {
            if (p.Id == id) return p;
        }
        return null;
    }
}
