using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftSelect : MonoBehaviour
{
    public GameObject selectedObject;
    public Boolean isSelected = false;
    private GameManager gameManager;
    private GameObject Dialogue;
    private TMP_Text textComponent;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Dialogue = gameManager.Dialogue;
        textComponent = Dialogue.transform.GetChild(0).GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (gameManager.isCrafting)
    //    {
    //        if (selectedObject != null)
    //        {
    //            isSelected = !isSelected;
    //            if (isSelected)
    //            {
    //                gameManager.CraftSelected.Add(selectedObject.name);
    //                Debug.Log("Selected: " + selectedObject.name);
    //            }
    //            else
    //            {
    //                gameManager.CraftSelected.Remove(selectedObject.name);
    //                Debug.Log("Unselected: " + selectedObject.name);

    //            }

    //            string text = Dialogue.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text;

    //        }
    //    }
    //}
    // Add this method to your Crafter class
    //public void SetSpriteTransparency(string spriteName, float alpha)
    //{
    //    if (textComponent == null) return;

    //    textComponent.ForceMeshUpdate(); // Ensure text info is up to date
    //    TMP_TextInfo textInfo = textComponent.textInfo;

    //    bool foundSprite = false;
    //    byte alphaByte = (byte)(Mathf.Clamp01(alpha) * 255);

    //    // We need to scan through all characters to find sprite characters
    //    for (int i = 0; i < textInfo.characterCount; i++)
    //    {
    //        TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

    //        // Skip if not a sprite or not visible
    //        if (!charInfo.isVisible || charInfo.elementType != TMP_TextElementType.Sprite)
    //            continue;

    //        // Get the sprite name from the original text
    //        int spriteTagStart = charInfo.index;
    //        int spriteTagEnd = textComponent.text.IndexOf('>', spriteTagStart);
    //        if (spriteTagEnd == -1) continue;

    //        string spriteTag = textComponent.text.Substring(spriteTagStart, spriteTagEnd - spriteTagStart + 1);

    //        // Check if this is the sprite we want to modify
    //        if (spriteTag.Contains($"name=\"{spriteName}\""))
    //        {
    //            foundSprite = true;
    //            int meshIndex = charInfo.materialReferenceIndex;
    //            int vertexIndex = charInfo.vertexIndex;

    //            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

    //            // Get the current color but modify only the alpha
    //            Color32 currentColor = vertexColors[vertexIndex];
    //            currentColor.a = alphaByte;

    //            // Apply to all 4 vertices of the character quad
    //            vertexColors[vertexIndex + 0] = currentColor;
    //            vertexColors[vertexIndex + 1] = currentColor;
    //            vertexColors[vertexIndex + 2] = currentColor;
    //            vertexColors[vertexIndex + 3] = currentColor;

    //            // If you only want to modify the first matching sprite, break here
    //            break;
    //        }
    //    }

    //    if (foundSprite)
    //    {
    //        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    //    }
    //}
}
