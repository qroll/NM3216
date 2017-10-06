using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Transform enemy;
    public float distFromCamera = 10.0f;
    private static string[] ZONE_AXES = { "vertical", "horizontal" };
    private float min = 0.2f;
    private float max = 0.8f;

    // Use this for initialization
    void Start () {
        InvokeRepeating("SpawnEnemy", 1.0f, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {

	}

    void SpawnEnemy()
    {
        Vector3 position = GeneratePosition();
        Quaternion rotation = GenerateRotation(position);
        Debug.Log(position + " " + rotation);
        Instantiate(enemy, position, rotation);
    }

    Vector3 GeneratePosition() {
        string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        Vector3 position;
        if (axis == "horizontal")
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(min, max), sign, distFromCamera));
        } else
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(sign, Random.Range(min, max), distFromCamera));
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

}
