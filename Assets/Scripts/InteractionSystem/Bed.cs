using UnityEngine;

public class Bed : Interactable
{
    public override void Interact()
    {
        Sleep();
    }

    private void Sleep()
    {
        Debug.Log("Player is sleeping...");

        GameTimeManager.Instance.NextDay();
    }
}
