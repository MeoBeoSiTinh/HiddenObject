using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTouch : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject targetObject;
    public float transitionSpeed = 1.0f;
    private Vector3 targetPosition;
    private bool shouldMove = false;
    private bool hasMoved = false; // Add this flag to ensure the script runs only once

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = targetObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasMoved) return; // Exit if the object has already moved

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check if the touch phase is a tap (began and ended without moving)
            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
            {
                // Create a ray from the camera to the touch position
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit hit;

                // Check if the ray hits an object
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if the hit object is the one this script is attached to
                    if (hit.transform == transform)
                    {
                        Debug.Log("Target found: " + gameObject.name);
                        // Set the target position and start moving
                        targetPosition -= new Vector3(0, 1, 0);
                        shouldMove = true;
                    }
                }
            }
        }

        // Move the target object towards the target position
        if (shouldMove)
        {
            targetObject.transform.position = Vector3.MoveTowards(targetObject.transform.position, targetPosition, transitionSpeed * Time.deltaTime);
            if (targetObject.transform.position == targetPosition)
            {
                shouldMove = false;
                hasMoved = true; // Set the flag to true after the object has moved
            }
        }
    }
}
