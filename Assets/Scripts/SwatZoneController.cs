using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwatZoneController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
