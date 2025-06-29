using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleZombie : MonoBehaviour
{
    public float wanderSpeed = 1.5f;
    public float chaseSpeed = 2.5f;
    public float visionRange = 50f;
    public float attackRange = 3f;
    public float tileCheckDistance = 21f;
    public float decisionDelay = 1f;
    public LayerMask wallMask;
    public LayerMask playerMask;

    Transform player;
    public Animator anim;
    public SpriteRenderer sr;

    private Vector2 currentDirection;
    private bool isWaitingToDecide = false;
    private Coroutine crossroadDecisionCoroutine;

    void Start()
    {
        PickRandomDirection();
        if (player == null)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }
    }

    void Update()
    {
        if (player == null) return;

        Vector3 moveDir;
        float speed;

        if (CanSeePlayer())
        {
            Vector3 toPlayer = player.position - transform.position;
            float dist = toPlayer.magnitude;

            if (dist > attackRange)
            {
                moveDir = toPlayer.normalized;
                speed = chaseSpeed;
                transform.position += moveDir * speed * Time.deltaTime;
                SetAnimation(moveDir, isAttacking: false);
            }
            else
            {
                // Within attack range — stop and attack
                SetAnimation(toPlayer.normalized, isAttacking: true);
            }

            UpdateSortingOrder();
            return;
        }

        if (Physics2D.Raycast(transform.position, currentDirection, tileCheckDistance, wallMask))
        {
            PickRandomDirection();
            return;
        }

        if (IsAtCrossroad() && !isWaitingToDecide)
        {
            crossroadDecisionCoroutine = StartCoroutine(DelayedCrossroadDecision());
        }

        moveDir = currentDirection;
        speed = wanderSpeed;
        transform.position += (Vector3)moveDir * speed * Time.deltaTime;

        SetAnimation(moveDir, isAttacking: false);
        UpdateSortingOrder();
    }

    IEnumerator DelayedCrossroadDecision()
    {
        isWaitingToDecide = true;
        yield return new WaitForSeconds(decisionDelay);
        PickRandomDirection();
        isWaitingToDecide = false;
    }

    void PickRandomDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        List<Vector2> validDirs = new List<Vector2>();

        foreach (Vector2 dir in directions)
        {
            if (!Physics2D.Raycast(transform.position, dir, tileCheckDistance, wallMask))
            {
                validDirs.Add(dir);
            }
        }

        if (validDirs.Count > 1)
        {
            Vector2 reverse = -currentDirection;
            validDirs.Remove(reverse);
            if (validDirs.Count == 0) validDirs.Add(reverse);
        }

        if (validDirs.Count > 0)
        {
            currentDirection = validDirs[Random.Range(0, validDirs.Count)];
        }
    }

    bool IsAtCrossroad()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        int openCount = 0;

        foreach (Vector2 dir in directions)
        {
            if (!Physics2D.Raycast(transform.position, dir, tileCheckDistance, wallMask))
            {
                openCount++;
            }
        }

        return openCount > 2;
    }

    bool CanSeePlayer()
    {
        Vector2 toPlayer = (player.position - transform.position).normalized;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > visionRange) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, dist, wallMask);
        return hit.collider == null;
    }

    void SetAnimation(Vector2 dir, bool isAttacking)
    {
        int direction;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            direction = 1; // horizontal
            sr.flipX = dir.x < 0;
        }
        else if (dir.y > 0)
        {
            direction = 2; // up
            sr.flipX = false;
        }
        else
        {
            direction = 0; // down
            sr.flipX = false;
        }

        int state = isAttacking ? 1 : 0;

        anim.SetInteger("direction", direction);
        anim.SetInteger("state", state);
    }

    void UpdateSortingOrder()
    {
        if (sr == null || player == null) return;

        // Option 1: Relative to player (simple)
        if (transform.position.y > player.position.y)
        {
            sr.sortingOrder = 1; // behind
        }
        else
        {
            sr.sortingOrder = 3; // in front
        }

        // --- OR ---

        // Option 2: Global Y-sorting (uncomment below instead for dynamic auto-sorting)
        // sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
    void attack(){
        PlayerController.breathingCapacity-=2;
    }
}
