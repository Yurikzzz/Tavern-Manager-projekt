using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour
{
    [SerializeField] private Text dayText;

    void Start()
    {
        GameTimeManager.Instance.OnDayChanged += UpdateDay;

        UpdateDay(GameTimeManager.Instance.CurrentDay);
    }

    private void UpdateDay(int day)
    {
        dayText.text = $"Day {day}";
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged -= UpdateDay;
    }
}
