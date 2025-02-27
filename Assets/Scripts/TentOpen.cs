using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentOpen : MonoBehaviour
{
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

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
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
