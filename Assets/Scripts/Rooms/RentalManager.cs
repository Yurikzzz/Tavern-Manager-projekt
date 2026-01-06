using UnityEngine;
using System.Collections.Generic;

public class RentalManager : MonoBehaviour
{
    public static RentalManager Instance;
    private List<RoomManager> rooms = new List<RoomManager>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    public void RegisterRoom(RoomManager room)
    {
        if (!rooms.Contains(room)) rooms.Add(room);
    }

    public RoomManager GetAvailableRoom()
    {
        foreach (RoomManager room in rooms)
        {
            if (!room.isOccupied) return room;
        }
        return null;
    }

    public void ProcessNightlyRentals(out int totalCoins, out int totalPop)
    {
        totalCoins = 0;
        totalPop = 0;

        foreach (RoomManager room in rooms)
        {
            if (room.isOccupied)
            {
                int c, p;
                room.CalculateNightlyRewards(out c, out p);
                totalCoins += c;
                totalPop += p;
            }

            room.CheckOutAndReset();
        }

        Debug.Log($"RENTAL MANAGER: Handing over {totalCoins} coins and {totalPop} popularity.");
    }
}