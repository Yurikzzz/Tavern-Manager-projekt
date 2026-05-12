using UnityEngine;
using UnityEngine.EventSystems; 

public class ButtonSound : MonoBehaviour, IPointerDownHandler
{
    [Header("Drop the sound for this button here!")]
    public AudioClip mySound;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SFXManager.instance != null && mySound != null)
        {
            SFXManager.instance.PlaySound(mySound);
        }
    }
}