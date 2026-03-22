using System;
using UnityEngine;

/// <summary>
/// Simple HP for the player. Enemies damage it through <see cref="TakeDamage"/> (e.g. trigger contact).
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Maximum hit points; current health starts here in Awake.")]
    public int maxHealth = 10;

    [Tooltip("Current hit points (reset from maxHealth when the scene loads).")]
    public int currentHealth;

    /// <summary>Fires once when health first reaches 0.</summary>
    public event Action Died;

    private bool isDead;

    /// <summary>True after the player has died; read-only for other scripts.</summary>
    public bool IsDead => isDead;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    /// <summary>Subtracts hit points and logs each hit; logs once when health reaches 0.</summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        if (isDead)
            return;

        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Health is now {currentHealth}/{maxHealth}.");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            Debug.Log("PLAYER DIED — health reached 0.");
            Died?.Invoke();
        }
    }
}
