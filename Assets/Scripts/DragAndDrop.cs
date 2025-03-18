using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Transform parentTransform;
    public GameObject targetObject; // Cache the target object
    public Transform backgroundCanvas; // Reference to the background canvas
    private CameraHandle cam;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cam = Camera.main.GetComponent<CameraHandle>();
        parentTransform = rectTransform.parent; // Store the original parent
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetObject == null) return; // Only allow dragging if targetObject is not null

        canvasGroup.blocksRaycasts = false;
        startPosition = rectTransform.anchoredPosition;

        // Disable CameraHandle script from the main camera
        cam.uiDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetObject == null) return; // Only allow dragging if targetObject is not null

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (targetObject == null) return; // Only allow dragging if targetObject is not null

        // Re-enable CameraHandle script
        cam.uiDragging = false;

        canvasGroup.blocksRaycasts = true;

        // Check if the drop position is valid (e.g., over the game scene)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            // Change the background canvas color
            //if (backgroundCanvas != null)
            //{
            //    backgroundCanvas.GetComponent<Image>().color = Color.green; // Change to desired color
            //}
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = eventData.position;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

            if (hits.Length > 0)
            {
                // Sort hits by sorting layer and sorting order
                System.Array.Sort(hits, (a, b) =>
                {
                    SpriteRenderer rendererA = a.collider.GetComponent<SpriteRenderer>();
                    SpriteRenderer rendererB = b.collider.GetComponent<SpriteRenderer>();

                    if (rendererA != null && rendererB != null)
                    {
                        // Compare sorting layers
                        int layerComparison = SortingLayer.GetLayerValueFromID(rendererA.sortingLayerID)
                            .CompareTo(SortingLayer.GetLayerValueFromID(rendererB.sortingLayerID));

                        if (layerComparison != 0)
                            return layerComparison;

                        // Compare sorting orders within the same layer
                        return rendererB.sortingOrder.CompareTo(rendererA.sortingOrder);
                    }

                    // If no SpriteRenderer, fall back to distance
                    return a.distance.CompareTo(b.distance);
                });

                // Get the topmost hit
                RaycastHit2D topmostHit = hits[0];
                Special special = topmostHit.collider.GetComponent<Special>();
                if (special != null)
                {
                    Debug.Log("Special object found");
                    special.specialFound(targetObject, eventData.position);
                }
            }


        }
        else
        {
            return;
            
        }


        // Return the UI element to its original position
        rectTransform.SetParent(parentTransform);
        rectTransform.anchoredPosition = startPosition;
    }
}
