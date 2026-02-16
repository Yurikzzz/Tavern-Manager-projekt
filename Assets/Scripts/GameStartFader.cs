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
        yield return new WaitForSecondsRealtime(startDelay);

        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.unscaledDeltaTime * fadeSpeed;

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