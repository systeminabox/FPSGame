using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MusicTrigger2D : MonoBehaviour
{
    public MusicCue musicCue;
    private BoxCollider2D _collider;
    
    
    public void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!musicCue) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Music Triggered");
            AudioManager.Instance.StartQueuedMusic(musicCue);
            Destroy(gameObject);
        }
    }
}
