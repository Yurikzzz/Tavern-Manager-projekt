using System;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public NPCOrder customer;
    public Dish dish;
    public bool isServed;

    public Order(NPCOrder customer, Dish dish)
    {
        this.customer = customer;
        this.dish = dish;
        this.isServed = false;
    }
}

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    public event Action OnOrdersChanged;

    public List<Order> ActiveOrders { get; private set; } = new List<Order>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged += HandleDayChanged;
        }
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged -= HandleDayChanged;
        }
    }

    private void HandleDayChanged(int newDay)
    {
        ClearAllOrders();
    }

    public Order CreateOrder(NPCOrder customer, Dish dish)
    {
        Order order = new Order(customer, dish);
        ActiveOrders.Add(order);

        OnOrdersChanged?.Invoke();

        Debug.Log($"OrderManager: {customer.gameObject.name} ordered {dish.displayName}");
        return order;
    }

    public void MarkOrderServed(Order order)
    {
        order.isServed = true;
        ActiveOrders.Remove(order);

        OnOrdersChanged?.Invoke();
    }

    public void CancelOrder(Order order)
    {
        if (ActiveOrders.Contains(order))
        {
            ActiveOrders.Remove(order);
            OnOrdersChanged?.Invoke();
        }
    }

    public void ClearAllOrders()
    {
        ActiveOrders.Clear();
        OnOrdersChanged?.Invoke();
    }
}