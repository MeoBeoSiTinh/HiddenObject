using System.Collections;
using System.Collections.Generic;
using System.Net.Cache;
using TMPro;
using UnityEngine;

public class Crafter : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject CrafterMenu;
    private Camera cam;
    public List<craftRecipe> recipes;
    public int currectRecipeIndex = 0;
    public float smoothTime = 0.3f;
    public GameObject Exclamation;

    public void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        CrafterMenu = gameManager.Dialogue;
        cam = Camera.main;
        Exclamation = gameObject.transform.GetChild(0).gameObject;
        if (recipes.Count > 0)
        {
            Exclamation.SetActive(true);
        }
        else
        {
            Exclamation.SetActive(false);
        }
    }
    public void ShowRecipe()
    {
        if(recipes.Count == 0)
        {
            return;
        }
        Vector3 targetPosition = gameObject.transform.position;
        targetPosition.z = cam.transform.position.z;
        targetPosition.x = targetPosition.x + 0.5f;
        StartCoroutine(MoveCamera(targetPosition));
        gameManager.enableButton(0);
        gameManager.enableButton(2);
        if (currectRecipeIndex == 0)
        {
            gameManager.disableButton(0);
        }
        if (recipes.Count <= 1)
        {
            gameManager.disableButton(2);
        }
    }



    private IEnumerator MoveCamera(Vector3 targetPosition)
    {
        setText();
        gameManager.OpenDialogue();
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
            Camera.main.orthographicSize = Mathf.Lerp(startSize, 4f, t);
            yield return null; // Wait for the next frame
        }
        
        // Ensure the camera reaches the exact target position and zoom level
        Camera.main.transform.position = targetPosition;
        Camera.main.orthographicSize = 4f;

        // Re-enable CameraHandle script
        cameraHandle.enabled = true;
    }
    public void setText()
    {
        string text = "";
        craftRecipe recipe = recipes[currectRecipeIndex];
        {
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                if (i > 0)
                {
                    text += "+ ";
                }
                text += $"<sprite name=\"{recipe.ingredients[i]}\"> ";
            }
            text += $"= <sprite name=\"{recipe.RecipeName}\">\n";
        }
        CrafterMenu.transform.GetChild(0).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
    }

    public void SetResultText()
    {
        if (recipes.Count == 0)
        {
            return;
        }

        craftRecipe recipe = recipes[currectRecipeIndex];
        string resultText = $"<sprite name=\"{recipe.RecipeName}\">";

        CrafterMenu.transform.GetChild(0).GetComponentInChildren<TMPro.TextMeshProUGUI>().text = resultText;
    }


}
