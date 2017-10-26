using UnityEngine;

public class FireflyEnemy : Enemy
{

    public Transform blind;

    public FireflyEnemy()
    {
        movement = 1.0f;
        hitsLeft = 1;
        type = Enemy.Type.FIREFLY;
    }

    // Returns true if the enemy is killed on this hit
    public override bool Swat()
    {
        hitsLeft--;
        if (hitsLeft == 0)
        {
            Instantiate(blind, transform.position, Quaternion.identity);
        }
        return hitsLeft <= 0;
    }

}
