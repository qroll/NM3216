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
    
}
