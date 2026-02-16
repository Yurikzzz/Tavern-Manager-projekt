using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject startPopup;

    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        TimeManager.RequestPause();
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        TimeManager.RequestUnpause();
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu"); 
    }
}

public static class TimeManager
{
    private static int pauseRequests = 0;

    public static void RequestPause()
    {
        pauseRequests++;
        UpdateTimeScale();
    }

    public static void RequestUnpause()
    {
        pauseRequests--;
        if (pauseRequests < 0) pauseRequests = 0;
        UpdateTimeScale();
    }

    private static void UpdateTimeScale()
    {
        Time.timeScale = (pauseRequests > 0) ? 0f : 1f;
        Debug.Log("Current Pause Requests: " + pauseRequests);
    }
}
