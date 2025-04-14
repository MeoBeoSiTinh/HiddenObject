using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swaySpeed = 1f;
    public float maxSwayAngle = 4f;

    private Vector3 basePosition;  // The fixed base position
    private float treeHeight;     // Distance from base to object center
    private Quaternion startRot;  // Initial rotation

    void Start()
    {
        // Store initial state
        startRot = transform.rotation;
        basePosition = GetBasePosition();
        treeHeight = transform.position.y - basePosition.y;
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
