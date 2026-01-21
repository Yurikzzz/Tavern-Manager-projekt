using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [Header("Settings")]
    public Transform focus;
    public RoomCameraController.ResolutionType targetResolution;

    [Tooltip("Only for main rooms.")]
    public Collider2D roomBounds;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var camController = Camera.main.GetComponent<RoomCameraController>();
            if (camController != null)
            {
                camController.MoveToRoom(focus, targetResolution, roomBounds);
            }
        }
    }
}