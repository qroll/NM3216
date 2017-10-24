using UnityEngine;

public class WebEnemy : Enemy
{

    public WebEnemy()
    {
        movement = 1.0f;
        hitsLeft = 1;
        type = Enemy.Type.WEB;
    }
    
}
