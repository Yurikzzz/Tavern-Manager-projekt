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
        Instance = this;
    }

    void Start()
    {
        if (SaveManager.instance != null)
        {
            Coins = SaveManager.instance.currentData.coins;
            Popularity = SaveManager.instance.currentData.popularity;
        }

        OnCoinsChanged?.Invoke(Coins);
        OnPopularityChanged?.Invoke(Popularity);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        if (Coins < 0) Coins = 0;

        Debug.Log($"PlayerProgress: {(amount >= 0 ? "+" : "")}{amount} coins (total {Coins})");
        OnCoinsChanged?.Invoke(Coins);
    }

    public void AddPopularity(int amount)
    {
        Popularity += amount;
        if (Popularity < 0) Popularity = 0;

        Debug.Log($"PlayerProgress: {(amount >= 0 ? "+" : "")}{amount} popularity (total {Popularity})");
        OnPopularityChanged?.Invoke(Popularity);
    }
}