using UnityEngine;

/*public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject zombiePrefab;
    public float spawnRadius = 2f;
    public LayerMask obstacleLayers;
    public bool disableSelfCollider = true;

    [Header("Base Spawning Values")]
    public float baseFrequency = 0.25f;  // Base: 1 zombie every 4 seconds
    public int baseMaxZombies = 1;

    private Transform player;
    private float spawnCooldown;
    private float spawnTimer;
    private int maxZombies;
    private int currentZombies;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        spawnTimer = Random.Range(0f, 1f); // Slight desync across spawners
    }

    void Start()
    {
        if (disableSelfCollider && TryGetComponent<Collider2D>(out var col))
            col.enabled = false;
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
            return;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null) return;

        // Calculate frequency and cap from player
        float playerFreq = pc.GetTotalSpawnFrequency();   // e.g., max 2/s
        float playerCap = pc.GetMaxZombieCap();           // e.g., 12 total

        float finalFreq = baseFrequency + (playerFreq / 3f); // Divide by 3 spawners
        float finalCap = baseMaxZombies + (playerCap / 3f);

        spawnCooldown = finalFreq > 0f ? 1f / finalFreq : 999f;
        maxZombies = Mathf.Clamp(Mathf.FloorToInt(finalCap), 1, 999);

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && currentZombies < maxZombies)
        {
            Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            if (IsValidSpawnPosition(spawnPos))
            {
                GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);

                if (zombie.TryGetComponent<ZombieHealth>(out var zh))
                {
                    zh.SetOwningSpawner(this);
                    currentZombies++;
                }
            }

            spawnTimer = spawnCooldown;
        }
    }

    bool IsValidSpawnPosition(Vector2 pos)
    {
        return Physics2D.OverlapCircle(pos, 0.3f, obstacleLayers) == null;
    }

    public void NotifyZombieKilled()
    {
        currentZombies = Mathf.Max(currentZombies - 1, 0);
    }
}
*/