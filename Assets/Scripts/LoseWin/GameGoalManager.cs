using UnityEngine;
using UnityEngine.SceneManagement;

public class GameGoalManager : MonoBehaviour
{
    public static GameGoalManager Instance { get; private set; }

    [Header("Goal Settings")]
    [Tooltip("The total number of days the player has to achieve the goal.")]
    public int targetDays = 7;

    [Tooltip("The amount of popularity needed by the end of the target days to win.")]
    public int targetPopularity = 100;

    [Header("Scene Routing")]
    [Tooltip("The name of your Main Menu scene to load upon losing.")]
    public string mainMenuSceneName = "MainMenu";

    private bool hasEvaluated = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameTimeManager.Instance.CurrentDay > targetDays)
        {
            hasEvaluated = true;
        }

        GameTimeManager.Instance.OnDayChanged += CheckWinCondition;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnDayChanged -= CheckWinCondition;
        }
    }

    private void CheckWinCondition(int currentDay)
    {
        if (hasEvaluated) return;

        if (currentDay > targetDays)
        {
            hasEvaluated = true;

            if (PlayerProgress.Instance.Popularity >= targetPopularity)
            {
                WinGame();
            }
            else
            {
                LoseGame();
            }
        }
    }

    private void WinGame()
    {
        Debug.Log("WIN: Player has reached the popularity goal! Entering Sandbox Mode.");

    }

    private void LoseGame()
    {
        Debug.Log("LOSE: Player failed to reach the popularity goal. Wiping save and returning to Menu.");

        if (SaveManager.instance != null)
        {
            SaveManager.instance.StartNewGame();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}