using System;
using UnityEngine;
using UnityEngine.Audio;


public class AudioDeck : MonoBehaviour
{
    public AudioSource introSource;
    public AudioSource loopSource;
    public AudioMixerGroup mixerGroup;

    
    public MusicCue currentCue;
    
    private float level;            // fade position, 0..1
    private float target;           // where level is heading, 0 or 1
    private float fadeSpeed = 1f;   // level units per second

    public bool IsActive => currentCue != null;

    public void Init(AudioMixerGroup group)
    {
        introSource = CreateSource("Intro", group);
        loopSource = CreateSource("Loop", group);
    }

    private AudioSource CreateSource(string sourceName, AudioMixerGroup group)
    {
        var go = new GameObject(sourceName);
        go.transform.SetParent(transform);

        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.outputAudioMixerGroup = group;
        source.volume = 0f;
        return source;
    }
    
    public void Play(MusicCue cue, float fadeTime)
    {
        if (cue == null || cue.loopClip == null) return;

        // Clear any old playback, including a pending scheduled loop
        introSource.Stop();
        loopSource.Stop();

        currentCue = cue;

        // Start silent if fading in, full if there's no fade
        level = fadeTime > 0f ? 0f : 1f;
        target = 1f;
        fadeSpeed = fadeTime > 0f ? 1f / fadeTime : 1f;
        ApplyVolume(); // set the volume BEFORE the sources start playing

        loopSource.clip = cue.loopClip;
        loopSource.loop = true;

        double start = AudioSettings.dspTime + 0.05;

        if (cue.introClip != null)
        {
            double introLength = (double)cue.introClip.samples / cue.introClip.frequency;

            introSource.clip = cue.introClip;
            introSource.loop = false;
            introSource.PlayScheduled(start);
            loopSource.PlayScheduled(start + introLength);
        }
        else
        {
            loopSource.PlayScheduled(start);
        }
    }

    public void StartQueuedMusic(MusicCue cue)
    {
        if(cue.loopClip == null) return;
        if (cue.introClip == null)
        {
            loopSource.clip = cue.loopClip;
            loopSource.loop = true;
            loopSource.Play();
            return;
        }
        
        var introLength = (double)cue.introClip.samples / (double)cue.introClip.frequency;
        
        var now = AudioSettings.dspTime + 0.05;
        
        currentCue = cue;
        introSource.clip = cue.introClip;
        introSource.loop = false;
        
        introSource.PlayScheduled(now);
      
        loopSource.clip = cue.loopClip;
        loopSource.loop = true;
        loopSource.PlayScheduled(now + introLength);
    }

    public void FadeOut(float fadeTime)
    {
        if (!IsActive) return;

        target = 0f;
        if (fadeTime > 0f) fadeSpeed = 1f / fadeTime;
        else level = 0f; // Update will stop it next frame
    }

    private void Update()
    {
        if (!IsActive) return;

        level = Mathf.MoveTowards(level, target, fadeSpeed * Time.unscaledDeltaTime);
        ApplyVolume();

        if (target <= 0f && level <= 0f)
            Stop();
    }

    private void ApplyVolume()
    {
        // level moves linearly; the sine curve makes the crossfade equal-power
        float v = Mathf.Sin(level * Mathf.PI * 0.5f);
        introSource.volume = v;
        loopSource.volume = v;
    }

    private void Stop()
    {
        introSource.Stop();
        loopSource.Stop();
        introSource.clip = null;
        loopSource.clip = null;
        currentCue = null;
    }
    
    
}
