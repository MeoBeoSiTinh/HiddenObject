using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class WrongTarget : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject targetImagePrefab; // Prefab for the target image

    // Start is called before the first frame update
    void Start()
    {
        // Cache the main camera for performance
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if there is a touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Check if the touch is over a UI element
            if (IsPointerOverUIObject(touch))
            {
                return;
            }

            // Check if the touch phase is a tap (began and ended without moving)
            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1 && touch.deltaPosition.magnitude < 10f)
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
                        // Create the target image at the touch position
                        CreateTargetImage(touch.position);
                        Debug.Log("Wrong target ");
                    }
                }
            }
        }
    }

    private bool IsPointerOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void CreateTargetImage(Vector2 touchPosition)
    {
        // Instantiate the target image prefab  
        GameObject targetImageInstance = Instantiate(targetImagePrefab);

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
}
