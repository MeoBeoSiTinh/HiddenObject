using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TargetFind : MonoBehaviour
{
    private GameManager gameManager;
    private Camera mainCamera;
    public Transform UiHotbar; // Reference to the UI hotbar
    private GameObject targetImagePrefab; // Prefab for the target image

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
            if (touch.phase == TouchPhase.Began)
            {
                // Create a ray from the camera to the touch position
                Ray ray = mainCamera.ScreenPointToRay(touch.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                // Sort hits by distance
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (RaycastHit hit in hits)
                {
                    // Check if the hit object is the one this script is attached to
                    if (hit.transform == transform)
                    {
                        // Create the target image at the touch position
                        CreateTargetImage(touch.position);
                        break;
                    }
                    else
                    {
                        // If there is any object in front of the target, do nothing
                        break;
                    }
                }
            }
        }
    }

    private void CreateTargetImage(Vector2 touchPosition)
    {
        // Instantiate the target image prefab
        targetImagePrefab = new GameObject("TargetImage");
        Image targetImage = targetImagePrefab.AddComponent<Image>();
        targetImagePrefab.transform.SetParent(GameObject.Find("Canvas").transform);

        // Set the size and sprite of the target image
        targetImage.rectTransform.sizeDelta = new Vector2(1, 1); // Adjust size as needed
        targetImage.sprite = gameObject.GetComponent<SpriteRenderer>().sprite;

        // Convert touch position to local position in the Canvas
        RectTransform canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            touchPosition,
            mainCamera,
            out localPoint
        );

        // Set the initial position of the target image to the touch position
        targetImage.rectTransform.anchoredPosition = localPoint;

        // Find the UI hotbar by name and assign it to UiHotbar
        Debug.Log("Icon" + gameObject.name);
        GameObject hotbarObject = GameObject.Find("Icon" + gameObject.name);

        if (hotbarObject != null)
        {
            UiHotbar = hotbarObject.transform;
        }
        else
        {
            Debug.LogError("Hotbar object not found in the scene.");
        }

        Debug.Log("Target found: " + gameObject.name);
        gameManager.TargetFound(gameObject.name);

        // Start the flying animation
        StartCoroutine(FlyToToolbar(targetImagePrefab));

        // Disable the target object in the scene
        gameObject.SetActive(false);
    }

    private IEnumerator FlyToToolbar(GameObject flyingImage)
    {
        float duration = 1f; // Duration of the animation
        float elapsedTime = 0f;
        RectTransform flyingImageRect = flyingImage.GetComponent<RectTransform>();
        Vector2 startPosition = flyingImageRect.anchoredPosition;

        // Get the end position (UiHotbar's anchored position)
        RectTransform hotbarRect = UiHotbar.GetComponent<RectTransform>();
        Vector2 endPosition = hotbarRect.anchoredPosition;

        Debug.Log("Start Position: " + startPosition);
        Debug.Log("End Position: " + endPosition);

        while (elapsedTime < duration)
        {
            // Smoothly interpolate the position of the flying image
            flyingImageRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the flying image after reaching the toolbar
        Destroy(flyingImage);

        // Optionally, update the toolbar (e.g., increment a counter or add the item to a list)
        Debug.Log("Item added to toolbar!");
    }
}