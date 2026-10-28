using UnityEngine;
using UnityEditor;

namespace MordiAudio.MusicSystem
{
	public class LinkedSectionEditorWindow : ExtendedEditorWindow
	{
		public static void Open(DynamicMusicSystem musicSystem) {
			LinkedSectionEditorWindow window = GetWindow<LinkedSectionEditorWindow>("Edit Linked Sections");

			window.serializedObject = new SerializedObject(musicSystem);
		}

		private void OnGUI() {
			if (serializedObject == null) {
				return;
			}

			DynamicMusicSystem musicSystem = (DynamicMusicSystem)serializedObject.targetObject;

			GUI.enabled = (musicSystem.CurrentPlayState == DynamicMusicSystem.PlayState.stopped);

			int trackId = 1;
			foreach (SerializedProperty trackProp in serializedObject.FindProperty("tracks")) {
				EditorGUILayout.Space(5);
				string leadingZero = trackId < 10 ? "0" : "";
				EditorGUILayout.LabelField($"Track {leadingZero}{trackId}", EditorStyles.boldLabel);
				foreach (SerializedProperty sectionProp in trackProp.FindPropertyRelative("sections")) {
					GUIContent content = new GUIContent($"{sectionProp.FindPropertyRelative("name").stringValue} goes to...");
					EditorGUILayout.PropertyField(sectionProp.FindPropertyRelative("linkedSection"), content);
				}
				trackId++;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}