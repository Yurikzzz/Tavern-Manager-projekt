using UnityEngine;

public class BarInteractable : Interactable
{
    private PlayerCarry playerCarry;

    void Awake()
    {
        playerCarry = FindObjectOfType<PlayerCarry>();
        if (playerCarry == null)
            Debug.LogWarning("BarInteractable: No PlayerCarry found in scene.");
    }

    public override void Interact()
    {
        if (playerCarry == null || OrderManager.Instance == null)
            return;

        if (playerCarry.HasDish)
        {
            Debug.Log("Bar: Player already carrying something.");
            return;
        }

        Order nextOrder = null;
        foreach (var order in OrderManager.Instance.ActiveOrders)
        {
            if (!order.isServed)
            {
                nextOrder = order;
                break;
            }
        }

        if (nextOrder == null)
        {
            Debug.Log("Bar: No pending orders.");
            return;
        }

        playerCarry.TakeDish(nextOrder.dish);

        Debug.Log($"Bar: Prepared {nextOrder.dish.displayName} for {nextOrder.customer.gameObject.name}");
    }
}
