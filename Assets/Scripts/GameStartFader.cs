using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStartFader : MonoBehaviour
{
    public Image blackScreen;
    public float fadeSpeed = 1f;
    public float startDelay = 0.5f;

    void Start()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            Color c = blackScreen.color;
            c.a = 1f;
            blackScreen.color = c;

            StartCoroutine(FadeInRoutine());
        }
    }

    IEnumerator FadeInRoutine()
    {
        // 1. REVERTED: Now uses standard time. If the game is paused, this timer pauses too!
        yield return new WaitForSeconds(startDelay);

        float alpha = 1f;
        while (alpha > 0)
        {
            // 2. REVERTED: Now uses Time.deltaTime. 
            // If the game is paused (timeScale = 0), alpha stops changing.
            alpha -= Time.deltaTime * fadeSpeed;

            if (blackScreen != null)
            {
                Color c = blackScreen.color;
                c.a = alpha;
                blackScreen.color = c;
            }
            yield return null;
        }

        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
            blackScreen.raycastTarget = false;
        }
    }
}