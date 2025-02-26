using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    // Start is called before the first frame update
    public List<MyLevelData> data;
}

[System.Serializable]
public class MyLevelData
{
    public string LevelName;
    public List<MyTarget> target;
    public GameObject LevelPrefab;
}

[System.Serializable]
public class MyTarget
{
    public string TargetName;
    public GameObject TargetPrefab;
}
