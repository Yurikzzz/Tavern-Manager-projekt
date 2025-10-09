using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    public Transform currentTarget;      
    public float moveSpeed = 3f;
    public float zoomSpeed = 3f;

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        if(currentTarget != null)
        {
            // Smoothly move toward target position
            Vector3 targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
        }

        // Smoothly interpolate zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
    }

    public void MoveToRoom(Transform newTarget, float newZoom)
    {
        currentTarget = newTarget;
        targetZoom = newZoom;
    }
}

