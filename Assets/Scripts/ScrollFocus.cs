using UnityEngine;
using UnityEngine.UI;

public class ScrollFocus : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect; // Reference to the ScrollRect
    [SerializeField] private Camera canvasCamera;   // Camera used by the Canvas (Screen Space - Camera)
    [SerializeField] private RectTransform viewport; // The ScrollRect's viewport RectTransform
    [SerializeField] private float scrollDuration = 0.5f; // Duration of the scroll animation in seconds

    // Check if an icon is visible in the viewport
    public bool IsIconVisible(RectTransform iconRect)
    {
        if (!scrollRect || !viewport || !canvasCamera || !iconRect)
        {
            Debug.LogWarning("Missing required components.");
            return false;
        }

        // Get the icon's bounds in world space
        Vector3[] iconCorners = new Vector3[4];
        iconRect.GetWorldCorners(iconCorners);

        // Get the viewport's bounds in world space
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);

        // Convert corners to screen space
        Rect screenViewport = GetScreenRect(viewportCorners);
        Rect screenIcon = GetScreenRect(iconCorners);

        // Check if the icon's rect overlaps with the viewport's rect
        return screenViewport.Overlaps(screenIcon);
    }

    // Scroll to make the icon visible with smooth animation
    public void ScrollToIcon(RectTransform iconRect)
    {
        if (!IsIconVisible(iconRect))
        {
            Vector2 targetNormalizedPos = CalculateNormalizedPosition(iconRect);
            StartCoroutine(SmoothScrollTo(targetNormalizedPos));
        }
    }

    // Coroutine for smooth scrolling
    private System.Collections.IEnumerator SmoothScrollTo(Vector2 targetNormalizedPos)
    {
        Vector2 startPos = scrollRect.normalizedPosition;
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scrollDuration;
            // Optional: Apply easing (e.g., ease-out)
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out effect
            scrollRect.normalizedPosition = Vector2.Lerp(startPos, targetNormalizedPos, t);
            yield return null;
        }

        // Ensure final position is exact
        scrollRect.normalizedPosition = targetNormalizedPos;
    }

    // Calculate the final screen position of the icon after scrolling (without scrolling)
    public Vector2 GetIconPositionAfterScroll(RectTransform iconRect)
    {
        if (!IsIconVisible(iconRect))
        {
            // Simulate scrolling by calculating the normalized position
            Vector2 normalizedPos = CalculateNormalizedPosition(iconRect);

            // Calculate the content's new position in world space
            RectTransform content = scrollRect.content;
            Vector2 contentSize = content.rect.size;
            Vector2 viewportSize = viewport.rect.size;

            // Calculate the content's offset based on normalized position
            float contentX = normalizedPos.x * (contentSize.x - viewportSize.x);
            Vector2 contentAnchoredPos = content.anchoredPosition;
            Vector2 newContentAnchoredPos = new Vector2(-contentX, contentAnchoredPos.y);

            // Calculate the icon's new world position relative to the content's new position
            Vector3 iconWorldPos = iconRect.position;
            Vector3 contentWorldPos = content.position;
            Vector3 relativePos = iconWorldPos - contentWorldPos;
            Vector3 newContentWorldPos = content.position + (Vector3)(newContentAnchoredPos - contentAnchoredPos);
            Vector3 newIconWorldPos = newContentWorldPos + relativePos;

            // Convert to screen space
            return RectTransformUtility.WorldToScreenPoint(canvasCamera, newIconWorldPos);
        }
        else
        {
            // If already visible, return current screen position
            return RectTransformUtility.WorldToScreenPoint(canvasCamera, iconRect.position);
        }
    }

    // Helper: Calculate the normalized scroll position to center the icon in the viewport
    private Vector2 CalculateNormalizedPosition(RectTransform iconRect)
    {
        RectTransform content = scrollRect.content;
        Vector2 contentSize = content.rect.size;
        Vector2 viewportSize = viewport.rect.size;

        // Get the icon's anchored position relative to the content
        Vector2 iconLocalPos = content.InverseTransformPoint(iconRect.position);

        // Calculate the target content offset to center the icon in the viewport
        float targetX = iconLocalPos.x + content.anchoredPosition.x - (viewportSize.x / 2) + (iconRect.rect.width / 2);

        // Clamp the offset to valid bounds
        float maxX = contentSize.x - viewportSize.x;
        targetX = Mathf.Clamp(targetX, 0, maxX);

        // Convert to normalized position (horizontal scrolling)
        float normalizedX = maxX > 0 ? targetX / maxX : 0;

        // Return normalized position (y remains unchanged for horizontal scrolling)
        return new Vector2(normalizedX, scrollRect.normalizedPosition.y);
    }

    // Helper: Convert world corners to a screen-space Rect
    private Rect GetScreenRect(Vector3[] corners)
    {
        Vector2 min = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[0]);
        Vector2 max = min;

        for (int i = 1; i < 4; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[i]);
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}