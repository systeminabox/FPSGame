using UnityEngine;


[CreateAssetMenu(fileName = "New Music Cue", menuName = "Audio/Music Cue", order = 0)]
public class MusicCue : ScriptableObject
{
    public AudioClip introClip;
    public AudioClip loopClip;
    public AudioClip outroClip;
}
