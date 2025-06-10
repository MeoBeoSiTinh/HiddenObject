using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FocusCircleController : MonoBehaviour
{
    public Material circleMaterial;
    private Camera mainCamera;
    private float radius;


    void Start()
    {
        mainCamera = Camera.main;
        if (circleMaterial == null)
        {
            circleMaterial = GetComponent<Image>().material;
        }
        // Set aspect ratio dynamically
        float aspectRatio = (float)Screen.width / Screen.height;
        circleMaterial.SetFloat("_AspectRatio", aspectRatio);
    }

    //void Update()
    //{
    //    // Convert world position to UI position
    //    Vector2 viewportPos = mainCamera.WorldToViewportPoint(targetPosition);
    //    circleMaterial.SetVector("_CircleCenter", viewportPos);
    //    circleMaterial.SetFloat("_Radius", radius);
    //    circleMaterial.SetFloat("_FadeWidth", fadeWidth);
    //}

    public IEnumerator StartFocusing(Vector2 pos, float radius)
    {
        mainCamera = Camera.main;
        Vector2 targetPosition = mainCamera.WorldToViewportPoint(pos);
        circleMaterial.SetVector("_CircleCenter", targetPosition);
        circleMaterial.SetFloat("_Radius", 1);
        float progress = 0f;
        float duration = 1f;
        yield return null;
        gameObject.SetActive(true);
        while (progress < duration)
        {
            progress += Time.deltaTime / duration;
            float newRadius = Mathf.Lerp(1, radius, progress);
            circleMaterial.SetFloat("_Radius", newRadius);
            yield return null;
        }
        this.radius = radius;
    }

    public IEnumerator StopFocusing()
    {
        float progress = 0f;
        float duration = 0.8f;
        yield return null;
        while (progress < duration)
        {
            progress += Time.deltaTime / duration;
            float newRadius = Mathf.Lerp(this.radius, 1, progress);
            circleMaterial.SetFloat("_Radius", newRadius);
            yield return null;
        }
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}