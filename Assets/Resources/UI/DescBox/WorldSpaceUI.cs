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
    private float smoothSpeed = 0f;

    private RectTransform _rectTransform;
    private Canvas _parentCanvas;
    private Camera _canvasCamera;
    private Vector3[] _worldCorners = new Vector3[4];

    private void Awake()
    {
        
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
        }
    }

    private void LateUpdate()
    {

        UpdateUIPosition();
    }

    private void UpdateUIPosition()
    {
        // Calculate target world position
        Vector3 targetWorldPos = new Vector3(0, 0, 0) + worldOffset;

        // Convert to viewport position (0-1 space)
        Vector3 viewportPos = _canvasCamera.WorldToViewportPoint(targetWorldPos);

        // Convert to canvas space
        Vector2 canvasPos = new Vector2(
            (viewportPos.x * _parentCanvas.pixelRect.width) - (_parentCanvas.pixelRect.width * 0.5f),
            (viewportPos.y * _parentCanvas.pixelRect.height) - (_parentCanvas.pixelRect.height * 0.5f)
        );

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
        UpdateUIPosition();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, 0) + worldOffset);
        Gizmos.DrawSphere(new Vector3(0, 0, 0) + worldOffset, 0.1f);
    }
#endif
}