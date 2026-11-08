using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue Asset", menuName = "Audio/Dialogue", order = 0)]
public class DialogueAsset : ScriptableObject
{
    public string line;
    public string speakerName;
    public AudioClip audioClip;
    public DialogueAsset nextLine;
}
