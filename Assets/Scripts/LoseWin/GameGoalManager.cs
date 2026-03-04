using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameGoalManager : MonoBehaviour
{
    public static GameGoalManager Instance { get; private set; }

    [Header("Goal Settings")]
    public int targetDays = 7;
    public int targetPopularity = 50;

    [Header("Scene Routing")]
    public string mainMenuSceneName = "MainMenu";

    [Header("UI References")]
    public GameObject evaluationPanelRoot;
    public RectTransform paperRect;
    public TextMeshProUGUI letterText;
    public GameObject stampVisual;
    public Button stampButton;
    public GameObject stampPromptText;

    [Header("Animation Settings")]
    public float slideDuration = 1.0f;
    public float stampImpactDuration = 0.3f;
    public Vector2 offScreenPosition = new Vector2(0, -1000);
    public Vector2 centerPosition = Vector2.zero;

    [Header("Messages")]
    [Tooltip("Use {0} for Actual Popularity, and {1} for Target Days.")]
    [TextArea(5, 10)]
    public string winMessage = "OFFICIAL NOTICE: CITY HALL\n\n" +
        "To the Tavern Owner,\n\n" +
        "Following your probational period of {1} days, we have concluded our review.\n\n" +
        "You have achieved a Popularity of {0}, exceeding city standards. Your establishment license is hereby fully reinstated.\n\n" +
        "Sign below to acknowledge receipt.";

    [Tooltip("Use {0} for Actual Popularity, and {1} for Target Days.")]
    [TextArea(5, 10)]
    public string loseMessage = "OFFICIAL NOTICE: CITY HALL\n\n" +
        "To the Tavern Owner,\n\n" +
        "Following your probational period of {1} days, we have concluded our review.\n\n" +
        "You have only achieved a Popularity of {0}, failing to meet city standards. You are ordered to close the tavern immediately.\n\n" +
        "Sign below to forfeit ownership.";

    private bool hasEvaluated = false;
    private bool hasStamped = false;
    private bool isWinResult = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (evaluationPanelRoot != null) evaluationPanelRoot.SetActive(false);

        if (GameTimeManager.Instance.CurrentDay > targetDays)
        {
            hasEvaluated = true;
        }
    }

    public void EvaluateGoal(int currentDay)
    {
        if (hasEvaluated) return;

        if (currentDay > targetDays)
        {
            hasEvaluated = true;
            int actualPopularity = PlayerProgress.Instance.Popularity;
            isWinResult = actualPopularity >= targetPopularity;

            StartCoroutine(ShowEvaluationLetter(actualPopularity));
        }
    }

    private IEnumerator ShowEvaluationLetter(int finalPopularity)
    {
        if (evaluationPanelRoot != null) evaluationPanelRoot.SetActive(true);
        if (stampVisual != null) stampVisual.SetActive(false);
        if (stampButton != null) stampButton.interactable = false;
        if (stampPromptText != null) stampPromptText.SetActive(true);

        hasStamped = false;

        if (letterText != null)
        {
            string template = isWinResult ? winMessage : loseMessage;
            letterText.text = string.Format(template, finalPopularity, targetDays);
        }

        if (paperRect != null) paperRect.anchoredPosition = offScreenPosition;

        TimeManager.RequestPause();
        Time.timeScale = 0f;

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

        StartCoroutine(StampAndResolveSequence());
    }

    private IEnumerator StampAndResolveSequence()
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

        if (isWinResult)
        {
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

            if (evaluationPanelRoot != null) evaluationPanelRoot.SetActive(false);

            Time.timeScale = 1f;
            TimeManager.RequestUnpause();
        }
        else
        {
            Time.timeScale = 1f;

            if (SaveManager.instance != null)
            {
                SaveManager.instance.StartNewGame();
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}