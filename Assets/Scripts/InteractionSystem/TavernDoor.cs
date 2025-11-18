using UnityEngine;
using UnityEngine.UI;

public class TavernDoor : Interactable
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [Header("Confirmation UI")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Text confirmationLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private SpriteRenderer spriteRenderer;
    private bool isOpen = false;
    private enum PendingAction { None, Open, Close }
    private PendingAction pendingAction = PendingAction.None;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmAction);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelAction);
        }
    }

    public override void Interact()
    {
        var timeManager = GameTimeManager.Instance;

        switch (timeManager.CurrentTime)
        {
            case GameTimeManager.TimeOfDay.Morning:
                ShowConfirmation(PendingAction.Open);
                break;

            case GameTimeManager.TimeOfDay.Afternoon:
                ShowConfirmation(PendingAction.Close);
                break;

            default:
                Debug.Log("You can only open in the morning or close in the evening.");
                break;
        }
    }

    private void ShowConfirmation(PendingAction action)
    {
        if (confirmationPanel == null || confirmationLabel == null)
        {
            Debug.LogWarning("Confirmation UI has not been assigned on the TavernDoor.");
            return;
        }

        pendingAction = action;
        confirmationPanel.SetActive(true);
        confirmationLabel.text = action == PendingAction.Open
            ? "Are you sure you want to open the tavern?"
            : "Are you sure you want to close the tavern?";
    }

    private void ConfirmAction()
    {
        if (pendingAction == PendingAction.None)
        {
            return;
        }

        var timeManager = GameTimeManager.Instance;

        if (pendingAction == PendingAction.Open)
        {
            OpenTavern();
            timeManager.SetTime(GameTimeManager.TimeOfDay.Afternoon);
        }
        else if (pendingAction == PendingAction.Close)
        {
            CloseTavern();
            timeManager.SetTime(GameTimeManager.TimeOfDay.Night);
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

    private void OpenTavern()
    {
        if (isOpen) return;
        isOpen = true;
        spriteRenderer.sprite = openSprite;
        Debug.Log("The tavern is now open for business!");
    }

    private void CloseTavern()
    {
        if (!isOpen) return;
        isOpen = false;
        spriteRenderer.sprite = closedSprite;
        Debug.Log("The tavern is now closed for the night!");
    }
}