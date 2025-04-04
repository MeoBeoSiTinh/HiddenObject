using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Crafter : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject CrafterMenu;
    public List<GameObject> CrafterList;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CrafterMenu = gameManager.CrafterMenu;
    }
    public void ShowRecipe()
    {
        gameManager.OpenCrafter();
        TextMeshProUGUI text = CrafterMenu.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        text.text = "I can craft stuffs, give me: ";
        for (int i = 0; i < CrafterList.Count; i++)
        {
            text.text += CrafterList[i].name;
            if (i < CrafterList.Count - 1)
            {
                text.text += " + ";
            }
        }

    }
    
}
