using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance;

    [Header("Background Animations")]
    [Tooltip("The Animator component on your Background object")]
    public Animator backgroundAnimator;

    [Tooltip("Exact name of the Day animation state in the Animator")]
    public string dayAnimationState = "Background_Day";

    [Tooltip("Exact name of the Night animation state in the Animator")]
    public string nightAnimationState = "Background_Night";

    [Header("Time Settings")]
    [Tooltip("How long the tavern stays open (in seconds) before automatically closing.")]
    public float afternoonDuration = 300f;

    [Header("Transitions")]
    [Tooltip("Assign the exact same Black Screen Image you use for your GameStartFader here.")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    public enum TimeOfDay { Morning, Afternoon, Night }
    public TimeOfDay CurrentTime { get; private set; } = TimeOfDay.Morning;
    public int CurrentDay { get; private set; } = 1;

    public float AfternoonProgress
    {
        get
        {
            if (CurrentTime != TimeOfDay.Afternoon || afternoonDuration <= 0f)
                return 0f;
            return Mathf.Clamp01(afternoonTimerElapsed / afternoonDuration);
        }
    }

    public event Action<TimeOfDay> OnTimeChanged;
    public event Action<int> OnDayChanged;

    private Coroutine afternoonTimerRoutine;
    private float afternoonTimerElapsed = 0f;

    void Awake()
    {
        Instance = this;

        // Try to find the animator if one wasn't assigned in the inspector
        if (backgroundAnimator == null)
            backgroundAnimator = GetComponent<Animator>();
        if (backgroundAnimator == null)
            backgroundAnimator = GetComponentInChildren<Animator>();

        if (backgroundAnimator == null)
            Debug.LogWarning("GameTimeManager: No Animator assigned for the background.");

        ApplyBackgroundForTime(CurrentTime);
    }

    void Start()
    {
        if (SaveManager.instance != null)
        {
            CurrentDay = SaveManager.instance.currentData.dayNumber;
        }
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
        if (afternoonTimerRoutine != null)
        {
            StopCoroutine(afternoonTimerRoutine);
            afternoonTimerRoutine = null;
        }

        CurrentTime = newTime;
        afternoonTimerElapsed = 0f;

        if (CurrentTime == TimeOfDay.Afternoon)
        {
            afternoonTimerRoutine = StartCoroutine(AfternoonTimer());
        }

        ApplyBackgroundForTime(CurrentTime);
        OnTimeChanged?.Invoke(CurrentTime);
        Debug.Log($"Time set to: {CurrentTime}");
    }

    private IEnumerator AfternoonTimer()
    {
        while (afternoonTimerElapsed < afternoonDuration)
        {
            afternoonTimerElapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("GameTimeManager: Afternoon time limit reached. Closing tavern.");
        SetTime(TimeOfDay.Night);
    }

    void ApplyBackgroundForTime(TimeOfDay time)
    {
        if (backgroundAnimator == null) return;

        // Play the correct animation based on the time of day
        switch (time)
        {
            case TimeOfDay.Morning:
            case TimeOfDay.Afternoon:
                backgroundAnimator.Play(dayAnimationState);
                break;
            case TimeOfDay.Night:
                backgroundAnimator.Play(nightAnimationState);
                break;
        }
    }

    public void NextDay()
    {
        if (CurrentTime == TimeOfDay.Night)
        {
            StartCoroutine(DayTransitionSequence());
        }
        else
        {
            Debug.LogWarning("Cannot advance to next day unless it's night.");
        }
    }

    private IEnumerator DayTransitionSequence()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = true;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Color c = fadeImage.color;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }

            Color finalC = fadeImage.color;
            finalC.a = 1f;
            fadeImage.color = finalC;
        }

        CurrentDay++;
        SetTime(TimeOfDay.Morning);

        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveGame();
        }

        OnDayChanged?.Invoke(CurrentDay);
        Debug.Log($"New day started: Day {CurrentDay}");

        yield return null;
        yield return new WaitUntil(() => Time.timeScale > 0f);

        if (fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Color c = fadeImage.color;
                c.a = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }

            Color finalC = fadeImage.color;
            finalC.a = 0f;
            fadeImage.color = finalC;
            fadeImage.raycastTarget = false;
        }
    }
}