using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boxman : MonoBehaviour
{
    [Header("Movement Area")]
    public Vector2 areaCenter = Vector2.zero;
    public Vector2 areaSize = new Vector2(10f, 10f);
    public float moveSpeed = 0.6f;
    public float waitTimeMin = 1f;
    public float waitTimeMax = 3f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool unbox = false;
    public SkeletonAnimation anim;
    private Coroutine moveCoroutine;

    [SerializeField] private AudioClip carton;
    [SerializeField] private AudioClip claim;


    private void Start()
    {
        anim = GetComponentInChildren<SkeletonAnimation>();
        moveCoroutine = StartCoroutine(RandomMoveRoutine());
    }

    private IEnumerator RandomMoveRoutine()
    {
        while (!unbox)
        {
            // Pick a random position within the area
            float x = Random.Range(areaCenter.x - areaSize.x / 2, areaCenter.x + areaSize.x / 2);
            float y = Random.Range(areaCenter.y - areaSize.y / 2, areaCenter.y + areaSize.y / 2);
            targetPosition = new Vector3(x, y, transform.position.z);
            isMoving = true;
            anim.AnimationState.SetAnimation(0, "move", true);

            // Move towards the target position
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f && !unbox)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            isMoving = false;
            anim.AnimationState.SetAnimation(0, "idle", true);
            // Wait for a random time before moving again
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            float timer = 0f;
            while (timer < waitTime && !unbox)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void interact()
    {
        string name = anim.AnimationName;
        switch (name)
        {
            case "move":
            case "idle":
                unbox = true;
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                    moveCoroutine = null;
                }
                isMoving = false;
                anim.AnimationState.SetAnimation(0, "idle", true);
                StartCoroutine(unboxing());
                break;
            case "click":
            case "idle2":
                if(claim != null)
                {
                    SoundFXManager.Instance.PlaySoundFXClip(claim, transform, 1f);
                }   
                anim.AnimationState.SetAnimation(0, "idle3", true);
                break;
        }
    }

    private IEnumerator unboxing()
    {
        var animPlay = anim.AnimationState.SetAnimation(0, "click", false);
        if (carton != null)
        {
            SoundFXManager.Instance.PlaySoundFXClip(carton, transform, 2f);

        }
        yield return new WaitForSpineAnimationComplete(animPlay);
        anim.AnimationState.SetAnimation(0, "idle2", true);
        yield return null;
    }
}
