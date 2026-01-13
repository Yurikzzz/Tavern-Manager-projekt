using UnityEngine;
using UnityEngine.UI;

public class TimeProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent GameObject that holds the bar (to hide/show the whole thing).")]
    public GameObject visualRoot;

    [Tooltip("The Image component that will fill up. Ensure Image Type is set to 'Filled'.")]
    public Image fillImage;

    void Start()
    {
        UpdateVisibility();
    }

    void Update()
    {
        if (GameTimeManager.Instance == null) return;

        bool isAfternoon = GameTimeManager.Instance.CurrentTime == GameTimeManager.TimeOfDay.Afternoon;

        if (visualRoot != null && visualRoot.activeSelf != isAfternoon)
        {
            visualRoot.SetActive(isAfternoon);
        }

        if (isAfternoon && fillImage != null)
        {
            fillImage.fillAmount = GameTimeManager.Instance.AfternoonProgress;
        }
    }

    private void UpdateVisibility()
    {
        if (GameTimeManager.Instance != null && visualRoot != null)
        {
            bool isAfternoon = GameTimeManager.Instance.CurrentTime == GameTimeManager.TimeOfDay.Afternoon;
            visualRoot.SetActive(isAfternoon);
        }
    }
}