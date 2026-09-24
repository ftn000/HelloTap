using System;
using UnityEngine;

[System.Serializable]
public class GameProjectData
{
    [SerializeField] private string id;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private double requiredCodeLines;
    [SerializeField] private double rewardMoney;
    [SerializeField] private double passiveMoneyIncomePerSec;
    [SerializeField] private bool isCompleted;

    public string Id => id;
    public string Title => title;
    public string Description => description;
    public double RequiredCodeLines => requiredCodeLines;
    public double RewardMoney => rewardMoney;
    public double PassiveMoneyIncomePerSec => passiveMoneyIncomePerSec;
    public bool IsCompleted { get => isCompleted; set => isCompleted = value; }

    public GameProjectData(string id, string title, string description, double requiredCodeLines, double rewardMoney, double passiveMoneyIncomePerSec)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.requiredCodeLines = requiredCodeLines;
        this.rewardMoney = rewardMoney;
        this.passiveMoneyIncomePerSec = passiveMoneyIncomePerSec;
        this.isCompleted = false;
    }

    public float GetProgress(double currentCode)
    {
        if (isCompleted) return 1f;
        if (requiredCodeLines <= 0) return 1f;
        return Mathf.Clamp01((float)(currentCode / requiredCodeLines));
    }
}
