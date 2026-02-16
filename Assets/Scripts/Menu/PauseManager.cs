using UnityEngine;

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
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);

        if (startPopup != null && !startPopup.activeSelf)
        {
            Time.timeScale = 1f;
        }
        else if (startPopup == null)
        {
            Time.timeScale = 1f;
        }
    }
}