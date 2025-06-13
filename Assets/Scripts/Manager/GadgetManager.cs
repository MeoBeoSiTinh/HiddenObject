using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GadgetManager : MonoBehaviour
{
    Camera cam;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [Header("Compass Settings")]
    public GameObject pointerPrefab; // UI Image prefab
    public Canvas canvas; // Reference to your UI Canvas
    private GameObject pointerInstance;
    private Coroutine compassCoroutine;
    private List<GameObject> currentTargets;
    public void Start()
    {
        cam = Camera.main;
    }

    public void Scan(List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No targets provided for scanning.");
            return;
        }

        GameObject nearestTarget = null;
        float minDistance = float.MaxValue;
        Vector3 cameraPosition = cam.transform.position;

        foreach (var target in targets)
        {
            float distance = Vector3.Distance(cameraPosition, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = target;
            }
        }

        if (nearestTarget != null)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-3.0f, 3.0f),
                Random.Range(-3.0f, 3.0f),
                0f
            );

            Vector3 targetPosition = new Vector3(
                nearestTarget.transform.position.x + randomOffset.x,
                nearestTarget.transform.position.y + randomOffset.y,
                Camera.main.transform.position.z
            );

            StartCoroutine(MoveCamera(targetPosition));
        }
    }

    private IEnumerator MoveCamera(Vector3 targetPosition)
    {
        CameraManager cameraManager = cam.GetComponent<CameraManager>();
        while (Vector3.Distance(Camera.main.transform.position, targetPosition) > 0.01f)
        {
            Camera.main.transform.position = Vector3.SmoothDamp(
                Camera.main.transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
            cameraManager.RestrictCameraPosition();
            if(cameraManager.IsCameraTouchingBorder())
            {
                break;
            }
            yield return null;
        }
    }

    public void Compass(List<GameObject> targets)
    {
        if (compassCoroutine != null)
        {
            return;
        }

        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No targets provided for compass.");
            return;
        }

        currentTargets = targets;

        // Create UI pointer
        if (pointerPrefab != null && canvas != null)
        {
            pointerInstance = Instantiate(pointerPrefab, canvas.transform);
            pointerInstance.transform.localPosition = Vector3.zero;
        }

        // Start 10-second compass duration
        compassCoroutine = StartCoroutine(UpdateCompass(10f));
    }

    private IEnumerator UpdateCompass(float duration)
    {
        RectTransform pointerRect = pointerInstance?.GetComponent<RectTransform>();
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (pointerRect == null) yield break;

            GameObject nearestTarget = FindNearestTarget();
            if (nearestTarget != null)
            {
                // Calculate direction
                Vector3 screenPos = cam.WorldToScreenPoint(nearestTarget.transform.position);
                bool isBehind = screenPos.z < 0;

                if (isBehind) screenPos *= -1;

                Vector3 screenDirection = (screenPos - new Vector3(Screen.width / 2, Screen.height / 2)).normalized;
                float angle = Mathf.Atan2(screenDirection.y, screenDirection.x) * Mathf.Rad2Deg;

                // Apply rotation
                pointerRect.rotation = Quaternion.Euler(0, 0, angle);

                // Optional: Update alpha based on target position
                //pointerRect.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1f, 0f, timer / duration);
            }

            yield return null;
        }

        // Auto-cleanup after duration
        StopCompass();
    }

    private GameObject FindNearestTarget()
    {
        if (currentTargets == null || currentTargets.Count == 0)
            return null;

        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 camPos = cam.transform.position;

        foreach (GameObject target in currentTargets)
        {
            if (target == null) continue;
            float distance = Vector3.Distance(target.transform.position, camPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = target;
            }
        }
        return nearest;
    }

    private void StopCompass()
    {
        if (compassCoroutine != null)
        {
            StopCoroutine(compassCoroutine);
            compassCoroutine = null;
        }
        if (pointerInstance != null)
        {
            Destroy(pointerInstance);
            pointerInstance = null;
        }
    }
}
