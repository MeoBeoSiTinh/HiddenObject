using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Crafter : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject CrafterMenu;
    private Camera cam;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CrafterMenu = gameManager.Dialogue;
        cam = Camera.main;
    }
    public void ShowRecipe()
    {
        Vector3 targetPosition = gameObject.transform.position;
        targetPosition.z = cam.transform.position.z;
        targetPosition.x = targetPosition.x + 0.5f;
        StartCoroutine(MoveCamera(targetPosition));
    }



    private IEnumerator MoveCamera(Vector3 targetPosition)
    {
        float duration = 1f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, 4f, t);
            yield return null; // Wait for the next frame
        }
        gameManager.OpenCrafter();
        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = 4f;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }

}
