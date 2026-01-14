using System;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public static event Action<GameObject> OnNpcDestroyed;

    public float moveSpeed = 2f;
    public Transform exitPoint;

    private Table.Seat targetSeat;
    private Table targetTable;

    private bool isSitting = false;
    private bool isLeaving = false;

    private float groundY;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D npcCollider;
    private int originalSortingOrder;

    void Start()
    {
        groundY = transform.position.y;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null) originalSortingOrder = spriteRenderer.sortingOrder;

        if (npcCollider != null)
            npcCollider.enabled = false;

        FindSeat();

        if (exitPoint == null)
        {
            var tavernDoor = FindObjectOfType<TavernDoor>();
            if (tavernDoor != null)
                exitPoint = tavernDoor.transform;
        }
    }

    void Update()
    {
        if (isLeaving)
        {
            HandleLeaving();
            return;
        }

        if (targetSeat == null || isSitting)
            return;

        Vector3 targetPos = new Vector3(
            targetSeat.seatTransform.position.x,
            groundY,
            transform.position.z
        );

        UpdateAnimation(targetPos);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(transform.position, targetPos);
        if (distance < 0.05f)
        {
            SitDown();
        }
    }

    void FindSeat()
    {
        Table[] tables = FindObjectsOfType<Table>();

        foreach (Table table in tables)
        {
            if (!table.HasFreeSeat)
                continue;

            Table.Seat seat = table.GetFreeSeat();
            if (seat != null)
            {
                targetSeat = seat;
                targetTable = table;
                targetTable.OccupySeat(seat);
                return;
            }
        }

        Debug.Log("NPC: No free seat found.");
    }

    void SitDown()
    {
        isSitting = true;

        transform.position = new Vector3(
            targetSeat.seatTransform.position.x,
            groundY,
            transform.position.z
        );

        animator.SetBool("isWalking", false);
        animator.SetBool("isSitting", true);

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder - 1;
        }

        spriteRenderer.flipX = targetSeat.faceLeft;

        if (npcCollider != null)
            npcCollider.enabled = true;

        NPCOrder npcOrder = GetComponent<NPCOrder>();
        if (npcOrder != null)
            npcOrder.StartOrder();

        var patience = GetComponent<CustomerPatience>();
        if (patience != null)
            patience.SitAndStartWaiting();
    }

    public void StartLeaving()
    {
        if (isLeaving)
            return;

        isLeaving = true;
        isSitting = false;

        if (npcCollider != null)
            npcCollider.enabled = false;

        animator.SetBool("isSitting", false);
        animator.SetBool("isWalking", true);

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }

        if (targetTable != null && targetSeat != null)
        {
            targetTable.FreeSeat(targetSeat);
            targetTable = null;
            targetSeat = null;
        }
    }

    void HandleLeaving()
    {
        if (exitPoint == null)
        {
            Destroy(gameObject, 0.5f);
            return;
        }

        Vector3 targetPos = new Vector3(
            exitPoint.position.x,
            groundY,
            transform.position.z
        );

        UpdateAnimation(targetPos);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector2.Distance(transform.position, targetPos);
        if (distance < 0.05f)
        {
            Destroy(gameObject);
        }
    }

    void UpdateAnimation(Vector3 targetPos)
    {
        float horizontalDelta = targetPos.x - transform.position.x;
        bool isMoving = Mathf.Abs(horizontalDelta) > 0.01f;

        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isSitting", isSitting);

        if (isMoving)
            spriteRenderer.flipX = horizontalDelta < 0;
    }

    void OnDestroy()
    {
        if (targetTable != null && targetSeat != null)
        {
            targetTable.FreeSeat(targetSeat);
        }

        OnNpcDestroyed?.Invoke(gameObject);
    }
}