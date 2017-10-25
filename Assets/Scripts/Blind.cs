using UnityEngine;

public class Blind : MonoBehaviour {

    private float t = 0.0f;
    private float alphaT = 0.0f;
    private float maxT = 2.0f;

    // Use this for initialization
    void Start()
    {

	}
	
	// Update is called once per frame
	void Update()
    {
        if (t < 1.0f)
        {
            t += Time.deltaTime;
            return;
        }
        float alpha = Mathf.Lerp(1, 0, alphaT / maxT);
        if (alpha == 0)
        {
            Object.Destroy(this);
        } else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            alphaT += Time.deltaTime;
        }

    }

}
