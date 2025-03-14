using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public float targetSize; // Size of the camera zoom out
    public GameObject CameraRenderer;
    public GameObject Confetti;

    public GameObject LevelMenuHolder;
    public GameObject levelCompleteUI;
    public GameObject mainMenuUI;
    public GameObject inGameUi;
    public GameObject hotbarUi;
    public bool isHotBarMinimized = false;


    public void Start()
    {
        int i = 1;
        Transform LevelPanel = GameObject.Find("LevelPanel").transform;
        foreach (MyLevelData data in levelData.data)
        {
            GameObject LevelHolder = Instantiate(LevelMenuHolder, LevelPanel);
            LevelHolder.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Level " + i;
            i++;
        }
    }

    public void LoadLevel(int levelIndex)
    {   
        
        Debug.Log("Load Level: " + levelIndex);
        if (levelIndex < 0 || levelIndex + 1  > levelData.data.Count)
        {
            Debug.Log("Game Complete");
            DeleteCurrentLevel();
            ResetMainCameraPosition();
            mainMenuUI.SetActive(true);
            inGameUi.SetActive(false);
            return;
        }

        DeleteCurrentLevel();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        currentLevelInstance = Instantiate(levelInfor.LevelPrefab);
        Camera.main.GetComponent<CameraHandle>().background_GameObject = GameObject.Find("Background");
        currentLevelIndex = levelIndex;
        // Populate allTargetsList with all targets in every stage
        allTargetsList = new List<MyTarget>();
        TextMeshProUGUI stageName = GameObject.Find("StageName").GetComponentInChildren<TextMeshProUGUI>();
        Debug.Log("Level Name: " + levelInfor.LevelName);
        stageName.text = levelInfor.LevelName;
        foreach (var stage in levelInfor.stage)
        {
            allTargetsList.AddRange(stage.target);
        }
        LoadStage(0);
        mapHiding.SetActive(true);
        foreach (Transform child in mapHiding.transform)
        {
            child.gameObject.SetActive(true);
        }
        CameraRenderer.GetComponent<CameraFocus>().ResetMap(currentLevelIndex + 1);
        CameraRenderer.SetActive(true);

    }

    public void LoadStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
        MyLevelData levelInfor = levelData.data[currentLevelIndex];
        if (levelInfor == null) return;

        targetList = new List<MyTarget>(levelInfor.stage[stageIndex].target);
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject.Find(targetList[i].TargetName).GetComponent<TargetFind>().enabled = true;
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
        StartCoroutine(MoveCameraToStage(stageIndex));
        UpdateHotBar();
    }

    private IEnumerator MoveCameraToStage(int stageIndex)
    {
        float duration = 1f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition;
        float startSize = Camera.main.orthographicSize;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();
        

        // Determine the target position based on the stage index
        switch (stageIndex)
        {
            case 0:
                yield break;
            case 1:
                cameraHandle.enabled = false;
                targetPosition = new Vector3(5, 10, Camera.main.transform.position.z);
                break;
            case 2:
                cameraHandle.enabled = false;
                targetPosition = new Vector3(-5, -10, Camera.main.transform.position.z);
                break;
            case 3:
                cameraHandle.enabled = false;
                targetPosition = new Vector3(5, -10, Camera.main.transform.position.z);
                break;
            default:
                yield break; // Exit if the stage index is invalid
        }
        if (targetPosition == null)
        {
            yield return null;
        }
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = targetSize;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }




    public void DeleteCurrentLevel()
    {
        if (currentLevelInstance == null) return;
        Destroy(currentLevelInstance);
        currentLevelInstance = null;
    }

    public void TargetFound(string name)
    {
        GameObject target = GameObject.Find(name);
        targetList.RemoveAll(x => x.TargetName == name);
        int targetIndex = allTargetsList.FindIndex(x => x.TargetName == name);
        Debug.Log("index: " + targetIndex);
        //change Hotbar color
        Transform slot = toolbarSlotsParent.GetChild(targetIndex).GetChild(0);
        Image bg = slot.GetComponentInChildren<Image>();

        //assign asset to the target
        Transform slot2 = toolbarSlotsParent.GetChild(targetIndex).GetChild(1);
        //DragAndDrop asset = slot2.GetComponentInChildren<DragAndDrop>();
        //Debug.Log("Target Found in Manager: " + name);
        //asset.targetObject = target;
        target.SetActive(false);
        target.GetComponent<SpriteRenderer>().enabled = true;

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
                clearHotBar();
                Confetti.SetActive(true);
                StartCoroutine(ShowLevelCompleteUIWithDelay());
                return;
            }
            Debug.Log("Stage Complete");
            LoadStage(currentStageIndex + 1);
        }
    }

    private IEnumerator ShowLevelCompleteUIWithDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust the delay as needed
        levelCompleteUI.SetActive(true);
    }

    public void UpdateHotBar()
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + targetList[i].TargetName);
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(newSlotObject.transform);
            RectTransform backgroundRectTransform = backgroundObject.AddComponent<RectTransform>();
            backgroundRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the background  
            backgroundRectTransform.localScale = new Vector3(135, 135, 135); // Set scale of the slot
            backgroundRectTransform.anchoredPosition3D = new Vector3(backgroundRectTransform.anchoredPosition3D.x, backgroundRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image background = backgroundObject.AddComponent<Image>();
            background.sprite = Resources.Load<Sprite>("UI/HotbarslotUI");

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image image = iconObject.AddComponent<Image>();

            // Add CanvasGroup to iconObject  
            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            // Add DragAndDrop script to iconObject  
            //DragAndDrop dragAndDrop = iconObject.AddComponent<DragAndDrop>();
            //dragAndDrop.backgroundCanvas = backgroundObject.transform; // Assign background to DragAndDrop  

            Description description = iconObject.AddComponent<Description>();
            description.description = targetList[i].Description;
            description.uiPrefab = Resources.Load<GameObject>("UI/DescBox");

            background.color = new Color(1f, 1f, 1f); // White color  

            // Set the sprite of the image to the target's icon  
            if (i < targetList.Count)
            {
                image.sprite = targetList[i].TargetPrefab.GetComponent<SpriteRenderer>().sprite;
            }

            image.gameObject.SetActive(true);
        }
    }

    public void clearHotBar()
    {
        foreach (Transform child in toolbarSlotsParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnContinueClicked()
    {
        levelCompleteUI.SetActive(false);
        Confetti.SetActive(false);
        LoadLevel(currentLevelIndex + 1);
    }
    public void OnStartClicked()
    {
        mainMenuUI.SetActive(false);
        inGameUi.SetActive(true);
        LoadLevel(0);
    }
    public void OnMinimizeClicked()
    {
        if (isHotBarMinimized)
        {
            LeanTween.moveY(hotbarUi.GetComponent<RectTransform>(), hotbarUi.GetComponent<RectTransform>().anchoredPosition.y + toolbarSlotsParent.GetComponent<RectTransform>().rect.height, 0.5f).setEase(LeanTweenType.easeInQuad);
            GameObject.Find("MinimizeButton").GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
            isHotBarMinimized = false;
        }
        else
        {
            LeanTween.moveY(hotbarUi.GetComponent<RectTransform>(), hotbarUi.GetComponent<RectTransform>().anchoredPosition.y - toolbarSlotsParent.GetComponent<RectTransform>().rect.height, 0.5f).setEase(LeanTweenType.easeInQuad);
            GameObject.Find("MinimizeButton").GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180);

            isHotBarMinimized = true;
        }        
        Debug.Log("Minimize Clicked");

    }


    // Update is called once per frame
    void Update()
    {
       
    }
    public void ResetMainCameraPosition()
    {
        Camera.main.transform.position = new Vector3(-5, 10, -10); // Reset to default position
        Camera.main.orthographicSize = 8; // Reset to default size
    }
}
