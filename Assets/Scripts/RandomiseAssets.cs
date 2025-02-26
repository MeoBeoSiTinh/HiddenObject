using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomiseAssets : MonoBehaviour
{
    public string folderPath = "map1/tainguyen/cay"; // Folder inside Resources where PNGs are stored
    public int numberOfObjects = 80; // Number of PNGs to place
    public Vector2 areaSize = new Vector2(20, 40); // Area size for placing PNGs

    void Start()
    {
        PlaceRandomPNGs();
    }

    void PlaceRandomPNGs()
    {
        // Load all PNG files from the specified folder in Resources
        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);

        if (sprites.Length == 0)
        {
            Debug.LogError("No PNG files found in folder: " + folderPath);
            return;
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Random position within the defined area
            Vector3 randomPosition = new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2),
                0 // Z-axis for 2D (adjust for 3D if needed)
            );

            // Randomly select a sprite from the loaded sprites
            Sprite randomSprite = sprites[Random.Range(0, sprites.Length)];

            // Create a new GameObject to display the sprite
            GameObject pngObject = new GameObject("PNGObject " + i);
            pngObject.transform.position = randomPosition;

            // Add a SpriteRenderer component and assign the random sprite
            SpriteRenderer renderer = pngObject.AddComponent<SpriteRenderer>();
            renderer.sprite = randomSprite;
        }
    }
}
