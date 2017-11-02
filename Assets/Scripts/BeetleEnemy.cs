using UnityEngine;

public class BeetleEnemy : Enemy
{

    public float distance;

    private float t = 0;
    private bool inRange = false;
    private bool inView = true;
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
                    inRange = true;
                    inView = false;
                    t = 0;
                }
                else
                {
                    t += Time.deltaTime;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, t);
                    sr.color = new Color(1, 1, 1, alpha);

                    SpriteRenderer plusSr = transform.Find("Plus").GetComponent<SpriteRenderer>();
                    plusSr.color = new Color(1, 1, 1, alpha);
                }
            }

            if (!inView)
            {
                t += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0, 1, t * 2.0f);
                sr.color = new Color(1, 1, 1, alpha);

                SpriteRenderer plusSr = transform.Find("Plus").GetComponent<SpriteRenderer>();
                plusSr.color = new Color(1, 1, 1, alpha);

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
