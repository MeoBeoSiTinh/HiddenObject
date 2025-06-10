using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pointer : MonoBehaviour
    
{
    private void OnEnable()
    {
        StartCoroutine(pointer());
    }

    private void OnDisable()
    {
        StopCoroutine(pointer());
    }


    private IEnumerator pointer()
    {
        float minScale = 0.4f;
        float maxScale = 0.5f;
        float speed = 2f;
        bool scalingUp = true;

        while (true)
        {
            Vector3 scale = transform.localScale;
            if (scalingUp)
            {
                scale.x += speed * Time.deltaTime * (maxScale - minScale);
                scale.y += speed * Time.deltaTime * (maxScale - minScale);
                scale.z += speed * Time.deltaTime * (maxScale - minScale);
                if (scale.x >= maxScale)
                {
                    scale = new Vector3(maxScale, maxScale, maxScale);
                    scalingUp = false;
                }
            }
            else
            {
                scale.x -= speed * Time.deltaTime * (maxScale - minScale);
                scale.y -= speed * Time.deltaTime * (maxScale - minScale);
                scale.z -= speed * Time.deltaTime * (maxScale - minScale);
                if (scale.x <= minScale)
                {
                    scale = new Vector3(minScale, minScale, minScale);
                    scalingUp = true;
                }
            }
            transform.localScale = scale;
            yield return null;
        }
    }


}
