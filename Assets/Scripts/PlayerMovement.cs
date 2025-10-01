using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player

    private Rigidbody2D rb;
    private float moveInput; // single float for left/right

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get horizontal input only (A/D or Left/Right arrows)
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        // Move player only along X axis
        rb.MovePosition(rb.position + new Vector2(moveInput, 0f) * moveSpeed * Time.fixedDeltaTime);
    }
}
