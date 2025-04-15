using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTreeToChildren : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Tree>() == null)
            {
                child.gameObject.AddComponent<Tree>();
            }
        }
    }
}
