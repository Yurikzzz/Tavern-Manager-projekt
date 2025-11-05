using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    public Dish carriedDish;  
    public bool HasDish => carriedDish != null;

    public void TakeDish(Dish dish)
    {
        carriedDish = dish;
        Debug.Log($"Player is now carrying: {dish.displayName}");
    }

    public void ClearDish()
    {
        if (carriedDish != null)
            Debug.Log($"Player dropped/used: {carriedDish.displayName}");

        carriedDish = null;
    }
}
