using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuScript : MonoBehaviour
{
    private UIDocument _document;
    private Button _startButton;
    private GameManager gameManager;


    private void Awake()
    {
        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject != null)
        {
            gameManager = gameManagerObject.GetComponent<GameManager>();
        }
        else
        {
            Debug.LogError("GameManager object not found in the scene.");
        }
        _document = GetComponent<UIDocument>();
        _startButton = _document.rootVisualElement.Q<Button>("StartButton") as Button;
        _startButton.RegisterCallback<ClickEvent>(OnPlayButtonClick);
        gameManager.toolbarSlotsParent.gameObject.SetActive(false); // Disable hotbar UI

    }
    private void OnDisable()
    {
        _startButton.UnregisterCallback<ClickEvent>(OnPlayButtonClick);
    }
    private void OnPlayButtonClick(ClickEvent evt)
    {
        Debug.Log("Play button clicked");
        gameObject.SetActive(false);
        gameManager.toolbarSlotsParent.gameObject.SetActive(true);
        gameManager.LoadLevel(0);
    }
}

