using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
public class TabButton : MonoBehaviour, IPointerClickHandler

{
    public TabGroup tabGroup;
    // Start is called before the first frame update
    void Start()
    {
        tabGroup.Subscribe(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }


}
