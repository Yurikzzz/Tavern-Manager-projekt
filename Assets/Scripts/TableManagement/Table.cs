using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public Transform[] seats;
    private List<Transform> occupiedSeats = new List<Transform>();

    public bool HasFreeSeat => occupiedSeats.Count < seats.Length;

    public Transform GetFreeSeat()
    {
        foreach (var seat in seats)
        {
            if (!occupiedSeats.Contains(seat))
                return seat;
        }
        return null;
    }

    public void OccupySeat(Transform seat)
    {
        if (!occupiedSeats.Contains(seat))
            occupiedSeats.Add(seat);
    }

    public void FreeSeat(Transform seat)
    {
        occupiedSeats.Remove(seat);
    }
}

