using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartNewGame()
    {
        TimeManager.Reset();
        if (PauseManager.isPaused) PauseManager.isPaused = false;
        Debug.Log("Starting new game...");  

        if (SaveManager.instance != null)
        {
            Debug.Log("Resetting save data for new game...");
            SaveManager.instance.StartNewGame();
        }

        SceneManager.LoadScene("SampleScene");
    }

    public void ContinueGame()
    {
        TimeManager.Reset();
        if (PauseManager.isPaused) PauseManager.isPaused = false;

        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }
}