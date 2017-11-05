using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{

    private AudioSource menuMusic;

    void Start()
    {
        AudioSource[] gos = FindObjectsOfType<AudioSource>();
        foreach (AudioSource go in gos)
        {
            if (go.name == "MenuMusic")
            {
                menuMusic = go.GetComponent<AudioSource>();
            }
        }

        MenuMusic script = menuMusic.GetComponent<MenuMusic>();
        script.Fade();
    }

    public void OnMainMenuClick()
    {
        SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
    }

}
