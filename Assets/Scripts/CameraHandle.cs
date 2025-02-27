using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0 && isZooming)
        {
            isZooming = false;
        }

        if (Input.touchCount == 1)
        {
            if (!isZooming)
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

                // Restrict the zoom size to be unable to be larger than the background
                Camera cam = camera_GameObject.GetComponent<Camera>();
                Bounds backgroundBounds = background_GameObject.GetComponent<SpriteRenderer>().bounds;
                float maxZoomOut = Mathf.Min(backgroundBounds.size.x * Screen.height / Screen.width, backgroundBounds.size.y) / 2;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 0.1f, maxZoomOut);

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

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = backgroundBounds.min.x + horzExtent;
        float maxX = backgroundBounds.max.x - horzExtent;
        float minY = backgroundBounds.min.y + vertExtent - 5;
        float maxY = backgroundBounds.max.y - vertExtent;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        cam.transform.position = pos;

        // Slide back slowly if the camera reaches the end of the background
        if (pos.x == minX || pos.x == maxX)
        {
            cam.transform.Translate(new Vector3(pos.x == minX ? 2 : -2, 0, 0) * Time.deltaTime);
        }
        if (pos.y == minY || pos.y == maxY)
        {
            cam.transform.Translate(new Vector3(0, pos.y == minY ? 2 : -2, 0) * Time.deltaTime);
        }
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
