using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{

    public Text loadingText;
    public GameObject helpUI;
    public GameObject mainUI;

    public AudioSource menuMusic;

    private bool loading;
    private float currentVol;
    private float deltaTime = 0;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1.0f;
        loadingText.enabled = false;

        currentVol = menuMusic.volume;

        // Screen.SetResolution(Screen.height, Screen.height, Screen.fullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        if (loading)
        {
            menuMusic.volume = Mathf.Lerp(currentVol, 0, deltaTime/2.0f);
            deltaTime += Time.deltaTime;
            loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));
        }
    }

    IEnumerator LoadGame()
    {
        loading = true;
        // yield return new WaitForSeconds(3);

        AsyncOperation operation = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
        operation.allowSceneActivation = false;

        //Wait until the last operation fully loads to return anything
        while (operation.progress < 0.9f || menuMusic.volume > 0f)
        {
            yield return null;
        }

        operation.allowSceneActivation = true;
    }

    public void OnPlayClick()
    {
        loadingText.enabled = true;
        StartCoroutine(LoadGame());
    }

    public void OnHelpClick()
    {
        helpUI.SetActive(true);
        mainUI.SetActive(false);
    }

    public void OnMainClick()
    {
        helpUI.SetActive(false);
        mainUI.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
