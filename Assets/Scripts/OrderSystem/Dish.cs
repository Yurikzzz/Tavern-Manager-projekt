using UnityEngine;

[CreateAssetMenu(fileName = "Dish", menuName = "Tavern/Dish")]
public class Dish : ScriptableObject
{
    public string displayName;
    public Sprite icon;

    [Header("Rewards")]
    public int coinReward = 15;
    public int popularityReward = 5;
}