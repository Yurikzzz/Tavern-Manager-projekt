using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f; 
    public float runSpeed = 6f;  

    public Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (moveInput > 0)
            spriteRenderer.flipX = true;
        else if (moveInput < 0)
            spriteRenderer.flipX = false;

        animator.SetFloat("Speed", Mathf.Abs(moveInput * currentSpeed)); 
        animator.SetBool("isRunning", isRunning);
    }

    void FixedUpdate()
    {
        Vector2 newPosition = rb.position + new Vector2(moveInput, 0f) * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}

