using UnityEngine;

/// <summary>
/// Damages the player while this enemy's trigger collider overlaps them.
/// Put this on the enemy root next to <see cref="EnemyFollow"/>; mark the enemy's Collider2D as <b>Is Trigger</b>.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    [Tooltip("Hit points removed each time the cooldown allows.")]
    [SerializeField] private int damage = 1;

    [Tooltip("Seconds between damage ticks while still overlapping the player.")]
    [SerializeField] private float damageCooldown = 0.5f;

    private float lastDamageTime = float.NegativeInfinity;

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
            return;

        if (Time.time - lastDamageTime < damageCooldown)
            return;

        lastDamageTime = Time.time;
        health.TakeDamage(damage);
    }
}
