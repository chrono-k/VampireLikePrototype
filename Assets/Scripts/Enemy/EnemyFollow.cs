using UnityEngine;

/// <summary>
/// Constantly moves this body toward a target in the X/Y plane (top-down).
/// Assign <see cref="target"/> from the spawner after spawn, or drag the player in the Inspector for testing.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyFollow : MonoBehaviour
{
    [Tooltip("World units per second toward the target.")]
    public float moveSpeed = 3f;

    [Tooltip("Usually the player. Can be set at runtime by the spawner.")]
    public Transform target;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        if (toTarget.sqrMagnitude < 0.0001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toTarget.normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
}
