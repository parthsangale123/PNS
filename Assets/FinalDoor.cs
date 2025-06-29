using UnityEngine;

public class FinalDoor : MonoBehaviour
{
    public int maxHits = 5;
    private int currentHits;
    GameObject g1;
    public PlayerController.WeaponType requiredWeapon;

    void Start()
    {
        currentHits = maxHits;

        // Pick a random required weapon (excluding Gun and None)
        var values = System.Enum.GetValues(typeof(PlayerController.WeaponType));
        do
        {
            requiredWeapon = (PlayerController.WeaponType)values.GetValue(Random.Range(0, values.Length));
        } while (requiredWeapon == PlayerController.WeaponType.None || requiredWeapon == PlayerController.WeaponType.Gun);

        Debug.Log("FinalDoor requires: " + requiredWeapon);
    }

    public void TakeHits(int amount)
    {
        currentHits -= amount;
        if (currentHits <= 0)
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().win();
           

            Destroy(gameObject);
            Debug.Log("Final door destroyed!");
            
        }
    }
}
