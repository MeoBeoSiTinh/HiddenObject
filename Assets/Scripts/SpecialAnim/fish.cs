using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class fish : MonoBehaviour
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
        Transform group = transform.GetChild(0);
        group.GetChild(0).gameObject.SetActive(true);
        LeanTween.rotateZ(group.gameObject, 4f, 0.3f).setOnComplete(() =>
        {
            LeanTween.rotateZ(group.gameObject, 9f, 0.3f).setOnComplete(() =>
            {
                LeanTween.rotateZ(group.gameObject, 4f, 0.3f).setOnComplete(() =>
                {
                    LeanTween.rotateZ(group.gameObject, 9f, 0.3f).setOnComplete(() =>
                    {
                        LeanTween.rotateZ(group.gameObject, 4f, 0.3f).setOnComplete(() =>
                        {
                            LeanTween.rotateZ(group.gameObject, 9f, 0.3f).setOnComplete(() =>
                            {
                                LeanTween.rotateZ(group.gameObject, -50f, 0.3f).setOnComplete(() =>
                                {
                                    gameObject.GetComponent<Special>().resultObject.GetComponent<TargetFind>().SpecialTargetFound(touchPosition);
                                    gameObject.GetComponent<Special>().resultObject.GetComponent<TargetFind>().CreateSpineAnimation(touchPosition);
                                });
                            });
                        });
                    });
                });
            });
        });
    }
}
