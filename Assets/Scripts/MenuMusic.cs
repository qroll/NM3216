using System.Collections;
using UnityEngine;

public class MenuMusic : MonoBehaviour {

    private AudioSource menuMusic;
    private float currentVol;
    private float deltaTime = 0;

    void Start () {
        Object.DontDestroyOnLoad(transform.gameObject);

        menuMusic = GetComponent<AudioSource>();
        currentVol = menuMusic.volume;
    }

    public void Fade()
    {
        StartCoroutine(FadeMenuMusic());
    }

    IEnumerator FadeMenuMusic()
    {
        while (menuMusic.volume > 0)
        {
            menuMusic.volume = Mathf.Lerp(currentVol, 0, deltaTime / 1.0f);
            deltaTime += Time.deltaTime;
            yield return null;
        }
        Object.Destroy(menuMusic.gameObject);
    }

}
