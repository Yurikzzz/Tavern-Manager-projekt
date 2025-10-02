using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactableMask;
    public GameObject ePrompt;

    private Interactable currentTarget;
    private Interactable lastTarget;

    void Update()
    {
        CheckForInteractable();

        if (currentTarget != null)
        {
            if (lastTarget != currentTarget)
            {
                if (lastTarget != null) lastTarget.HidePrompt();
                currentTarget.ShowPrompt();
                lastTarget = currentTarget;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                currentTarget.Interact();
            }
        }
        else
        {
            if (lastTarget != null)
            {
                lastTarget.HidePrompt();
                lastTarget = null;
            }
        }

        void CheckForInteractable()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRange, interactableMask);

            if (hit != null)
            {
                currentTarget = hit.GetComponent<Interactable>();
            }
            else
            {
                currentTarget = null;
            }
        }
    }
}

