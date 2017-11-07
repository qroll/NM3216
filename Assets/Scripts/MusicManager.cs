using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour, IObservable
{

    [Tooltip("Global music setting. If disabled, music does not play at all. Useful for recording sound without music.")]
    public bool isEnabled = true;
    
    private static MusicManager _instance;

    private AudioSource menuMusic;
    private IList<IObserver> observers = new List<IObserver>();

    private MusicManager()
    {

    }

    public bool isMuted { get; private set; }

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
            Mute();
        }
        else
        {
            Unmute();
        }
    }

    void Mute()
    {
        ToggleMenuMusic(false);
        isMuted = false;
        NotifyAll(EventType.UNMUTE);
    }

    void Unmute()
    {
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