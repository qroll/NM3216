using UnityEngine;

public class SleepingZoneController : MonoBehaviour
{

    public GameManager gm;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Enemy"))
        {
            gm.EnemyReached(obj);
        }
    }

}
