using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class DayNightLight : MonoBehaviour
{
    private Light2D targetLight;
    private Color originalColor;
    private Color nightColor;

    [Header("Color Settings")]
    [Tooltip("The hex color to switch to at night. Must include the # symbol.")]
    public string nightColorHex = "#3D62FF";

    void Start()
    {
        targetLight = GetComponent<Light2D>();

        originalColor = targetLight.color;

        if (!ColorUtility.TryParseHtmlString(nightColorHex, out nightColor))
        {
            Debug.LogWarning("Invalid hex color code! Defaulting to blue.");
            nightColor = Color.blue;
        }

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTimeChanged += HandleTimeChange;

            HandleTimeChange(GameTimeManager.Instance.CurrentTime);
        }
        else
        {
            Debug.LogWarning("DayNightLight couldn't find GameTimeManager.Instance!");
        }
    }

    private void HandleTimeChange(GameTimeManager.TimeOfDay newTime)
    {
        if (newTime == GameTimeManager.TimeOfDay.Night)
        {
            targetLight.color = nightColor;
        }
        else
        {
            targetLight.color = originalColor;
        }
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTimeChanged -= HandleTimeChange;
        }
    }
}