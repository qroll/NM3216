﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour {

    public Text loadingText;

	// Use this for initialization
	void Start () {
        StartCoroutine(LoadGame());
    }
	
	// Update is called once per frame
	void Update () {
        loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
    }

    IEnumerator LoadGame()
    {
        // yield return new WaitForSeconds(3);

        AsyncOperation operation = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        operation.allowSceneActivation = false;

        //Wait until the last operation fully loads to return anything
        while (operation.progress < 0.9f)
        {
            loadingText.text = "Loading... " + string.Format("{0:P1}", operation.progress);
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}
