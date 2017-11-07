using UnityEngine;

public class BeetleEnemy : Enemy
{

    public float distance;
    
    private float deltaTimeFadeOut = 0;
    private float deltaTimeFadeIn = 0;
    private bool inRange = false;
    private bool hasFadedOut = false;
    private bool hasFadedIn = true;
    private SpriteRenderer sr;

    private const float FADE_OUT_SPEED = 2.0f;
    private const float FADE_IN_SPEED = 2.0f;

    public BeetleEnemy()
    {
        movement = 0.4f;
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
            if (!hasFadedOut)
            {
                if (transform.position.magnitude <= (distance * 2f))
                {
                    hasFadedOut = true;
                }
            }
            else if (!inRange)
            {
                if (transform.position.magnitude <= distance)
                {
                    inRange = true;
                    hasFadedIn = false;
                }
                else
                {
                    deltaTimeFadeOut += Time.deltaTime;
                    float alpha = 1 - Mathf.SmoothStep(0, 1, deltaTimeFadeOut * FADE_OUT_SPEED);
                    sr.color = new Color(1, 1, 1, alpha);
                    healthBarSR.color = new Color(1, 1, 1, alpha);
                }
            }

            if (!hasFadedIn)
            {
                deltaTimeFadeIn += Time.deltaTime;
                float alpha = Mathf.SmoothStep(0, 1, deltaTimeFadeIn * FADE_IN_SPEED);
                sr.color = new Color(1, 1, 1, alpha);
                if (healthBarSR != null)
                {
                    healthBarSR.color = new Color(1, 1, 1, alpha);
                }

                if (alpha == 1.0f)
                {
                    hasFadedIn = true;
                }
            }

            Move();
        }
    }

}
