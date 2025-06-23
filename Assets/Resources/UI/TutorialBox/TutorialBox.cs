using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialBox : MonoBehaviour
{
    public void ChangeText(string text)
    {
        TextMeshProUGUI TextUI = GetComponentInChildren<TextMeshProUGUI>();
        TextUI.text = text;
    }

    public IEnumerator popUp(Vector2 normalizedPosition)
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 referenceResolution = canvas.GetComponent<CanvasScaler>().referenceResolution;

        // Set fixed anchors (center of canvas)
        rect.anchorMin = new Vector2(0.5f, 0.5f); // Center
        rect.anchorMax = new Vector2(0.5f, 0.5f); // Center
        rect.pivot = new Vector2(0.5f, 0.5f);    // Center pivot

        // Calculate anchoredPosition based on normalized coordinates
        Vector2 anchoredPosition = new Vector2(
            normalizedPosition.x * referenceResolution.x / 2f,
            normalizedPosition.y * referenceResolution.y / 2f
        );

        // Preserve prefab's sizeDelta (do not set it)
        rect.anchoredPosition = anchoredPosition;
        rect.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        yield return null;
        LeanTween.scale(rect, new Vector3(1f, 1f, 1f), 1f).setEaseOutBack();
        yield return null;
    }

    public IEnumerator popDown()
    {
        LeanTween.scale(gameObject, new Vector3(0.1f, 0.1f, 0.1f), 0.4f)
            .setEase(LeanTweenType.easeOutQuint)
            .setOnComplete(() => Destroy(gameObject));
        yield return null;
    }

    public void AddDestroyOnClick()
    {
        gameObject.AddComponent<DestroyOnOutsideClick>();
    }
}