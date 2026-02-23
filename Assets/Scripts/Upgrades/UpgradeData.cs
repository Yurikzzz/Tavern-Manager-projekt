using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Tavern/Upgrade")]
public class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string ID;
    public string displayName;
    public int cost;

    [Header("Bonuses")]
    public int coinIncomeBonus;
    public int popularityBonus;
    [Tooltip("Reduces how fast patience drains. E.g., 0.2 will make the bar empty 20% slower.")]
    public float patienceBonus;

    [Header("Visuals")]
    public Sprite visualSprite;
}