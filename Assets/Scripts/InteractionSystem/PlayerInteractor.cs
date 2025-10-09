using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactableMask;

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
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableMask);

            Interactable closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                Interactable interactable = hit.GetComponent<Interactable>();
                if (interactable != null)
                {
                    float dist = Vector2.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }
            currentTarget = closest;
        }
    }
}

