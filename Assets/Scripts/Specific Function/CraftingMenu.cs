using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingMenu : MonoBehaviour
{
    private GameManager gameManager;
    public void OnDisable()
    {
        gameManager.isCrafting = false;
        gameManager.CraftSelected.Clear();
    }
    public void OnEnable()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameManager.isCrafting = true;
    }

}
