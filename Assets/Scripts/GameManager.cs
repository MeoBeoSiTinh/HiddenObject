using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using Spine.Unity;

public class GameManager : MonoBehaviour
{
    public LevelData levelData;
    public int currentLevelIndex;
    private int currentStageIndex;
    private int specialStageIndex;
    private GameObject currentLevelInstance;
    private List<MyTarget> targetList;
    private List<MyTarget> specialList;
    public List<MyTarget> allTargetsList; // New list to contain all targets in every stage
    public List<MyTarget> allSpecialList; // New list to contain all special targets

    public GameObject mapHiding;
    public float targetSize; // Size of the camera zoom out
    public GameObject CameraRenderer;

    [Header("UI Settings")]
    public GameObject LevelMenuHolder;
    public GameObject levelCompleteUI;
    public GameObject mainMenuUI;
    public GameObject inGameUi;
    public GameObject hotbarUi;
    public TabGroup tabGroup;
    public GameObject Confetti;
    public GadgetManager gadgetManager;
    public Transform toolbarSlotsParent;
    public Transform SpecialSlotsParent;
    public GameObject specialFoundUi;
    public GameObject DescBox;
    public bool isHotBarMinimized = false;


    public void Start()
    {
        int i = 1;
        Transform LevelPanel = GameObject.Find("LevelPanel").transform;
        foreach (MyLevelData data in levelData.data)
        {
            GameObject LevelHolder = Instantiate(LevelMenuHolder, LevelPanel);
            LevelHolder.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Level " + i;

            // Add Button component and set its callback
            Button levelButton = LevelHolder.AddComponent<Button>();
            int levelIndex = i - 1; // Capture the current value of i
            levelButton.onClick.AddListener(() => LoadLevel(levelIndex));

            i++;
        }
        gadgetManager = GameObject.Find("GadgetManager").GetComponent<GadgetManager>();
    }

