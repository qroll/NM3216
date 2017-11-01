using UnityEngine;

public class BeetleEnemy : Enemy
{

    public float distance;

    private float t = 0;
    private bool inRange = false;
    private SpriteRenderer sr;

    public BeetleEnemy()
    {
        movement = 0.8f;
        hitsLeft = 2;
        type = Enemy.Type.BEETLE;
    }

    public override void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public override void Update()
    {
        if (isTrapped)
        {
            Trapped();
        }
        else
        {
            if (!inRange)
            {
                if (transform.position.magnitude <= distance)
                {
                    sr.color = new Color(1, 1, 1, 1);
                    
                    SpriteRenderer plusSr = transform.Find("Plus").GetComponent<SpriteRenderer>();
                    plusSr.color = new Color(1, 1, 1, 1);

                    inRange = true;
                } else
                {
                    t += Time.deltaTime;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, t);
                    sr.color = new Color(1, 1, 1, alpha);

                    SpriteRenderer plusSr = transform.Find("Plus").GetComponent<SpriteRenderer>();
                    plusSr.color = new Color(1, 1, 1, alpha);
                }
            }

            Move();
        }
    }
    
}
