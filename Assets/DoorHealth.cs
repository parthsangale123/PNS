using UnityEngine;

public class DoorHealth : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits;

    void Start()
    {
        currentHits = maxHits;
    }

    public void TakeHits(int amount)
    {
        currentHits -= amount;
        if (currentHits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
