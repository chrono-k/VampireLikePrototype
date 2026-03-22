using UnityEngine;

/// <summary>
/// Runs once when the player dies: stop movement, stop spawning, log game over.
/// Keeps <see cref="PlayerHealth"/> free of references to movement or the spawner.
/// </summary>
public class GameOverHandler : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private EnemySpawner enemySpawner;

    private bool gameOverHandled;

    void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    void OnPlayerDied()
    {
        if (gameOverHandled)
            return;

        gameOverHandled = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disabling the component stops InvokeRepeating on EnemySpawner.
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        Debug.Log("GAME OVER");
    }
}
