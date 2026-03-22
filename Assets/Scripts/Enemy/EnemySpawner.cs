using UnityEngine;

/// <summary>
/// Spawns the same enemy prefab on an interval, at a random point on a circle around the player.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Tooltip("Prefab with EnemyFollow, Rigidbody2D, trigger Collider2D, and EnemyContactDamage on the root.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Player transform; spawn positions are offset from here.")]
    [SerializeField] private Transform player;

    [Tooltip("Distance from the player where new enemies appear.")]
    [SerializeField] private float spawnRadius = 8f;

    [Tooltip("Seconds between each spawn.")]
    [SerializeField] private float spawnInterval = 2f;

    void Start()
    {
        // First spawn at t=0, then every spawnInterval seconds.
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null || player == null)
            return;

        // Random direction in the XY plane, then place on the ring around the player.
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;
        Vector3 spawnPosition = player.position + offset;

        GameObject instance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // Wire the enemy to chase this player (works even if the prefab has no default target).
        EnemyFollow follow = instance.GetComponent<EnemyFollow>();
        if (follow != null)
            follow.target = player;
    }
}
