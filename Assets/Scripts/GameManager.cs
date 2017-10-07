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
    public CDebug.EDebugLevel debugLevel;

    public Transform enemy;

    private static string[] ZONE_AXES = { "vertical", "horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;
    
    // Game status
    private int killCount = 0;
    private float lastIncreased = 0;
    
    // Use this for initialization
    void Start () {
        CDebug.SetDebugLoggingLevel((int) debugLevel);
    }
	
	// Update is called once per frame
	void Update () {
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
        Vector3 position = GeneratePosition();
        Quaternion rotation = GenerateRotation(position);
        CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("spawn position={0} | rotation={1} | time={2}", position, rotation, Time.time));
        Instantiate(enemy, position, rotation);
    }

    Vector3 GeneratePosition() {
        string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        Vector3 position;
        if (axis == "horizontal")
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(AXIS_MIN, AXIS_MAX), sign, distFromCamera));
        } else
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(sign, Random.Range(AXIS_MIN, AXIS_MAX), distFromCamera));
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
    }

}
