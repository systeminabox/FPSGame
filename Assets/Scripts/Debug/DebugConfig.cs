using UnityEngine;

[CreateAssetMenu(fileName = "DebugConfig", menuName = "Debug/Debug Config", order = 1)]
public class DebugConfig : ScriptableObject
{
    public int maximumCachedLogMessages = 1000;
}