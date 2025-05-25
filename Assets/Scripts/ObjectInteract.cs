using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInteract : MonoBehaviour
{
    public List<string> ingredients = new List<string>();
    public GameObject result;
    public GameObject textBox;
    private float textBoxDuration = 3.5f; // Duration in seconds
    [SerializeField] public Vector2 SpawnLocation;
    private GameManager gameManager;
    private bool isInteractable = true;
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void Interact()
    {
        if (!isInteractable)
        {
            return;
        }
        bool Interactable = false;
        List<string> foundTarget = gameManager.foundTarget;
        Interactable = !ingredients.Except(foundTarget).Any();

        if (Interactable)
        {
            Vector3 targetPosition = SpawnLocation;
            targetPosition.z = -10f;
            StartCoroutine(MoveCamera(targetPosition));
            StartCoroutine(HandleResult());
            isInteractable = false;
            SoundFXManager.Instance.PlaySoundFXClip(GetComponent<ObjectTouch>().soundFx, transform, 1f);

        }
        else
        {
            // Destroy any previous text box before creating a new one
            GameObject existingTextBox = GameObject.FindWithTag("TextBox");
            if (existingTextBox != null)
            {
                StopAllCoroutines();
                Destroy(existingTextBox);
            }

            string text = "";
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (i > 0)
                {
                    text += " ";
                }
                text += $"<sprite name=\"{ingredients[i]}\"> ";
            }
            Vector3 pos = transform.position;
            pos.y += 1.8f;

            GameObject spawnedTextBox = Instantiate(textBox, pos, Quaternion.identity);
            spawnedTextBox.transform.localScale = textBox.transform.localScale * 0.1f;
            LeanTween.scale(spawnedTextBox, textBox.transform.localScale, 0.4f).setEase(LeanTweenType.easeOutBack);
            spawnedTextBox.tag = "TextBox"; // Ensure the prefab or instance has this tag
            TextMeshProUGUI textMeshPro = spawnedTextBox.GetComponentInChildren<TextMeshProUGUI>();
            textMeshPro.text = text;

            List<string> missingIngredients = ingredients.Except(foundTarget).ToList();
            ScrollRectFocus scroll = GameObject.Find("Scroll").GetComponent<ScrollRectFocus>();
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
            StartCoroutine(WrongAnim());

            StartCoroutine(FadeAndDestroyTextBox(spawnedTextBox, textBoxDuration));
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
        float distance = 1.5f;
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

            float duration = 0.5f; // Duration for each transition
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

    private IEnumerator MoveCamera(Vector3 targetPosition)
    {
        float duration = 0.5f; // Duration of the camera movement in seconds
        float elapsedTime = 0f;
        Vector3 startPosition = Camera.main.transform.position;
        float startSize = Camera.main.orthographicSize;

        // Disable CameraHandle script
        CameraHandle cameraHandle = Camera.main.GetComponent<CameraHandle>();

        cameraHandle.enabled = false;
        // Smoothly move the camera to the target position and zoom out
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, 5f, t);
            yield return null; // Wait for the next frame
        }

        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = 5f;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }

}
