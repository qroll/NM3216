using UnityEngine;

public class Enemy : MonoBehaviour {

    public bool isTrapped = false;
    public float movement = 1;

    public float angle = 0;
    public float radius = 0.5f;

    public int hitsLeft = 1;

    // Use this for initialization
    void Start()
    {

	}
	
	// Update is called once per frame
	void Update()
    {
        if (isTrapped)
        {
            float x = radius * Mathf.Sin(angle);
            float y = radius * Mathf.Cos(angle);
            Vector3 position = new Vector3(x, y, transform.position.z);

            // update position and rotation around the pivot point
            transform.position = position;
            transform.right = position;

            angle += Time.deltaTime;
        } else
        {
            transform.Translate(movement * new Vector3(0, -1, 0) * Time.deltaTime);
        }
	}

    // Returns true if the enemy is killed on this hit
    public bool Swat()
    {
        hitsLeft--;
        return hitsLeft <= 0;
    }
}
