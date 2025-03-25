using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class TargetFind : MonoBehaviour
{
    private GameManager gameManager;
    private Camera mainCamera;
    public Transform UiHotbar; // Reference to the UI hotbar
    private GameObject targetImagePrefab; // Prefab for the target image
    public GameObject spineAnimationPrefab; // Prefab for the Spine animation
    private float touchStartTime;
    public float touchThresholdTime = 0.2f; // Threshold time for touch
    private GameObject SpecialUI;
    private void Start()
    {
        // Cache the main camera for performance
        mainCamera = Camera.main;
        Input.simulateMouseWithTouches = false;
        // Find the GameManager object by name and get the GameManager component
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManager = gameManagerObject.GetComponent<GameManager>();
            SpecialUI = gameManager.specialFoundUi;
        }
        else
        {
            Debug.LogError("GameManager object not found in the scene.");
        }
    }

    private void FixedUpdate()
    {
        // Check if there is a touch input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartTime = Time.time;
            }

            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1 && touch.deltaPosition.magnitude < 1f && (Time.time - touchStartTime) <= touchThresholdTime)
            {
                Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
                RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosition, Vector2.zero);

                if (hits.Length > 0)
                {
                    // Sort hits by sorting layer and sorting order
                    System.Array.Sort(hits, (a, b) =>
                    {
                        SpriteRenderer rendererA = a.collider.GetComponent<SpriteRenderer>();
                        SpriteRenderer rendererB = b.collider.GetComponent<SpriteRenderer>();

                        if (rendererA != null && rendererB != null)
                        {
                            // Compare sorting layers
                            int layerComparison = SortingLayer.GetLayerValueFromID(rendererA.sortingLayerID)
                                .CompareTo(SortingLayer.GetLayerValueFromID(rendererB.sortingLayerID));

                            if (layerComparison != 0)
                                return layerComparison;

                            // Compare sorting orders within the same layer
                            return rendererB.sortingOrder.CompareTo(rendererA.sortingOrder);
                        }

                        // If no SpriteRenderer, fall back to distance
                        return a.distance.CompareTo(b.distance);
                    });

                    // Get the topmost hit
                    RaycastHit2D topmostHit = hits[0];

                    // Check if the hit object is the one this script is attached to
                    if (topmostHit.transform == transform)
                    {
                        
                        if(gameObject.tag == "Special")
                        {
                            CreateSpineAnimation(touch.position);
                            SpecialTargetFound(touch.position);
                        }
                        else
                        {
                            // Create the Spine animation at the touch position
                            CreateSpineAnimation(touch.position);
                            // Create the target image at the touch position
                            CreateTargetImage(touch.position);
                        }
                        
                    }
                }
            }
        }
    }

    public void CreateTargetImage(Vector2 touchPosition)
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
        GameObject hotbarObject = GameObject.Find("Icon" + gameObject.name);

        if (hotbarObject != null)
        {
            UiHotbar = hotbarObject.transform;
        }
        else
        {
            Debug.LogError("Hotbar object not found in the scene.");
        }

        // Start the flying animation
        StartCoroutine(FlyToToolbar(targetImagePrefab));

    }

    public void CreateSpineAnimation(Vector2 touchPosition)
    {
        // Instantiate the Spine animation prefab
        GameObject spineAnimation = Instantiate(spineAnimationPrefab, touchPosition, Quaternion.identity);
        spineAnimation.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Ensure the Spine animation has a RectTransform component
        RectTransform spineRectTransform = spineAnimation.GetComponent<RectTransform>();
        if (spineRectTransform == null)
        {
            spineRectTransform = spineAnimation.AddComponent<RectTransform>();
        }

        // Set the initial position of the Spine animation to the touch position
        RectTransform canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            touchPosition,
            mainCamera,
            out localPoint
        );

        spineRectTransform.anchoredPosition = localPoint;

        // Adjust size and scale
        spineRectTransform.sizeDelta = new Vector2(100, 100); // Adjust size as needed
        spineRectTransform.localScale = new Vector3(80, 80, 80); // Adjust scale as needed

        // Destroy the Spine animation GameObject after a delay
        Destroy(spineAnimation, 1f); // Adjust the delay as needed
    }

    private IEnumerator FlyToToolbar(GameObject flyingImage)
    {
        if (gameManager.isHotBarMinimized)
        {
            gameManager.OnMinimizeClicked();
        }
        gameObject.GetComponent<SpriteRenderer>().enabled = false;

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
        endPosition.y = uiLocaction.anchoredPosition.y;
        endPosition.x += parentRect.anchoredPosition.x;

        // Define control points for an upward-then-downward parabola
        Vector2 midPoint = (startPosition + endPosition) * 0.5f;
        float parabolaHeight = 400f; // Adjust this to control the "height" of the parabola

        Vector2 controlPoint = new Vector2(
            midPoint.x,
            Mathf.Max(startPosition.y, endPosition.y) + parabolaHeight
        );

        // Move the image along the parabolic path with slow start and end
        LeanTween.value(flyingImage, 0f, 1f, 1f) // Increased duration to 1.5s for smoother effect
            .setEase(LeanTweenType.easeInOutQuad) // Starts slow, speeds up, then slows down
            .setOnUpdate((float t) =>
            {
                flyingImageRect.anchoredPosition = CalculateQuadraticBezierPoint(t, startPosition, controlPoint, endPosition);

            })
            .setOnComplete(() =>
            {
                try
                {
                    Destroy(flyingImage);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error destroying flying image: " + e.Message);
                }
                gameObject.SetActive(false);

                gameManager.TargetFound(gameObject);
            });

        yield return null;
    }

    // Helper method to calculate a point on a quadratic Bezier curve
    private Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;

        return point;
    }
    
    public void SpecialTargetFound(Vector2 touchPosition)
    {
        // Instantiate the target image prefab
        targetImagePrefab = new GameObject("TargetImage");
        Image targetImage = targetImagePrefab.AddComponent<Image>();
        targetImagePrefab.transform.SetParent(GameObject.Find("Canvas").transform);

        // Add Canvas component and override sorting layer
        Canvas canvas = targetImagePrefab.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 5002;

        // Set the size and sprite of the target image
        Sprite sourceSprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        targetImage.sprite = sourceSprite;
        targetImage.rectTransform.localScale = new Vector3(1f, 1f, 1f); // Adjust scale as needed

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

        // Create a separate GameObject for the shining effect
        GameObject shiningEffect = new GameObject("ShiningEffect");
        Image shiningImage = shiningEffect.AddComponent<Image>();
        shiningEffect.transform.SetParent(targetImagePrefab.transform); // Make shiningEffect a child of targetImage

        // Add Canvas component and override sorting layer for shining effect
        Canvas shiningCanvas = shiningEffect.AddComponent<Canvas>();
        shiningCanvas.overrideSorting = true;
        shiningCanvas.sortingOrder = 5001;

        // Set the source image for the shining effect
        shiningImage.sprite = Resources.Load<Sprite>("shining/light");
        shiningImage.rectTransform.sizeDelta = new Vector2(131, 131); // Adjust size as needed
        shiningImage.rectTransform.localScale = new Vector3(1f, 1f, 1f); // Adjust scale as needed
        shiningImage.rectTransform.anchoredPosition = Vector2.zero; // Center the shining effect in the parent

        // Find the UI hotbar by name and assign it to UiHotbar
        GameObject hotbarObject = GameObject.Find("Icon" + gameObject.name);

        if (hotbarObject != null)
        {
            UiHotbar = hotbarObject.transform;
        }
        else
        {
            Debug.LogError("Hotbar object not found in the scene.");
        }
        // Start the flying animation
        StartCoroutine(SpecialEffect(targetImagePrefab, shiningEffect));

    }

    private IEnumerator SpecialEffect(GameObject specialImage, GameObject shiningEffect)
    {
        // Get RectTransforms
        RectTransform specialImageRect = specialImage.GetComponent<RectTransform>();
        RectTransform shiningRect = shiningEffect.GetComponent<RectTransform>();

        // Store initial position
        Vector2 startPosition = specialImageRect.anchoredPosition;
        Vector2 endPosition = new Vector2(0, 100);

        // Move the image to the center of the screen
        LeanTween.value(specialImage, 0f, 1f, 1f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
                specialImageRect.anchoredPosition = newPosition;
            });

        // Scale the parent image
        LeanTween.scale(specialImageRect, new Vector3(6f, 6f, 6f), 1f)
            .setEase(LeanTweenType.easeInOutQuad);

        // Rotate the shining effect
        LeanTween.rotateAroundLocal(shiningEffect, Vector3.forward, 360f, 8f) // Slowed down rotation speed
            .setEase(LeanTweenType.easeInOutQuad)
            .setLoopCount(-1); // Infinite loops

        // Scale the child
        LeanTween.scale(shiningRect, new Vector3(1.5f, 1.5f, 1.5f), 1f)
            .setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });

        // Call the game manager function
        gameManager.specialFound(gameObject, specialImage);
        

        yield return null;
    }
}
