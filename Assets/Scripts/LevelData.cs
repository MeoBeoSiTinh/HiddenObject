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
    public List<MyStage> stage;
    public GameObject LevelPrefab;
    public List<SpecialGroup> specialGroup;

}

[System.Serializable]
public class MyTarget
{
    public string TargetName;
    public GameObject TargetPrefab;
    public string Description;
}

[System.Serializable]
public class MyStage
{
    public string StageName;
    public List<MyTarget> target;
}

[System.Serializable]
public class SpecialGroup
{
    public List<MyTarget> special;
}
