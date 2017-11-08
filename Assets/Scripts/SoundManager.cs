using UnityEngine;

public class SoundManager : MonoBehaviour
{
    
    public AudioClip buttonSelected;

    private static SoundManager _instance;

    private AudioSource soundManager;

    private SoundManager()
    {

    }
    
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<SoundManager>();
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
            soundManager = GetComponent<AudioSource>();
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }

    }

    public void ButtonSelected()
    {
        soundManager.PlayOneShot(buttonSelected);
    }
    
}