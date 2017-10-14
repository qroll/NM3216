using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float distFromCamera = 10.0f;
    public float startDelay = 1.0f;
    public float swatRadius = 0.3f;
    public float spawnFreqRate = 1.0f;
    public float deltaSpawnFreqRate = 0.1f;
    public float minSpawnFreqRate = 0.5f;
    public float spawnNumRate = 1.0f;
    public float deltaSpawnNumRate = 1.0f;
    public float maxSpawnNumRate = 10.0f;
    public CDebug.EDebugLevel debugLevel = CDebug.EDebugLevel.TRACE;

    public Transform enemy;
    public Sprite frogDisabledSprite;
    public Sprite frogEnabledSprite;
    public GameObject[] swatters;
    public GameObject[] frogs;

    public const string ZONE_UP = "Up";
    public const string ZONE_DOWN = "Down";
    public const string ZONE_LEFT = "Left";
    public const string ZONE_RIGHT = "Right";

    private static string[] ZONE_AXES = { "Vertical", "Horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;
    private static float DISABLE_TIME = 2.0f;

    // Game status
    private bool isGameOver = false;
    private int killCount = 0;
    private float lastIncreased = 0;
    private Dictionary<string, bool> swatterEnabled = new Dictionary<string, bool>();
    private Dictionary<string, Queue<GameObject>> infestedZones = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> swatterSprites = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Start () {
        CDebug.SetDebugLoggingLevel((int) debugLevel);

        // initialise infested zones
        infestedZones.Add("Up", new Queue<GameObject>());
        infestedZones.Add("Down", new Queue<GameObject>());
        infestedZones.Add("Left", new Queue<GameObject>());
        infestedZones.Add("Right", new Queue<GameObject>());
        
        // initialise swatters
        foreach (GameObject swatter in swatters)
        {
            swatter.SetActive(false);
        }

        swatterEnabled.Add("Up", true);
        swatterEnabled.Add("Down", true);
        swatterEnabled.Add("Left", true);
        swatterEnabled.Add("Right", true);

        swatterSprites.Add("Up", frogs[0]);
        swatterSprites.Add("Down", frogs[1]);
        swatterSprites.Add("Left", frogs[2]);
        swatterSprites.Add("Right", frogs[3]);
    }
	
	// Update is called once per frame
	void Update () {
        if (isGameOver)
        {
            return;
        }

        CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("time={0} | last update={1}", Time.time, (startDelay + lastIncreased)));
        if (Time.time - (startDelay + lastIncreased) > spawnFreqRate)
        {
            for (int i = 0; i < spawnNumRate; i++)
            {
                SpawnEnemy();
            }
            lastIncreased = Time.time;
        }
    }

    void SpawnEnemy()
    {
        string zone = GenerateZone();
        Vector3 position = GeneratePosition(zone);
        Quaternion rotation = GenerateRotation(position);
        CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("spawn position={0} | rotation={1} | time={2}", position, rotation, Time.time));
        GameObject spawn = Instantiate(enemy, position, rotation).gameObject;
        GameObject go = new GameObject("Zone");
        go.tag = "Zone" + zone;
        go.transform.parent = spawn.transform;

        Movement control = (Movement) spawn.GetComponent("Movement");
        control.zone = zone;
    }

    string GenerateZone()
    {
        string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        if (axis == "Horizontal")
        {
            return sign == 1 ? ZONE_RIGHT : ZONE_LEFT;
        } else
        {
            return sign == 1 ? ZONE_UP : ZONE_DOWN;
        }
    }

    Vector3 GeneratePosition(string zone) {
        // string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        // int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        Vector3 position;
        if (zone == ZONE_UP)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.5f, AXIS_MAX), 1, distFromCamera));
        } else if (zone == ZONE_DOWN)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(AXIS_MIN, 0.5f), 0, distFromCamera));
        } else if (zone == ZONE_LEFT)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(0, Random.Range(0.5f, AXIS_MAX), distFromCamera));
        } else
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(1, Random.Range(AXIS_MIN, 0.5f), distFromCamera));
        }

        return position;
    }

    Quaternion GenerateRotation(Vector3 position)
    {
        Vector3 direction = new Vector3(0,0,0) - position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return rotation;
    }

    public void Swat(string zone)
    {
        CDebug.Log(CDebug.EDebugLevel.INFO, zone);

        Queue<GameObject> queue = infestedZones[zone];

        if (queue.Count > 0)
        {
            CDebug.Log(CDebug.EDebugLevel.TRACE, "destroying bug in infestation zone");
            GameObject enemy = queue.Dequeue();
            AnimateFrog(zone, enemy);
            Object.Destroy(enemy);
            return;
        }

        GameObject closest = FindClosestEnemy(zone);

        bool isSwatterActive = swatterEnabled[zone];

        if (isSwatterActive && closest != null)
        {
            AnimateFrog(zone, closest);
            Object.Destroy(closest.transform.parent.gameObject);
        } else if (isSwatterActive)
        {
            // disable the swatter temporarily
            swatterEnabled[zone] = false;
            SpriteRenderer sr = swatterSprites[zone].GetComponent<SpriteRenderer>();
            sr.sprite = frogDisabledSprite;
            StartCoroutine(EnableSwatter(DISABLE_TIME, zone));
        }
    }

    private GameObject FindClosestEnemy(string zone)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Zone" + zone);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 position = new Vector3(0, 0, 0);

        Vector3 coords = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1 - swatRadius, distFromCamera));
        float radius = coords.sqrMagnitude;

        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.parent.position - position;
            float length = diff.sqrMagnitude;

            if (length < radius && length < minDistance)
            {
                closest = go;
                minDistance = length;
            }
        }

        return closest;
    }

    private void AnimateFrog(string zone, GameObject obj)
    {
        GameObject frog = swatterSprites[zone];
        Transform tongue = frog.transform.Find("tongue-length");

        Vector3 originalScale = tongue.localScale;
        Vector3 originalPosition = tongue.position;

        float scale = 2.0f;

        ExtendTongue(frog, originalPosition, scale);
        StartCoroutine(RetractTongue(frog, originalScale, originalPosition));
    }

    void ExtendTongue(GameObject frog, Vector3 originalPosition, float scale)
    {
        Transform tongue = frog.transform.Find("tongue-length");
        float scaleBy = scale / tongue.localScale.y;
        tongue.localScale = new Vector3(tongue.localScale.x, scale, tongue.localScale.z);
        tongue.transform.Translate(1/scale * new Vector3(0, 1, 0));
        CDebug.Log(CDebug.EDebugLevel.TRACE, scale / 2 * new Vector3(0, 1, 0));
    }

    IEnumerator RetractTongue(GameObject frog, Vector3 originalScale, Vector3 originalPosition)
    {
        yield return new WaitForSeconds(0.2f);
        Transform tongue = frog.transform.Find("tongue-length");

        // reset the sprite
        tongue.transform.position = originalPosition;
        tongue.localScale = originalScale;
    }

    IEnumerator DisableSwatter(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    IEnumerator EnableSwatter(float delay, string zone)
    {
        yield return new WaitForSeconds(delay);
        swatterEnabled[zone] = true;
        SpriteRenderer sr = swatterSprites[zone].GetComponent<SpriteRenderer>();
        sr.sprite = frogEnabledSprite;
    }

    public void EnemySwatted()
    {
        killCount++;
        spawnFreqRate = Mathf.Max(spawnFreqRate - deltaSpawnFreqRate, minSpawnFreqRate);
        spawnNumRate = Mathf.Min(spawnNumRate + deltaSpawnNumRate, maxSpawnNumRate);
        CDebug.Log(CDebug.EDebugLevel.INFO, "kill count=" + killCount + " | spawn freq=" + spawnFreqRate + " | spawn num=" + spawnNumRate);
    }

    public void EnemyReached(GameObject obj)
    {
        Movement control = (Movement) obj.GetComponent("Movement");
        control.isTrapped = true;
        Queue<GameObject> queue = infestedZones[control.zone];
        queue.Enqueue(obj);

        if (queue.Count >= 3)
        {
            isGameOver = true;
            CDebug.Log(CDebug.EDebugLevel.INFO, "game over");
        }
    }

}
