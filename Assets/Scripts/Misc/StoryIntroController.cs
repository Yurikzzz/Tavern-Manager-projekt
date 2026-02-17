using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StoryIntroController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject introPanelRoot;   
    public RectTransform paperRect;        
    public TextMeshProUGUI storyText;     
    public GameObject stampVisual;      
    public Button stampButton;          

    [Header("QoL")]
    public GameObject stampPromptText;    

    [Header("Animation Settings")]
    public float slideDuration = 1.0f;     
    public float stampImpactDuration = 0.3f;
    public Vector2 offScreenPosition = new Vector2(0, -1000);
    public Vector2 centerPosition = Vector2.zero;

    [Header("Story Settings")]
    [TextArea(5, 10)]
    public string letterContent = "OFFICIAL NOTICE: CITY HALL\n\n" +
        "To the Tavern Owner,\n\n" +
        "Your establishment is failing to meet city standards. " +
        "You are hereby granted a probational period of 7 days.\n\n" +
        "Improve your Popularity to 50 or forfeit ownership immediately.\n\n" +
        "Sign below to acknowledge receipt.";

    private bool hasStamped = false;

    IEnumerator Start()
    {
        if (paperRect != null) paperRect.anchoredPosition = offScreenPosition;

        yield return null;

        if (stampPromptText != null) stampPromptText.SetActive(false);

        if (GameTimeManager.Instance != null && GameTimeManager.Instance.CurrentDay == 1)
        {
            if (introPanelRoot != null) introPanelRoot.SetActive(true);
            StartCoroutine(StartIntroSequence());
        }
        else
        {
            if (introPanelRoot != null) introPanelRoot.SetActive(false);
        }
    }

    private IEnumerator StartIntroSequence()
    {
        if (stampVisual != null) stampVisual.SetActive(false);
        if (storyText != null) storyText.text = letterContent;
        if (stampButton != null) stampButton.interactable = false;

        if (stampPromptText != null) stampPromptText.SetActive(true);

        TimeManager.RequestPause();

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            t = t * t * (3f - 2f * t);

            if (paperRect != null)
                paperRect.anchoredPosition = Vector2.Lerp(offScreenPosition, centerPosition, t);

            yield return null;
        }

        if (paperRect != null) paperRect.anchoredPosition = centerPosition;
        if (stampButton != null) stampButton.interactable = true;
    }

    public void OnStampClicked()
    {
        if (hasStamped) return;
        hasStamped = true;

        if (stampPromptText != null) stampPromptText.SetActive(false);

        StartCoroutine(StampAnimationSequence());
    }

    private IEnumerator StampAnimationSequence()
    {
        if (stampVisual != null) stampVisual.SetActive(true);
        if (stampButton != null) stampButton.interactable = false;

        Transform stampTrans = stampVisual.transform;
        Vector3 startScale = new Vector3(1.5f, 1.5f, 1f);
        Vector3 endScale = Vector3.one;

        float elapsed = 0f;
        while (elapsed < stampImpactDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / stampImpactDuration;
            stampTrans.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        stampTrans.localScale = endScale;

        yield return new WaitForSecondsRealtime(0.8f);

        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            t = t * t * (3f - 2f * t);

            if (paperRect != null)
                paperRect.anchoredPosition = Vector2.Lerp(centerPosition, offScreenPosition, t);

            yield return null;
        }

        if (introPanelRoot != null) introPanelRoot.SetActive(false);
        TimeManager.RequestUnpause();
    }
}