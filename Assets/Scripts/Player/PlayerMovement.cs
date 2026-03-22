using UnityEngine;

/// <summary>
/// Top-down movement via Rigidbody2D. Reads input in Update, applies velocity in FixedUpdate.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("World units per second when moving.")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Top-down: no gravity pull on the body.
        rb.gravityScale = 0f;
    }

    void Update()
    {
        // Horizontal = A/D + Left/Right; Vertical = W/S + Up/Down (Input Manager defaults).
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        if (moveInput.sqrMagnitude > 0.0001f)
        {
            // Normalize so diagonals are not faster than cardinals.
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
