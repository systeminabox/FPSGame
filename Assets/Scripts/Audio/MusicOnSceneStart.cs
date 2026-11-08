
using UnityEngine;

public class MusicOnSceneStart : MonoBehaviour
{
    [SerializeField]MusicCue musicCue;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (musicCue)
        {
            AudioManager.Instance.StartQueuedMusic(musicCue);
        }
    }

   
}
