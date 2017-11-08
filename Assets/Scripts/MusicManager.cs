using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour, IObservable
{

    [Tooltip("Global music setting. If disabled, music does not play at all. Useful for recording sound without music.")]
    public bool isEnabled = true;
    
    private static MusicManager _instance;

    private AudioSource menuMusic;
    private IList<IObserver> observers = new List<IObserver>();
    private float deltaTime;
    private Coroutine coroutine;

    private const float GAME_OVER_FADE_OUT_TIME = 2.0f;

    private MusicManager()
    {

    }

    public bool isMuted { get { return menuMusic.mute; } }

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
            Unmute();
        }
        else
        {
            Mute();
        }
    }

    void Mute()
    {
        menuMusic.mute = true;
        NotifyAll(EventType.MUTE);
    }

    void Unmute()
    {
        menuMusic.mute = false;
        NotifyAll(EventType.UNMUTE);
    }
   
    public void GameOver()
    {
        coroutine = StartCoroutine(GameOverFadeOut());
    }

    IEnumerator GameOverFadeOut()
    {
        while (deltaTime < GAME_OVER_FADE_OUT_TIME)
        {
            menuMusic.pitch = Mathf.SmoothStep(1, 0, deltaTime / GAME_OVER_FADE_OUT_TIME);
            deltaTime += Time.deltaTime;
            yield return null;
        }

        menuMusic.Stop();
        deltaTime = 0;
        menuMusic.pitch = 1;
    }

    public void StartMusicAfterGameOver()
    {
        StartCoroutine(WaitForGameOverFadeOut(coroutine));
    }

    IEnumerator WaitForGameOverFadeOut(Coroutine c)
    {
        yield return c;
        if (!menuMusic.isPlaying)
        {
            menuMusic.Play();
        }   
    }

    /********************************************
     * Implementation of the Observer interface *
     ********************************************/

    public void Subscribe(IObserver observer)
    {
        observers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void NotifyAll(EventType e)
    {
        foreach (IObserver observer in observers)
        {
            observer.Notify(e);
        }
    }

}