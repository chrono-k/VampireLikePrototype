using UnityEngine;

/// <summary>
/// Simple top-down movement using Rigidbody2D.
/// Add this to your player, assign a Rigidbody2D (or let RequireComponent add one).
/// Set the Rigidbody2D Gravity Scale to 0 for classic top-down feel.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    // How fast the player moves in units per second (tweak in the Inspector).
    public float moveSpeed = 5f;

    private Rigidbody2D rb;

    // Latest input from the keyboard; updated every frame in Update().
    private Vector2 moveInput;

    void Awake()
    {
        // Cache the Rigidbody2D so we don't call GetComponent every physics step.
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Built-in axes: Horizontal = A/D + Left/Right, Vertical = W/S + Up/Down.
        // GetAxisRaw returns -1, 0, or 1 with no smoothing — good for snappy movement.
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // Physics runs on a fixed timestep, so we change velocity here.
        Vector2 velocity;

        if (moveInput.sqrMagnitude > 0.0001f)
        {
            // Without normalizing, diagonals would be faster (√2×) because (1,1) is longer than (1,0).
            velocity = moveInput.normalized * moveSpeed;
        }
        else
        {
            // No keys held — stop so we don't keep sliding.
            velocity = Vector2.zero;
        }

        rb.linearVelocity = velocity;
    }
}
