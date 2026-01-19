using UnityEngine;

public class CustomerPatience : MonoBehaviour
{
    [Header("Patience Settings")]
    public float maxPatience = 10f;
    public float currentPatience;
    public float drainMultiplier = 1f;
    public bool waiting = false;
    private bool served = false;

    [Header("Penalty Settings")]
    public int popularityPenalty = 5;
    public RewardFeedbackUI rewardUI;

    [Header("Bar Elements")]
    public Transform barRoot;
    public Transform barFill;
    private Vector3 barFillBaseScale;

    private NPCController npcController;
    private NPCOrder npcOrder;

    private void Awake()
    {
        currentPatience = maxPatience;

        if (barFill != null)
            barFillBaseScale = barFill.localScale;

        npcController = GetComponent<NPCController>();
        if (npcController == null)
            npcController = GetComponentInParent<NPCController>();

        npcOrder = GetComponent<NPCOrder>();
        if (npcOrder == null)
            npcOrder = GetComponentInParent<NPCOrder>();

        if (rewardUI == null)
            rewardUI = GetComponentInChildren<RewardFeedbackUI>(true);
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged += HandleTimeChanged;
    }

    private void OnEnable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(GameTimeManager.TimeOfDay newTime)
    {
        if (newTime == GameTimeManager.TimeOfDay.Night)
        {
            if (npcOrder != null && npcOrder.HasOrder)
            {
                waiting = false;
                if (barRoot != null)
                    barRoot.gameObject.SetActive(false);
            }
        }
    }

    public void SitAndStartWaiting()
    {
        waiting = true;
        served = false;
        currentPatience = maxPatience;

        if (barRoot != null)
            barRoot.gameObject.SetActive(true);

        UpdateBar();
    }

    private void Update()
    {
        if (!waiting || served) return;

        currentPatience -= Time.deltaTime * drainMultiplier;
        if (currentPatience < 0f) currentPatience = 0f;

        UpdateBar();

        if (currentPatience <= 0f)
        {
            waiting = false;
            OnPatienceExpired();
        }
    }

    private void UpdateBar()
    {
        if (barFill == null) return;

        float t = currentPatience / maxPatience;
        Vector3 s = barFillBaseScale;
        s.x *= t;
        barFill.localScale = s;

        var sr = barFill.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

    public void OnServed()
    {
        served = true;
        waiting = false;

        if (barRoot != null)
            barRoot.gameObject.SetActive(false);
    }

    private void OnPatienceExpired()
    {
        if (barRoot != null)
            barRoot.gameObject.SetActive(false);

        if (OrderManager.Instance != null && npcOrder != null)
        {
            Order myOrder = null;
            foreach (var order in OrderManager.Instance.ActiveOrders)
            {
                if (order.customer == npcOrder)
                {
                    myOrder = order;
                    break;
                }
            }

            if (myOrder != null)
            {
                Debug.Log($"Customer {name} ran out of patience! Cancelling order.");
                OrderManager.Instance.CancelOrder(myOrder);
            }
        }

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AddPopularity(-popularityPenalty);
        }

        if (rewardUI != null)
        {
            rewardUI.ShowReward(0, -popularityPenalty);
        }

        DailyRewardManager.Instance?.RecordLeftWithoutServed(popularityPenalty);

        DailyRewardManager.Instance?.RecordLeftWithoutServed();

        if (npcController != null)
        {
            npcController.StartLeaving();
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }
}