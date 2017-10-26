using UnityEngine;

public class Blind : MonoBehaviour
{

    private float elapsedTime = 0.0f;
    private float fadeTime = 0.0f;

    private const float DURATION = 1.0f;
    private const float FADE_DURATION = 2.0f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // keep at full brightness for the specified duration
        if (elapsedTime < DURATION)
        {
            elapsedTime += Time.deltaTime;
            return;
        }

        // fade over time
        float alpha = Mathf.Lerp(1, 0, fadeTime / FADE_DURATION);
        if (alpha == 0)
        {
            Object.Destroy(this);
        }
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            fadeTime += Time.deltaTime;
        }

    }

}
