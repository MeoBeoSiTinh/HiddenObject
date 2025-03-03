using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public LevelData levelData;
    public int currentLevelIndex;
    private int currentStageIndex;
    private GameObject currentLevelInstance;
    private List<MyTarget> targetList;
    public List<MyTarget> allTargetsList; // New list to contain all targets in every stage
    public Transform toolbarSlotsParent;
    public GameObject mapHiding;

    public void LoadLevel(int levelIndex)
    {
        Debug.Log("Load Level: " + levelIndex);
        if (levelIndex < 0 || levelIndex >= levelData.data.Count) return;

        DeleteCurrentLevel();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        currentLevelInstance = Instantiate(levelInfor.LevelPrefab);

        currentLevelIndex = levelIndex;
        // Populate allTargetsList with all targets in every stage
        allTargetsList = new List<MyTarget>();
        foreach (var stage in levelInfor.stage)
        {
            allTargetsList.AddRange(stage.target);
        }
        LoadStage(0);
        UpdateHotBar(levelIndex);
        mapHiding.SetActive(true);
        foreach (Transform child in mapHiding.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void LoadStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
        MyLevelData levelInfor = levelData.data[currentLevelIndex];
        if (levelInfor == null) return;

        targetList = new List<MyTarget>(levelInfor.stage[stageIndex].target);
        for (int i = 0; i < targetList.Count; i++)
        {
            Debug.Log("Target: " + targetList[i].TargetName);
        }

        // Disable the child of mapHiding with index equal to stageIndex - 1
        if (stageIndex - 1 >= 0 && stageIndex - 1 < mapHiding.transform.childCount)
        {
            mapHiding.transform.GetChild(stageIndex - 1).gameObject.SetActive(false);
        }

        // Set the currentStage value in mainCamera CameraHandle script to stageIndex
        Camera.main.GetComponent<CameraHandle>().currentStage = stageIndex;

        // Start the coroutine to move the camera
        //StartCoroutine(MoveCameraToStage(stageIndex));
    }

    private IEnumerator MoveCameraToStage(int stageIndex)
    {
        float duration = 2f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition;

        // Determine the target position based on the stage index
        switch (stageIndex)
        {
            case 0:
                targetPosition = new Vector3(-5, 10, Camera.main.transform.position.z);
                break;
            case 1:
                targetPosition = new Vector3(5, 10, Camera.main.transform.position.z);
                break;
            case 2:
                targetPosition = new Vector3(-5, -10, Camera.main.transform.position.z);
                break;
            case 3:
                targetPosition = new Vector3(5, -10, Camera.main.transform.position.z);
                break;
            default:
                yield break; // Exit if the stage index is invalid
        }

        // Smoothly move the camera to the target position
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the camera reaches the exact target position
        Camera.main.transform.position = targetPosition;
    }




    public void DeleteCurrentLevel()
    {
        if (currentLevelInstance == null) return;
        Destroy(currentLevelInstance);
        currentLevelInstance = null;
    }

    public void TargetFound(string name)
    {
        targetList.RemoveAll(x => x.TargetName == name);
        int targetIndex = allTargetsList.FindIndex(x => x.TargetName == name);
        Debug.Log("index: " + targetIndex);
        //change Hotbar color
        Transform slot = toolbarSlotsParent.GetChild(targetIndex).GetChild(0);
        Image bg = slot.GetComponentInChildren<Image>();

        //assign asset to the target
        Transform slot2 = toolbarSlotsParent.GetChild(targetIndex).GetChild(1);
        DragAndDrop asset = slot2.GetComponentInChildren<DragAndDrop>();
        Debug.Log("Target Found in Manager: " + name);
        asset.targetObject = GameObject.Find(name);

        if (bg != null)
        {
            bg.color = new Color(1f, 1f, 0.5f); // Light yellow color  
        }

        //change level
        if (targetList.Count == 0)
        {
            if (currentStageIndex + 1 >= levelData.data[currentLevelIndex].stage.Count)
            {
                Debug.Log("Level Complete");
                allTargetsList.Clear();
                LoadLevel(currentLevelIndex + 1);
                return;
            }
            Debug.Log("Stage Complete");
            LoadStage(currentStageIndex + 1);
        }
    }

    public void UpdateHotBar(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelData.data.Count) return;

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        // Clear existing slots  
        foreach (Transform child in toolbarSlotsParent)
        {
            Destroy(child.gameObject);
        }

        // Calculate total targets in all stages  
        for (int i = 0; i < allTargetsList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + allTargetsList[i].TargetName);
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            Image newSlot = newSlotObject.AddComponent<Image>();

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(newSlotObject.transform);
            RectTransform backgroundRectTransform = backgroundObject.AddComponent<RectTransform>();
            backgroundRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the background  
            Image background = backgroundObject.AddComponent<Image>();

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            Image image = iconObject.AddComponent<Image>();

            // Add CanvasGroup to iconObject  
            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            // Add DragAndDrop script to iconObject  
            DragAndDrop dragAndDrop = iconObject.AddComponent<DragAndDrop>();
            dragAndDrop.backgroundCanvas = backgroundObject.transform; // Assign background to DragAndDrop  

            background.color = new Color(1f, 1f, 1f); // White color  

            // Set the sprite of the image to the target's icon  
            if (i < allTargetsList.Count)
            {
                image.sprite = allTargetsList[i].Icon;
            }

            image.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
