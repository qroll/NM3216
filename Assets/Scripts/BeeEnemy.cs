using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeEnemy : Enemy
{

    public float distance;

    private bool isPaused = false;
    private bool hasStopped = false;

    public BeeEnemy()
    {
        movement = 1.2f;
        hitsLeft = 1;
        type = Enemy.Type.BEE;
    }

    public override void Move()
    {
        if (isPaused)
        {
            Vector3 backwards = PPU_SCALE * new Vector3(0, 0.5f, 0) * Time.deltaTime;
            transform.Translate(backwards);
            return;
        }

        if (!hasStopped && transform.position.magnitude <= (distance + 1))
        {
            isPaused = true;
            hasStopped = true;
            StartCoroutine(SpeedUp());
        }

        Vector3 translation = movement * PPU_SCALE * new Vector3(0, -1, 0) * Time.deltaTime;
        transform.Translate(translation);
    }

    IEnumerator SpeedUp()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 0.8f));
        isPaused = false;
        movement = 2.0f;
    }

}
