using UnityEngine;

public class NPCInteractable : Interactable
{
    private NPCOrder npcOrder;
    private PlayerCarry playerCarry;

    void Awake()
    {
        npcOrder = GetComponent<NPCOrder>();
        playerCarry = FindObjectOfType<PlayerCarry>();
    }

    public override void Interact()
    {
        if (npcOrder == null || playerCarry == null)
            return;

        if (!playerCarry.HasDish)
        {
            Debug.Log($"{name}: Player has nothing to serve.");
            return;
        }

        bool success = npcOrder.TryServe(playerCarry.carriedDish);

        if (success)
        {
            Debug.Log($"{name}: Thanks for the {playerCarry.carriedDish.displayName}!");
            playerCarry.ClearDish();

        }
        else
        {
            Debug.Log($"{name}: That’s not what I ordered.");
        }
    }
}
