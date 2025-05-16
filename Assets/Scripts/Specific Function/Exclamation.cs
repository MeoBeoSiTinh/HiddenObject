using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exclamation : MonoBehaviour
{
    private Vector3 startPosition;
    public float amplitude = 0.2f; // How far up and down
    public float frequency = 2f;   // How fast

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
}
