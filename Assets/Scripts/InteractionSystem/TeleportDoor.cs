using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeleportDoor : Interactable
{
    [Header("Player spawn")]
    public Transform playerDestination;

    [Header("Camera point")]
    public Transform cameraTarget;

    [Header("Fading Settings")]
    public Image blackScreen;
    public float fadeSpeed = 2f;

    private RoomCameraController camController;

    void Start()
    {
        camController = Camera.main.GetComponent<RoomCameraController>();
        if (promptUI != null) promptUI.SetActive(false);
    }

    public override void Interact()
    {
        HidePrompt();
        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }

        GameObject.FindGameObjectWithTag("Player").transform.position = playerDestination.position;

        if (camController != null && cameraTarget != null)
        {
            camController.currentTarget = cameraTarget;

            camController.SnapImmediately();
        }

        yield return new WaitForSeconds(0.2f);

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(0);
    }

    private void SetAlpha(float a)
    {
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = a;
            blackScreen.color = c;
        }
    }
}