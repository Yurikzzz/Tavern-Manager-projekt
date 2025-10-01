using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f; 
    public float runSpeed = 6f;  

    private Rigidbody2D rb;
    private float moveInput;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + new Vector2(moveInput, 0f) * currentSpeed * Time.fixedDeltaTime);
    }
}

