using UnityEngine;

public class LadybugEnemy : Enemy
{

    public LadybugEnemy()
    {
        movement = 0.6f;
        hitsLeft = 2;
        type = Enemy.Type.LADYBUG;
    }

}
