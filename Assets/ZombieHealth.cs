using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHits = 5;
    private int currentHits;

    public Animator animator;
    private bool isDead = false;

    public float checkRadius = 7*21f; // Range to keep zombie alive if player is close
    private float killTimer;
    private Transform player;
    private float spawnFrequency = 5f; // default, will be overwritten by PlayerController

    void Start()
    {
        currentHits = maxHits;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null)
                spawnFrequency = Mathf.Min(5f, 1f / pc.GetTotalSpawnFrequency());
        }

        killTimer = spawnFrequency;
    }

    void Update()
    {
        if (isDead) return;

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        if (distance > checkRadius)
        {
            
            killTimer -= Time.deltaTime;
            if (killTimer <= 0f)
            {
                Die();
            }
        }
        else
        {
            // Reset timer if player is nearby
            killTimer = spawnFrequency;
        }
    }

    public void TakeHits(int hitCount)
    {
        if (isDead) return;

        currentHits -= hitCount;
        currentHits = Mathf.Max(currentHits, 0);

        if (currentHits <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        if (animator != null)
            animator.SetTrigger("die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        MazeGeneratorTilemap.zombiesAlive--;
        Destroy(gameObject, 1.5f);
    }
}
