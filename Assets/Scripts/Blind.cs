using UnityEngine;

public class Blind : MonoBehaviour {

    private float t = 0.0f;
    private float maxT = 3.0f;

    // Use this for initialization
    void Start()
    {

	}
	
	// Update is called once per frame
	void Update()
    {
        float alpha = Mathf.Lerp(1, 0, t / maxT);
        if (alpha == 0)
        {
            Object.Destroy(this);
        } else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            t += Time.deltaTime;
        }

    }

}
