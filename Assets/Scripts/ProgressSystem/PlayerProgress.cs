using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance { get; private set; }

    [Header("Player resources")]
    public int Coins = 0;
    public int Popularity = 0;

    public event System.Action<int> OnCoinsChanged;
    public event System.Action<int> OnPopularityChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        OnCoinsChanged?.Invoke(Coins);
        OnPopularityChanged?.Invoke(Popularity);
    }

    public void AddCoins(int amount)
    {
        Coins += Mathf.Max(0, amount);
        Debug.Log($"PlayerProgress: +{amount} coins (total {Coins})");
        OnCoinsChanged?.Invoke(Coins);
    }

    public void AddPopularity(int amount)
    {
        Popularity += Mathf.Max(0, amount);
        Debug.Log($"PlayerProgress: +{amount} popularity (total {Popularity})");
        OnPopularityChanged?.Invoke(Popularity);
    }
}