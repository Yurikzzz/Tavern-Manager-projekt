using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Backgrounds")]
    public Sprite DayBackground;
    public Sprite NightBackground;

    [Header("Renderer")]
    public SpriteRenderer backgroundRenderer;

    public enum TimeOfDay { Morning, Afternoon, Night }
    public TimeOfDay CurrentTime { get; private set; } = TimeOfDay.Morning;
    public int CurrentDay { get; private set; } = 1;

    public event Action<TimeOfDay> OnTimeChanged;
    public event Action<int> OnDayChanged;

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

        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null)
            backgroundRenderer = GetComponentInChildren<SpriteRenderer>();

        if (backgroundRenderer == null)
            Debug.LogWarning("GameTimeManager: No SpriteRenderer assigned or found. Assign one in inspector.");

        ApplyBackgroundForTime(CurrentTime);

        Debug.Log($"Current day: {CurrentDay}, Time: {CurrentTime}");
    }

    public void AdvanceTime()
    {
        switch (CurrentTime)
        {
            case TimeOfDay.Morning:
                SetTime(TimeOfDay.Afternoon);
                break;
            case TimeOfDay.Afternoon:
                SetTime(TimeOfDay.Night);
                break;
            case TimeOfDay.Night:
                NextDay();
                break;
        }
    }

    public void SetTime(TimeOfDay newTime)
    {
        CurrentTime = newTime;
        ApplyBackgroundForTime(CurrentTime);
        OnTimeChanged?.Invoke(CurrentTime);
        Debug.Log($"Time set to: {CurrentTime}");
    }

    void ApplyBackgroundForTime(TimeOfDay time)
    {
        if (backgroundRenderer == null) return; 

        switch (time)
        {
            case TimeOfDay.Morning:
                if (DayBackground != null) backgroundRenderer.sprite = DayBackground;
                break;
            case TimeOfDay.Afternoon:
                if (DayBackground != null) backgroundRenderer.sprite = DayBackground;
                break;
            case TimeOfDay.Night:
                if (NightBackground != null) backgroundRenderer.sprite = NightBackground;
                break;
        }
    }

    public void NextDay()
    {
        if (CurrentTime == TimeOfDay.Night)
        {
            CurrentDay++;
            SetTime(TimeOfDay.Morning);
            OnDayChanged?.Invoke(CurrentDay);
            Debug.Log($"New day started: Day {CurrentDay}");
        }
        else
        {
            Debug.LogWarning("Cannot advance to next day unless it's night.");
        }
    }
}
