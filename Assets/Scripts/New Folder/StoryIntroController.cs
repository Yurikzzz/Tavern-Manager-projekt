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

    [Header("Animation Settings")]
    public float slideDuration = 0.8f;
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

    void Start()
    {
        Debug.Log("StoryIntroController Started.");

        if (GameTimeManager.Instance == null)
        {
            Debug.LogError("GameTimeManager not found! Make sure it is in the scene.");
            return;
        }

        Debug.Log($"Current Day is: {GameTimeManager.Instance.CurrentDay}");

        if (GameTimeManager.Instance.CurrentDay == 1)
        {
            Debug.Log("Day 1 detected. Starting Intro...");
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
        Time.timeScale = 0f;

        if (stampVisual != null) stampVisual.SetActive(false);

        if (storyText != null) storyText.text = letterContent;

        if (paperRect != null) paperRect.anchoredPosition = offScreenPosition;

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
        Time.timeScale = 1f; 
    }
}