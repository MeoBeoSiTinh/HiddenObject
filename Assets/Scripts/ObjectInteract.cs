using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInteract : MonoBehaviour
{
    public List<string> ingredients = new List<string>();
    public GameObject result;
    private float textBoxDuration = 3.5f; // Duration in seconds
    [SerializeField] public Vector2 SpawnLocation;
    [SerializeField] public GameObject Noti;
    [SerializeField] private GameObject Tick;

    private GameManager gameManager;
    private bool finished = false;
    Camera mainCamera;
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        mainCamera = Camera.main;
    }

    public void CreateTextBox()
    {
        if (finished)
        {
            return;
        }
        gameManager.CurrentTextbox = gameObject;
        List<string> foundTarget = gameManager.foundTarget;
        // Destroy any previous text box before creating a new one
        GameObject existingTextBox = GameObject.FindWithTag("TextBox");
        if (existingTextBox != null)
        {
            Destroy(existingTextBox);
        }


        GameObject spawnedTextBox = Instantiate(Resources.Load<GameObject>("UI/DescBox/Box"));
        spawnedTextBox.transform.SetParent(GameObject.Find("Canvas").transform);
          
        Vector3 pos = transform.position;
        StartCoroutine(MoveCamera(pos, 6f));
        pos.y += 1.8f;
        spawnedTextBox.GetComponent<WorldSpaceUI>().worldOffset = pos;
        spawnedTextBox.GetComponent<WorldSpaceUI>().Init();
        spawnedTextBox.AddComponent<DestroyOnOutsideClick>();

        if (spawnedTextBox != null)
        {
            for (int i = 0; i < spawnedTextBox.transform.GetChild(0).childCount; i++)
            {
                GameObject child = spawnedTextBox.transform.GetChild(0).GetChild(i).gameObject;
                if (child.name != "Button")
                {
                    Destroy(child);
                }
            }
        }
        Button Button = spawnedTextBox.transform.Find("Button").GetComponent<Button>();
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(() => Interact());

        for (int i = 0; i < ingredients.Count; i++)
        {
            
            GameObject ingredient = gameManager.allTargetsList.Find(x => x.name == ingredients[i]);

            //Background
            GameObject background = new GameObject(ingredient.name + "(recipe)");
            background.transform.SetParent(spawnedTextBox.transform.GetChild(0));
            RectTransform rectTransform = background.AddComponent<RectTransform>();
            rectTransform.transform.localPosition = new Vector3(0, 0, 0);
            rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            Image image = background.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("UI/DescBox/Rectangle 32");


            //Icon
            GameObject icon = new GameObject(ingredient.name + "(recipe Icon)");
            icon.transform.SetParent(background.transform);
            RectTransform iconRect = icon.AddComponent<RectTransform>();
            iconRect.localScale = new Vector3(1f, 1f, 1f);
            iconRect.transform.localPosition = new Vector3(0, 0, 0);

            Image iconImage = icon.AddComponent<Image>();
            iconImage.sprite = ingredient.GetComponent<SpriteRenderer>().sprite;

            float defaultSize = 100f;
            float width = iconImage.sprite.rect.width;
            float height = iconImage.sprite.rect.height;
            float scale = 1f;

            scale = defaultSize / Mathf.Max(width, height);

            iconRect.sizeDelta = new Vector2(width * scale, height * scale);

            //tick
            if(foundTarget.Contains(ingredients[i]))
            {
                GameObject tick = Instantiate(Tick);
                tick.transform.SetParent(background.transform);
                tick.transform.localPosition = new Vector2(55f, -55f);
                tick.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            }

            //plus
            if (i < ingredients.Count -1)
            {
                GameObject plus = Instantiate(Resources.Load<GameObject>("UI/DescBox/plus"));
                plus.transform.SetParent(background.transform);
                plus.transform.localPosition = new Vector2(105f, 0f);
                plus.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            }

            if (i == ingredients.Count - 1)
            {
                GameObject equal = Instantiate(Resources.Load<GameObject>("UI/DescBox/equal"));
                equal.transform.SetParent(background.transform);
                equal.transform.localPosition = new Vector2(105f, 0f);
                equal.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

                //Background Result
                GameObject backgroundResult = new GameObject(result.name + "(recipe)");
                backgroundResult.transform.SetParent(spawnedTextBox.transform.GetChild(0));
                RectTransform rectTransformResult = backgroundResult.AddComponent<RectTransform>();
                rectTransformResult.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                Image imageResult = backgroundResult.AddComponent<Image>();
                imageResult.sprite = Resources.Load<Sprite>("UI/DescBox/Rectangle 32");
                imageResult.color = new Color(0.121f, 0.816f, 0.867f); // #1FD0DD in normalized RGB

                //Icon
                GameObject iconResult = new GameObject(result.name + "(recipe Icon)");
                iconResult.transform.SetParent(backgroundResult.transform);
                RectTransform iconRectResult = iconResult.AddComponent<RectTransform>();
                iconRectResult.localScale = new Vector3(1f, 1f, 1f);
                Image iconImageResult = iconResult.AddComponent<Image>();
                iconImageResult.sprite = result.GetComponent<SpriteRenderer>().sprite;

                float widthResult = iconImageResult.sprite.rect.width;
                float heightResult = iconImageResult.sprite.rect.height;

                scale = defaultSize / Mathf.Max(widthResult, heightResult);

                iconRectResult.sizeDelta = new Vector2(widthResult * scale, heightResult * scale);
            }

            spawnedTextBox.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Start with a small scale
            LeanTween.scale(spawnedTextBox, new Vector3(1f, 1f, 1f), 0.4f).setEase(LeanTweenType.easeOutBack);
            spawnedTextBox.tag = "TextBox"; // Ensure the prefab or instance has this tag
        }
    }


    public void Interact()
    {
        Debug.Log("Interact called on " + gameObject.name); 
        if (finished)
        {
            return;
        }
        bool Interactable = false;
        List<string> foundTarget = gameManager.foundTarget;
        Interactable = !ingredients.Except(foundTarget).Any();

        if (Interactable)
        {
            GameObject textBoxObj = GameObject.FindWithTag("TextBox");
            Destroy(textBoxObj);
            Vector3 targetPosition = SpawnLocation;
            targetPosition.z = -10f;
            StartCoroutine(MoveCamera(targetPosition, 6f));
            StartCoroutine(HandleResult());
            finished = true;
            SoundFXManager.Instance.PlaySoundFXClip(GetComponent<ObjectTouch>().soundFx, transform, 1f);
            Noti.SetActive(false);

        }
        else
        {

            List<string> missingIngredients = ingredients.Except(foundTarget).ToList();
            ScrollRectFocus scroll = GameObject.Find("Scroll").GetComponent<ScrollRectFocus>();
            foreach (string name in missingIngredients)
            {
                GameObject icon = GameObject.Find(name + "(recipe)");
                if (icon != null)
                {
                    StartCoroutine(FlashRed(icon));
                }

                GameObject iconBar = GameObject.Find("Icon" + name);
                if (iconBar != null)
                {
                    StartCoroutine(FlashRed(iconBar));

                    bool isVisible = scroll.IsElementVisible(icon.GetComponent<RectTransform>());
                    if (!isVisible)
                    {
                        scroll.ScrollToView(iconBar.GetComponent<RectTransform>());
                        Debug.Log("not visible");
                    }
                }
            }
            StartCoroutine(WrongAnim());

        }
    }

    private IEnumerator FadeAndDestroyTextBox(GameObject textBoxObj, float duration)
    {
        CanvasGroup canvasGroup = textBoxObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textBoxObj.AddComponent<CanvasGroup>();
        }
        yield return new WaitForSeconds(duration);
        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }
        Destroy(textBoxObj);
    }

    private IEnumerator HandleResult()
    {
        if (gameObject.GetComponentInChildren<SkeletonAnimation>() != null)
        {
            CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

            // Set cameraHandle backgroundBounds to area around the camera only
            float camHeight = 8f * 1.5f;
            float camWidth = camHeight * Camera.main.aspect;

            // Define bounds centered on camera, with width and height matching the camera's view
            cameraHandle.backgroundBounds = new Bounds(
                new Vector3(SpawnLocation.x, SpawnLocation.y, 0),
                new Vector3(camWidth, camHeight, 1) // Set z size to 1 to avoid 0-size bounds
            );
            cameraHandle.bounceIntensity = 0.1f; // Set bounce intensity to a small value

            var skeletonAnim = gameObject.GetComponentInChildren<SkeletonAnimation>();
            bool resultSpawned = false;

            // Add event handler for animation event
            Spine.TrackEntry animPlay = skeletonAnim.AnimationState.SetAnimation(0, "craft", false);
            animPlay.Event += (entry, e) =>
            {
                if (!resultSpawned && e.Data.Name == "craft")
                {
                    GameObject resultObject = Instantiate(result, SpawnLocation, Quaternion.identity);
                    resultObject.name = result.name;
                    AnimateWithDecreasingBounces(resultObject);
                    resultSpawned = true;
                }
            };

            yield return new WaitForSpineAnimationComplete(animPlay);
            cameraHandle.backgroundBounds = GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().bounds;
            cameraHandle.bounceIntensity = 0.4f; // Reset bounce intensity to a higher value
            skeletonAnim.AnimationState.SetAnimation(0, "idle", true);
        }
        yield return null;
    }


    void AnimateWithDecreasingBounces(GameObject obj)
    {
        float duration = 1.5f;
        float distance = 1.2f;
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

    private IEnumerator FlashRed(GameObject icon)
    {
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null)
        {
            Color originalColor = new Color(1, 1, 1);
            Color flashColor = new Color(0.984f, 0.431f, 0.431f); // Equivalent to "FB6E6E" in RGB (normalized to 0-1 range)  

            float duration = 0.25f; // Duration for each transition
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

    private IEnumerator WrongAnim()
    {

        if (gameObject.GetComponentInChildren<SkeletonAnimation>() != null)
        {
            AudioClip no = Resources.Load<AudioClip>("Map1 Test/Sound/No");
            var animPlay = gameObject.GetComponentInChildren<SkeletonAnimation>().AnimationState.SetAnimation(0, "wrong", false);
            SoundFXManager.Instance.PlaySoundFXClip(no, transform, 1f);
            yield return new WaitForSpineAnimationComplete(animPlay);
            gameObject.GetComponentInChildren<SkeletonAnimation>().AnimationState.SetAnimation(0, "idle", true);
        }
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, float targetSize)
    {
        float duration = 0.4f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;
        targetPosition.z = -10f;
        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
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

}
