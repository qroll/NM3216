using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("up");
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Debug.Log("down");
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("left");
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Debug.Log("right");
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Enemy"))
        {
            Object.Destroy(obj);
        }
    }

}
