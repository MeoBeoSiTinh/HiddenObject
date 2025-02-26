using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour

{
    public LevelData levelData;
    public int currentLevelIndex;
    public Transform prefabsSpawn;
    private GameObject currentLevelInstance;
    public List<MyTarget> targetList;
    // Start is called before the first frame update
    void Start()
    {

        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        Debug.Log("Load Level: " + levelIndex);
        if (levelIndex < 0 || levelIndex >= levelData.data.Count) return;

        DeleteCurrentLevel();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;
        Debug.Log("Level Name: " + levelInfor.LevelPrefab.name);
        currentLevelInstance = Instantiate(levelInfor.LevelPrefab, prefabsSpawn);

        currentLevelIndex = levelIndex;
       

        targetList = new List<MyTarget>(levelInfor.target);

        for (int i = 0; i < targetList.Count; i++)
        {
            Debug.Log("Target: " + targetList[i].TargetName);
        }

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
        Debug.Log("Target Found in Manager: " + name);
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

    // Update is called once per frame
    void Update()
    {

    }
}
