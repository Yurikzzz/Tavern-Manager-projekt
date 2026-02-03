using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TavernDecoration : MonoBehaviour
{
    [Tooltip("Drag the ScriptableObject for this specific item here")]
    public UpgradeData myUpgradeData;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpgradeManager.Instance.OnUpgradePurchased += HandlePurchase;

        UpdateVisuals();
    }

    void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased -= HandlePurchase;
        }
    }

    void HandlePurchase(UpgradeData data)
    {
        if (data.ID == myUpgradeData.ID)
        {
            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        bool isOwned = UpgradeManager.Instance.HasPurchased(myUpgradeData);

        spriteRenderer.enabled = isOwned;

        if (isOwned)
        {
            spriteRenderer.sprite = myUpgradeData.visualSprite;
        }
    }
}