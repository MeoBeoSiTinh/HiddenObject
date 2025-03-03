using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class CameraHandle : MonoBehaviour
{
    public GameObject camera_GameObject;
    public GameObject background_GameObject; // Add a reference to the background GameObject

    Vector2 StartPosition;
    Vector2 DragStartPosition;
    Vector2 DragNewPosition;
    Vector2 Finger0Position;
    float DistanceBetweenFingers;
    bool isZooming;

    // Define the stages
    public int currentStage;

    // Define min and max zoom levels
    private float minZoom =2.5f;
    private float maxZoom =10f;


    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0 && isZooming)
        {
            isZooming = false;
        }

        if (Input.touchCount == 1)
        {
            if (!isZooming && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 NewPosition = GetWorldPosition();
                    Vector2 PositionDifference = NewPosition - StartPosition;
                    camera_GameObject.transform.Translate(-PositionDifference);
                    RestrictCameraPosition(); // Restrict the camera position after moving
                }
                StartPosition = GetWorldPosition();
            }
        }
        else if (Input.touchCount == 2)
        {
            if (Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                isZooming = true;

                DragNewPosition = GetWorldPositionOfFinger(1);
                Vector2 PositionDifference = DragNewPosition - DragStartPosition;

                if (Vector2.Distance(DragNewPosition, Finger0Position) < DistanceBetweenFingers)
                    camera_GameObject.GetComponent<Camera>().orthographicSize += (PositionDifference.magnitude);

                if (Vector2.Distance(DragNewPosition, Finger0Position) >= DistanceBetweenFingers)
                    camera_GameObject.GetComponent<Camera>().orthographicSize -= (PositionDifference.magnitude);
                switch (currentStage)
                {
                    case 0:
                        minZoom = 2.5f;
                        maxZoom = 10f;
                        break;
                    case 1:
                        minZoom = 2.5f;
                        maxZoom = 12f;
                        break;
                    case 2:
                        minZoom = 2.5f;
                        maxZoom = 20f;
                        break;
                    case 3:
                        minZoom = 2.5f;
                        maxZoom = 20f;
                        break;
                }
                // Restrict the zoom size to be unable to be larger than the background
                Camera cam = camera_GameObject.GetComponent<Camera>();
                Bounds backgroundBounds = background_GameObject.GetComponent<SpriteRenderer>().bounds;
                float maxZoomOut = Mathf.Min(backgroundBounds.size.x * Screen.height / Screen.width, backgroundBounds.size.y) / 2;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, Mathf.Min(maxZoom, maxZoomOut));

                DistanceBetweenFingers = Vector2.Distance(DragNewPosition, Finger0Position);
                RestrictCameraPosition(); // Restrict the camera position after zooming
            }
            DragStartPosition = GetWorldPositionOfFinger(1);
            Finger0Position = GetWorldPositionOfFinger(0);
        }
    }

    void RestrictCameraPosition()
    {
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
                maxX = backgroundBounds.min.x + stageWidth - horzExtent;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 3;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 1:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.max.y - stageHeight + vertExtent - 3;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 2:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 3;
                maxY = backgroundBounds.max.y - vertExtent;
                break;
            case 3:
                minX = backgroundBounds.min.x + horzExtent;
                maxX = backgroundBounds.max.x - horzExtent;
                minY = backgroundBounds.min.y + vertExtent - 3;
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

    Vector2 GetWorldPosition()
    {
        return camera_GameObject.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
    }

    Vector2 GetWorldPositionOfFinger(int FingerIndex)
    {
        return camera_GameObject.GetComponent<Camera>().ScreenToWorldPoint(Input.GetTouch(FingerIndex).position);
    }
}
