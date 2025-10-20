using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance; 

    public enum TimeOfDay { Morning, Afternoon, Night }
    public TimeOfDay CurrentTime { get; private set; } = TimeOfDay.Morning;
    public int CurrentDay { get; private set; } = 1;

    public event Action<TimeOfDay> OnTimeChanged;
    public event Action<int> OnDayChanged;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Debug.Log("Current day: " + CurrentDay + ", Time: " + CurrentTime);
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
        OnTimeChanged?.Invoke(CurrentTime);
        Debug.Log($"Time set to: {CurrentTime}");
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
        else {
            Debug.LogWarning("Cannot advance to next day unless it's night.");
        }       
    }
}
