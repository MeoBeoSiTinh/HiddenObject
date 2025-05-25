using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Sway Settings")]

    public float maxSwayAngle = 4f;

    private Vector3 basePosition;  // The fixed base position
    private float treeHeight;     // Distance from base to object center
    private Quaternion startRot;  // Initial rotation
    private float swaySpeed;      // Random sway speed

    void Start()
    {
        // Store initial position
        basePosition = GetBasePosition();
        treeHeight = transform.position.y - basePosition.y;

        // Store initial rotation PROPERLY
        startRot = transform.rotation;

        // Add random initial tilt (in Euler angles, not Quaternion)
        float randomZRotation = Random.Range(-1, 1);
        startRot = Quaternion.Euler(0, 0, randomZRotation);

        // Random sway speed
        swaySpeed = Random.Range(1f, 2f);
    }

    void Update()
    {
        // Calculate sway angle (-maxAngle to +maxAngle)
        float sway = Mathf.Sin(Time.time * swaySpeed) * maxSwayAngle;

        // Reset to original position/rotation
        transform.rotation = startRot;
        transform.position = basePosition + Vector3.up * treeHeight;

        // Apply rotation around base
        transform.RotateAround(basePosition, Vector3.forward, sway);
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

        // Default to assuming pivot is at base
        return transform.position;
    }
}
