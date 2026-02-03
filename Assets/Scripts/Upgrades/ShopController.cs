using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("References")]
    public GameObject shopWindow;       
    public Transform contentContainer;  
    public GameObject itemPrefab;   

    private bool isOpen = false;

    void Start()
    {
        shopWindow.SetActive(false);

        PopulateShop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopWindow.SetActive(isOpen);

        Time.timeScale = isOpen ? 0 : 1;

        if (isOpen)
        {
            RefreshAllItems();
        }
    }

    void PopulateShop()
    {
        foreach (var data in UpgradeManager.Instance.allUpgrades)
        {
            GameObject newItem = Instantiate(itemPrefab, contentContainer);

            ShopItemUI uiScript = newItem.GetComponent<ShopItemUI>();
            if (uiScript != null)
            {
                uiScript.Initialize(data);
            }
        }
    }

    void RefreshAllItems()
    {
        foreach (Transform child in contentContainer)
        {
            ShopItemUI item = child.GetComponent<ShopItemUI>();
            if (item != null) item.UpdateState();
        }
    }
}