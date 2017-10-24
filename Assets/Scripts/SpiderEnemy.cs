using UnityEngine;

public class SpiderEnemy : Enemy
{
    public WebEnemy web;

    private bool webShot = false;
    private float elapsedTime = 0;

    public SpiderEnemy()
    {
        movement = 0.5f;
        hitsLeft = 1;
    }

    protected override void Update()
    {
        if (!webShot && elapsedTime + Time.deltaTime > 0.5f)
        {
            ShootWeb();
        } else
        {
            elapsedTime += Time.deltaTime;
            Move();
        }
    }

    void ShootWeb()
    {
        Transform webObj = Instantiate(web, transform.position, transform.rotation).transform;
        webObj.transform.Translate(new Vector3(0, -1, 0));

        // populate with zone tag
        GameObject zoneInfo = new GameObject("Zone");
        zoneInfo.tag = "Zone" + zone;
        zoneInfo.transform.parent = webObj;

        webShot = true;
    }
    
}
