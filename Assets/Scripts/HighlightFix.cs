using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class HighlightFix : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GetComponent<Selectable>().OnDeselect(null);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        GetComponent<Selectable>().OnDeselect(null);
    }

}