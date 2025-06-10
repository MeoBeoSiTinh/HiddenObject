using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exclamation : MonoBehaviour
{
    private Vector3 startPosition;
    public float amplitude = 0.2f; // How far up and down
    public float frequency = 0.5f;   // How fast


    private IEnumerator Jiggling()
    {
        while (true)
        {
            float elapsed = 0f;
            while (true)
            {
                elapsed += Time.deltaTime;
                float yOffset = Mathf.Sin(elapsed * frequency * Mathf.PI * 2) * amplitude;
                transform.position = startPosition + new Vector3(0, yOffset, 0);
                yield return null;
            }
        }
    }

    void OnEnable()
    {
        startPosition = transform.position;
        StartCoroutine(Jiggling());
    }

    void OnDisable()
    {
        StopCoroutine(Jiggling());
    }
}
