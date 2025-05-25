using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectFocus : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float scrollDuration = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    [SerializeField][Range(0, 0.5f)] private float visibilityMargin = 0.1f; // 10% margin

    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
        content = scrollRect.content;
    }

    /// <summary>
    /// Checks if the target element is currently visible in the scroll view
    /// </summary>
    public bool IsElementVisible(RectTransform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return false;

        Vector3[] targetCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);
        viewport.GetWorldCorners(viewportCorners);

        if (scrollRect.vertical)
        {
            float viewportMinY = viewportCorners[0].y + (viewportCorners[1].y - viewportCorners[0].y) * visibilityMargin;
            float viewportMaxY = viewportCorners[1].y - (viewportCorners[1].y - viewportCorners[0].y) * visibilityMargin;
            return targetCorners[0].y < viewportMaxY && targetCorners[1].y > viewportMinY;
        }
        else // Horizontal
        {
            float viewportMinX = viewportCorners[0].x + (viewportCorners[3].x - viewportCorners[0].x) * visibilityMargin;
            float viewportMaxX = viewportCorners[3].x - (viewportCorners[3].x - viewportCorners[0].x) * visibilityMargin;
            return targetCorners[0].x < viewportMaxX && targetCorners[3].x > viewportMinX;
        }
    }

    /// <summary>
    /// Smoothly scrolls to make the target element visible
    /// </summary>
    public void ScrollToView(RectTransform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy) return;

        Canvas.ForceUpdateCanvases();   
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Vector2 targetPos = CalculateNormalizedPosition(target);

        if (scrollRect.vertical)
        {
            LeanTween.value(gameObject, scrollRect.verticalNormalizedPosition,
                targetPos.y, scrollDuration)
                .setEase(easeType)
                .setOnUpdate((float val) => {
                    scrollRect.verticalNormalizedPosition = val;
                });
        }

        if (scrollRect.horizontal)
        {
            LeanTween.value(gameObject, scrollRect.horizontalNormalizedPosition,
                targetPos.x, scrollDuration)
                .setEase(easeType)
                .setOnUpdate((float val) => {
                    scrollRect.horizontalNormalizedPosition = val;
                });
        }
    }

    public Vector2 CalculateNormalizedPosition(RectTransform target)
    {
        if (target == null) return scrollRect.normalizedPosition;

        // Force immediate layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Get all relevant dimensions
        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentSize = content.rect.size;
        Vector2 targetSize = target.rect.size;

        // Convert target position to content's local space
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);
        Vector2 targetMin = content.InverseTransformPoint(targetCorners[0]); // Bottom-left corner
        Vector2 targetMax = content.InverseTransformPoint(targetCorners[2]); // Top-right corner

        Vector2 normalizedPos = scrollRect.normalizedPosition;

        if (scrollRect.horizontal)
        {
            float contentWidth = contentSize.x;
            float viewportWidth = viewportSize.x;
            float targetWidth = targetMax.x - targetMin.x;

            // Special case: element at very beginning (left edge)
            if (targetMin.x <= 0 && targetMax.x <= viewportWidth)
            {
                normalizedPos.x = 0f; // Snap to start
            }
            // Calculate required position to make target visible
            else if (targetMax.x > viewportWidth)
            {
                // Need to scroll right
                normalizedPos.x = Mathf.Clamp01((targetMax.x - viewportWidth + targetWidth / 2) / (contentWidth - viewportWidth));
            }
            else if (targetMin.x < 0)
            {
                // Need to scroll left
                normalizedPos.x = Mathf.Clamp01(targetMin.x / (contentWidth - viewportWidth));
            }
        }

        if (scrollRect.vertical)
        {
            float contentHeight = contentSize.y;
            float viewportHeight = viewportSize.y;
            float targetHeight = targetMax.y - targetMin.y;

            // Calculate required position to make target visible
            if (targetMax.y > viewportHeight)
            {
                // Need to scroll up
                normalizedPos.y = Mathf.Clamp01(1 - ((targetMax.y - viewportHeight + targetHeight / 2) / (contentHeight - viewportHeight)));
            }
            else if (targetMin.y < 0)
            {
                // Need to scroll down
                normalizedPos.y = Mathf.Clamp01(1 - (targetMin.y / (contentHeight - viewportHeight)));
            }
        }

        Debug.Log($"Calculated normalized position: {normalizedPos}");
        return normalizedPos;
    }

    /// <summary>
    /// Calculates the final anchoredPosition of a target after scrolling to make it visible
    /// </summary>
    public Vector2 CalculateFinalIconPosition(RectTransform target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return target.anchoredPosition;

        // Force all layout calculations
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Get current positions and sizes
        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentSize = content.rect.size;
        Vector2 targetSize = target.rect.size;
        Vector2 contentPivot = content.pivot;

        // Calculate the target's position in viewport space
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);
        Vector2 targetViewportMin = viewport.InverseTransformPoint(targetCorners[0]); // Bottom-left
        Vector2 targetViewportMax = viewport.InverseTransformPoint(targetCorners[2]); // Top-right

        // Calculate required movement to make fully visible
        Vector2 requiredMovement = Vector2.zero;

        // Horizontal check
        if (scrollRect.horizontal)
        {
            if (targetViewportMax.x > viewportSize.x) // Off right edge
                requiredMovement.x = targetViewportMax.x - viewportSize.x;
            else if (targetViewportMin.x < 0) // Off left edge
                requiredMovement.x = targetViewportMin.x;
        }

        // Vertical check
        if (scrollRect.vertical)
        {
            if (targetViewportMin.y < 0) // Off bottom edge
                requiredMovement.y = targetViewportMin.y;
            else if (targetViewportMax.y > viewportSize.y) // Off top edge
                requiredMovement.y = targetViewportMax.y - viewportSize.y;
        }

        // Convert viewport movement to content movement
        Vector2 contentMovement = new Vector2(
            requiredMovement.x * (contentSize.x / viewportSize.x),
            requiredMovement.y * (contentSize.y / viewportSize.y)
        );

        // Calculate final anchored position
        Vector2 finalPosition = target.anchoredPosition;
        finalPosition -= contentMovement; // Movement is inverse for anchoredPosition

        return finalPosition;
    }
    private void OnScrollComplete()
    {
        // Optional: Add any completion logic here
    }

    /// <summary>
    /// Immediately stops any ongoing scroll animation
    /// </summary>
    public void StopCurrentScroll()
    {
        LeanTween.cancel(gameObject);
    }
}