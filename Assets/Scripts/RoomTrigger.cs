using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public Transform cameraTargetInside;
    public Transform cameraTargetOutside;
    public BoxCollider2D roomCollider;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Camera.main.GetComponent<RoomCameraController>().MoveToRoom(cameraTargetInside);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Camera.main.GetComponent<RoomCameraController>().MoveToRoom(cameraTargetOutside);
    }
}