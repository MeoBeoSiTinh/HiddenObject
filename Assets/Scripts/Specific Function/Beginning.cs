using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Beginning : MonoBehaviour
{
    // Start is called before the first frame update
    public IEnumerator Begin(int level)
    {
        switch (level)
        {
            case 0:

                Camera.main.transform.position = new Vector3(-6, 11, -10);
                yield return new WaitForSeconds(2f);
                GameObject animHolder = GameObject.Find("main");
                SkeletonAnimation anim = animHolder.transform.GetChild(0).GetComponent<SkeletonAnimation>();
                Vector3 pos = anim.transform.position;
                pos.z = -10;
                yield return MoveCamera(pos, 5f);
                pos.z = 0;
                pos.y += 1.5f;
                string text = $"<sprite name=\"CA\"> ";
                GameObject textBox = Resources.Load<GameObject>("Box");
                GameObject spawnedTextBox = Instantiate(textBox, pos, Quaternion.identity);
                spawnedTextBox.transform.localScale = textBox.transform.localScale * 0.1f;
                LeanTween.scale(spawnedTextBox, textBox.transform.localScale, 0.4f).setEase(LeanTweenType.easeOutBack);
                spawnedTextBox.tag = "TextBox"; // Ensure the prefab or instance has this tag
                TextMeshProUGUI textMeshPro = spawnedTextBox.GetComponentInChildren<TextMeshProUGUI>();
                textMeshPro.text = text;



                var hungry = anim.AnimationState.SetAnimation(0, "wrong", false);
                yield return new WaitForSpineAnimationComplete(hungry);
                anim.AnimationState.SetAnimation(0, "idle", true);
                pos = anim.transform.position;
                pos.z = -10;
                StartCoroutine(MoveCamera(pos, 10f));
                StartCoroutine(FadeAndDestroyTextBox(spawnedTextBox, 1.5f));

                break;

            default:
                Debug.Log("Unknown level");
                break;
        }
        yield return null;
    }


    private IEnumerator MoveCamera(Vector3 targetPosition, float targetSize)
    {
        float duration = 3f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = targetSize;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }

    private IEnumerator FadeAndDestroyTextBox(GameObject textBoxObj, float duration)
    {
        CanvasGroup canvasGroup = textBoxObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textBoxObj.AddComponent<CanvasGroup>();
        }
        yield return new WaitForSeconds(duration);
        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }
        Destroy(textBoxObj);
    }
}
