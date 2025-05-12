using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LayoutGroupMultiRemover : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    [SerializeField] private bool destroyImmediately = false;

    public void RemoveMultipleAndSlide(List<RectTransform> elementsToRemove)
    {
        elementsToRemove.RemoveAll(item => item == null);

        if (elementsToRemove.Count == 0 || layoutGroup == null)
        {
            return;
        }

        StartCoroutine(RemoveMultipleRoutine(elementsToRemove));
    }

    private IEnumerator RemoveMultipleRoutine(List<RectTransform> elementsToRemove)
    {
        // 1. Disable layout systems
        ContentSizeFitter sizeFitter = layoutGroup.GetComponent<ContentSizeFitter>();
        bool wasFitterEnabled = sizeFitter != null && sizeFitter.enabled;
        if (sizeFitter != null) sizeFitter.enabled = false;

        layoutGroup.enabled = false;
        Canvas.ForceUpdateCanvases();

        // 2. Store remaining children and their CURRENT positions
        List<RectTransform> remainingChildren = new List<RectTransform>();
        List<RectTransform> AllChildren = new List<RectTransform>();
        List<Vector2> originalPositions = new List<Vector2>();

        foreach (RectTransform child in layoutGroup.transform)
        {
            AllChildren.Add(child);
            if (!elementsToRemove.Contains(child))
            {
                remainingChildren.Add(child);
                originalPositions.Add(child.anchoredPosition);
            }
            else if (!destroyImmediately)
            {
                child.gameObject.SetActive(false);
            }
        }

        // 3. Get TARGET positions (critical steps)
        // First force immediate rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());

        // Wait for THREE frames to ensure layout is fully updated
        yield return null;
        yield return null;
        yield return null;

        // Now capture target positions
        Vector2[] targetPositions = new Vector2[remainingChildren.Count];
        for (int i = 0; i < remainingChildren.Count; i++)
        {
            if (remainingChildren[i] != null)
            {

                targetPositions[i] = AllChildren[i].anchoredPosition;

                // DEBUG: Verify positions are different
                if (originalPositions[i] == targetPositions[i])
                {
                    Debug.LogWarning($"Element {remainingChildren[i].name} has identical positions! " +
                                   $"Original: {originalPositions[i]}, Target: {targetPositions[i]}");
                }

                // Reset to original position for animation
                remainingChildren[i].anchoredPosition = originalPositions[i];
            }
        }

        // 4. Animate if positions are actually different
        bool positionsDiffer = false;
        for (int i = 0; i < remainingChildren.Count; i++)
        {
            if (remainingChildren[i] != null && originalPositions[i] != targetPositions[i])
            {
                positionsDiffer = true;
                LeanTween.move(remainingChildren[i], targetPositions[i], slideDuration)
                    .setEase(easeType);
            }
        }

        // 5. If no movement needed, skip animation
        if (!positionsDiffer)
        {
            Debug.Log("No position change detected - removing immediately");
            foreach (var element in elementsToRemove)
            {
                if (element != null) Destroy(element.gameObject);
            }
            yield break;
        }

        // 6. Wait for animation
        yield return new WaitForSeconds(slideDuration);

        // 7. Final cleanup
        foreach (var element in elementsToRemove)
        {
            if (element != null) Destroy(element.gameObject);
        }

        // 8. Restore layout systems
        if (wasFitterEnabled && sizeFitter != null)
        {
            sizeFitter.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        layoutGroup.enabled = true;
    }
}