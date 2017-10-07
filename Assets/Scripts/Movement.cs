using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public bool isTrapped = false;
    public string zone;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isTrapped)
        {
            transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime);
        }
        
	}
}
