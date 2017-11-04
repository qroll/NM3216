using UnityEngine;

public class BeetleEnemy : Enemy
{

    public float distance;

    private float deltaTime = 0;
    private bool inRange = false;
    private bool inView = true;
    private SpriteRenderer sr;

    public BeetleEnemy()
    {
        movement = 0.6f;
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
                    inRange = true;
                    inView = false;
                    deltaTime = 0;
                }
                else
                {
                    deltaTime += Time.deltaTime;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, deltaTime);
                    sr.color = new Color(1, 1, 1, alpha);
                    healthBarSR.color = new Color(1, 1, 1, alpha);
                }
            }

            if (!inView)
            {
                deltaTime += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0, 1, deltaTime * 2.0f);
                sr.color = new Color(1, 1, 1, alpha);
                healthBarSR.color = new Color(1, 1, 1, alpha);

                if (alpha == 1.0f)
                {
                    inView = true;
                }
            }
            else
            {
                Move();
            }
        }
    }

}
