using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Room Status")]
    public bool isClean = true;
    public bool isOccupied = false;
    public int tasksRemaining = 0;

    [Header("Configuration")]
    public Transform guestEntrance;
    public TeleportDoor roomDoor;
    public List<CleaningTask> allTasks = new List<CleaningTask>();

    [Header("Door Checkmark (optional)")]
    [Tooltip("Sprite used as checkmark shown on/near the door when the room is clean.")]
    public Sprite checkmarkSprite;
    [Tooltip("Optional empty Transform to position the checkmark. If assigned, the checkmark will be parented to this transform and placed at its origin.")]
    public Transform checkmarkAnchor;
    [Tooltip("Local offset relative to the door (used only when no anchor is assigned).")]
    public Vector3 checkmarkLocalPosition = new Vector3(0f, 1.2f, 0f);
    [Tooltip("Sorting order for the created SpriteRenderer (higher = drawn on top).")]
    public int checkmarkSortingOrder = 50;
    [Tooltip("Optional sorting layer name for the checkmark sprite.")]
    public string checkmarkSortingLayer = "Default";

    private GameObject checkmarkGO;
    private SpriteRenderer checkmarkRenderer;

    private bool wasCleanWhenRented = false;

    void Start()
    {
        if (allTasks.Count == 0)
        {
            CleaningTask[] tasksFound = GetComponentsInChildren<CleaningTask>(true);
            allTasks.AddRange(tasksFound);
        }

        if (RentalManager.Instance != null)
        {
            RentalManager.Instance.RegisterRoom(this);
        }

        if (roomDoor != null && !isOccupied)
        {
            roomDoor.SetLocked(false);
        }

        GenerateMorningMess();

        // Ensure checkmark exists and reflects current state
        InitializeCheckmarkIfNeeded();
        UpdateCheckmarkVisibility();
    }

    public void TaskCompleted()
    {
        tasksRemaining--;
        if (tasksRemaining <= 0)
        {
            tasksRemaining = 0;
            isClean = true;
            UpdateCheckmarkVisibility();
        }
    }

    public void OccupyRoom()
    {
        isOccupied = true;
        wasCleanWhenRented = isClean;

        if (roomDoor != null)
        {
            roomDoor.SetLocked(true);
        }
    }

    public void CalculateNightlyRewards(out int coins, out int popularity)
    {
        coins = 25;
        popularity = wasCleanWhenRented ? 10 : 5;
    }

    public void CheckOutAndReset()
    {
        if (isOccupied)
        {
            isOccupied = false;

            if (roomDoor != null)
            {
                roomDoor.SetLocked(false);
            }

            GenerateMorningMess();
            UpdateCheckmarkVisibility();
        }
    }

    public void GenerateMorningMess()
    {
        isClean = false;
        tasksRemaining = 0;

        foreach (CleaningTask task in allTasks)
        {
            if (task.type == CleaningTask.TaskType.Bed)
            {
                task.ResetTask();
                tasksRemaining++;
            }
            else
            {
                bool spawnTrash = Random.Range(0, 2) == 0;
                if (spawnTrash)
                {
                    task.ResetTask();
                    tasksRemaining++;
                }
                else
                {
                    task.gameObject.SetActive(false);
                }
            }
        }

        // Make sure checkmark is hidden after mess generation
        UpdateCheckmarkVisibility();
    }

    // --- Checkmark helpers ------------------------------------------------
    private void InitializeCheckmarkIfNeeded()
    {
        if (checkmarkSprite == null) return; // nothing to do without a sprite
        if (checkmarkRenderer != null) return; // already created

        // If an explicit anchor was provided, use it as parent and position origin.
        // Otherwise parent to the door (if any) or to the room and use the fallback local offset.
        Transform parent = (checkmarkAnchor != null) ? checkmarkAnchor : ((roomDoor != null) ? roomDoor.transform : this.transform);

        checkmarkGO = new GameObject("RoomCheckmark");
        checkmarkGO.transform.SetParent(parent, false);

        if (checkmarkAnchor != null)
        {
            // anchor provided: put at anchor origin
            checkmarkGO.transform.localPosition = Vector3.zero;
        }
        else
        {
            // no anchor: use fallback offset relative to parent
            checkmarkGO.transform.localPosition = checkmarkLocalPosition;
        }

        checkmarkGO.transform.localRotation = Quaternion.identity;
        checkmarkGO.transform.localScale = Vector3.one;

        checkmarkRenderer = checkmarkGO.AddComponent<SpriteRenderer>();
        checkmarkRenderer.sprite = checkmarkSprite;
        checkmarkRenderer.sortingOrder = checkmarkSortingOrder;
        if (!string.IsNullOrEmpty(checkmarkSortingLayer))
            checkmarkRenderer.sortingLayerName = checkmarkSortingLayer;

        // default visibility follows current isClean
        checkmarkRenderer.enabled = isClean;
    }

    private void UpdateCheckmarkVisibility()
    {
        // Lazily create the checkmark if a sprite was assigned
        InitializeCheckmarkIfNeeded();

        if (checkmarkRenderer != null)
        {
            checkmarkRenderer.enabled = isClean;
        }
    }
}