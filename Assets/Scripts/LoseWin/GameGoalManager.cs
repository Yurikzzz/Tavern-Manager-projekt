using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameGoalManager : MonoBehaviour
{
    public static GameGoalManager Instance { get; private set; }

    [Header("Goal Settings")]
    public int targetDays = 7;
    public int targetPopularity = 50;

    [Header("Scene Routing")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Win/Lose UI Panels")]
    [Tooltip("The parent GameObject of the Win Screen.")]
    public GameObject winPanel;
    [Tooltip("The parent GameObject of the Lose Screen.")]
    public GameObject losePanel;

    [Header("Win/Lose UI Text")]
    public TextMeshProUGUI winTextComponent;
    public TextMeshProUGUI loseTextComponent;

    [Header("Messages")]
    [Tooltip("Use {0} for the Player's Final Popularity, and {1} for Target Days.")]
    [TextArea(3, 5)]
    public string winMessage = "Congratulations! You saved the tavern!\n\nYou managed to get {0} popularity in {1} days.";

    [Tooltip("Use {0} for the Player's Final Popularity, and {1} for Target Days.")]
    [TextArea(3, 5)]
    public string loseMessage = "You failed the city's inspection...\n\nYou only managed to get {0} popularity in {1} days.\n\nThe tavern will be closed down.";

    private bool hasEvaluated = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Make sure both screens are hidden on start
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Don't evaluate if we are loading a save already past the deadline
        if (GameTimeManager.Instance.CurrentDay > targetDays)
        {
            hasEvaluated = true;
        }
    }

    // This is now called by the DaySummaryUI, NOT the DayChange Event
    public void EvaluateGoal(int currentDay)
    {
        if (hasEvaluated) return;

        // E.g., Target is 7 days. On the morning of Day 8, DaySummaryUI closes and triggers this.
        if (currentDay > targetDays)
        {
            hasEvaluated = true;

            // Get the player's actual popularity they achieved
            int actualPopularity = PlayerProgress.Instance.Popularity;

            if (actualPopularity >= targetPopularity)
            {
                ShowWinScreen(actualPopularity);
            }
            else
            {
                ShowLoseScreen(actualPopularity);
            }
        }
    }

    private void ShowWinScreen(int finalPopularity)
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (winTextComponent != null)
        {
            winTextComponent.text = string.Format(winMessage, finalPopularity, targetDays);
        }

        // Pause the game so they can read the win screen
        TimeManager.RequestPause();
        Time.timeScale = 0f;
    }

    private void ShowLoseScreen(int finalPopularity)
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (loseTextComponent != null)
        {
            loseTextComponent.text = string.Format(loseMessage, finalPopularity, targetDays);
        }

        // Pause the game so they can read the lose screen
        TimeManager.RequestPause();
        Time.timeScale = 0f;
    }

    // --- BUTTON EVENTS ---

    // Assign this to your "Continue" button on the Win Screen
    public void ContinueSandboxMode()
    {
        if (winPanel != null) winPanel.SetActive(false);
        Time.timeScale = 1f;
        TimeManager.RequestUnpause();
    }

    // Assign this to your "Head Back" button on the Lose Screen
    public void ReturnToMainMenuAndWipe()
    {
        // Unpause before loading a scene just to be safe
        Time.timeScale = 1f;

        if (SaveManager.instance != null)
        {
            SaveManager.instance.StartNewGame();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}