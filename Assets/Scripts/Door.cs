using UnityEngine;
using System.Collections;

public class Door : Interactable
{
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    private SpriteRenderer spriteRenderer;

    private bool isOpen = false;
    [SerializeField] private float autoCloseDelay = 3f; 

    private Coroutine autoCloseRoutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = closedSprite;
    }

    public override void Interact()
    {
        if (!isOpen)
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        blockingCollider.enabled = false;
        spriteRenderer.sprite = openSprite;

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
        }
        autoCloseRoutine = StartCoroutine(AutoCloseAfterDelay());
    }

    private void CloseDoor()
    {
        isOpen = false;
        blockingCollider.enabled = true;
        spriteRenderer.sprite = closedSprite;

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }
    }

    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoor();
    }
}
