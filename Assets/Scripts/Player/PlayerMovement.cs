using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Top-down movement via Rigidbody2D. Reads input in Update, applies velocity in FixedUpdate.
/// Uses the Input System package (not the legacy Input Manager).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Tooltip("World units per second when moving.")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private InputAction moveAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Top-down: no gravity pull on the body.
        rb.gravityScale = 0f;

        // Vector2 action: two separate 2DVector composites so WASD and arrow keys both work.
        moveAction = new InputAction(
            name: "Move",
            type: InputActionType.Value,
            binding: null,
            interactions: null,
            processors: null,
            expectedControlType: "Vector2");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
    }

    void OnEnable()
    {
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void OnDestroy()
    {
        moveAction?.Dispose();
    }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
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
