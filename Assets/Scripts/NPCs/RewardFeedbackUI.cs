using UnityEngine;
using TMPro;
using System.Collections;

public class RewardFeedbackUI : MonoBehaviour
{
    [Header("Data References")]
    [Tooltip("The TextMeshPro component for the Coin amount")]
    public TMP_Text coinText;
    [Tooltip("Coin sprite.")]
    public GameObject coinIconObject;

    [Tooltip("The TextMeshPro component for the Popularity amount")]
    public TMP_Text popularityText;
    [Tooltip("Popularity sprite.")]
    public GameObject popularityIconObject;

    [Header("Animation Settings")]
    public float displayDuration = 2.0f;
    public float floatSpeed = 0.5f;
    public Vector3 moveDirection = Vector3.up;

    private Vector3 initialLocalPos;
    private TMP_Text[] childTexts;
    private SpriteRenderer[] childSprites;

    private void Awake()
    {
        initialLocalPos = transform.localPosition;

        childTexts = GetComponentsInChildren<TMP_Text>();
        childSprites = GetComponentsInChildren<SpriteRenderer>();

        gameObject.SetActive(false);
    }

    public void ShowReward(int coins, int popularity)
    {
        bool showCoins = (coins != 0);

        if (coinText != null)
        {
            coinText.gameObject.SetActive(showCoins);
            coinText.text = (coins > 0 ? "+" : "") + coins;
        }

        if (coinIconObject != null)
        {
            coinIconObject.SetActive(showCoins);
        }

        bool showPopularity = (popularity != 0);

        if (popularityText != null)
        {
            popularityText.gameObject.SetActive(showPopularity);
            popularityText.text = (popularity > 0 ? "+" : "") + popularity;
        }

        if (popularityIconObject != null)
        {
            popularityIconObject.SetActive(showPopularity);
        }

        gameObject.SetActive(true);

        transform.localPosition = initialLocalPos;
        SetAlpha(1f);

        StopAllCoroutines();
        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        float timer = 0f;

        while (timer < displayDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / displayDuration;

            transform.localPosition += moveDirection * floatSpeed * Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, progress);
            SetAlpha(alpha);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        if (childTexts != null)
        {
            foreach (var txt in childTexts)
                txt.alpha = alpha;
        }

        if (childSprites != null)
        {
            foreach (var sr in childSprites)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }
}