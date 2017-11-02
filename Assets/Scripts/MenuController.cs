﻿using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour {

    public GameObject button;

	void Start ()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);
    }
	
	void Update () {
        if (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(button);
        }
    }

}
