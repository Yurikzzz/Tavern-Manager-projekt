using System;
using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    public Dish carriedDish;  
    public bool HasDish => carriedDish != null;

    public event Action<Dish> OnCarryChanged;

    public void TakeDish(Dish dish)
    {
        carriedDish = dish;
        Debug.Log($"Player is now carrying: {dish.displayName}");
        OnCarryChanged?.Invoke(carriedDish);
    }

    public void ClearDish()
    {
        if (carriedDish != null)
            Debug.Log($"Player dropped/used: {carriedDish.displayName}");

        carriedDish = null;
        OnCarryChanged?.Invoke(null);
    }

    public void Drop()
    {
        if (!HasDish)
        {
            Debug.Log("PlayerCarry.Drop(): no dish to drop.");
            return;
        }

        ClearDish();
    }
}
