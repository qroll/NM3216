using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public Transform enemy;
    public float distFromCamera = 10.0f;

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
        float min = 0.2f;
        float max = 0.8f;
        Vector3 position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(min, max), Random.Range(min, max), distFromCamera));

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
