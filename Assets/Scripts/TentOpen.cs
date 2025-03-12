using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentOpen : MonoBehaviour
{
    private Camera mainCamera;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

        // Check if there is a touch input
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // Check if the touch phase is a tap (began and ended without moving)
            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1 && touch.deltaPosition.magnitude < 10f)
            {
                Vector2 touchPosition = mainCamera.ScreenToWorldPoint(touch.position);
                RaycastHit2D[] hits = Physics2D.RaycastAll(touchPosition, Vector2.zero);

                if (hits.Length > 0)
                {
                    // Sort hits by sorting layer and sorting order
                    System.Array.Sort(hits, (a, b) =>
                    {
                        SpriteRenderer rendererA = a.collider.GetComponent<SpriteRenderer>();
                        SpriteRenderer rendererB = b.collider.GetComponent<SpriteRenderer>();

                        if (rendererA != null && rendererB != null)
                        {
                            // Compare sorting layers
                            int layerComparison = SortingLayer.GetLayerValueFromID(rendererA.sortingLayerID)
                                .CompareTo(SortingLayer.GetLayerValueFromID(rendererB.sortingLayerID));

                            if (layerComparison != 0)
                                return layerComparison;

                            // Compare sorting orders within the same layer
                            return rendererB.sortingOrder.CompareTo(rendererA.sortingOrder);
                        }

                        // If no SpriteRenderer, fall back to distance
                        return a.distance.CompareTo(b.distance);
                    });

                    // Get the topmost hit
                    RaycastHit2D topmostHit = hits[0];

                    // Check if the hit object is the one this script is attached to
                    if (topmostHit.transform == transform)
                    {
                        StartCoroutine(DestroyAfterDelay(0.05f));
                    }
                }
            }
        }
    }
    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
