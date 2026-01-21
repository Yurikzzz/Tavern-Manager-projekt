using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomCameraController : MonoBehaviour
{
    [Header("Dependencies")]
    public Transform playerTransform; 

    private PixelPerfectCamera pixelCam;
    private Camera standardCam;

    private Transform staticTarget; 
    private Collider2D roomBounds; 
    private bool isFollowingPlayer = false;

    public enum ResolutionType
    {
        WideRoom_640x360,
        SmallRoom_480x270
    }

    void Awake()
    {
        pixelCam = GetComponent<PixelPerfectCamera>();
        standardCam = GetComponent<Camera>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    void LateUpdate()
    {
        if (isFollowingPlayer && playerTransform != null && roomBounds != null)
        {
            float cameraHalfHeight = (float)pixelCam.refResolutionY / (pixelCam.assetsPPU * 2f);
            float cameraHalfWidth = (float)pixelCam.refResolutionX / (pixelCam.assetsPPU * 2f);

            Bounds b = roomBounds.bounds;

            float minX = b.min.x + cameraHalfWidth;
            float maxX = b.max.x - cameraHalfWidth;
            float minY = b.min.y + cameraHalfHeight;
            float maxY = b.max.y - cameraHalfHeight;

            Vector3 newPos = playerTransform.position;

            float clampedX = Mathf.Clamp(newPos.x, minX, Mathf.Max(minX, maxX));
            float clampedY = Mathf.Clamp(newPos.y, minY, Mathf.Max(minY, maxY));

            transform.position = new Vector3(clampedX, clampedY, -10f);
        }
        else if (staticTarget != null)
        {
            transform.position = new Vector3(staticTarget.position.x, staticTarget.position.y, -10f);
        }
    }

    public void MoveToRoom(Transform centerTarget, ResolutionType resType, Collider2D bounds)
    {
        SnapToResolution(resType);

        if (bounds != null)
        {
            roomBounds = bounds;
            isFollowingPlayer = true;
            staticTarget = null;
        }
        else
        {
            staticTarget = centerTarget;
            isFollowingPlayer = false;
            roomBounds = null;

            if (centerTarget != null)
                transform.position = new Vector3(centerTarget.position.x, centerTarget.position.y, -10f);
        }
    }

    private void SnapToResolution(ResolutionType type)
    {
        if (pixelCam == null) return;

        switch (type)
        {
            case ResolutionType.WideRoom_640x360:
                pixelCam.refResolutionX = 640;
                pixelCam.refResolutionY = 360;
                break;

            case ResolutionType.SmallRoom_480x270:
                pixelCam.refResolutionX = 480;
                pixelCam.refResolutionY = 270;
                break;
        }

        pixelCam.enabled = false;
        pixelCam.enabled = true;
    }
}