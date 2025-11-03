using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float moveSpeed = 2f;

    private Transform targetSeat;
    private Table targetTable;
    private bool isSitting = false;

    private float groundY;

    void Start()
    {
        groundY = transform.position.y;

        FindSeat();
    }

    void Update()
    {
        if (targetSeat == null || isSitting)
            return;

        Vector3 targetPos = new Vector3(
            targetSeat.position.x,
            groundY,               
            transform.position.z
        );

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

            Transform seat = table.GetFreeSeat();
            if (seat != null)
            {
                targetSeat = seat;
                targetTable = table;
                targetTable.OccupySeat(targetSeat);
                return;
            }
        }

        Debug.Log("NPC: No free seat found, just standing around.");
    }

    void SitDown()
    {
        isSitting = true;

        transform.position = new Vector3(
            targetSeat.position.x,
            groundY,               
            transform.position.z
        );

        // Later: play sitting animation, change sprite, etc.
    }

    void OnDestroy()
    {
        if (targetTable != null && targetSeat != null)
        {
            targetTable.FreeSeat(targetSeat);
        }
    }
}
