using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Unity.VisualScripting;

public class UnlockHint : MonoBehaviour, IPointerClickHandler
{
    [Header("Direct Reference Approach")]
    public GameObject targetObject;


    public void OnPointerClick(PointerEventData eventData)
    {
        // Approach 1: Directly modify a value on another GameObject
        if (targetObject != null)
        {
            Description desc = targetObject.GetComponent<Description>();
            if (desc != null)
            {
                desc.isUnlocked = true;
            }
            GameObject Locked = GameObject.Find("Locked");
            Locked.SetActive(false);
            GameObject Desc = desc.DescBox;
            Debug.Log(desc.description);
            Desc.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = desc.description;
            Desc.transform.GetChild(1).gameObject.SetActive(true);
        }

    }
}