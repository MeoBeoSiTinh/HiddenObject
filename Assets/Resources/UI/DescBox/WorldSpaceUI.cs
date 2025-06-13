using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class WorldSpaceUI : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The world-space object to follow")]
    public Transform target;

    [Tooltip("Offset from the target's world position")]
    public Vector3 worldOffset = Vector3.up;

    [Header("UI Behavior")]
    [Tooltip("Smooth movement speed (0 for instant)")]
    [SerializeField] public float smoothSpeed = 0f;

    [Tooltip("Padding from screen edges when clamping (in pixels)")]
    [SerializeField] public float edgePadding = 20f;

    [Tooltip("Hotbar UI to stay above")]
    [SerializeField] private RectTransform hotbarRect;

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    private Camera _canvasCamera;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _rectTransform = GetComponent<RectTransform>();
        _parentCanvas = GetComponentInParent<Canvas>();

        if (_parentCanvas == null || _parentCanvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.LogError("WorldSpaceUI requires a Screen Space-Camera canvas parent!", this);
            enabled = false;
            return;
        }

        _canvasCamera = _parentCanvas.worldCamera;
        if (_canvasCamera == null)
        {
            Debug.LogError("No camera assigned to canvas!", this);
            enabled = false;
            return;
        }

        // Optional: Find hotbar if not assigned in Inspector
        if (hotbarRect == null)
        {
            GameObject hotbar = GameObject.Find("Hotbar");
            if (hotbar != null)
            {
                hotbarRect = hotbar.GetComponent<RectTransform>();
                if (hotbarRect == null)
                {
                    Debug.LogWarning("Hotbar GameObject found but has no RectTransform!", this);
                }
            }
            else
            {
                Debug.LogWarning("Hotbar GameObject not found. Assign hotbarRect in Inspector.", this);
            }
        }
        if (target == null)
        {
            target = GameManager.Instance.transform;
        }

    }

    private void LateUpdate()
    {
        if (target != null)
        {
            UpdateUIPosition();
        }
        else
        {
            Debug.LogWarning("Target is not assigned!", this);
        }
    }

    private void UpdateUIPosition()
    {
        // Calculate target world position
        Vector3 targetWorldPos = target.position + worldOffset;

        // Convert to viewport position (0-1 space)
        Vector3 viewportPos = _canvasCamera.WorldToViewportPoint(targetWorldPos);

        // Check if the target is on-screen (including some buffer for the UI size)
        bool isOnScreen = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

        // Convert to canvas space
        Vector2 canvasPos;
        if (isOnScreen)
        {
            // On-screen: use standard canvas position
            canvasPos = new Vector2(
                (viewportPos.x * _parentCanvas.pixelRect.width) - (_parentCanvas.pixelRect.width * 0.5f),
                (viewportPos.y * _parentCanvas.pixelRect.height) - (_parentCanvas.pixelRect.height * 0.5f)
            );
        }
        else
        {
            // Off-screen: clamp to screen edges in the direction of the target
            Vector2 viewportCenter = new Vector2(0.5f, 0.5f);
            Vector2 direction = new Vector2(viewportPos.x, viewportPos.y) - viewportCenter;

            // Normalize direction and scale to reach screen edge (accounting for aspect ratio)
            float aspectRatio = _parentCanvas.pixelRect.width / _parentCanvas.pixelRect.height;
            direction.x *= aspectRatio; // Adjust for non-square screens
            float maxExtent = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
            if (maxExtent > 0)
            {
                direction /= maxExtent; // Scale to unit vector along the longest axis
            }

            // Calculate clamped viewport position (0 to 1, with padding)
            float paddingX = edgePadding / _parentCanvas.pixelRect.width;
            float paddingY = edgePadding / _parentCanvas.pixelRect.height;
            Vector2 clampedViewportPos = viewportCenter + direction * 0.5f; // Move to edge
            clampedViewportPos.x = Mathf.Clamp(clampedViewportPos.x, paddingX, 1f - paddingX);
            clampedViewportPos.y = Mathf.Clamp(clampedViewportPos.y, paddingY, 1f - paddingY);

            // Convert clamped viewport position to canvas space
            canvasPos = new Vector2(
                (clampedViewportPos.x * _parentCanvas.pixelRect.width) - (_parentCanvas.pixelRect.width * 0.5f),
                (clampedViewportPos.y * _parentCanvas.pixelRect.height) - (_parentCanvas.pixelRect.height * 0.5f)
            );
        }

        // Ensure the UI stays above the hotbar
        if (hotbarRect != null)
        {
            // Get hotbar's top edge in canvas space
            Vector3[] hotbarCorners = new Vector3[4];
            hotbarRect.GetWorldCorners(hotbarCorners);
            float hotbarTopY = _parentCanvas.transform.InverseTransformPoint(hotbarCorners[1]).y; // Top-left corner
            float uiHeight = _rectTransform.rect.height * _parentCanvas.scaleFactor;
            float minY = hotbarTopY + uiHeight * 0.5f + edgePadding; // Keep UI above hotbar with padding
            canvasPos.y = Mathf.Max(canvasPos.y, minY);
        }

        // Apply position
        if (smoothSpeed > 0)
        {
            _rectTransform.anchoredPosition = Vector2.Lerp(
                _rectTransform.anchoredPosition,
                canvasPos,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            _rectTransform.anchoredPosition = canvasPos;
        }
    }

    /// <summary>
    /// Call this if you change the target at runtime
    /// </summary>
    public void RefreshTarget(Transform newTarget)
    {
        target = newTarget;
        if (enabled && target != null)
        {
            UpdateUIPosition();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(target.position, target.position + worldOffset);
            Gizmos.DrawSphere(target.position + worldOffset, 0.1f);
        }
    }
#endif
}