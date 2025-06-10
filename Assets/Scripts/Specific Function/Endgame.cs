using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endgame : MonoBehaviour
{
    public IEnumerator End(int level)
    {
        switch(level)
        {
            case 0:
                
                SkeletonAnimation anim1 = GameObject.Find("main").GetComponentInChildren<SkeletonAnimation>();
                Vector3 pos1 = anim1.transform.position;
                yield return MoveCamera(pos1, 8f);
                var craft1 =  anim1.AnimationState.SetAnimation(0, "wrong", false);
                StartCoroutine(CreateAndFadeInFirePit(new Vector2(0.5f, -0.5f), 0.5f));
                yield return new WaitForSpineAnimationComplete(craft1);
                anim1.AnimationState.SetAnimation(0, "idle", true);
                break;
            case 1:

                //SkeletonAnimation anim2 = GameObject.Find("main").GetComponent<SkeletonAnimation>();
                //Vector3 pos2 = anim2.transform.position;
                //AudioClip sound2 = Resources.Load<AudioClip>("Map1 Test/Sound/main");
                //yield return MoveCamera(pos2, 6f);
                //SoundFXManager.Instance.PlaySoundFXClip(sound2, transform, 0.5f);
                //var craft2 = anim2.AnimationState.SetAnimation(0, "craft", false);
                //yield return new WaitForSpineAnimationComplete(craft2);
                //anim2.AnimationState.SetAnimation(0, "idle2", true);
                break;
            default:
                Debug.Log("Unknown level");
                break;
        }

        yield return null;
    }


    private IEnumerator CreateAndFadeInFirePit(Vector2 position, float fadeDuration)
    {
        GameObject prefab = Resources.Load<GameObject>("Map1 Test/fire pit");
        if (prefab != null)
        {
            GameObject firepit = Instantiate(prefab, position, prefab.transform.rotation);
            SpriteRenderer sr = firepit.GetComponentInChildren<SpriteRenderer>();
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
}
