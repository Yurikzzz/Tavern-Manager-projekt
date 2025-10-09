using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public GameObject promptUI;

    void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public abstract void Interact();

    public virtual void ShowPrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(true);
    }

    public virtual void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}


