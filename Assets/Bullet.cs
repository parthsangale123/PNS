using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 3f;
    public int damage = 3;

    private Vector2 direction;

    void Start(){
        Destroy(gameObject, lifeTime);
    }
    public void SetDirection(Vector2 dir)
{
    // Rotate 90 degrees behind (clockwise)
    direction = new Vector2(dir.y, -dir.x).normalized;
}


    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("zombie"))
        {
            ZombieHealth zh = other.gameObject.GetComponent<ZombieHealth>();
            if (zh != null)
                zh.TakeHits(damage);

            Destroy(gameObject);
        }

        else{
            Destroy(gameObject);
        }
        
    }
}
