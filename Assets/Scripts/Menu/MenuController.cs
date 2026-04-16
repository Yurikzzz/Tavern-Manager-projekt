using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro

public class MenuController : MonoBehaviour
{
    [Header("Confirmation UI")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private enum PendingAction { None, NewGame, Quit }
    private PendingAction pendingAction = PendingAction.None;

    private void Awake()
    {
        // Hide panel on startup
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        // Listeners for the popup buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmAction);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelAction);
        }
    }

    // --- MAIN MENU BUTTON TRIGGERS ---
    // If your UI buttons are already set up in the Inspector to call StartNewGame(), 
    // you don't need to change them. They will now trigger the popup instead of instantly loading.

    public void StartNewGame()
    {
        ShowConfirmation(PendingAction.NewGame);
    }

    public void ContinueGame()
    {
        // Continuing usually bypasses confirmation, so we just execute directly.
        ExecuteContinueGame();
    }

    public void QuitGame()
    {
        ShowConfirmation(PendingAction.Quit);
    }

    // --- CONFIRMATION LOGIC ---

    private void ShowConfirmation(PendingAction action)
    {
        if (confirmationPanel == null || confirmationLabel == null)
        {
            Debug.LogWarning("Confirmation UI has not been assigned on the MenuController.");
            return;
        }

        pendingAction = action;
        confirmationPanel.SetActive(true);

        // Set text based on what the player is trying to do
        confirmationLabel.text = action == PendingAction.NewGame
            ? "Are you sure you want to start a new game? Any previous unsaved progress will be lost."
            : "Are you sure you want to quit the game?";
    }

    private void ConfirmAction()
    {
        if (pendingAction == PendingAction.None) return;

        // Route to the correct execution method based on what we are confirming
        if (pendingAction == PendingAction.NewGame)
        {
            ExecuteNewGame();
        }
        else if (pendingAction == PendingAction.Quit)
        {
            ExecuteQuitGame();
        }

        HideConfirmation();
    }

    private void CancelAction()
    {
        HideConfirmation();
    }

    private void HideConfirmation()
    {
        pendingAction = PendingAction.None;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    // --- ACTUAL EXECUTION LOGIC ---
    // These contain your original code, safely tucked away until confirmed.

    private void ExecuteNewGame()
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

    private void ExecuteContinueGame()
    {
        TimeManager.Reset();
        if (PauseManager.isPaused) PauseManager.isPaused = false;

        SceneManager.LoadScene("SampleScene");
    }

    private void ExecuteQuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }
}