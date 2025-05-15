using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
using Spine.Unity;
using System.Linq;
using Unity.VisualScripting;
using Spine.Unity.Examples;

public class GameManager : MonoBehaviour
{
    public LevelData levelData;
    public int currentLevelIndex;
    private int currentStageIndex;
    private int specialStageIndex;
    private GameObject currentLevelInstance;
    private List<MyTarget> targetList;
    public List<MyTarget> specialList;
    public List<MyTarget> allTargetsList;
    public List<MyTarget> allSpecialList;
    public List<string> foundTarget;
    public GameObject mapHiding;
    public float targetSize; // Size of the camera zoom out
    public GameObject CameraRenderer;
    public GameObject CurrentCrafter;

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
    public Transform PlayerInfo;
    public GameObject Dialogue;
    public GameObject ProgressBar;


    [Header("PlayerData")]
    public string username = "Meobeo";

    [Header("Boolean")]
    public bool isHotBarMinimized = false;
    public bool isCrafting = false;


    private class SaveObject
    {
        public string username;
    }
    public void Awake()
    {
        //SaveSystem.Init();
        //Load();
        //Transform name = PlayerInfo.GetChild(0).GetChild(0);
        //name.GetComponent<TextMeshProUGUI>().text = username;

    }

    public void Save() {
        SaveObject saveObject = new SaveObject
        {
            username = username
        };
        string json = JsonUtility.ToJson(saveObject);
        SaveSystem.Save(json);
    }
    public void Load()
    {
        string saveString = SaveSystem.Load();
        if (saveString != null)
        {
            SaveObject saveObject = JsonUtility.FromJson<SaveObject>(saveString);
            username = saveObject.username;
        }
    }
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
        CreateEmptyUIElementAtParentRectPosition(toolbarSlotsParent.GetComponent<RectTransform>());
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
        Camera.main.GetComponent<CameraHandle>().background_GameObject = GameObject.FindGameObjectWithTag("Background");
        currentLevelIndex = levelIndex;

        // Populate allTargetsList with all targets in every stage
        allTargetsList = new List<MyTarget>();

        //TextMeshProUGUI stageName = GameObject.Find("StageName").GetComponentInChildren<TextMeshProUGUI>();
        //stageName.text = levelInfor.LevelName;

