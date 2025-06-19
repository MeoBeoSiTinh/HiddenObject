using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Map1 : MonoBehaviour
{
    GameManager gameManager;
    private List<string> foundTarget;
    FocusCircleController focusCircleController;
    [SerializeField] private GameObject tutBox;
    private GameObject Box;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        focusCircleController = gameManager.FocusCircle.GetComponent<FocusCircleController>();
        StartCoroutine(Tutorial());

    }

    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(2f);
        foundTarget = gameManager.foundTarget;
        if (!foundTarget.Contains("day thung"))
        {
            yield return CameraManager.Instance.MoveCamera(new Vector3(-2.5f, -3.2f, -10f), 7f, 1.5f);
            if (focusCircleController != null)
            {
                yield return focusCircleController.StartFocusing(new Vector2(-2.5f, -3.2f), 0.1f);

            }
            CameraManager.Instance.allowed = false;

            createTutBox("Open the bushes and pick up the hidden object",new Vector3(0, -600, 0));
            yield return CreateAndFadeInPointerDestroyOnClick(new Vector2(-2.5f, -2.7f), 0.5f);
        }
        while (GameObject.FindGameObjectsWithTag("Pointer").Length > 0)
        {
            yield return null; // Wait for the next frame
        }
        yield return new WaitForSeconds(0.5f);
        while (!foundTarget.Contains("day thung"))
        {
            yield return CreateAndFadeInPointer(new Vector2(-2.5f, -2.7f), 0.5f);
            yield return null; // Wait for the next frame
        }
        
        Destroy(GameObject.FindGameObjectWithTag("Pointer"));
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());
        StartCoroutine(focusCircleController.StopFocusing());
        CameraManager.Instance.allowed = true;
        yield return new WaitForSeconds(0.5f);
        createTutBox("Find the remaining objects", new Vector3(0, -1600, 0));
        yield return new WaitForSeconds(5f);
        StartCoroutine(Box.GetComponent<TutorialBox>().popDown());


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
