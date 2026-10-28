using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MordiAudio.MusicSystem.Demo
{
    public class StatusTextFetcher : MonoBehaviour
    {
        public DynamicMusicSystem musicSystem;

        Text text;

        private void Awake() {
            text = GetComponent<Text>();
        }

        private void Update() {
            // Fetch some info about what's going on with the dynamic music system
            if (musicSystem == null)
                return;

            string str = "Status";

            // PlayState of music system
            str += $"\n\n<b>{CapitalizeFirstLetter(musicSystem.CurrentPlayState.ToString())}</b>";

            // Display current state
            if (musicSystem.currentState != null)
                str += $"\nState: <b>{musicSystem.currentState.name}</b>";

            DynamicMusicSystem.Track track = musicSystem.GetTrack(0);
            if (track != null) {

                // Display current section of first track
                if (track.currentSection != null)
                    str += $"\nSection: <b>{track.currentSection.name}</b>";

                // Display queued section of first track
                if (track.queuedSections != null) {
                    if (track.queuedSections.Count > 0)
                        str += $"\nQueued sections:";

                    for (int i = 0; i < track.queuedSections.Count; i++) {
                        str += $"\n<b>{track.queuedSections[i].name}</b>";
                    }
                }
            }

            text.text = str;
        }

        string CapitalizeFirstLetter(string str) {
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }
    }
}
