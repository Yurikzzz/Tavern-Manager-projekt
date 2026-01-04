using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [System.Serializable]
    public class Seat
    {
        public Transform seatTransform;
        public bool faceLeft;
    }

    public Seat[] seats;
    private List<Seat> occupiedSeats = new List<Seat>();

    public bool HasFreeSeat => occupiedSeats.Count < seats.Length;

    public Seat GetFreeSeat()
    {
        foreach (var seat in seats)
        {
            if (!occupiedSeats.Contains(seat))
                return seat;
        }
        return null;
    }

    public void OccupySeat(Seat seat)
    {
        if (!occupiedSeats.Contains(seat))
            occupiedSeats.Add(seat);
    }

    public void FreeSeat(Seat seat)
    {
        occupiedSeats.Remove(seat);
    }
}
