using System.Collections.Generic;
using UnityEngine;

public class BarUIController : MonoBehaviour
{
    [Header("UI Root")]
    public GameObject barUIRoot;

    [Header("Orders UI")]
    public Transform ordersListParent;
    public GameObject orderButtonPrefab;

    [Header("Dishes UI")]
    public Transform dishesGridParent;
    public GameObject dishButtonPrefab;
    public Dish[] allDishes;

    [Header("References")]
    public PlayerCarry playerCarry;

    private bool isOpen = false;

    private Order selectedOrder;
    private OrderButtonUI selectedOrderButton;

    private DishButtonUI selectedDishButton;

    private readonly List<GameObject> spawnedOrderButtons = new List<GameObject>();
    private readonly List<DishButtonUI> dishButtons = new List<DishButtonUI>();

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

    void OnEnable()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrdersChanged += HandleOrdersChanged;
        }
    }

    void OnDisable()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrdersChanged -= HandleOrdersChanged;
        }
    }

    private void HandleOrdersChanged()
    {
        if (isOpen)
        {
            RefreshOrdersList();
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
        RefreshDishGrid();
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

    public void RefreshOrdersList()
    {
        Order previousSelection = selectedOrder;

        foreach (var go in spawnedOrderButtons)
        {
            if (go != null) Destroy(go);
        }
        spawnedOrderButtons.Clear();

        selectedOrderButton = null;

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

        bool foundPrevious = false;

        foreach (var order in OrderManager.Instance.ActiveOrders)
        {
            if (order.isServed)
                continue;

            GameObject buttonObj = Instantiate(orderButtonPrefab, ordersListParent);
            spawnedOrderButtons.Add(buttonObj);

            OrderButtonUI buttonUI = buttonObj.GetComponent<OrderButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Setup(order, this);

                if (previousSelection != null && order == previousSelection)
                {
                    SelectOrder(buttonUI);
                    foundPrevious = true;
                }
            }
        }

        if (!foundPrevious)
        {
            selectedOrder = null;
        }
    }

    public void SelectOrder(OrderButtonUI button)
    {
        if (selectedOrderButton != null)
        {
            selectedOrderButton.SetSelected(false);
        }

        selectedOrderButton = button;

        if (selectedOrderButton != null)
        {
            selectedOrderButton.SetSelected(true);
            selectedOrder = selectedOrderButton.Order;

            Debug.Log($"BarUI: Selected order for {selectedOrder.customer.gameObject.name}");
        }
        else
        {
            selectedOrder = null;
        }
    }

    public void RefreshDishGrid()
    {
        foreach (Transform child in dishesGridParent)
        {
            Destroy(child.gameObject);
        }
        dishButtons.Clear();
        selectedDishButton = null;

        if (dishesGridParent == null || allDishes == null)
            return;

        foreach (var dish in allDishes)
        {
            if (dish == null) continue;

            GameObject btnObj = Instantiate(dishButtonPrefab, dishesGridParent);
            DishButtonUI btnUI = btnObj.GetComponent<DishButtonUI>();

            if (btnUI != null)
            {
                btnUI.Setup(dish, this);
                dishButtons.Add(btnUI);
            }
        }
    }

    public void SelectDish(DishButtonUI button)
    {
        if (selectedDishButton != null)
        {
            selectedDishButton.SetSelected(false);
        }

        selectedDishButton = button;

        if (selectedDishButton != null)
        {
            selectedDishButton.SetSelected(true);
            Debug.Log($"BarUI: Selected dish {selectedDishButton.Dish.displayName}");
        }
    }

    public void ConfirmSelectedDish()
    {
        if (playerCarry == null)
        {
            Debug.Log("Bar: No PlayerCarry assigned.");
            return;
        }

        if (playerCarry.HasDish)
        {
            Debug.Log("Bar: Player already carrying something.");
            return;
        }

        if (selectedDishButton == null)
        {
            Debug.Log("Bar: No dish selected.");
            return;
        }

        if (selectedOrder == null)
        {
            Debug.Log("Bar: No order selected. You can still carry the dish, but it won't be marked for anyone.");
        }

        Dish chosenDish = selectedDishButton.Dish;
        playerCarry.TakeDish(chosenDish);

        Debug.Log($"Bar: Prepared {chosenDish.displayName}.");

        if (selectedOrder != null && selectedOrder.customer != null)
        {
            selectedOrder.customer.ShowDeliveryMarker();
        }

        Close();
    }
}