using System.Collections.Generic;
using UnityEngine;

public class BarUIController : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject barUIRoot;

    [Header("Orders UI")]
    public Transform ordersListParent;      // OrdersList object
    public GameObject orderButtonPrefab;    // OrderButton prefab

    [Header("References")]
    public PlayerCarry playerCarry;

    private bool isOpen = false;
    private Order selectedOrder;
    private readonly List<GameObject> spawnedOrderButtons = new List<GameObject>();

    void Start()
    {
        if (barUIRoot != null)
            barUIRoot.SetActive(false);

        if (playerCarry == null)
        {
            playerCarry = FindObjectOfType<PlayerCarry>();
            if (playerCarry == null)
                Debug.LogWarning("BarUIController: No PlayerCarry found in scene.");
        }
    }

    public void Open()
    {
        if (barUIRoot == null)
        {
            Debug.LogWarning("BarUIController: barUIRoot is not assigned.");
            return;
        }

        barUIRoot.SetActive(true);
        isOpen = true;

        RefreshOrdersList();
    }

    public void Close()
    {
        if (barUIRoot == null)
            return;

        barUIRoot.SetActive(false);
        isOpen = false;
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    // Called when the UI opens, or when you later want to refresh
    public void RefreshOrdersList()
    {
        // Clear old buttons
        foreach (var go in spawnedOrderButtons)
        {
            if (go != null) Destroy(go);
        }
        spawnedOrderButtons.Clear();
        selectedOrder = null;

        if (ordersListParent == null)
        {
            Debug.LogWarning("BarUIController: ordersListParent not assigned.");
            return;
        }

        if (OrderManager.Instance == null)
        {
            Debug.LogWarning("BarUIController: No OrderManager in scene.");
            return;
        }

        foreach (var order in OrderManager.Instance.ActiveOrders)
        {
            if (order.isServed)
                continue; // skip already served orders

            GameObject buttonObj = Instantiate(orderButtonPrefab, ordersListParent);
            spawnedOrderButtons.Add(buttonObj);

            OrderButtonUI buttonUI = buttonObj.GetComponent<OrderButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(order, this);
            }
        }
    }

    // Called by OrderButtonUI when a button is clicked
    public void SelectOrder(Order order)
    {
        selectedOrder = order;
        Debug.Log($"BarUI: Selected order for {order.customer.gameObject.name} - {order.dish.displayName}");
        // Later: we can visually highlight the selected button
    }

    // Hook this to the Confirm button's OnClick in the Inspector
    public void ConfirmSelectedOrder()
    {
        if (playerCarry == null || OrderManager.Instance == null)
            return;

        if (playerCarry.HasDish)
        {
            Debug.Log("Bar: Player already carrying something.");
            return;
        }

        if (selectedOrder == null)
        {
            Debug.Log("Bar: No order selected.");
            return;
        }

        if (selectedOrder.isServed)
        {
            Debug.Log("Bar: Selected order is already served.");
            return;
        }

        // Give player the dish for the selected order
        playerCarry.TakeDish(selectedOrder.dish);

        Debug.Log($"Bar: Prepared {selectedOrder.dish.displayName} for {selectedOrder.customer.gameObject.name}");

        // For now, just close the UI after confirming
        Close();
    }
}
