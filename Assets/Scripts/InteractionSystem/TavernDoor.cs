using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TavernDoor : Interactable
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [Header("Confirmation UI")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI confirmationLabel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Auto-open settings")]
    [SerializeField] private float openDuration = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isOpen = false;
    private enum PendingAction { None, Open, Close }
    private PendingAction pendingAction = PendingAction.None;

    private Coroutine openSpriteRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
        spriteRenderer.flipX = false;

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

    private void OnEnable()
    {
        NPCSpawner.OnNpcSpawned += HandleNpcSpawned;
        NPCController.OnNpcDestroyed += HandleNpcDestroyed;
    }

    private void OnDisable()
    {
        NPCSpawner.OnNpcSpawned -= HandleNpcSpawned;
        NPCController.OnNpcDestroyed -= HandleNpcDestroyed;
    }

    private void HandleNpcSpawned(GameObject npc)
    {
        if (openSpriteRoutine != null)
            StopCoroutine(openSpriteRoutine);

        openSpriteRoutine = StartCoroutine(ShowOpenSpriteTemporarily(openDuration));
    }

    private void HandleNpcDestroyed(GameObject npc)
    {
        if (openSpriteRoutine != null)
            StopCoroutine(openSpriteRoutine);

        openSpriteRoutine = StartCoroutine(ShowOpenSpriteTemporarily(openDuration));
    }

    private IEnumerator ShowOpenSpriteTemporarily(float seconds)
    {
        spriteRenderer.sprite = openSprite;
        spriteRenderer.flipX = true;

        yield return new WaitForSeconds(seconds);

        spriteRenderer.sprite = closedSprite;
        spriteRenderer.flipX = false;

        openSpriteRoutine = null;
    }

    public override void Interact()
    {
        var timeManager = GameTimeManager.Instance;
        if (timeManager == null)
        {
            Debug.LogWarning("GameTimeManager instance missing.");
            return;
        }

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
        if (timeManager == null)
        {
            Debug.LogWarning("GameTimeManager instance missing.");
            HideConfirmation();
            return;
        }

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
        Debug.Log("The tavern is now open for business!");
    }

    private void CloseTavern()
    {
        if (!isOpen) return;
        isOpen = false;
        Debug.Log("The tavern is now closed for the night!");
    }
}