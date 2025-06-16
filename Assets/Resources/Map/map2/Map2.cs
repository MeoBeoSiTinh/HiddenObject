using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    }

    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(2f);
        yield return CameraManager.Instance.MoveCamera(new Vector3(-12.6f, -8f, -10f), 8f, 2f);
        if(focusCircleController != null)
        {
            yield return focusCircleController.StartFocusing(new Vector2(-12.6f, -8f), 0.1f);
        }
        createTutBox("He needs somethings from you.");
        yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-12.6f, -7f), 0.5f);
        CameraManager.Instance.allowed = false;
        while (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield return null; // Wait for the next frame
        }
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        CameraManager.Instance.allowed = true;
        StartCoroutine(focusCircleController.StopFocusing());
        yield return new WaitForSeconds(0.5f);
        createTutBox("Find him what he needs");
        yield return new WaitForSeconds(3f);
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());

        while (!gameManager.foundTarget.Contains("Cần câu") || !gameManager.foundTarget.Contains("Giun đất"))
        {
            yield return null; // Wait for the next frame
        }
        yield return CameraManager.Instance.MoveCamera(new Vector3(-12.6f, -8f, -10f), 8f, 2f);
        if (focusCircleController != null)
        {
            yield return focusCircleController.StartFocusing(new Vector2(-12.6f, -8f), 0.1f);
        }
        createTutBox("Give him what he needs to get the fish");
        yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-12.6f, -7f), 0.5f);
        CameraManager.Instance.allowed = false;
        while (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
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
    private void createTutBox(string text)
    {
        GameObject tut = Instantiate(tutBox);
        tut.transform.SetParent(GameObject.Find("Canvas").transform, false);
        tut.GetComponent<TutorialBox>().ChangeText(text);
        Box = tut;
        StartCoroutine(Box.GetComponent<TutorialBox>().popUp());
    }


}
