using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float distFromCamera = 10.0f;
    public float startDelay = 1.0f;
    public float spawnFreqRate = 1.0f;
    public float deltaSpawnFreqRate = 0.5f;
    public float minSpawnFreqRate = 0.5f;
    public float spawnNumRate = 1.0f;
    public float deltaSpawnNumRate = 1.0f;
    public float maxSpawnNumRate = 10.0f;
    public int maxSpawn = 3;
    public CDebug.EDebugLevel debugLevel;

    public Transform enemy;

    private static string[] ZONE_AXES = { "vertical", "horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;

    // Game status
    private bool isGameOver = false;
    private int killCount = 0;
    private float lastIncreased = 0;
    private Dictionary<string, Queue<GameObject>> infestedZones = new Dictionary<string, Queue<GameObject>>();

    // Use this for initialization
    void Start () {
        CDebug.SetDebugLoggingLevel((int) debugLevel);
        infestedZones.Add("up", new Queue<GameObject>());
        infestedZones.Add("down", new Queue<GameObject>());
        infestedZones.Add("left", new Queue<GameObject>());
        infestedZones.Add("right", new Queue<GameObject>());
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

        Movement control = (Movement) spawn.GetComponent("Movement");
        control.zone = zone;
    }

    string GenerateZone()
    {
        string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        if (axis == "horizontal")
        {
            return sign == 1 ? "right" : "left";
        } else
        {
            return sign == 1 ? "up" : "down";
        }
    }

    Vector3 GeneratePosition(string zone) {
        // string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        // int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        Vector3 position;
        if (zone == "left")
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(AXIS_MIN, 0.5f), 0, distFromCamera));
        } else if (zone == "right")
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.5f, AXIS_MAX), 1, distFromCamera));
        } else if (zone == "up")
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
        
        if (queue.Count >= 3)
        {
            isGameOver = true;
            CDebug.Log(CDebug.EDebugLevel.INFO, "game over");
        } else
        {
            queue.Enqueue(obj);
        }
    }

}
