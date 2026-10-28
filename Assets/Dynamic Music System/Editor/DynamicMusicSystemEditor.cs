using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections.Generic;

namespace MordiAudio.MusicSystem
{
    [CustomEditor(typeof(DynamicMusicSystem))]
    public class DynamicMusicSystemEditor : Editor
    {
        const int WAVEFORM_TEX_HEIGHT = 30;

        struct StateSelection
        {
            public DynamicMusicSystem.Track track;
            public DynamicMusicSystem.State state;
            public bool exists;
        }

        struct TimelineLabelToDraw
        {
            public string name;
            public Rect rect;
            public GUIStyle style;
        }
        List<TimelineLabelToDraw> timelineLabelsToDraw;

        Material coloredMaterial, faderKnobMaterial;

        DynamicMusicSystem musicSystem;
        DynamicMusicSystem.Track trackToRemove;
        DynamicMusicSystem.State stateToRemove;
        double timePrev = 0f;
        float deltaTime = 0f;
        float timeSinceTracksUpdate;

        List<DynamicMusicSystem.Track> soloedTracks;

        List<string> sectionNames;
        List<string> transitionNames;

        class Styles
        {
            public GUIStyle sectionLabel, transitionLabel, emptyListLabel, transportButton, addButton, faderKnob, stateButton, logo,
                sectionButton, fadeDurationField, trackSettingsFoldout, trackSettingsBox, trackSettingsLabel, trackSettingsToggle;

            public Styles() {
                // Transport buttons
                if (transportButton == null) {
                    transportButton = new GUIStyle(GUI.skin.button) {
                        fixedWidth = 50,
                        fixedHeight = 42
                    };
                }

                // Style for timeline labels
                if (sectionLabel == null) {
                    sectionLabel = new GUIStyle {
                        alignment = TextAnchor.UpperCenter,
                        fontSize = 8
                    };
                    sectionLabel.normal.textColor = Utility.Colors.colHighlight;
                }
                if (transitionLabel == null) {
                    transitionLabel = new GUIStyle(sectionLabel);
                    transitionLabel.normal.textColor = Utility.Colors.colHighlightAlt;
                }

                // Style for empty state/track list label
                if (emptyListLabel == null) {
                    emptyListLabel = new GUIStyle {
                        alignment = TextAnchor.MiddleCenter
                    };
                    emptyListLabel.normal.textColor = Color.gray;
                }

                // Style for little "+" buttons
                if (addButton == null) {
                    addButton = new GUIStyle(GUI.skin.button) {
                        fixedWidth = 25,
                        fixedHeight = 25
                    };
                }

                // Style for volume fader knob
                if (faderKnob == null) {
                    faderKnob = new GUIStyle();
                    faderKnob.normal.background = (Texture2D)Utility.Assets.LoadTexture("dms_fader_knob");
                    faderKnob.margin = new RectOffset(0, 0, 0, 0);
                    faderKnob.padding = new RectOffset(0, 0, 0, 0);
                }

                // Style for state buttons
                if (stateButton == null) {
                    stateButton = new GUIStyle(GUI.skin.button) {
                        fixedWidth = 25,
                        fixedHeight = 19,
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 1, 0)
                    };
                }

                // Style for logo
                if (logo == null) {
                    logo = new GUIStyle() {
                        margin = new RectOffset(0, 0, 2, 0)
                    };
                }

                // Section and transition buttons
                sectionButton = new GUIStyle(GUI.skin.button);
                sectionButton.stretchWidth = true;
                sectionButton.fixedHeight = 20;

                trackSettingsFoldout = new GUIStyle(EditorStyles.foldout);
                trackSettingsFoldout.fontStyle = FontStyle.Bold;
                trackSettingsFoldout.fixedHeight = 20;
                trackSettingsFoldout.margin = new RectOffset(0, 0, 1, 0);

                fadeDurationField = new GUIStyle(EditorStyles.numberField);
                fadeDurationField.fixedHeight = 18;
                fadeDurationField.margin = new RectOffset(0, 4, 0, 0);
                fadeDurationField.alignment = TextAnchor.MiddleLeft;

                trackSettingsBox = new GUIStyle(EditorStyles.helpBox);
                trackSettingsBox.padding = new RectOffset(10, 10, 10, 10);

                trackSettingsLabel = new GUIStyle(EditorStyles.label);
                trackSettingsLabel.margin = new RectOffset(0, 5, 4, 0);
                trackSettingsLabel.fixedWidth = 30;

                trackSettingsToggle = new GUIStyle(EditorStyles.toggle);
                trackSettingsToggle.fixedHeight = 18;
                trackSettingsToggle.margin = new RectOffset(0, 0, 0, 0);
            }
        }
        Styles styles;

        class Content
        {
            public GUIContent playButton, stopImmediatelyButton, stopButton, addTrackButton, addStateButton,
            removeStateButton, removeTrackButton, soloButton, muteButton,
            activeStateButton, inactiveStateButton, sectionButton, transitionButton, fadeOnStateSwitch,
            startFadeToggle, stopFadeToggle, stateButton, refreshButton;

            public Content() {
                playButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_play"), "Start playback");
                stopButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_stop"), "Stop playback");
                stopImmediatelyButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_stopimm"), "Stop playback immediately");
                addTrackButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_add"), "Add new track");
                addStateButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_add"), "Add new state");
                refreshButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_refresh"), "Refresh clip metadata");
                removeTrackButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_remove"), "Remove track");
                removeStateButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_remove"), "Remove state");
                soloButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_solo"), "Solo");
                muteButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_mute"), "Mute");
                activeStateButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_active"), "This is the active state");
                inactiveStateButton = new GUIContent(Utility.Assets.LoadTexture("dms_icon_inactive"), "Activate this state");
                startFadeToggle = new GUIContent("Fade in on start", "Enables volume fading in when playback is started");
                stopFadeToggle = new GUIContent("Fade out on stop", "Enables volume fading out when playback is stopped");
                fadeOnStateSwitch = new GUIContent("Fade between states", "Enables volume fading in/out upon entering/exiting states");
                sectionButton = new GUIContent("?", "Go to this section");
                transitionButton = new GUIContent("?", "Use this transition when switching to next section");
                stateButton = new GUIContent("?", "Select which states this track should play in (if set to none, it will always play)");
            }
        }
        Content content;

        SerializedProperty tracksProp, outputGroupProp, playOnAwakeProp, playOnAwakeDelayProp;

        //////////////////////
        //  Initialization  //
        //////////////////////

        void InitializeContent() {
            content = new Content();
        }

        void InitializeStyles() {
            styles = new Styles();
        }

        //////////////////
        //  Main stuff  //
        //////////////////

