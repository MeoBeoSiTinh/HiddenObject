using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;

public class LayoutGroupMultiRemover : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;
    [SerializeField] private bool destroyImmediately = false;

    [Header("Animation Settings")]
    [SerializeField] private SkeletonGraphic smokeAnimationPrefab;
    [SerializeField] private float smokeAnimationDuration = 0.5f;
    [SerializeField] private Vector2 smokeOffset = new Vector2(0, 0);

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

        // 3. Play smoke animations on elements being removed
        List<Coroutine> smokeAnimations = new List<Coroutine>();
        foreach (RectTransform child in layoutGroup.transform)
        {
            AllChildren.Add(child);
            if (!elementsToRemove.Contains(child))
            {
                remainingChildren.Add(child);
                originalPositions.Add(child.anchoredPosition);
            }
            else
            {
                if (!destroyImmediately)
                {
                    // Play smoke animation before deactivating
                    if (smokeAnimationPrefab != null)
                    {
                        var smoke = Instantiate(smokeAnimationPrefab, child.parent);
                        smoke.rectTransform.anchoredPosition = child.anchoredPosition + smokeOffset;
                        smokeAnimations.Add(StartCoroutine(PlayAndDestroySmoke(smoke, smokeAnimationDuration)));
                    }
                    child.gameObject.SetActive(false);
                }
            }
        }

        // Wait for smoke animations to complete
        foreach (var anim in smokeAnimations)
        {
            yield return anim;
        }

        // 4. Get TARGET positions
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
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

                remainingChildren[i].anchoredPosition = originalPositions[i];
            }
        }

        // 5. Animate if positions are different
        for (int i = 0; i < remainingChildren.Count; i++)
        {
            if (remainingChildren[i] != null && originalPositions[i] != targetPositions[i])
            {
                LeanTween.move(remainingChildren[i], targetPositions[i], slideDuration)
                    .setEase(easeType);
            }
        }

        // 6. Final cleanup
        yield return new WaitForSeconds(slideDuration);

        foreach (var element in elementsToRemove)
        {
            if (element != null) Destroy(element.gameObject);
        }

        // 7. Restore layout systems
        if (wasFitterEnabled && sizeFitter != null)
        {
            sizeFitter.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        layoutGroup.enabled = true;
    }

    private IEnumerator PlayAndDestroySmoke(SkeletonGraphic smoke, float duration)
    {
        smoke.gameObject.SetActive(true);
        smoke.AnimationState.SetAnimation(0, "smoke", false);

        yield return new WaitForSeconds(duration);

        Destroy(smoke.gameObject);
    }
}