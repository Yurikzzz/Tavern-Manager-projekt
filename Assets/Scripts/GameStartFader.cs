using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartFader : MonoBehaviour
{
    public Image blackScreen; // Drag your UI Panel here
    public float fadeSpeed = 1f;
    public float startDelay = 0.5f; // How long to stay black before fading

    void Start()
    {
        if (blackScreen != null)
        {
            // 1. Force the screen to be solid black immediately
            blackScreen.gameObject.SetActive(true);
            Color c = blackScreen.color;
            c.a = 1f; // 1 = Solid Black
            blackScreen.color = c;

            // 2. Start the fade
            StartCoroutine(FadeInRoutine());
        }
    }

    IEnumerator FadeInRoutine()
    {
        // Optional: Wait a moment in the dark so objects have time to load
        yield return new WaitForSeconds(startDelay);

        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;

            if (blackScreen != null)
            {
                Color c = blackScreen.color;
                c.a = alpha;
                blackScreen.color = c;
            }
            yield return null;
        }

        // 3. Cleanup: Make sure it's perfectly invisible
        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;

            // CRITICAL: Turn off raycast so clicking doesn't hit the invisible UI
            blackScreen.raycastTarget = false;
        }
    }
}