using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactableMask;
    public GameObject ePrompt;

    private Interactable currentTarget;

    void Update()
    {
        CheckForInteractable();

        if (currentTarget != null)
        {
            ePrompt.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentTarget.Interact();
            }
        }
        else
        {
            ePrompt.SetActive(false);
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

