using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [Header("Base rewards per customer")]
    public int coinsPerCustomer = 10;
    public int popularityPerCustomer = 5;

    [Header("UI")]
    public GameObject daySummaryPrefab;

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
        float mul = correct ? 1f : 0.5f;
        int gainedCoins = Mathf.RoundToInt(coinsPerCustomer * mul);
        int gainedPopularity = Mathf.RoundToInt(popularityPerCustomer * mul);

        coinsGained += gainedCoins;
        popularityGained += gainedPopularity;

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.AddCoins(gainedCoins);
            PlayerProgress.Instance.AddPopularity(gainedPopularity);
            Debug.Log($"DailyRewardManager: Applied instant rewards: +{gainedCoins} coins, +{gainedPopularity} popularity.");
        }
        else
        {
            Debug.LogWarning("DailyRewardManager: No PlayerProgress found; instant rewards not applied.");
        }
    }

    public void RecordLeftWithoutServed()
    {
        customersLeft++;
    }

    private void OnDayChanged(int newDay)
    {
        Debug.Log($"DailyRewardManager: Day changed -> {newDay}. Collecting Rent...");

        if (RentalManager.Instance != null)
        {
            int rentCoins = 0;
            int rentPop = 0;

            RentalManager.Instance.ProcessNightlyRentals(out rentCoins, out rentPop);

            coinsGained += rentCoins;
            popularityGained += rentPop;

            if (PlayerProgress.Instance != null)
            {
                PlayerProgress.Instance.AddCoins(rentCoins);
                PlayerProgress.Instance.AddPopularity(rentPop);
            }
        }

        ShowSummary(newDay);
    }

    private void ShowSummary(int day)
    {
        if (daySummaryPrefab == null)
        {
            Debug.LogWarning("DailyRewardManager: daySummaryPrefab not assigned. Rewards are applied instantly; just resetting daily counters.");
            ResetDaily();
            return;
        }

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
            int displayDay = Mathf.Max(1, day - 1);
            ui.SetupAndShow(displayDay, customersServed, customersLeft, coinsGained, popularityGained);
        }
        else
        {
            Debug.LogWarning("DailyRewardManager: daySummaryPrefab does not contain DaySummaryUI component.");
            Destroy(go);
            ResetDaily();
        }
    }

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

    public int CustomersServed => customersServed;
    public int CustomersLeft => customersLeft;
    public int CoinsGained => coinsGained;
    public int PopularityGained => popularityGained;
}