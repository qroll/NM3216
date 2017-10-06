using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Transform enemy;
    public GameObject[] spawnPoints;

    private static string[] SPAWN_POINT = { "up", "down", "left", "right" };

	// Use this for initialization
	void Start () {
        InvokeRepeating("SpawnEnemy", 1.0f, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {

	}

    void SpawnEnemy()
    {
        int index = Mathf.FloorToInt(Random.value * 4);
        string spawnAt = SPAWN_POINT[index];
        Vector3 position = GeneratePosition(spawnAt, index);
        Quaternion rotation = GenerateRotation(spawnAt);
        Debug.Log(position + " " + rotation);
        Instantiate(enemy, position, rotation);
    }

    Vector3 GeneratePosition(string spawnAt, int index) {
        GameObject spawnPoint = spawnPoints[index];
        float x = spawnPoint.transform.position.x;
        float y = spawnPoint.transform.position.y;
        
        return new Vector3(x, y, 0);
    }

    Quaternion GenerateRotation(string spawnAt)
    {
        int angle;
        switch (spawnAt)
        {
            case "up":
                angle = 0;
                break;
            case "down":
                angle = 180;
                break;
            case "left":
                angle = 90;
                break;
            case "right":
                angle = 270;
                break;
            default:
                angle = 0;
                break;
        }
            
        return Quaternion.Euler(0, 0, angle);
    }

}
