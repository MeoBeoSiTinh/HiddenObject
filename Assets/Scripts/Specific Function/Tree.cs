using System.Collections;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Sway Settings")]
    private float maxSwayAngle = 4f;
    [Tooltip("Sway speed in degrees per second")]
    private float minSwaySpeed = 1f;
    private float maxSwaySpeed = 3f;

    private Vector3 basePosition;
    private float treeHeight;
    private Quaternion startRot;
    private float swaySpeed;
    private Coroutine swayCoroutine;

    void Start()
    {
        InitializeTree();
        swayCoroutine = StartCoroutine(SwayRoutine());
    }

    private void OnEnable()
    {
        InitializeTree();
        swayCoroutine = StartCoroutine(SwayRoutine());
    }

    void OnDisable()
    {
        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
        }
    }

    void InitializeTree()
    {
        basePosition = GetBasePosition();
        treeHeight = transform.position.y - basePosition.y;
        startRot = transform.rotation;

        // Add random initial tilt
        float randomZRotation = Random.Range(-1f, 1f);
        startRot = Quaternion.Euler(0, 0, randomZRotation);

        // Random sway speed (in degrees per second)
        swaySpeed = Random.Range(minSwaySpeed, maxSwaySpeed);
    }

    IEnumerator SwayRoutine()
    {
        float swayTimer = 0f;
        float swayDuration = 1f; // Duration for one full sway cycle

        while (true)
        {
            // Calculate progress through sway cycle (0 to 1)
            float progress = Mathf.PingPong(swayTimer * swaySpeed, maxSwayAngle * 2) / (maxSwayAngle * 2);

            // Calculate sway angle using smooth Sin wave
            float swayAngle = Mathf.Sin(progress * Mathf.PI * 2) * maxSwayAngle;

            // Apply rotation around base
            transform.rotation = startRot;
            transform.position = basePosition + Vector3.up * treeHeight;
            transform.RotateAround(basePosition, Vector3.forward, swayAngle);

            // Increment timer and wait
            swayTimer += Time.deltaTime;
            yield return null;
        }
    }

    Vector3 GetBasePosition()
    {
        // For SpriteRenderer (2D)
        if (TryGetComponent<SpriteRenderer>(out var sprite))
        {
            return transform.position - Vector3.up * sprite.bounds.extents.y;
        }
        // For MeshRenderer (3D)
        else if (TryGetComponent<MeshRenderer>(out var mesh))
        {
            return transform.position - Vector3.up * mesh.bounds.extents.y;
        }
        return transform.position;
    }
}