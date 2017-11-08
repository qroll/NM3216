using UnityEngine;
using UnityEngine.UI;

public class ButtonSelected : MonoBehaviour
{

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        SoundManager.Instance.ButtonSelected();
    }

}
