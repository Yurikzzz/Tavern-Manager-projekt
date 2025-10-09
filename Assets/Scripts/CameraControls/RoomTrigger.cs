using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField]
    public float targetZoom;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var camController = Camera.main.GetComponent<RoomCameraController>();
            camController.MoveToRoom(transform, targetZoom);
        }
    }
}