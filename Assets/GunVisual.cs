using UnityEngine;

public class GunVisualRotator : MonoBehaviour
{
    Camera mainCam;
    public Transform player;
    public Vector2 offset = Vector2.zero;
    void Start()
    {
        mainCam = Camera.main;
    }
    

    
    void Update()
    {
        if (mainCam == null) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorldPos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (player != null)
        {
            transform.position = (Vector2)player.position + offset*50f;
        }
    }
}
