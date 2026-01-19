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
    }

    public void TaskCompleted()
    {
        tasksRemaining--;
        if (tasksRemaining <= 0)
        {
            tasksRemaining = 0;
            isClean = true;
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
    }
}