using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneController : MonoBehaviour
{
    
    public void OnMainMenuClick()
    {
        SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
    }

}
