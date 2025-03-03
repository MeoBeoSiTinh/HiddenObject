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
        

        // Start the flying animation
        StartCoroutine(FlyToToolbar(targetImagePrefab));
        gameManager.TargetFound(gameObject.name);

        // Disable the target object in the scene
        gameObject.SetActive(false);

    }

    private IEnumerator FlyToToolbar(GameObject flyingImage)
    {
        float duration = 20f; // Duration of the animation
        float elapsedTime = 0f;
        RectTransform flyingImageRect = flyingImage.GetComponent<RectTransform>();
        Vector2 startPosition = flyingImageRect.anchoredPosition;

        // Disable the GridLayoutGroup to stop it from interfering
        GridLayoutGroup grid = UiHotbar.parent.GetComponent<GridLayoutGroup>();
        grid.enabled = false;

        // Get the end position (UiHotbar's anchored position)
        RectTransform hotbarRect = UiHotbar.GetComponent<RectTransform>();

        // Step 1: Get the current world position of the hotbarRect
        Vector3 hotbarWorldPosition = hotbarRect.TransformPoint(hotbarRect.rect.center);

        // Step 2: Change the anchor preset of the hotbarRect
        hotbarRect.anchorMin = new Vector2(0.5f, 0.5f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0.5f);
        hotbarRect.pivot = new Vector2(0.5f, 0.5f);

        // Step 3: Force Unity to update the layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(hotbarRect);

        // Step 4: Convert the world position back to the new anchored position
        RectTransform parentRect = hotbarRect.parent as RectTransform;
        Vector2 endPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, hotbarWorldPosition),
            null,
            out endPosition
        );

        // Set endPosition's y to UiHotbar's parent's y position
        endPosition.y = parentRect.anchoredPosition.y;

        grid.enabled = true;

        // Log the new anchored position
        Debug.Log("Start Position: " + startPosition);
        Debug.Log("End Position: " + endPosition);

        // Add a jumping effect
        float jumpHeight = 100f; // Height of the jump
        float jumpSpeed = 5f; // Speed of the jump

        while (elapsedTime < duration)
        {
            // Calculate the progress (0 to 1)
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // Smoothly move the flying image towards the end position
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, progress);

            // Add a jumping effect using Mathf.Sin
            float jump = Mathf.Sin(progress * Mathf.PI * jumpSpeed) * jumpHeight * (1 - progress);
            newPosition.y += jump;

            // Update the flying image's position
            flyingImageRect.anchoredPosition = newPosition;

            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is set exactly to the end position
        flyingImageRect.anchoredPosition = endPosition;

        // Destroy the flying image after reaching the toolbar
        try
        {
            Destroy(flyingImage);
            Debug.Log("Flying image destroyed successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error destroying flying image: " + e.Message);
        }

        // Optionally, update the toolbar (e.g., increment a counter or add the item to a list)
        Debug.Log("Item added to toolbar!");
    }
}