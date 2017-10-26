using UnityEngine;

public class SleepingZoneController : MonoBehaviour
{

    public GameManager gm;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Enemy"))
        {
            gm.EnemyReached(obj);
        }
    }

}
