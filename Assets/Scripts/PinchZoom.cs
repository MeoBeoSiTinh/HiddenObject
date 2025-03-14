using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
public class PinchZoom : MonoBehaviour
{
    private float prevMagnitude = 0;
    private float touchCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        var touch0contact = new InputAction
        (
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch0/press"
        );
        touch0contact.Enable();
        var touch1contact = new InputAction
        (
            type: InputActionType.Button,
            binding: "<Touchscreen>/touch1/press"
        );
        touch1contact.Enable();

        touch0contact.performed += _ =>
        {
            touchCount++;
        };
        touch1contact.performed += _ =>
        {
            touchCount++;
        };
        touch0contact.canceled += _ =>
        {
            touchCount--;
            prevMagnitude = 0;
        };
        touch1contact.canceled += _ =>
        {
            touchCount--;
            prevMagnitude = 0;
        };

        var touch0Pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch0/position"
        );
        touch0Pos.Enable();
        var touch1Pos = new InputAction
        (
            type: InputActionType.Value,
            binding: "<Touchscreen>/touch1/position"
        );
        touch1Pos.Enable();
        touch1Pos.performed += _ =>
        {
            if (touchCount != 2)
            {
                return;
            }
            var magnitude = (touch0Pos.ReadValue<Vector2>() - touch1Pos.ReadValue<Vector2>()).magnitude;
            if(prevMagnitude == 0)
            {
                prevMagnitude = magnitude;
            }
            var difference = magnitude - prevMagnitude;
            prevMagnitude = magnitude;
            cameraZoom(difference * 0.01f);
        };
    }

    private void cameraZoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + increment, 5, 12);
    }

}
