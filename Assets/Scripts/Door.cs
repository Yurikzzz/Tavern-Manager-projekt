using UnityEngine;

public class Door : Interactable
{
    public override void Interact()
    {
        Debug.Log("You have interacted with the door.");
    }
}
