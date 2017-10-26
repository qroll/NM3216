using UnityEngine;

public class FlyEnemy : Enemy
{
    // For fly movement
    public Vector3 m_centerPosition;
    public float m_degrees = 0;
    float m_period = 1.0f;

    public FlyEnemy()
    {
        movement = 3.0f;
        hitsLeft = 1;
        type = Enemy.Type.FLY;
    }

    public override void Move()
    {
        float deltaTime = Time.deltaTime;
        Vector3 translation = movement * new Vector3(0, -1, 0) * deltaTime;
        transform.Translate(translation);

        float x = m_period * Mathf.Sin(m_degrees);
        transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);

        m_degrees += deltaTime;
    }

}
