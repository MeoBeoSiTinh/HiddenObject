using UnityEngine;

public class UIRotate : MonoBehaviour
{
    void Awake()
    {
        LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 360f, 18f) 
            .setEase(LeanTweenType.linear)
            .setLoopCount(-1); // Infinite loops
    }
}