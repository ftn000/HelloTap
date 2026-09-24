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

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCurrenciesChanged += RefreshUI;
            GameManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            GameManager.Instance.OnProjectCompleted += HandleProjectCompleted;
        }

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
                if (binding.costText != null) binding.costText.text = $"{NumberFormatter.Format(upg.GetCurrentCost())} ₽";
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
                if (pBinding.rewardText != null) pBinding.rewardText.text = $"+{NumberFormatter.Format(prj.RewardMoney)} ₽ (+{prj.PassiveMoneyIncomePerSec}/сек)";

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
