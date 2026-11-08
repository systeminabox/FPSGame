using UnityEngine;

public class AudioWaitForTrack : MonoBehaviour
{
    AudioSource thisSource;
    [SerializeField] AudioSource otherSource;
    
    void Start()
    {
        thisSource = GetComponent<AudioSource>();
        
        
    }

    void Update()
    {
        if (otherSource.isPlaying) return;
        if (thisSource.isPlaying) return;
        thisSource.Play();
    }
}
