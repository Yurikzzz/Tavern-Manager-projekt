using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DaySummaryUI : MonoBehaviour
{
    [Header("UI fields")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI servedText;
    [SerializeField] private TextMeshProUGUI leftText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI popularityText;
    [SerializeField] private Button confirmButton;

    private int coinsToApply;
    private int popularityToApply;

    void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
    }

    public void SetupAndShow(int day, int served, int left, int coins, int popularity)
    {
        Debug.Log($"DaySummaryUI: SetupAndShow(day {day}) called");

        if (dayText != null) dayText.text = $"Day {day} summary";
        if (servedText != null) servedText.text = $"Customers served: {served}";
        if (leftText != null) leftText.text = $"Customers left: {left}";
        if (coinsText != null) coinsText.text = $"Coins gained: {coins}";
        if (popularityText != null) popularityText.text = $"Popularity gained: {popularity}";

        coinsToApply = coins;
        popularityToApply = popularity;

        TimeManager.RequestPause();
        Time.timeScale = 0f;
    }

    public void OnConfirm()
    {
        Debug.Log("DaySummaryUI: Confirm pressed. Closing summary.");
        DailyRewardManager.Instance?.ResetDaily();

        Time.timeScale = 1f;
        TimeManager.RequestUnpause();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirm);
    }
}