using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;


   public enum AudioChannel { Music, SFX , Dialogue, Master}

   public struct SfxVoice
   {
      public AudioSource Source;
      public float StartTime;

      public SfxVoice(AudioSource source)
      {
         this.Source = source;
         StartTime = Time.time;
      }
   
   }

   public class AudioManager : Singleton<AudioManager>
   {
      [SerializeField] private AudioMixer audioMixer;
      [SerializeField] private AudioSource musicIntroSource;
      [SerializeField] private AudioSource musicLoopSource;
   
      private readonly List<SfxVoice> sfxVoices = new List<SfxVoice>();
      [SerializeField] private AudioMixerGroup sfxGroup;
      [SerializeField] private AudioMixerGroup musicGroup;
      [SerializeField] int sfxPoolSize = 10;
   
      [SerializeField] private AudioSource dialogueSource;

      [SerializeField] private GameObject captionPanel;
      [SerializeField] private TextMeshProUGUI captionText;

      public List<AudioClip> clips;
      public MusicCue musicCues;

      private List<AudioDeck> audioDecks = new List<AudioDeck>();
      [Range(2, 5)]
      [SerializeField] private int maxAudioDecks = 2;
      private AudioDeck activeDeck = null;
      
      private Coroutine captionCoroutine;
   
      public void PlayMusic(AudioClip clip)
      {
         musicIntroSource.clip = clip;
         musicIntroSource.Play();
      }

      public List<SfxVoice> GetSfxSource()
      {
         return sfxVoices;
      }

      public AudioSource GetMusicSource()
      {
         return musicLoopSource;
      }
      
      public void StopMusic()
      {
         musicIntroSource.Stop();
         musicLoopSource.Stop();

         foreach (var deck in audioDecks)
         {
            deck.introSource.Stop();
            deck.loopSource.Stop();
         }
      }
      
      public void PlaySfx(AudioClip clip)
      {
         if (clip == null) return;

         SfxVoice voice = new SfxVoice(null);

         // Use a free voice if there is one
         foreach (var v in sfxVoices)
         {
            if (!v.Source.isPlaying)
            {
               voice = v;
               break;
            }
         }

         // Otherwise steal the oldest
         if (voice.Source == null)
         {
            voice = sfxVoices[0];
            foreach (var v in sfxVoices)
            {
               if (v.StartTime < voice.StartTime)
                  voice = v;
            }
         }

         voice.Source.clip = clip;
         voice.Source.Play();
         voice.StartTime = Time.unscaledTime;
      }
   
      protected override void Awake()
      {
         base.Awake();
      
         var sfxChild = new GameObject("SFX");
         sfxChild.transform.SetParent(transform);

         if (musicGroup == null)
         {
            Debug.LogError("No AudioMixerGroup assigned to AudioManager for music");
         }
      
         for(int i = 0; i < sfxPoolSize; i++)
         {
            var source = sfxChild.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = sfxGroup;
            sfxVoices.Add(new SfxVoice { Source = source });
         }
         CreateAudioDecks();
      
      
      }

      public void PlayDialogue(DialogueAsset dialogue)
      {
         dialogueSource.clip = dialogue.audioClip;
         
         
         
         
         if (captionCoroutine != null) StopCoroutine(captionCoroutine);
         captionCoroutine = StartCoroutine(ShowCaption(dialogue));
         
         
         dialogueSource.Play();
         
         
      }

      IEnumerator ShowCaption(DialogueAsset dialogue)
      {
         
         double duration = (double)dialogue.audioClip.samples / (double) dialogue.audioClip.frequency;
         
         captionPanel.SetActive(true);
         captionText.text = "[" + dialogue.speakerName + "] " + dialogue.line;
         yield return new WaitForSeconds((float)duration);
         if (dialogue.nextLine != null)
         {
            PlayDialogue(dialogue.nextLine);
            
         }
         else
         {
            captionPanel.SetActive(false);
         }
         
         
      }
      
      private void Start()
      {
      
         LoadVolume();
      
      
         if (musicCues)
         {
            StartQueuedMusic(musicCues);
         }
      
      
      }

      private void CreateAudioDecks()
      {
         for(int i = 0; i < maxAudioDecks; i++)
         {
            var newAudioDeck = new GameObject("AudioDeck " + i).AddComponent<AudioDeck>();
            newAudioDeck.Init(musicGroup);
            newAudioDeck.transform.SetParent(transform);
            audioDecks.Add(newAudioDeck);
         }
      }

      public void StartQueuedMusic(MusicCue cue, float fadeInTime = 0.5f, float fadeOutTime = 0.5f) 
      {
         if (activeDeck)
         {
            activeDeck.FadeOut(fadeOutTime);
         }

         foreach (var deck in audioDecks)
         {
            if (deck != activeDeck)
            {
               deck.Play(cue, fadeInTime);
               activeDeck = deck;
               return;
            }
         }
      }

      public void SetVolume(float volume, AudioChannel channel = AudioChannel.Master)
      {
         volume = ConvertVolume(volume);
         switch (channel)
         {
            case AudioChannel.Music:
               audioMixer.SetFloat("MusicVolume", volume);
               PlayerPrefs.SetFloat("MusicVolume", volume);
               break;
            case AudioChannel.SFX:
               audioMixer.SetFloat("SFXVolume", volume);
               PlayerPrefs.SetFloat("SFXVolume", volume);
               break;
            case AudioChannel.Dialogue:
               audioMixer.SetFloat("DialogueVolume", volume);
               PlayerPrefs.SetFloat("DialogueVolume", volume);
               break;
            case AudioChannel.Master:
               audioMixer.SetFloat("MasterVolume", volume);
               PlayerPrefs.SetFloat("MasterVolume", volume);
               break;
         }
      }

      private void LoadVolume()
      {
         if(PlayerPrefs.HasKey("MusicVolume"))
            audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
         if(PlayerPrefs.HasKey("SFXVolume"))
            audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
         if(PlayerPrefs.HasKey("DialogueVolume"))
            audioMixer.SetFloat("DialogueVolume", PlayerPrefs.GetFloat("DialogueVolume"));
         if(PlayerPrefs.HasKey("MasterVolume"))
            audioMixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume"));
      }

      public float GetVolume(AudioChannel channel)
      {
         float db = -80;
         audioMixer.GetFloat(channel.ToString() + "Volume", out db);
         float volume = Mathf.Pow(10, db / 20);
      
         return volume;
      }

      private float ConvertVolume(float volume)
      {
         var vmax = Mathf.Max(volume, 0.0001f);
      
         return Mathf.Log10(vmax) * 20;
      }
   
   }
