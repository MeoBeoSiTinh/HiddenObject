using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraFocus : MonoBehaviour
{
    public Camera mainCamera; // Assign your 2D camera in the Inspector
    public float activationDistance = 1f;
    public float proximityRadius = 1f;
    private List<GameObject> dynamicObjects = new List<GameObject>(); // Initialize the list to store all relevant 2D objects
                                                                      // Add this field to the class
    private readonly HashSet<string> excludedRendererNames = new HashSet<string> { "Background", "may", "main" };

    public void Start()
    {

    }

    private void Update()
    {
        // Scale activationDistance and proximityRadius with camera size
        float scaledActivationDistance = activationDistance * mainCamera.orthographicSize;
        float scaledProximityRadius = proximityRadius * mainCamera.orthographicSize;

        // Loop through all dynamic objects and enable/disable them
        foreach (GameObject obj in dynamicObjects)
        {
            if (obj == null) // Check if the object got destroyed
            {
                continue;
            }

            if (IsObjectNearCamera(obj, scaledProximityRadius) || IsObjectVisibleToCamera(obj))
            {
                EnableObject(obj);
            }
            else
            {
                DisableObject(obj);
            }
        }
    }

    private bool IsObjectNearCamera(GameObject obj, float scaledProximityRadius)
    {
        // Check if the object is within the scaled proximity radius of the camera
        float distance = Vector2.Distance(mainCamera.transform.position, obj.transform.position);
        return distance <= scaledProximityRadius;
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

        if (map != null)
        {
            // Recursively find all SpriteRenderers in the map
            FindAllSpriteRenderers(map.transform);
        }
        else
        {
            Debug.LogWarning("Map not found: map" + levelIndex + "(Clone)");
        }
    }

    // Recursively find all GameObjects with SpriteRenderer in children
    private void FindAllSpriteRenderers(Transform parent)
    {
        // Check SpriteRenderer (only if enabled)
        SpriteRenderer spriteRenderer = parent.GetComponent<SpriteRenderer>();
        bool hasActiveSpriteRenderer = (spriteRenderer != null && spriteRenderer.enabled);

        // Check MeshRenderer (only if enabled)
        MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();
        bool hasActiveMeshRenderer = (meshRenderer != null && meshRenderer.enabled);

        // Replace the relevant line in FindAllSpriteRenderers:
        if ((hasActiveSpriteRenderer || hasActiveMeshRenderer) && !excludedRendererNames.Contains(parent.name))
        dynamicObjects.Add(parent.gameObject);
        

        // Recursively check children
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            FindAllSpriteRenderers(child);
        }
    }
}
