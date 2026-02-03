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

    [Header("Visuals")]
    public Sprite visualSprite; 
}