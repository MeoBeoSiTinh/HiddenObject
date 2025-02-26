using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetFind : MonoBehaviour

{
    public GameManager gameManager;
    private Camera mainCamera;

    private void Start()
    {
        // Cache the main camera for performance
        mainCamera = Camera.main;

        // Find the GameManager object by name and get the GameManager component
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManager = gameManagerObject.GetComponent<GameManager>();
        }
        else
        {
            Debug.LogError("GameManager object not found in the scene.");
        }
    }

    private void Update()
    {
        // Check if there is a touch input
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
                        gameManager.TargetFound(gameObject.name);
                    }
                }
            }
        }
    }
}
