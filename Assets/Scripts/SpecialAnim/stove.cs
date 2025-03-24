using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class stove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void play(GameObject target, Vector2 touchPosition)
    {
        Transform smoke = transform.GetChild(0);
        smoke.gameObject.SetActive(true);
        LeanTween.scale(smoke.gameObject, new Vector3(1.5f, 1.5f, 1.5f), 1f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            smoke.gameObject.SetActive(false);
            gameObject.GetComponent<Special>().resultObject.GetComponent<TargetFind>().SpecialTargetFound(touchPosition);
            gameObject.GetComponent<Special>().resultObject.GetComponent<TargetFind>().CreateSpineAnimation(touchPosition);
        });
    }
}
