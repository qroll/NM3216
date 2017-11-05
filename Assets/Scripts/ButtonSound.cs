using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, ISelectHandler
{

    private AudioSource buttonSound;

    void Start()
    {
        buttonSound = GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (buttonSound != null)
        {
            buttonSound.Play();
        }
    }

    public void PlaySound()
    {
        if (buttonSound != null)
        {
            buttonSound.Play();
        }
    }

}
