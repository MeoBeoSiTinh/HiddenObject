using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map2 : MonoBehaviour
{
    GameManager gameManager;
    private List<string> foundTarget;
    FocusCircleController focusCircleController;
    [SerializeField] private GameObject tutBox;
    private GameObject Box;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        StartCoroutine(Tutorial());
        focusCircleController = gameManager.FocusCircle.GetComponent<FocusCircleController>();
        CameraManager.Instance.transform.position = new Vector3(-12.6f, -8f, -10f);

    }

    private IEnumerator Tutorial()
    {
        yield return CameraManager.Instance.MoveCamera(new Vector3(-12.6f, -8f, -10f), 8f, 0.5f);
        if(focusCircleController != null)
        {
            yield return focusCircleController.StartFocusing(new Vector2(-12.6f, -8f), 0.1f);
        }
        createTutBox("He needs somethings from you.", new Vector3(0, 0.4f, 0));
        yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-12.6f, -7f), 0.5f);
        CameraManager.Instance.allowed = false;
        while (GameObject.FindGameObjectsWithTag("TextBox").Length <= 0)
        {
            yield return null; // Wait for the next frame
        }
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        CameraManager.Instance.allowed = true;
        StartCoroutine(focusCircleController.StopFocusing());
        yield return new WaitForSeconds(0.5f);
        createTutBox("Tap on Claim", new Vector3(0, 0.5f, 0));

        bool claimButtonClicked = false;
        while (!claimButtonClicked)
        {
            GameObject claimButtonObj = GameObject.FindGameObjectWithTag("TextBox"); // Adjust to your button's name
                                                                                     // Alternatively, use: GameObject.FindGameObjectWithTag("ClaimButtonTag") if tagged
            if (claimButtonObj != null)
            {
                Button claimButton = claimButtonObj.GetComponentInChildren<Button>();
                if (claimButton != null)
                {
                    // Add temporary listener
                    UnityEngine.Events.UnityAction onClickAction = () => claimButtonClicked = true;
                    claimButton.onClick.AddListener(onClickAction);

                    // Wait until the button is clicked
                    while (!claimButtonClicked)
                    {
                        yield return null; // Wait for the next frame
                    }

                    // Remove the listener to clean up
                    claimButton.onClick.RemoveListener(onClickAction);
                }
                else
                {
                    Debug.LogWarning("Claim button does not have a Button component!");
                }
            }
            else
            {
                Debug.LogWarning("Claim button not found in the scene!");
            }
        }
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        createTutBox("Find him what he need", new Vector3(0, -0.48f, 0));


        while (!gameManager.foundTarget.Contains("Cần câu") || !gameManager.foundTarget.Contains("Giun đất"))
        {
            yield return null; // Wait for the next frame
        }
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        yield return CameraManager.Instance.MoveCamera(new Vector3(-12.6f, -8f, -10f), 8f, 1f);
        if (focusCircleController != null)
        {
            StartCoroutine(focusCircleController.StartFocusing(new Vector2(-12.6f, -8f), 0.1f));
        }
        createTutBox("Give him what he needs to get the fish", new Vector3(0, 0.4f, 0));
        yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-12.6f, -7f), 0.5f);
        CameraManager.Instance.allowed = false; 
        while (GameObject.FindGameObjectsWithTag("TextBox").Length <= 0)
        {
            yield return null; // Wait for the next frame
        }
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        StartCoroutine(focusCircleController.StopFocusing());
        while (!GameObject.Find("Cá sống"))
        {
            yield return null; // Wait for the next frame
        }
        yield return new WaitForSeconds(1.5f);

        CameraManager.Instance.allowed = true;
    }

    private IEnumerator CreateAndFadeInPointer(Vector2 position, float fadeDuration)
    {
        if (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield break;
        }
        GameObject pointerPrefab = Resources.Load<GameObject>("Pointer/Pointer");
        if (pointerPrefab != null)
        {
            GameObject pointer = Instantiate(pointerPrefab, position, pointerPrefab.transform.rotation);
            SpriteRenderer sr = pointer.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) yield break;

            Color transparent = sr.color;
            transparent.a = 0f;
            Color opaque = sr.color;

            float progress = 0f;
            while (progress < 1f)
            {
                if (sr == null) yield break;
                progress += Time.deltaTime / fadeDuration;
                sr.color = Color.Lerp(transparent, opaque, progress);
                yield return null;
            }
        }
    }

    private IEnumerator CreateAndFadeInPointerDestroyOnClick(Vector2 position, float fadeDuration)
    {
        if (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield break;
        }
        GameObject pointerPrefab = Resources.Load<GameObject>("Pointer/Pointer");
        if (pointerPrefab != null)
        {
            GameObject pointer = Instantiate(pointerPrefab, position, pointerPrefab.transform.rotation);
            pointer.AddComponent<DestroyOnAnyNonUIClick>();
            SpriteRenderer sr = pointer.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) yield break;

            Color transparent = sr.color;
            transparent.a = 0f;
            Color opaque = sr.color;

            float progress = 0f;
            while (progress < 1f)
            {
                if (sr == null) yield break;
                progress += Time.deltaTime / fadeDuration;
                sr.color = Color.Lerp(transparent, opaque, progress);
                yield return null;
            }
        }
    }
    private void createTutBox(string text, Vector3 position)
    {
        GameObject tut = Instantiate(tutBox);
        tut.transform.SetParent(GameObject.Find("Canvas").transform, false);
        tut.GetComponent<TutorialBox>().ChangeText(text);
        Box = tut;
        StartCoroutine(Box.GetComponent<TutorialBox>().popUp(position));
    }


}
