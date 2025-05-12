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

        if (!IsElementVisible(target))
        {
            float targetNormalizedPosition = CalculateNormalizedPosition(target);
            AnimateScroll(targetNormalizedPosition);
        }
    }

    private float CalculateNormalizedPosition(RectTransform target)
    {
        // Convert positions to content's local space
        Vector2 elementPos = content.InverseTransformPoint(target.position);
        Vector2 contentPos = content.InverseTransformPoint(content.position);
        Vector2 relativePos = contentPos - elementPos;

        if (scrollRect.vertical)
        {
            float elementHeight = target.rect.height;
            float contentHeight = content.rect.height;
            float viewportHeight = viewport.rect.height;

            // Adjust for element size and center it
            float adjustedPos = relativePos.y - (elementHeight / 2f);
            return 1 - Mathf.Clamp01(adjustedPos / (contentHeight - viewportHeight));
        }
        else // Horizontal
        {
            float elementWidth = target.rect.width;
            float contentWidth = content.rect.width;
            float viewportWidth = viewport.rect.width;

            // Adjust for element size and center it
            float adjustedPos = relativePos.x - (elementWidth / 2f);
            return Mathf.Clamp01(adjustedPos / (contentWidth - viewportWidth));
        }
    }

    private void AnimateScroll(float targetNormalizedPosition)
    {
        if (scrollRect.vertical)
        {
            LeanTween.value(gameObject, scrollRect.verticalNormalizedPosition,
                targetNormalizedPosition, scrollDuration)
                .setEase(easeType)
                .setOnUpdate(UpdateVerticalScroll)
                .setOnComplete(OnScrollComplete);
        }
        else // Horizontal
        {
            LeanTween.value(gameObject, scrollRect.horizontalNormalizedPosition,
                targetNormalizedPosition, scrollDuration)
                .setEase(easeType)
                .setOnUpdate(UpdateHorizontalScroll)
                .setOnComplete(OnScrollComplete);
        }
    }

    private void UpdateVerticalScroll(float value)
    {
        scrollRect.verticalNormalizedPosition = value;
    }

    private void UpdateHorizontalScroll(float value)
    {
        scrollRect.horizontalNormalizedPosition = value;
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