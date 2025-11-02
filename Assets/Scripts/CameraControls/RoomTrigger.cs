using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField]
    public float targetZoom;
    public Transform focus;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var camController = Camera.main.GetComponent<RoomCameraController>();
            camController.MoveToRoom(focus, targetZoom);
        }
    }
}