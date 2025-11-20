using UnityEngine;
using UnityEngine.UI;

public class Bed : Interactable
{
    [Header("Confirmation UI")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Text confirmationLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private enum PendingAction { None, Sleep }
    private PendingAction pendingAction = PendingAction.None;

    private void Awake()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmAction);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelAction);
    }

    public override void Interact()
    {
        ShowConfirmation();
    }

    private void ShowConfirmation()
    {
        if (confirmationPanel == null || confirmationLabel == null)
        {
            Debug.LogWarning("Confirmation UI has not been assigned on the Bed.");
            return;
        }

        pendingAction = PendingAction.Sleep;
        confirmationLabel.text = "Are you sure you want to sleep and advance to the next day?";
        confirmationPanel.SetActive(true);
    }

    private void ConfirmAction()
    {
        if (pendingAction != PendingAction.Sleep)
            return;

        Sleep();
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
            confirmationPanel.SetActive(false);
    }

    private void Sleep()
    {
        Debug.Log("Player is sleeping...");
        GameTimeManager.Instance.NextDay();
    }
}