    public void LoadLevel(int levelIndex)
    {

        if (levelIndex < 0 || levelIndex + 1 > levelData.data.Count)
        {
            DeleteCurrentLevel();
            ResetMainCameraPosition();
            mainMenuUI.SetActive(true);
            inGameUi.SetActive(false);
            return;
        }
        mainMenuUI.SetActive(false);
        inGameUi.SetActive(true);

        DeleteCurrentLevel();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        currentLevelInstance = Instantiate(levelInfor.LevelPrefab);
        Camera.main.GetComponent<CameraHandle>().background_GameObject = GameObject.Find("Background");
        currentLevelIndex = levelIndex;
        // Populate allTargetsList with all targets in every stage
        allTargetsList = new List<MyTarget>();
        TextMeshProUGUI stageName = GameObject.Find("StageName").GetComponentInChildren<TextMeshProUGUI>();
        stageName.text = levelInfor.LevelName;
        foreach (var stage in levelInfor.stage)
        {
            allTargetsList.AddRange(stage.target);
        }
        foreach (var group in levelInfor.specialGroup)
        {
            allSpecialList.AddRange(group.special);
        }
        
        UpdateSpecialBar();
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

    public void TargetFound(GameObject target)
    {
        int targetIndex = allTargetsList.FindIndex(x => x.TargetName == target.name);
        if (targetIndex == -1)
        {
            return;
        }
        else
        {
            targetList.RemoveAll(x => x.TargetName == target.name);
            //change Hotbar color
            Transform slot = toolbarSlotsParent.GetChild(targetIndex).GetChild(0);
            Image bg = slot.GetComponentInChildren<Image>();

            //assign asset to the target
            Transform slot2 = toolbarSlotsParent.GetChild(targetIndex).GetChild(1);
            Image icon = slot2.GetComponent<Image>();
            Color iconColor = icon.color;
            iconColor.a = 0.5f; // Reduce transparency to 50%
            icon.color = iconColor;

            GameObject tick = new GameObject("tick");
            tick.transform.SetParent(toolbarSlotsParent.GetChild(targetIndex).transform);
            RectTransform tickRect = tick.AddComponent<RectTransform>();
            Image tickImage = tick.AddComponent<Image>();
            tickImage.sprite = Resources.Load<Sprite>("UI/tick");

            // Set size equal to sprite size
            if (tickImage.sprite != null)
            {
                tickRect.sizeDelta = new Vector2(tickImage.sprite.rect.width, tickImage.sprite.rect.height);
            }
            tickRect.localPosition = new Vector3(0f, 0f, 0f);
            tickRect.localScale = new Vector3(1f, 1f, 1f);
        }
        if (targetList.Count == 0 && currentStageIndex + 1 < levelData.data[currentLevelIndex].stage.Count)
        {
            LoadStage(currentStageIndex + 1);
        }
        else if (targetList.Count == 0 && currentStageIndex + 1 >= levelData.data[currentLevelIndex].stage.Count)
        {
            loadSpecial(0);
        }
    }

    public void specialFound(GameObject target, GameObject image)
    {

        specialFoundUi.SetActive(true);
        specialFoundUi.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = target.name;
        int specialIndex = allSpecialList.FindIndex(x => x.TargetName == target.name);
        if (specialIndex == -1)
        {
            return;
        }
        else
        {
            specialList.RemoveAll(x => x.TargetName == target.name);
            //change Hotbar color
            Transform slot = SpecialSlotsParent.GetChild(specialIndex).GetChild(0);
            Image bg = slot.GetComponentInChildren<Image>();
            //assign asset to the target
            Transform slot2 = SpecialSlotsParent.GetChild(specialIndex).GetChild(1);
            DragAndDrop dragAndDrop = slot2.GetComponent<DragAndDrop>();
            dragAndDrop.targetObject = target;

            if (bg != null)
            {
                bg.sprite = Resources.Load<Sprite>("UI/SpecialObjectBG2");
            }
            else
            {
                Debug.Log("vailon bug");
            }
        }
        
        StartCoroutine(DestroyImageAfterDelay(image));

    }

    private IEnumerator DestroyImageAfterDelay(GameObject image)
    {
        
        yield return new WaitForSeconds(2.5f); // Delay for 3 seconds
        CloseSpecialFound(image);
        if (specialList.Count == 0 && specialStageIndex + 1 < levelData.data[currentLevelIndex].specialGroup.Count)
        {
            loadSpecial(specialStageIndex + 1);
        }
    }

    private IEnumerator ShowLevelCompleteUIWithDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust the delay as needed
        levelCompleteUI.SetActive(true);
    }
    public void UpdateSpecialBar()
    {
        for (int i = 0; i < allSpecialList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + allSpecialList[i].TargetName);
            newSlotObject.transform.SetParent(SpecialSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(newSlotObject.transform);
            RectTransform backgroundRectTransform = backgroundObject.AddComponent<RectTransform>();
            backgroundRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the background  
            backgroundRectTransform.localScale = new Vector3(120, 120, 120); // Set scale of the slot
            backgroundRectTransform.anchoredPosition3D = new Vector3(backgroundRectTransform.anchoredPosition3D.x, backgroundRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image background = backgroundObject.AddComponent<Image>();
            background.sprite = Resources.Load<Sprite>("UI/SpecialObjectBG1");

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            iconRectTransform.localScale = new Vector3(85, 85, 85); // Set scale of the slot
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image image = iconObject.AddComponent<Image>();

            GameObject unlockPrefab = Resources.Load<GameObject>("anim/unlock/unlock");
            GameObject unlockObject = Instantiate(unlockPrefab);
            unlockObject.transform.SetParent(newSlotObject.transform);
            RectTransform unlockRectTransform = unlockObject.AddComponent<RectTransform>();
            unlockRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon
            unlockRectTransform.localScale = new Vector3(50, 50, 50); // Set scale of the slot
            unlockRectTransform.anchoredPosition3D = new Vector3(unlockRectTransform.anchoredPosition3D.x, unlockRectTransform.anchoredPosition3D.y, 0); // Set z position to 0

            // Add CanvasGroup to iconObject  
            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            //Add DragAndDrop script to iconObject
            DragAndDrop dragAndDrop = iconObject.AddComponent<DragAndDrop>();
            dragAndDrop.backgroundCanvas = backgroundObject.transform; // Assign background to DragAndDrop  

            Description description = iconObject.AddComponent<Description>();
            description.DescBox = DescBox;

            image.sprite = allSpecialList[i].TargetPrefab.GetComponent<SpriteRenderer>().sprite;
            image.gameObject.SetActive(true);
            iconObject.SetActive(false);

        }
    }

    public void UpdateHotBar()
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + targetList[i].TargetName);
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1,1,1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(newSlotObject.transform);
            RectTransform backgroundRectTransform = backgroundObject.AddComponent<RectTransform>();
            backgroundRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the background  
            backgroundRectTransform.localScale = new Vector3(120, 120, 120); // Set scale of the slot
            backgroundRectTransform.anchoredPosition3D = new Vector3(backgroundRectTransform.anchoredPosition3D.x, backgroundRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image background = backgroundObject.AddComponent<Image>();
            background.sprite = Resources.Load<Sprite>("UI/HotbarslotUI");

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            iconRectTransform.localScale = new Vector3(85,85,85); // Set scale of the slot
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image image = iconObject.AddComponent<Image>();

            // Add CanvasGroup to iconObject  
            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            // Add DragAndDrop script to iconObject  
            //DragAndDrop dragAndDrop = iconObject.AddComponent<DragAndDrop>();
            //dragAndDrop.backgroundCanvas = backgroundObject.transform; // Assign background to DragAndDrop  

            

            background.color = new Color(1f, 1f, 1f); // White color  

            // Set the sprite of the image to the target's icon  
            if (i < targetList.Count)
            {
                image.sprite = targetList[i].TargetPrefab.GetComponent<SpriteRenderer>().sprite;
            }

            image.gameObject.SetActive(true);
        }
    }

    public void ResetMainCameraPosition()
    {
        Camera.main.transform.position = new Vector3(-5, 10, -10); // Reset to default position
        Camera.main.orthographicSize = 8; // Reset to default size
    }

    public void loadSpecial(int index)
    {
        specialStageIndex = index;
        MyLevelData levelInfor = levelData.data[currentLevelIndex];
        if (levelInfor == null) return;

        specialList = new List<MyTarget>(levelInfor.specialGroup[index].special);

        List<int> SpecialIndexList = new List<int>();
        foreach (var special in specialList)
        {
            int specialIndex = allSpecialList.FindIndex(x => x.TargetName == special.TargetName);
            SpecialIndexList.Add(specialIndex);
            Transform icon = SpecialSlotsParent.GetChild(specialIndex).GetChild(1);
            Transform Lock = SpecialSlotsParent.GetChild(specialIndex).GetChild(2);

            SkeletonAnimation skeletonAnimation = Lock.GetComponent<SkeletonAnimation>();
            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationName = "unlock";
            }

            GameObject.Find(special.TargetName).GetComponent<TargetFind>().enabled = true;
            icon.GetComponent<Description>().description = special.Description;
        }

        StartCoroutine(ActivateIconsWithDelay(SpecialIndexList));
    }

    private IEnumerator ActivateIconsWithDelay(List<int> specialIndexList)
    {
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        foreach (var specialIndex in specialIndexList)
        {
            Transform icon = SpecialSlotsParent.GetChild(specialIndex).GetChild(1);
            icon.gameObject.SetActive(true);
        }
    }

    public void clearHotBar()
    {
        foreach (Transform child in toolbarSlotsParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in SpecialSlotsParent)
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
        LoadLevel(0);
    }
    public void OnMinimizeClicked()
    {
        if (isHotBarMinimized)
        {
            LeanTween.moveY(hotbarUi.GetComponent<RectTransform>(), hotbarUi.GetComponent<RectTransform>().anchoredPosition.y + GameObject.Find("Hotbar").GetComponent<RectTransform>().rect.height, 0.5f).setEase(LeanTweenType.easeInQuad);
            GameObject.Find("MinimizeButton").GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
            isHotBarMinimized = false;
        }
        else
        {
            LeanTween.moveY(hotbarUi.GetComponent<RectTransform>(), hotbarUi.GetComponent<RectTransform>().anchoredPosition.y - GameObject.Find("Hotbar").GetComponent<RectTransform>().rect.height, 0.5f).setEase(LeanTweenType.easeInQuad);
            GameObject.Find("MinimizeButton").GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 180);

            isHotBarMinimized = true;
        }        

    }

    public void CloseSpecialFound(GameObject image)
    {
        Destroy(image);
        specialFoundUi.SetActive(false);
        if (specialList.Count == 0 && specialStageIndex + 1 >= levelData.data[currentLevelIndex].specialGroup.Count)
        {
            allSpecialList.Clear();
            allTargetsList.Clear();
            clearHotBar();
            DescBox.SetActive(false);
            Confetti.SetActive(true);
            StartCoroutine(ShowLevelCompleteUIWithDelay());
        }
    }

    public void OnHomeClicked()
    {

    }


    public void OnScanClicked()
    {
        List<GameObject> targets = new List<GameObject>();
        foreach (MyTarget target in targetList)
        {
            GameObject targetObject = GameObject.Find(target.TargetName);
            if (targetObject != null)
            {
                targets.Add(targetObject);
            }
        }
        gadgetManager.Scan(targets);
    }

    public void OnCompassClicked()
    {
        List<GameObject> targets = new List<GameObject>();
        foreach (MyTarget target in targetList)
        {
            GameObject targetObject = GameObject.Find(target.TargetName);
            if (targetObject != null)
            {
                targets.Add(targetObject);
            }
        }
        gadgetManager.Compass(targets);
    }
    
}
