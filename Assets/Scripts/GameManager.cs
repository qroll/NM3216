using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float distFromCamera = 10.0f;
    public float startDelay = 1.0f;
    public float spawnRate = 1.0f;
    public CDebug.EDebugLevel debugLevel;

    public Transform enemy;

    private static string[] ZONE_AXES = { "vertical", "horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;

    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }

            return instance;
        }
    }

    // Use this for initialization
    void Start () {
        InvokeRepeating("SpawnEnemy", startDelay, spawnRate);
        CDebug.SetDebugLoggingLevel((int) debugLevel);
    }
	
	// Update is called once per frame
	void Update () {

	}

    void SpawnEnemy()
    {
        Vector3 position = GeneratePosition();
        Quaternion rotation = GenerateRotation(position);
        CDebug.Log(CDebug.EDebugLevel.DEBUG, position + " " + rotation);
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

    public void EnemyReached(GameObject obj)
    {
        Movement control = (Movement) obj.GetComponent("Movement");
        control.isTrapped = true;
    }

}
