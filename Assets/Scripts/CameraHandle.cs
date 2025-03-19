using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraHandle : MonoBehaviour
{
    public GameObject camera_GameObject;
    public GameObject background_GameObject; // Add a reference to the background GameObject
    private float prevMagnitude = 0;
    private float touchCount = 0;
    bool isZooming;
    public bool uiDragging = false;
    private Camera cam;
    private float dragSpeed = 0.5f; // Adjust drag speed for smoothness
    private float zoomSpeed = 0.01f; // Adjust zoom speed for smoothness
    private float zoomVelocity = 0.1f; // Velocity for smooth zooming

    // Define the stages
    public int currentStage;

    // Define min and max zoom levels
    private float minZoom = 2.5f;
    private float maxZoom = 12f;

    void Start()
    {
        cam = camera_GameObject.GetComponent<Camera>();
        var touch0contact = new InputAction
        (
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch0/press"
        );
        touch0contact.Enable();
        var touch1contact = new InputAction
        (
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch1/press"
        );
        touch1contact.Enable();

        touch0contact.performed += _ =>
        {
            touchCount++;
        };
        touch1contact.performed += _ =>
        {
            touchCount++;
        };
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

        var touch0Pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/position"
        );
        touch0Pos.Enable();
        var touch1Pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/position"
        );
        touch1Pos.Enable();
        touch1Pos.performed += _ =>
        {
            if (touchCount != 2 || IsPointerOverUI(touch0Pos.ReadValue<Vector2>()) || IsPointerOverUI(touch1Pos.ReadValue<Vector2>()) || uiDragging)
            {
                return;
            }
            var magnitude = (touch1Pos.ReadValue<Vector2>() - touch0Pos.ReadValue<Vector2>()).magnitude;
            if (prevMagnitude == 0)
            {
                prevMagnitude = magnitude;
            }
            var difference = prevMagnitude - magnitude;
            prevMagnitude = magnitude;
            cameraZoom(difference * zoomSpeed);
            isZooming = true;
        };

        var touch0Drag = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/delta"
        );
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
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = touchPosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0; // Returns true if any UI element is hit
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentStage)
        {
            case 0:
                break;
            case 1:
                minZoom = 2.5f;
                maxZoom = 14f;
                break;
            case 2:
                minZoom = 2.5f;
                maxZoom = 25f;
                break;
            case 3:
                minZoom = 2.5f;
                maxZoom = 25f;
                break;
        }
    }

    private void cameraZoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + increment, minZoom, maxZoom);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, cam.orthographicSize + increment, ref zoomVelocity, 0.2f, Mathf.Infinity, Time.deltaTime);
        RestrictCameraPosition(); // Restrict the camera position after zooming
    }

    public void RestrictCameraPosition()
    {
        if (background_GameObject == null)
        {
            return;
        }
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
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.min.x + stageWidth - horzExtent + 2;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 6;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 1:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 6;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 2:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 5 / 2;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 3:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 5 / 2;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            default:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
        }

        // Clamp the camera's position to stay within the bounds
        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.position = pos;
    }
    public bool IsCameraTouchingBorder()
    {
        if (background_GameObject == null)
        {
            return false;
        }

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
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.min.x + stageWidth - horzExtent + 2;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 6;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 1:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 6;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 2:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 5 / 2;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 3:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 5 / 2;
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

}
