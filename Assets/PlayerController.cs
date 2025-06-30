using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public LayerMask zombieLayer;
    public LayerMask doorLayer;
    public GameObject g1;
    public Animator animator;
    bool attacking;
    int direction;
    public GameObject g2;
    public enum WeaponType { None, Lamp, Flower, Guitar, WaterBottle, Hairpin, Pencil, Gun, Chappal, Umbrella }
    public WeaponType equippedWeapon = WeaponType.None;
    bool over;
    public static float breathingCapacity = 200f;
    public float timeRemaining = 360f;
    private bool isDead = false;

    private List<WeaponType> inventory = new List<WeaponType>();

    public float meleeRange = 1.5f;
    public float meleeRadius = 0.6f;

    public Transform attackPointUp, attackPointDown, attackPointSide;
    public GameObject lampPrefab, flowerPrefab, guitarPrefab, waterBottlePrefab, hairpinPrefab, pencilPrefab, gunPrefab, chappalPrefab, umbrellaPrefab;
    public GameObject lampObj, flowerObj, guitarObj, waterBottleObj, hairpinObj, pencilObj, gunObj, chappalObj, umbrellaObj;

    public GameObject bulletPrefab;
    public Transform gunMuzzle;

    private WeaponPickup nearbyPickup = null;
    private Dictionary<WeaponType, GameObject> weaponPrefabs;

    public Slider s1;
    public Slider s2;
    public Slider s3;

    private static readonly Dictionary<WeaponType, (float freqBase, float capBase)> weaponWeights = new()
    {
        { WeaponType.Lamp,        (0.2f, 1.5f) },
        { WeaponType.Flower,      (0.3f, 1.6f) },
        { WeaponType.Guitar,      (0.6f, 2.5f) },
        { WeaponType.WaterBottle, (0.2f, 1.4f) },
        { WeaponType.Hairpin,     (0.1f, 1.2f) },
        { WeaponType.Pencil,      (0.15f, 1.3f) },
        { WeaponType.Gun,         (1.0f, 3.5f) },
        { WeaponType.Chappal,     (0.35f, 1.8f) },
        { WeaponType.Umbrella,    (0.4f, 2.2f) },
        { WeaponType.None,        (0f, 3f) }
    };

    void Start()
    {
        s1.maxValue = 15;
        s1.minValue = 0;
        s2.maxValue = 100;
        s2.minValue = 0;
        s3.maxValue = timeRemaining;
        s3.minValue = 0;

        breathingCapacity = 100f;

        weaponPrefabs = new Dictionary<WeaponType, GameObject>
        {
            { WeaponType.Lamp, lampPrefab },
            { WeaponType.Flower, flowerPrefab },
            { WeaponType.Guitar, guitarPrefab },
            { WeaponType.WaterBottle, waterBottlePrefab },
            { WeaponType.Hairpin, hairpinPrefab },
            { WeaponType.Pencil, pencilPrefab },
            { WeaponType.Gun, gunPrefab },
            { WeaponType.Chappal, chappalPrefab },
            { WeaponType.Umbrella, umbrellaPrefab }
        };
    }

    void Update()
    {
        if (isDead) return;
        if(over) return;
        HandleTimers();
        HandleMovementInput();
        HandleAttackInput();
        HandleInventoryInput();
        UpdateAnimation();

        s1.value = 45 / GetMaxZombieCap();
        if (breathingCapacity >= 0) s2.value = breathingCapacity;
        if (timeRemaining >= 0) s3.value = timeRemaining;
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if(over) return;
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) speed *= 2f;
        rb.linearVelocity = moveInput.normalized * speed;
    }

    void HandleTimers()
    {
        float drain = moveInput.magnitude > 0 ? 0.15f : 0f;
        if (Input.GetKey(KeyCode.LeftShift)) drain *= 12f;
        for (int i = 1; i < inventory.Count; i++)
            drain += Mathf.Pow(0.03f, 0.9f) * Mathf.Pow(i, 1.4f);

        breathingCapacity -= drain * Time.deltaTime;
        breathingCapacity = Mathf.Max(breathingCapacity, 0f);
        timeRemaining -= Time.deltaTime;

        if (breathingCapacity <= 0f || timeRemaining <= 0f)
            Die();
    }

    void HandleMovementInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;
    }

    void HandleAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (equippedWeapon == WeaponType.Gun)
                Shoot();
            else
            {
                attacking = true;
                animator.SetInteger("State", 3);
            }
        }
    }

    public void MeleeAttack()
    {
        int hits = GetHitCountFromWeapon(equippedWeapon);
        breathingCapacity -= 0.5f + GetBreathCostFromWeapon(equippedWeapon);

        Transform attackPoint = GetAttackPointFromDirection();
        if (attackPoint == null) return;

        Collider2D[] zombies = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius, zombieLayer);
        foreach (Collider2D target in zombies)
        {
            ZombieHealth zh = target.GetComponent<ZombieHealth>();
            if (zh != null) zh.TakeHits(hits);
        }

        Collider2D[] doors = Physics2D.OverlapCircleAll(attackPoint.position, meleeRadius, doorLayer);
        foreach (Collider2D door in doors)
        {
            var dh = door.GetComponent<DoorHealth>();
            if (dh != null)
            {
                dh.TakeHits(hits);
                continue;
            }

            var fd = door.GetComponent<FinalDoor>();
            if (fd != null && equippedWeapon == fd.requiredWeapon)
                fd.TakeHits(hits);
        }

        animator.SetInteger("State", 0);
        attacking = false;
    }

    Transform GetAttackPointFromDirection()
    {
        return Mathf.Abs(lastMoveDir.y) > Mathf.Abs(lastMoveDir.x)
            ? (lastMoveDir.y > 0 ? attackPointUp : attackPointDown)
            : attackPointSide;
    }

    void Shoot()
    {
        if (bulletPrefab == null || gunMuzzle == null) return;

        breathingCapacity -= 0.15f;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 shootDirection = (mouseWorldPos - gunMuzzle.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, gunMuzzle.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            float bulletSpeed = 120f;
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    int GetHitCountFromWeapon(WeaponType weapon)
    {
        return weapon switch
        {
            WeaponType.Lamp => 2,
            WeaponType.Flower => 3,
            WeaponType.Guitar => 4,
            WeaponType.WaterBottle => 2,
            WeaponType.Hairpin => 2,
            WeaponType.Pencil => 2,
            WeaponType.Chappal => 3,
            WeaponType.Umbrella => 4,
            WeaponType.Gun => 0,
            _ => 1
        };
    }

    float GetBreathCostFromWeapon(WeaponType weapon)
    {
        return weapon switch
        {
            WeaponType.Hairpin => 0.05f,
            WeaponType.Pencil => 0.05f,
            WeaponType.Flower => 0.1f,
            WeaponType.WaterBottle => 0.15f,
            WeaponType.Lamp => 0.15f,
            WeaponType.Guitar => 0.3f,
            WeaponType.Chappal => 0.1f,
            WeaponType.Umbrella => 0.25f,
            _ => 0.2f
        };
    }

    void HandleInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (nearbyPickup != null)
            {
                PickUpItem(nearbyPickup.weaponType);
                Destroy(nearbyPickup.gameObject);
                nearbyPickup = null;
            }
            else if (nearbyOxygen)
            {
                breathingCapacity = Mathf.Min(100f, breathingCapacity + 30f);
                Destroy(nearbyOxygen.gameObject);
                nearbyOxygen = null;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) ThrowLastItem();
        if (Input.GetKeyDown(KeyCode.Tab)) RotateInventoryForward();
        if (Input.GetKeyDown(KeyCode.R)) EquipNone();
    }

    public void PickUpItem(WeaponType item)
    {
        inventory.Add(item);
        equippedWeapon = inventory[0];
        UpdateHeldItemVisual();
    }

    public void ThrowLastItem()
    {
        if (inventory.Count > 0)
        {
            WeaponType dropped = inventory[^1];
            inventory.RemoveAt(inventory.Count - 1);
            equippedWeapon = inventory.Count > 0 ? inventory[0] : WeaponType.None;
            UpdateHeldItemVisual();

            if (weaponPrefabs.TryGetValue(dropped, out var prefab) && prefab != null)
            {
                Vector3 spawnPos = transform.position + (Vector3)(lastMoveDir.normalized * 0.5f);
                Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }
    }

    void RotateInventoryForward()
    {
        if (inventory.Count <= 1) return;
        WeaponType first = inventory[0];
        inventory.RemoveAt(0);
        inventory.Add(first);
        equippedWeapon = inventory[0];
        UpdateHeldItemVisual();
    }

    void EquipNone()
    {
        equippedWeapon = WeaponType.None;
        UpdateHeldItemVisual();
    }

    void UpdateHeldItemVisual()
    {
        lampObj.SetActive(false); flowerObj.SetActive(false); guitarObj.SetActive(false);
        waterBottleObj.SetActive(false); hairpinObj.SetActive(false); pencilObj.SetActive(false);
        gunObj.SetActive(false); chappalObj.SetActive(false); umbrellaObj.SetActive(false);

        switch (equippedWeapon)
        {
            case WeaponType.Lamp: lampObj.SetActive(true); break;
            case WeaponType.Flower: flowerObj.SetActive(true); break;
            case WeaponType.Guitar: guitarObj.SetActive(true); break;
            case WeaponType.WaterBottle: waterBottleObj.SetActive(true); break;
            case WeaponType.Hairpin: hairpinObj.SetActive(true); break;
            case WeaponType.Pencil: pencilObj.SetActive(true); break;
            case WeaponType.Gun: gunObj.SetActive(true); break;
            case WeaponType.Chappal: chappalObj.SetActive(true); break;
            case WeaponType.Umbrella: umbrellaObj.SetActive(true); break;
        }
    }

    void UpdateAnimation()
    {
        if (moveInput == Vector2.zero && !attacking)
            animator.SetInteger("State", 0);
        else if (!attacking)
            animator.SetInteger("State", 1);

        Vector2 refDir = moveInput != Vector2.zero ? moveInput : lastMoveDir;

        if (Mathf.Abs(refDir.y) > Mathf.Abs(refDir.x))
        {
            direction = refDir.y > 0 ? 2 : 0;
            spriteRenderer.flipX = false;
        }
        else
        {
            direction = 1;
            spriteRenderer.flipX = refDir.x < 0;
            Vector3 attackPos = attackPointSide.localPosition;
            attackPos.x = Mathf.Abs(attackPos.x) * (spriteRenderer.flipX ? -1 : 1);
            attackPointSide.localPosition = attackPos;
        }

        animator.SetInteger("Direction", direction);
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Die");
        g1.SetActive(true);
        Time.timeScale=0f;
    }

    private GameObject nearbyOxygen;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Item"))
        {
            WeaponPickup pickup = other.GetComponent<WeaponPickup>();
            if (pickup != null)
                nearbyPickup = pickup;
        }
        else if (other.CompareTag("Oxygen"))
        {
            nearbyOxygen = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Item") && other.GetComponent<WeaponPickup>() == nearbyPickup)
            nearbyPickup = null;
        else if (other.CompareTag("Oxygen") && other.gameObject == nearbyOxygen)
            nearbyOxygen = null;
    }

    public float GetTotalSpawnFrequency()
    {
        float freq = 0f;
        if (weaponWeights.TryGetValue(equippedWeapon, out var held))
            freq += held.freqBase;

        for (int i = 1; i < inventory.Count; i++)
        {
            if (weaponWeights.TryGetValue(inventory[i], out var bag))
                freq += Mathf.Pow(bag.freqBase, 1.4f);
        }

        return Mathf.Clamp(freq, 0.2f, 2f);
    }

    public int GetMaxZombieCap()
    {
        float cap = 0f;
        if (weaponWeights.TryGetValue(equippedWeapon, out var held))
            cap += held.capBase;

        for (int i = 1; i < inventory.Count; i++)
        {
            if (weaponWeights.TryGetValue(inventory[i], out var bag))
                cap += Mathf.Pow(bag.capBase, 1.25f);
        }

        return Mathf.Min(25, Mathf.CeilToInt(cap));
    }

    void OnDrawGizmosSelected()
    {
        if (attackPointUp != null) Gizmos.DrawWireSphere(attackPointUp.position, meleeRadius);
        if (attackPointDown != null) Gizmos.DrawWireSphere(attackPointDown.position, meleeRadius);
        if (attackPointSide != null) Gizmos.DrawWireSphere(attackPointSide.position, meleeRadius);
    }
    public void win(){
        g2.SetActive(true);
        over=true;
        Time.timeScale=0f;
    }
}
