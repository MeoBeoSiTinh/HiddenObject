using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour

{
    public LevelData levelData;
    public int currentLevelIndex;
    public Transform prefabsSpawn;
    private GameObject currentLevelInstance;
    private List<MyTarget> targetList;
    public Transform toolbarSlotsParent;


    public void LoadLevel(int levelIndex)
    {
        Debug.Log("Load Level: " + levelIndex);
        if (levelIndex < 0 || levelIndex >= levelData.data.Count) return;

        DeleteCurrentLevel();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        currentLevelInstance = Instantiate(levelInfor.LevelPrefab, prefabsSpawn);

        currentLevelIndex = levelIndex;
       

        targetList = new List<MyTarget>(levelInfor.target);

        for (int i = 0; i < targetList.Count; i++)
        {
            Debug.Log("Target: " + targetList[i].TargetName);
        }

        UpdateHotBar(levelIndex);

    }

    public void DeleteCurrentLevel()
    {
        if (currentLevelInstance == null) return;
        Destroy(currentLevelInstance);
        currentLevelInstance = null;

    }

    public void TargetFound(string name)
    {
        //listing target
        int targetIndex = targetList.FindIndex(x => x.TargetName == name);
        if (targetIndex != -1)
        {
            targetList.RemoveAt(targetIndex);
        }
        else
        {
            Debug.Log("Target not found: " + name);
        }

        //change Hotbar color
        Transform slot = toolbarSlotsParent.GetChild(targetIndex).GetChild(0);
        Image image = slot.GetComponentInChildren<Image>();

        if (image != null)
        {
            image.color = new Color(1f, 1f, 0.5f); // Light yellow color  
        }

        //change level
        if (targetList.Count == 0)
        {
            if (currentLevelIndex + 1 >= levelData.data.Count)
            {
                Debug.Log("Game Complete");
                DeleteCurrentLevel();
                return;
            }
            Debug.Log("Level Complete");
            LoadLevel(currentLevelIndex + 1);
        }
    }

    public void UpdateHotBar(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelData.data.Count) return;

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        for (int i = 0; i < toolbarSlotsParent.childCount; i++)
        {
            Transform slot = toolbarSlotsParent.GetChild(i).GetChild(1);
            Image image = slot.GetComponentInChildren<Image>();
            DragAndDrop dragAndDrop = slot.GetComponentInChildren<DragAndDrop>();

            Transform slot2 = toolbarSlotsParent.GetChild(i).GetChild(0);
            Image background = slot2.GetComponentInChildren<Image>();

            if (image != null)
            {
                background.color = new Color(1f, 1f, 1f); // White color
                if (i < levelInfor.target.Count)
                {
                    // Assign the icon to the Image component and enable the GameObject
                    Debug.Log("Icon: " + levelInfor.target[i].Icon);
                    dragAndDrop.targetName = levelInfor.target[i].TargetName;
                    image.sprite = levelInfor.target[i].Icon;
                    image.gameObject.SetActive(true);
                }
                else
                {
                    // Hide the Image component if there's no icon for this slot
                    image.gameObject.SetActive(false);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
