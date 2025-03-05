using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TargetFind : MonoBehaviour
{
    private GameManager gameManager;
    private Camera mainCamera;
    public Transform UiHotbar; // Reference to the UI hotbar
    private GameObject targetImagePrefab; // Prefab for the target image
    private GameObject targetNamePopup; // Popup for the target name

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
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                // If the touch is moving or stationary, do nothing (user is dragging or pinching)
                return;
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
    }



    private IEnumerator FlyToToolbar(GameObject flyingImage)
    {
        RectTransform flyingImageRect = flyingImage.GetComponent<RectTransform>();
        Vector2 startPosition = flyingImageRect.anchoredPosition;

        // Get the end position (UiHotbar's anchored position)
        RectTransform hotbarRect = UiHotbar.GetComponent<RectTransform>();

        // Step 1: Get the current world position of the hotbarRect
        Vector3 hotbarWorldPosition = hotbarRect.TransformPoint(hotbarRect.rect.center);

        // Step 4: Convert the world position back to the new anchored position
        RectTransform parentRect = hotbarRect.parent as RectTransform;
        Vector2 endPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, hotbarWorldPosition),
            null,
            out endPosition
        );
        RectTransform uiLocaction = GameObject.Find("UILocation").GetComponent<RectTransform>();
        // Set endPosition's y to UiHotbar's parent's y position
        endPosition.y = uiLocaction.anchoredPosition.y;

        // Log the new anchored position
        Debug.Log("Start Position: " + startPosition);
        Debug.Log("End Position: " + endPosition);

        // Use LeanTween.sequence to create a sequence of animations
        LeanTween.sequence()
            .append(() =>
            {
                // First, make the image jump up and down
                float jumpHeight = 100f; // Height of the jump
                float jumpDuration = 0.3f; // Duration of the jump

                LeanTween.moveY(flyingImageRect, startPosition.y + jumpHeight, jumpDuration / 2)
                    .setEase(LeanTweenType.easeOutQuad) // Jump up
                    .setOnComplete(() =>
                    {
                        LeanTween.moveY(flyingImageRect, startPosition.y, jumpDuration / 2)
                            .setEase(LeanTweenType.easeInQuad); // Jump down
                    });
            })
            .append(0.3f) // Wait for 1 second after the jump
            .append(() =>
            {
                // Then, move the image to the end position
                LeanTween.move(flyingImageRect, endPosition, 0.8f) // Duration of 1 second
                    .setEase(LeanTweenType.easeOutQuad) // Smooth easing
                    .setOnComplete(() =>
                    {
                        // Optional: Perform any action after the animation completes
                        Debug.Log("Flying image reached the toolbar!");

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

                        // Destroy the target name popup
                        Destroy(targetNamePopup);

                        // Call TargetFound method after the animation completes
                        gameManager.TargetFound(gameObject.name);

                        // Disable the target object in the scene
                        gameObject.SetActive(false);
                    });
            });

        yield return null;
    }
}
