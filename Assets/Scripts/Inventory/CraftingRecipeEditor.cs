using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(CraftingRecipe))]
    public class CraftingRecipeEditor : UnityEditor.Editor
    {
        private SerializedProperty _input1;
        private SerializedProperty _input2;
        private SerializedProperty _input3;
        private SerializedProperty _output;

        private ItemDefinition[] _items;
        private GUIContent[] _labels; // index 0 = "None"

        private void OnEnable()
        {
            _input1 = serializedObject.FindProperty(nameof(CraftingRecipe.input1));
            _input2 = serializedObject.FindProperty(nameof(CraftingRecipe.input2));
            _input3 = serializedObject.FindProperty(nameof(CraftingRecipe.input3));
            _output = serializedObject.FindProperty(nameof(CraftingRecipe.output));

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawItemPopup(_input1);
            DrawItemPopup(_input2);
            DrawItemPopup(_input3);
            EditorGUILayout.Space();
            DrawItemPopup(_output);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawItemPopup(SerializedProperty property)
        {
            var current = property.objectReferenceValue as ItemDefinition;
            int index = current == null ? 0 : Array.IndexOf(_items, current) + 1;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(new GUIContent(property.displayName), index, _labels);
            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = newIndex == 0 ? null : _items[newIndex - 1];
            }
        }
    }
}
