using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MordiAudio.MusicSystem
{
    [AddComponentMenu("Mordi Audio/Dynamic Music System")]
    public class DynamicMusicSystem : MonoBehaviour
    {
        public enum PlayState
        {
            stopped,
            stopping,
            playing
        }

        [System.Serializable]
        public class State
        {
            public string name, id;
        }

        [System.Serializable]
        public class Track
        {
            [System.Serializable]
            public class Section
            {
                public string name;
                public int intro, start, end, stop;
                public double introDuration, mainDuration, outroDuration; // in seconds
                public string linkedSection;

                public Section(string name) {
                    this.name = name;
                    intro = -1;
                    start = -1;
                    end = -1;
                    stop = -1;
                    linkedSection = "";
                }

                public void CalculateDurations(uint sampleRate) {
                    introDuration = (double)(start - intro) / sampleRate;
                    mainDuration = (double)(end - start) / sampleRate;
                    outroDuration = (double)(stop - end) / sampleRate;
                }
            }

            [System.Serializable]
            public class Transition
            {
                public string name;
                public int intro, end, stop;
                public double introDuration, outroDuration; // in seconds

                public Transition(string name) {
                    this.name = name;
                    intro = -1;
                    end = -1;
                    stop = -1;
                }

                public void CalculateDurations(uint sampleRate) {
                    introDuration = (double)(end - intro) / sampleRate;
                    outroDuration = (double)(stop - end) / sampleRate;
                }

                public bool IsValid() {
                    return (intro > -1 && end > -1 && stop > -1 && !string.IsNullOrEmpty(name));
                }
            }

            public Section currentSection;
            public List<Section> queuedSections;
            public List<Section> sections;
            public Section queuedStopSection;
            public Section nextSection;
            public List<string> stateIDs;

            public Transition nextTransition;
            public List<Transition> transitions;

            public AudioSource[] sources;
            public Wave.Metadata metadata;

            public float volume;
            public AudioClip clip;

            public int nextSourceToPlayIndex = 0;
            public double scheduledSectionSwitchTime;
            public bool shouldPlayInCurrentState;

            public bool fadeOnStateSwitch = true, fadeOnStart = false, fadeOnStop = true;
            public float stateFadeInDuration = 5f, stateFadeOutDuration = 5f, startFadeDuration = 5f, stopFadeDuration = 5f;
            float currentFadeDuration, currentFadeTarget;
            bool isFadingOut;

            public Track() {
                sources = new AudioSource[3];
                shouldPlayInCurrentState = true;
            }

            public void SetNextTransition(string transitionName) {
                nextTransition = FindTransition(transitionName);
            }

            Transition FindTransition(string name) {
                if (transitions == null)
                    return null;

                foreach (Transition t in transitions) {
                    if (t.name == name)
                        return t;
                }
                return null;
            }

            public void UpdateNextSection() {
                Section next = currentSection;

                // Set to linked section (if provided)
                if (currentSection != null) {
                    if (!string.IsNullOrEmpty(currentSection.linkedSection)) {
                        Section linkedSection = FindSection(currentSection.linkedSection);
                        if (linkedSection != null) {
                            next = linkedSection;
                        }
                    }
                }

                // Set to queued section (if any)
                if (queuedSections != null) {
                    if (queuedSections.Count > 0) {
                        next = queuedSections[0];
                    }
                }
                nextSection = next;
            }

            public void ClearSectionQueue() {
                if (queuedSections == null)
                    return;
                queuedSections.Clear();
                UpdateNextSection();
            }

            public void QueueSection(string sectionName, bool queueStop) {
                if (sections == null) return;

                foreach (Section section in sections) {
                    if (sectionName == section.name) {
                        AddSectionToQueue(section);
                        if (queueStop)
                            QueueStop();
                        break;
                    }
                }
            }

            public void QueueSectionNext(bool queueStop) {
                if (sections == null) return;

                for (int i = 0; i < sections.Count; i++) {
                    if (currentSection.name == sections[i].name) {
                        if (sections.Count - 1 > i)
                            AddSectionToQueue(sections[i + 1]);
                        else
                            AddSectionToQueue(sections[0]);
                        if (queueStop)
                            QueueStop();
                        break;
                    }
                }
            }

            public void QueueSectionPrevious(bool queueStop) {
                if (sections == null) return;

                for (int i = 0; i < sections.Count; i++) {
                    if (currentSection.name == sections[i].name) {
                        if (i == 0)
                            AddSectionToQueue(sections[sections.Count - 1]);
                        else
                            AddSectionToQueue(sections[i - 1]);
                        if (queueStop)
                            QueueStop();
                        break;
                    }
                }
            }

            public void QueueSectionFirst() {
                if (sections == null) return;
                if (sections.Count > 0)
                    AddSectionToQueue(sections[0]);
            }

            Section FindSection(int index) {
                if (sections == null)
                    return null;

                if (sections.Count > index)
                    return sections[index];
                return null;
            }

            Section FindSection(string name) {
                if (sections == null)
                    return null;

                foreach (Section s in sections) {
                    if (s.name == name)
                        return s;
                }
                return null;
            }

            void AddSectionToQueue(Section section) {
                if (queuedSections == null)
                    queuedSections = new List<Section>();

                queuedSections.Add(section);
                UpdateNextSection();
            }

            /// <summary>
            /// Tells the track to stop after the last queued section, or current section if none are queued.
            /// </summary>
            public void QueueStop() {
                if (fadeOnStop) {
                    UpdateFadeTargetAndDuration(0f, stopFadeDuration);
                    isFadingOut = true;
                } else {
                    if (HasQueuedSection)
                        queuedStopSection = queuedSections[queuedSections.Count - 1];
                    else
                        queuedStopSection = currentSection;
                }
            }

            private bool muted;
            public bool Muted {
                get { return muted; }
                set {
                    muted = value;
                    for (int i = 0; i < sources.Length; i++) {
                        if (sources[i] != null)
                            sources[i].mute = muted;
                    }
                }
            }

            public bool IsPlaying {
                get {
                    foreach (AudioSource s in sources) {
                        if (s != null) {
                            if (s.isPlaying)
                                return true;
                        }
                    }
                    return false;
                }
            }

            public bool HasClip {
                get {
                    foreach (AudioSource s in sources) {
                        if (s != null) {
                            if (s.clip != null)
                                return true;
                        }
                    }
                    return false;
                }
            }

            public bool HasQueuedSection {
                get {
                    if (queuedSections != null) {
                        if (queuedSections.Count > 0)
                            return true;
                    }
                    return false;
                }
            }

            public bool IsSilent {
                get {
                    if (sources == null)
                        return true;
                    bool silent = true;
                    for (int i = 0; i < sources.Length; i++) {
                        if (sources[i] == null)
                            continue;
                        if (sources[i].isPlaying && sources[i].volume > 0f) {
                            silent = false;
                            break;
                        }
                    }
                    return silent;
                }
            }

            public bool HasSections {
                get {
                    if (sections == null)
                        return false;
                    if (sections.Count == 0)
                        return false;
                    return true;
                }
            }

            public void StartPlayback(string sectionName = "", string transitionName = "") {
                if (!HasClip)
                    return;

                // Initialize volume
                volume = GetInitialVolume();

                // Initialize fade target
                float fadeTar = volume;
                if (fadeOnStart) {
                    volume = 0f;
                }
                UpdateFadeTargetAndDuration(fadeTar, startFadeDuration);
                isFadingOut = false;

                // Apply variables to audio sources
                for (int i = 0; i < sources.Length; i++) {
                    if (sources[i] != null) {
                        sources[i].Stop();
                        sources[i].timeSamples = 0;
                        sources[i].loop = false;
                        sources[i].volume = volume;
                    }
                }
                nextSourceToPlayIndex = 0;

                // Set section variables
                if (sectionName == "")
                    currentSection = FindSection(0);
                else
                    currentSection = FindSection(sectionName);
                queuedStopSection = null;

                // Set next transition, unless the given transition is an empty string (which could mean a transition has been set before starting playback)
                if (!string.IsNullOrEmpty(transitionName))
                    nextTransition = FindTransition(transitionName);

                // Validate transition
                if (nextTransition != null) {
                    if (!nextTransition.IsValid()) {
                        nextTransition = null;
                    }
                }

                // Prepare to play initial section
                if (currentSection != null) {

                    // Set playback position
                    sources[0].timeSamples = currentSection.intro;
                    sources[1].timeSamples = currentSection.intro;

                    if (nextTransition == null) {
                        scheduledSectionSwitchTime = AudioSettings.dspTime + currentSection.introDuration + currentSection.mainDuration;

                        // Start playing first source
                        sources[0].PlayScheduled(AudioSettings.dspTime);

                    } else {
                        sources[2].timeSamples = nextTransition.intro;
                        double longestIntroDuration = 0.0;

                        if (nextTransition.introDuration > currentSection.introDuration) {
                            // Play transition and then the section
                            longestIntroDuration = nextTransition.introDuration;
                            sources[2].timeSamples = nextTransition.intro;
                            sources[2].Play();
                            sources[0].PlayScheduled(AudioSettings.dspTime + nextTransition.introDuration - currentSection.introDuration);
                        } else {
                            // Play section and then the transition
                            longestIntroDuration = currentSection.introDuration;
                            sources[2].timeSamples = nextTransition.intro;
                            sources[2].PlayScheduled(AudioSettings.dspTime + currentSection.introDuration - nextTransition.introDuration);
                            sources[0].Play();
                        }
                        sources[2].SetScheduledEndTime(AudioSettings.dspTime + nextTransition.introDuration + nextTransition.outroDuration);
                        scheduledSectionSwitchTime = AudioSettings.dspTime + longestIntroDuration + currentSection.mainDuration;
                        nextTransition = null;
                    }

                    // Prepare next section
                    UpdateNextSection();
                } else {
                    sources[0].loop = true;
                    sources[0].Play();
                }
            }

            public void HandleVolumeFading(float deltaTime) {
                float t = currentFadeTarget;
                if (volume < t) {
                    volume += deltaTime / currentFadeDuration;
                    if (volume > t)
                        volume = t;
                    UpdateSourceVolumes();
                }

                if (volume > t) {
                    volume -= deltaTime / currentFadeDuration;
                    if (volume < t)
                        volume = t;
                    UpdateSourceVolumes();
                }
            }

            float GetInitialVolume() {
                return shouldPlayInCurrentState ? 1f : 0f;
            }

            void UpdateFadeTargetAndDuration(float target, float duration) {
                currentFadeTarget = target;
                currentFadeDuration = duration;
            }

            public void HandleSectionSwitching() {
                if (nextSection == null)
                    return;

                if (nextSection.name == null)
                    return;

                // Get the longest intro duration by comparing the next section and next transition
                double longestIntroDuration = nextSection.introDuration;
                if (nextTransition != null) {
                    if (nextTransition.introDuration > longestIntroDuration)
                        longestIntroDuration = nextTransition.introDuration;
                }

                if (AudioSettings.dspTime > scheduledSectionSwitchTime - (longestIntroDuration + 1)) {

                    // If intro time is longer than the time until scheduled transition...
                    if ((scheduledSectionSwitchTime - AudioSettings.dspTime) < longestIntroDuration) {
                        // Loop current section one more time
                        nextSection = currentSection;
                    } else {
                        // Schedule play for next transition
                        if (nextTransition != null) {
                            if (nextTransition.name != null) {
                                if (sources[2] != null) {
                                    sources[2].timeSamples = nextTransition.intro;
                                    sources[2].PlayScheduled(scheduledSectionSwitchTime - nextTransition.introDuration);
                                    sources[2].SetScheduledEndTime(scheduledSectionSwitchTime + nextTransition.outroDuration);

                                    nextTransition = null;
                                }
                            }
                        }
                    }

                    // Remove first queued section from list (if it is indeed the next section)
                    if (queuedSections != null) {
                        if (queuedSections.Count > 0) {
                            if (queuedSections[0] == nextSection)
                                queuedSections.RemoveAt(0);
                        }
                    }

                    // Flip index
                    nextSourceToPlayIndex = 1 - nextSourceToPlayIndex;

                    // Schedule play on the other source (unless track is queued to stop on this section)
                    if (queuedStopSection != currentSection) {
                        sources[nextSourceToPlayIndex].PlayScheduled(scheduledSectionSwitchTime - nextSection.introDuration);
                        sources[nextSourceToPlayIndex].timeSamples = nextSection.intro;

                        // If we are not using fading, the next source should have its volume set to 0 if it's not in the current state
                        if (!fadeOnStateSwitch) {
                            sources[nextSourceToPlayIndex].volume = (shouldPlayInCurrentState ? 1f : 0f);
                        }
                    }

                    // Schedule end on this source
                    sources[1 - nextSourceToPlayIndex].SetScheduledEndTime(scheduledSectionSwitchTime + currentSection.outroDuration);

                    // Set next scheduled section switch time
                    scheduledSectionSwitchTime = scheduledSectionSwitchTime + nextSection.mainDuration;

                    // Update current section
                    currentSection = nextSection;

                    // Update next section
                    UpdateNextSection();
                }
            }

            public void UpdateSourceVolumes() {
                if (sources == null)
                    return;

                for (int i = 0; i < sources.Length; i++) {
                    if (sources[i] != null) {
                        sources[i].volume = volume * volume;
                    }
                }
            }

            public void UpdateState(State currentState) {
                bool prev = shouldPlayInCurrentState;

                shouldPlayInCurrentState = true;
                if (stateIDs != null && currentState != null) {
                    if (stateIDs.Count > 0)
                        shouldPlayInCurrentState = stateIDs.Contains(currentState.id);
                }

                if (fadeOnStateSwitch && !isFadingOut) {
                    // Entered state
                    if (shouldPlayInCurrentState && !prev) {
                        UpdateFadeTargetAndDuration(1f, stateFadeInDuration);
                    }
                    // Exited state
                    if (!shouldPlayInCurrentState && prev) {
                        UpdateFadeTargetAndDuration(0f, stateFadeOutDuration);
                    }
                }
            }

            //#if UNITY_EDITOR

            public Material waveformMaterial;
            public bool trackSettingsFoldout;

            //#endif
        }

        public List<Track> tracks;
        public PlayState CurrentPlayState { get; private set; }
        public List<State> states;
        public State currentState;
        public AudioMixerGroup outputAudioMixerGroup;

        [SerializeField]
        bool playOnAwake = false;

        [SerializeField]
        float playOnAwakeDelay = 0f;

        private void Awake() {
            if (playOnAwake) {
                // Note: We invoke with a slight delay to fix a bug where the dspTime seems to count while the game is still loading,
                // which would lead to the first section switch happening too early.
                Invoke(nameof(PlayOnAwake), playOnAwakeDelay);
            }

            // Go to initial state
            GoToStateFirst();
        }

        void PlayOnAwake() {
            StartPlaybackAllTracks();
        }

        private void Update() {
            if (CurrentPlayState != PlayState.stopped)
                UpdateTracks(Time.unscaledDeltaTime);
        }

        // Public because editor script needs access to it
        public void UpdateTracks(float deltaTime) {
            bool allTracksAreStopped = true, allTracksAreSilent = true;

            foreach (Track t in tracks) {

                // Handle fading
                t.HandleVolumeFading(deltaTime);

                if (t.IsPlaying) {
                    // Handle sections
                    t.HandleSectionSwitching();

                    allTracksAreStopped = false;

                    if (!t.IsSilent)
                        allTracksAreSilent = false;
                }
            }

            // Stop playback if all tracks are stopped
            if (allTracksAreStopped || (CurrentPlayState == PlayState.stopping && allTracksAreSilent)) {
                StopPlaybackAllTracksImmediate();
            }
        }

        /// <summary>
        /// Start playing all tracks, starting at a given section using a given transition. If no section is supplied, the first one (in timeline order) will be used.
        /// If no state is set, the first one will be entered.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        public void StartPlaybackAllTracks() {
            StartPlaybackAllTracks("", "");
        }

        /// <summary>
        /// Start playing all tracks, starting at a given section using a given transition. If no section is supplied, the first one (in timeline order) will be used.
        /// If no state is set, the first one will be entered.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        public void StartPlaybackAllTracks(string sectionName) {
            StartPlaybackAllTracks(sectionName, "");
        }

        /// <summary>
        /// Start playing all tracks, optionally starting at a given section using a given transition. If no section is supplied, the first one (in timeline order) will be used.
        /// If no state is set, the first one will be entered.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        /// <param name="transitionName">Name of transition (case sensitive)</param>
        public void StartPlaybackAllTracks(string sectionName, string transitionName) {
            if (CurrentPlayState != PlayState.stopped || tracks == null)
                return;

            // Set state to playing
            CurrentPlayState = PlayState.playing;

            // Go to first state by default if no state is set
            if (!CurrentStateIsValid())
                GoToStateFirst();

            // Start playback on all tracks
            foreach (Track t in tracks) {
                t.StartPlayback(sectionName, transitionName);
            }
        }

        /// <summary>
        /// Check whether or not the music system is currently in a valid state based on the current list of states.
        /// </summary>
        /// <returns>True or False</returns>
        bool CurrentStateIsValid() {
            if (currentState == null)
                return false;

            if (states.Count == 0)
                return false;

            for (int i = 0; i < states.Count; i++) {
                if (currentState.id == states[i].id)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Tell all tracks to fade out and eventually stop.
        /// </summary>
        public void StopPlaybackAllTracks() {
            if (CurrentPlayState != PlayState.playing)
                return;

            CurrentPlayState = PlayState.stopping;
            foreach (Track t in tracks) {
                t.QueueStop();
            }
        }

        /// <summary>
        /// Stop playback on all tracks immediately, ignoring fadeout time.
        /// </summary>
        public void StopPlaybackAllTracksImmediate() {
            if (CurrentPlayState == PlayState.stopped)
                return;

            if (tracks == null)
                return;

            foreach (Track t in tracks) {
                if (t.sources != null) {
                    foreach (AudioSource s in t.sources) {
                        if (s != null)
                            s.Stop();
                    }
                }
                t.volume = 0f;
                t.UpdateSourceVolumes();
                t.ClearSectionQueue();
            }
            CurrentPlayState = PlayState.stopped;
        }

        /// <summary>
        /// Set all tracks to go to a given section. Any items in the section queue will be cleared.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        public void GoToSectionAllTracks(string sectionName) {
            GoToSectionAllTracks(sectionName, "");
        }

        /// <summary>
        /// Set all tracks to go to a given section, optionally with a given transition. Any items in the section queue will be cleared.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        /// <param name="transitionName">Name of transition (case sensitive, optional)</param>
        public void GoToSectionAllTracks(string sectionName, string transitionName) {
            if (CurrentPlayState != PlayState.playing) {
                StartPlaybackAllTracks(sectionName, transitionName);
                return;
            }

            foreach (Track track in tracks) {
                track.ClearSectionQueue();
                track.QueueSection(sectionName, false);
                if (transitionName != "")
                    track.SetNextTransition(transitionName);
            }
        }

        /// <summary>
        /// Set the transition to be used on the next section switch on all tracks.
        /// </summary>
        /// <param name="transitionName">Name of transition (case sensitive).</param>
        public void SetTransitionAllTracks(string transitionName) {
            foreach (Track track in tracks) {
                track.SetNextTransition(transitionName);
            }
        }

        /// <summary>
        /// Removes all items from the section queue (including queued stops) on all tracks.
        /// </summary>
        public void ClearSectionQueueAllTracks() {
            foreach (Track track in tracks) {
                track.ClearSectionQueue();
            }
        }

        /// <summary>
        /// Queue a given section on all tracks.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        public void QueueSectionAllTracks(string sectionName) {
            QueueSectionAllTracks(sectionName, false);
        }

        /// <summary>
        /// Queue a given section on all tracks.
        /// </summary>
        /// <param name="sectionName">Name of section (case sensitive)</param>
        /// <param name="queueStop">Whether or not to queue a stop after queueing the section</param>
        public void QueueSectionAllTracks(string sectionName, bool queueStop) {
            foreach (Track track in tracks) {
                track.QueueSection(sectionName, queueStop);
            }
        }

        /// <summary>
        /// Queue the next section (in relation to currently playing section) on all tracks, in timeline-order.
        /// </summary>
        /// <param name="queueStop">Whether or not to queue a stop after queueing the section</param>
        public void QueueSectionNextAllTracks(bool queueStop = false) {
            foreach (Track track in tracks) {
                track.QueueSectionNext(queueStop);
            }
        }

        /// <summary>
        /// Queue the previous section (in relation to currently playing section) on all tracks, in timeline-order.
        /// </summary>
        /// <param name="queueStop">Whether or not to queue a stop after queueing the section</param>
        public void QueueSectionPreviousAllTracks(bool queueStop = false) {
            foreach (Track track in tracks) {
                track.QueueSectionPrevious(queueStop);
            }
        }

        /// <summary>
        /// Queues a stop on all tracks. Any sections already in the queue will play. The stop will occur at the end of the final section.
        /// </summary>
        public void QueueStopAllTracks() {
            foreach (Track track in tracks) {
                track.QueueStop();
            }
        }

        /// <summary>
        /// Enter the first state defined in the list of states.
        /// </summary>
        public void GoToStateFirst() {
            if (states == null)
                return;

            if (states.Count == 0) {
                GoToState(null as State);
                return;
            }

            GoToState(states[0]);
        }

        /// <summary>
        /// Enters the given state on all tracks.
        /// </summary>
        /// <param name="name">Name of the state to enter (case sensitive)</param>
        public void GoToState(string name) {
            foreach (State state in states) {
                if (state.name == name) {
                    GoToState(state);
                    break;
                }
            }
        }

        /// <summary>
        /// Enters the given state on all tracks.
        /// </summary>
        /// <param name="state">A state object</param>
        public void GoToState(State state) {
            currentState = state;

            if (tracks != null) {
                for (int i = 0; i < tracks.Count; i++) {
                    tracks[i].UpdateState(currentState);
                }
            }
        }

        /// <summary>
        /// Count how many tracks this system contains.
        /// </summary>
        /// <returns>Track count</returns>
        public int GetTrackCount() {
            if (tracks == null)
                return 0;
            return tracks.Count;
        }

        /// <summary>
        /// Returns a track by index (zero-based). Note that the index number displayed in the inspector is not zero-based.
        /// </summary>
        /// <param name="index">Index of track in the tracks-list</param>
        /// <returns></returns>
        public Track GetTrack(int index) {
            if (tracks == null)
                return null;
            if (index < tracks.Count)
                return tracks[index];
            return null;
        }

        /// <summary>
        /// Sets the audio mixer group output of all AudioSource components used by all tracks. Meant for internal use while editing.
        /// </summary>
        public void UpdateOutputAudioMixerGroupAllTracks() {
            if (tracks == null)
                return;

            for (int i = 0; i < tracks.Count; i++) {
                if (tracks[i].sources == null)
                    continue;
                for (int n = 0; n < tracks[i].sources.Length; n++) {
                    if (tracks[i].sources[n] != null)
                        tracks[i].sources[n].outputAudioMixerGroup = outputAudioMixerGroup;
                }
            }
        }
    }
}