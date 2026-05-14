using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // Automatically adds an AudioSource to the door
public class Door : Interactable
{
    [Header("Door Visuals")]
    [SerializeField] private Collider2D blockingCollider;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;

    [Header("Door Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Settings")]
    [SerializeField] private float autoCloseDelay = 3f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isOpen = false;
    private Coroutine autoCloseRoutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); // Grab the audio source

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

        // Play the open sound!
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

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

        // Play the close sound!
        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

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