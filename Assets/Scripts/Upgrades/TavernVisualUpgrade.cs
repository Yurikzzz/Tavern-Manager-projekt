using UnityEngine;
using UnityEngine.UI;

public class TavernVisualUpgrade : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Which upgrade triggers these changes?")]
    public UpgradeData upgradeRequired;

    [Header("Action 1: Appear/Disappear")]
    [Tooltip("These objects will be HIDDEN until an upgrade is bought.")]
    public GameObject[] showWhenBought;

    [Tooltip("These objects will be SHOWN until you an upgrade is bought.")]
    public GameObject[] hideWhenBought;

    [Header("Action 2: Sprite Swap")]
    [Tooltip("Change the sprite of these objects")]
    public SpriteRenderer[] spritesToChange;
    [Tooltip("If true, it uses the sprite from the UpgradeData. If false, uses the one below.")]
    public bool useIconFromUpgrade = false;
    public Sprite alternativeSprite; 

    private void Start()
    {
        UpdateVisuals();

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased += OnUpgradePurchased;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradePurchased -= OnUpgradePurchased;
        }
    }

    private void OnUpgradePurchased(UpgradeData data)
    {
        if (data == upgradeRequired)
        {
            UpdateVisuals();
        }
    }

    public void UpdateVisuals()
    {
        if (upgradeRequired == null) return;

        bool isOwned = UpgradeManager.Instance.HasPurchased(upgradeRequired);

        foreach (var obj in showWhenBought)
        {
            if (obj != null) obj.SetActive(isOwned);
        }

        foreach (var obj in hideWhenBought)
        {
            if (obj != null) obj.SetActive(!isOwned);
        }

        if (isOwned && spritesToChange.Length > 0)
        {
            Sprite targetSprite = useIconFromUpgrade ? upgradeRequired.visualSprite : alternativeSprite;

            if (targetSprite != null)
            {
                foreach (var sr in spritesToChange)
                {
                    if (sr != null) sr.sprite = targetSprite;
                }
            }
        }
    }
}