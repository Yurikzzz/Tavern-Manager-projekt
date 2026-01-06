using UnityEngine;
using System.Collections;

public class RentalGuestController : Interactable
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform barPosition;
    public Transform exitPosition;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer sr;
    private string movingParam = "isWalking";

    private RoomManager assignedRoom;
    private bool hasRoom = false;
    private bool isLeaving = false;
    private bool isMoving = false;
    private Vector3 currentTarget;
    private Collider2D interactionCollider;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        interactionCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (interactionCollider != null) interactionCollider.enabled = false;
        HidePrompt();

        if (barPosition != null)
        {
            MoveTo(barPosition.position);
        }
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

            if (sr != null)
            {
                if (currentTarget.x < transform.position.x) sr.flipX = true;
                else if (currentTarget.x > transform.position.x) sr.flipX = false;
            }

            if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
            {
                isMoving = false;
                transform.position = currentTarget;
                OnDestinationReached();
            }
        }

        if (animator != null)
        {
            animator.SetBool(movingParam, isMoving);
        }
    }

    void MoveTo(Vector3 target)
    {
        currentTarget = target;
        isMoving = true;
    }

    void OnDestinationReached()
    {
        if (isLeaving)
        {
            Destroy(gameObject);
            return;
        }

        if (!hasRoom)
        {
            if (interactionCollider != null) interactionCollider.enabled = true;
        }
        else
        {
            if (assignedRoom != null) assignedRoom.OccupyRoom();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasRoom && !isLeaving)
        {
            ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HidePrompt();
        }
    }

    public override void Interact()
    {
        assignedRoom = RentalManager.Instance.GetAvailableRoom();

        if (assignedRoom != null)
        {
            hasRoom = true;
            HidePrompt();
            if (interactionCollider != null) interactionCollider.enabled = false;

            Debug.Log("Guest: Thanks! Heading to room.");

            if (assignedRoom.guestEntrance != null)
                MoveTo(assignedRoom.guestEntrance.position);
            else
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("Guest: Oh, full? Okay, bye.");
            LeaveTavern();
        }
    }

    void LeaveTavern()
    {
        HidePrompt();

        if (interactionCollider != null) interactionCollider.enabled = false;

        isLeaving = true;

        if (exitPosition != null)
        {
            MoveTo(exitPosition.position);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}