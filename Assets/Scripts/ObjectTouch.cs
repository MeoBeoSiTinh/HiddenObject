using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectTouch : MonoBehaviour
{
    private GameManager gameManager;
    private Camera mainCamera;
    public Transform UiHotbar; // Reference to the UI hotbar
    private GameObject targetImagePrefab; // Prefab for the target image
    public GameObject wrongImage; // Prefab for the wrong image
    public GameObject spineAnimationPrefab; // Prefab for the Spine animation
    private float touchStartTime;
    private readonly float touchThresholdTime = 0.5f; // Threshold time for touch
    private bool isSpreading = false; // Flag to check if the object is spreading

    [SerializeField] public AudioClip soundFx;
    [SerializeField] private bool removeAfterFound = true;

    private void Awake()
    {
        // Cache the main camera for performance
        mainCamera = Camera.main;
        Input.simulateMouseWithTouches = false;
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
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (IsPointerOverUIObject(touch))
            {
                return;
            }

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
                    Debug.Log(topmostHit.collider.gameObject.tag);
                    // Check if the hit object is the one this script is attached to
                    if (topmostHit.transform == transform)
                    {
                        switch (gameObject.tag)
                        {
                            case "Normal":
                                TryGetComponent<Boxman>(out Boxman boxman);
                                if (boxman != null)
                                {
                                    if (boxman.unbox && boxman.anim.AnimationName != "idle3")
                                    {
                                        CreateSpineAnimation(touch.position);
                                        CreateTargetImage(touch.position);
                                    }
                                    boxman.interact();
                                }
                                else
                                {
                                    CreateSpineAnimation(touch.position);
                                    CreateTargetImage(touch.position);
                                }
                                if(soundFx != null)
                                {
                                    SoundFXManager.Instance.PlaySoundFXClip(soundFx, transform, 1f);
                                }
                                break;
                            //case "Background":
                            //    CreateWrongImage(touch.position);
                            //    break;
                            case "Openable":
                                StartCoroutine(FadeOpen(0.35f));
                                break;
                            case "Jiggle":
                                JiggleObject();
                                SoundFXManager.Instance.PlaySoundFXClip(soundFx, transform, 1f);

                                break;
                            case "DoubleBush":
                                DoubleBush();
                                SoundFXManager.Instance.PlaySoundFXClip(soundFx, transform, 1f);

                                break;
                            case "X-Shake":
                                XShake();
                                SoundFXManager.Instance.PlaySoundFXClip(soundFx, transform, 1f);

                                break;
                            case "Interact":
                                gameObject.GetComponent<ObjectInteract>().Interact();

                                break;
                            case "Tree":
                                gameObject.GetComponent<AddLeafToTree>().addLeaf();
                                SoundFXManager.Instance.PlaySoundFXClip(soundFx, transform, 1f);

                                break;
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
        targetImage.sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        targetImage.rectTransform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Adjust scale as needed
        targetImage.rectTransform.sizeDelta = new Vector2(targetImage.sprite.rect.width, targetImage.sprite.rect.height); // Adjust size as needed


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
            hotbarObject.transform.hasChanged = false;
            UiHotbar = hotbarObject.transform;
        }
        else
        {
            Debug.Log("Icon" + gameObject.name);
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
        ScrollRectFocus scroll = GameObject.Find("Scroll").GetComponent<ScrollRectFocus>();
        GameObject icon = GameObject.Find("Icon" + name);
        bool isVisible = scroll.IsElementVisible(icon.GetComponent<RectTransform>());
        if (!isVisible)
        {
            scroll.ScrollToView(icon.GetComponent<RectTransform>());
            yield return new WaitForSeconds(0.5f); // Wait for the scroll to finish

        }




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
        endPosition.x += parentRect.anchoredPosition.x;
        //if (!isVisible)
        //{
        //    endPosition = scroll.CalculateFinalIconPosition(icon.GetComponent<RectTransform>());
        //    endPosition.x *= 2;
        //}
        endPosition.y = uiLocaction.anchoredPosition.y;



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
                if (removeAfterFound)
                {
                    gameObject.SetActive(false);
                }
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
    



    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void CreateWrongImage(Vector2 touchPosition)
    {
        // Instantiate the target image prefab  
        GameObject targetImageInstance = Instantiate(wrongImage);

        // Get the Image component from the instantiated prefab  
        Image targetImage = targetImageInstance.GetComponent<Image>();

        // Set the parent of the instantiated prefab to the Canvas  
        targetImageInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Set the size and sprite of the target image  
        targetImage.rectTransform.sizeDelta = new Vector2(100, 100); // Adjust size as needed  

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

        // Animate the target image to pop up and then fade out  
        targetImage.color = new Color(targetImage.color.r, targetImage.color.g, targetImage.color.b, 0);
        LeanTween.scale(targetImage.rectTransform, Vector3.one * 1.5f, 0.5f).setEaseOutBack();
        LeanTween.alpha(targetImage.rectTransform, 1, 0.5f).setEaseOutBack().setOnComplete(() =>
        {
            LeanTween.alpha(targetImage.rectTransform, 0, 0.3f).setDelay(0.5f).setOnComplete(() =>
            {
                Destroy(targetImageInstance);
            });
        });
    }
    private IEnumerator FadeOpen(float duration)
    {
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        gameObject.SetActive(false);
    }
    // Add this method to the ObjectTouch class
    public void JiggleObject()
    {
        // Ensure the object has a transform
        Transform objTransform = transform;

        // Stop any previous jiggling
        LeanTween.cancel(gameObject);

        // Store the original rotation
        Quaternion originalRotation = objTransform.rotation;

        // Jiggle parameters
        float jiggleAngle = 20f; // degrees to rotate up/down
        float duration = 0.07f; // time for each half-jiggle
        int jiggleCount = 2; // number of up-down cycles

        // Sequence: rotate to +angle, then -angle, then back to 0, repeat
        LTSeq seq = LeanTween.sequence();
        for (int i = 0; i < jiggleCount; i++)
        {
            seq.append(LeanTween.rotateZ(gameObject, jiggleAngle, duration).setEase(LeanTweenType.easeInOutSine));
            seq.append(LeanTween.rotateZ(gameObject, -jiggleAngle, duration * 2).setEase(LeanTweenType.easeInOutSine));
            seq.append(LeanTween.rotateZ(gameObject, 0f, duration).setEase(LeanTweenType.easeInOutSine));
        }
        // Ensure final rotation is exactly original
        seq.append(() => { objTransform.rotation = originalRotation; });
    }


    public void DoubleBush()
    {
        Transform parent = gameObject.transform.parent;
        GameObject left = parent.Find("Left").gameObject;
        GameObject right = parent.Find("Right").gameObject;

        // Cancel any previous tweens
        LeanTween.cancel(left);
        LeanTween.cancel(right);

        if (!isSpreading)
        {
            // Set the initial positions
            // Store original positions (in case you want to reset later)
            Vector3 leftOriginalPos = left.transform.localPosition;
            Vector3 rightOriginalPos = right.transform.localPosition;

            // Calculate target positions (spread out along X axis)
            float spreadDistance = 0.38f; // Adjust as needed
            Vector3 leftTargetPos = leftOriginalPos + Vector3.left * spreadDistance;
            Vector3 rightTargetPos = rightOriginalPos + Vector3.right * spreadDistance;

            // Animate with LeanTween
            float duration = 0.4f;
            LeanTween.moveLocal(left, leftTargetPos, duration).setEase(LeanTweenType.easeOutBack);
            LeanTween.moveLocal(right, rightTargetPos, duration).setEase(LeanTweenType.easeOutBack);
            LeanTween.scaleY(left,0.75f, duration/2).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
            {
                LeanTween.scaleY(left, 1f, duration / 2).setEase(LeanTweenType.easeOutBack);
            });
            LeanTween.scaleY(right, 0.75f, duration / 2).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
            {
                LeanTween.scaleY(right, 1f, duration / 2).setEase(LeanTweenType.easeOutBack);
            });
            left.GetComponent<ObjectTouch>().isSpreading = true;
            right.GetComponent<ObjectTouch>().isSpreading = true;
        }
        else
        {
            float duration = 0.4f;
            LeanTween.moveLocal(left, new Vector2(-0.3f, 0f), duration).setEase(LeanTweenType.easeOutBack);
            LeanTween.moveLocal(right, new Vector2(0.3f,0f), duration).setEase(LeanTweenType.easeOutBack);
            left.GetComponent<ObjectTouch>().isSpreading = false;
            right.GetComponent<ObjectTouch>().isSpreading = false;
        }
        
    }

    public void XShake()
    {
        // Ensure the object has a transform
        Transform objTransform = transform;
        // Stop any previous jiggling
        LeanTween.cancel(gameObject);
        // Store the original position
        Vector3 originalPosition = objTransform.localPosition;
        // Shake parameters
        float shakeDistance = 0.1f; // distance to shake
        float duration = 0.3f; // time for the shake
        // Shake left and right
        LeanTween.moveLocalX(gameObject, originalPosition.x + shakeDistance, duration / 2)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong(1);
    }

    

    
    

    
}
