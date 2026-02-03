using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Shop Settings")]
    public UpgradeData[] allUpgrades;

    private HashSet<string> purchasedUpgradeIDs = new HashSet<string>();
    public event System.Action<UpgradeData> OnUpgradePurchased;

    public int TotalCoinBonus { get; private set; } = 0;
    public int TotalPopularityBonus { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool HasPurchased(UpgradeData upgrade)
    {
        return purchasedUpgradeIDs.Contains(upgrade.ID);
    }

    public void BuyUpgrade(UpgradeData upgrade)
    {
        if (HasPurchased(upgrade)) return;

        if (PlayerProgress.Instance.Coins >= upgrade.cost)
        {
            PlayerProgress.Instance.AddCoins(-upgrade.cost);
            purchasedUpgradeIDs.Add(upgrade.ID);

            TotalCoinBonus += upgrade.coinIncomeBonus;
            TotalPopularityBonus += upgrade.popularityBonus;

            OnUpgradePurchased?.Invoke(upgrade);
            Debug.Log($"Bought {upgrade.displayName}. New Coin Bonus: +{TotalCoinBonus}");
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}