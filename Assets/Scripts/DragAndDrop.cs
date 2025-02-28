using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Transform parentTransform;
    public GameObject targetObject; // Cache the target object

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        parentTransform = rectTransform.parent; // Store the original parent
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        startPosition = rectTransform.anchoredPosition;

        // Disable CameraHandle script from the main camera
        Camera.main.GetComponent<CameraHandle>().enabled = false;

    }

    public void OnDrag(PointerEventData eventData)
    {
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
        // Re-enable CameraHandle script
        Camera.main.GetComponent<CameraHandle>().enabled = true;

        canvasGroup.blocksRaycasts = true;

        // Check if the drop position is valid (e.g., over the game scene)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Dropped on the GameScene");
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPosition.z = 0; // Ensure the Z position is correct for 2D

            // Move the target to the drop location
            if (targetObject != null)
            {
                targetObject.transform.position = worldPosition;
                targetObject.SetActive(true);

            }
        }

        // Return the UI element to its original position
        rectTransform.SetParent(parentTransform);
        rectTransform.anchoredPosition = startPosition;
    }
}