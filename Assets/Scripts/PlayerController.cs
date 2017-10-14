using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameManager gm;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            gm.Swat("Up");
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            gm.Swat("Down");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gm.Swat("Left");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            gm.Swat("Right");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Enemy"))
        {
            gm.EnemyReached(obj);
        }
    }

}
