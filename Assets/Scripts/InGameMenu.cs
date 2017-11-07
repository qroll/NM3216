using UnityEngine;

public class InGameMenu : MonoBehaviour, IObserver
{

    public float distFromCamera = 10.0f;

    public GameObject cross;

    void Start()
    {
        // Subscribe to and initialize music mute status
        MusicManager.Instance.Subscribe(this);
        cross.SetActive(MusicManager.Instance.isMuted);

        // Position the menu at the top right corner of the screen
        // First, convert coordinates from world space to canvas space
        RectTransform canvas = GetComponent<RectTransform>();
        float point = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.95f, distFromCamera)).y;
        Vector2 viewportPoint = Camera.main.WorldToViewportPoint(new Vector2(point, point));
        float x = (viewportPoint.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f);
        float y = (viewportPoint.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f);
        // Then set the coordinates of the menu
        RectTransform menu = transform.Find("Menu").GetComponent<RectTransform>();
        menu.anchoredPosition = new Vector2(x, y);
    }

    void OnDestroy()
    {
        MusicManager.Instance.Unsubscribe(this);
    }

    /**********************************************
     * Implementation of the Observable interface *
     **********************************************/

    public void Notify(EventType e)
    {
        switch (e)
        {
            case EventType.MUTE:
                cross.SetActive(true);
                break;
            case EventType.UNMUTE:
                cross.SetActive(false);
                break;
            default:
                break;
        }
    }

}
