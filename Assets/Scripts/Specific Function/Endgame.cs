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
                
                SkeletonAnimation anim = GameObject.Find("main").GetComponent<SkeletonAnimation>();
                Vector3 pos = anim.transform.position;
                pos.z = -10;
                yield return MoveCamera(pos);
                AudioClip main = Resources.Load<AudioClip>("Map1 Test/Sound/main");
                SoundFXManager.Instance.PlaySoundFXClip(main, anim.transform, 1f);
                var craft =  anim.AnimationState.SetAnimation(0, "craft", false);
                yield return new WaitForSpineAnimationComplete(craft);
                anim.AnimationState.SetAnimation(0, "idle2", true);
                break;
            default:
                Debug.Log("Unknown level");
                break;
        }

        yield return null;
    }


    private IEnumerator MoveCamera(Vector3 targetPosition)
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
            Camera.main.orthographicSize = Mathf.Lerp(startSize, 5f, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = 5f;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }
}
