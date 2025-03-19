using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Description : MonoBehaviour, IPointerDownHandler
{
    public GameObject uiPrefab; // Assign your UI prefab in the Inspector
    private Canvas canvas;
    public string description;
    public float fadeDuration = 0.5f; // Duration of the fade effect

    private void Start()
    {
        // Automatically find the Canvas in the parent hierarchy
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Convert screen touch position to Canvas local position
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            canvas.worldCamera,
            out localPosition
        );

        // Instantiate the prefab and set its position
        GameObject newUIElement = Instantiate(uiPrefab, canvas.transform);
        RectTransform rectTransform = newUIElement.GetComponent<RectTransform>();
        TextMeshProUGUI text = newUIElement.GetComponentInChildren<TextMeshProUGUI>();
        text.text = description;

        if (rectTransform != null)
        {
            // Adjust the position to be above the touched UI object
            localPosition.y += rectTransform.rect.height;
            localPosition.x += rectTransform.rect.width / 4;
            rectTransform.anchoredPosition = localPosition;
        }
        else
        {
            Debug.LogError("The prefab does not have a RectTransform component.");
        }

        // Start the fade out animation using LeanTween after a delay of 2 seconds
        CanvasGroup canvasGroup = newUIElement.AddComponent<CanvasGroup>();
        LeanTween.alphaCanvas(canvasGroup, 0, fadeDuration).setDelay(1.5f).setOnComplete(() => Destroy(newUIElement));
    }
}
