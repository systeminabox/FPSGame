using UnityEngine;

[CreateAssetMenu(fileName = "New Scene Data", menuName = "Scene Data")]
public class SceneDataSO : ScriptableObject
{
    public string UniqueName;
    public int SceneIndex;
}
