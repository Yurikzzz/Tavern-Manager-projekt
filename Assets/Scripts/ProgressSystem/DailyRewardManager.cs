using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [Header("Base rewards per customer")]
    public int coinsPerCustomer = 10;
    public int popularityPerCustomer = 1;

    [Header("UI")]
    public GameObject daySummaryPrefab; // assign prefab in inspector

    private int customersServed = 0;
    private int customersLeft = 0;
    private int coinsGained = 0;
    private int popularityGained = 0;

    private bool subscribedToDayChanged = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        TrySubscribeToGameTime();
    }

    void Start()
    {
        // in case GameTimeManager wasn't ready in Awake
        TrySubscribeToGameTime();
    }

    void OnDestroy()
    {
        if (subscribedToDayChanged && GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void TrySubscribeToGameTime()
    {
        if (!subscribedToDayChanged && GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged += OnDayChanged;
            subscribedToDayChanged = true;
            Debug.Log("DailyRewardManager: Subscribed to GameTimeManager.OnDayChanged");
        }
    }

    public void RecordServed(bool correct)
    {
        customersServed++;
        float mul = correct ? 1f : 0.25f;
        coinsGained += Mathf.RoundToInt(coinsPerCustomer * mul);
        popularityGained += Mathf.RoundToInt(popularityPerCustomer * mul);
    }

    public void RecordLeftWithoutServed()
    {
        customersLeft++;
    }

    private void OnDayChanged(int newDay)
    {
        Debug.Log($"DailyRewardManager: Day changed -> {newDay}, showing summary.");
        ShowSummary(newDay);
    }

    private void ShowSummary(int day)
    {
        if (daySummaryPrefab == null)
        {
            Debug.LogWarning("DailyRewardManager: daySummaryPrefab not assigned. Applying rewards immediately.");
            ApplyRewards();
            ResetDaily();
            return;
        }

        // instantiate under Canvas if available so UI is visible
        var canvas = FindObjectOfType<Canvas>();
        GameObject go;
        if (canvas != null)
            go = Instantiate(daySummaryPrefab, canvas.transform);
        else
            go = Instantiate(daySummaryPrefab);

        go.SetActive(true);
        go.transform.SetAsLastSibling();

        var ui = go.GetComponent<DaySummaryUI>();
        if (ui != null)
        {
            ui.SetupAndShow(day, customersServed, customersLeft, coinsGained, popularityGained);
        }
        else
        {
            Debug.LogWarning("DailyRewardManager: daySummaryPrefab does not contain DaySummaryUI component.");
            Destroy(go);
            ApplyRewards();
            ResetDaily();
        }
    }

    // Called by UI confirm
    public void ApplyRewards()
    {
        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AddCoins(coinsGained);
            PlayerProgress.Instance.AddPopularity(popularityGained);
            Debug.Log($"DailyRewardManager: Applied rewards: +{coinsGained} coins, +{popularityGained} popularity.");
        }
        else
        {
            Debug.LogWarning("DailyRewardManager: No PlayerProgress found; rewards not applied.");
        }
    }

    public void ResetDaily()
    {
        customersServed = 0;
        customersLeft = 0;
        coinsGained = 0;
        popularityGained = 0;
    }

    // optional getters
    public int CustomersServed => customersServed;
    public int CustomersLeft => customersLeft;
    public int CoinsGained => coinsGained;
    public int PopularityGained => popularityGained;
}