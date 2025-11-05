using UnityEngine;

public class NPCOrder : MonoBehaviour
{
    [Header("What this NPC can order")]
    public Dish[] possibleDishes;

    public Order CurrentOrder { get; private set; }

    public bool HasOrder => CurrentOrder != null && !CurrentOrder.isServed;

    public void StartOrder()
    {
        if (HasOrder)
            return;

        if (possibleDishes == null || possibleDishes.Length == 0)
        {
            Debug.LogWarning($"NPCOrder: {name} has no possible dishes assigned.");
            return;
        }

        Dish chosen = possibleDishes[Random.Range(0, possibleDishes.Length)];

        if (OrderManager.Instance == null)
        {
            Debug.LogWarning("NPCOrder: No OrderManager in scene.");
            return;
        }

        CurrentOrder = OrderManager.Instance.CreateOrder(this, chosen);

        Debug.Log($"{name} started an order for {chosen.displayName}");
    }

    public bool TryServe(Dish dishFromPlayer)
    {
        if (!HasOrder)
            return false;

        if (dishFromPlayer == CurrentOrder.dish)
        {
            OrderManager.Instance.MarkOrderServed(CurrentOrder);
            Debug.Log($"{name} was served {dishFromPlayer.displayName}");
            return true;
        }

        return false;
    }
}
