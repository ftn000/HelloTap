using System;
using UnityEngine;

public enum UpgradeCategory
{
    Hardware,       // Железо: увеличивает силу клика (строки за тап)
    PassiveStaff,   // Автоматизация: пассивные строки кода в секунду
    Fuel,           // Еда и энергетики: буст и криты
    Comfort         // Мебель и обстановка: множители дохода
}

[System.Serializable]
public class UpgradeItem
{
    [SerializeField] private string id;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private UpgradeCategory category;
    [SerializeField] private double baseCost;
    [SerializeField] private double costMultiplier = 1.15;
    [SerializeField] private double basePower;
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private int maxLevel = 0; // 0 = без ограничений

    public string Id => id;
    public string Title => title;
    public string Description => description;
    public UpgradeCategory Category => category;
    public double BaseCost => baseCost;
    public double CostMultiplier => costMultiplier;
    public double BasePower => basePower;
    public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public int MaxLevel => maxLevel;

    public UpgradeItem(string id, string title, string description, UpgradeCategory category, double baseCost, double basePower, double costMultiplier = 1.15, int maxLevel = 0)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.category = category;
        this.baseCost = baseCost;
        this.basePower = basePower;
        this.costMultiplier = costMultiplier;
        this.maxLevel = maxLevel;
        this.currentLevel = 0;
    }

    /// <summary>
    /// Стоимость улучшения для текущего уровня: Cost = BaseCost * (CostMultiplier ^ Level)
    /// </summary>
    public double GetCurrentCost()
    {
        return Math.Floor(baseCost * Math.Pow(costMultiplier, currentLevel));
    }

    /// <summary>
    /// Суммарный прирост мощности от текущего уровня
    /// </summary>
    public double GetTotalPower()
    {
        return currentLevel * basePower;
    }

    public bool IsMaxLevel()
    {
        return maxLevel > 0 && currentLevel >= maxLevel;
    }

    public bool CanAfford(double availableCurrency)
    {
        if (IsMaxLevel()) return false;
        return availableCurrency >= GetCurrentCost();
    }

    public bool Buy(ref double currency)
    {
        double cost = GetCurrentCost();
        if (CanAfford(currency))
        {
            currency -= cost;
            currentLevel++;
            return true;
        }
        return false;
    }
}
