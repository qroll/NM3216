using UnityEngine;

public class LadybugEnemy : Enemy
{

    public LadybugEnemy()
    {
        movement = 1.0f;
        hitsLeft = 2;
        type = Enemy.Type.LADYBUG;
    }

}
