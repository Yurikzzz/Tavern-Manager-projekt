using UnityEngine;

public class NPCInteractable : Interactable
{
    private NPCController npcController; 

    void Awake()
    {
        npcController = GetComponent<NPCController>();
    }

    public override void Interact()
    {
        Debug.Log("Interacted with NPC: " + gameObject.name);
    }
}
