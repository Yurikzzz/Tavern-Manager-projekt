using System.Collections;
using UnityEngine;

public class NPCOrder : MonoBehaviour
{
    public Dish[] possibleDishes;
    public float eatingDuration = 10f;

    public Order CurrentOrder { get; private set; }
    public bool HasOrder => CurrentOrder != null && !CurrentOrder.isServed;

    private Coroutine eatingRoutine;
    private NPCController npcController;

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

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
        if (dishFromPlayer == null)
            return false;

        if (CurrentOrder == null)
        {
            Debug.Log($"{name}: I didn't order anything, but thanks...");
            return true;
        }

        bool correct = (dishFromPlayer == CurrentOrder.dish);

        if (correct)
        {
            OrderManager.Instance.MarkOrderServed(CurrentOrder);
            Debug.Log($"{name}: Mmm, that's exactly my {dishFromPlayer.displayName}!");

            if (eatingRoutine == null)
                eatingRoutine = StartCoroutine(EatAndLeave());
        }
        else
        {
            Debug.Log($"{name}: Wrong dish, but thanks...");

            OrderManager.Instance.MarkOrderServed(CurrentOrder);

            if (eatingRoutine == null)
                eatingRoutine = StartCoroutine(EatAndLeave());
        }

        return true; 
    }


    private IEnumerator EatAndLeave()
    {
        Debug.Log($"{name} starts eating...");

        yield return new WaitForSeconds(eatingDuration);

        Debug.Log($"{name} finished eating and is going to the door.");

        if (npcController != null)
        {
            npcController.StartLeaving();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