        foreach (var stage in levelInfor.stage)
        {
            allTargetsList.AddRange(stage.target);
        }
        specialList.AddRange(levelInfor.special);
        allSpecialList.AddRange(levelInfor.special);
        foundTarget.Clear();
        UpdateSpecialBar();
        UpdateHotBar();
        LoadStage(0);
        ProgressBar.GetComponent<ProgressBar>().resetProgressBar();
        mapHiding.SetActive(true);
        foreach (Transform child in mapHiding.transform)
        {
            child.gameObject.SetActive(true);
        }
        //CameraRenderer.GetComponent<CameraFocus>().ResetMap(currentLevelIndex + 1);
        //CameraRenderer.SetActive(true);

    }

    public void LoadStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
        MyLevelData levelInfor = levelData.data[currentLevelIndex];
        if (levelInfor == null) return;

        targetList = new List<MyTarget>(levelInfor.stage[stageIndex].target);
        for (int i = 0; i < targetList.Count; i++)
        {
            try
            {
                GameObject.Find(targetList[i].TargetName).GetComponent<ObjectTouch>().enabled = true;
            }
            catch
            {
                Debug.Log("Object not found: " + targetList[i].TargetName);
            }
        }

        // Disable the child of mapHiding with index equal to stageIndex - 1  

        // Set the currentStage value in mainCamera CameraHandle script to stageIndex  
        Camera.main.GetComponent<CameraHandle>().currentStage = stageIndex;

        // Start the coroutine to move the camera  
        StartCoroutine(MoveCameraToStage(stageIndex));
        if (stageIndex > 0 && stageIndex <= mapHiding.transform.childCount)
        {
            Transform up = mapHiding.transform.GetChild(stageIndex - 1).GetChild(0);
            Transform down = mapHiding.transform.GetChild(stageIndex - 1).GetChild(1);
            Transform left = mapHiding.transform.GetChild(stageIndex - 1).GetChild(2);
            Transform right = mapHiding.transform.GetChild(stageIndex - 1).GetChild(3);

            Vector3 originalUpPosition = up.position;
            Vector3 originalDownPosition = down.position;
            Vector3 originalLeftPosition = left.position;
            Vector3 originalRightPosition = right.position;

            // Animate each transform to move in their respective directions  
            LeanTween.moveY(up.gameObject, up.position.y + 8f, 2f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                up.position = originalUpPosition;
            });
            LeanTween.moveY(down.gameObject, down.position.y - 8f, 2f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                down.position = originalDownPosition;
            });
            LeanTween.moveX(left.gameObject, left.position.x - 8f, 2f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                left.position = originalLeftPosition;
            });
            LeanTween.moveX(right.gameObject, right.position.x + 8f, 2f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
            {
                right.position = originalRightPosition;
                mapHiding.transform.gameObject.SetActive(false);
            });
        }
    }

    private IEnumerator MoveCameraToStage(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0:
                yield break;
            case 1:
                targetSize = 22f; break;
        }
        float duration = 1f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 targetPosition;
        float startSize = Camera.main.orthographicSize;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
        targetPosition = new Vector3(0, 0, Camera.main.transform.position.z);
        if (targetPosition == null)
        {
            yield break;
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
        yield return null; // Wait for the current frame to finish


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
            Transform slot = toolbarSlotsParent.GetChild(targetIndex + allSpecialList.Count);
            Image bg = slot.GetComponentInChildren<Image>();
            bg.color = Color.green;

            foundTarget.Add(target.name);
            //assign asset to the target
            //Transform slot2 = toolbarSlotsParent.GetChild(targetIndex).GetChild(0);
            //Image icon = slot2.GetComponent<Image>();
            //CraftSelect craftSelect = slot2.GetComponent<CraftSelect>();
            //craftSelect.selectedObject = target;


        }
        if (targetList.Count == 0 && currentStageIndex + 1 < levelData.data[currentLevelIndex].stage.Count)
        {
            LoadStage(currentStageIndex + 1);
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
            foundTarget.Add(target.name);
            ProgressBar progress = ProgressBar.GetComponent<ProgressBar>();
            int foundSpecial = (allSpecialList.Count - specialList.Count);
            Debug.Log("foundSpecial: " + foundSpecial);
            Debug.Log("all: " + allSpecialList.Count);
            Debug.Log("remain: " + specialList.Count);
            float targetValue = 33 * foundSpecial;
            Debug.Log("targetValue: " + targetValue);
            StartCoroutine(increaseProgress(progress, (int) targetValue));

            //change Hotbar color
            Transform slot = toolbarSlotsParent.GetChild(specialIndex);
            Image bg = slot.GetComponentInChildren<Image>();
            //assign asset to the target
            Transform slot2 = toolbarSlotsParent.GetChild(specialIndex).GetChild(0);

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
    private IEnumerator increaseProgress(ProgressBar progress, int targetValue)
    {
        float duration = 1f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        int startValue = progress.current;
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            progress.current = (int)Mathf.Lerp(startValue, targetValue, t);
            yield return null; // Wait for the next frame
        }
        if (targetValue >= 1)
        {
            progress.transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
        if (targetValue >= 50)
        {
            progress.transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
        if (targetValue >= 99)
        {
            progress.transform.GetChild(3).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
        yield return null; // Wait for the current frame to finish
    }

    private IEnumerator DestroyImageAfterDelay(GameObject image)
    {
        
        yield return new WaitForSeconds(2.5f); // Delay for 3 seconds
        CloseSpecialFound(image);
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
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();
            newSlot.sprite = Resources.Load<Sprite>("UI/SpecialObjectBG2");


            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            iconRectTransform.localScale = new Vector3(85, 85, 85); // Set scale of the slot
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image image = iconObject.AddComponent<Image>();

            //GameObject unlockPrefab = Resources.Load<GameObject>("anim/unlock/unlock");
            //GameObject unlockObject = Instantiate(unlockPrefab);
            //unlockObject.transform.SetParent(newSlotObject.transform);
            //RectTransform unlockRectTransform = unlockObject.AddComponent<RectTransform>();
            //unlockRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon
            //unlockRectTransform.localScale = new Vector3(50, 50, 50); // Set scale of the slot
            //unlockRectTransform.anchoredPosition3D = new Vector3(unlockRectTransform.anchoredPosition3D.x, unlockRectTransform.anchoredPosition3D.y, 0); // Set z position to 0

            // Add CanvasGroup to iconObject  
            //CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            //Description description = iconObject.AddComponent<Description>();
            //description.DescBox = DescBox;
            //description.description = allSpecialList[i].Description;

            image.sprite = allSpecialList[i].TargetPrefab.GetComponent<SpriteRenderer>().sprite;
            image.gameObject.SetActive(true);

        }
    }

    public void UpdateHotBar()
    {
        for (int i = 0; i < allTargetsList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + allTargetsList[i].TargetName);
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1,1,1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();
            newSlot.sprite = Resources.Load<Sprite>("UI/HotbarslotUI");


            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();
            iconRectTransform.anchoredPosition = new Vector2(0, 0); // Set position of the icon
            iconRectTransform.sizeDelta = new Vector2(1, 1); // Set size of the icon  
            iconRectTransform.localScale = new Vector3(85,85,85); // Set scale of the icon
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image image = iconObject.AddComponent<Image>();

            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            CraftSelect craftSelect = iconObject.AddComponent<CraftSelect>();


            image.sprite = allTargetsList[i].TargetPrefab.GetComponent<SpriteRenderer>().sprite;

            image.gameObject.SetActive(true);
        }
    }

    public void ResetMainCameraPosition()
    {
        Camera.main.transform.position = new Vector3(0, 0, 0); // Reset to default position
        Camera.main.orthographicSize = 8; // Reset to default size
    }

    //public void loadSpecial(int index)
    //{
    //    specialStageIndex = index;
    //    MyLevelData levelInfor = levelData.data[currentLevelIndex];
    //    if (levelInfor == null) return;

    //    specialList = new List<MyTarget>(levelInfor.specialGroup[index].special);

    //    List<int> SpecialIndexList = new List<int>();
    //    foreach (var special in specialList)
    //    {
    //        int specialIndex = allSpecialList.FindIndex(x => x.TargetName == special.TargetName);
    //        SpecialIndexList.Add(specialIndex);
    //        Transform icon = SpecialSlotsParent.GetChild(specialIndex).GetChild(1);
    //        Transform Lock = SpecialSlotsParent.GetChild(specialIndex).GetChild(2);

    //        SkeletonAnimation skeletonAnimation = Lock.GetComponent<SkeletonAnimation>();
    //        if (skeletonAnimation != null)
    //        {
    //            skeletonAnimation.AnimationName = "unlock";
    //        }

    //        GameObject.Find(special.TargetName).GetComponent<TargetFind>().enabled = true;
    //        icon.GetComponent<Description>().description = special.Description;
    //    }

    //    StartCoroutine(ActivateIconsWithDelay(SpecialIndexList));
    //}

    private IEnumerator ActivateIconsWithDelay(List<int> specialIndexList)
    {
        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        foreach (var specialIndex in specialIndexList)
        {
            Transform icon = SpecialSlotsParent.GetChild(specialIndex).GetChild(1);
            icon.gameObject.SetActive(true);
        }
    }
    private GameObject CreateEmptyUIElementAtParentRectPosition(RectTransform parentRect)
    {
        // Create an empty GameObject
        GameObject emptyUIElement = new GameObject("UILocation");

        // Add RectTransform component
        RectTransform rectTransform = emptyUIElement.AddComponent<RectTransform>();


        // Get the world position of the parentRect
        Vector3 parentWorldPosition = parentRect.TransformPoint(parentRect.rect.center);

        // Convert the world position to local position in the Canvas
        RectTransform canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, parentWorldPosition),
            null,
            out localPoint
        );
        // Set the anchored position of the empty UI element to the local point
        rectTransform.anchoredPosition = localPoint;

        return emptyUIElement;
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
        ClearIngameMenu();
    }

    public void ClearIngameMenu()
    {
        specialFoundUi.SetActive(false);
        if (specialList.Count == 0)
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

    public void OpenDialogue()
    {
        Dialogue.SetActive(true);
    }
    
    public void OnCraftClicked()
    {
        Crafter crafter = CurrentCrafter.GetComponent<Crafter>();
        Boolean craftable = false;
        craftRecipe recipe = crafter.recipes[crafter.currectRecipeIndex];
        craftable = !recipe.ingredients.Except(foundTarget).Any();
        if (craftable)
        {
            
            StartCoroutine(HandleResultObject(recipe, crafter));
        }
        else
        {
            List<string> missingIngredients = recipe.ingredients.Except(foundTarget).ToList();
            ScrollRectFocus scroll = GameObject.Find("Scroll").GetComponent<ScrollRectFocus>();
            CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();
            cameraHandle.ShakeCamera(0.2f, 0.1f);
            foreach (string name in missingIngredients)
            {
                GameObject icon = GameObject.Find("Icon" + name);
                if (icon != null)
                {
                    StartCoroutine(FlashRed(icon));

                    bool isVisible = scroll.IsElementVisible(icon.GetComponent<RectTransform>());
                    if (!isVisible)
                    {
                        scroll.ScrollToView(icon.GetComponent<RectTransform>());
                        Debug.Log("visible");
                    }
                }
            }
        }
    }


    private IEnumerator FlashRed(GameObject icon)
    {
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null)
        {
            Color originalColor = new Color(1, 1, 1);
            Color flashColor = new Color(0.984f, 0.431f, 0.431f); // Equivalent to "FB6E6E" in RGB (normalized to 0-1 range)  

            float duration = 0.3f; // Duration for each transition
            for (int i = 0; i < 1; i++) // Flash times  
            {
                // Gradually change to flashColor
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    iconImage.color = Color.Lerp(originalColor, flashColor, elapsedTime / duration);
                    yield return null;
                }

                // Gradually change back to originalColor
                elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    iconImage.color = Color.Lerp(flashColor, originalColor, elapsedTime / duration);
                    yield return null;
                }
            }
        }
    }
    private IEnumerator HandleResultObject(craftRecipe recipe, Crafter crafter)
    {

        Camera mainCamera = Camera.main;
        // Instantiate the result object at the center position
        Vector2 position = CurrentCrafter.transform.position;
        Dialogue.transform.GetChild(1).gameObject.SetActive(false);
        //Rotate the dialogue box
        LeanTween.rotateY(Dialogue.transform.GetChild(0).GetChild(1).gameObject, 90f, 0.25f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            crafter.SetResultText();
            LeanTween.rotateY(Dialogue.transform.GetChild(0).GetChild(1).gameObject, 180f, 0.25f).setEase(LeanTweenType.easeInOutQuad);
        });
        yield return new WaitForSeconds(1.5f);

        GameObject resultObject = Instantiate(recipe.Result, position, Quaternion.identity);
        resultObject.name = recipe.Result.name;
        Vector2 UiPos = mainCamera.WorldToScreenPoint(position);
        
        List<RectTransform> IconToRemove = new List<RectTransform>();
        foreach (string name in recipe.ingredients)
        {
            if (allSpecialList.Find(x => x.TargetName == name) == null)
            {
                GameObject icon = GameObject.Find("Icon" + name);
                allTargetsList.Remove(allTargetsList.Find(x => x.TargetName == name));
                IconToRemove.Add(icon.GetComponent<RectTransform>());
            }
        }

        //Delete crafted recipe
        crafter.recipes.Remove(recipe);
        crafter.currectRecipeIndex = 0;
        if (crafter.recipes.Count == 1)
        {
            disableButton(2);
        }
        else
        {
            enableButton(2);
        }
        disableButton(0);

        //UI HotBar transition
        LayoutGroupMultiRemover remover = GetComponent<LayoutGroupMultiRemover>();
        remover.RemoveMultipleAndSlide(IconToRemove);


        //Result bounce
        AnimateWithDecreasingBounces(resultObject);
        Dialogue.transform.GetChild(0).GetChild(1).rotation = Quaternion.Euler(0, 0, 0);
        Dialogue.SetActive(false);
        Dialogue.transform.GetChild(1).gameObject.SetActive(true);
        yield return null; // Wait for the current frame to finish
    }

    void AnimateWithDecreasingBounces(GameObject obj)
    {
        float duration = 1.5f;
        float distance = 1f;
        float originalY = obj.transform.position.y;

        // Horizontal movement
        LeanTween.moveX(obj, obj.transform.position.x + distance, duration)
            .setEase(LeanTweenType.linear);

        // First bounce (highest)
        LeanTween.moveY(obj, originalY + 0.8f, duration * 0.2f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                LeanTween.moveY(obj, originalY, duration * 0.2f)
                    .setEase(LeanTweenType.easeInQuad)
                    .setOnComplete(() => {
                        // Second bounce (medium)
                        LeanTween.moveY(obj, originalY + 0.5f, duration * 0.1f)
                            .setEase(LeanTweenType.easeOutQuad)
                            .setOnComplete(() => {
                                LeanTween.moveY(obj, originalY, duration * 0.1f)
                                    .setEase(LeanTweenType.easeInQuad)
                                    .setOnComplete(() => {
                                        // Third bounce (smallest)
                                        LeanTween.moveY(obj, originalY + 0.2f, duration * 0.05f)
                                            .setEase(LeanTweenType.easeOutQuad)
                                            .setOnComplete(() => {
                                                LeanTween.moveY(obj, originalY, duration * 0.05f)
                                                    .setEase(LeanTweenType.easeInQuad);
                                            });
                                    });
                            });
                    });
            });
    }

    

    public void onNextClick()
    {

        Crafter crafter = GameObject.FindGameObjectWithTag("Crafter").GetComponent<Crafter>();
        if(crafter.currectRecipeIndex < crafter.recipes.Count - 1)
        {
            crafter.currectRecipeIndex++;
            enableButton(0);
            if (crafter.currectRecipeIndex == crafter.recipes.Count - 1)
            {
                disableButton(2);
            }
            crafter.setText();
        }
        else
        {
            crafter.currectRecipeIndex = 0;
        }

    }

    public void onPreviousClick()
    {
        Crafter crafter = GameObject.FindGameObjectWithTag("Crafter").GetComponent<Crafter>();
        if (crafter.currectRecipeIndex > 0)
        {
            crafter.currectRecipeIndex--;
            enableButton(2);
            if (crafter.currectRecipeIndex == 0)
            {
                disableButton(0);
            }
            crafter.setText();
        }
        else
        {
            crafter.currectRecipeIndex = 0;
        }
    }

    public void disableButton(int index)
    {
        Dialogue.transform.GetChild(0).GetChild(index).GetComponent<Button>().interactable = false;
        Dialogue.transform.GetChild(0).GetChild(index).GetComponent<Image>().color = new Color(0, 0, 0, 0);
    }
    public void enableButton(int index)
    {
        Dialogue.transform.GetChild(0).GetChild(index).GetComponent<Button>().interactable = true;
        Dialogue.transform.GetChild(0).GetChild(index).GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }
}
