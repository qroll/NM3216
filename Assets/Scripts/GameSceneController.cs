using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneController : MonoBehaviour {
    
	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
    
    }
    
    public void OnMainMenuClick()
    {
        SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
    }
}
