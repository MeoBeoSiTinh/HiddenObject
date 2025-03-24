using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class CloseDesc : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            CheckForClickOutside();
        }
    }

    private void CheckForClickOutside()
    {
        // Check if we clicked on any UI element
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool clickedInside = false;

        foreach (RaycastResult result in results)
        {
            // Check if clicked object is this UI element or its child
            if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
            {
                clickedInside = true;
                break;
            }
        }

        // If click was outside, deactivate
        if (!clickedInside)
        {
            gameObject.SetActive(false);
        }
    }
}