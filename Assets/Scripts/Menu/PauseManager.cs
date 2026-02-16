using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;

    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false); 
        Time.timeScale = 1f; 
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuPanel.SetActive(true); 
        Time.timeScale = 0f;  
        isPaused = true;
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
}