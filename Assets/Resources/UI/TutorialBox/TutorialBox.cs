using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialBox : MonoBehaviour
{
    // Start is called before the first frame update

    public void ChangeText(string text)
    {
        TextMeshProUGUI TextUI = GetComponentInChildren<TextMeshProUGUI>();
        TextUI.text = text;
    }

    public IEnumerator popUp()
    {
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -350, 0);
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);
        yield return null;
        LeanTween.scale(gameObject.GetComponent<RectTransform>(), new Vector3(1f, 1f, 1f), 1f).setEaseOutBounce();
        yield return null;
    }

    public IEnumerator popDown() 
    {
        LeanTween.scale(gameObject, new Vector3(0.1f, 0.1f, 0.1f), 0.4f).setEase(LeanTweenType.easeOutQuint).setOnComplete(() =>
        {
            Destroy(gameObject);
        });

        yield return null;
    }
}
