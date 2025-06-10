using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DestroyOnOutsideClick : MonoBehaviour, IPointerDownHandler
{
    [Header("Settings")]
    [Tooltip("Time delay before destruction (seconds)")]
    public float destroyDelay = 0f;

    [Tooltip("Should child clicks also count as 'inside'?")]
    public bool includeChildren = true;

    private RectTransform _rectTransform;
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _raycaster = GetComponentInParent<GraphicRaycaster>();
        _eventSystem = EventSystem.current;

        if (_raycaster == null)
        {
            Debug.LogError("No GraphicRaycaster found in parent canvas!", this);
            enabled = false;
        }

        if (_eventSystem == null)
        {
            Debug.LogError("No EventSystem in scene!", this);
            enabled = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Ignore if clicking on this element or its children
        if (includeChildren && eventData.pointerEnter != null &&
            (eventData.pointerEnter == gameObject || eventData.pointerEnter.transform.IsChildOf(transform)))
        {
            return;
        }

        // Only proceed if clicking outside
        Destroy(gameObject, destroyDelay);
    }

    // Alternative detection for non-UI clicks
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            CheckForOutsideClick();
        }
    }

    private void CheckForOutsideClick()
    {
        PointerEventData eventData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
        _raycaster.Raycast(eventData, results);

        // Check if we hit anything that isn't this or its children
        bool hitValidTarget = false;
        foreach (var result in results)
        {
            if (result.gameObject == gameObject || (includeChildren && result.gameObject.transform.IsChildOf(transform)))
            {
                hitValidTarget = true;
                break;
            }
        }

        if (!hitValidTarget)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void OnDestroy()
    {
        // Clean up any pending delayed destruction
        CancelInvoke();
    }
}