using UnityEngine;

public class CleaningTask : Interactable
{
    public enum TaskType { Trash, Bed, Dust }

    [Header("Task Settings")]
    public TaskType type;
    public RoomManager myRoom; 

    [Header("Visuals (For Beds Only)")]
    public Sprite cleanSprite;
    public Sprite dirtySprite;
    private SpriteRenderer sr;
    private Collider2D myCollider;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
    }

    public override void Interact()
    {
        if (type == TaskType.Bed)
        {
            if (sr != null && cleanSprite != null) sr.sprite = cleanSprite;

            if (myCollider != null) myCollider.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }

        HidePrompt();

        if (myRoom != null)
        {
            myRoom.TaskCompleted();
        }
    }

    public void ResetTask()
    {
        if (type == TaskType.Bed)
        {
            if (sr != null && dirtySprite != null) sr.sprite = dirtySprite;
            if (myCollider != null) myCollider.enabled = true;
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }
}