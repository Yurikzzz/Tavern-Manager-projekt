using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

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

    void Start()
    {
        if (SaveManager.instance != null)
        {
            LoadSavedUpgrades(SaveManager.instance.currentData.boughtUpgrades);
        }
    }

    private void LoadSavedUpgrades(List<string> savedIDs)
    {
        purchasedUpgradeIDs.Clear();
        TotalCoinBonus = 0;
        TotalPopularityBonus = 0;

        foreach (string savedID in savedIDs)
        {
            purchasedUpgradeIDs.Add(savedID);

            foreach (UpgradeData upgrade in allUpgrades)
            {
                if (upgrade.ID == savedID)
                {
                    TotalCoinBonus += upgrade.coinIncomeBonus;
                    TotalPopularityBonus += upgrade.popularityBonus;
                    break;
                }
            }
        }

        Debug.Log($"Loaded {purchasedUpgradeIDs.Count} upgrades. Total Coin Bonus: {TotalCoinBonus}");
    }

    public List<string> GetPurchasedUpgradesList()
    {
        return purchasedUpgradeIDs.ToList();
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