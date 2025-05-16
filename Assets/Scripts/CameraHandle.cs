using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraHandle : MonoBehaviour
{
    public GameObject camera_GameObject;
    public GameObject background_GameObject;
    private float prevMagnitude = 0;
    private float touchCount = 0;
    bool isZooming;
    public bool uiDragging = false;
    private Camera cam;
    private float dragSpeed = 0.5f;
    private float zoomSpeed = 0.01f;
    private float zoomVelocity = 0.1f;

    // Bounce effect variables
    private bool isBouncing = false;
    private Vector3 bounceDirection;
    private float bounceDecay = 0.9f; // How quickly the bounce settles (0.8-0.95 feels good)
    private float bounceIntensity = 0.4f; // How strong the bounce is

    // Define the stages
    public int currentStage;

    // Define min and max zoom levels
    private float minZoom;
    private float maxZoom;

    void Start()
    {
        cam = camera_GameObject.GetComponent<Camera>();

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
        };
        touch1contact.canceled += _ =>
        {
            touchCount--;
            prevMagnitude = 0;
            isZooming = false;
        };

        var touch0Pos = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch0/position");
        var touch1Pos = new InputAction(type: InputActionType.Value, binding: "<Touchscreen>/touch1/position");
        touch0Pos.Enable();
        touch1Pos.Enable();

        touch1Pos.performed += _ =>
        {
            if (touchCount != 2 || IsPointerOverUI(touch0Pos.ReadValue<Vector2>()) || IsPointerOverUI(touch1Pos.ReadValue<Vector2>()) || uiDragging)
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
            if (touchCount == 1 && !isZooming && !IsPointerOverUI(touch0Pos.ReadValue<Vector2>()) && !uiDragging)
            {
                Vector2 delta = ctx.ReadValue<Vector2>();
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * dragSpeed * Time.deltaTime;
                camera_GameObject.transform.Translate(move);
                RestrictCameraPosition();
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
        switch (currentStage)
        {
            case 0:
                minZoom = 3f;
                maxZoom = 15f;
                break;
            case 1:
                minZoom = 3f;
                maxZoom = 25f;
                break;
        }

        // Apply bounce effect if active
        if (isBouncing && touchCount == 0)
        {
            camera_GameObject.transform.position += bounceDirection * bounceIntensity;
            bounceDirection *= bounceDecay; // Reduce bounce over time

            // Stop bouncing when barely moving
            if (bounceDirection.magnitude < 0.02f)
            {
                isBouncing = false;
            }
        }
    }

    private void cameraZoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + increment, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, cam.orthographicSize + increment, ref zoomVelocity, 0.2f, Mathf.Infinity, Time.deltaTime);
        RestrictCameraPosition();
    }

    public void RestrictCameraPosition()
    {
        if (background_GameObject == null)
            return;

        Camera cam = camera_GameObject.GetComponent<Camera>();
        Bounds backgroundBounds = background_GameObject.GetComponent<SpriteRenderer>().bounds;

        // Calculate the maximum orthographic size based on the background bounds  
        float maxOrthographicSize = Mathf.Min(backgroundBounds.size.x * Screen.height / Screen.width, backgroundBounds.size.y) / 2;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, Mathf.Min(maxZoom, maxOrthographicSize));

        // Calculate the camera's viewable area  
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        // Calculate the bounds for each stage  
        float stageWidth = backgroundBounds.size.x / 2;
        float stageHeight = backgroundBounds.size.y / 2;

        float minX, maxX, minY, maxY;

        switch (currentStage)
        {
            case 0:
                minX = backgroundBounds.center.x - stageWidth / 2 + cam.orthographicSize * 0.5f;
                maxX = backgroundBounds.center.x + stageWidth / 2 - cam.orthographicSize * 0.5f;
                minY = backgroundBounds.center.y - stageHeight / 2 + cam.orthographicSize * 0.5f;
                maxY = backgroundBounds.center.y + stageHeight / 2 - cam.orthographicSize * 0.5f;
                break;
            case 1:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - (1.4f * 1/8 * cam.orthographicSize) - 1 / 4 * cam.orthographicSize;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            default:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
        }

        // CLAMP FIRST (original behavior)  
        Vector3 oldPos = camera_GameObject.transform.position;
        Vector3 clampedPos = oldPos;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);
        camera_GameObject.transform.position = clampedPos;

        // Calculate bounce direction if we hit a border  
        if (clampedPos != oldPos)
        {
            isBouncing = true;
            bounceDirection = (clampedPos - oldPos).normalized * 0.5f; // Small initial pushback  
        }
    }

    public bool IsCameraTouchingBorder()
    {
        if (background_GameObject == null)
            return false;

        Camera cam = camera_GameObject.GetComponent<Camera>();
        Bounds backgroundBounds = background_GameObject.GetComponent<SpriteRenderer>().bounds;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float stageWidth = backgroundBounds.size.x / 2;
        float stageHeight = backgroundBounds.size.y / 2;

        float minX, maxX, minY, maxY;

        switch (currentStage)
        {
            case 0:
                minX = backgroundBounds.center.x - stageWidth / 2;
                maxX = backgroundBounds.center.x + stageWidth / 2;
                minY = backgroundBounds.center.y - stageHeight / 2;
                maxY = backgroundBounds.center.y + stageHeight / 2;
                break;
            case 1:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 4f - 1 / 4 * cam.orthographicSize;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            default:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
        }

        Vector3 pos = cam.transform.position;
        return pos.x <= minX || pos.x >= maxX || pos.y <= minY || pos.y >= maxY;
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        StartCoroutine(PerformCameraShake(duration, magnitude));
    }

    private IEnumerator PerformCameraShake(float duration, float magnitude)
    {
        Vector3 originalPosition = camera_GameObject.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            camera_GameObject.transform.localPosition = new Vector3(originalPosition.x + offsetX, originalPosition.y + offsetY, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camera_GameObject.transform.localPosition = originalPosition;
    }
}