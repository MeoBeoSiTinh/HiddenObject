using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftSelect : MonoBehaviour, IPointerDownHandler
{
    public GameObject selectedObject;
    public Boolean isSelected = false;
    private GameManager gameManager;
    private GameObject Dialogue;
    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Dialogue = gameManager.Dialogue;
    }   
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerEnter == gameObject && gameManager.isCrafting)
        {
            if (selectedObject != null)
            {
                isSelected = !isSelected;
                String text = "";
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
                for (int i = 0; i < gameManager.CraftSelected.Count; i++)
                {
                    string name = gameManager.CraftSelected[i];
                    if(i>0)
                    {
                        text += "+ ";
                    }
                    text += $"<sprite name=\"{name}\"> ";
                }
                text += " = ?";

                Dialogue.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text = text;

            }
        }
    }
}
