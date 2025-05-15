using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

    public int maximum;
    public int current;
    public Image mask;
    public Image fill;
    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        getCurrentFill();
        
    }

    void getCurrentFill()
    {
        float fillAmount = (float)current / (float)maximum;
        mask.fillAmount = fillAmount;
        fill.color = color;
    }

    public void resetProgressBar()
    {
        current = 0;
        mask.fillAmount = 0;
        for(int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<Image>().sprite = Resources.Load<Sprite>("UI/star");
        }
    }
}
