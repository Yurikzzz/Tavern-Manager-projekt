using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DayDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;

    [Header("Time of Day UI")]
    [SerializeField] private Image timeOfDayImage;
    [SerializeField] private Sprite morningSprite;
    [SerializeField] private Sprite afternoonSprite;
    [SerializeField] private Sprite nightSprite;

    void Start()
    {
        var gm = GameTimeManager.Instance;
        if (gm == null)
        {
            Debug.LogWarning("GameTimeManager instance not found.");
            return;
        }

        gm.OnDayChanged += UpdateDay;
        gm.OnTimeChanged += UpdateTime;

        UpdateDay(gm.CurrentDay);
        UpdateTime(gm.CurrentTime);
    }

    private void UpdateDay(int day)
    {
        if (dayText != null)
            dayText.text = $"Day {day}";
    }

    private void UpdateTime(GameTimeManager.TimeOfDay time)
    {
        if (timeOfDayImage == null)
            return;

        Sprite spriteToSet = time switch
        {
            GameTimeManager.TimeOfDay.Morning => morningSprite,
            GameTimeManager.TimeOfDay.Afternoon => afternoonSprite,
            GameTimeManager.TimeOfDay.Night => nightSprite,
            _ => null
        };

        timeOfDayImage.sprite = spriteToSet;
        timeOfDayImage.enabled = spriteToSet != null;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged -= UpdateDay;
            GameTimeManager.Instance.OnTimeChanged -= UpdateTime;
        }
    }
}