using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(CraftingRecipe))]
    public class CraftingRecipeEditor : UnityEditor.Editor
    {
        private const float NameWidth = 60f;
        private const float CountWidth = 40f;
        private const float Padding = 8f;
        
        private SerializedProperty _input1;
        private SerializedProperty _input1Count;
        private SerializedProperty _input2;
        private SerializedProperty _input2Count;
        private SerializedProperty _input3;
        private SerializedProperty _input3Count;
        private SerializedProperty _output;
        private SerializedProperty _outputCount;

        private ItemDefinition[] _items;
        private GUIContent[] _labels; // index 0 = "None"
        
        private float _popupMinWidth = -1f;

        private void OnEnable()
        {
            _input1 = serializedObject.FindProperty(nameof(CraftingRecipe.input1));
            _input1Count = serializedObject.FindProperty(nameof(CraftingRecipe.input1Count));
            _input2 = serializedObject.FindProperty(nameof(CraftingRecipe.input2));
            _input2Count = serializedObject.FindProperty(nameof(CraftingRecipe.input2Count));
            _input3 = serializedObject.FindProperty(nameof(CraftingRecipe.input3));
            _input3Count = serializedObject.FindProperty(nameof(CraftingRecipe.input3Count));
            _output = serializedObject.FindProperty(nameof(CraftingRecipe.output));
            _outputCount = serializedObject.FindProperty(nameof(CraftingRecipe.outputCount));

            RefreshItems();
        }

        private void RefreshItems()
        {
            _items = AssetDatabase.FindAssets($"t:{nameof(ItemDefinition)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemDefinition>)
                .Where(item => item != null)
                .OrderBy(item => item.name)
                .ToArray();

            _labels = new[] { new GUIContent("None") }
                .Concat(_items.Select(item => new GUIContent(item.name)))
                .ToArray();
            
            _popupMinWidth = -1f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // EditorStyles is only safe to use during GUI, so measure lazily here.
            if (_popupMinWidth < 0f)
            {
                _popupMinWidth = _labels
                    .Select(l => EditorStyles.popup.CalcSize(l).x)
                    .Max() + Padding;
            }

            DrawRow(_input1, _input1Count);
            EditorGUILayout.Space();
            DrawRow(_input2, _input2Count);
            EditorGUILayout.Space();
            DrawRow(_input3, _input3Count);
            EditorGUILayout.Space();
            DrawRow(_output, _outputCount);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRow(SerializedProperty item, SerializedProperty count)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(item.displayName, GUILayout.Width(NameWidth));
            DrawItemPopup(item);
            EditorGUILayout.LabelField("x", GUILayout.Width(12f));
            EditorGUILayout.PropertyField(count, GUIContent.none, GUILayout.Width(CountWidth));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawItemPopup(SerializedProperty property)
        {
            var current = property.objectReferenceValue as ItemDefinition;
            int index = current == null ? 0 : Array.IndexOf(_items, current) + 1;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(
                index, _labels,
                GUILayout.MinWidth(_popupMinWidth),
                GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = newIndex == 0 ? null : _items[newIndex - 1];
            }
        }
    }
}
