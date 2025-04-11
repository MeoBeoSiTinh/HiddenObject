using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftSelect : MonoBehaviour, IPointerDownHandler
{
    public GameObject selectedObject;
    public Boolean isSelected = false;
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }   
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject && gameManager.isCrafting)
        {
            if (selectedObject != null)
            {
                isSelected = !isSelected;
                if (isSelected)
                {
                    gameManager.CraftSelected.Add(selectedObject.name);
                    Debug.Log("Selected: " + selectedObject.name);
                }
                else
                {
                    gameManager.CraftSelected.Remove(selectedObject.name);
                    Debug.Log("Unselected: " + selectedObject.name);
                }
            }
        }
    }
}
