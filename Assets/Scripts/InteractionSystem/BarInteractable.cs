using UnityEngine;

public class BarInteractable : Interactable
{
    public BarUIController barUI;   

    public override void Interact()
    {
        barUI.Toggle();
    }
}
