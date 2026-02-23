using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Button buyButton;
    public TextMeshProUGUI buttonText;

    private UpgradeData myData;

    public void Initialize(UpgradeData data)
    {
        myData = data;

        if (iconImage != null) iconImage.sprite = data.visualSprite;
        nameText.text = data.displayName;
        costText.text = data.cost.ToString();

        string desc = "";
        if (data.coinIncomeBonus > 0) desc += $"Coin Bonus: +{data.coinIncomeBonus}\n";
        if (data.popularityBonus > 0) desc += $"Popularity Bonus: +{data.popularityBonus}\n";

        if (data.patienceBonus > 0) desc += $"Patience Bonus: +{data.patienceBonus * 100}%\n";

        descriptionText.text = desc.TrimEnd();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        UpdateState();

        UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;
    }

    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
    }

    void OnUpgradePurchased(UpgradeData data)
    {
        UpdateState();
    }

    void OnBuyClicked()
    {
        UpgradeManager.Instance.BuyUpgrade(myData);
    }

    public void UpdateState()
    {
        bool isOwned = UpgradeManager.Instance.HasPurchased(myData);

        if (isOwned)
        {
            buyButton.interactable = false;
            buttonText.text = "Owned";
        }
        else
        {
            buyButton.interactable = true;
            buttonText.text = "Buy";

            bool canAfford = PlayerProgress.Instance.Coins >= myData.cost;
            buyButton.image.color = canAfford ? Color.white : Color.red;
        }
    }
}