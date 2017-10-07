using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public GameObject[] swatters;

    private static float ACTIVE_TIME = 0.2f;

	// Use this for initialization
	void Start () {
        foreach (GameObject swatter in swatters)
        {
            swatter.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "up");
            swatters[0].SetActive(true);
            StartCoroutine(DisableSwatter(ACTIVE_TIME, swatters[0]));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "down");
            swatters[1].SetActive(true);
            StartCoroutine(DisableSwatter(ACTIVE_TIME, swatters[1]));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "left");
            swatters[2].SetActive(true);
            StartCoroutine(DisableSwatter(ACTIVE_TIME, swatters[2]));
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "right");
            swatters[03].SetActive(true);
            StartCoroutine(DisableSwatter(ACTIVE_TIME, swatters[3]));
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;
        if (obj.CompareTag("Enemy"))
        {
            GameManager.Instance.EnemyReached(obj);
        }
    }

    IEnumerator DisableSwatter(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

}
