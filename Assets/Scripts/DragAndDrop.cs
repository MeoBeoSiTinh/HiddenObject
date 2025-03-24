using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Transform parentTransform;
    public GameObject targetObject; // Cache the target object
    public Transform backgroundCanvas; // Reference to the background canvas
    private CameraHandle cam;

    private bool isDragging = false;
    private Coroutine holdCoroutine;
    public float holdDuration = 0f;
    private Camera eventCamera;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cam = Camera.main.GetComponent<CameraHandle>();
        parentTransform = rectTransform.parent; // Store the original parent
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetObject == null) return;

        eventCamera = eventData.pressEventCamera;
        startPosition = rectTransform.anchoredPosition; // Store the initial position on pointer down
        holdCoroutine = StartCoroutine(HoldRoutine(eventData));
    }

    private IEnumerator HoldRoutine(PointerEventData eventData)
    {
        float timer = 0;
        Vector2 initialTouchPosition = eventData.position; // Store the initial touch position

        while (timer < holdDuration)
        {
            if (targetObject == null)
                yield break;

            // Check if the touch position has changed
            //if ((Vector2)Input.mousePosition != initialTouchPosition)
            //{
            //    Debug.Log("Touch position changed, cancelling hold");
            //    yield break;
            //}

            timer += Time.deltaTime;
            yield return null;
        }

        if (targetObject == null)
            yield break;

        StartDragging();
    }

    private void StartDragging()
    {
        isDragging = true;
        canvasGroup.blocksRaycasts = false;
        startPosition = rectTransform.anchoredPosition;
        rectTransform.SetParent(backgroundCanvas);
        cam.uiDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        if (isDragging)
        {
            EndDragging(eventData);
        }

        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            // Update position based on current mouse position
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                Input.mousePosition,
                eventCamera,
                out Vector2 localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }
    }

    private void EndDragging(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        cam.uiDragging = false;

        // Check if dropping over the game scene
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 worldPosition = eventCamera.ScreenToWorldPoint(eventData.position);
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

            if (hits.Length > 0)
            {
                // Sorting logic here (same as original)
                System.Array.Sort(hits, (a, b) =>
                {
                    SpriteRenderer rendererA = a.collider.GetComponent<SpriteRenderer>();
                    SpriteRenderer rendererB = b.collider.GetComponent<SpriteRenderer>();

                    if (rendererA != null && rendererB != null)
                    {
                        int layerComparison = SortingLayer.GetLayerValueFromID(rendererA.sortingLayerID)
                            .CompareTo(SortingLayer.GetLayerValueFromID(rendererB.sortingLayerID));
                        if (layerComparison != 0) return layerComparison;
                        return rendererB.sortingOrder.CompareTo(rendererA.sortingOrder);
                    }
                    return a.distance.CompareTo(b.distance);
                });

                RaycastHit2D topmostHit = hits[0];
                Special special = topmostHit.collider.GetComponent<Special>();
                if (special != null)
                {
                    special.specialFound(targetObject, eventData.position);
                }
            }
        }

        // Reset UI element
        rectTransform.SetParent(parentTransform);
        rectTransform.anchoredPosition = startPosition;
    }
}