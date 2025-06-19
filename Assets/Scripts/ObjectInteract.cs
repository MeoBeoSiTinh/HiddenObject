using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInteract : MonoBehaviour
{
    public List<string> ingredients = new List<string>();
    public GameObject result;
    private float textBoxDuration = 0.8f; // Duration in seconds
    private float spinSpeed = 0.8f; // Duration in seconds
    private float fadeDuration = 0.3f; // Duration in seconds

    [SerializeField] public Vector2 SpawnLocation;
    [SerializeField] private Sprite notiImage;
    [SerializeField] private GameObject Tick;

    private GameObject Noti;
    private GameManager gameManager;
    private bool finished = false;
    CameraManager cam;
    void Start()
    {
        gameManager = GameManager.Instance;
        cam = Camera.main.GetComponent<CameraManager>();
        CreateNoti();
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
        StartCoroutine(cam.GetComponent<CameraManager>().MoveCamera(pos, 6f, 1f));
        pos.y += 1;
        WorldSpaceUI worldSpaceUI = spawnedTextBox.AddComponent<WorldSpaceUI>();
        worldSpaceUI.worldOffset = pos;
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
           
            StartCoroutine(HandleResult());
            finished = true;
            Destroy(Noti);

        }
        else
        {

            List<string> missingIngredients = ingredients.Except(foundTarget).ToList();
            ScrollFocus scroll = GameObject.Find("Scroll").GetComponent<ScrollFocus>();
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

                    bool isVisible = scroll.IsIconVisible(icon.GetComponent<RectTransform>());
                    if (!isVisible)
                    {
                        scroll.ScrollToIcon(iconBar.GetComponent<RectTransform>());
                        Debug.Log("not visible");
                    }
                }
            }
            StartCoroutine(WrongAnim());

        }
    }

    private IEnumerator CollapseAndDestroyTextBox(GameObject textBoxObj, float duration)
    {
        LeanTween.scale(textBoxObj, new Vector3(0.1f, 0.1f, 0.1f), 0.4f).setEase(LeanTweenType.easeOutQuint).setOnComplete(() =>
        {
            Destroy(textBoxObj);
        });

        yield return null;
    }

    private IEnumerator HandleResult()
    {
        if (gameObject.GetComponentInChildren<SkeletonAnimation>() != null)
        {
            cam.allowed = false;

            GameObject textBoxObj = GameObject.FindWithTag("TextBox");
            textBoxObj.GetComponent<DestroyOnOutsideClick>().enabled = false;
            yield return spinTextbox(textBoxObj, spinSpeed);

            var skeletonAnim = gameObject.GetComponentInChildren<SkeletonAnimation>();
            yield return cam.MoveCamera(SpawnLocation, 6f, 2f);
            // Add event handler for animation event
            var animPlay = skeletonAnim.AnimationState.SetAnimation(0, "craft", false);
            SoundFXManager.Instance.PlaySoundFXClip(GetComponent<ObjectTouch>().soundFx, transform, 1f);
            yield return new WaitForSpineAnimationComplete(animPlay);
            skeletonAnim.AnimationState.SetAnimation(0, "idle", true);
            GameObject resultObject = Instantiate(result, SpawnLocation, Quaternion.identity);
            resultObject.name = result.name;
            AnimateWithDecreasingBounces(resultObject);
            cam.allowed = true;
        }
        else
        {
            Debug.LogWarning("SkeletonAnimation component not found on the object.");
        }
        yield return null;
    }

    private IEnumerator spinTextbox(GameObject textBoxObj, float duration)
    {
        LeanTween.rotateY(textBoxObj.gameObject, 90f, duration/2)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnComplete(() =>
            {
                // Step 2: Change the content
                for (int i = 0; i < textBoxObj.transform.GetChild(0).childCount - 1; i++)
                {
                    GameObject child = textBoxObj.transform.GetChild(0).GetChild(i).gameObject;
                    if (child.name != "Button")
                    {
                        Destroy(child);
                    }
                }
                textBoxObj.transform.GetChild(1).gameObject.SetActive(false); // Hide the button

                // Step 3: Rotate back to 0 degrees
                LeanTween.rotateY(textBoxObj.gameObject, 0f, duration/2)
                    .setEase(LeanTweenType.easeInOutSine);
            });
        yield return new WaitForSeconds(duration + textBoxDuration);
        yield return CollapseAndDestroyTextBox(textBoxObj, fadeDuration);
        textBoxObj.transform.GetChild(1).gameObject.SetActive(false); // Hide the button
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

    private void CreateNoti()
    {
        GameObject noti = new GameObject(gameObject.name + "(noti)");
        noti.transform.SetParent(GameObject.Find("Canvas").transform);
        RectTransform rectTransform = noti.AddComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, 0, 0); // Adjust position as needed
        Image image = noti.AddComponent<Image>();
        image.sprite = notiImage;
        rectTransform.sizeDelta = new Vector2(notiImage.rect.width, notiImage.rect.height); // Set size as needed
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
        WorldSpaceUI worldSpaceUI = noti.AddComponent<WorldSpaceUI>();
        worldSpaceUI.edgePadding = 40f;
        worldSpaceUI.worldOffset = transform.position + new Vector3(0.3f, 0.7f, 0);
        worldSpaceUI.Init();
        Noti = noti;
    }

}
