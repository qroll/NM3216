using UnityEngine;

public class BeeEnemy : Enemy
{

    public BeeEnemy()
    {
        movement = 1.0f;
        hitsLeft = 1;
        type = Enemy.Type.BEE;
    }

}
