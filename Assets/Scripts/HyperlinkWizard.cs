using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperlinkWizard : MonoBehaviour {
    
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

}
