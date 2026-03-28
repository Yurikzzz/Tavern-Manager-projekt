using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("How far to move everything down (in pixels)")]
    public float shiftAmount = 2f;

    private Button myButton;

    private RectTransform[] childRects;
    private Vector2[] originalPositions;

    void Start()
    {
        myButton = GetComponent<Button>();

        int childCount = transform.childCount;
        childRects = new RectTransform[childCount];
        originalPositions = new Vector2[childCount];

        for (int i = 0; i < childCount; i++)
        {
            childRects[i] = transform.GetChild(i).GetComponent<RectTransform>();
            if (childRects[i] != null)
            {
                originalPositions[i] = childRects[i].anchoredPosition;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (myButton != null && !myButton.interactable) return;

        for (int i = 0; i < childRects.Length; i++)
        {
            if (childRects[i] != null)
            {
                childRects[i].anchoredPosition = new Vector2(originalPositions[i].x, originalPositions[i].y - shiftAmount);
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        RestorePositions();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestorePositions();
    }

    private void RestorePositions()
    {
        for (int i = 0; i < childRects.Length; i++)
        {
            if (childRects[i] != null)
            {
                childRects[i].anchoredPosition = originalPositions[i];
            }
        }
    }
}