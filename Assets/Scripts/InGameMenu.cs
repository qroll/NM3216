using UnityEngine;

public class InGameMenu : MonoBehaviour, IObserver
{

    public GameObject cross;

    public float distFromCamera = 10.0f;

    void Start()
    {
        MusicManager.Instance.Subscribe(this);

        // Position the pause button at the top right corner of the game map
        RectTransform canvas = GetComponent<RectTransform>();
        RectTransform menu = transform.Find("Menu").GetComponent<RectTransform>();
        float point = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.95f, distFromCamera)).y;
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(new Vector2(point, point));
        float x = (viewportPos.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f);
        float y = (viewportPos.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f);
        Vector2 rectTransformPos = new Vector2(x, y);

        menu.anchoredPosition = rectTransformPos;
    }

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
