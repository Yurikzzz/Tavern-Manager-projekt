using UnityEngine;

public class TavernDoor : Interactable
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    private SpriteRenderer spriteRenderer;
    private bool isOpen = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    public override void Interact()
    {
        var timeManager = GameTimeManager.Instance;

        switch (timeManager.CurrentTime)
        {
            case GameTimeManager.TimeOfDay.Morning:
                OpenTavern();
                timeManager.SetTime(GameTimeManager.TimeOfDay.Afternoon);
                break;

            case GameTimeManager.TimeOfDay.Afternoon:
                CloseTavern();
                timeManager.SetTime(GameTimeManager.TimeOfDay.Night);
                break;

            default:
                Debug.Log("You can only open in the morning or close in the evening.");
                break;
        }
    }

    private void OpenTavern()
    {
        if (isOpen) return;
        isOpen = true;
        spriteRenderer.sprite = openSprite;
        Debug.Log("The tavern is now open for business!");
        // later: enable NPC spawner or door collider
    }

    private void CloseTavern()
    {
        if (!isOpen) return;
        isOpen = false;
        spriteRenderer.sprite = closedSprite;
        Debug.Log("The tavern is now closed for the night!");
        // later: disable NPC spawner or door collider
    }
}
