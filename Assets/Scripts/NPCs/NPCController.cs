using System;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public static event Action<GameObject> OnNpcDestroyed;

    public float moveSpeed = 2f;

    public Transform exitPoint;

    private Transform targetSeat;
    private Table targetTable;
    private bool isSitting = false;
    private bool isLeaving = false;


    private float groundY;

    void Start()
    {
        groundY = transform.position.y;

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

        NPCOrder npcOrder = GetComponent<NPCOrder>();
        if (npcOrder != null)
        {
            npcOrder.StartOrder();
        }

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

        if (targetTable != null && targetSeat != null)
        {
            targetTable.FreeSeat(targetSeat);
            targetTable = null;
            targetSeat = null;
        }

        Debug.Log(name + " is leaving the tavern.");
    }

    private void HandleLeaving()
    {
        if (exitPoint == null)
        {
            var tavernDoor = FindObjectOfType<TavernDoor>();
            if (tavernDoor != null)
            {
                exitPoint = tavernDoor.transform;
            }
            else
            {
                Debug.LogWarning($"{name}: exitPoint not set — destroying in 0.5s. Assign exitPoint on the NPC prefab or set a TavernDoor in the scene.");
                Destroy(gameObject, 0.5f);
                return;
            }
        }

        Vector3 targetPos = new Vector3(
            exitPoint.position.x,
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
            Destroy(gameObject);
        }
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