using System;
using UnityEngine;

/// <summary>
/// Модель достижения (ачивки) для GameDev Idle.
/// Каждое открытое достижение даёт постоянный процентный бонус к доходу.
/// </summary>
[Serializable]
public class AchievementData
{
    [SerializeField] private string id;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private double bonusFraction; // Например 0.05 = +5% к доходу
    [SerializeField] private bool isUnlocked;

    public string Id => id;
    public string Title => title;
    public string Description => description;
    public double BonusFraction => bonusFraction;
    public bool IsUnlocked { get => isUnlocked; set => isUnlocked = value; }

    public AchievementData(string id, string title, string description, double bonusFraction)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.bonusFraction = bonusFraction;
        this.isUnlocked = false;
    }
}
