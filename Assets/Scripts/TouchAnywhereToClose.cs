using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchAnywhereToClose : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if we clicked directly on this background panel (not a child UI element)
        if (eventData.pointerEnter == gameObject)
        {
            gameObject.SetActive(false);
        }
    }
}
