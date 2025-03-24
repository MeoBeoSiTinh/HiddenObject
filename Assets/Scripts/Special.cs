using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Special : MonoBehaviour
{
    public GameObject interactObject;
    public GameObject resultObject;
    public string animRequired;
    GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void specialFound(GameObject target, Vector2 touchPosition)
    {
        if (interactObject.name == target.name)
        {
            if (!string.IsNullOrEmpty(animRequired))
            {
                gameObject.GetComponent(animRequired).GetType().GetMethod("play").Invoke(gameObject.GetComponent(animRequired), new object[] { target, touchPosition });
            }
            else
            {
                resultObject.GetComponent<TargetFind>().SpecialTargetFound(touchPosition);
                resultObject.GetComponent<TargetFind>().CreateSpineAnimation(touchPosition);
            }
        }
        else
        {
            return;
        }
    }
}
