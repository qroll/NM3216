using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour, IObservable
{

    public bool isEnabled = true;

    private AudioSource menuMusic;
    private  bool isMuted;
    /*
    private float maxVolume;
    private float currentVolume;
    private float deltaTime;
    private Coroutine coroutine;
    */
    private IList<IObserver> observers = new List<IObserver>();

    private static MusicManager _instance;

    private const float FADE_TIME = 1.5f;

    private MusicManager()
    {

    }

    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MusicManager>();
            }

            return _instance;
        }
    }

    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
            menuMusic = GetComponent<AudioSource>();
            // maxVolume = menuMusic.volume;

            if (!isEnabled)
            {
                menuMusic.Pause();
            }
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
        
    }

    public void Update()
    {
        if (!isEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            /*
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                currentVolume = menuMusic.volume;
            }
            */

            ToggleMute();
        }
    }

    public void ToggleMute()
    {
        if (!isEnabled)
        {
            return;
        }

        if (isMuted)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    void FadeIn()
    {
        // coroutine = StartCoroutine(FadeMenuMusic(currentVolume, maxVolume));
        ToggleMenuMusic(false);
        isMuted = false;
        NotifyAll(EventType.UNMUTE);
    }

    void FadeOut()
    {
        // coroutine = StartCoroutine(FadeMenuMusic(currentVolume, 0.0f));
        ToggleMenuMusic(true);
        isMuted = true;
        NotifyAll(EventType.MUTE);
    }

    void ToggleMenuMusic(bool toMute)
    {
        if (toMute)
        {
            menuMusic.Pause();
        }
        else
        {
            menuMusic.UnPause();
        }
    }

    // Appears to be causing race conditions, resulting in music being forcefully
    // replayed from the beginning
    /*
    IEnumerator FadeMenuMusic(float startVolume, float endVolume)
    {
        deltaTime = 0;

        if (endVolume > 0.0f)
        {
            menuMusic.UnPause();
        }

        while (!Mathf.Approximately(menuMusic.volume, endVolume))
        {
            menuMusic.volume = Mathf.SmoothStep(startVolume, endVolume, deltaTime / FADE_TIME);
            deltaTime += Time.deltaTime;
            yield return null;
        }

        if (Mathf.Approximately(menuMusic.volume, 0.0f))
        {
            menuMusic.Pause();
        }
    }
    */

    public void Subscribe(IObserver observer)
    {
        observers.Add(observer);
    }

    public void NotifyAll(EventType e)
    {
        foreach (IObserver observer in observers)
        {
            observer.Notify(e);
        }
    }

}