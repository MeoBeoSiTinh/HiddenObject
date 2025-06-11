using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Map1 : MonoBehaviour
{
    GameManager gameManager;
    private List<string> foundTarget;
    FocusCircleController focusCircleController;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        focusCircleController = gameManager.FocusCircle.GetComponent<FocusCircleController>();
        StartCoroutine(Tutorial());

    }

    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(3f);
        foundTarget = gameManager.foundTarget;
        if(!foundTarget.Contains("day thung"))
        {
            yield return MoveCamera(new Vector3(-2.5f, -3.2f, -10f), 7f);
            Camera.main.GetComponent<CameraHandle>().allowed = false;
            yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-2.5f, -2.7f), 0.5f);
        }
        while (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield return null; // Wait for the next frame
        }
        yield return new WaitForSeconds(0.5f);
        while (!foundTarget.Contains("day thung"))
        {
            yield return CreateAndFadeInPointer(new Vector2(-2.5f, -2.7f), 0.5f);
            yield return null; // Wait for the next frame
        }
        Destroy(GameObject.FindGameObjectWithTag("Pointer"));
        StartCoroutine(focusCircleController.StopFocusing());
        Camera.main.GetComponent<CameraHandle>().allowed = true;

    }

    private IEnumerator CreateAndFadeInPointer(Vector2 position, float fadeDuration)
    {
        if(GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield break;
        }
        GameObject pointerPrefab = Resources.Load<GameObject>("Pointer/Pointer");
        if (pointerPrefab != null)
        {
            GameObject pointer = Instantiate(pointerPrefab, position, pointerPrefab.transform.rotation);
            SpriteRenderer sr = pointer.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) yield break;

            Color transparent = sr.color;
            transparent.a = 0f;
            Color opaque = sr.color;

            float progress = 0f;
            while (progress < 1f)
            {
                if(sr == null) yield break;
                progress += Time.deltaTime / fadeDuration;
                sr.color = Color.Lerp(transparent, opaque, progress);
                yield return null;
            }
        }
    }

    private IEnumerator CreateAndFadeInPointerDestroyOnClick(Vector2 position, float fadeDuration)
    {
        if (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield break;
        }
        GameObject pointerPrefab = Resources.Load<GameObject>("Pointer/Pointer");
        if (pointerPrefab != null)
        {
            GameObject pointer = Instantiate(pointerPrefab, position, pointerPrefab.transform.rotation);
            pointer.AddComponent<DestroyOnAnyNonUIClick>();
            SpriteRenderer sr = pointer.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) yield break;

            Color transparent = sr.color;
            transparent.a = 0f;
            Color opaque = sr.color;

            float progress = 0f;
            while (progress < 1f)
            {
                if (sr == null) yield break;
                progress += Time.deltaTime / fadeDuration;
                sr.color = Color.Lerp(transparent, opaque, progress);
                yield return null;
            }
        }
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, float targetSize)
    {
        float duration = 0.8f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;
        targetPosition.z = -10f;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            yield return null; // Wait for the next frame
        }


        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = targetSize;

        yield return focusCircleController.StartFocusing(targetPosition, 0.1f);


        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }
}
