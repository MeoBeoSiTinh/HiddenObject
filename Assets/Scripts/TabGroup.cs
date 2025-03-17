using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public List<GameObject> objectsToSwap;
    public List<GameObject> iconToSwap;


    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(button);
    }
     
    public void OnTabSelected(TabButton button)
    {
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if (i == index)
            {
                iconToSwap[i].SetActive(true);
                objectsToSwap[i].SetActive(true);
            }
            else
            {
                iconToSwap[i].SetActive(false);
                objectsToSwap[i].SetActive(false);
            }
        }
    }

}
