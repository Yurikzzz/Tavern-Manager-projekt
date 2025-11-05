using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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

    public List<Order> ActiveOrders { get; private set; } = new List<Order>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Order CreateOrder(NPCOrder customer, Dish dish)
    {
        Order order = new Order(customer, dish);
        ActiveOrders.Add(order);

        Debug.Log($"OrderManager: {customer.gameObject.name} ordered {dish.displayName}");
        return order;
    }

    public void MarkOrderServed(Order order)
    {
        order.isServed = true;
    }
}
