using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // Automatically adds an AudioSource
public class TeleportDoor : Interactable
{
    [Header("Destinations")]
    public Transform playerDestination;
    public Transform cameraTarget;

    [Header("Camera Settings")]
    public RoomCameraController.ResolutionType targetResolution;
    public Collider2D roomBounds;

    [Header("UI")]
    public Image blackScreenImage;
    public float fadeDuration = 0.5f;

    [Header("Audio")]
    public AudioClip interactSound; // The single sound effect for teleporting

    private static bool isGlobalTransitionActive = false;

    private Collider2D doorCollider;
    private AudioSource audioSource; // Reference to our audio source

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>(); // Grab the Audio Source
    }

    public void SetLocked(bool isLocked)
    {
        if (doorCollider != null) doorCollider.enabled = !isLocked;
    }

    public override void Interact()
    {
        // Don't play the sound or teleport if we are already transitioning
        if (isGlobalTransitionActive) return;

        // Play the teleport sound right as the interaction starts!
        if (interactSound != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        isGlobalTransitionActive = true;
        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                Color c = blackScreenImage.color;
                c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                blackScreenImage.color = c;
                yield return null;
            }
            Color final = blackScreenImage.color;
            final.a = 1f;
            blackScreenImage.color = final;
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerDestination.position;
        }

        if (Camera.main != null)
        {
            RoomCameraController camController = Camera.main.GetComponent<RoomCameraController>();
            if (camController != null)
            {
                camController.MoveToRoom(cameraTarget, targetResolution, roomBounds);
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (blackScreenImage != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                Color c = blackScreenImage.color;
                c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                blackScreenImage.color = c;
                yield return null;
            }
            Color final = blackScreenImage.color;
            final.a = 0f;
            blackScreenImage.color = final;
            blackScreenImage.gameObject.SetActive(false);
        }

        isGlobalTransitionActive = false;
    }
}