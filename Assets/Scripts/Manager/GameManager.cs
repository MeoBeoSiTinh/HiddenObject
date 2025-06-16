using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    public LevelData levelData;
    public int currentLevelIndex;
    private GameObject currentLevelInstance;
    private List<GameObject> targetList;
    public List<GameObject> allTargetsList;
    public List<string> foundTarget;
    public GameObject CameraRenderer;
    public GameObject CurrentTextbox;

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
    public GameObject DescBox;
    public Transform PlayerInfo;
    public GameObject ProgressBar;
    public GameObject FocusCircle;
    public GameObject SpecialFound;


    [Header("PlayerData")]
    public string username = "Meobeo";

    [Header("Boolean")]
    public bool isHotBarMinimized = false;

    [Header("Sound")]
    [SerializeField] public AudioClip BackgroundMusic;
    [SerializeField] private AudioClip starSound;
    [SerializeField] private AudioClip allStarSound;
    [SerializeField] private AudioClip victory;



    private class SaveObject
    {
        public string username;
    }
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

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
        Application.targetFrameRate = 60;

        //GenerateLevelMenu();
        LoadLevel(currentLevelIndex);
        gadgetManager = GameObject.Find("GadgetManager").GetComponent<GadgetManager>();
        CreateEmptyUIElementAtParentRectPosition(toolbarSlotsParent.GetComponent<RectTransform>());
    }

    public void GenerateLevelMenu()
    {
        int i = 1;
        Transform LevelPanel = GameObject.Find("LevelPanel").transform;
        // Clear existing level menu items
        foreach (Transform child in LevelPanel)
        {
            Destroy(child.gameObject);
        }
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
    }

    public void LoadLevel(int levelIndex)
    {
        
        if (levelIndex < 0 || levelIndex + 1 > levelData.data.Count)
        {
            DeleteCurrentLevel();
            ResetMainCameraPosition();
            mainMenuUI.SetActive(true);
            GenerateLevelMenu();
            inGameUi.SetActive(false);
            return;
        }
        mainMenuUI.SetActive(false);
        inGameUi.SetActive(true);

        DeleteCurrentLevel();
        ResetMainCameraPosition();

        MyLevelData levelInfor = levelData.data[levelIndex];

        if (levelInfor == null) return;

        currentLevelInstance = Instantiate(levelInfor.LevelPrefab);
        Bounds newbounds = currentLevelInstance.transform.Find("Background").GetComponent<SpriteRenderer>().bounds;
        Camera.main.GetComponent<CameraManager>().updateBounds(newbounds);


        currentLevelIndex = levelIndex;

        // Populate allTargetsList with all targets in every stage
        allTargetsList = new List<GameObject>();

        //TextMeshProUGUI stageName = GameObject.Find("StageName").GetComponentInChildren<TextMeshProUGUI>();
        //stageName.text = levelInfor.LevelName;

        allTargetsList.AddRange(levelInfor.TargetList);
        targetList = allTargetsList.ToList();

        foundTarget.Clear();
        UpdateHotBar();

        ProgressBar.GetComponent<ProgressBar>().resetProgressBar();
        CameraRenderer.GetComponent<CameraFocus>().ResetMap(currentLevelIndex + 1);
        CameraRenderer.SetActive(true);

        SoundFXManager.Instance.PlayLoopingSoundFXClip(BackgroundMusic, Camera.main.transform, 0.6f);
        //StartCoroutine(gameObject.GetComponent<Beginning>().Begin(currentLevelIndex));
    }


    public void DeleteCurrentLevel()
    {
        if (currentLevelInstance == null) return;
        Destroy(currentLevelInstance);
        currentLevelInstance = null;
        CameraRenderer.SetActive(false);
        SoundFXManager.Instance.StopAllSoundFX();
    }

    public void TargetFound(GameObject target)
    {
        int targetIndex = allTargetsList.FindIndex(x => x.name == target.name);
        if (targetIndex == -1)
        {
            return;
        }
        else
        {
            targetList.RemoveAll(x => x.name == target.name);
            //change Hotbar color
            Transform slot = toolbarSlotsParent.GetChild(targetIndex);
            Image bg = slot.GetComponentInChildren<Image>();


            GameObject tick = new GameObject("Tick");
            tick.transform.SetParent(slot);
            RectTransform tickRectTransform = tick.AddComponent<RectTransform>();
            Image tickImage = tick.AddComponent<Image>();
            tickImage.sprite = Resources.Load<Sprite>("UI/tick");
            tickRectTransform.sizeDelta = new Vector2(61, 50); // Set size of the icon
            tickRectTransform.localPosition = new Vector3(40, -40, 0); // Set position of the icon
            tickRectTransform.localScale = new Vector3(1, 1, 1); // Set scale of the icon

            foundTarget.Add(target.name);
            float progress = (float)foundTarget.Count / allTargetsList.Count * 100;
            StartCoroutine(increaseProgress((int)progress));


            EndgameCheck();
        }
    }
    public void EndgameCheck()
    {
        if (targetList.Count == 0)
        {
            StartCoroutine(EndgameSequence());
        }   
    }

    private IEnumerator EndgameSequence()
    {
        yield return StartCoroutine(gameObject.GetComponent<Endgame>().End(currentLevelIndex));
        allTargetsList.Clear();
        SoundFXManager.Instance.StopAllSoundFX();
        SoundFXManager.Instance.PlaySoundFXClip(victory, transform, 0.6f);
        clearHotBar();
        Confetti.SetActive(true);
        yield return StartCoroutine(ShowLevelCompleteUIWithDelay());
    }


    private IEnumerator increaseProgress(int targetValue)
    {
        ProgressBar progress = ProgressBar.GetComponent<ProgressBar>();

        float duration = 1f; // Duration of the progress increase
        float elapsedTime = 0f;
        int startValue = progress.current;

        // Cache the star transforms to avoid repeated GetChild calls
        Transform star30 = progress.transform.GetChild(1);
        Transform star70 = progress.transform.GetChild(2);
        Transform star100 = progress.transform.GetChild(3);

        // Store original scales
        Vector3 originalScale = star30.localScale;
        float scaleDuration = 0.5f; // Duration of the scale animation

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            progress.current = (int)Mathf.Lerp(startValue, targetValue, t);

            // Check for thresholds
            if (progress.current == 30 || progress.current == 70)
            {
                SoundFXManager.Instance.PlaySoundFXClip(starSound, transform, 1f);
                Transform starToAnimate = progress.current == 30 ? star30 : star70;
                StartCoroutine(ScaleStar(starToAnimate, originalScale, scaleDuration));
            }
            else if (progress.current == 100)
            {
                SoundFXManager.Instance.PlaySoundFXClip(allStarSound, transform, 1f);
                StartCoroutine(ScaleStar(star30, originalScale, scaleDuration));
                StartCoroutine(ScaleStar(star70, originalScale, scaleDuration));
                StartCoroutine(ScaleStar(star100, originalScale, scaleDuration));
            }

            yield return null;
        }

        // Update star sprites
        if (targetValue >= 30)
        {
            star30.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
        if (targetValue >= 70)
        {
            star70.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
        if (targetValue >= 100)
        {
            star100.GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/starFilled");
        }
    }

    private IEnumerator ScaleStar(Transform star, Vector3 originalScale, float duration)
    {
        float elapsedTime = 0f;
        float maxScale = 1.5f; // How much bigger the star should get

        // Scale up
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration / 2);
            star.localScale = Vector3.Lerp(originalScale, originalScale * maxScale, t);
            yield return null;
        }

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < duration / 2)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (duration / 2);
            star.localScale = Vector3.Lerp(originalScale * maxScale, originalScale, t);
            yield return null;
        }

        // Ensure we end at exactly the original scale
        star.localScale = originalScale;
    }


    private IEnumerator ShowLevelCompleteUIWithDelay()
    {
        yield return new WaitForSeconds(2f); // Adjust the delay as needed
        levelCompleteUI.SetActive(true);
    }

    public void UpdateHotBar()
    {
        for (int i = 0; i < allTargetsList.Count; i++)
        {
            GameObject newSlotObject = new GameObject("Icon" + allTargetsList[i].name);
            newSlotObject.transform.SetParent(toolbarSlotsParent);
            RectTransform rectTransform = newSlotObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1, 1); // Set size of the slot
            rectTransform.localScale = new Vector3(1, 1, 1); // Set scale of the slot
            rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, 0); // Set z position to 0
            Image newSlot = newSlotObject.AddComponent<Image>();
            newSlot.sprite = Resources.Load<Sprite>("UI/HotbarslotUI");

            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(newSlotObject.transform);
            RectTransform iconRectTransform = iconObject.AddComponent<RectTransform>();

            Image image = iconObject.AddComponent<Image>();
            CanvasGroup canvasGroup = iconObject.AddComponent<CanvasGroup>();

            image.sprite = allTargetsList[i].GetComponent<SpriteRenderer>().sprite;

            // Scale down if width or height > 80, maintaining aspect ratio
            float defaultSize = 95f;
            float width = image.sprite.rect.width;
            float height = image.sprite.rect.height;
            float scale = 1f;

            scale = defaultSize / Mathf.Max(width, height);
            
            iconRectTransform.sizeDelta = new Vector2(width * scale, height * scale);
            iconRectTransform.anchoredPosition = new Vector2(0, 0); // Set position of the icon
            iconRectTransform.localScale = new Vector3(1f, 1f, 1f); // Set scale of the icon
            iconRectTransform.anchoredPosition3D = new Vector3(iconRectTransform.anchoredPosition3D.x, iconRectTransform.anchoredPosition3D.y, 0); // Set z position to 0

            image.gameObject.SetActive(true);
        }
    }

    public void ResetMainCameraPosition()
    {
        Camera.main.transform.position = new Vector3(0, 0, -10); // Reset to default position
        Camera.main.orthographicSize = 8; // Reset to default size
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
    }

    public void OnContinueClicked()
    {
        levelCompleteUI.SetActive(false);
        Confetti.SetActive(false);
        SoundFXManager.Instance.StopAllSoundFX();
        LoadLevel(currentLevelIndex + 1);
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

    

    public void OnHomeClicked()
    {

    }


    public void OnScanClicked()
    {
        List<GameObject> targets = new List<GameObject>();
        foreach (GameObject target in targetList)
        {
            GameObject targetObject = GameObject.Find(target.name);
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
        foreach (GameObject target in targetList)
        {
            GameObject targetObject = GameObject.Find(target.name);
            if (targetObject != null)
            {
                targets.Add(targetObject);
            }
        }
        gadgetManager.Compass(targets);
    }

    public void OnClaimClicked()
    {
        CurrentTextbox.GetComponent<ObjectInteract>().Interact();
    }
    
    

    
    
    
}
