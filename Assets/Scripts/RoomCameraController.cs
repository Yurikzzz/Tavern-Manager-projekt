using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    public Transform currentTarget;      
    public float moveSpeed = 3f;         

    void Update()
    {
        if (currentTarget != null)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(currentTarget.position.x, currentTarget.position.y, transform.position.z),
                Time.deltaTime * moveSpeed
            );
        }
    }

    public void MoveToRoom(Transform newTarget)
    {
        currentTarget = newTarget;
    }
}

