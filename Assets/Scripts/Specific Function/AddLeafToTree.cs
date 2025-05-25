using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

public class AddLeafToTree : MonoBehaviour
{
    [SerializeField] private GameObject Leaf;
    [SerializeField] private int colorIndex = 0;

    public void addLeaf()
    {
        if (Leaf != null && colorIndex != 0)
        {
            GameObject leafInstance = Instantiate(Leaf);
            leafInstance.transform.SetParent(transform, false);
            leafInstance.transform.localPosition = Vector3.zero;

            // Get the SkeletonAnimation component
            SkeletonAnimation skeletonAnimation = leafInstance.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                string skinName = "leaf" + colorIndex;
                Skin skin = skeletonAnimation.Skeleton.Data.FindSkin(skinName);


                if (skin != null)
                {
                    skeletonAnimation.Skeleton.SetSkin(skin);
                    skeletonAnimation.Skeleton.SetSlotsToSetupPose();
                    skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
                }
                else
                {
                    Debug.LogError($"Skin '{skinName}' not found!");
                }
            }
            StartCoroutine(Tree(skeletonAnimation, leafInstance));
        }
    }
    public IEnumerator Tree(SkeletonAnimation anim, GameObject leaf)
    {
        var animplay = anim.AnimationState.SetAnimation(0, "leaf", false);
        Debug.Log("played");
        yield return new WaitForSpineAnimationComplete(animplay);
        Destroy(leaf);
        yield return null;
    }
}