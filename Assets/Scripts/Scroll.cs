using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour
{
    // Speed of rotation
    public float rotationSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rt = GetComponent<RectTransform>();
        LeanTween.rotate(rt, new Vector3(0, 0, 360), 1f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setLoopClamp()
                .setRepeat(-1); // Set the loop to infinite
        Debug.Log("Rotating");
    }

}
