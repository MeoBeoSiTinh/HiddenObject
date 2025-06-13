using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public Bounds backgroundBounds;
    private float prevMagnitude = 0;
    private float touchCount = 0;
    bool isZooming;
    private Camera cam;
    private float dragSpeed = 0.5f;
    private float zoomSpeed = 0.01f;
    private float zoomVelocity = 0.1f;
    public bool allowed = true;

    // Bounce effect variables for position
    private bool isBouncing = false;
    private Vector3 bounceDirection;
    private float bounceDecay = 0.9f; // How quickly the bounce settles
    public float bounceIntensity = 0.4f; // How strong the bounce is

    // Zoom bounce variables
    private bool isZoomBouncing = false;
    private float zoomBounceDuration = 0.6f; // Duration of zoom bounce-back

    // Define the stages
    public int currentStage;

    // Define min and max zoom levels
    public float minZoom = 5f;
    public float maxZoom = 8f;
    private readonly float zoomOvershoot = 1f; // Allow temporary overshoot

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        cam = gameObject.GetComponent<Camera>();

        var touch0contact = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/touch0/press");
        var touch1contact = new InputAction(type: InputActionType.Button, binding: "<Touchscreen>/touch1/press");
        touch0contact.Enable();
        touch1contact.Enable();

        touch0contact.performed += _ => touchCount++;
        touch1contact.performed += _ => touchCount++;
        touch0contact.canceled += _ =>
        {
            touchCount--;
            prevMagnitude = 0;
            isZooming = false;
            CheckZoomBounce();
        };
        touch1contact.canceled += _ =>
        {
            touchCount--;
            prevMagnitude = 0;
            isZooming = false;
            CheckZoomBounce();
        };

        var touch0Pos = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch0/position");
        var touch1Pos = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch1/position");
        touch0Pos.Enable();
        touch1Pos.Enable();

        touch1Pos.performed += _ =>
        {
            if (touchCount != 2 || IsPointerOverUI(touch0Pos.ReadValue<Vector2>()) || IsPointerOverUI(touch1Pos.ReadValue<Vector2>()) || !allowed)
                return;

            var magnitude = (touch1Pos.ReadValue<Vector2>() - touch0Pos.ReadValue<Vector2>()).magnitude;
            if (prevMagnitude == 0)
                prevMagnitude = magnitude;

            var difference = prevMagnitude - magnitude;
            prevMagnitude = magnitude;
            cameraZoom(difference * zoomSpeed);
            isZooming = true;
        };

        var touch0Drag = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch0/delta");
        touch0Drag.Enable();
        touch0Drag.performed += ctx =>
        {
            if (touchCount == 1 && !isZooming && !IsPointerOverUI(touch0Pos.ReadValue<Vector2>()) && allowed)
            {
                Vector2 delta = ctx.ReadValue<Vector2>();
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed * Time.deltaTime;
                gameObject.transform.Translate(move);
            }
        };
    }

    private bool IsPointerOverUI(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = touchPosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void Update()
    {
        // Apply position bounce effect if active
        if (isBouncing && touchCount == 0)
        {
            gameObject.transform.position += bounceDirection * 0.05f * cam.orthographicSize;
            bounceDirection *= bounceDecay; // Reduce bounce over time

            // Stop bouncing when barely moving
            if (bounceDirection.magnitude < 0.02f)
            {
                isBouncing = false;
            }
        }
        RestrictCameraPosition();
    }

    private void cameraZoom(float increment)
    {
        if (isZoomBouncing) return; // Prevent zooming during bounce-back

        // Apply zoom increment
        float targetSize = cam.orthographicSize + increment;
        targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom + zoomOvershoot);

        // Smoothly apply zoom
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref zoomVelocity, 0.05f, Mathf.Infinity, Time.deltaTime);
    }

    private void CheckZoomBounce()
    {
        if (touchCount == 0 && !isZoomBouncing && cam.orthographicSize > maxZoom + 0.001f) // Small tolerance
        {
            isZoomBouncing = true;
            LeanTween.value(gameObject, cam.orthographicSize, maxZoom, zoomBounceDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setOnUpdate((float size) => cam.orthographicSize = size)
                .setOnComplete(() =>
                {
                    isZoomBouncing = false;
                    cam.orthographicSize = maxZoom; // Ensure exact value
                    RestrictCameraPosition();
                });
        }
    }

    public void RestrictCameraPosition()
    {
        if (backgroundBounds == null)
            return;

        // Calculate the maximum orthographic size based on the background bounds
        float maxOrthographicSize = Mathf.Min(backgroundBounds.size.x * Screen.height / Screen.width, backgroundBounds.size.y) / 2;

        // Allow temporary overshoot up to maxZoom + zoomOvershoot, but clamp for final bounds
        if (!isZoomBouncing) // Skip clamping during bounce-back animation
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, Mathf.Min(maxZoom + zoomOvershoot, maxOrthographicSize));
        }

        // Calculate the camera's viewable area
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculate the bounds for each stage
        float stageWidth = backgroundBounds.size.x / 2;
        float stageHeight = backgroundBounds.size.y / 2;

        float minX = backgroundBounds.min.x + horzExtent;
        float maxX = backgroundBounds.max.x - horzExtent;
        float minY = backgroundBounds.min.y + vertExtent - (0.1f * cam.orthographicSize);
        float maxY = backgroundBounds.max.y - vertExtent;

        // CLAMP FIRST
        Vector3 oldPos = gameObject.transform.position;
        Vector3 clampedPos = oldPos;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);
        gameObject.transform.position = clampedPos;

        // Calculate bounce direction if we hit a border
        if (clampedPos != oldPos)
        {
            isBouncing = true;
            bounceDirection = (clampedPos - oldPos).normalized * bounceIntensity;
        }
    }

    public bool IsCameraTouchingBorder()
    {
        if (backgroundBounds == null)
            return false;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = backgroundBounds.min.x + horzExtent;
        float maxX = backgroundBounds.max.x - horzExtent;
        float minY = backgroundBounds.min.y + vertExtent - (0.1f * cam.orthographicSize);
        float maxY = backgroundBounds.max.y - vertExtent;

        Vector3 pos = cam.transform.position;
        return pos.x <= minX || pos.x >= maxX || pos.y <= minY || pos.y >= maxY;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(PerformCameraShake(duration, magnitude));
    }

    private IEnumerator PerformCameraShake(float duration, float magnitude)
    {
        Vector3 originalPosition = gameObject.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            gameObject.transform.localPosition = new Vector3(originalPosition.x + offsetX, originalPosition.y + offsetY, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.transform.localPosition = originalPosition;
    }

    public void updateBounds(Bounds newBound)
    {
        backgroundBounds = newBound;
        RestrictCameraPosition();
    }

    public IEnumerator MoveCamera(Vector3 targetPosition, float targetSize, float duration)
    {
        targetPosition.z = -10f;

        // Store start values
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;

        // Use LeanTween for position and zoom
        LeanTween.move(Camera.main.gameObject, targetPosition, duration)
            .setEase(LeanTweenType.easeInOutQuart); // Gentle sine easing
        LeanTween.value(Camera.main.gameObject, startSize, targetSize, duration)
            .setEase(LeanTweenType.easeInOutQuart)
            .setOnUpdate((float size) => Camera.main.orthographicSize = size);

        // Wait for LeanTween to complete
        yield return new WaitForSeconds(duration);

        // Ensure exact values
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = targetSize;
    }
}