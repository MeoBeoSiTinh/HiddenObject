using UnityEngine;
using UnityEngine.EventSystems;

public class DestroyOnAnyNonUIClick : MonoBehaviour
{
    [Tooltip("Time delay before destruction (seconds)")]
    public float destroyDelay = 0f;

    private void Update()
    {
        // Skip if clicking on UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Touch input only, not hold or drag
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && touch.tapCount == 1)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}