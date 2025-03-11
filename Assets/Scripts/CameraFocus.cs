using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFocus : MonoBehaviour
{
    public Camera mainCamera; // Assign your 2D camera in the Inspector
    public float activationDistance = 10f;
    public float proximityRadius = 5f;
    private List<GameObject> dynamicObjects = new List<GameObject>(); // Initialize the list to store all relevant 2D objects

    public void Start()
    {

    }

    private void Update()
    {
        // Loop through all dynamic objects and enable/disable them
        foreach (GameObject obj in dynamicObjects)
        {
            if (IsObjectNearCamera(obj) || IsObjectVisibleToCamera(obj))
            {
                EnableObject(obj);
            }
            else
            {
                DisableObject(obj);
            }
        }
    }

    private bool IsObjectNearCamera(GameObject obj)
    {
        // Check if the object is within the proximity radius of the camera
        float distance = Vector2.Distance(mainCamera.transform.position, obj.transform.position);
        return distance <= proximityRadius;
    }

    private bool IsObjectVisibleToCamera(GameObject obj)
    {
        // Check if the object is within the camera's viewport
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(obj.transform.position);

        // Check if the object is within the camera's view frustum
        return viewportPosition.x >= 0 && viewportPosition.x <= 1 &&
               viewportPosition.y >= 0 && viewportPosition.y <= 1 &&
               viewportPosition.z > 0; // Ensure the object is in front of the camera
    }

    private void EnableObject(GameObject obj)
    {
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
    }

    private void DisableObject(GameObject obj)
    {
        if (obj.activeSelf)
        {
            obj.SetActive(false);
        }
    }

    public void ResetMap(int levelIndex)
    {
        GameObject map = GameObject.Find("map" + levelIndex + "(Clone)");
        dynamicObjects.Clear(); // Clear the list before adding new objects
        for (int i = 0; i < map.transform.childCount; i++)
        {
            if (map.transform.GetChild(i).GetComponent<TargetFind>() == null)
                dynamicObjects.Add(map.transform.GetChild(i).gameObject);
        }
        dynamicObjects.RemoveAll(x => x.name == "Background");
    }
}
