using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Special : MonoBehaviour
{
    public GameObject interactObject;
    public GameObject resultObject;
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
            resultObject.GetComponent<TargetFind>().CreateTargetImage(touchPosition);
            resultObject.GetComponent<TargetFind>().CreateSpineAnimation(touchPosition);
            gameManager.TargetFound(target);
        }
        else
            return;
    }
}
