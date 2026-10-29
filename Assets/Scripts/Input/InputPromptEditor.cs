#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Keep this file inside a folder named "Editor".
[CustomEditor(typeof(InputPrompt))]
public class InputPromptEditor : Editor
{
    private SerializedProperty _actionProp;
    private SerializedProperty _keyboardSchemeProp;
    private SerializedProperty _gamepadSchemeProp;
    private SerializedProperty _keyboardIdProp;
    private SerializedProperty _gamepadIdProp;
    private SerializedProperty _overrideText;
    private SerializedProperty _text;
    
    InputPrompt _inputPrompt;
    
    private GUIContent m_CustomiseUILabel = new GUIContent("Customise UI");

    private void OnEnable()
    {
        _inputPrompt = (InputPrompt)target;
        
        _actionProp = serializedObject.FindProperty("inputActionReference");
        _keyboardSchemeProp = serializedObject.FindProperty("keyboardScheme");
        _gamepadSchemeProp = serializedObject.FindProperty("gamepadScheme");
        _keyboardIdProp = serializedObject.FindProperty("keyboardBindingId");
        _gamepadIdProp = serializedObject.FindProperty("gamepadBindingId");
        _overrideText = serializedObject.FindProperty("overrideText");
        _text = serializedObject.FindProperty("labelText");
    }

    public override void OnInspectorGUI()
    {
        // Draws everything except the [HideInInspector] binding id fields
        DrawDefaultInspector();

        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Binding", EditorStyles.boldLabel);

        InputAction action = (_actionProp.objectReferenceValue as InputActionReference)?.action;
        if (action == null)
        {
            EditorGUILayout.HelpBox("Assign an Input Action Reference to choose bindings.", MessageType.Info);
        }
        else
        {
            DrawBindingPopup("Keyboard Binding", action, _keyboardSchemeProp.stringValue, _keyboardIdProp);
            DrawBindingPopup("Gamepad Binding", action, _gamepadSchemeProp.stringValue, _gamepadIdProp);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(m_CustomiseUILabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(_overrideText);
            if(_inputPrompt.overrideText)
            {
                EditorGUILayout.PropertyField(_text);
            }
                
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawBindingPopup(string label, InputAction action, string scheme, SerializedProperty idProp)
    {
        var names = new List<string> { "(First binding for scheme)" };
        var ids = new List<string> { string.Empty };

        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            InputBinding binding = bindings[i];

            // Skip the composite header itself, only list real bindings and composite parts
            if (binding.isComposite) continue;

            // Only list bindings assigned to this control scheme
            if (string.IsNullOrEmpty(binding.groups)) continue;
            if (!binding.groups.Split(InputBinding.Separator).Contains(scheme)) continue;

            string display = action.GetBindingDisplayString(i,
                InputBinding.DisplayStringOptions.DontUseShortDisplayNames |
                InputBinding.DisplayStringOptions.IgnoreBindingOverrides);

            if (binding.isPartOfComposite)
            {
                display = $"{ObjectNames.NicifyVariableName(binding.name)}: {display}";
            }

            // '/' would create submenus in the popup
            names.Add(display.Replace('/', '\\'));
            ids.Add(binding.id.ToString());
        }

        int current = Mathf.Max(0, ids.IndexOf(idProp.stringValue));
        int selected = EditorGUILayout.Popup(label, current, names.ToArray());

        if (selected != current)
        {
            idProp.stringValue = ids[selected];
        }
    }
}
#endif