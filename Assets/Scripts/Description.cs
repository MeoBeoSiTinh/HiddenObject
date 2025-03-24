using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Description : MonoBehaviour, IPointerDownHandler
{
    public GameObject DescBox; // Assign your UI prefab in the Inspector
    private Canvas canvas;
    public string description;
    public Boolean isUnlocked = false;

    private void Start()
    {
        // Automatically find the Canvas in the parent hierarchy
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DescBox.transform.GetChild(0).GetChild(0).GetComponent<UnlockHint>().targetObject = gameObject;

        // Convert screen touch position to Canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // Set the x position of DescBox to the local point's x position
        Vector3 newPosition = DescBox.transform.localPosition;
        newPosition.x = localPoint.x;
        DescBox.transform.localPosition = newPosition;
        if (description != null)
        {
            if (isUnlocked)
            {
                TextMeshProUGUI desc = DescBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                desc.text = description;
                Transform box = DescBox.transform.GetChild(0);
                box.gameObject.SetActive(false);
                Transform box1 = DescBox.transform.GetChild(1);
                box1.gameObject.SetActive(true);
            }
            else
            {
                Transform box = DescBox.transform.GetChild(0);
                box.gameObject.SetActive(true);
                Transform box1 = DescBox.transform.GetChild(1);
                box1.gameObject.SetActive(false);
            }
        }
        else
        {
            TextMeshProUGUI desc = DescBox.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            desc.text = "Find all previous objects first!";
            Transform box = DescBox.transform.GetChild(0);
            box.gameObject.SetActive(false);
            Transform box1 = DescBox.transform.GetChild(1);
            box1.gameObject.SetActive(true);

        }
        DescBox.SetActive(true);
    }
}
