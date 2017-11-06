using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{

    public Text loadingText;
    public GameObject helpUI;
    public GameObject mainUI;
    public GameObject creditsUI;

    private bool loading;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1.0f;
        loadingText.enabled = false;

        // Screen.SetResolution(Screen.height, Screen.height, Screen.fullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        if (loading)
        {
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
        while (operation.progress < 0.9f)
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
        creditsUI.SetActive(false);
    }

    public void OnMainClick()
    {
        helpUI.SetActive(false);
        mainUI.SetActive(true);
        creditsUI.SetActive(false);
    }

    public void OnCreditsClick()
    {
        helpUI.SetActive(false);
        mainUI.SetActive(false);
        creditsUI.SetActive(true);
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
