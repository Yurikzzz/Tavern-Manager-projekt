using System.Collections;
using UnityEngine;

public class NPCOrder : MonoBehaviour
{
    [Header("What this NPC can order")]
    public Dish[] possibleDishes;

    [Header("Behaviour")]
    public float eatingDuration = 10f;

    [Header("UI / Markers")]
    public GameObject deliveryMarker;

    [Header("Feedback Icons")]
    [Tooltip("The visual object to show when the order is correct (e.g. Green Checkmark)")]
    public GameObject correctFeedbackIcon;
    [Tooltip("The visual object to show when the order is wrong (e.g. Red X)")]
    public GameObject wrongFeedbackIcon;
    [Tooltip("How long the feedback icon stays visible")]
    public float feedbackDuration = 2.0f;

    public Order CurrentOrder { get; private set; }
    public bool HasOrder => CurrentOrder != null && !CurrentOrder.isServed;

    private Coroutine eatingRoutine;
    private NPCController npcController;

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    void Start()
    {
        if (deliveryMarker != null)
            deliveryMarker.SetActive(false);

        if (correctFeedbackIcon != null) correctFeedbackIcon.SetActive(false);
        if (wrongFeedbackIcon != null) wrongFeedbackIcon.SetActive(false);
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

    public void ShowDeliveryMarker()
    {
        if (deliveryMarker != null)
            deliveryMarker.SetActive(true);
    }

    public void HideDeliveryMarker()
    {
        if (deliveryMarker != null)
            deliveryMarker.SetActive(false);
    }

    public bool TryServe(Dish dishFromPlayer)
    {
        var patience = GetComponent<CustomerPatience>();
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
            Debug.Log($"{name}: Mmm, that's exactly my {dishFromPlayer.displayName}!");
            ShowFeedback(correctFeedbackIcon);
        }
        else
        {
            Debug.Log($"{name}: Wrong dish, but thanks...");
            ShowFeedback(wrongFeedbackIcon);
        }

        DailyRewardManager.Instance?.RecordServed(correct);

        OrderManager.Instance.MarkOrderServed(CurrentOrder);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (patience != null)
            patience.OnServed();

        HideDeliveryMarker();

        if (eatingRoutine == null)
            eatingRoutine = StartCoroutine(EatAndLeave());

        return true;
    }

    private void ShowFeedback(GameObject icon)
    {
        if (icon != null)
        {
            StartCoroutine(FeedbackRoutine(icon));
        }
    }

    private IEnumerator FeedbackRoutine(GameObject icon)
    {
        icon.SetActive(true);
        yield return new WaitForSeconds(feedbackDuration);
        icon.SetActive(false);
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

    void OnDestroy()
    {
        if (CurrentOrder != null && OrderManager.Instance != null)
        {
            OrderManager.Instance.ActiveOrders.Remove(CurrentOrder);
        }
    }
}