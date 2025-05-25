using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;
using Spine.Unity;
using Event = UnityEngine.Event;

public class SpineSkinController : MonoBehaviour
{
   public SkeletonAnimation skeletonAnimation;
   [SpineSkin]
   public string[] skinNames;

   private Skin _combinedSkin;
   
   // protected Skeleton skeleton;
   
   private void Start()
   {
       CombineSkins(skinNames);
   }

   public void CombineSkins(params string[] skins)
   {
       _combinedSkin = new Skin("_strCombinedSkinName");
       Skeleton skeletonRenderer = skeletonAnimation.skeleton;
       for (int i = 0; i < skins.Length; i++)
       {
           string skinName = skins[i];
           if (skeletonRenderer.Data.FindSkin(skinName) != null)
           {
               _combinedSkin.AddSkin(skeletonRenderer.Data.FindSkin(skinName));
           }
           skeletonRenderer.SetSkin(_combinedSkin);
           skeletonRenderer.SetSlotsToSetupPose();
       }   
   }
}