        void OnEnable() {
            // Load materials
            coloredMaterial = Utility.Assets.LoadMaterial("dms_mat_default");
            faderKnobMaterial = Utility.Assets.LoadMaterial("dms_mat_faderknob");

            // Reset objects to remove
            trackToRemove = null;
            stateToRemove = null;

            InitializeContent();

            timelineLabelsToDraw ??= new List<TimelineLabelToDraw>();

            sectionNames = new List<string>();
            transitionNames = new List<string>();

            // Get serialized properties
            tracksProp = serializedObject.FindProperty("tracks");
            outputGroupProp = serializedObject.FindProperty("outputAudioMixerGroup");
            playOnAwakeProp = serializedObject.FindProperty("playOnAwake");
            playOnAwakeDelayProp = serializedObject.FindProperty("playOnAwakeDelay");
        }

        private void OnDestroy() {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) {
                if (musicSystem != null)
                    musicSystem.StopPlaybackAllTracksImmediate();
            }
        }

        public override void OnInspectorGUI() {
            // Copying from an existing style (buttons) requires an OnGUI call, so we can't initialize them in OnEnable
            if (styles == null)
                InitializeStyles();

            // Calculate deltaTime
            deltaTime = (float)(EditorApplication.timeSinceStartup - timePrev);
            timeSinceTracksUpdate += deltaTime;

            // Get target
            if (musicSystem == null) {
                musicSystem = (DynamicMusicSystem)target;
            }

            // Check if we can use preview playback (which is not possible if viewing as prefab, except when in the full prefab editor)
            bool canUsePreview = !EditorUtility.IsPersistent(musicSystem);

            if (!canUsePreview) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Can't preview playback in inspector when viewing as prefab.\nEnter full prefab mode, or view in scene instead.", styles.emptyListLabel);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(2f);

            // Playback control buttons
            GUILayout.BeginHorizontal();
            GUI.enabled = AnyTrackHasClip() && canUsePreview;
            switch (musicSystem.CurrentPlayState) {
                case DynamicMusicSystem.PlayState.stopped:
                    if (GUILayout.Button(content.playButton, styles.transportButton))
                        musicSystem.StartPlaybackAllTracks();
                    break;
                case DynamicMusicSystem.PlayState.stopping:
                    if (GUILayout.Button(content.stopImmediatelyButton, styles.transportButton))
                        musicSystem.StopPlaybackAllTracksImmediate();
                    break;
                case DynamicMusicSystem.PlayState.playing:
                    if (GUILayout.Button(content.stopButton, styles.transportButton))
                        musicSystem.StopPlaybackAllTracks();
                    break;
                default:
                    break;
            }

            GUI.enabled = true;
            GUILayout.Space(2f);

            // Buttons for every section and transition
            UpdateSectionButtonList();
            UpdateTransitionButtonList();

            GUI.enabled = (musicSystem.CurrentPlayState != DynamicMusicSystem.PlayState.stopped);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Transitions", GUILayout.MaxWidth(75), GUILayout.Height(20));
            if (transitionNames.Count > 0) {
                foreach (string s in transitionNames) {
                    GUI.backgroundColor = Color.white;
                    for (int i = 0; i < musicSystem.tracks.Count; i++) {
                        if (musicSystem.tracks[i].nextTransition != null) {
                            if (s == musicSystem.tracks[i].nextTransition.name) {
                                GUI.backgroundColor = Utility.Colors.colHighlightAlt;
                                break;
                            }
                        }
                    }
                    content.transitionButton.text = s;
                    if (GUILayout.Button(content.transitionButton, styles.sectionButton, GUILayout.MinWidth(50)))
                        musicSystem.SetTransitionAllTracks(s);
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("[Cancel]", styles.sectionButton, GUILayout.MaxWidth(75)))
                    musicSystem.SetTransitionAllTracks("");
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sections", GUILayout.MaxWidth(75), GUILayout.Height(20));
            if (sectionNames.Count > 0) {
                foreach (string s in sectionNames) {
                    GUI.backgroundColor = Color.white;
                    for (int i = 0; i < musicSystem.tracks.Count; i++) {
                        if (musicSystem.tracks[i].queuedSections != null) {
                            for (int n = 0; n < musicSystem.tracks[i].queuedSections.Count; n++) {
                                if (s == musicSystem.tracks[i].queuedSections[n].name) {
                                    GUI.backgroundColor = Utility.Colors.colHighlight;
                                    break;
                                }
                            }
                        }
                    }
                    content.sectionButton.text = s;
                    if (GUILayout.Button(content.sectionButton, styles.sectionButton, GUILayout.MinWidth(25)))
                        musicSystem.GoToSectionAllTracks(s);
                }
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("[Cancel]", styles.sectionButton, GUILayout.MaxWidth(75)))
                    musicSystem.ClearSectionQueueAllTracks();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            DrawSeparator(new Color(0.325f, 0.325f, 0.325f), 1, 20);

            bool showEmptyTrackListLabel = false;
            if (musicSystem.tracks != null) {

                // Draw tracks
                if (tracksProp != null) {
                    int trackNum = tracksProp.arraySize;
                    for (int i = 0; i < trackNum; i++) {
                        DisplayTrack(i, tracksProp.GetArrayElementAtIndex(i));
                        UpdateWaveformTexture(musicSystem.tracks[i]);
                    }
                }

                DrawTimelineLabels(null, 0);

                // Update track sources (handle looping, fading, etc)
                if (!EditorApplication.isPlaying && timeSinceTracksUpdate > 0.016f) { // Limiting update rate a bit
                    if (musicSystem.CurrentPlayState != DynamicMusicSystem.PlayState.stopped)
                        musicSystem.UpdateTracks(timeSinceTracksUpdate);

                    timeSinceTracksUpdate = 0f;
                }

                if (musicSystem.tracks.Count == 0)
                    showEmptyTrackListLabel = true;
            } else {
                showEmptyTrackListLabel = true;
            }

            if (showEmptyTrackListLabel) {
                GUILayout.Space(5f);
                GUILayout.FlexibleSpace();
                GUILayout.Label("No tracks", styles.emptyListLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Space(8f);
            }

            // "Add track" button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped;
            if (GUILayout.Button(content.addTrackButton, styles.addButton)) {
                AddTrack();
                musicSystem.StopPlaybackAllTracksImmediate();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            DrawSeparator(new Color(0.325f, 0.325f, 0.325f), 1, 20);

            // List of states
            bool showEmptyStateListLabel = false;
            if (musicSystem.states != null) {
                GUILayout.BeginVertical();
                int statesPerRow = 3;
                for (int i = 0; i < Mathf.CeilToInt((float)musicSystem.states.Count / statesPerRow); i++) {

                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    for (int n = 0; n < statesPerRow; n++) {
                        int index = i * statesPerRow + n;
                        if (index > musicSystem.states.Count - 1)
                            break;

                        GUI.enabled = musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped;

                        string fieldValue = EditorGUILayout.TextField(musicSystem.states[index].name, GUILayout.MaxWidth(120));
                        if (fieldValue != musicSystem.states[index].name) {
                            RenameState(musicSystem.states[index], fieldValue);
                        }

                        GUI.enabled = true;

                        if (musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped) {
                            if (GUILayout.Button(content.removeStateButton, styles.stateButton)) {
                                stateToRemove = musicSystem.states[index];
                            }
                        } else {

                            GUIContent stateButtonContent = content.inactiveStateButton;
                            if (musicSystem.states[index].id == musicSystem.currentState.id) {
                                stateButtonContent = content.activeStateButton;
                            }

                            if (GUILayout.Button(stateButtonContent, styles.stateButton)) {
                                musicSystem.GoToState(musicSystem.states[index]);
                            }

                        }

                        GUILayout.Label("", GUILayout.Width(3f));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(3f);
                }
                GUILayout.EndVertical();

                if (musicSystem.states.Count == 0) {
                    showEmptyStateListLabel = true;
                }
            } else {
                showEmptyStateListLabel = true;
            }

            if (showEmptyStateListLabel) {
                GUILayout.FlexibleSpace();
                GUILayout.Label("No states", styles.emptyListLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10f);

            // Add state
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped;
            if (GUILayout.Button(content.addStateButton, styles.addButton)) {
                AddState();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.enabled = true;

            DrawSeparator(new Color(0.325f, 0.325f, 0.325f), 1, 20);

            // Output mixer group
            EditorGUI.BeginChangeCheck();
            outputGroupProp.objectReferenceValue = (AudioMixerGroup)EditorGUILayout.ObjectField("Output", outputGroupProp.objectReferenceValue, typeof(AudioMixerGroup), false);
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
                musicSystem.UpdateOutputAudioMixerGroupAllTracks();
            }

            GUILayout.Space(2f);

            // Toggle 'play on awake'
            GUILayout.BeginHorizontal();
            playOnAwakeProp.boolValue = GUILayout.Toggle(playOnAwakeProp.boolValue, "Play on awake");

            GUILayout.Label("Delay (seconds)");
            GUI.enabled = playOnAwakeProp.boolValue;
            playOnAwakeDelayProp.floatValue = EditorGUILayout.FloatField(playOnAwakeDelayProp.floatValue);
            playOnAwakeDelayProp.floatValue = Mathf.Clamp(playOnAwakeDelayProp.floatValue, 0.1f, 300f);
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);

            // Open linked sections editor
            if (GUILayout.Button("Edit linked sections")) {
                LinkedSectionEditorWindow.Open((DynamicMusicSystem)target);
            }

            GUILayout.Space(5f);

            // Remove track queued for removal
            if (trackToRemove != null) {
                RemoveTrack(trackToRemove);
                trackToRemove = null;
            }

            // Remove state queued for removal
            if (stateToRemove != null) {
                RemoveState(stateToRemove);
                stateToRemove = null;
            }

            // Time.deltaTime doesn't work in editor scripts, but can be gleamed from timeSinceStartup
            timePrev = EditorApplication.timeSinceStartup;

            // Apply any modified properties
            serializedObject.ApplyModifiedProperties();
        }

        void DisplayTrack(int index, SerializedProperty prop) {
            // Get track object
            DynamicMusicSystem.Track track = musicSystem.tracks[index];

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            DrawVolumeFader(prop.FindPropertyRelative("volume").floatValue);

            GUILayout.BeginVertical();
            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();

            // Track label (with foldout)
            string labelStr = "Track " + (index < 9 ? "0" + (index + 1) : (index + 1).ToString());
            SerializedProperty settingsFoldoutProp = prop.FindPropertyRelative("trackSettingsFoldout");
            EditorGUI.indentLevel++;
            settingsFoldoutProp.boolValue = EditorGUILayout.Foldout(settingsFoldoutProp.boolValue, labelStr, true, styles.trackSettingsFoldout);
            EditorGUI.indentLevel--;

            bool enabledState = (musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped);
            GUI.enabled = enabledState;

            // States dropdown
            content.stateButton.text = GetStateDropdownString(track);
            if (GUILayout.Button(content.stateButton, GUILayout.Width(Mathf.Max(120, Screen.width / 3)), GUILayout.Height(20))) {
                DrawStatesMenu(track);
            }

            // Refresh
            if (GUILayout.Button(content.refreshButton, GUILayout.Width(30))) {
                OnTrackClipChanged(track.clip, prop, index);
            }

            // Remove
            if (GUILayout.Button(content.removeTrackButton, GUILayout.Width(30))) {
                trackToRemove = track;
            }

            GUILayout.EndHorizontal();

            // Track settings
            if (settingsFoldoutProp.boolValue) {
                GUILayout.Space(3f);
                GUILayout.BeginVertical(styles.trackSettingsBox);

                // Fade on start
                GUILayout.BeginHorizontal();
                SerializedProperty fadeStartProp = prop.FindPropertyRelative("fadeOnStart");
                fadeStartProp.boolValue = GUILayout.Toggle(fadeStartProp.boolValue, content.startFadeToggle, styles.trackSettingsToggle, GUILayout.MaxWidth(150));

                // Duration field
                if (fadeStartProp.boolValue) {
                    SerializedProperty startFadeDurationProp = prop.FindPropertyRelative("startFadeDuration");
                    startFadeDurationProp.floatValue = EditorGUILayout.FloatField(startFadeDurationProp.floatValue, styles.fadeDurationField, GUILayout.MaxWidth(50));
                    startFadeDurationProp.floatValue = Mathf.Clamp(startFadeDurationProp.floatValue, 0.001f, 99f);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(3f);

                // Fade on stop
                GUILayout.BeginHorizontal();
                bool hasSections = track.HasSections;
                GUI.enabled = hasSections && enabledState;
                SerializedProperty fadeStopProp = prop.FindPropertyRelative("fadeOnStop");
                fadeStopProp.boolValue = GUILayout.Toggle(fadeStopProp.boolValue, content.stopFadeToggle, styles.trackSettingsToggle, GUILayout.MaxWidth(150));
                GUI.enabled = enabledState;

                if (fadeStopProp.boolValue) {
                    SerializedProperty stopFadeDurationProp = prop.FindPropertyRelative("stopFadeDuration");
                    stopFadeDurationProp.floatValue = EditorGUILayout.FloatField(stopFadeDurationProp.floatValue, styles.fadeDurationField, GUILayout.MaxWidth(50));
                    stopFadeDurationProp.floatValue = Mathf.Clamp(stopFadeDurationProp.floatValue, 0.001f, 99f);
                }
                GUILayout.EndHorizontal();
                if (!hasSections) {
                    EditorGUILayout.HelpBox("Fading on stop is required for tracks without sections.", MessageType.Info);
                }
                GUILayout.Space(3f);

                // Fade between states
                GUILayout.BeginHorizontal();
                SerializedProperty fadeOnStateSwitch = prop.FindPropertyRelative("fadeOnStateSwitch");
                fadeOnStateSwitch.boolValue = GUILayout.Toggle(fadeOnStateSwitch.boolValue, content.fadeOnStateSwitch, styles.trackSettingsToggle, GUILayout.MaxWidth(150));

                // Duration field
                if (fadeOnStateSwitch.boolValue) {
                    SerializedProperty stateFadeInDurationProp = prop.FindPropertyRelative("stateFadeInDuration");
                    stateFadeInDurationProp.floatValue = EditorGUILayout.FloatField(stateFadeInDurationProp.floatValue, styles.fadeDurationField, GUILayout.MaxWidth(50));
                    stateFadeInDurationProp.floatValue = Mathf.Clamp(stateFadeInDurationProp.floatValue, 0.001f, 99f);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(3f);

                // Duration field
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                if (fadeOnStateSwitch.boolValue) {
                    SerializedProperty stateFadeOutDurationProp = prop.FindPropertyRelative("stateFadeOutDuration");
                    stateFadeOutDurationProp.floatValue = EditorGUILayout.FloatField(stateFadeOutDurationProp.floatValue, styles.fadeDurationField, GUILayout.MaxWidth(50));
                    stateFadeOutDurationProp.floatValue = Mathf.Clamp(stateFadeOutDurationProp.floatValue, 0.001f, 99f);
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(3f);
            }

            GUILayout.Space(2f);
            GUILayout.BeginHorizontal();

            // Audio clip
            SerializedProperty clipProp = prop.FindPropertyRelative("clip");
            EditorGUI.BeginChangeCheck();
            clipProp.objectReferenceValue = EditorGUILayout.ObjectField(clipProp.objectReferenceValue, typeof(AudioClip), false, GUILayout.MaxWidth(2000), GUILayout.Height(20));
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Changed track audio clip");
                OnTrackClipChanged((AudioClip)clipProp.objectReferenceValue, prop, index);
                serializedObject.ApplyModifiedProperties();
            }

            // Mute
            GUI.enabled = !AnyTrackIsSoloed();
            if (!TrackIsMuted(track)) {
                if (GUILayout.Button(content.muteButton, GUILayout.Width(30))) {
                    track.Muted = true;
                }
            } else {
                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button(content.muteButton, GUILayout.Width(30))) {
                    track.Muted = false;
                }
                GUI.backgroundColor = Color.white;
            }
            GUI.enabled = true;

            // Solo
            if (!TrackIsSoloed(track)) {
                if (GUILayout.Button(content.soloButton, GUILayout.Width(30))) {
                    SoloTrack(track);
                }
            } else {
                GUI.backgroundColor = new Color(1f, 1f, 0.4f);
                if (GUILayout.Button(content.soloButton, GUILayout.Width(30))) {
                    UnSoloTrack(track);
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndHorizontal();

            // Timeline and waveform
            DrawTimeline(track, prop);

            GUILayout.Space(5f);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            // Draw all properties (used for debugging)
            //DrawProperties(prop, true);
        }

        #region Drawing

        // Used for debugging
        void DrawProperties(SerializedProperty prop, bool drawChildren) {
            string lastPropPath = string.Empty;
            foreach (SerializedProperty p in prop) {
                if (p.isArray && p.propertyType == SerializedPropertyType.Generic) {

                    EditorGUILayout.BeginHorizontal();
                    p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (p.isExpanded) {
                        EditorGUI.indentLevel++;
                        DrawProperties(p, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                } else {
                    if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) { continue; }
                    lastPropPath = p.propertyPath;
                    EditorGUILayout.PropertyField(p, drawChildren);
                }
            }

        }

        void DrawSeparator(Color color, int thickness = 2, int padding = 10) {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.width = Screen.width;
            r.x = Screen.width / 2 - r.width / 2;
            EditorGUI.DrawRect(r, color);
        }

        void DrawStatesMenu(DynamicMusicSystem.Track track) {
            GenericMenu menu = new GenericMenu();

            bool trackIsNotInAnyStates = true;
            if (musicSystem.states != null) {
                for (int i = 0; i < musicSystem.states.Count; i++) {
                    bool isOn = false;
                    if (track.stateIDs != null) {
                        isOn = track.stateIDs.Contains(musicSystem.states[i].id);
                    }

                    // Pass a state selection with all the info we need to add/remove state from track
                    StateSelection stateSelection = new StateSelection {
                        track = track,
                        state = musicSystem.states[i],
                        exists = isOn
                    };

                    menu.AddItem(new GUIContent(musicSystem.states[i].name), isOn, OnStateSelected, stateSelection);
                    if (isOn)
                        trackIsNotInAnyStates = false;
                }
            }
            menu.AddSeparator("");

            // Pass a state selection with state set to null
            StateSelection stateSelectionNone = new StateSelection {
                track = track,
                state = null
            };

            menu.AddItem(new GUIContent("None"), trackIsNotInAnyStates, OnStateSelected, stateSelectionNone);
            menu.ShowAsContext();
        }

        void DrawTimeline(DynamicMusicSystem.Track track, SerializedProperty trackProp) {
            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();

            Rect rect = GUILayoutUtility.GetRect(10, 10000, 9 + WAVEFORM_TEX_HEIGHT, 9 + WAVEFORM_TEX_HEIGHT);

            if (Event.current.type == EventType.Repaint) {
                int xoff = 4;
                float width = rect.width - xoff - 4, height = 9;
                GUI.BeginClip(rect);
                GL.PushMatrix();

                GL.Clear(false, false, Color.black);
                coloredMaterial.SetPass(0);

                GL.Begin(GL.QUADS);

                // Timeline and outline background
                GL.Color(Utility.Colors.colTimelineBackground);
                GL.Vertex3(xoff - 1, -1, 0);
                GL.Vertex3(xoff + width + 1, -1, 0);
                GL.Vertex3(xoff + width + 1, rect.height, 0);
                GL.Vertex3(xoff - 1, rect.height, 0);

                // Waveform background
                GL.Color(Utility.Colors.colWaveformBackground);
                GL.Vertex3(xoff, height, 0);
                GL.Vertex3(xoff + width, height, 0);
                GL.Vertex3(xoff + width, rect.height - 1, 0);
                GL.Vertex3(xoff, rect.height - 1, 0);

                GL.End();

                GL.Color(Color.red);
                GL.Begin(GL.LINES);

                GL.Vertex3(xoff, 0, 0);
                GL.Vertex3(xoff, 9, 0);

                GL.End();

                // Waveform texture
                if (track.waveformMaterial != null) {
                    if (track.waveformMaterial.mainTexture != null) {
                        track.waveformMaterial.SetPass(0);
                        GL.Begin(GL.QUADS);
                        DrawTexturedQuad(xoff, 9, width, WAVEFORM_TEX_HEIGHT - 1);
                        GL.End();
                        coloredMaterial.SetPass(0);
                    }
                }

                if (track != null) {
                    float x, w = width;
                    uint sampleCount = track.clip != null ? (uint)track.clip.samples : track.metadata.sampleCount;

                    GL.Begin(GL.LINES);

                    // Draw sections and transitions
                    if (track.sections != null) {
                        foreach (DynamicMusicSystem.Track.Section section in track.sections) {
                            DrawSection(xoff, 0, w, height, section, sampleCount);

                            TimelineLabelToDraw label = new TimelineLabelToDraw {
                                name = section.name,
                                rect = new Rect(
                                    rect.x + xoff + ((float)section.start / sampleCount) * w,
                                    rect.y,
                                    ((float)(section.end - section.start) / sampleCount) * w,
                                    rect.height),
                                style = styles.sectionLabel
                            };
                            timelineLabelsToDraw.Add(label);
                        }
                        foreach (DynamicMusicSystem.Track.Transition transition in track.transitions) {
                            DrawTransition(xoff, 0, w, height, transition, sampleCount);

                            TimelineLabelToDraw label = new TimelineLabelToDraw {
                                name = transition.name,
                                rect = new Rect(
                                    rect.x + xoff + ((float)transition.intro / sampleCount) * w,
                                    rect.y,
                                    ((float)(transition.stop - transition.intro) / sampleCount) * w,
                                    rect.height),
                                style = styles.transitionLabel
                            };
                            timelineLabelsToDraw.Add(label);
                        }
                    }

                    // Play cursor for all sources
                    if (track.HasClip) {
                        GL.Color(Utility.Colors.colHighlight);
                        foreach (AudioSource s in track.sources) {
                            if (s != null) {
                                if (s.isPlaying) {
                                    x = Mathf.Round(w * ((float)s.timeSamples / sampleCount));
                                    GL.Vertex3(xoff + x + 1, 0, 0);
                                    GL.Vertex3(xoff + x + 1, height + WAVEFORM_TEX_HEIGHT * s.volume - 1, 0);
                                }
                            }
                        }
                    }

                    GL.End();
                }

                // Pop the current matrix for rendering, and end the drawing clip.
                GL.PopMatrix();
                GUI.EndClip();
            }
            GUILayout.EndHorizontal();
        }

        void DrawSection(float x, float y, float w, float h, DynamicMusicSystem.Track.Section section, uint sampleCount) {
            float introX = Mathf.Round(((float)section.intro / sampleCount) * w) + 1;
            float startX = Mathf.Round(((float)section.start / sampleCount) * w) + 1;
            float endX = Mathf.Round(((float)section.end / sampleCount) * w) + 1;
            float stopX = Mathf.Round(((float)section.stop / sampleCount) * w) + 1;

            GL.Color(Utility.Colors.colHighlight * 0.5f);
            // Intro
            GL.Vertex3(x + introX, h, 0);
            GL.Vertex3(x + introX, 0, 0);
            // Intro to Start
            GL.Vertex3(x + introX, 1, 0);
            GL.Vertex3(x + startX, 1, 0);
            GL.Color(Utility.Colors.colHighlight);
            // Start
            GL.Vertex3(x + startX, h, 0);
            GL.Vertex3(x + startX, 0, 0);
            // Start to end
            GL.Vertex3(x + startX, 1, 0);
            GL.Vertex3(x + endX, 1, 0);
            // End
            GL.Vertex3(x + endX, 0, 0);
            GL.Vertex3(x + endX, h, 0);
            GL.Color(Utility.Colors.colHighlight * 0.5f);
            // End to stop
            GL.Vertex3(x + endX, 1, 0);
            GL.Vertex3(x + stopX, 1, 0);
            // Stop
            GL.Vertex3(x + stopX, 0, 0);
            GL.Vertex3(x + stopX, h, 0);
        }

        void DrawTransition(float x, float y, float w, float h, DynamicMusicSystem.Track.Transition transition, uint sampleCount) {
            float introX = Mathf.Round(((float)transition.intro / sampleCount) * w) + 1;
            float endX = Mathf.Round(((float)transition.end / sampleCount) * w) + 1;
            float stopX = Mathf.Round(((float)transition.stop / sampleCount) * w) + 1;

            GL.Color(Utility.Colors.colHighlightAlt);
            // Intro
            GL.Vertex3(x + introX, h, 0);
            GL.Vertex3(x + introX, 0, 0);
            // Intro to End
            GL.Vertex3(x + introX, 1, 0);
            GL.Vertex3(x + endX, 1, 0);
            // End
            GL.Color(Utility.Colors.colHighlightAlt * 0.6f);
            GL.Vertex3(x + endX, 0, 0);
            GL.Vertex3(x + endX, h, 0);
            GL.Color(Utility.Colors.colHighlightAlt * 0.7f);
            // End to stop
            GL.Vertex3(x + endX, 1, 0);
            GL.Vertex3(x + stopX, 1, 0);
            // Stop
            GL.Vertex3(x + stopX, 0, 0);
            GL.Vertex3(x + stopX, h, 0);
        }

        void DrawTimelineLabels(SerializedProperty trackProp, int trackIndex) {
            if (timelineLabelsToDraw != null) {
                for (int i = 0; i < timelineLabelsToDraw.Count; i++) {
                    GUI.Label(timelineLabelsToDraw[i].rect, timelineLabelsToDraw[i].name, timelineLabelsToDraw[i].style);
                }
                timelineLabelsToDraw.Clear();
            }
        }

        void DrawVolumeFader(float volume) {
            Rect rect = GUILayoutUtility.GetRect(20, 80);
            int x, y, w, h;

            if (Event.current.type == EventType.Repaint) {
                x = 4;
                y = 6;
                w = 12;
                h = 81;

                GUI.BeginClip(rect);
                GL.PushMatrix();

                GL.Clear(false, false, Color.black);

                coloredMaterial.SetPass(0);
                GL.Begin(GL.QUADS);

                // Back
                DrawQuad(x + w / 2 - 2, y, 4, h, Utility.Colors.colFaderBackground);

                GL.End();

                faderKnobMaterial.SetPass(0);
                GL.Begin(GL.QUADS);

                // Knob
                int knobH = 21;
                h += 4;
                y = Mathf.FloorToInt(4 + (h - knobH) - (volume * (h - knobH)));
                DrawTexturedQuad(x - 1, y, 14, knobH);

                GL.End();

                GL.PopMatrix();
                GUI.EndClip();
            }
        }

        void DrawQuad(float x, float y, float w, float h, Color col) {
            DrawQuad(x, y, w, h, col, col, col, col);
        }

        void DrawQuad(float x, float y, float w, float h, Color colTop, Color colBottom) {
            DrawQuad(x, y, w, h, colTop, colTop, colBottom, colBottom);
        }

        void DrawQuad(float x, float y, float w, float h, Color colTopLeft, Color colTopRight, Color colBottomRight, Color colBottomLeft) {
            GL.Color(colTopLeft);
            GL.Vertex3(x, y, 0);
            GL.Color(colTopRight);
            GL.Vertex3(x + w, y, 0);
            GL.Color(colBottomRight);
            GL.Vertex3(x + w, y + h, 0);
            GL.Color(colBottomLeft);
            GL.Vertex3(x, y + h, 0);
        }

        void DrawTexturedQuad(float x, float y, float w, float h) {
            GL.Color(Color.white);
            GL.TexCoord2(0, 1);
            GL.Vertex3(x, y, 0);
            GL.TexCoord2(1, 1);
            GL.Vertex3(x + w, y, 0);
            GL.TexCoord2(1, 0);
            GL.Vertex3(x + w, y + h, 0);
            GL.TexCoord2(0, 0);
            GL.Vertex3(x, y + h, 0);
        }

        #endregion

        #region Tracks

        void ResetWaveformTexture(DynamicMusicSystem.Track track) {
            if (track == null)
                return;

            track.waveformMaterial.mainTexture = null;
        }

        void UpdateWaveformTexture(DynamicMusicSystem.Track track) {
            if (track == null)
                return;

            if (track.waveformMaterial == null)
                track.waveformMaterial = new Material(Shader.Find("UI/Unlit/Transparent"));

            if (!track.HasClip) {
                track.waveformMaterial.mainTexture = null;
                return;
            }

            track.waveformMaterial.mainTexture = AssetPreview.GetAssetPreview(track.clip);
        }

        void AddTrack() {
            if (musicSystem.tracks == null)
                musicSystem.tracks = new List<DynamicMusicSystem.Track>();

            DynamicMusicSystem.Track track = new DynamicMusicSystem.Track();
            musicSystem.tracks.Add(track);
        }

        void RemoveTrack(DynamicMusicSystem.Track track) {
            if (track == null)
                return;

            if (track.sources != null) {
                for (int i = 0; i < track.sources.Length; i++) {
                    if (track.sources[i] != null)
                        DestroyImmediate(track.sources[i]);
                }
            }
            musicSystem.tracks.Remove(track);
        }

        void UpdateTrackSources(DynamicMusicSystem.Track track) {
            AudioSource[] sources = musicSystem.GetComponents<AudioSource>();
            int num = track.sources.Length;
            if (track.transitions == null) {
                num = 2;
            } else {
                if (track.transitions.Count == 0)
                    num = 2;
            }
            for (int i = 0; i < num; i++) {
                if (track.sources[i] != null)
                    continue;

                // Find existing
                for (int n = 0; n < sources.Length; n++) {
                    bool sourceIsFree = true;
                    for (int t = 0; t < musicSystem.tracks.Count; t++) {
                        if (musicSystem.tracks[t].sources == null)
                            continue;
                        for (int s = 0; s < musicSystem.tracks[t].sources.Length; s++) {
                            if (musicSystem.tracks[t].sources[s] == sources[n]) {
                                sourceIsFree = false;
                                break;
                            }
                        }
                    }
                    if (sourceIsFree) {
                        track.sources[i] = sources[n];
                        break;
                    }
                }

                // Generate new
                if (track.sources[i] == null) {
                    track.sources[i] = musicSystem.gameObject.AddComponent<AudioSource>();
                }

                // Adjust AudioSource settings
                track.sources[i].playOnAwake = false;
                track.sources[i].outputAudioMixerGroup = musicSystem.outputAudioMixerGroup;
                track.sources[i].priority = 0;
                track.sources[i].reverbZoneMix = 0f;
            }

            // If track has no transitions, remove the third AudioSource (if it exists)
            bool removeThird = false;
            if (track.transitions == null) {
                removeThird = true;
            } else {
                if (track.transitions.Count == 0)
                    removeThird = true;
            }

            if (removeThird && track.sources[2] != null) {
                DestroyImmediate(track.sources[2]);
                track.sources[2] = null;
            }

            // If track has no sections, force fade out on stop
            if (!track.HasSections) {
                track.fadeOnStop = true;
            }

            // Update clip
            for (int i = 0; i < track.sources.Length; i++) {
                if (track.sources[i] != null)
                    track.sources[i].clip = track.clip;
            }
        }

        void OnTrackClipChanged(AudioClip clip, SerializedProperty trackProp, int trackIndex) {
            DynamicMusicSystem.Track track = musicSystem.tracks[trackIndex];

            track.waveformMaterial.mainTexture = null;
            track.metadata = null;

            ApplyMetadataToSerializedProperty(new Wave.Metadata(), trackProp.FindPropertyRelative("metadata"));
            serializedObject.ApplyModifiedProperties();

            if (clip != null) {
                string assetPath = AssetDatabase.GetAssetPath(clip.GetEntityId());

                if (assetPath == "" || assetPath == null) {
                    Debug.LogWarning("Couldn't find asset path for clip: " + clip.name);
                } else {
                    Wave.Metadata data = Wave.Reader.GetMetadata(assetPath);
                    if (data != null)
                        ApplyMetadataToSerializedProperty(data, trackProp.FindPropertyRelative("metadata"));
                    else
                        track.clip = null;
                }
            }

            UpdateTrackSectionsAndTransitions(track.metadata, trackIndex);
            UpdateTrackSources(track);
            ResetWaveformTexture(track);
        }

        void UpdateTrackSectionsAndTransitions(Wave.Metadata data, int trackIndex) {
            DynamicMusicSystem.Track track = musicSystem.tracks[trackIndex];

            if (track == null)
                return;

            // Create new or remove all sections from track
            if (track.sections == null)
                track.sections = new List<DynamicMusicSystem.Track.Section>();
            else
                track.sections.Clear();

            if (track.transitions == null)
                track.transitions = new List<DynamicMusicSystem.Track.Transition>();
            else
                track.transitions.Clear();

            if (data == null)
                return;

            if (data.cues == null)
                return;

            // Get list of possible section names
            List<string> possibleSectionNames = GetPossibleSectionNamesFromCues(data.cues);

            // Loop through and validate each section
            foreach (string sectionName in possibleSectionNames) {

                // Create new track section and give it a name
                DynamicMusicSystem.Track.Section section = new DynamicMusicSystem.Track.Section(sectionName);

                string linkedSectionName = "";

                for (int i = 0; i < data.cues.Length; i++) {
                    string cueName = data.cues[i].name;
                    // Intro
                    if (data.cues[i].name == sectionName + "-Intro")
                        section.intro = (int)data.cues[i].position;
                    // Start
                    if (data.cues[i].name == sectionName + "-Start")
                        section.start = (int)data.cues[i].position;
                    // End
                    if (data.cues[i].name == sectionName + "-End")
                        section.end = (int)data.cues[i].position;
                    // End (with linked section)
                    string endCueLinkedName = sectionName + "-End-Goto";
                    if (cueName.Contains(endCueLinkedName)) {
                        linkedSectionName = cueName.Substring(endCueLinkedName.Length);
                        section.end = (int)data.cues[i].position;
                    }
                    // Stop
                    if (data.cues[i].name == sectionName + "-Stop")
                        section.stop = (int)data.cues[i].position;
                }

                // Check that cues are lined up correctly
                if (section.end > section.start) {

                    // The outro-cue is not required, and is set to end if it was not found
                    if (section.stop == -1 || section.stop < section.end)
                        section.stop = section.end;

                    // Check whether this section is a transition or a loop
                    if (section.start > -1) {

                        // The intro-cue is not required, and is set to start if it was not found
                        if (section.intro == -1)
                            section.intro = section.start;

                        // Calculate section durations
                        section.CalculateDurations(data.sampleRate);

                        // Set linked section
                        if (!string.IsNullOrEmpty(linkedSectionName)) {
                            section.linkedSection = linkedSectionName;
                        }

                        // Add section to list
                        track.sections.Add(section);
                    } else {

                        // Validate transition
                        if (section.intro > -1 && section.end > section.intro) {
                            // Create new transition and give it a name
                            DynamicMusicSystem.Track.Transition transition = new DynamicMusicSystem.Track.Transition(sectionName);

                            // Copy section data into transition
                            transition.name = section.name;
                            transition.intro = section.intro;
                            transition.end = section.end;
                            transition.stop = section.stop;

                            // Calculate transition durations
                            transition.CalculateDurations(data.sampleRate);

                            // Add transition to list
                            track.transitions.Add(transition);
                        }
                    }
                }

            }

            // Update the sections serialized property
            SerializedProperty tracksProp = serializedObject.FindProperty("tracks");
            SerializedProperty trackProp = null;
            for (int i = 0; i < tracksProp.arraySize; i++) {
                if (i == trackIndex) {
                    trackProp = tracksProp.GetArrayElementAtIndex(i);
                    break;
                }
            }
            if (trackProp != null) {

                // Modify sections on serialized object
                SerializedProperty sectionsProp = trackProp.FindPropertyRelative("sections");
                if (sectionsProp != null) {
                    sectionsProp.ClearArray();
                    if (track.sections != null) {
                        for (int i = 0; i < track.sections.Count; i++) {
                            sectionsProp.InsertArrayElementAtIndex(i);
                            SerializedProperty sectionProp = sectionsProp.GetArrayElementAtIndex(i);
                            sectionProp.FindPropertyRelative("name").stringValue = track.sections[i].name;
                            sectionProp.FindPropertyRelative("intro").intValue = track.sections[i].intro;
                            sectionProp.FindPropertyRelative("start").intValue = track.sections[i].start;
                            sectionProp.FindPropertyRelative("end").intValue = track.sections[i].end;
                            sectionProp.FindPropertyRelative("stop").intValue = track.sections[i].stop;
                            sectionProp.FindPropertyRelative("introDuration").doubleValue = track.sections[i].introDuration;
                            sectionProp.FindPropertyRelative("mainDuration").doubleValue = track.sections[i].mainDuration;
                            sectionProp.FindPropertyRelative("outroDuration").doubleValue = track.sections[i].outroDuration;
                            sectionProp.FindPropertyRelative("linkedSection").stringValue = track.sections[i].linkedSection;
                        }
                    }
                }

                // Modify transitions on serialized object
                SerializedProperty transitionsProp = trackProp.FindPropertyRelative("transitions");
                if (transitionsProp != null) {
                    transitionsProp.ClearArray();
                    if (track.transitions != null) {
                        for (int i = 0; i < track.transitions.Count; i++) {
                            transitionsProp.InsertArrayElementAtIndex(i);
                            SerializedProperty transitionProp = transitionsProp.GetArrayElementAtIndex(i);
                            transitionProp.FindPropertyRelative("name").stringValue = track.transitions[i].name;
                            transitionProp.FindPropertyRelative("intro").intValue = track.transitions[i].intro;
                            transitionProp.FindPropertyRelative("end").intValue = track.transitions[i].end;
                            transitionProp.FindPropertyRelative("stop").intValue = track.transitions[i].stop;
                            transitionProp.FindPropertyRelative("introDuration").doubleValue = track.transitions[i].introDuration;
                            transitionProp.FindPropertyRelative("outroDuration").doubleValue = track.transitions[i].outroDuration;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        List<string> GetPossibleSectionNamesFromCues(Wave.Cue[] cues) {
            List<string> sectionNames = new List<string>();
            for (int i = 0; i < cues.Length; i++) {
                // Find first dash
                int firstDash = cues[i].name.IndexOf("-");
                if (firstDash < 1)
                    continue;
                string possibleSectionName = cues[i].name.Substring(0, firstDash);
                if (!sectionNames.Contains(possibleSectionName))
                    sectionNames.Add(possibleSectionName);
            }
            return sectionNames;
        }

        bool AnyTrackHasClip() {
            if (musicSystem.tracks == null)
                return false;

            for (int i = 0; i < musicSystem.tracks.Count; i++) {
                if (musicSystem.tracks[i].HasClip)
                    return true;
            }
            return false;
        }

        #endregion

        #region Solo/Mute

        void SoloTrack(DynamicMusicSystem.Track track) {
            if (track == null)
                return;

            if (soloedTracks == null)
                soloedTracks = new List<DynamicMusicSystem.Track>();

            soloedTracks.Add(track);
            foreach (DynamicMusicSystem.Track t in musicSystem.tracks) {
                if (!soloedTracks.Contains(t)) {
                    t.Muted = true;
                }
            }
            track.Muted = false;
        }

        void UnSoloTrack(DynamicMusicSystem.Track track) {
            if (soloedTracks == null || track == null)
                return;

            if (soloedTracks.Contains(track)) {
                soloedTracks.Remove(track);
            }

            if (soloedTracks.Count == 0) {
                foreach (DynamicMusicSystem.Track t in musicSystem.tracks) {
                    t.Muted = false;
                }
            } else {
                track.Muted = true;
            }
        }

        bool TrackIsSoloed(DynamicMusicSystem.Track track) {
            if (soloedTracks == null)
                return false;

            return soloedTracks.Contains(track);
        }

        bool AnyTrackIsSoloed() {
            if (soloedTracks == null)
                return false;

            return (soloedTracks.Count > 0);
        }

        bool TrackIsMuted(DynamicMusicSystem.Track track) {
            if (track.sources == null)
                return false;

            for (int i = 0; i < track.sources.Length; i++) {
                if (track.sources[i] != null) {
                    if (track.sources[i].mute)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region States

        void AddState() {
            if (musicSystem == null)
                return;

            if (musicSystem.states == null)
                musicSystem.states = new List<DynamicMusicSystem.State>();

            DynamicMusicSystem.State state = new DynamicMusicSystem.State {
                name = GetUniqueStateName(),
                id = GetUniqueStateID()
            };
            musicSystem.states.Add(state);
        }

        string GetUniqueStateName() {
            int num = 0;
            bool nameIsTaken;
            string name;
            do {
                nameIsTaken = false;
                num++;
                name = "State " + num;

                if (musicSystem.states == null)
                    break;

                if (musicSystem.states.Count == 0)
                    break;

                foreach (DynamicMusicSystem.State state in musicSystem.states) {
                    if (state.name == name) {
                        nameIsTaken = true;
                    }
                }
            } while (nameIsTaken);

            return name;
        }

        string GetUniqueStateID() {
            return System.Guid.NewGuid().ToString();
        }

        void RemoveState(DynamicMusicSystem.State state) {
            if (musicSystem == null || musicSystem.states == null)
                return;

            if (state != null) {

                // Remove state from all tracks
                if (musicSystem.tracks != null) {
                    for (int i = 0; i < musicSystem.tracks.Count; i++) {
                        if (musicSystem.tracks[i].stateIDs != null) {
                            musicSystem.tracks[i].stateIDs.Remove(state.id);
                        }
                    }
                }

                musicSystem.states.Remove(state);

                if (musicSystem.states.Count == 0)
                    musicSystem.states = null;
            }
        }

        void RenameState(DynamicMusicSystem.State state, string name) {
            if (musicSystem == null)
                return;
            bool nameIsAvailable = true;
            for (int i = 0; i < musicSystem.states.Count; i++) {
                if (musicSystem.states[i].name == name)
                    nameIsAvailable = false;
            }
            if (nameIsAvailable)
                state.name = name;
        }

        string GetStateDropdownString(DynamicMusicSystem.Track track) {
            string str = "";
            int count = 0;

            if (musicSystem.states != null) {
                for (int i = 0; i < musicSystem.states.Count; i++) {
                    if (track.stateIDs != null) {
                        if (track.stateIDs.Contains(musicSystem.states[i].id)) {
                            str += musicSystem.states[i].name + ", ";
                            count++;
                        }
                    }
                }
            }

            if (count > 0) {
                if (str.Length > 28)
                    str = count + " state(s)";
                else
                    str = str.Substring(0, str.Length - 2);

            } else
                str = "None";

            return str;
        }

        void AddStateToTrack(DynamicMusicSystem.State state, DynamicMusicSystem.Track track) {
            if (state == null || track == null)
                return;

            if (track.stateIDs == null)
                track.stateIDs = new List<string>();

            track.stateIDs.Add(state.id);
        }

        void RemoveStateFromTrack(DynamicMusicSystem.State state, DynamicMusicSystem.Track track) {
            if (state == null || track == null)
                return;

            if (track.stateIDs != null) {
                track.stateIDs.Remove(state.id);
            }
        }

        void OnStateSelected(object stateSelection) {
            StateSelection selection = (StateSelection)stateSelection;

            if (selection.state == null) {
                // Remove all states from track
                if (selection.track != null) {
                    if (selection.track.stateIDs != null) {
                        selection.track.stateIDs.Clear();
                    }
                }
            } else {
                if (selection.exists) {
                    RemoveStateFromTrack(selection.state, selection.track);
                } else {
                    AddStateToTrack(selection.state, selection.track);
                }
            }
        }

        #endregion

        #region Various

        static string SecondsToTime(float seconds) {
            System.TimeSpan time = System.TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss\:ff");
        }

        // Force repaint for smooth animations
        public override bool RequiresConstantRepaint() {
            if (musicSystem == null)
                return false;

            return musicSystem.CurrentPlayState != DynamicMusicSystem.PlayState.stopped;
        }

        void ApplyMetadataToSerializedProperty(Wave.Metadata data, SerializedProperty dataProp) {
            dataProp.FindPropertyRelative("filename").stringValue = data.filename;
            dataProp.FindPropertyRelative("duration").floatValue = data.duration;
            dataProp.FindPropertyRelative("fileBytes").intValue = (int)data.fileBytes;
            dataProp.FindPropertyRelative("riffTypeID").stringValue = data.riffTypeID;
            dataProp.FindPropertyRelative("compressionCode").intValue = (int)data.compressionCode;
            dataProp.FindPropertyRelative("channelCount").intValue = (int)data.channelCount;
            dataProp.FindPropertyRelative("sampleRate").intValue = (int)data.sampleRate;
            dataProp.FindPropertyRelative("avgBytesPerSec").intValue = (int)data.avgBytesPerSec;
            dataProp.FindPropertyRelative("blockAlign").intValue = (int)data.blockAlign;
            dataProp.FindPropertyRelative("bitRate").intValue = (int)data.bitRate;
            dataProp.FindPropertyRelative("sampleCount").intValue = (int)data.sampleCount;
            dataProp.FindPropertyRelative("maxSampleValue").intValue = data.maxSampleValue;

            // Cues
            SerializedProperty cuesProp = dataProp.FindPropertyRelative("cues");
            cuesProp.ClearArray();
            if (data.cues != null) {
                for (int i = 0; i < data.cues.Length; i++) {
                    cuesProp.InsertArrayElementAtIndex(i);
                    SerializedProperty cueProp = cuesProp.GetArrayElementAtIndex(i);
                    cueProp.FindPropertyRelative("ID").intValue = (int)data.cues[i].ID;
                    cueProp.FindPropertyRelative("name").stringValue = data.cues[i].name;
                    cueProp.FindPropertyRelative("position").intValue = (int)data.cues[i].position;
                    cueProp.FindPropertyRelative("dataChunkID").intValue = (int)data.cues[i].dataChunkID;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateSectionButtonList() {
            sectionNames.Clear();
            if (musicSystem.tracks != null) {
                for (int t = 0; t < musicSystem.tracks.Count; t++) {
                    if (musicSystem.tracks[t].sections != null) {
                        for (int i = 0; i < musicSystem.tracks[t].sections.Count; i++) {
                            if (!sectionNames.Contains(musicSystem.tracks[t].sections[i].name))
                                sectionNames.Add(musicSystem.tracks[t].sections[i].name);
                        }
                    }
                }
            }
        }

        void UpdateTransitionButtonList() {
            transitionNames.Clear();
            if (musicSystem.tracks != null) {
                for (int t = 0; t < musicSystem.tracks.Count; t++) {
                    if (musicSystem.tracks[t].transitions != null) {
                        for (int i = 0; i < musicSystem.tracks[t].transitions.Count; i++) {
                            if (!transitionNames.Contains(musicSystem.tracks[t].transitions[i].name))
                                transitionNames.Add(musicSystem.tracks[t].transitions[i].name);
                        }
                    }
                }
            }
        }

        #endregion
    }
}