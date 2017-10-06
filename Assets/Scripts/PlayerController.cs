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
            CDebug.Log(CDebug.EDebugLevel.INFO, "up");
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "down");
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "left");
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "right");
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
