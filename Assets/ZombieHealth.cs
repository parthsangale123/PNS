using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public int maxHits = 5;
    private int currentHits;

    public Animator animator; // Optional if you want death animation
    private bool isDead = false;



    void Start()
    {
        currentHits = maxHits;
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
        isDead = true;
        Debug.Log($"{gameObject.name} died!");
       // MazeGeneratorTilemap.zombiesAlive--;
        if (animator != null)
        {
            animator.SetTrigger("die");
        }
        
        // Disable movement & collision (optional)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f); // Allow death animation time
    }
}
